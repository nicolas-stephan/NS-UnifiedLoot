using System.Collections.Generic;
using System.Text;
using UnityEngine;
using NS.UnifiedLoot.UnifiedLoot.Runtime.Core;
using NS.UnifiedLoot.UnifiedLoot.Runtime.Random;
using NS.UnifiedLoot.UnifiedLoot.Runtime.Strategies;

namespace NS.UnifiedLoot.Examples {
    /// <summary>
    /// Dynamic Scaling Example.
    /// Demonstrates how to adjust loot quality and quantity based on player stats and game difficulty.
    /// </summary>
    public class ScalingExample : MonoBehaviour, ILootFactory<ScalingItemDef, ScalingItemInstance> {
        private static readonly Key<int> PlayerLevel = new("PlayerLevel");
        private static readonly Key<float> Difficulty = new("Difficulty");

        [Header("Tables")]
        [SerializeField] private ScalingItemTable? equipmentTable;

        [Header("Player Settings")]
        [Range(1, 100)]
        [SerializeField] private int playerLevel = 1;
        [Range(1f, 5f)]
        [SerializeField] private float gameDifficulty = 1f;

        private LootPipeline<ScalingItemDef> _scalingPipeline = null!;
        private readonly Context _context = new();
        private readonly List<BuiltLootResult<ScalingItemDef, ScalingItemInstance>> _results = new();

        #region scalingPipeline
        private void Awake() {
            // Build the scaling pipeline
            _scalingPipeline = new LootPipeline<ScalingItemDef>()
                .WithRandom(UnityRandom.Instance)
                // 1. Dynamic Weighting: Rare items get more likely as level increases
                .AddStrategy(new ModifyWeightStrategy<ScalingItemDef>((entry, ctx) => {
                    var level = ctx.GetOrDefault(PlayerLevel, 1);
                    var rarity = entry.Entry.Item?.rarity ?? ScalingRarity.Common;

                    return rarity switch {
                        ScalingRarity.Poor => entry.Weight / (1f + level * 0.1f),
                        ScalingRarity.Rare => entry.Weight * (1f + level * 0.05f),
                        ScalingRarity.Epic => entry.Weight * (1f + level * 0.025f),
                        _ => entry.Weight
                    };
                }))
                // 2. Main roll
                .AddStrategy(new WeightedRandomStrategy<ScalingItemDef>(rollCount: 2))
                // 3. Dynamic Quantity: More loot on higher difficulties
                .AddStrategy(new ModifyQuantityStrategy<ScalingItemDef>((qty, ctx) => {
                    var diff = ctx.GetOrDefault(Difficulty, 1f);
                    return Mathf.RoundToInt(qty * diff);
                }))
                .AddStrategy(new ConsolidateResultsStrategy<ScalingItemDef>());
        }
        #endregion

        #region rollScalingLoot
        [ContextMenu("Roll Scaling Loot")]
        public void RollLoot() {
            _context
                .Set(PlayerLevel, playerLevel)
                .Set(Difficulty, gameDifficulty);

            _results.Clear();
            _scalingPipeline.ExecuteAndBuild(equipmentTable!.ToTable(), this, _results, _context);

            LogResults();
        }
        #endregion

        #region scalingFactory
        public ScalingItemInstance Create(ScalingItemDef definition, Context context, IRandom random) {
            var level = context.GetOrDefault(PlayerLevel, 1);
            var diff = context.GetOrDefault(Difficulty, 1f);

            // Scale power based on base range, player level, and difficulty
            float basePower = random.Range(definition.minBasePower, definition.maxBasePower);
            var finalPower = Mathf.RoundToInt(basePower * (1f + level * 0.1f) * diff);

            return new ScalingItemInstance {
                Name = definition.itemName,
                Rarity = definition.rarity,
                Color = definition.color,
                Power = finalPower
            };
        }
        #endregion

        private void LogResults() {
            var sb = new StringBuilder();
            sb.AppendLine($"<b>[Dynamic Scaling]</b> (Lvl: {playerLevel}, Diff: {gameDifficulty:F1})");
            if (_results.Count == 0)
                sb.AppendLine("- No items dropped.");
            else
                foreach (var result in _results)
                    sb.AppendLine($"- {result}");

            Debug.Log(sb.ToString());
        }
    }
}