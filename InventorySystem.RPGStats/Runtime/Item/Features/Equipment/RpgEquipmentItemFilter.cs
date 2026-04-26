using System.Collections.Generic;
using System.Linq;
using CupkekGames.InventorySystem;

namespace CupkekGames.InventorySystem.RPGStats
{
    /// <summary>
    /// RPG bridge: filters inventory by equipment-type tabs / consumable sentinel (matches sample UX).
    /// </summary>
    public sealed class RpgEquipmentItemFilter : IEquipmentItemFilter
    {
        public List<InventoryItem> Filter(
            List<InventoryItem> items,
            int filterIndex,
            IInventoryItemDatabase database,
            IReadOnlyList<EquipmentSlotDescriptor> descriptors)
        {
            if (filterIndex < 0)
                return items;

            int equipmentFilterCount = 0;
            for (int i = 0; i < descriptors.Count; i++)
            {
                if (!RpgEquipmentSlotMetadataInternals.IsConsumableCategory(descriptors[i]))
                    equipmentFilterCount++;
            }

            bool isEquipmentFilter = filterIndex < equipmentFilterCount;

            return items.Where(item =>
            {
                if (item == null)
                    return false;

                InventoryItemDefinition itemDefinition = database.GetItemDefinition(item);
                EquipableFeature equip = itemDefinition?.GetFeature<EquipableFeature>();

                if (isEquipmentFilter)
                {
                    int idx = 0;
                    for (int i = 0; i < descriptors.Count; i++)
                    {
                        if (RpgEquipmentSlotMetadataInternals.IsConsumableCategory(descriptors[i]))
                            continue;
                        if (idx == filterIndex)
                        {
                            return RpgEquipmentSlotMetadataInternals.TryGetEquipmentType(descriptors[i], out string typeKey) &&
                                   equip != null &&
                                   !equip.EquipmentType.IsEmpty &&
                                   equip.EquipmentType.Key == typeKey;
                        }

                        idx++;
                    }

                    return false;
                }

                return equip == null;
            }).ToList();
        }
    }
}
