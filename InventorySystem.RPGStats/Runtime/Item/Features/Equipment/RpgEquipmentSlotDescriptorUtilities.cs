using System.Collections.Generic;
using CupkekGames.InventorySystem;
using UnityEngine;

namespace CupkekGames.InventorySystem.RPGStats
{
    public static class RpgEquipmentSlotDescriptorUtilities
    {
        /// <summary>
        /// Appends one equipment-type slot to <paramref name="target"/> — the single-slot form of
        /// <see cref="AppendEquipmentTypeSlots"/> for callers that build their layout row by row
        /// (e.g. iterating a name-keyed slot map).
        /// </summary>
        public static void AppendEquipmentTypeSlot(
            IList<EquipmentSlotDescriptor> target,
            string equipmentTypeKey,
            string rootElementName,
            Sprite emptyIcon = null,
            string childElementName = null,
            string displayName = null)
        {
            if (target == null || string.IsNullOrEmpty(equipmentTypeKey) || string.IsNullOrEmpty(rootElementName))
                return;

            var layout = new RpgEquipmentSlotLayoutBuilder();
            layout.AddEquipmentTypeSlot(equipmentTypeKey, rootElementName, emptyIcon, childElementName, displayName);
            foreach (EquipmentSlotDescriptor d in layout.Build())
                target.Add(d);
        }

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
