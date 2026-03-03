using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using NS.UnifiedLoot;

namespace NS.UnifiedLoot.Examples {
    /// <summary>
    /// End-to-end example of UnifiedLoot in a simple RPG enemy-kill scenario.
    ///
    /// <para><b>Table data — two ways to supply it:</b></para>
    /// <list type="number">
    ///   <item>
    ///     <b>Asset-driven (designer path):</b> create <see cref="GoblinItemTable"/> assets via
    ///     <c>Create → UnifiedLoot/Examples/Goblin Item Table</c> and assign them in the Inspector.
    ///     The probability summary and validation warnings appear live in the Inspector
    ///     (powered by <c>LootTableAssetEditor</c>).
    ///   </item>
    ///   <item>
    ///     <b>Code-defined (fallback):</b> leave all table slots empty and the component builds
    ///     hard-coded tables at runtime — useful for quick iteration.
    ///   </item>
    /// </list>
    ///
    /// <para><b>Pipelines are always code-defined</b> — they describe <em>how</em> to roll,
    /// while the assets describe <em>what</em> can drop.</para>
    ///
    /// <para><b>Features demonstrated:</b>
    /// <see cref="LootTableAsset{TItem}"/>, <see cref="CompositeTable{T}"/>,
    /// <see cref="WeightedRandomStrategy{T}"/>, <see cref="BonusRollStrategy{T}"/>,
    /// <see cref="ConsolidateResultsStrategy{T}"/>, <see cref="ModifyWeightStrategy{T}"/>,
    /// <see cref="SoftPityStrategy{T}"/>, <see cref="ILootObserver{T}"/>,
    /// <see cref="IResettable"/>, <see cref="LootContext"/>.</para>
    ///
    /// Right-click the component header in the Inspector to trigger rolls via the context menu.
    /// </summary>
    public class GoblinEncounterExample : MonoBehaviour {
        private static readonly ContextKey<float> Luck = new("Luck");
        private static readonly ContextKey<int> PlayerLevel = new("PlayerLevel");
        private static readonly ContextKey<string> Source = new("Source");

        [Header("Goblin table  (leave empty to use the code-defined fallback)")]
        [Tooltip("Create via: Create → UnifiedLoot/Examples/Goblin Item Table")]
        [SerializeField] private GoblinItemTable? goblinTable;

        [Header("Captain table pools  (all three must be assigned to use the asset path)")]
        [Tooltip("Common drops: coins, potions. Assign a GoblinItemTable asset.")]
        [SerializeField] private GoblinItemTable? captainCommonTable;

        [Tooltip("Weapon drops: swords, staff. Assign a GoblinItemTable asset.")]
        [SerializeField] private GoblinItemTable? captainWeaponTable;

        [Tooltip("Rare drops: boss-exclusive items. Assign a GoblinItemTable asset.")]
        [SerializeField] private GoblinItemTable? captainRareTable;

        [Tooltip("Selection weight for each captain sub-table (should sum to 1).")]
        [SerializeField] private float captainCommonWeight = 0.60f;
        [SerializeField] private float captainWeaponWeight = 0.35f;
        [SerializeField] private float captainRareWeight = 0.05f;

        [Header("Player stats")]
        [Range(0f, 1f)]
        [Tooltip("0 = no luck bonus.  1 = guaranteed bonus roll every kill.  Typical: 0.1–0.4")]
        [SerializeField] private float playerLuck = 0.25f;

        [Header("Boss pity  (Goblin Captain)")]
        [Tooltip("Empty boss runs before the rare-drop chance starts climbing.")]
        [SerializeField] private int softPityStart = 3;
        [Tooltip("Empty boss runs that guarantee a rare drop (100 %).")]
        [SerializeField] private int hardPityAt = 7;


        private LootPipeline<GoblinItem> _goblinPipeline = null!;
        private LootPipeline<GoblinItem> _captainPipeline = null!;
        private SoftPityStrategy<GoblinItem> _captainPity = null!;
        private ILootTable<GoblinItem> _activeGoblinTable = null!;
        private ILootTable<GoblinItem> _activeCaptainTable = null!;
        private LootContext _context = null!;

        private void Awake() {
            _context = new LootContext()
                .Set(Luck, playerLuck)
                .Set(PlayerLevel, 10);
            _activeGoblinTable = ResolveGoblinTable();
            _activeCaptainTable = ResolveCaptainTable();
            BuildGoblinPipeline();
            BuildCaptainPipeline();
        }

        private void OnDestroy() {
            foreach (var s in _goblinPipeline.Strategies.OfType<IResettable>())
                s.ResetAll();
            foreach (var s in _captainPipeline.Strategies.OfType<IResettable>())
                s.ResetAll();
        }


        /// <summary>
        /// Returns the designer asset if one is assigned; otherwise builds a code-defined
        /// table with the same entries as a sensible default.
        /// </summary>
        private ILootTable<GoblinItem> ResolveGoblinTable() {
            return goblinTable as ILootTable<GoblinItem> ?? new LootTable<GoblinItem>()
                .Add(GoblinItem.Coin, weight: 60f, minQuantity: 1, maxQuantity: 10)
                .Add(GoblinItem.HealthPotion, weight: 20f)
                .Add(GoblinItem.ManaPotion, weight: 10f)
                .Add(GoblinItem.IronSword, weight: 7f)
                .Add(GoblinItem.SteelSword, weight: 2f)
                .Add(GoblinItem.MagicStaff, weight: 1f);
        }

        /// <summary>
        /// Builds the captain's <see cref="CompositeTable{T}"/>.
        /// Uses the three designer assets if all are assigned; otherwise falls back to
        /// inline code-defined sub-tables.
        /// </summary>
        private ILootTable<GoblinItem> ResolveCaptainTable() {
            var common = captainCommonTable as ILootTable<GoblinItem> ?? new LootTable<GoblinItem>()
                .Add(GoblinItem.Coin, weight: 60f, minQuantity: 5, maxQuantity: 25)
                .Add(GoblinItem.HealthPotion, weight: 25f)
                .Add(GoblinItem.ManaPotion, weight: 15f);

            var weapons = captainWeaponTable as ILootTable<GoblinItem> ?? new LootTable<GoblinItem>()
                .Add(GoblinItem.IronSword, weight: 50f)
                .Add(GoblinItem.SteelSword, weight: 35f)
                .Add(GoblinItem.MagicStaff, weight: 15f);

            var rare = captainRareTable as ILootTable<GoblinItem> ?? new LootTable<GoblinItem>()
                .Add(GoblinItem.GoblinKingsCrown, weight: 1f);

            return new CompositeTable<GoblinItem>("Captain")
                .Add(common, captainCommonWeight)
                .Add(weapons, captainWeaponWeight)
                .Add(rare, captainRareWeight);
        }

        private void BuildCaptainPipeline() {
            //  1. ModifyWeight — GoblinKingsCrown weight scales with player luck
            //  2. WeightedRandom(1) — single roll from the CompositeTable
            //  3. SoftPity — ramps up the rare drop chance after consecutive failures
            _captainPity = new SoftPityStrategy<GoblinItem>(softPityStart, hardPityAt);

            _captainPipeline = new LootPipeline<GoblinItem>()
                .WithRandom(UnityRandom.Instance)
                .AddStrategy(new ModifyWeightStrategy<GoblinItem>((entry, ctx) => {
                    if (!Equals(entry.Entry.Item, GoblinItem.GoblinKingsCrown))
                        return entry.Weight;
                    var luck = ctx.GetOrDefault(Luck);
                    return entry.Weight * (1f + luck * 3f);
                }))
                .AddStrategy(new WeightedRandomStrategy<GoblinItem>(rollCount: 1))
                .AddStrategy(_captainPity)
                .AddObserver(new ConsoleObserver<GoblinItem>("Captain"));
        }

        private void BuildGoblinPipeline() {
            //  1. ModifyWeight — rare items (SteelSword, MagicStaff) get a luck bonus
            //  2. WeightedRandom(3) — three rolls per kill
            //  3. BonusRoll — luck-based chance at a 4th free roll
            //  4. Consolidate — stack repeated Coins / Potions
            _goblinPipeline = new LootPipeline<GoblinItem>()
                .WithRandom(UnityRandom.Instance)
                .AddStrategy(new ModifyWeightStrategy<GoblinItem>((entry, ctx) => {
                    var item = entry.Entry.Item;
                    if (Equals(item, GoblinItem.SteelSword) || Equals(item, GoblinItem.MagicStaff)) {
                        var luck = ctx.GetOrDefault(Luck);
                        return entry.Weight * (1f + luck * 3f); // up to 4× at max luck
                    }

                    return entry.Weight;
                }))
                .AddStrategy(new WeightedRandomStrategy<GoblinItem>(rollCount: 3))
                .AddStrategy(new BonusRollStrategy<GoblinItem>(Luck))
                .AddStrategy(new ConsolidateResultsStrategy<GoblinItem>())
                .AddObserver(new ConsoleObserver<GoblinItem>("Goblin"));
        }

        private void UpdateContext() => _context.Set(Luck, playerLuck);

        [ContextMenu("Kill Goblin (roll loot)")]
        public List<LootResult<GoblinItem>> KillGoblin() {
            UpdateContext();
            _context.Set(Source, "Goblin");
            var results = _goblinPipeline.Execute(_activeGoblinTable, _context);
            return results;
        }

        [ContextMenu("Kill Goblin Captain (roll loot)")]
        public List<LootResult<GoblinItem>> KillGoblinCaptain() {
            UpdateContext();
            _context.Set(Source, "Goblin Captain");
            var results = _captainPipeline.Execute(_activeCaptainTable, _context);
            Debug.Log($"[Pity] Current failure count: {_captainPity.GetFailureCount(_activeCaptainTable.Id)}");
            return results;
        }

        [ContextMenu("Reset Captain Pity Counter")]
        public void ResetCaptainPity() {
            _captainPity.ResetAll();
            Debug.Log("[UnifiedLoot] Captain pity counter reset.");
        }

        private static void LogResults(string source, List<LootResult<GoblinItem>> results) { }

        private sealed class ConsoleObserver<T> : ILootObserver<T> {
            private readonly string _tag;
            public ConsoleObserver(string tag) => _tag = tag;

            public void OnRollComplete(ILootTable<T> table, IReadOnlyList<LootResult<T>> results, LootContext context) {
                var source = context.Get(Source);
                Debug.Log($"[Observer:{_tag}] {results.Count} result(s)");
                if (results.Count == 0) {
                    Debug.Log($"[UnifiedLoot] {source} dropped nothing.");
                    return;
                }

                var sb = new StringBuilder($"[UnifiedLoot] {source} dropped:  ");
                foreach (var r in results)
                    sb.Append(r.Quantity > 1 ? $"{r.Item} ×{r.Quantity}   " : $"{r.Item}   ");
                Debug.Log(sb);
            }
        }
    }
}