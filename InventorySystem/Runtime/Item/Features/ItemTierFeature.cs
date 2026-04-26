using System;
using System.Collections.Generic;
using CupkekGames.Data;
using CupkekGames.Luna;
using CupkekGames.Systems;
using UnityEngine;
using UnityEngine.UIElements;

namespace CupkekGames.InventorySystem
{
    /// <summary>
    /// Optional tier key for USS / slot visuals, tooltips, and any systems keyed by <see cref="InventoryConstants.ItemTierCatalogId"/>,
    /// shared by equipment, consumables, and other items.
    /// Tooltips resolve human-readable text via registered <see cref="IValueCatalog"/> (same as the CatalogKey inspector); fallback is the raw key.
    /// Slot USS classes on the background use <see cref="UssClassUtility.ToUssSlug"/> on <see cref="IValueCatalog.GetDisplayValue"/> when available (e.g. <c>common</c>),
    /// otherwise on the key (digits preserved). <see cref="ClearSlotVisual"/> removes legacy key classes and every slug that could be applied for known tier keys.
    /// </summary>
    [Serializable]
    [FeatureGroup("Presentation")]
    public sealed class ItemTierFeature : IItemFeature, IItemSlotVisual
    {
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

        /// <summary>USS class for slot background: slug from display when resolved, else slug from key.</summary>
        private static string GetSlotUssClass(CatalogKey tier)
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
            label.style.color = Color.white;
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

            string uss = GetSlotUssClass(effective);
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
                    string uss = GetSlotUssClass(lookup);
                    if (!string.IsNullOrEmpty(uss) && removed.Add(uss))
                        background.RemoveFromClassList(uss);
                }
            }
        }

        public IFeature CloneFeature() => new ItemTierFeature(this);
    }
}
