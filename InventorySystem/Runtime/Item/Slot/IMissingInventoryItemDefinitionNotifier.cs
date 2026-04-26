using System.Collections.Generic;
using UnityEngine;

namespace CupkekGames.InventorySystem
{
    /// <summary>
    /// Raised when a slot needs an <see cref="InventoryItemDefinition"/> for tooltip/UI but none was resolved.
    /// Inject a shared instance across slots if you want one log per key per screen instead of per slot.
    /// </summary>
    public interface IMissingInventoryItemDefinitionNotifier
    {
        void NotifyMissingDefinition(string itemKey);
    }

    /// <summary>Logs at most once per distinct <paramref name="itemKey"/> for this notifier instance.</summary>
    public sealed class LogOnceMissingInventoryItemDefinitionNotifier : IMissingInventoryItemDefinitionNotifier
    {
        private readonly HashSet<string> _loggedKeys = new();

        public void NotifyMissingDefinition(string itemKey)
        {
            if (string.IsNullOrEmpty(itemKey))
                itemKey = "<empty-key>";
            if (!_loggedKeys.Add(itemKey))
                return;
            Debug.LogWarning(
                $"Inventory item slot: missing InventoryItemDefinition for item key '{itemKey}'. Tooltip will use fallback text.");
        }
    }

    /// <summary>No-op for tests or headless contexts.</summary>
    public sealed class NullMissingInventoryItemDefinitionNotifier : IMissingInventoryItemDefinitionNotifier
    {
        public void NotifyMissingDefinition(string itemKey) { }
    }
}
