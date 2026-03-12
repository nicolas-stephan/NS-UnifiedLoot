using System.Collections.Generic;
using System.Text;
using UnityEngine;
using NS.UnifiedLoot.UnifiedLoot.Runtime.Core;
using NS.UnifiedLoot.UnifiedLoot.Runtime.Random;
using NS.UnifiedLoot.UnifiedLoot.Runtime.Strategies;
using NS.UnifiedLoot.UnifiedLoot.Runtime.Tables;

namespace NS.UnifiedLoot.Examples {
    /// <summary>
    /// Boss Loot Example demonstrating multi-stage drops and guaranteed item types.
    ///
    /// <para><b>Multi-Stage Drops:</b></para>
    /// <para>Bosses often drop loot in stages: 100% guaranteed currency, a few rolls for materials,
    /// and a high-stakes roll for equipment.</para>
    ///
    /// <para><b>Guaranteed Item Types:</b></para>
    /// <para>Using <see cref="GuaranteedDropStrategy{T}"/> ensures that even with bad RNG,
    /// the player doesn't walk away empty-handed from a specific loot pool.</para>
    /// </summary>
    public class BossLootExample : MonoBehaviour, ILootFactory<BossItemDef, BossItemInstance> {
        [Header("Tables")]
        [SerializeField] private BossItemTable? currencyTable;
        [SerializeField] private BossItemTable? materialTable;
        [SerializeField] private BossItemTable? equipmentTable;

        private LootPipeline<BossItemDef> _currencyPipeline = null!;
        private LootPipeline<BossItemDef> _materialPipeline = null!;
        private LootPipeline<BossItemDef> _equipmentPipeline = null!;

        private readonly List<BuiltLootResult<BossItemDef, BossItemInstance>> _allDrops = new();
        private readonly Context _context = new();

        private void Awake() {
            // 1. Currency Pipeline: Simple weighted random, but we could use fixed quantities
            _currencyPipeline = new LootPipeline<BossItemDef>()
                .WithRandom(UnityRandom.Instance)
                .AddStrategy(new WeightedRandomStrategy<BossItemDef>(rollCount: 1))
                .AddObserver(new BossLootObserver("Currency"));

            // 2. Material Pipeline: Multiple rolls for various materials
            _materialPipeline = new LootPipeline<BossItemDef>()
                .WithRandom(UnityRandom.Instance)
                .AddStrategy(new WeightedRandomStrategy<BossItemDef>(rollCount: 5))
                .AddStrategy(new ConsolidateResultsStrategy<BossItemDef>())
                .AddObserver(new BossLootObserver("Materials"));

            // 3. Equipment Pipeline: One roll, guaranteed to drop something if the table is valid

            #region guaranteedDropStrategy
            _equipmentPipeline = new LootPipeline<BossItemDef>()
                .WithRandom(UnityRandom.Instance)
                .AddStrategy(new WeightedRandomStrategy<BossItemDef>(rollCount: 1))
                // If the equipment table had "Empty" entries or low drop chances,
                // this ensures we still get one item from the table.
                .AddStrategy(new GuaranteedDropStrategy<BossItemDef>())
                .AddObserver(new BossLootObserver("Equipment"));
            #endregion
        }

        #region multiStageRoll
        [ContextMenu("Kill Boss (Multi-Stage Loot)")]
        public void KillBoss() {
            _allDrops.Clear();
            Debug.Log("<color=red><b>BOSS KILLED!</b></color> Starting loot sequence...");

            // Stage 1: Currency
            if (currencyTable != null)
                _currencyPipeline.ExecuteAndBuild(currencyTable.ToTable(), this, _allDrops, _context);

            // Stage 2: Materials
            if (materialTable != null)
                _materialPipeline.ExecuteAndBuild(materialTable.ToTable(), this, _allDrops, _context);

            // Stage 3: Equipment
            if (equipmentTable != null)
                _equipmentPipeline.ExecuteAndBuild(equipmentTable.ToTable(), this, _allDrops, _context);

            LogResults();
        }
        #endregion

        #region bossFactory
        public BossItemInstance Create(BossItemDef definition, Context context, IRandom random) {
            var powerBase = definition.Rarity switch {
                BossItemRarity.Common => 10,
                BossItemRarity.Rare => 25,
                BossItemRarity.Epic => 50,
                BossItemRarity.Legendary => 100,
                BossItemRarity.Artifact => 250,
                _ => 0
            };

            return new BossItemInstance {
                Name = definition.ItemName,
                Color = definition.Color,
                Rarity = definition.Rarity,
                Icon = definition.IconUnicode,
                Power = powerBase + random.Range(0, 10)
            };
        }
        #endregion

        private void LogResults() {
            var sb = new StringBuilder();
            sb.AppendLine($"[Boss Loot] Total items dropped: {_allDrops.Count}");
            foreach (var drop in _allDrops)
                sb.AppendLine($"- {drop}");

            Debug.Log(sb.ToString());
        }

        private class BossLootObserver : ILootObserver<BossItemDef> {
            private readonly string _stage;
            public BossLootObserver(string stage) => _stage = stage;

            public void OnRollComplete(ILootTable<BossItemDef> table, IReadOnlyList<LootResult<BossItemDef>> results, Context context)
                => Debug.Log($"[Stage: {_stage}] Rolled {results.Count} items.");
        }
    }
}