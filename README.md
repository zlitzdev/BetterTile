# Better Tile

A custom editor tool for Unity designed to simplify tile workflows.

## Features

1. Easy and straightforward rule editing.
2. Tile decorator.
3. (Planned feature) Animation editing.

## Usage

1. Create a **Tile Set**: Right click > `Create/Zlitz/Extra2D/Better Tile/Tile Set`. Double-click to open editor.
2. Drop any related sliced textures in the designated area.
3. Create a new **Tile**: Add a new **Tile** in the **Tiles** list and customize its name, color and properties to suit your needs. A **Tile** can inherit from another **Tile** and add its sprites to the sprite pool. Select `Overwrite rules` to overwrites the base sprite pool instead of adding to it.
4. Create a new **Category**: Add a new **Category** in the **Categories** list and customize its name and color. Add any existing **Tile** to it. **Categories** are used to specify connection rules that are related to multiple **Tiles**.
5. Paint tile: Select `Tile` brush type, then select a **Tile** from the list. Click on any sprites used for that **Tile**.
6. Paint rule: Seelct `Connection` brush type, then select either a **Tile** or a **Category**. Click on any connections that need to be matched with the selected **Tile**/**Cateogry**.
7. Paint weight: Select `Weight` brush type, then enter a weight. Click on a sprite to change its weight in the sprite pool.
8. Special features:
- **Self**: A special rule that can be used in `Connection` painting. This is used to specify connection rules between a **Tile** and itself.
- **Decorator**: Decorator sprites are sprites placed in empty tiles of a **Tilemap**. To specify a sprite as a decorator sprite, paint in `Tile` mode. This is also used to specify connection rules between a **Tile** and an empty tile.
