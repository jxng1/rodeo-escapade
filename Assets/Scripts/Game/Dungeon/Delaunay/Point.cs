using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Point
{
    public float x { get; }
    public float y { get; }

    public HashSet<Triangle> adjacentTriangles { get; } = new HashSet<Triangle>();

    public Point(float x, float y)
    {
        this.x = x;
        this.y = y;
    }
}
