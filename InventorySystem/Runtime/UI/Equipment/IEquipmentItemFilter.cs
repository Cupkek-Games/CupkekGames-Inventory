using System.Collections.Generic;

namespace CupkekGames.InventorySystem
{
    /// <summary>
    /// Filters inventory items by equipment UI filter index (e.g. per-slot tabs).
    /// </summary>
    public interface IEquipmentItemFilter
    {
        List<InventoryItem> Filter(
            List<InventoryItem> items,
            int filterIndex,
            IInventoryItemDatabase database,
            IReadOnlyList<EquipmentSlotDescriptor> descriptors);
    }
}
