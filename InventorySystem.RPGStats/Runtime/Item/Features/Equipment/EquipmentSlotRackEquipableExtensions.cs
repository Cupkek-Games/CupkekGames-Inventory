using System.Collections.Generic;
using CupkekGames.InventorySystem;

namespace CupkekGames.InventorySystem.RPGStats
{
    public static class EquipmentSlotRackEquipableExtensions
    {
        public static List<EquipableFeature> CollectEquipableFeatures(
            this EquipmentSlotRack rack,
            IInventoryItemDatabase database)
        {
            var equippedFeatures = new List<EquipableFeature>();
            if (rack == null || database == null)
                return equippedFeatures;

            foreach (InventoryItemSlotController slot in rack.Slots)
            {
                if (slot.IsEmpty || slot.Item == null)
                    continue;

                InventoryItemDefinition definition = database.GetItemDefinition(slot.Item);
                EquipableFeature feat = definition?.GetFeature<EquipableFeature>();
                if (feat != null)
                    equippedFeatures.Add(feat);
            }

            return equippedFeatures;
        }
    }
}
