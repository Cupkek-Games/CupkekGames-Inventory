using CupkekGames.Data;
using UnityEngine.UIElements;

namespace CupkekGames.InventorySystem
{
    /// <summary>
    /// Optional feature hook for per-item slot chrome (e.g. <see cref="ItemTierFeature"/> USS on the slot background). Tooltip lines for the same concern live on <see cref="IItemFeature"/> (see <see cref="ItemTierFeature"/>).
    /// </summary>
    public interface IItemSlotVisual : IFeature
    {
        void ApplySlotVisual(VisualElement slotRoot, in ItemSlotContext context);
        void ClearSlotVisual(VisualElement slotRoot);
    }
}
