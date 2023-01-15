using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEditor;

public class GameMap : MonoBehaviour
{

    [Range(1, 100)]
    public int width;

    [Range(1, 100)]
    public int height;

    [Range(1, 10000)]
    public int numMines;

    public Tilemap tilemap;
    public Tile hiddenTile, flagTile, mineTile;
    public Tile[] numberTiles = new Tile[9];
    public Grid grid;

    private int[,] mineMap;
    private bool[,] flagMap;
    private bool[,] revealedMap;
    private bool firstClick;
    private int hiddenSquares;

    // Awake is called before start so that the tilemap is generated before the camera is set up
    void Awake()
    {
        mineMap     = new int[width, height];
        revealedMap = new bool[width, height];
        flagMap     = new bool[width, height];
        
        
        hiddenSquares = width * height;

        firstClick = true;

        generateTilemap();
        
    }

    private void generateMines(int xClick, int yClick)
    {
        int minesGenerated = 0;
        int xPos, yPos;
        
        // Add mines until total = numMines
        // Just purely picking a random pos might not be great with very dense boards
        while (minesGenerated < numMines)
        {
            xPos = Random.Range(0, width); // subtract 1 because it's inclusive
            yPos = Random.Range(0, height);

            // Check that there's not already a mine and that it won't make the first square have a number
            if (mineMap[xPos,yPos] != -1 && 
                !(System.Math.Abs(xPos - xClick) <= 1 && System.Math.Abs(yPos - yClick) <= 1))
            {
                mineMap[xPos, yPos] = -1;
                minesGenerated++;
            }
        }

        // Fill in the counts for all the non-mine squares
        for (int ii = 0; ii < width; ii++)
        {
            for (int jj = 0; jj < height; jj++)
            {
                if (mineMap[ii, jj] != -1)
                {
                    mineMap[ii, jj] = countNeighboringMines(ii, jj);
                }
            }
        }

    }

    private int countNeighboringMines(int xPos, int yPos)
    {
        int sum = 0;

        bool lookLeft  = xPos > 0;
        bool lookRight = xPos < width - 1;
        bool lookDown  = yPos > 0;
        bool lookUp    = yPos < height - 1;

        // Top Left
        if (lookLeft && lookUp && mineMap[xPos - 1, yPos + 1] == -1)    sum += 1;

        // Top
        if (lookUp && mineMap[xPos, yPos + 1] == -1)                    sum += 1;

        // Top Right
        if (lookRight && lookUp && mineMap[xPos + 1, yPos + 1] == -1)   sum += 1;

        // Left
        if (lookLeft && mineMap[xPos - 1, yPos] == -1)                  sum += 1;

        // Right
        if (lookRight && mineMap[xPos + 1, yPos] == -1)                 sum += 1;

        // Bottom Left
        if (lookLeft && lookDown && mineMap[xPos - 1, yPos - 1] == -1)  sum += 1;

        // Bottom
        if (lookDown && mineMap[xPos, yPos - 1] == -1)                  sum += 1;

        // Bottom Right
        if (lookRight && lookDown && mineMap[xPos + 1, yPos - 1] == -1) sum += 1;

        return sum;
    }

    private void generateTilemap()
    {
        Vector3Int tilePos;
        Tile currentTile;

        tilemap.ClearAllTiles();

        for (int ii = 0; ii < width; ii++)
        {
            for (int jj = 0; jj < height; jj++)
            {

                currentTile = GetTile(ii, jj);
                tilePos = new Vector3Int(ii, jj, 0);
                tilemap.SetTile(tilePos, currentTile);
            }
        }

    }

    private Tile GetTile(int xPos, int yPos)
    {
        if (flagMap[xPos, yPos])
        {
            return flagTile;
        }
        
        if (!revealedMap[xPos, yPos])
        {
            return hiddenTile;
        }

        switch (mineMap[xPos, yPos])
        {
            case -1:
                return mineTile;

            default:
                return numberTiles[mineMap[xPos, yPos]];
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
            if (!firstClick)
            {
                if (!revealedMap[coordinate.x, coordinate.y])
                {
                    OpenTile(coordinate.x, coordinate.y);
                } else if (CanChord(coordinate.x, coordinate.y))
                {
                    OpenNeighbors(coordinate.x, coordinate.y);
                }
                

                if (hiddenSquares == numMines)
                {
                    Debug.Log("A winner is you");
                }
            }
            else
            {
                firstClick = false;
                generateMines(coordinate.x, coordinate.y);
                OpenTile(coordinate.x, coordinate.y);
            }
            
        }

        // Right Click
        if (Input.GetMouseButtonDown(1))
        {
            mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            coordinate = grid.WorldToCell(mouseWorldPos);
            ToggleFlag(coordinate.x, coordinate.y);
        }

    }

    private void OpenTile(int xPos, int yPos)
    {
        if (xPos >= 0 && xPos < width &&
            yPos >= 0 && yPos < height && 
            !flagMap[xPos, yPos])
        {
            if (!revealedMap[xPos, yPos])
            {
                revealedMap[xPos, yPos] = true;
                tilemap.SetTile(new Vector3Int(xPos, yPos, 0), GetTile(xPos, yPos));
                hiddenSquares--;
                if (mineMap[xPos, yPos] == 0)
                {
                    OpenNeighbors(xPos, yPos);
                }
                if (mineMap[xPos, yPos] == -1)
                {
                    Debug.Log("KA-BOOM");
                }
            }
        }
    }

    // Open neighboring cells. Used for expanding clearings and for chords,
    // so it checks if there is a flag before opening.
    private void OpenNeighbors(int xPos, int yPos)
    {
        // I think these checks are redundant since OpenTile checks it.
        bool lookLeft = xPos > 0;
        bool lookRight = xPos < width - 1;
        bool lookDown = yPos > 0;
        bool lookUp = yPos < height - 1;

        // Top Left
        if (lookLeft && lookUp && !flagMap[xPos, yPos]) OpenTile(xPos - 1, yPos + 1);

        // Top
        if (lookUp && !flagMap[xPos, yPos]) OpenTile(xPos, yPos + 1);

        // Top Right
        if (lookRight && lookUp && !flagMap[xPos, yPos]) OpenTile(xPos + 1, yPos + 1);

        // Left
        if (lookLeft && !flagMap[xPos, yPos]) OpenTile(xPos - 1, yPos);

        // Right
        if (lookRight && !flagMap[xPos, yPos]) OpenTile(xPos + 1, yPos);

        // Bottom Left
        if (lookLeft && lookDown && !flagMap[xPos, yPos]) OpenTile(xPos - 1, yPos - 1);

        // Bottom
        if (lookDown && !flagMap[xPos, yPos]) OpenTile(xPos, yPos - 1);

        // Bottom Right
        if (lookRight && lookDown && !flagMap[xPos, yPos]) OpenTile(xPos + 1, yPos - 1);
    }

    private void ToggleFlag(int xPos, int yPos)
    {
        if (xPos >= 0 && xPos < width &&
            yPos >= 0 && yPos < height &&
            !revealedMap[xPos, yPos])
        {
            flagMap[xPos, yPos] = !flagMap[xPos, yPos];
            tilemap.SetTile(new Vector3Int(xPos, yPos, 0), GetTile(xPos, yPos));
        }
    }

    // Can Chord if it is revealed (checked for before this is called)
    // and it is a number greater than 0
    // and that number equals the number of neighboring flags.
    private bool CanChord(int xPos, int yPos)
    {
        if (mineMap[xPos, yPos] == 0) return false;

        int sum = 0;

        bool lookLeft = xPos > 0;
        bool lookRight = xPos < width - 1;
        bool lookDown = yPos > 0;
        bool lookUp = yPos < height - 1;

        // Top Left
        if (lookLeft && lookUp && flagMap[xPos - 1, yPos + 1]) sum += 1;

        // Top
        if (lookUp && flagMap[xPos, yPos + 1]) sum += 1;

        // Top Right
        if (lookRight && lookUp && flagMap[xPos + 1, yPos + 1]) sum += 1;

        // Left
        if (lookLeft && flagMap[xPos - 1, yPos]) sum += 1;

        // Right
        if (lookRight && flagMap[xPos + 1, yPos]) sum += 1;

        // Bottom Left
        if (lookLeft && lookDown && flagMap[xPos - 1, yPos - 1]) sum += 1;

        // Bottom
        if (lookDown && flagMap[xPos, yPos - 1]) sum += 1;

        // Bottom Right
        if (lookRight && lookDown && flagMap[xPos + 1, yPos - 1]) sum += 1;

        return sum == mineMap[xPos, yPos];
    }
}
