using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BasicMineGenerator : MonoBehaviour, IMineGenerator
{

    int width, height;
    int[,] mineSquares;

    public int[,] GenerateMines(int[,] mineSquaresIn, int numMines, Vector3Int click)
    {
        int minesGenerated = 0;
        Vector3Int pos = new Vector3Int();
        mineSquares = mineSquaresIn;
        height = mineSquares.GetLength(1);
        width = mineSquares.GetLength(0);

        // Add mines until total = numMines
        // Just purely picking a random pos might not be great with very dense boards
        while (minesGenerated < numMines)
        {
            pos.x = UnityEngine.Random.Range(0, width); // subtract 1 because it's inclusive
            pos.y = UnityEngine.Random.Range(0, height);

            // Check that there's not already a mine and that it won't make the first square have a number
            if (mineSquares[pos.x, pos.y] != -1 &&
                !(System.Math.Abs(pos.x - click.x) <= 1 && System.Math.Abs(pos.y - click.y) <= 1))
            {
                mineSquares[pos.x, pos.y] = -1;
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
                    pos.x = ii;
                    pos.y = jj;
                    mineSquares[ii, jj] = CountNeighboringMines(pos);
                }
            }
        }

        return mineSquares;
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

    public int GetMineOrNumber(Vector3Int pos)
    {
        return mineSquares[pos.x, pos.y];
    }
}
