using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using NS.UnifiedLoot;
using NS.UnifiedLoot.UnifiedLoot.Runtime.Core;
using NS.UnifiedLoot.UnifiedLoot.Runtime.Random;
using NS.UnifiedLoot.UnifiedLoot.Runtime.Strategies;
using NS.UnifiedLoot.UnifiedLoot.Runtime.Tables;

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
    /// <see cref="LootTableAsset{TItem}"/>, <see cref="CompositeTableBuilder{T}"/>,
    /// <see cref="WeightedRandomStrategy{T}"/>, <see cref="BonusRollStrategy{T}"/>,
    /// <see cref="ConsolidateResultsStrategy{T}"/>, <see cref="ModifyWeightStrategy{T}"/>,
    /// <see cref="SoftPityStrategy{T}"/>, <see cref="ILootObserver{T}"/>,
    /// <see cref="ILootFactory{TDef, TInstance}"/>, <see cref="LootPipelineExtensions.ExecuteAndBuild{TDef, TInstance}"/>,
    /// <see cref="IResettable"/>, <see cref="Context"/>.</para>
    ///
    /// Right-click the component header in the Inspector to trigger rolls via the context menu.
    /// </summary>
    public class GoblinEncounterExample : MonoBehaviour, ILootFactory<GoblinItemDef, GoblinItemInstance> {
        private static readonly Key<float> Luck = new("Luck");
        private static readonly Key<int> PlayerLevel = new("PlayerLevel");
        private static readonly Key<string> Source = new("Source");

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

        [Header("Fallback Definitions (used if assets above are empty)")]
        [SerializeField] private GoblinItemDef? coinDef;
        [SerializeField] private GoblinItemDef? healthPotionDef;
        [SerializeField] private GoblinItemDef? manaPotionDef;
        [SerializeField] private GoblinItemDef? ironSwordDef;
        [SerializeField] private GoblinItemDef? steelSwordDef;
        [SerializeField] private GoblinItemDef? magicStaffDef;
        [SerializeField] private GoblinItemDef? goblinKingsCrownDef;

        [Header("Selection weight for each captain sub-table (should sum to 1).")]
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


        private LootPipeline<GoblinItemDef> _goblinPipeline = null!;
        private LootPipeline<GoblinItemDef> _captainPipeline = null!;
        private SoftPityStrategy<GoblinItemDef> _captainPity = null!;
        private ILootTable<GoblinItemDef> _activeGoblinTable = null!;
        private ILootTable<GoblinItemDef> _activeCaptainTable = null!;
        private Context _context = null!;

        private List<BuiltLootResult<GoblinItemDef, GoblinItemInstance>> _reusableResults = new();

        private void Awake() {
            _context = new Context()
                .Set(Luck, playerLuck)
                .Set(PlayerLevel, 10);
            _activeGoblinTable = ResolveGoblinTable();
            _activeCaptainTable = ResolveCaptainTable();
            _reusableResults = new List<BuiltLootResult<GoblinItemDef, GoblinItemInstance>>();
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
        /// Factory method to create runtime instances from definitions.
        /// Demonstrates how to inject logic (like luck-based value scaling) during the build process.
        /// </summary>
        public GoblinItemInstance Create(GoblinItemDef definition, Context context, IRandom random) {
            return new GoblinItemInstance {
                Name = definition.ItemName,
                Color = definition.Color,
                Icon = definition.IconUnicode,
                // Fuller example: randomize value slightly based on luck
                Value = Mathf.RoundToInt(definition.BaseValue * random.Range(0.9f, 1.1f) * (1f + context.GetOrDefault(Luck) * 0.5f))
            };
        }

        /// <summary>
        /// Returns the designer asset if one is assigned; otherwise builds a code-defined
        /// table using the fallback references.
        /// </summary>
        private ILootTable<GoblinItemDef> ResolveGoblinTable() {
            if (goblinTable != null)
                return goblinTable.ToTable();

            return new LootTable<GoblinItemDef>()
                .Add(coinDef!, 60f, 1, 10)
                .Add(healthPotionDef!, 20f)
                .Add(manaPotionDef!, 10f)
                .Add(ironSwordDef!, 7f)
                .Add(steelSwordDef!, 2f)
                .Add(magicStaffDef!);
        }

        /// <summary>
        /// Builds the captain's <see cref="LootTable{T}"/> using <see cref="CompositeTableBuilder{T}"/>.
        /// Uses the three designer assets if all are assigned; otherwise falls back to
        /// inline code-defined sub-tables using the fallback references.
        /// </summary>
        private ILootTable<GoblinItemDef> ResolveCaptainTable() {
            var common = captainCommonTable != null ? captainCommonTable.ToTable() : new LootTable<GoblinItemDef>();
            if (captainCommonTable == null) {
                common.Add(coinDef!, weight: 60f, minQuantity: 5, maxQuantity: 25);
                common.Add(healthPotionDef!, weight: 25f);
                common.Add(manaPotionDef!, weight: 15f);
            }

            var weapons = captainWeaponTable != null ? captainWeaponTable.ToTable() : new LootTable<GoblinItemDef>();
            if (captainWeaponTable == null) {
                weapons.Add(ironSwordDef!, weight: 50f);
                weapons.Add(steelSwordDef!, weight: 35f);
                weapons.Add(magicStaffDef!, weight: 15f);
            }

            var rare = captainRareTable != null ? captainRareTable.ToTable() : new LootTable<GoblinItemDef>();
            if (captainRareTable == null) {
                rare.Add(goblinKingsCrownDef!, weight: 1f);
            }

            return new CompositeTableBuilder<GoblinItemDef>()
                .Add(common, captainCommonWeight)
                .Add(weapons, captainWeaponWeight)
                .Add(rare, captainRareWeight)
                .Build();
        }

        private void BuildCaptainPipeline() {
            //  1. ModifyWeight — GoblinKingsCrown weight scales with player luck
            //  2. WeightedRandom(1) — single roll from the Flattened table
            //  3. SoftPity — ramps up the rare drop chance after consecutive failures
            _captainPity = new SoftPityStrategy<GoblinItemDef>(softPityStart, hardPityAt);

            _captainPipeline = new LootPipeline<GoblinItemDef>()
                .WithRandom(UnityRandom.Instance)
                .AddStrategy(new ModifyWeightStrategy<GoblinItemDef>((entry, ctx) => {
                    if (goblinKingsCrownDef == null || !ReferenceEquals(entry.Entry.Item, goblinKingsCrownDef))
                        return entry.Weight;
                    var luck = ctx.GetOrDefault(Luck);
                    return entry.Weight * (1f + luck * 3f);
                }))
                .AddStrategy(new WeightedRandomStrategy<GoblinItemDef>(rollCount: 1))
                .AddStrategy(_captainPity)
                .AddObserver(new ConsoleObserver<GoblinItemDef>("Captain"));
        }

        private void BuildGoblinPipeline() {
            //  1. ModifyWeight — rare items (SteelSword, MagicStaff) get a luck bonus
            //  2. WeightedRandom(3) — three rolls per kill
            //  3. BonusRoll — luck-based chance at a 4th free roll
            //  4. Consolidate — stack repeated Coins / Potions
            _goblinPipeline = new LootPipeline<GoblinItemDef>()
                .WithRandom(UnityRandom.Instance)
                .AddStrategy(new ModifyWeightStrategy<GoblinItemDef>((entry, ctx) => {
                    var item = entry.Entry.Item;
                    if (ReferenceEquals(item, steelSwordDef) || ReferenceEquals(item, magicStaffDef)) {
                        var luck = ctx.GetOrDefault(Luck);
                        return entry.Weight * (1f + luck * 3f); // up to 4× at max luck
                    }

                    return entry.Weight;
                }))
                .AddStrategy(new WeightedRandomStrategy<GoblinItemDef>(rollCount: 3))
                .AddStrategy(new BonusRollStrategy<GoblinItemDef>(Luck))
                .AddStrategy(new ConsolidateResultsStrategy<GoblinItemDef>())
                .AddObserver(new ConsoleObserver<GoblinItemDef>("Goblin"));
        }

        private void UpdateContext() => _context.Set(Luck, playerLuck);

        [ContextMenu("Kill Goblin (roll loot)")]
        public List<BuiltLootResult<GoblinItemDef, GoblinItemInstance>> KillGoblin() {
            UpdateContext();
            _context.Set(Source, "Goblin");
            _reusableResults.Clear();
            _goblinPipeline.ExecuteAndBuild(_activeGoblinTable, this, _reusableResults, _context);
            LogBuiltResults("Goblin", _reusableResults);
            return _reusableResults;
        }

        [ContextMenu("Kill Goblin Captain (roll loot)")]
        public List<BuiltLootResult<GoblinItemDef, GoblinItemInstance>> KillGoblinCaptain() {
            UpdateContext();
            _context.Set(Source, "Goblin Captain");
            _reusableResults.Clear();
            _captainPipeline.ExecuteAndBuild(_activeCaptainTable, this, _reusableResults, _context);
            LogBuiltResults("Goblin Captain", _reusableResults);
            Debug.Log($"[Pity] Current failure count: {_captainPity.GetFailureCount(_activeCaptainTable.Id)}");
            return _reusableResults;
        }

        [ContextMenu("Reset Captain Pity Counter")]
        public void ResetCaptainPity() {
            _captainPity.ResetAll();
            Debug.Log("[UnifiedLoot] Captain pity counter reset.");
        }

        private void LogBuiltResults(string source, List<BuiltLootResult<GoblinItemDef, GoblinItemInstance>> results) {
            if (results.Count == 0)
                return;
            var sb = new StringBuilder($"[UnifiedLoot] {source} built:  ");
            foreach (var r in results)
                sb.Append($"{r}   ");
            Debug.Log(sb.ToString());
        }

        private sealed class ConsoleObserver<T> : ILootObserver<T> {
            private readonly string _tag;
            public ConsoleObserver(string tag) => _tag = tag;

            public void OnRollComplete(ILootTable<T> table, IReadOnlyList<LootResult<T>> results, Context context) {
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