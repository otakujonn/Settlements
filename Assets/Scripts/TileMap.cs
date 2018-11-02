using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TileMap : MonoBehaviour
{
    // Serialized parameters

    // Cached references
    GameControl gameControl;

    //
    // Set up Dictionaries and Lists for tilees
    public Dictionary<string, Tile> tiles;
    public List<string> tileKeys;
    public Dictionary<Tile, GameObject> tileToGameObjectMap;
    public Dictionary<GameObject, Tile> gameObjectToTileMap;

    // Use this for initialization
    void Start()
    {
        gameControl = GameControl.controller;

        GenerateMap(gameControl.mapShape);
    }

    // private bool animationIsPlaying = false;

    // Update is called once per frame
    void Update()
    {

    }

    virtual public void GenerateMap(MapShape mapShape = MapShape.HEX)
    {
        //Generate flat map, currently defaulting to sea level

        int mapRadius = gameControl.mapRadius;
        int mapWidth = gameControl.mapWidth;

        tiles = new Dictionary<string, Tile>();
        tileKeys = new List<string>();
        tileToGameObjectMap = new Dictionary<Tile, GameObject>();
        gameObjectToTileMap = new Dictionary<GameObject, Tile>();
        if (mapShape == MapShape.SQUARE)
        {
            for (int row = 0; row < mapWidth; row++)
            {
                for (int column = 0; column < mapWidth; column++)
                {
                    int columnCoord = column - (row - (row % 1)) / 2;
                    int rowCoord = row;

                    GameObject hexGO = CreateNewTile(columnCoord, rowCoord);

                    hexGO.GetComponentInChildren<TextMesh>().text =
                        string.Format("{0},{1}", column, row);
                }
            }
        }
        else
        {
            for (int row = 0; row < mapWidth; row++)
            {
                int howManyCols = mapWidth - Mathf.Abs(row - mapRadius);

                for (int column = 0; column < howManyCols; column++)
                {
                    int rowCoord = row < mapRadius ? column - row : column - mapRadius;
                    int columnCoord = row - mapRadius;

                    GameObject hexGO = CreateNewTile(columnCoord, rowCoord);

                    hexGO.GetComponentInChildren<TextMesh>().text =
                        string.Format("{0},{1}", columnCoord, rowCoord);
                }
            }
        }
        app.view.hexMap.UpdateTileVisuals();

        // Moving camera to Map Center if Square
        if (mapShape == MapShape.SQUARE)
        {
            Tile centerTile = app.model.hexMap.GetTileAt(mapRadius / 2, mapRadius);
            Vector3 position = Camera.main.transform.position;
            position.x = centerTile.Position().x;
            position.z = centerTile.Position().z - 10;

            Camera.main.transform.position = position;
        }
    }

    private GameObject CreateNewTile(int columnCoord, int rowCoord)
    {
        //Instantiate a Hex
        Tile hex = new Tile(columnCoord, rowCoord);
        hex.TerrainType = Biomes.FIELD; //TODO: replace
        hex.Elevation = app.model.hexMap.StartingElevation;
        hex.IsPlayable = true;

        app.model.hexMap.tiles.Add(columnCoord + ", " + rowCoord, hex);
        app.model.hexMap.tileKeys.Add(columnCoord + ", " + rowCoord);

        Vector3 pos = hex.Position();

        GameObject hexGO = app.view.hexMap.GenerateTile(pos);

        app.model.hexMap.tileToGameObjectMap.Add(hex, hexGO);
        app.model.hexMap.gameObjectToTileMap.Add(hexGO, hex);

        hexGO.name = "Hex: " + columnCoord + "," + rowCoord;
        return hexGO;
    }

    public void ElevateArea(int q, int r, int radius, float centerHeight = 1f)
    {

        Tile centerHex = app.model.hexMap.GetTileAt(q, r);

        Tile[] areaHexes = GetHexesWithinRadiusOf(centerHex, radius);

        foreach (Tile tile in areaHexes)
        {
            if (radius == 0)
                radius = 1;

            tile.Elevation = centerHeight * Mathf.Lerp(1f, 0.25f, Mathf.Pow(HexMathHelper.Distance(centerHex, tile) / radius, 2f));
            if (tile.Elevation >= app.model.hexMap.HeightMountain)
            {
                tile.TerrainType = Biomes.MOUNTAIN;
                tile.IsPlayable = false;
            }
        }
    }

    public void SinkArea(int q, int r, int radius, float centerHeight = -1f)
    {

        Tile centerHex = app.model.hexMap.GetTileAt(q, r);

        Tile[] areaHexes = GetHexesWithinRadiusOf(centerHex, radius);

        foreach (Tile tile in areaHexes)
        {
            if (radius == 0)
                radius = 1;

            tile.Elevation = centerHeight * Mathf.Lerp(1f, 0.25f, Mathf.Pow(HexMathHelper.Distance(centerHex, tile) / radius, 2f));
            if (tile.Elevation < app.model.hexMap.HeightFlat)
            {
                tile.TerrainType = Biomes.SEA;
                tile.IsPlayable = false;
            }
        }
    }

    public Tile GetRandomHexAtDistance(Tile startingHex, int distance)
    {
        if (distance <= 0)
            return null;

        List<Tile> hexRing = new List<Tile>();

        Tile currentHex = HexMathHelper.HexAdd(startingHex, HexMathHelper.HexMultiply(HexMathHelper.HEX_DIRECTIONS[4], distance));

        for (int i = 0; i < 6; i++)
        {
            for (int j = 0; j < distance; j++)
            {
                if (app.model.hexMap.InMap(currentHex.X, currentHex.Y))
                    hexRing.Add(currentHex);
                currentHex = HexMathHelper.HexNeighbor(currentHex, i);
            }
        }

        if (hexRing.Count == 0)
            return null;
        //Debug.Log("hexRing.Count : " + hexRing.Count);

        Tile[] ringArray = hexRing.ToArray();

        int index = Random.Range(0, (hexRing.Count));

        return ringArray[index];
    }

    public Tile[] GetLineOfHexes(Tile startingHex, Tile endingHex)
    {
        List<Tile> results = new List<Tile>();

        int N = HexMathHelper.HexDistance(startingHex, endingHex);
        float step = 1.0f / Mathf.Max(N, 1);
        for (int i = 0; i <= N; i++)
        {
            Vector3 hexHolder = HexMathHelper.HexRound(HexMathHelper.HexLerp(startingHex, endingHex, step * i));
            if (app.model.hexMap.InMap((int)hexHolder.x, (int)hexHolder.y))
                results.Add(app.model.hexMap.GetTileAt((int)hexHolder.x, (int)hexHolder.y));
        }

        return results.ToArray();
    }

    public Tile[] GetHexesWithinRadiusOf(Tile centerHex, int radius)
    {
        List<Tile> results = new List<Tile>();

        for (int dx = -radius; dx <= radius; dx++)
        {
            for (int dy = Mathf.Max(-radius, -dx - radius); dy <= Mathf.Min(radius, -dx + radius); dy++)
            {
                int xCoord = centerHex.X + dx;
                int yCoord = centerHex.Y + dy;

                if (app.model.hexMap.InMap(xCoord, yCoord))
                    results.Add(app.model.hexMap.GetTileAt(xCoord, yCoord));
            }
        }
        return results.ToArray();
    }

    public bool InNeighborhood(List<Tile> hexList, Tile hex, int radius)
    {
        Tile[] neighborhood = GetHexesWithinRadiusOf(hex, radius);

        foreach (Tile neighbor in hexList)
        {
            if (neighborhood.Contains(neighbor))
                return true;
        }

        return false;
    }

    // We should be able to make this void and just make Location assignments here directly
    public Tile[] GenerateLocationList(int minDistribution, int howManySamples, int howManyTries, List<ILocation> locationTypes)
    {
        List<Tile> processList = new List<Tile>();
        List<Tile> locationList = new List<Tile>();
        int listTracker;
        int currentType = 0;

        Tile firstPoint;
        if (app.model.hexMap.Shape == MapShape.SQUARE)
        {
            firstPoint = app.model.hexMap.GetTileAt(app.model.hexMap.MapRadius / 2, app.model.hexMap.MapRadius);
        }
        else
        {
            firstPoint = app.model.hexMap.GetTileAt(0, 0);
        }
        Tile point;
        Tile newHex;

        // howManySamples++;

        processList.Add(firstPoint);
        listTracker = locationList.Count;

        while (processList.Count != 0)
        {
            point = processList[0];
            processList.RemoveAt(0);
            int numberOfTries;
            for (numberOfTries = 0; numberOfTries < howManyTries; numberOfTries++)
            {
                newHex = GenerateRandomLocationHex(point, minDistribution);

                // Debug.Log("Current Type:" + locationTypes[currentType].LocationName);

                if ((app.model.hexMap.InMap(newHex) && !InNeighborhood(locationList, newHex, minDistribution)) && listTracker < howManySamples)
                {
                    // Check for playable tiles around the location
                    Tile[] neighbors = GetHexesWithinRadiusOf(newHex, 1);
                    int playableNeighbors = 0;
                    foreach (var neighbor in neighbors)
                    {
                        if (neighbor.IsPlayable)
                        {
                            playableNeighbors++;
                        }
                    }
                    if (playableNeighbors >= 3)
                    {
                        //Debug.Log("Type: " + currentType + "Attempt " + numberOfTries + ": Enough playableNeighbors.");

                        // Need to also ensure that various locations have correct placement according to their rulesets
                        if (locationTypes[currentType].IsValidTileLocation(newHex, neighbors))
                        {
                            processList.Add(newHex);
                            Tile tile = app.model.hexMap.GetTileAt(newHex.X, newHex.Y);
                            tile.TerrainType = Biomes.LOCATION;
                            tile.IsPlayable = false;
                            tile.Feature = locationTypes[currentType].GetLocationType;

                            locationList.Add(tile);

                            listTracker = locationList.Count;
                            currentType++;
                        }
                        else
                        {
                            //Debug.Log("Attempt " + numberOfTries + ": Did not fit Tile Rules.");
                        }
                    }
                    else
                    {
                        //Debug.Log("Attempt " + numberOfTries + ": Not enough playableNeighbors.");
                    }
                }
            }
        }
        //if (listTracker < howManySamples)
        //{
        //    locationList.Clear();
        //    // TODO: replace current locationType with a new type
        //    // locationTypes.FindIndex();
        //    locationList = GenerateLocationList(minDistribution, howManySamples, howManyTries, locationTypes).ToList<Tile>();
        //}
        return locationList.ToArray();
    }

    public Tile GenerateRandomLocationHex(Tile hex, int minDistance)
    { //non-uniform, favours points closer to the inner ring, leads to denser packings
        float r1 = Random.Range(0f, 1f); //random point between 0 and 1
        float r2 = Random.Range(0f, 1f);
        //random radius between mindist and 2 * mindist
        float radius = (minDistance) * (r1 + 1);
        //random angle
        float angle = 2 * Mathf.PI * r2;
        //the new point is generated around the point (x, y)
        int newX = (int)(hex.X + radius * Mathf.Cos(angle));
        int newY = (int)(hex.Y + radius * Mathf.Sin(angle));

        Tile result = new Tile(newX, newY);

        return result;
    }

    public void HighLightPlayableTiles(Biomes biome)
    {
        int highlightedTiles = 0;

        // Does player have any settlements?
        //   Yes:
        Debug.Log("Settlements : " + app.controller.turnCon.playerSettlements.Count());
        if (app.controller.turnCon.playerSettlements.Count() != 0)
        {
            // Do any of the adjacent tiles have TerrainType "biome"?
            //   Yes: Highlight adjacent tiles
            foreach (Tile tile in app.controller.turnCon.playerSettlements)
            {
                Tile[] neighborhood = GetHexesWithinRadiusOf(tile, 1);
                foreach (Tile neighbor in neighborhood)
                {
                    if (neighbor.TerrainType == biome && neighbor.OwnedBy == null)
                    {
                        app.view.hexMap.HighlightTile(neighbor);
                        neighbor.IsHighlighted = true;
                        highlightedTiles++;
                    }
                }
            }
        }
        if (highlightedTiles == 0)
        {
            // No to one or the other above?
            //   Highlight all tiles of this biome.
            foreach (KeyValuePair<string, Tile> pair in app.model.hexMap.tiles)
            {
                if (pair.Value.TerrainType == biome && pair.Value.OwnedBy == null)
                {
                    app.view.hexMap.HighlightTile(pair.Value);
                    pair.Value.IsHighlighted = true;
                }
            }
        }
    }

    public void ClearHighlightedTiles()
    {
        foreach (KeyValuePair<string, Tile> pair in app.model.hexMap.tiles)
        {
            if (pair.Value.IsHighlighted)
            {
                app.view.hexMap.RemoveHighlight(pair.Value);
            }
        }
    }
}
