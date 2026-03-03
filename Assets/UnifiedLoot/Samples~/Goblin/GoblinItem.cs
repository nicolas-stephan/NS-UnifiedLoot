namespace NS.UnifiedLoot.Examples {
    /// <summary>
    /// All droppable items in the goblin encounter example.
    /// Serialized directly into <see cref="GoblinItemTable"/> entries — Unity renders
    /// this enum as a dropdown in the Inspector.
    /// </summary>
    public enum GoblinItem {
        Coin,
        HealthPotion,
        ManaPotion,
        IronSword,
        SteelSword,
        MagicStaff,
        GoblinKingsCrown,
    }
}
