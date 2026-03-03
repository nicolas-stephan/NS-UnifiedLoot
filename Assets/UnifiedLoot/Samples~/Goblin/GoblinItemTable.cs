using UnityEngine;

namespace NS.UnifiedLoot.Examples {
    /// <summary>
    /// Designer-editable loot table for the goblin encounter example.
    ///
    /// <para><b>How to create an asset:</b><br/>
    /// Right-click in the Project window → <c>Create → UnifiedLoot/Examples/Goblin Item Table</c>.
    /// </para>
    ///
    /// <para><b>How to fill it in:</b><br/>
    /// Expand the Entries list and add rows.  Each row has:
    /// <list type="bullet">
    ///   <item><b>Item</b> — pick from the <see cref="GoblinItem"/> dropdown.</item>
    ///   <item><b>Weight</b> — relative drop chance (higher = more common). The Inspector
    ///         shows the effective % next to each weight field.</item>
    ///   <item><b>Quantity</b> — min/max quantity rolled when this entry drops.</item>
    /// </list>
    /// </para>
    ///
    /// <para><b>Assign the asset</b> to the matching slot on <see cref="GoblinEncounterExample"/>
    /// and it will override the code-defined fallback table at runtime.</para>
    /// </summary>
    [CreateAssetMenu(
        menuName = "UnifiedLoot/Examples/Goblin Item Table",
        fileName = "New Goblin Item Table")]
    public class GoblinItemTable : LootTableAsset<GoblinItem> { }
}
