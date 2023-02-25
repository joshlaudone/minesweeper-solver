using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MineBoardLoader : MonoBehaviour, IMineGenerator
{

    int width, height;
    int[,] mineSquares;

    public int[,] GenerateMines(int[,] mineSquaresIn, int numMines, Vector3Int click)
    {
        string[] lines = System.IO.File.ReadAllLines("C:\\Users\\joshl\\Unity\\Minesweeper\\Assets\\Boards\\evil\\1707789460-hd-34.in");

        return mineSquares;
    }

}
