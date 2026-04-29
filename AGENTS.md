# CupkekGames Inventory — AI Agent Instructions

## Package Overview

**CupkekGames Inventory** (`com.cupkekgames.inventory`) is the inventory + equipment system. Items, stacks, slots, drag-and-drop, equipment-stat modifiers via RPGStats bridge.

## Critical: Do not hand-edit Unity serialized assets or `.meta` files

Apply scene/SO changes in Unity Editor; preserve `.meta` GUIDs.

## Package Structure

```
com.cupkekgames.inventory/
  package.json
  README.md
  AGENTS.md
  InventorySystem/                ← CupkekGames.InventorySystem.asmdef
    Runtime/                        (items, stacks, slots, drag-and-drop, equipment)
    Editor/
  InventorySystem.RPGStats/       ← CupkekGames.InventorySystem.RPGStats.asmdef
    Runtime/                        (equipment ↔ stat-modifier bridge)
```

## Dependencies

- `com.cupkekgames.singleton` (drag-session singleton)
- `com.cupkekgames.luna` (UI components for inventory display)
- `com.cupkekgames.data` (inventory data persistence)
- `com.cupkekgames.rpgstats` (stat modifiers)
- `com.cupkekgames.services`

## Coding Conventions

- **Namespaces**: `CupkekGames.InventorySystem`, `CupkekGames.InventorySystem.RPGStats`
- **Asmdefs**: GUID references
- **String keys for item IDs**: cross-references between equipment and stats use string keys
- **Strict typing**
