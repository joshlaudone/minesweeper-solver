using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEditor;

public class GameMap : MonoBehaviour
{

    [Range(1, 100)]
    public int width;

    [Range(1, 100)]
    public int height;

    public int numMines;

    public Tilemap tilemap;
    public Tile hiddenTile, flagTile, mineTile;
    public Tile[] numberTiles = new Tile[9];
    public Grid grid;

    public MineMap mineMap;

    // Awake is called before start so that the tilemap is generated before the camera is set up
    void Awake()
    {
        mineMap.Initialize(width, height, numMines);

        InitializeTilemap();
        
    }

    private void InitializeTilemap()
    {
        Vector3Int tilePos = new Vector3Int();

        tilemap.ClearAllTiles();

        for (int ii = 0; ii < width; ii++)
        {
            for (int jj = 0; jj < height; jj++)
            {
                tilePos.x = ii;
                tilePos.y = jj;
                tilemap.SetTile(tilePos, hiddenTile);
            }
        }

    }

    private bool IsInBounds(int xPos, int yPos)
    {

        if (   xPos < 0 || xPos >= width
            || yPos < 0 || yPos >= height)
        {
            return false;
        }

        return true;
    }

    private Tile GetTile(int xPos, int yPos)
    {
        if (!mineMap.IsRevealed(xPos, yPos))
        {
            if (mineMap.IsFlagged(xPos, yPos))
            {
                return flagTile;
            }
            return hiddenTile;
        }

        switch (mineMap.GetMineOrNumber(xPos, yPos))
        {
            case -1:
                return mineTile;

            default:
                return numberTiles[mineMap.GetMineOrNumber(xPos, yPos)];
        }
    }

    // Update is called once per frame
    void Update()
    {

        Vector3 mouseWorldPos;
        Vector3Int coordinate;

        // Left Click
        if (Input.GetMouseButtonDown(0))
        {
            mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            coordinate = grid.WorldToCell(mouseWorldPos);

            if (IsInBounds(coordinate.x, coordinate.y))
            {

                if (!mineMap.IsRevealed(coordinate.x, coordinate.y))
                {
                    mineMap.OpenTile(coordinate.x, coordinate.y);
                    UpdateTiles();
                }
                else if (mineMap.CanChord(coordinate.x, coordinate.y))
                {
                    mineMap.OpenNeighbors(coordinate.x, coordinate.y);
                    UpdateTiles();
                }
            }
            
        }

        // Right Click
        if (Input.GetMouseButtonDown(1))
        {
            mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            coordinate = grid.WorldToCell(mouseWorldPos);

            if (IsInBounds(coordinate.x, coordinate.y))
            {
                mineMap.ToggleFlag(coordinate.x, coordinate.y);
                UpdateTiles();
            }
        }

    }

    private void UpdateTiles()
    {
        Vector3Int pos;

        Queue<Vector3Int> updatedTiles = new Queue<Vector3Int>(mineMap.GetUpdatedTiles().ToArray());

        while (updatedTiles.Count > 0)
        {
            pos = updatedTiles.Dequeue();
            tilemap.SetTile(new Vector3Int(pos.x, pos.y, 0), GetTile(pos.x, pos.y));
        }
    }


}
