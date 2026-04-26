using System;

namespace CupkekGames.InventorySystem
{
    /// <summary>
    /// Opaque key/value for <see cref="EquipmentSlotDescriptor.Metadata"/>. Meaning is defined by bridge/game code.
    /// </summary>
    [Serializable]
    public struct SlotMetadataEntry
    {
        public string Key;
        public string Value;
    }
}
