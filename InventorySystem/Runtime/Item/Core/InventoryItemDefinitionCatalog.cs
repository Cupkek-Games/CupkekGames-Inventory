using CupkekGames.Data;
using UnityEngine;

namespace CupkekGames.InventorySystem
{
    [CreateAssetMenu(
        fileName = "InventoryItemDefinitionCatalog",
        menuName = "CupkekGames/Inventory/Catalog/Item Definitions")]
    public class InventoryItemDefinitionCatalog : AssetCatalog<InventoryItemDefinitionSO> { }
}
