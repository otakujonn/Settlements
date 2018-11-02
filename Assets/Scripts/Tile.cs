using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tile : MonoBehaviour
{

    private readonly Vector3 tileCoords;
    private float radius = 1f;

    public int X { get { return (int)tileCoords.x; } }
    public int Y { get { return (int)tileCoords.y; } }
    public int Z { get { return (int)tileCoords.z; } }

    public float Elevation { get; set; }
    public float Moisture { get; set; }
    public Biomes TerrainType { get; set; }

    // Data for map generation and maybe in-game effects
    public Locations Feature { get; set; }
    public bool IsPlayable { get; set; }
    public bool IsHighlighted { get; set; }
    public string OwnedBy { get; set; }

    public Tile(int q, int r) : this(q, r, -(q + r)) { }

    public Tile(int q, int r, int s)
    {
        tileCoords = new Vector3(q, r, s);

        Feature = Locations.NONE;
    }

    /// <summary>
    /// Returns a Vector3 World position
    /// </summary>
    /// <returns></returns>
    public Vector3 Position()
    {
        return new Vector3(
            HexHorizontalSpacing() * (this.X + this.Y / 2f),
            0,
            HexVerticalSpacing() * this.Y
        );
    }

    public float TileHeight()
    {
        return radius * 2;
    }

    public float TileWidth()
    {
        return (Mathf.Sqrt(3) / 2) * TileHeight();
    }

    public float HexVerticalSpacing()
    {
        return (TileHeight() * 0.75f) + 0.05f;
    }

    public float HexHorizontalSpacing()
    {
        return TileWidth() + 0.05f;
    }

    public Vector3 getCoordinates()
    {
        return new Vector3(tileCoords.x, tileCoords.y, tileCoords.z);
    }

}
