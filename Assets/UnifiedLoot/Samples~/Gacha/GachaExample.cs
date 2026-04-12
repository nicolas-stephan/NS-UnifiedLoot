using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using NS.UnifiedLoot;

namespace NS.UnifiedLoot.Examples {
    /// <summary>
    /// Gacha & Loot Box Example.
    /// Demonstrates pity systems, multi-pulls, and correct use of context reuse.
    /// </summary>
    public class GachaExample : MonoBehaviour, ILootFactory<GachaItemDef, GachaItemInstance> {
        [Header("Banner Settings")]
        [SerializeField] private GachaItemTable? bannerTable;

        [Header("Pity Settings")]
        [Tooltip("Hard pity for 5-star (guaranteed at 90)")]
        [SerializeField] private int hardPity5Star = 90;
        [Tooltip("Soft pity for 5-star (starts at 70)")]
        [SerializeField] private int softPityStart5Star = 70;
        [Tooltip("Hard pity for 4-star (guaranteed at 10)")]
        [SerializeField] private int hardPity4Star = 10;

        private LootPipeline<GachaItemDef> _gachaPipeline = null!;
        private GachaPitySystem _pitySystem = null!;
        private Context _context = null!;

        private readonly List<BuiltLootResult<GachaItemDef, GachaItemInstance>> _recentPulls = new();

        #region gachaPipeline
        private void Awake() {
            // 1. Initialize the pity system (holds state internally via IPityTracker)
            _pitySystem = new GachaPitySystem(hardPity4Star, hardPity5Star, softPityStart5Star);

            // 2. Reuse a single context for efficiency and data persistence across pulls
            _context = new Context();

            // 3. Build the gacha pipeline
            _gachaPipeline = new LootPipeline<GachaItemDef>()
                .WithRandom(UnityRandom.Instance)
                // Soft Pity: Modify weight of 5-star items using the pity system's current state
                .AddStrategy(new ModifyWeightStrategy<GachaItemDef>(_pitySystem.GetSoftPityWeight))
                // Main roll
                .AddStrategy(new WeightedRandomStrategy<GachaItemDef>(rollCount: 1))
                // Hard Pity: Force results if threshold reached and update pity state
                .AddStrategy(_pitySystem);
        }
        #endregion

        #region pullMethods
        [ContextMenu("Single Pull")]
        public void SinglePull() {
            _recentPulls.Clear();
            _gachaPipeline.ExecuteAndBuild(bannerTable!.ToTable(), this, _recentPulls, _context);

            LogResults("Single Pull");
        }

        [ContextMenu("Multi Pull (10x)")]
        public void MultiPull() {
            _recentPulls.Clear();

            Debug.Log("<color=orange><b>Multi-Pull Start!</b></color>");
            for (var i = 0; i < 10; i++)
                _gachaPipeline.ExecuteAndBuild(bannerTable!.ToTable(), this, _recentPulls, _context);

            LogResults("10-Pull");
        }
        #endregion

        public GachaItemInstance Create(GachaItemDef definition, Context context, IRandom random) {
            return new GachaItemInstance {
                Name = definition.ItemName,
                Rarity = definition.Rarity,
                Color = definition.Color,
                Icon = definition.IconUnicode
            };
        }

        private void LogResults(string label) {
            var sb = new StringBuilder();
            sb.AppendLine($"<b>[{label}]</b> Results:");
            foreach (var pull in _recentPulls) {
                var color = pull.Definition.Rarity switch {
                    GachaRarity.FiveStar => "yellow",
                    GachaRarity.FourStar => "purple",
                    _ => "white"
                };
                sb.AppendLine($"- <color={color}>{pull}</color>");
            }

            sb.AppendLine($"<i>Pity: 5? ({_pitySystem.Pity5}/{hardPity5Star}), 4? ({_pitySystem.Pity4}/{hardPity4Star})</i>");
            Debug.Log(sb.ToString());
        }

        #region pitySystem
        /// <summary>
        /// A cohesive pity system that handles soft pity weight modification,
        /// hard pity enforcement, and success/failure tracking.
        ///
        /// <para>By implementing both <see cref="ILootStrategy{T}"/> and <see cref="ILootObserver{T}"/>,
        /// it can be added to the pipeline once to handle multiple stages of the process.</para>
        /// </summary>
        private class GachaPitySystem : ILootGeneratorStrategy<GachaItemDef> {
            private const int Key4 = 4;
            private const int Key5 = 5;

            private readonly IPityTracker _tracker = new PityTracker();
            private readonly int _threshold4;
            private readonly int _threshold5;
            private readonly int _softPityStart5;


            public GachaPitySystem(int threshold4, int threshold5, int softPityStart5) {
                _threshold4 = threshold4;
                _threshold5 = threshold5;
                _softPityStart5 = softPityStart5;
            }

            public int Pity5 => _tracker.GetFailures(Key5);
            public int Pity4 => _tracker.GetFailures(Key4);

            /// <summary>
            /// Logic for soft pity weight modification.
            /// </summary>
            public float GetSoftPityWeight(WeightedEntry<GachaItemDef> we, Context context) {
                if (we.Entry.Item?.Rarity != GachaRarity.FiveStar)
                    return we.Weight;

                var pity = _tracker.GetFailures(Key5);
                if (pity < _softPityStart5)
                    return we.Weight;

                // Increase weight linearly after soft pity start
                var boost = 1f + (pity - _softPityStart5) * 10f;
                return we.Weight * boost;
            }

            /// <summary>
            /// Logic for hard pity enforcement (guaranteed drops).
            /// Runs after the main roll strategy.
            /// </summary>
            public void Process(LootWorkingSet<GachaItemDef> workingSet, Context context) {
                // If the roll didn't happen or produced nothing, we can't enforce pity here
                if (workingSet.Results.Count == 0)
                    return;

                var pity5 = _tracker.GetFailures(Key5);
                var pity4 = _tracker.GetFailures(Key4);

                // 1. Check 5-star hard pity (must happen first as it's higher priority)
                var got5Star = false;
                if (
                    pity5 >= _threshold5 - 1 &&
                    workingSet.Results.All(r => r.Item.Rarity != GachaRarity.FiveStar)
                ) {
                    workingSet.Results.Clear();
                    got5Star = TryRollFiltered(workingSet, GachaRarity.FiveStar);
                }

                // 2. Check 4-star hard pity (only if no 5-star or 4-star dropped)
                var hasHighRarity = !got5Star && workingSet.Results.Any(r => r.Item.Rarity >= GachaRarity.FourStar);
                var got4Star = false;
                if (!hasHighRarity && pity4 >= _threshold4 - 1) {
                    workingSet.Results.Clear();
                    got4Star = TryRollFiltered(workingSet, GachaRarity.FourStar);
                }

                // 5-star resets on 5-star drop, otherwise increments
                _tracker.Record(Key5, got5Star ? PityResult.Success : PityResult.Failure);

                // 4-star resets on either 4-star or 5-star drop, otherwise increments
                _tracker.Record(Key4, got5Star || got4Star ? PityResult.Success : PityResult.Failure);
            }

            private bool TryRollFiltered(LootWorkingSet<GachaItemDef> workingSet, GachaRarity rarity) {
                var filteredEntries = workingSet.WeightedEntries
                    .Where(we => we.Entry.Item != null && we.Entry.Item.Rarity == rarity)
                    .ToList();

                if (filteredEntries.Count == 0)
                    return false;

                var totalWeight = filteredEntries.Sum(we => we.Weight);
                var roll = workingSet.Random.Range(0f, totalWeight);
                var current = 0f;

                foreach (var we in filteredEntries) {
                    current += we.Weight;
                    if (!(roll <= current))
                        continue;
                    workingSet.AddResult(we.Entry, we.Index, totalWeight > 0 ? roll / totalWeight : 0);
                    return true;
                }

                // Fallback
                var first = filteredEntries[0];
                workingSet.AddResult(first.Entry, first.Index, 1f);
                return true;
            }
        }
        #endregion
    }
}
