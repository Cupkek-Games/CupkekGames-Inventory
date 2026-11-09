using System;
using System.Collections.Generic;
using System.Globalization;
using CupkekGames.Data;
using CupkekGames.Luna;
using CupkekGames.Services;
using UnityEngine.UIElements;

namespace CupkekGames.InventorySystem
{
    /// <summary>
    /// Optional tier key for USS / slot visuals, tooltips, and any systems keyed by <see cref="InventoryConstants.ItemTierCatalogId"/>,
    /// shared by equipment, consumables, and other items.
    /// Tooltips resolve human-readable text via registered <see cref="IValueCatalog"/> (same as the CatalogKey inspector); fallback is the raw key.
    /// Slot USS classes on the background use <see cref="UssClassUtility.ToUssSlug"/> on <see cref="IValueCatalog.GetDisplayValue"/> when available (e.g. <c>common</c>),
    /// otherwise on the key (digits preserved). <see cref="ClearSlotVisual"/> removes legacy key classes and every slug that could be applied for known tier keys.
    /// The tooltip's tier line carries <see cref="TooltipTierUssClass"/> plus a modifier with the same slug, and no inline colour, so a theme colours it.
    /// </summary>
    [Serializable]
    [FeatureGroup("Presentation")]
    public sealed class ItemTierFeature : IItemFeature, IItemSlotVisual
    {
        /// <summary>
        /// USS class on the tooltip's tier line ("Tier: Rare"). The line also carries a modifier with
        /// the slug the slot gets (<c>rare</c> becomes <c>item-tooltip-tier--rare</c>), so a theme
        /// colours every tier line with <c>.item-tooltip-tier</c> and one tier with
        /// <c>.item-tooltip-tier--rare</c>. The modifier, not the bare slug, keeps a theme's slot tier
        /// rules (<c>.rare</c>) off the line. Without a theme rule the line takes the tooltip's label colour.
        /// </summary>
        public const string TooltipTierUssClass = "item-tooltip-tier";

        [CatalogKeyConstraint(InventoryConstants.ItemTierCatalogId)]
        public CatalogKey Tier;

        public ItemTierFeature() { }

        public ItemTierFeature(ItemTierFeature other)
        {
            Tier = other?.Tier ?? default;
        }

        /// <summary>Definition tier, or <see cref="ItemTierRuntimeState.TierOverride"/> on <paramref name="item"/> when set.</summary>
        public CatalogKey GetEffectiveTier(InventoryItem item)
        {
            if (item != null)
            {
                ItemTierRuntimeState state = item.GetState<ItemTierRuntimeState>();
                if (state != null && !state.TierOverride.IsEmpty)
                    return state.TierOverride;
            }

            return Tier;
        }

        /// <summary>
        /// <see cref="GetEffectiveTier"/> as a number, for games that key tiers by index ("0", "1", ...).
        /// Throws on an empty or non-numeric key: an authoring bug, not a tier.
        /// </summary>
        public int GetEffectiveTierIndex(InventoryItem item)
        {
            CatalogKey effective = GetEffectiveTier(item);
            if (effective.IsEmpty || !int.TryParse(effective.Key, NumberStyles.Integer, CultureInfo.InvariantCulture, out int tier))
                throw new FormatException(
                    $"[Inventory] tier key '{effective.Key}' on item '{item?.Key.Key}' is not a tier index.");

            return tier;
        }

        /// <summary>
        /// First non-empty <see cref="IValueCatalog.GetDisplayValue"/> for <paramref name="tier"/>'s catalog id and key; otherwise <c>null</c>.
        /// </summary>
        private static string TryResolveTierDisplay(CatalogKey tier)
        {
            if (tier.IsEmpty)
                return null;

            string catalogId = string.IsNullOrEmpty(tier.Catalog)
                ? InventoryConstants.ItemTierCatalogId
                : tier.Catalog;

            IReadOnlyList<IValueCatalog> catalogs = ServiceLocator.GetAll<IValueCatalog>(catalogId);
            for (int i = 0; i < catalogs.Count; i++)
            {
                string s = catalogs[i].GetDisplayValue(tier.Key);
                if (!string.IsNullOrEmpty(s))
                    return s;
            }

            return null;
        }

        /// <summary>
        /// USS slug for the tier: slug from display when resolved, else slug from key. The slot
        /// background carries it as is; the tooltip's tier line as a <see cref="TooltipTierUssClass"/> modifier.
        /// </summary>
        private static string GetTierUssClass(CatalogKey tier)
        {
            if (tier.IsEmpty)
                return null;

            string display = TryResolveTierDisplay(tier);
            if (!string.IsNullOrEmpty(display))
                return UssClassUtility.ToUssSlug(display);

            return UssClassUtility.ToUssSlug(tier.Key);
        }

        /// <summary>Catalog key with empty <see cref="CatalogKey.Catalog"/> so tier resolution uses <see cref="InventoryConstants.ItemTierCatalogId"/>.</summary>
        private static CatalogKey TierKeyForLookup(string tierKey)
        {
            return new CatalogKey { Catalog = "", Key = tierKey };
        }

        public void BuildTooltipContent(VisualElement host, in ItemTooltipContext context)
        {
            CatalogKey effective = GetEffectiveTier(context.Item);
            if (host == null || effective.IsEmpty)
                return;

            string display = TryResolveTierDisplay(effective) ?? effective.Key;
            var label = new Label($"Tier: {display}");
            label.AddToClassList("text-xl");
            label.AddToClassList("mt-0");
            label.AddToClassList("pt-0");
            // No inline colour: an inline style beats every sheet, so a light tooltip card could
            // not darken the line. The theme colours it through these two classes. The tier is a
            // modifier, not the slot's bare slug, so slot tier rules (.rare) stay off the line.
            label.AddToClassList(TooltipTierUssClass);
            string uss = GetTierUssClass(effective);
            if (!string.IsNullOrEmpty(uss))
                label.AddToClassList(TooltipTierUssClass + "--" + uss);
            host.Add(label);
        }

        public void ApplySlotVisual(VisualElement slotRoot, in ItemSlotContext context)
        {
            CatalogKey effective = GetEffectiveTier(context.Item);
            if (slotRoot == null || effective.IsEmpty)
                return;

            Button background = slotRoot.Q<Button>("Background");
            if (background == null)
                return;

            string uss = GetTierUssClass(effective);
            if (!string.IsNullOrEmpty(uss))
                background.AddToClassList(uss);
        }

        public void ClearSlotVisual(VisualElement slotRoot)
        {
            if (slotRoot == null)
                return;

            Button background = slotRoot.Q<Button>("Background");
            if (background == null)
                return;

            IReadOnlyList<ICatalog> catalogs = ServiceLocator.GetAll<ICatalog>(InventoryConstants.ItemTierCatalogId);
            var removed = new HashSet<string>(StringComparer.Ordinal);
            foreach (ICatalog catalog in catalogs)
            {
                foreach (string tierKey in catalog.GetKeys())
                {
                    if (string.IsNullOrEmpty(tierKey))
                        continue;

                    // Legacy: USS used raw catalog keys (e.g. "0").
                    if (removed.Add(tierKey))
                        background.RemoveFromClassList(tierKey);

                    CatalogKey lookup = TierKeyForLookup(tierKey);
                    string uss = GetTierUssClass(lookup);
                    if (!string.IsNullOrEmpty(uss) && removed.Add(uss))
                        background.RemoveFromClassList(uss);
                }
            }
        }

        public IFeature CloneFeature() => new ItemTierFeature(this);
    }
}
