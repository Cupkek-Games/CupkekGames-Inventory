using System.Collections.Generic;

namespace CupkekGames.InventorySystem
{
    /// <summary>
    /// Chooses which equipment slot index should receive an item (highlight, click-to-equip, etc.).
    /// </summary>
    public interface IEquipmentSlotTargeting
    {
        int GetTargetSlotIndex(
            InventoryItem item,
            InventoryItemDefinition definition,
            IReadOnlyList<EquipmentSlotDescriptor> descriptors,
            int slotCount);
    }
}
