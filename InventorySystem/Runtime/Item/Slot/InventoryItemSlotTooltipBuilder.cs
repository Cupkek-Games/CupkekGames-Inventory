using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using CupkekGames.Luna;

namespace CupkekGames.InventorySystem
{
    /// <summary>
    /// Builds <see cref="TooltipContainerSetup"/> for an inventory slot from item + definition + optional stat comparison.
    /// <para>
    /// Game-specific runtime state (farm timers, market prices, quest progress, etc.) can be
    /// injected via <see cref="AddExtensionProvider{T}"/> and consumed by features through
    /// <see cref="ItemTooltipContext.GetExtension{T}"/>.
    /// </para>
    /// </summary>
    public sealed class InventoryItemSlotTooltipBuilder
    {
        private readonly IInventoryItemDatabase _database;
        private readonly GameObject _owner;
        private readonly IMissingInventoryItemDefinitionNotifier _missingDefinitionNotifier;

        private List<ExtensionProviderEntry> _extensionProviders;

        private struct ExtensionProviderEntry
        {
            public Type Type;
            public Func<InventoryItem, InventoryItemDefinition, object> Factory;
        }

        public InventoryItemSlotTooltipBuilder(
            IInventoryItemDatabase database,
            GameObject owner,
            IMissingInventoryItemDefinitionNotifier missingDefinitionNotifier)
        {
            _database = database;
            _owner = owner;
            _missingDefinitionNotifier = missingDefinitionNotifier ??
                                         throw new ArgumentNullException(nameof(missingDefinitionNotifier));
        }

        /// <summary>
        /// Registers a factory that supplies runtime data of type <typeparamref name="T"/> to
        /// tooltip features via <see cref="ItemTooltipContext.GetExtension{T}"/>.
        /// Replaces any previously registered provider for the same type.
        /// </summary>
        public void AddExtensionProvider<T>(
            Func<InventoryItem, InventoryItemDefinition, T> factory) where T : class
        {
            if (factory == null) throw new ArgumentNullException(nameof(factory));
            _extensionProviders ??= new List<ExtensionProviderEntry>();

            var entry = new ExtensionProviderEntry
            {
                Type = typeof(T),
                Factory = (item, def) => factory(item, def)
            };

            for (int i = 0; i < _extensionProviders.Count; i++)
            {
                if (_extensionProviders[i].Type == entry.Type)
                {
                    _extensionProviders[i] = entry;
                    return;
                }
            }
            _extensionProviders.Add(entry);
        }

        /// <summary>
        /// Removes the extension provider for <typeparamref name="T"/> if one is registered.
        /// </summary>
        public void RemoveExtensionProvider<T>() where T : class
        {
            if (_extensionProviders == null) return;
            var type = typeof(T);
            for (int i = 0; i < _extensionProviders.Count; i++)
            {
                if (_extensionProviders[i].Type == type)
                {
                    _extensionProviders.RemoveAt(i);
                    return;
                }
            }
        }

        private List<KeyValuePair<Type, object>> BuildExtensions(
            InventoryItem item, InventoryItemDefinition definition)
        {
            if (_extensionProviders == null || _extensionProviders.Count == 0)
                return null;

            var extensions = new List<KeyValuePair<Type, object>>(_extensionProviders.Count);
            for (int i = 0; i < _extensionProviders.Count; i++)
            {
                var provider = _extensionProviders[i];
                object value = provider.Factory(item, definition);
                if (value != null)
                    extensions.Add(new KeyValuePair<Type, object>(provider.Type, value));
            }
            return extensions.Count > 0 ? extensions : null;
        }

        /// <param name="slotIconBackground">Icon source from the slot button; use <c>default</c> when the slot has no background yet.</param>
        public TooltipContainerSetup Build(
            InventoryItem item,
            InventoryItemDefinition itemDefinition,
            StyleBackground slotIconBackground,
            ICollection<string> attributeNames,
            Func<InventoryItem, InventoryItemDefinition, ItemStatData> getComparison)
        {
            if (item == null)
                return new TooltipContainerSetup(null, new Label(""), new Label(""), null);

            InventoryItemDefinition definition = itemDefinition ?? _database?.GetItemDefinition(item);
            if (definition == null)
            {
                string key = item.Key.IsEmpty ? "<empty-key>" : item.Key.Key;
                _missingDefinitionNotifier.NotifyMissingDefinition(key);
            }

            Label tooltipName = new Label(item.DisplayName(definition));
            Label tooltipDescription = new Label(definition?.Description ?? string.Empty);
            VisualElement tooltipIcon = new VisualElement();
            tooltipIcon.style.backgroundImage = slotIconBackground;

            VisualElement bottom = new VisualElement();
            bottom.AddToClassList("item-tooltip-stats");
            var tooltipContext = new ItemTooltipContext(
                _database,
                item,
                definition,
                attributeNames,
                getComparison,
                _owner,
                BuildExtensions(item, definition));
            item.BuildItemTooltipStats(bottom, in tooltipContext);

            return new TooltipContainerSetup(tooltipIcon, tooltipName, tooltipDescription, bottom);
        }
    }
}
