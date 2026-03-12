using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using NS.UnifiedLoot.UnifiedLoot.Runtime.Core;
using NS.UnifiedLoot.UnifiedLoot.Runtime.Random;
using NS.UnifiedLoot.UnifiedLoot.Runtime.Strategies;
using NS.UnifiedLoot.UnifiedLoot.Runtime.Tables;

namespace NS.UnifiedLoot.Examples {
    /// <summary>
    /// Gacha & Loot Box Example.
    /// Demonstrates pity systems, multi-pulls, and dynamic drop rates.
    /// </summary>
    public class GachaExample : MonoBehaviour, ILootFactory<GachaItemDef, GachaItemInstance> {
        private static readonly Key<int> PityCount5Star = new("Pity5Star");
        private static readonly Key<int> PityCount4Star = new("Pity4Star");

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
        private int _currentPity5Star;
        private int _currentPity4Star;

        private readonly List<BuiltLootResult<GachaItemDef, GachaItemInstance>> _recentPulls = new();

        #region gachaPipeline
        private void Awake() {
            // Build the gacha pipeline
            _gachaPipeline = new LootPipeline<GachaItemDef>()
                .WithRandom(UnityRandom.Instance)
                // 1. Soft Pity: Modify weight of 5-star items based on failure count
                .AddStrategy(new ModifyWeightStrategy<GachaItemDef>((entry, ctx) => {
                    if (entry.Entry.Item is not { Rarity: GachaRarity.FiveStar })
                        return entry.Weight;

                    var pity = ctx.GetOrDefault(PityCount5Star);
                    if (pity < softPityStart5Star)
                        return entry.Weight;

                    // Increase weight linearly after soft pity start
                    var boost = 1f + (pity - softPityStart5Star) * 10f;
                    return entry.Weight * boost;
                }))
                // 2. Main roll
                .AddStrategy(new WeightedRandomStrategy<GachaItemDef>(rollCount: 1))
                // 3. Hard Pity (4-star): If no 4-star or 5-star dropped in 10 rolls, force a 4-star
                .AddStrategy(new GachaHardPityStrategy(hardPity4Star, hardPity5Star))
                .AddObserver(new GachaObserver(this));
        }
        #endregion

        #region pullMethods
        [ContextMenu("Single Pull")]
        public void SinglePull() {
            _recentPulls.Clear();
            var context = CreateContext();

            _gachaPipeline.ExecuteAndBuild(bannerTable!.ToTable(), this, _recentPulls, context);

            LogResults("Single Pull");
        }

        [ContextMenu("Multi Pull (10x)")]
        public void MultiPull() {
            _recentPulls.Clear();

            Debug.Log("<color=orange><b>Multi-Pull Start!</b></color>");
            for (var i = 0; i < 10; i++) {
                var context = CreateContext();
                _gachaPipeline.ExecuteAndBuild(bannerTable!.ToTable(), this, _recentPulls, context);
            }

            LogResults("10-Pull");
        }
        #endregion

        private Context CreateContext() {
            return new Context()
                .Set(PityCount5Star, _currentPity5Star)
                .Set(PityCount4Star, _currentPity4Star);
        }

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

            sb.AppendLine($"<i>Pity: 5★ ({_currentPity5Star}/{hardPity5Star}), 4★ ({_currentPity4Star}/{hardPity4Star})</i>");
            Debug.Log(sb.ToString());
        }

        #region customStrategy
        /// <summary>
        /// Custom strategy to handle gacha-specific hard pity.
        /// If the roll produced nothing of high rarity, and we hit the threshold, force a high rarity drop.
        /// </summary>
        private class GachaHardPityStrategy : ILootGeneratorStrategy<GachaItemDef> {
            private readonly int _threshold4;
            private readonly int _threshold5;

            public GachaHardPityStrategy(int threshold4, int threshold5) {
                _threshold4 = threshold4;
                _threshold5 = threshold5;
            }

            public void Process(LootWorkingSet<GachaItemDef> workingSet, Context context) {
                var pity5 = context.GetOrDefault(PityCount5Star);
                var pity4 = context.GetOrDefault(PityCount4Star);

                // If we reached 5-star hard pity
                if (pity5 >= _threshold5 - 1) {
                    workingSet.Results.Clear();
                    TryRollFiltered(workingSet, entry => entry.Rarity == GachaRarity.FiveStar);
                    return;
                }

                // If we reached 4-star hard pity AND didn't get a 5-star
                var hasHighRarity = workingSet.Results.Any(r => r.Item.Rarity >= GachaRarity.FourStar);
                if (hasHighRarity || pity4 < _threshold4 - 1)
                    return;

                workingSet.Results.Clear();
                TryRollFiltered(workingSet, entry => entry.Rarity == GachaRarity.FourStar);
            }

            private void TryRollFiltered(LootWorkingSet<GachaItemDef> workingSet, System.Predicate<GachaItemDef> predicate) {
                var filteredEntries = workingSet.WeightedEntries
                    .Where(we => we.Entry.Item != null && predicate(we.Entry.Item))
                    .ToList();

                if (filteredEntries.Count == 0)
                    return;

                var totalWeight = filteredEntries.Sum(we => we.Weight);
                var roll = workingSet.Random.Range(0f, totalWeight);
                var current = 0f;

                foreach (var we in filteredEntries) {
                    current += we.Weight;
                    if (!(roll <= current))
                        continue;
                    workingSet.AddResult(we.Entry, we.Index, totalWeight > 0 ? roll / totalWeight : 0);
                    return;
                }

                // Fallback to first if somehow failed
                var first = filteredEntries[0];
                workingSet.AddResult(first.Entry, first.Index, 1f);
            }
        }
        #endregion

        #region gachaObserver
        private class GachaObserver : ILootObserver<GachaItemDef> {
            private readonly GachaExample _example;
            public GachaObserver(GachaExample example) => _example = example;

            public void OnRollComplete(ILootTable<GachaItemDef> table, IReadOnlyList<LootResult<GachaItemDef>> results, Context context) {
                var got5Star = results.Any(r => r.Item.Rarity == GachaRarity.FiveStar);
                var got4Star = results.Any(r => r.Item.Rarity == GachaRarity.FourStar);

                if (got5Star)
                    _example._currentPity5Star = 0;
                else
                    _example._currentPity5Star++;

                if (got5Star || got4Star)
                    _example._currentPity4Star = 0;
                else
                    _example._currentPity4Star++;
            }
        }
        #endregion
    }
}