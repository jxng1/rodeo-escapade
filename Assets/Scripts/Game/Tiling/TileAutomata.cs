using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEditor;
public class TileAutomata : MonoBehaviour
{

    [Range(0, 100)]
    public int initialChance;

    [Range(1, 8)]
    public int birthLimit;

    [Range(1, 8)]
    public int deathLimit;

    [Range(1, 10)]
    public int iterations;
    private int count = 0;
    private int[,] terrainMap;
    public Vector3Int tileMapSize;

    public Tilemap topMap;
    public Tilemap bottomMap;

    public Tile topTile;
    public Tile bottomTile;

    int width;
    int height;

    /*
    Simulates a new tilemap generation based on the previous tile map state. 
    */
    public void simulate(int iterations)
    {
        clearMaps(false); // Clears the tile map, but doesn't set the array values to null.
        width = tileMapSize.x;
        height = tileMapSize.y;

        if (terrainMap == null)
        { // If a completely new tile map, generate an initialise some positions of starting life.
            terrainMap = new int[width, height];
            initPos();
        }

        for (int i = 0; i < iterations; i++)
        { // For each iteration allowed, generate new tile positions.
            terrainMap = generateTilePos(terrainMap);
        }

        for (int x = 0; x < width; x++)
        { // Adds tiles to the tile map for display.
            for (int y = 0; y < height; y++)
            {
                if (terrainMap[x, y] == 1)
                {
                    topMap.SetTile(new Vector3Int(-x + width / 2, -y + height / 2, 0), topTile);
                }
                bottomMap.SetTile(new Vector3Int(-x + width / 2, -y + height / 2, 0), bottomTile);
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        // if (Input.GetMouseButtonDown(0)) {
        //     simulate(iterations);
        // }

        // if (Input.GetMouseButtonDown(1)) {
        //     clearMaps(true);
        // }

        // if (Input.GetMouseButton(2)) {
        //     saveAssetMap();
        // }
    }

    public void saveAssetMap()
    {
        string saveName = "tileMap_" + count++;
        var mf = GameObject.Find("Grid");

        if (mf)
        {
            var savePath = "Assets/Prefabs/Saved Generated Tilemaps/" + saveName + ".prefab";
            if (PrefabUtility.SaveAsPrefabAsset(mf, savePath))
            {
                EditorUtility.DisplayDialog("Tilemap saved", "Tilemap saved under the name:" + savePath, "Continue");
            }
            else
            {
                EditorUtility.DisplayDialog("Tilemap not saved", "Error occured.", "Continue");
            }
        }
    }

    public int[,] generateTilePos(int[,] oldMap)
    {
        int[,] newMap = new int[width, height];
        int neighbour;
        BoundsInt bounds = new BoundsInt(-1, -1, 0, 3, 3, 1); // Generates a bounding box for tile position generation of new life/curbing.

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                neighbour = 0;
                foreach (var b in bounds.allPositionsWithin)
                { // Calculate the amount of neighbours within said bounding box.
                    if (b.x == 0 && b.y == 0) continue; // If looking at the bounding center, carry on.
                    if (x + b.x >= 0 && x + b.x < width && y + b.y >= 0 && y + b.y < height)
                    { // If not traversing outside of allowed grid, then add the values.
                        neighbour += oldMap[x + b.x, y + b.y];
                    }
                    else
                    {
                        neighbour++;
                    }
                }

                if (oldMap[x, y] == 1)
                { // Calculate whether or not tile should die.
                    if (neighbour < deathLimit)
                    {
                        newMap[x, y] = 0;
                    }
                    else
                    {
                        newMap[x, y] = 1;
                    }
                }
                else if (oldMap[x, y] == 0)
                {
                    if (neighbour > birthLimit)
                    {
                        newMap[x, y] = 1;
                    }
                    else
                    {
                        newMap[x, y] = 0;
                    }
                }
            }
        }

        return newMap;
    }

    public void initPos()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                terrainMap[x, y] = Random.Range(1, 101) < initialChance ? 1 : 0;
            }
        }
    }

    public void clearMaps(bool complete)
    {
        topMap.ClearAllTiles();
        bottomMap.ClearAllTiles();

        if (complete)
        {
            terrainMap = null;
        }
    }
};