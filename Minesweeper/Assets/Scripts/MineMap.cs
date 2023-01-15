using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MineMap : MonoBehaviour
{

    int width, height;
    int numMines;
    int hiddenSafeSquares;
    int remainingMines;
    int[,] mineSquares;
    bool[,] flaggedSquares;
    bool[,] revealedSquares;
    bool firstClick;
    bool dead;

    public BasicMineGenerator mineGen;
    public TestMineGenerator testMineGen;
    public TextMeshProUGUI remainingMinesText;

    HashSet<Vector3Int> updatedTiles = new HashSet<Vector3Int>();

    public void Initialize(int widthIn, int heightIn, int numMinesIn)
    {

        width = widthIn;
        height = heightIn;
        numMines = numMinesIn;

        mineSquares = new int[width, height];
        revealedSquares = new bool[width, height];
        flaggedSquares = new bool[width, height];

        hiddenSafeSquares = width * height;

        numMines = Math.Min(numMines, hiddenSafeSquares - 9);
        SetRemainingMines(numMines);

        firstClick = true;
        dead = false;

        //mineGen = GetComponent("MineGen");

    }

    public void Initialize(int[,] mineSquaresIn, int numMinesIn, Vector3Int startSq)
    {

        width = mineSquaresIn.GetLength(0);
        height = mineSquaresIn.GetLength(1);

        mineSquares = mineSquaresIn;
        revealedSquares = new bool[width, height];
        flaggedSquares = new bool[width, height];

        hiddenSafeSquares = width * height - numMines;

        SetRemainingMines(numMinesIn);

        firstClick = false;
        dead = false;

        testMineGen.GenerateMines(mineSquaresIn);
        OpenTile(startSq);

        //mineGen = GetComponent("MineGen");

    }

    public int CountNeighboringMines(Vector3Int pos)
    {
        int sum = 0;

        HashSet<Vector3Int> neighbors = GetNeighbors(pos);

        foreach (Vector3Int currentPos in neighbors)
        {
            if (GetMineOrNumber(currentPos) == -1)
            {
                sum++;
            }
        }

        return sum;
    }

    public int CountNeighboringFlags(Vector3Int pos)
    {
        int sum = 0;

        HashSet<Vector3Int> neighbors = GetNeighbors(pos);

        foreach (Vector3Int currentPos in neighbors)
        {
            if (IsFlagged(currentPos))
            {
                sum++;
            }
        }

        return sum;
    }

    public void OpenTile(Vector3Int pos)
    {
        if (firstClick)
        {
            mineSquares = mineGen.GenerateMines(mineSquares, numMines, pos);
            firstClick = false;
        }
        if (!revealedSquares[pos.x, pos.y])
        {
            revealedSquares[pos.x, pos.y] = true;
            updatedTiles.Add(pos);
            hiddenSafeSquares--;
            if (mineSquares[pos.x, pos.y] == 0)
            {
                OpenNeighbors(pos);
            }
        }
        if (mineSquares[pos.x, pos.y] == -1)
        {
            Debug.Log("KA-BOOM");
            dead = true;
        }
    }

    public HashSet<Vector3Int> GetNeighbors(Vector3Int pos)
    {
        HashSet<Vector3Int> neighbors = new HashSet<Vector3Int>();

        bool lookLeft = pos.x > 0;
        bool lookRight = pos.x < width - 1;
        bool lookDown = pos.y > 0;
        bool lookUp = pos.y < height - 1;

        // Top Left
        if (lookLeft && lookUp) neighbors.Add(pos + Vector3Int.left + Vector3Int.up);

        // Top
        if (lookUp) neighbors.Add(pos + Vector3Int.up);

        // Top Right
        if (lookRight && lookUp) neighbors.Add(pos + Vector3Int.right + Vector3Int.up);

        // Left
        if (lookLeft) neighbors.Add(pos + Vector3Int.left);

        // Right
        if (lookRight) neighbors.Add(pos + Vector3Int.right);

        // Bottom Left
        if (lookLeft && lookDown) neighbors.Add(pos + Vector3Int.left + Vector3Int.down);

        // Bottom
        if (lookDown) neighbors.Add(pos + Vector3Int.down);

        // Bottom Right
        if (lookRight && lookDown) neighbors.Add(pos + Vector3Int.right + Vector3Int.down);

        return neighbors;
    }

    // Open neighboring cells. Used for expanding clearings and for chords,
    // so it checks if there is a flag before opening.
    public void OpenNeighbors(Vector3Int pos)
    {
        HashSet<Vector3Int> neighbors = GetNeighbors(pos);

        foreach (Vector3Int currentPos in neighbors)
        {
            if (!IsFlagged(currentPos))
            {
                OpenTile(currentPos);
            }
        }
    }

    public void ToggleFlag(Vector3Int pos)
    {
        if (pos.x >= 0 && pos.x < width &&
            pos.y >= 0 && pos.y < height &&
            !revealedSquares[pos.x, pos.y])
        {
            flaggedSquares[pos.x, pos.y] = !flaggedSquares[pos.x, pos.y];
            updatedTiles.Add(pos);
            if (flaggedSquares[pos.x, pos.y])
                SetRemainingMines(remainingMines - 1);
            else
                SetRemainingMines(remainingMines + 1);
        }
    }

    // Can Chord if it is revealed (checked for before this is called)
    // and it is a number greater than 0
    // and that number equals the number of neighboring flags.
    public bool CanChord(Vector3Int pos)
    {
        if (mineSquares[pos.x, pos.y] == 0) return false;
        return CountNeighboringFlags(pos) == mineSquares[pos.x, pos.y];
    }

    public int GetMineOrNumber(Vector3Int pos)
    {
        return mineSquares[pos.x, pos.y];
    }

    public bool IsFlagged(Vector3Int pos)
    {
        return flaggedSquares[pos.x, pos.y];
    }

    public bool IsRevealed(Vector3Int pos)
    {
        return revealedSquares[pos.x, pos.y];
    }

    public int RemainingMines()
    {
        return remainingMines;
    }

    public HashSet<Vector3Int> GetUpdatedTiles()
    {
        return updatedTiles;
    }

    public void ClearUpdatedTiles()
    {
        updatedTiles.Clear();
    }

    void SetRemainingMines(int newRemainingMines)
    {
        remainingMines = newRemainingMines;
        remainingMinesText.text = "Remaining Mines: " + remainingMines;
    }

}
