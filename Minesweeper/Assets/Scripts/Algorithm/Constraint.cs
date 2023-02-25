using System.Collections.Generic;
using UnityEngine;

class Constraint
{
    public Vector3Int numberSquare;
    public HashSet<Vector3Int> constrainedSquares;
    public int minMines;
    public int maxMines;

    public Constraint(Vector3Int numberSquare, HashSet<Vector3Int> constrainedSquares, int minMines, int maxMines)
    {
        this.numberSquare = numberSquare;
        this.minMines = minMines;
        this.maxMines = maxMines;
        this.constrainedSquares = new();
        foreach (Vector3Int square in constrainedSquares)
        {
            this.constrainedSquares.Add(new(square.x, square.y));
        }
    }

    public Constraint(Constraint constraint)
    {
        this.numberSquare = constraint.numberSquare;
        this.minMines = constraint.minMines;
        this.maxMines = constraint.maxMines;
        this.constrainedSquares = new();
        foreach (Vector3Int square in constraint.constrainedSquares)
        {
            this.constrainedSquares.Add(new(square.x, square.y));
        }
    }

    public bool Equals(Constraint other)
    {
        if (numberSquare != other.numberSquare) return false;
        if (minMines != other.minMines) return false;
        if (maxMines != other.maxMines) return false;
        if (constrainedSquares.Count != other.constrainedSquares.Count) return false;

        foreach (Vector3Int square in other.constrainedSquares)
        {
            if (!constrainedSquares.Contains(square)) return false;
        }

        return true;
    }

    // Check if the constraint is applied to the same squares
    public bool SameSquares(Constraint other)
    {
        if (numberSquare != other.numberSquare) return false;
        if (constrainedSquares.Count != other.constrainedSquares.Count) return false;

        foreach (Vector3Int square in other.constrainedSquares)
        {
            if (!constrainedSquares.Contains(square)) return false;
        }

        return true;
    }
}
