using System;
using System.Collections.Generic;
using UnityEngine;

namespace CupkekGames.InventorySystem
{
    /// <summary>
    /// UI binding + persistence anchor for one logical slot on an equipment-style rack.
    /// Rules for which items fit: optional per-row <see cref="AcceptPolicy"/> (runtime only), else the default policy
    /// passed to <see cref="EquipmentSlotRack.Rebuild"/>. Bridge layers may also use <see cref="Metadata"/>.
    /// </summary>
    [Serializable]
    public class EquipmentSlotDescriptor
    {
        /// <summary>Optional persistence override when <see cref="SlotId"/> is empty — stored in <see cref="Metadata"/>.</summary>
        public const string SaveKeyMetadataKey = "saveKey";

        [Tooltip("Stable key for save data (e.g. Hand, Consumable).")]
        public string SlotId = "";

        /// <summary>Opaque entries (e.g. bridge-specific keys). Core does not interpret values.</summary>
        public List<SlotMetadataEntry> Metadata = new();

        public string DisplayName = "";
        public Sprite EmptyIcon;

        [Tooltip("Query under the screen root for this slot's container.")]
        public string RootElementName = "";

        [Tooltip("If set, slot is RootElementName.Q(ChildElementName), e.g. InventoryItem under EquipmentHand.")]
        public string ChildElementName = "";

        /// <summary>
        /// Per-slot acceptance; not serialized (Unity cannot persist interface references). When null, the rack uses its
        /// default <see cref="IEquipmentSlotAcceptPolicy"/>.
        /// </summary>
        [NonSerialized]
        public IEquipmentSlotAcceptPolicy AcceptPolicy;

        /// <summary>Ordinal key lookup in <see cref="Metadata"/>.</summary>
        public bool TryGetMetadata(string key, out string value)
        {
            value = null;
            if (Metadata == null || string.IsNullOrEmpty(key))
                return false;

            for (int i = 0; i < Metadata.Count; i++)
            {
                SlotMetadataEntry e = Metadata[i];
                if (string.Equals(e.Key, key, StringComparison.Ordinal))
                {
                    value = e.Value;
                    return true;
                }
            }

            return false;
        }

        public string GetMetadataOrDefault(string key, string defaultValue = "")
            => TryGetMetadata(key, out string v) ? v : defaultValue;

        public void SetOrReplaceMetadata(string key, string value)
        {
            if (Metadata == null || string.IsNullOrEmpty(key))
                return;

            for (int i = 0; i < Metadata.Count; i++)
            {
                if (string.Equals(Metadata[i].Key, key, StringComparison.Ordinal))
                {
                    Metadata[i] = new SlotMetadataEntry { Key = key, Value = value };
                    return;
                }
            }

            Metadata.Add(new SlotMetadataEntry { Key = key, Value = value });
        }

        /// <summary>
        /// <see cref="SlotId"/> if set; else metadata <see cref="SaveKeyMetadataKey"/>; else <see cref="RootElementName"/>.
        /// </summary>
        public string GetPersistenceKey()
        {
            if (!string.IsNullOrEmpty(SlotId))
                return SlotId;
            if (TryGetMetadata(SaveKeyMetadataKey, out string saveKey) && !string.IsNullOrEmpty(saveKey))
                return saveKey;
            return RootElementName;
        }
    }
}
