using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class IsNeighbor
{
    [Test]
    public void HorizontalNeighbor()
    {
        Vector3Int a = new(1, 0, 0);
        Vector3Int b = new(0, 0, 0);
        Assert.AreEqual(true, Sweepotron_AI.IsNeighbor(a, b));
    }

    [Test]
    public void VerticalNeighbor()
    {
        Vector3Int a = new(5, 6, 0);
        Vector3Int b = new(5, 5, 0);
        Assert.AreEqual(true, Sweepotron_AI.IsNeighbor(a, b));
    }

    [Test]
    public void DiagonalNeighbor()
    {
        Vector3Int a = new(4, 5, 0);
        Vector3Int b = new(3, 4, 0);
        Assert.AreEqual(true, Sweepotron_AI.IsNeighbor(a, b));
    }

    [Test]
    public void SameColumn()
    {
        Vector3Int a = new(19, 2, 0);
        Vector3Int b = new(19, 7, 0);
        Assert.AreEqual(false, Sweepotron_AI.IsNeighbor(a, b));
    }

    [Test]
    public void SameRow()
    {
        Vector3Int a = new(6, 4, 0);
        Vector3Int b = new(3, 4, 0);
        Assert.AreEqual(false, Sweepotron_AI.IsNeighbor(a, b));
    }

    [Test]
    public void FarApart()
    {
        Vector3Int a = new(21, 15, 0);
        Vector3Int b = new(3, 8, 0);
        Assert.AreEqual(false, Sweepotron_AI.IsNeighbor(a, b));
    }
}
