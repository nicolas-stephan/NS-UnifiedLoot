using UnityEngine;

namespace NS.UnifiedLoot.Examples {
    #region scalingItemDef
    public enum ScalingRarity {
        Poor,
        Common,
        Uncommon,
        Rare,
        Epic
    }

    [System.Serializable]
    public class ScalingItemDef {
        public string itemName = "Gear";
        public ScalingRarity rarity = ScalingRarity.Common;
        public Color color = Color.white;
        public int minBasePower = 10;
        public int maxBasePower = 20;

        public override string ToString() => $"[{rarity}] {itemName}";
    }
    #endregion

    #region scalingItemInstance
    public class ScalingItemInstance {
        public string Name { get; set; } = string.Empty;
        public ScalingRarity Rarity { get; set; }
        public int Power { get; set; }
        public Color Color { get; set; }

        public override string ToString() => $"{Name} ({Rarity}) - Power: {Power}";
    }
    #endregion
}