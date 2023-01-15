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

    public Sweepotron_AI sweepy;

    // Awake is called before start so that the tilemap is generated before the camera is set up
    void Awake()
    {
        mineMap.Initialize(width, height, numMines);

        InitializeTilemap();
        sweepy.Initialize(width, height);

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

    private bool IsInBounds(Vector3Int pos)
    {

        if (   pos.x < 0 || pos.x >= width
            || pos.y < 0 || pos.y >= height)
        {
            return false;
        }

        return true;
    }

    private Tile GetTile(Vector3Int pos)
    {
        if (!mineMap.IsRevealed(pos))
        {
            if (mineMap.IsFlagged(pos))
            {
                return flagTile;
            }
            return hiddenTile;
        }

        switch (mineMap.GetMineOrNumber(pos))
        {
            case -1:
                return mineTile;

            default:
                return numberTiles[mineMap.GetMineOrNumber(pos)];
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

            if (IsInBounds(coordinate))
            {

                if (!mineMap.IsRevealed(coordinate))
                {
                    mineMap.OpenTile(coordinate);
                    UpdateTiles();
                }
                else if (mineMap.CanChord(coordinate))
                {
                    mineMap.OpenNeighbors(coordinate);
                    UpdateTiles();
                }
            }
            
        }

        // Right Click
        if (Input.GetMouseButtonDown(1))
        {
            mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            coordinate = grid.WorldToCell(mouseWorldPos);

            if (IsInBounds(coordinate))
            {
                mineMap.ToggleFlag(coordinate);
                UpdateTiles();
            }
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            bool madeProgress = true;

            while (madeProgress)
            {
                sweepy.SinglePointAlg();
                sweepy.SetOverlapAlg();
                madeProgress = UpdateTiles();
            }
        }

    }

    private bool UpdateTiles()
    {
        HashSet<Vector3Int> updatedTiles = new HashSet<Vector3Int>(mineMap.GetUpdatedTiles());
        mineMap.ClearUpdatedTiles();

        foreach (Vector3Int pos in updatedTiles)
        {
            tilemap.SetTile(pos, GetTile(pos));
        }

        return updatedTiles.Count > 0;
    }


}
