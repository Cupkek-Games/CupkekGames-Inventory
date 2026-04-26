using System.Collections.Generic;
using CupkekGames.InventorySystem;
using UnityEngine;

namespace CupkekGames.InventorySystem.RPGStats
{
    public static class RpgEquipmentSlotDescriptorUtilities
    {
        public static void AppendEquipmentTypeSlots(
            IList<EquipmentSlotDescriptor> target,
            IReadOnlyList<string> equipmentTypeKeys,
            IReadOnlyList<string> rootElementNames,
            IReadOnlyList<Sprite> backgrounds = null,
            string childElementName = null)
        {
            if (target == null || equipmentTypeKeys == null || rootElementNames == null)
                return;

            var layout = new RpgEquipmentSlotLayoutBuilder();
            layout.AddEquipmentTypeSlots(equipmentTypeKeys, rootElementNames, backgrounds, childElementName);
            foreach (EquipmentSlotDescriptor d in layout.Build())
                target.Add(d);
        }

        public static void AppendConsumableSlot(
            IList<EquipmentSlotDescriptor> target,
            string slotId,
            string displayName,
            string rootElementName,
            Sprite emptyIcon,
            string childElementName = "")
        {
            if (target == null)
                return;

            var layout = new RpgEquipmentSlotLayoutBuilder();
            layout.AddConsumableSlot(slotId, displayName, rootElementName, emptyIcon, childElementName);
            foreach (EquipmentSlotDescriptor d in layout.Build())
                target.Add(d);
        }
    }
}
