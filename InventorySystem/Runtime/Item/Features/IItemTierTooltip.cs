using CupkekGames.Data;
using UnityEngine.UIElements;

namespace CupkekGames.InventorySystem
{
    /// <summary>
    /// Draws an item's tier in its tooltip the game's own way: a pill, or nothing here when another
    /// line carries it (a meal's row of pills). <see cref="ItemTierFeature.BuildTooltipContent"/>
    /// asks for one first through the tooltip's extensions
    /// (<see cref="InventoryItemSlotTooltipBuilder.AddExtensionProvider{T}"/>), then the
    /// <see cref="CupkekGames.Services.ServiceLocator"/>; with neither, the tier is the plain
    /// "Tier: Rare" line.
    /// </summary>
    public interface IItemTierTooltip
    {
        /// <summary>
        /// Adds the tier's element to <paramref name="host"/>: <paramref name="tier"/> is the item's
        /// effective tier key, <paramref name="display"/> its readable name (the raw key when no
        /// catalog names it).
        /// </summary>
        void Build(VisualElement host, CatalogKey tier, string display, in ItemTooltipContext context);
    }
}
