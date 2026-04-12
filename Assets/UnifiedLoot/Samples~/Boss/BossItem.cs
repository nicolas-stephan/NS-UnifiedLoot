using UnityEngine;

namespace NS.UnifiedLoot.Examples {
    #region bossItemDef
    public enum BossItemRarity {
        Common,
        Rare,
        Legendary,
        Epic,
        Artifact
    }

    [System.Serializable]
    public class BossItemDef {
        public string ItemName = "Boss Drop";
        public Color Color = Color.white;
        public BossItemRarity Rarity = BossItemRarity.Common;
        public string IconUnicode = "??";
        public bool IsWeapon;

        public override string ToString() => $"{IconUnicode} {ItemName}";
    }
    #endregion

    #region bossItemInstance
    public class BossItemInstance {
        public string Name { get; set; } = string.Empty;
        public Color Color { get; set; }
        public BossItemRarity Rarity { get; set; }
        public string Icon { get; set; } = string.Empty;
        public int Power { get; set; }

        public override string ToString() => $"{Icon} {Name} (Power: {Power})";
    }
    #endregion
}
