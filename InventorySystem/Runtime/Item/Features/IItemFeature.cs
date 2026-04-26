using CupkekGames.Data;
using UnityEngine.UIElements;

namespace CupkekGames.InventorySystem
{
    public interface IItemFeature : IFeature
    {
        void BuildTooltipContent(VisualElement host, in ItemTooltipContext context);
    }
}
