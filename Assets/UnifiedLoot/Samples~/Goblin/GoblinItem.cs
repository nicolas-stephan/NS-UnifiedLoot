using UnityEngine;

namespace NS.UnifiedLoot.Examples {
    #region goblinItemDef
    public enum Rarity {
        Common,
        Rare,
        Legendary
    }

    [System.Serializable]
    public class GoblinItemDef {
        public string ItemName = "New Item";
        public Color Color = Color.white;
        public int BaseValue = 10;
        public string IconUnicode = "📦";
        public Rarity Rarity = Rarity.Common;

        public override string ToString() => $"{IconUnicode} {ItemName}";
    }
    #endregion

    #region goblinItemInstance
    public class GoblinItemInstance {
        public string Name { get; set; } = string.Empty;
        public Color Color { get; set; }
        public int Value { get; set; }
        public string Icon { get; set; } = string.Empty;
        public Rarity Rarity { get; set; }

        public override string ToString() => $"{Icon} {Name} ({Value}g)";
    }
    #endregion
}