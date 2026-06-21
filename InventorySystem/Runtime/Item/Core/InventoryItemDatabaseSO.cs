using System.Collections.Generic;
using CupkekGames.Data;
using CupkekGames.Services;
using UnityEngine;

namespace CupkekGames.InventorySystem
{
  /// <summary>
  /// ScriptableObject implementation of <see cref="IInventoryItemDatabase"/> backed by
  /// <see cref="IAssetCatalog"/> / <see cref="ICatalog"/> from <see cref="ServiceLocator"/>.
  /// Multiple <see cref="InventoryItemDefinitionCatalog"/> assets may register with the same
  /// <see cref="InventoryConstants.ItemsCatalogId"/> (<c>append: true</c>); this type merges keys and resolves definitions by querying each <see cref="IAssetCatalog{InventoryItemDefinitionSO}"/> until a key hits.
  /// </summary>
  [CreateAssetMenu(
    fileName = "InventoryItemDatabase",
    menuName = "CupkekGames/Inventory/Inventory Item Database")]
  public class InventoryItemDatabaseSO : ScriptableObject, IInventoryItemDatabase
  {
    private IAssetCatalog<Sprite> _iconCatalog;

    private static IReadOnlyList<IAssetCatalog<InventoryItemDefinitionSO>> ItemDefinitionCatalogs =>
      ServiceLocator.GetAll<IAssetCatalog<InventoryItemDefinitionSO>>(InventoryConstants.ItemsCatalogId);

    private static IReadOnlyList<ICatalog> ItemsCatalogs =>
      ServiceLocator.GetAll<ICatalog>(InventoryConstants.ItemsCatalogId);

    private static void EnsureItemsDatabasesRegistered()
    {
      if (ItemDefinitionCatalogs.Count == 0)
        throw new System.InvalidOperationException(
          $"No IAssetCatalog<InventoryItemDefinitionSO> registered with key '{InventoryConstants.ItemsCatalogId}'. " +
          "Add at least one InventoryItemDefinitionCatalog to your ServiceRegistry.");
    }

    private IAssetCatalog<Sprite> ResolveIconCatalog() =>
      _iconCatalog ??=
        ServiceLocator.Get<IAssetCatalog<Sprite>>(InventoryConstants.IconsCatalogId, silent: true);

    public IEnumerable<string> GetAllItemKeys()
    {
      var seen = new HashSet<string>();
      IReadOnlyList<ICatalog> catalogs = ItemsCatalogs;
      for (int i = 0; i < catalogs.Count; i++)
      {
        ICatalog cat = catalogs[i];
        if (cat == null)
          continue;
        foreach (string key in cat.GetKeys())
        {
          if (!string.IsNullOrEmpty(key))
            seen.Add(key);
        }
      }

      return seen;
    }

    public InventoryItemDefinition GetItemDefinition(CatalogKey itemKey)
    {
      if (itemKey.IsEmpty)
        return null;

      string catalog = string.IsNullOrEmpty(itemKey.Catalog)
        ? InventoryConstants.ItemsCatalogId
        : itemKey.Catalog;

      if (catalog != InventoryConstants.ItemsCatalogId)
        return null;

      return GetItemDefinition(itemKey.Key);
    }

    public InventoryItemDefinition GetItemDefinition(string key)
    {
      if (string.IsNullOrEmpty(key))
        return null;

      EnsureItemsDatabasesRegistered();

      IReadOnlyList<IAssetCatalog<InventoryItemDefinitionSO>> definitionCatalogs = ItemDefinitionCatalogs;
      for (int i = 0; i < definitionCatalogs.Count; i++)
      {
        // TryGetValue: a miss in one catalog of a multi-catalog probe is
        // expected and must not trigger GetValue's dev-build warning.
        if (definitionCatalogs[i] != null &&
            definitionCatalogs[i].TryGetValue(key, out var obj) &&
            obj is InventoryItemDefinitionSO definition)
          return definition.Data;
      }

      return null;
    }

    public Sprite GetItemIcon(InventoryItem item)
    {
      IAssetCatalog<Sprite> iconCatalog = ResolveIconCatalog();
      if (iconCatalog == null)
      {
        Debug.LogWarning(
          $"IAssetCatalog<Sprite>('{InventoryConstants.IconsCatalogId}') not found in ServiceLocator");
        return null;
      }

      InventoryItemDefinition definition = GetItemDefinition(item.Key);
      if (definition == null)
      {
        Debug.LogWarning(
          $"InventoryItemDefinition not found for key: {item.Key.Key}");
        return null;
      }

      CatalogKey iconRef = definition.IconKey;
      string iconCatalogId = string.IsNullOrEmpty(iconRef.Catalog)
        ? InventoryConstants.IconsCatalogId
        : iconRef.Catalog;

      IAssetCatalog<Sprite> resolvedIconCatalog =
        ServiceLocator.Get<IAssetCatalog<Sprite>>(iconCatalogId, silent: true) ?? iconCatalog;

      Sprite sprite = resolvedIconCatalog.GetValue(iconRef.Key);
      if (sprite == null)
        Debug.LogWarning($"Sprite not found for key: {iconRef.Key}");

      return sprite;
    }

    public InventoryItem CreateItem(InventoryItemReference itemReference)
    {
      var item = new InventoryItem(itemReference);

      InventoryItemDefinition definition = GetItemDefinition(itemReference.ItemKey);
      if (definition != null)
        item.BaseMaxStackAmount = definition.MaxStackAmount;

      return item;
    }
  }
}
