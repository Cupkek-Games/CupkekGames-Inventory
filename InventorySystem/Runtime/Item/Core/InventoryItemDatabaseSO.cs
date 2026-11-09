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

    /// <summary>The icon of the item's definition. Throws when the item has no definition or the icon is missing.</summary>
    public Sprite GetItemIcon(InventoryItem item)
    {
      InventoryItemDefinition definition = GetItemDefinition(item.Key)
        ?? throw new KeyNotFoundException($"[Inventory] no item definition for key '{item.Key.Key}'.");
      return GetItemIcon(definition);
    }

    /// <summary>
    /// The sprite under <see cref="InventoryItemDefinition.IconKey"/>; an empty catalog on the key means
    /// <see cref="InventoryConstants.IconsCatalogId"/>. Throws when the catalog is not registered or holds
    /// no sprite for the key: a missing icon is a content bug, not a blank image.
    /// </summary>
    public Sprite GetItemIcon(InventoryItemDefinition definition)
    {
      CatalogKey iconRef = definition.IconKey;
      string iconCatalogId = string.IsNullOrEmpty(iconRef.Catalog)
        ? InventoryConstants.IconsCatalogId
        : iconRef.Catalog;

      IAssetCatalog<Sprite> iconCatalog = ServiceLocator.Get<IAssetCatalog<Sprite>>(iconCatalogId);
      if (!iconCatalog.TryGetValue(iconRef.Key, out Object found) || found is not Sprite sprite)
        throw new KeyNotFoundException(
          $"[Inventory] item '{definition.Name}' has no icon: catalog '{iconCatalogId}' holds no sprite for key '{iconRef.Key}'.");

      return sprite;
    }

    public InventoryItem CreateItem(InventoryItemReference itemReference)
    {
      var item = new InventoryItem(itemReference);

      InventoryItemDefinition definition = GetItemDefinition(itemReference.ItemKey);
      if (definition != null)
      {
        item.BaseMaxStackAmount = definition.MaxStackAmount;
        // The constructor clamped the amount to the default cap of 1; clamp it
        // again against the definition's real one.
        item.SetAmount(itemReference.Amount);
      }

      return item;
    }
  }
}
