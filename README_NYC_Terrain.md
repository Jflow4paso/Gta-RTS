## NYC terrain setup (Unity)

1) Create or duplicate a scene
- Duplicate `Assets/Scenes/GameScene.unity` to `Assets/Scenes/GameScene_NYC.unity`.
- Ensure there is a `Terrain` GameObject in the scene. You can assign `Assets/New Terrain.asset` to it or create a new Terrain from GameObject > 3D Object > Terrain.

2) Assign terrain layers
- Menu: Tools > Terrain > Assign Default TerrainLayers.
- This attaches layers from `Resources/Terrain/Layers` (grass, dirt, sand, soil) which reference textures at `Resources/Terrain/Textures`.

3) Optional: import an NYC heightmap
- If you have a DEM (heightmap) for NYC, import it as a Texture2D and make sure "Read/Write Enabled" is ON.
- Use Unity Terrain Tools (Window > Package Manager > Terrain Tools) to import the heightmap into the active Terrain.
- If you do not have a heightmap yet, you can keep the terrain flat.

4) Optional: paint textures via masks
- Put grayscale mask textures into `Assets/Resources/Terrain/Textures/` with names:
  - `grass-heightmap.png` (used for grass coverage)
  - `dirt.png`, `sand.png`, `soil.png` (optional coverage masks)
- Enable Read/Write for these textures (Import Settings).
- Menu: Tools > Terrain > Apply Splat From Masks. The tool samples the masks and paints the alphamap. Missing masks default to the first layer.

5) NavMesh and minimap
- `GameManager` expects a `Terrain` object with a `NavMeshSurface` component. It auto-bakes at runtime via `Globals.UpdateNavMeshSurface()`.
- Minimap sizing and world conversion are auto-configured in `GameManager._SetupMinimap()`.

6) Save map metadata (size, spawnpoints, scene)
- In your NYC scene, create a top-level `Spawnpoints` GameObject with one child per player.
- Menu: Tools > Maps > Extract Current Scene Metadata to save a `MapData` asset in `Assets/Resources/ScriptableObjects/Maps/`.

Notes
- The provided layers use textures: grass, dirt, sand, soil. You can add more TerrainLayers by creating new `.terrainlayer` assets and placing them in `Resources/Terrain/Layers`.
- The mask painter normalizes weights per pixel. If all masks are zero at a pixel, it assigns weight to the first layer.