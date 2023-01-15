using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEditor;

public class MineMap : MonoBehaviour
{

    int width, height;
    int numMines;
    int[,] mineSquares;
    bool[,] flaggedSquares;
    bool[,] revealedSquares;
    bool firstClick;
    int hiddenSquares;
    bool dead;

    Queue<Vector3Int> updatedTiles = new Queue<Vector3Int>();

    public void Initialize(int widthIn, int heightIn, int numMinesIn)
    {

        width    = widthIn;
        height   = heightIn;
        numMines = numMinesIn;

        mineSquares = new int[width, height];
        revealedSquares = new bool[width, height];
        flaggedSquares = new bool[width, height];

        hiddenSquares = width * height;

        firstClick = true;
        dead = false;

    }

    private void GenerateMines(int xClick, int yClick)
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
            if (mineSquares[xPos, yPos] != -1 &&
                !(System.Math.Abs(xPos - xClick) <= 1 && System.Math.Abs(yPos - yClick) <= 1))
            {
                mineSquares[xPos, yPos] = -1;
                minesGenerated++;
            }
        }

        // Fill in the counts for all the non-mine squares
        for (int ii = 0; ii < width; ii++)
        {
            for (int jj = 0; jj < height; jj++)
            {
                if (mineSquares[ii, jj] != -1)
                {
                    mineSquares[ii, jj] = CountNeighboringMines(ii, jj);
                }
            }
        }

    }

    private int CountNeighboringMines(int xPos, int yPos)
    {
        int sum = 0;

        bool lookLeft = xPos > 0;
        bool lookRight = xPos < width - 1;
        bool lookDown = yPos > 0;
        bool lookUp = yPos < height - 1;

        // Top Left
        if (lookLeft && lookUp && mineSquares[xPos - 1, yPos + 1] == -1) sum += 1;

        // Top
        if (lookUp && mineSquares[xPos, yPos + 1] == -1) sum += 1;

        // Top Right
        if (lookRight && lookUp && mineSquares[xPos + 1, yPos + 1] == -1) sum += 1;

        // Left
        if (lookLeft && mineSquares[xPos - 1, yPos] == -1) sum += 1;

        // Right
        if (lookRight && mineSquares[xPos + 1, yPos] == -1) sum += 1;

        // Bottom Left
        if (lookLeft && lookDown && mineSquares[xPos - 1, yPos - 1] == -1) sum += 1;

        // Bottom
        if (lookDown && mineSquares[xPos, yPos - 1] == -1) sum += 1;

        // Bottom Right
        if (lookRight && lookDown && mineSquares[xPos + 1, yPos - 1] == -1) sum += 1;

        return sum;
    }

    public void OpenTile(int xPos, int yPos)
    {
        if (firstClick)
        {
            GenerateMines(xPos, yPos);
            firstClick = false;
        }

        if (!revealedSquares[xPos, yPos])
        {
            Vector3Int queuePos = new Vector3Int(xPos, yPos, 0);
            updatedTiles.Enqueue(queuePos);
            revealedSquares[xPos, yPos] = true;
            hiddenSquares--;
            if (mineSquares[xPos, yPos] == 0)
            {
                OpenNeighbors(xPos, yPos);
            }
            if (mineSquares[xPos, yPos] == -1)
            {
                Debug.Log("KA-BOOM");
                dead = true;
            }
        }
    }

    // Open neighboring cells. Used for expanding clearings and for chords,
    // so it checks if there is a flag before opening.
    public void OpenNeighbors(int xPos, int yPos)
    {
        bool lookLeft = xPos > 0;
        bool lookRight = xPos < width - 1;
        bool lookDown = yPos > 0;
        bool lookUp = yPos < height - 1;

        // Top Left
        if (lookLeft && lookUp && !flaggedSquares[xPos, yPos]) OpenTile(xPos - 1, yPos + 1);

        // Top
        if (lookUp && !flaggedSquares[xPos, yPos]) OpenTile(xPos, yPos + 1);

        // Top Right
        if (lookRight && lookUp && !flaggedSquares[xPos, yPos]) OpenTile(xPos + 1, yPos + 1);

        // Left
        if (lookLeft && !flaggedSquares[xPos, yPos]) OpenTile(xPos - 1, yPos);

        // Right
        if (lookRight && !flaggedSquares[xPos, yPos]) OpenTile(xPos + 1, yPos);

        // Bottom Left
        if (lookLeft && lookDown && !flaggedSquares[xPos, yPos]) OpenTile(xPos - 1, yPos - 1);

        // Bottom
        if (lookDown && !flaggedSquares[xPos, yPos]) OpenTile(xPos, yPos - 1);

        // Bottom Right
        if (lookRight && lookDown && !flaggedSquares[xPos, yPos]) OpenTile(xPos + 1, yPos - 1);
    }

    public void ToggleFlag(int xPos, int yPos)
    {
        if (xPos >= 0 && xPos < width &&
            yPos >= 0 && yPos < height &&
            !revealedSquares[xPos, yPos])
        {
            flaggedSquares[xPos, yPos] = !flaggedSquares[xPos, yPos];
            Vector3Int queuePos = new Vector3Int(xPos, yPos, 0);
            updatedTiles.Enqueue(queuePos);
        }
    }

    // Can Chord if it is revealed (checked for before this is called)
    // and it is a number greater than 0
    // and that number equals the number of neighboring flags.
    public bool CanChord(int xPos, int yPos)
    {
        if (mineSquares[xPos, yPos] == 0) return false;

        int sum = 0;

        bool lookLeft = xPos > 0;
        bool lookRight = xPos < width - 1;
        bool lookDown = yPos > 0;
        bool lookUp = yPos < height - 1;

        // Top Left
        if (lookLeft && lookUp && flaggedSquares[xPos - 1, yPos + 1]) sum += 1;

        // Top
        if (lookUp && flaggedSquares[xPos, yPos + 1]) sum += 1;

        // Top Right
        if (lookRight && lookUp && flaggedSquares[xPos + 1, yPos + 1]) sum += 1;

        // Left
        if (lookLeft && flaggedSquares[xPos - 1, yPos]) sum += 1;

        // Right
        if (lookRight && flaggedSquares[xPos + 1, yPos]) sum += 1;

        // Bottom Left
        if (lookLeft && lookDown && flaggedSquares[xPos - 1, yPos - 1]) sum += 1;

        // Bottom
        if (lookDown && flaggedSquares[xPos, yPos - 1]) sum += 1;

        // Bottom Right
        if (lookRight && lookDown && flaggedSquares[xPos + 1, yPos - 1]) sum += 1;

        return sum == mineSquares[xPos, yPos];
    }

    public int GetMineOrNumber(int xPos, int yPos)
    {
        return mineSquares[xPos, yPos];
    }

    public bool IsFlagged(int xPos, int yPos)
    {
        return flaggedSquares[xPos, yPos];
    }

    public bool IsRevealed(int xPos, int yPos)
    {
        return revealedSquares[xPos, yPos];
    }

    public Queue<Vector3Int> GetUpdatedTiles()
    {
        return updatedTiles;
    }

    public void ClearUpdatedTiles()
    {
        updatedTiles.Clear();
    }
}
