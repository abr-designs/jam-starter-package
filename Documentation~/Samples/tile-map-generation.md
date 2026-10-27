---
title: Tile Map Generation
created_by: Claude (claude-opus-5-5)
reviewed_by: Alex Bedard-Reid
date: 2026-10-26
---
# Tile Map Generation

Builds a map out of tile prefabs (grass, sand, water, rock, etc.) from a single number called the **seed**. The same seed always builds the same map, so a level can be shared, replayed or rebuilt just by remembering its seed.

The sample ships with square tiles laid out as a grid or as a circle. The same pieces can be reused for any tile shape, see [Adding a New Map Shape](#adding-a-new-map-shape).

## Sample Scene

Open `MapTileGenerationSample.unity` and press Play. The `MapSample` component builds the map when the scene starts.

| Field | What it does |
|---|---|
| `Map Tileset` | The [Tileset](#tileset) to build the map from |
| `Map Seed` | The number the map is built from. Change it for a different map |
| `Use Radial` | Builds a circle instead of a rectangle |
| `Map Size` | Width & height of the rectangle, in tiles |
| `Map Radius` | Radius of the circle, in tiles |

While playing, press the `Regenerate` button on `MapSample` to rebuild the map.

## How It Fits Together

A map is made of four parts. Each part has one job:

| Part | Job | Sample version |
|---|---|---|
| **Tileset** | Lists which tiles exist & how often they appear | `Tileset.asset` |
| **Tile** | The component on each tile prefab | [`Square2DTile`](#tile-square2dtile) |
| **Map Generator** | Decides which tile goes where | [`Grid2DTileMapGenerator`, `Radial2DTileMapGenerator`](#map-generator-basemapgenerator) |
| **Map** | Holds the finished tiles & answers questions about them | [`Square2DTileMap`](#map-basemap) |

A fifth helper, the **Layout**, converts tile positions into world positions & lists each tile's neighbours. The square version is `Square2DLayout`.

The quickest way to build a map is one line:

```csharp
var map = Square2DTileMap.CreateGridMap(mapTileset, new Vector2Int(10, 10), transform, seed: 42);
```

| `Grid2DTileMapGenerator`                                               	| `Radial2DTileMapGenerator`                                             	|
|----------------------------------------------------------------------	|----------------------------------------------------------------------	|
| ![square](Documentation~/Images/Samples/map-tiles-square-layout.png) 	| ![radial](Documentation~/Images/Samples/map-tiles-radial-layout.png) 	|

## Tileset

Create one from **_Assets → Create → ScriptableObjects → Tileset_**.

| Field | What it does |
|---|---|
| `Tile Size` | Distance between the centres of two neighbouring tiles, in world units |
| `Generated Class Name` | Name of the [generated tile list](#tiletype). Empty uses the asset name followed by `Types` |
| `Generated Namespace` | Optional namespace for the generated tile list |
| `Tiles` | Every tile in the set (see below) |

Each entry in `Tiles` has:

| Field | What it does |
|---|---|
| `Name` | The tile's name, e.g. `Water`. This becomes its [TileType](#tiletype) |
| `Id` | Filled in automatically from the name. Read only |
| `Description` | Notes for yourself or for in-game text |
| `Spawn Weight` | How often the tile appears (`0` to `100`). A tile with `10` appears twice as often as a tile with `5`. `0` never appears |
| `Tile Variants` | One or more prefabs for this tile. One is picked at random each time the tile is placed |

> [!TIP]
> The sample `Tileset.asset` uses weights of Grass `10`, Rock `5`, Sand `3` & Water `6`.

## Tile: `Square2DTile`

`Square2DTile` is the component that goes on every square tile prefab. It knows its grid position & what kind of tile it is.

> [!IMPORTANT]
> Every tile prefab needs a `Collider`, assigned to the tile's `Collider` field. Clicks, hovering & `SetIsInteractable()` all use it.

```csharp
Vector2Int position = tile.Position;           // Where the tile sits on the grid, e.g. (3, 5)
int id = tile.TileID;                          // What kind of tile this is, compare it to a TileType
string name = tile.TileTypeDefinition.Name;    // The tile's name from the Tileset, e.g. "Water"
tile.SetIsInteractable(false);                 // Stops the tile from being clicked or hovered
```

Tiles report clicks & hovering through two events, so any script can listen without touching the tiles:

```csharp
BaseTile.OnTileClicked += tile => Debug.Log($"Clicked {tile.TileTypeDefinition.Name}");
BaseTile.OnTileHover += tile => Debug.Log($"Hovering {tile.name}");
```

`Square2DTile` also overrides four hooks, where you can add your own behaviour (the sample logs each one):

| Hook | Runs when |
|---|---|
| `OnInitialized()` | The tile has been placed on the map |
| `OnClickDown()` | The mouse button is pressed on the tile |
| `OnClickUp()` | The mouse button is released on the tile |
| `OnHovered()` | Every frame the mouse is over the tile |

> [!NOTE]
> Clicking & hovering use the legacy Input Manager. They are not called when the project only uses the new Input System.

## Map Generator: `BaseMapGenerator`

The map generator decides which tile goes at each position. The sample includes two:

| Generator | Shape | Map size |
|---|---|---|
| `Grid2DTileMapGenerator` | Rectangle | `Vector2Int` width & height |
| `Radial2DTileMapGenerator` | Circle | `int` radius |

Building a map happens in steps, called **passes**:

1. **Pre-pass** _(optional)_: place tiles before anything else. These tiles are kept.
2. **Default pass**: fills every empty position with a random tile, using each tile's `Spawn Weight`.
3. **Passes** _(optional)_: change the map after it is filled, e.g. add a border or a river.
4. The tile prefabs are created in the scene.

Each pass receives a `MapGenContext`, which holds the `Tiles` placed so far, the `Tileset`, the `Layout`, the `Seed` & a seeded `Random`. Use that `Random` for anything random, so the same seed keeps building the same map.

```csharp
var water = Array.Find(mapTileset.tiles, tile => tile.Id == TILE_TYPE.WATER);

// Pre-pass: always place water in the middle
Square2DTileMap.CreateGridMap(mapTileset, new Vector2Int(10, 10), transform, seed: 42,
    prepass: context => context.Tiles[new Vector2Int(5, 5)] = water);
```

The generator is reached through `map.MapGenerator`. Its seed functions pick which map comes next:

```csharp
map.MapGenerator.NextMap();       // Moves to the next seed (seed + 1)
map.MapGenerator.RandomMap();     // Moves to a random seed
map.MapGenerator.DailyMap();      // Uses today's date as the seed, the same map for everyone today
map.MapGenerator.LoadMap(1234);   // Uses a specific seed
map.Generate();                   // Builds the map from the chosen seed
```

To show the seed on screen, listen for `OnNewMapSeed`. Each generator class has its own event:

```csharp
Grid2DTileMapGenerator.OnNewMapSeed += seed => seedLabel.text = $"Seed: {seed}";
```

## Map: `BaseMap`

The map holds every placed tile & is the main thing your game scripts talk to. `Square2DTileMap` is the square version.

Create a map in one line:

```csharp
var gridMap = Square2DTileMap.CreateGridMap(mapTileset, new Vector2Int(10, 10), transform, seed: 42);
var circleMap = Square2DTileMap.CreateRadialMap(mapTileset, 6, transform, seed: 42);
```

Rebuild it:

```csharp
map.RegenerateMap();        // Rebuilds the same map again
map.RegenerateMap(1234);    // Rebuilds using a different seed
map.NextMap();              // Moves to the next seed & rebuilds
map.ClearMapTiles();        // Removes every tile from the scene
```

Find tiles:

```csharp
var tile = map.MapTiles[new Vector2Int(3, 5)];                      // The tile at a position
var firstWater = map.GetFirstTileWhere<Square2DTile>(TILE_TYPE.WATER); // The first water tile found
var allSand = map.GetAllTiles<Square2DTile>(TILE_TYPE.SAND);          // Every sand tile
var nearby = map.GetSurroundingTiles(tile, 2);                        // Every tile within 2 tiles of this one
var lake = map.FindAllSimilarConnected(tile);                         // This tile & every matching tile touching it
var details = map.GetTileDefinition(TILE_TYPE.ROCK);                  // The Tileset entry for rock
int seed = map.Seed;                                                  // The seed of the current map
```

> [!TIP]
> `FindAllSimilarConnected()` is handy for flood-fill style mechanics: counting the size of a lake, capturing a region, or matching groups of tiles.

## TileType

A `TileType` names one kind of tile, such as `GRASS` or `WATER`. Use it whenever a script needs to ask "what kind of tile is this?".

You never write these by hand. Each Tileset generates a list of them in a `Generated` folder next to the asset. The sample `Tileset.asset` generates `TILE_TYPE`:

```csharp
public static class TILE_TYPE
{
    public static readonly global::TileType GRASS = new global::TileType(450423069);
    public static readonly global::TileType ROCK = new global::TileType(2003664820);
    public static readonly global::TileType SAND = new global::TileType(-77154717);
    public static readonly global::TileType WATER = new global::TileType(-735671632);
}
```

Use them to compare tiles:

```csharp
if (tile.TileID == TILE_TYPE.WATER)
    Debug.Log("Splash!");
```

How the list stays up to date:

- Adding, renaming or removing a tile in the Tileset updates the list automatically.
- To update it by hand, use **_Tools → Map Tiles → Regenerate Tile Types_**.
- Builds stop with an error if the list is out of date, and the error names the menu above.
- Tile names are turned into code names: `Deep Water` becomes `DEEP_WATER`, `9 Lives` becomes `_9_LIVES`.

> [!IMPORTANT]
> A tile's Id comes from its name. Renaming a tile changes its Id, so scripts using the old name (e.g. `TILE_TYPE.WATER` after renaming to `Ocean`) stop compiling until they use the new name.

> [!WARNING]
> Keep each Tileset inside the `Assets` folder & outside any `Editor` folder, otherwise its list cannot be generated.

## Adding Your Own Tiles

A new kind of tile, such as lava or snow, only needs a prefab & a Tileset entry. No code is required.

1. Make a prefab for the tile, with a `Square2DTile` component & a `Collider`.
2. Add a new entry to the Tileset's `Tiles` list, give it a `Name` & `Spawn Weight`, and drag the prefab into `Tile Variants`.
3. Use the new [TileType](#tiletype) from your scripts, e.g. `TILE_TYPE.LAVA`.

## Adding a New Map Shape - Hexagons

A new tile **shape**, such as hexagons, needs four small scripts, one for each part from [How It Fits Together](#how-it-fits-together). The example below builds a hexagon map.

Hexagon positions use [cube coordinates](https://www.redblobgames.com/grids/hexagons/#coordinates-cube): three numbers `q`, `r` & `s` that always add up to `0`. They are stored in a `Vector3Int`, where `x` is `q`, `y` is `r` & `z` is `s`.

### 1. The Tile

Put this on every hexagon prefab, alongside a `Collider`.

```csharp
using Samples.MapTiles.Tiles;
using UnityEngine;

public class HexTile : BaseTile<Vector3Int>
{
    public override Vector3Int Position { get; set; }

    protected override void OnClickUp()
    {
        Debug.Log($"Clicked hex {Position}", gameObject);
    }
}
```

### 2. The Layout

Converts hex positions to world positions & lists the six neighbours. This lays hexagons out pointy side up, with `Tile Size` as the flat-to-flat width.

```csharp
using System.Collections.Generic;
using Samples.MapTiles.Tiles;
using UnityEngine;

public class HexLayout : ITileLayout<Vector3Int>
{
    private static readonly Vector3Int[] Directions =
    {
        new(1, -1, 0), new(1, 0, -1), new(0, 1, -1),
        new(-1, 1, 0), new(-1, 0, 1), new(0, -1, 1)
    };

    private static readonly float RowSpacing = Mathf.Sqrt(3f) / 2f;

    public float TileSize { get; }

    public HexLayout(float tileSize)
    {
        TileSize = tileSize;
    }

    public Vector3 ToWorld(Vector3Int hex)
    {
        // Only q (x) & r (y) are needed, since s is always -q - r
        return new Vector3(TileSize * (hex.x + hex.y / 2f), 0f, TileSize * RowSpacing * hex.y);
    }

    public Vector3Int FromWorld(Vector3 world)
    {
        var r = world.z / (TileSize * RowSpacing);
        var q = world.x / TileSize - r / 2f;
        var s = -q - r;

        var roundQ = Mathf.RoundToInt(q);
        var roundR = Mathf.RoundToInt(r);
        var roundS = Mathf.RoundToInt(s);

        // Rounding can break q + r + s = 0, so rebuild the axis that moved the most from the other two
        var diffQ = Mathf.Abs(roundQ - q);
        var diffR = Mathf.Abs(roundR - r);
        var diffS = Mathf.Abs(roundS - s);

        if (diffQ > diffR && diffQ > diffS)
            roundQ = -roundR - roundS;
        else if (diffR > diffS)
            roundR = -roundQ - roundS;
        else
            roundS = -roundQ - roundR;

        return new Vector3Int(roundQ, roundR, roundS);
    }

    public IEnumerable<Vector3Int> Neighbours(Vector3Int hex)
    {
        foreach (var direction in Directions)
            yield return hex + direction;
    }

    public IEnumerable<Vector3Int> CoordinatesInRadius(Vector3Int center, int radius)
    {
        for (var q = -radius; q <= radius; q++)
        {
            var rMin = Mathf.Max(-radius, -q - radius);
            var rMax = Mathf.Min(radius, -q + radius);

            for (var r = rMin; r <= rMax; r++)
                yield return center + new Vector3Int(q, r, -q - r);
        }
    }
}
```

### 3. The Map Generator

Fills a large hexagon of the given radius. It reuses the layout's `CoordinatesInRadius()`, so the generator never needs to know hex maths.

```csharp
using System;
using Samples.MapTiles.Generators;
using Samples.MapTiles.Tiles;
using UnityEngine;
using Utilities;
using Object = UnityEngine.Object;

public class HexMapGenerator : BaseMapGenerator<int, HexTile, Vector3Int>
{
    public HexMapGenerator(int seed, int mapRadius, Transform parent) : base(seed, mapRadius, parent) { }

    protected override void DefaultMapPass(MapGenContext<Vector3Int> context)
    {
        foreach (var hex in context.Layout.CoordinatesInRadius(Vector3Int.zero, MapSize))
        {
            // Skip anything placed by the pre-pass
            if (context.Tiles.ContainsKey(hex))
                continue;

            context.Tiles.Add(hex, GetRandomTileData(OriginalSeedRandom));
        }
    }

    protected override HexTile InstantiateTileObject(Vector3Int hex, TileTypeDefinition definition,
        MapGenContext<Vector3Int> context)
    {
        var prefab = definition.tileVariants.PickRandomElement(OriginalSeedRandom);

        if (prefab is not HexTile hexPrefab)
            throw new ArgumentException($"prefab [{prefab.name}] is not a {nameof(HexTile)}", nameof(prefab));

        var instance = Object.Instantiate(hexPrefab, context.Layout.ToWorld(hex), Quaternion.identity, Parent);
        instance.name = $"[{hex.x},{hex.y},{hex.z}]_{prefab.name}";
        instance.Init(definition, hex);

        return instance;
    }
}
```

### 4. The Map

Ties the Tileset, generator & layout together, with a one-line create function to match `Square2DTileMap`.

```csharp
using Samples.MapTiles.Maps;
using Samples.MapTiles.ScriptableObjects;
using UnityEngine;

public class HexTileMap : BaseMap<HexTile, Vector3Int>
{
    public HexTileMap(TilesetScriptableObject tileset, HexMapGenerator generator, HexLayout layout)
        : base(tileset, generator, layout) { }

    public static HexTileMap CreateMap(TilesetScriptableObject tileset, int mapRadius, Transform parent, int seed = 0)
    {
        var map = new HexTileMap(tileset, new HexMapGenerator(seed, mapRadius, parent), new HexLayout(tileset.tileSize));
        map.Generate();
        return map;
    }
}
```

### 5. Use It

Make a new Tileset whose `Tile Variants` use your `HexTile` prefabs, then build the map:

```csharp
var hexMap = HexTileMap.CreateMap(hexTileset, 5, transform, seed: 42);
var lake = hexMap.FindAllSimilarConnected(hexMap.MapTiles[Vector3Int.zero]);
```

Everything from [Map: `BaseMap`](#map-basemap) works on the hex map too. Neighbour searches now find six neighbours, since they ask `HexLayout`.
