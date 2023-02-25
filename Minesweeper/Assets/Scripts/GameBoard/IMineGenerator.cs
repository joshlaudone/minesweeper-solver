using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IMineGenerator
{

    public int[,] GenerateMines(int[,] mineSquares, int numMines, Vector3Int click);

}
