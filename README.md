# CupkekGames Inventory

Inventory system: items, stacks, slots, drag-and-drop. Plus RPGStats bridge for equipment-stat modifiers.

## What's inside

- **`InventorySystem/`** (CupkekGames.InventorySystem.asmdef) — inventory data + UI: items, stacks, slots, drag-and-drop manipulators, equipment.
- **`InventorySystem.RPGStats/`** (CupkekGames.InventorySystem.RPGStats.asmdef) — bridge: equipping items applies stat modifiers via the RPGStats system.

## Dependencies

- `com.cupkekgames.singletons` (drag-session singleton)
- `com.cupkekgames.luna` (inventory UI)
- `com.cupkekgames.data` (inventory data via IData)
- `com.cupkekgames.rpgstats` (equipment-stat modifiers)
- `com.cupkekgames.services`

## Installation

Embedded package. Install the four dependencies above first.
