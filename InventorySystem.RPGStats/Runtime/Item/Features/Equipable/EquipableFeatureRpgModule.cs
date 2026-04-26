using UnityEngine;

namespace CupkekGames.InventorySystem.RPGStats
{
    [CreateAssetMenu(
        fileName = "EquipableFeatureRpgModule",
        menuName = "CupkekGames/Inventory/RPG Stats/Equipable Feature RPG Module")]
    public class EquipableFeatureRpgModule : ScriptableObject, IEquipableFeatureRpgModule
    {
        private IEquipableFeatureAggregationService _aggregation;
        private IEquipableFeatureStatPreviewService _statPreview;

        public IEquipableFeatureAggregationService Aggregation =>
            _aggregation ??= new EquipableFeatureAggregationService();

        public IEquipableFeatureStatPreviewService StatPreview =>
            _statPreview ??= new EquipableFeatureStatPreviewService();
    }
}
