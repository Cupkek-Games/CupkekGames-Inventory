using System;

namespace CupkekGames.InventorySystem
{
    /// <summary>
    /// Grid views that surface <see cref="InventoryItemSlotController"/> click as a single event (list + pagination).
    /// </summary>
    public interface IInventoryItemSlotGridView
    {
        event Action<InventoryItemSlotController> OnItemSlotClick;
    }
}
