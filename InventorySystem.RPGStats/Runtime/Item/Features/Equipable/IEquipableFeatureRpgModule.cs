namespace CupkekGames.InventorySystem.RPGStats
{
    /// <summary>
    /// Composition root for equipable-related RPG helpers (aggregation, tooltip preview).
    /// Typically implemented by a <see cref="UnityEngine.ScriptableObject"/> asset listed under
    /// <c>ServiceRegistrySO._serviceEntries</c> (register as this interface).
    /// </summary>
    public interface IEquipableFeatureRpgModule
    {
        IEquipableFeatureAggregationService Aggregation { get; }
        IEquipableFeatureStatPreviewService StatPreview { get; }
    }
}
