# CupkekGames Inventory

Inventory system: items, stacks, slots, drag-and-drop. Plus RPGStats bridge for equipment-stat modifiers.

## What's inside

- **`InventorySystem/`** (CupkekGames.InventorySystem.asmdef) — inventory data + UI: items, stacks, slots, drag-and-drop manipulators, equipment.
- **`InventorySystem.RPGStats/`** (CupkekGames.InventorySystem.RPGStats.asmdef) — bridge: equipping items applies stat modifiers via the RPGStats system.

## Slot templates

`InventoryItemSlotController` binds a slot template by element names: `Background` (the slot's `Button`, required), `Icon` (optional: the item art goes there instead of the button's own background-image; read it back from `ArtElement`), and the optional corner labels `TopLeft`, `TopRight`, `BottomLeft`, `BottomRight` (the amount, `x8` by default; override `FormatAmount(int)` for your own format). The tooltip's tier line carries `item-tooltip-tier` plus a tier modifier (`item-tooltip-tier--rare`) and no inline colour, so the theme colours it. Details: [Inventory docs](https://docs.cupkek.games/inventory).

## Dependencies

- `com.cupkekgames.singletons` (drag-session singleton)
- `com.cupkekgames.luna` (inventory UI)
- `com.cupkekgames.data` (inventory data via IData)
- `com.cupkekgames.rpgstats` (equipment-stat modifiers)
- `com.cupkekgames.services`

## Installation

Embedded package. Install the four dependencies above first.
