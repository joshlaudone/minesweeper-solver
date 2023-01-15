using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEditor;
using System.IO;

public class GameMapTest : MonoBehaviour
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

    public string boardFolder;

    public TestMineGenerator mineGen;

    private int boardIdx;
    private string[] inputFiles;
    private Vector3Int startSq;

    // Awake is called before start so that the tilemap is generated before the camera is set up
    void Awake()
    {
        inputFiles = Directory.GetFiles(boardFolder, "*.in");
        boardIdx = 0;

        LoadBoard();

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
            RunFullAlg();
        }

        if (Input.GetKeyDown(KeyCode.Period))
        {
            RunPartialAlg();
        }

        if (Input.GetKeyDown(KeyCode.P))
        {
            TakeScreenshot();
        }

        if (Input.GetKeyDown(KeyCode.N))
        {
            boardIdx += 1;
            LoadBoard();
        }

        if (Input.GetKeyDown(KeyCode.C))
        {
            sweepy.ConstraintMapAlg();
            UpdateTiles();
        }

        if (Input.GetKeyDown(KeyCode.B))
        {
            RunBasicAlg();
        }

        if (Input.GetKeyDown(KeyCode.R))
        {
            sweepy.RecursiveBacktrackingAlg();
            UpdateTiles();
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

    private void LoadBoard()
    {
        if (boardIdx >= inputFiles.Length)
        {
            return;
        }

        int[,] mineBoard = new int[0, 0];
        string[] numbers;

        string[] lines = System.IO.File.ReadAllLines(inputFiles[boardIdx]);
        char[] delimiterChars = { ' ', '[', ']', ',', '(', ')' };

        for (int ii = 0; ii < lines.Length; ii++)
        {
            string[] currentLine = lines[ii].Split(": ");
            switch (currentLine[0])
            {
                case "Board Width":
                    width = int.Parse(currentLine[1]);
                    break;

                case "Board Height":
                    height = int.Parse(currentLine[1]);
                    break;

                case "Number of Mines":
                    numMines = int.Parse(currentLine[1]);
                    break;

                case "Starting Square":
                    numbers = currentLine[1].Split(delimiterChars, System.StringSplitOptions.RemoveEmptyEntries);
                    startSq.x = int.Parse(numbers[0]);
                    startSq.y = int.Parse(numbers[1]);
                    break;

                case "Mines":
                    mineBoard = new int[width, height];
                    numbers = currentLine[1].Split(delimiterChars, System.StringSplitOptions.RemoveEmptyEntries);
                    int x;
                    int y;
                    for (int jj = 1; jj < numbers.Length; jj += 2)
                    {
                        x = int.Parse(numbers[jj - 1]);
                        y = int.Parse(numbers[jj]);
                        mineBoard[x, y] = -1;
                    }
                    break;

                default:
                    break;
            }
        }

        mineMap.Initialize(mineBoard, numMines, startSq);

        InitializeTilemap();
        UpdateTiles();
        sweepy.Initialize(width, height);
    }

    void RunFullAlg()
    {
        bool madeProgress = true;

        while (madeProgress && mineMap.RemainingMines() != 0)
        {
            sweepy.SinglePointAlg();
            sweepy.SetOverlapAlg();

            madeProgress = UpdateTiles();
            if (madeProgress)
                continue;

            sweepy.ConstraintMapAlg();

            madeProgress = UpdateTiles();
            if (madeProgress)
                continue;

            sweepy.RecursiveBacktrackingAlg();

            madeProgress = UpdateTiles();
        }
    }

    void RunPartialAlg()
    {
        sweepy.SinglePointAlg();

        if (UpdateTiles())
            return;

        sweepy.SetOverlapAlg();

        if (UpdateTiles())
            return;

        sweepy.ConstraintMapAlg();

        if (UpdateTiles())
            return;

        sweepy.RecursiveBacktrackingAlg();
    }

    void RunBasicAlg()
    {
        bool madeProgress = true;

        while (madeProgress)
        {
            sweepy.SinglePointAlg();
            sweepy.SetOverlapAlg();
            madeProgress = UpdateTiles();
        }
    }

    void TakeScreenshot()
    {
        string screenshotLoc = inputFiles[Mathf.Min(boardIdx, inputFiles.Length - 1)];
        screenshotLoc = screenshotLoc.Replace("Boards", "Screenshots");
        screenshotLoc = screenshotLoc.Replace(".in", ".png");
        ScreenCapture.CaptureScreenshot(screenshotLoc);
    }


}
