using UnityEngine;

namespace NS.UnifiedLoot.Examples {
    /// <summary>
    /// Complex loot definition for the goblin encounter example.
    /// </summary>
    [System.Serializable]
    public class GoblinItemDef {
        public string ItemName = "New Item";
        public Color Color = Color.white;
        public int BaseValue = 10;
        public string IconUnicode = "📦"; // For simple console visualization

        public override string ToString() => ItemName;
    }

    /// <summary>
    /// The runtime instance of a goblin item.
    /// Created by a factory during the loot roll, potentially with randomized stats.
    /// </summary>
    public class GoblinItemInstance {
        public string Name { get; set; } = string.Empty;
        public Color Color { get; set; }
        public int Value { get; set; }
        public string Icon { get; set; } = string.Empty;

        public override string ToString() => $"{Icon} {Name} ({Value}g)";
    }
}
