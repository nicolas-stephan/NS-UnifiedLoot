using UnityEngine;

namespace NS.UnifiedLoot.Examples {
    #region gachaItemDef
    public enum GachaRarity {
        ThreeStar,
        FourStar,
        FiveStar
    }

    [System.Serializable]
    public class GachaItemDef {
        public string ItemName = "New Item";
        public GachaRarity Rarity = GachaRarity.ThreeStar;
        public Color Color = Color.white;
        public string IconUnicode = "??";

        public override string ToString() => $"[{Rarity}] {ItemName}";
    }
    #endregion

    #region gachaItemInstance
    public class GachaItemInstance {
        public string Name { get; set; } = string.Empty;
        public GachaRarity Rarity { get; set; }
        public Color Color { get; set; }
        public string Icon { get; set; } = string.Empty;

        public override string ToString() => $"{Icon} {Name} ({Rarity})";
    }
    #endregion
}
