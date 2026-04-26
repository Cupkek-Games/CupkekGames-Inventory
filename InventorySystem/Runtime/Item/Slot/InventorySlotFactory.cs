using System;
using UnityEngine;
using UnityEngine.UIElements;
using CupkekGames.Luna;

namespace CupkekGames.InventorySystem
{
    public delegate InventoryItemSlotController InventoryItemSlotFactoryDelegate(
        IInventoryItemDatabase database,
        GameObject owner,
        VisualElement parent,
        TooltipController tooltipController,
        TooltipPosition tooltipPosition);

    /// <summary>Built-in <see cref="InventoryItemSlotFactoryDelegate"/> values.</summary>
    public static class InventoryItemSlotFactory
    {
        public static readonly InventoryItemSlotFactoryDelegate Default = (db, owner, parent, tc, tp) =>
            new InventoryItemSlotController(db, owner, parent, tc, tp);

        /// <summary>
        /// One shared <see cref="IMissingInventoryItemDefinitionNotifier"/> dedupes missing-definition logs across all slots
        /// created with this factory (typical for a single inventory grid).
        /// </summary>
        public static InventoryItemSlotFactoryDelegate WithSharedMissingDefinitionNotifier(
            IMissingInventoryItemDefinitionNotifier notifier)
        {
            if (notifier == null)
                throw new ArgumentNullException(nameof(notifier));
            return (db, owner, parent, tc, tp) => new InventoryItemSlotController(db, owner, parent, tc, tp, notifier);
        }
    }
}
