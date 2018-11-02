using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameControl : MonoBehaviour
{
    // Serialized parameters
    [SerializeField] public MapShape mapShape;
    [SerializeField] public int mapRadius;
    [SerializeField] public int mapWidth;
    [SerializeField] public int numRows;
    [SerializeField] public int numColumns;

    public static GameControl controller;
    // Use this for initialization
    void Awake()
    {
        if (controller == null)
        {
            DontDestroyOnLoad(gameObject);
            controller = this;
        }
        else if (controller != this)
        {
            Destroy(gameObject);
        }
    }

    // Use this for initialization
    void Start()
    {
        mapShape = MapShape.HEX;
        // Normal Widths for Square and Hex respectively
        if (mapShape == MapShape.SQUARE)
        {
            mapWidth = 20;
        }
        else
        {
            mapWidth = 25;
        }
        mapRadius = mapWidth / 2;
        numRows = mapWidth;
        numColumns = mapWidth;
    }
}
