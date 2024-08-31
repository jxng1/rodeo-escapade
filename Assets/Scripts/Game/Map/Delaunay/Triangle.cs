using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Triangle
{
    public Point[] vertices { get; } = new Point[3];
    public Point circumcenter { get; private set; }

    public float radiusSquared;

    public IEnumerable<Triangle> trianglesWithSharedEdge
    {
        get
        {
            HashSet<Triangle> neighbours = new HashSet<Triangle>();

            foreach (Point vertex in vertices)
            {
                var trianglesWithSharedEdge = vertex.adjacentTriangles.Where(vertex =>
                {
                    return vertex != this && SharesEdgesWith(vertex);
                });
            }

            return neighbours;
        }
    }

    public Triangle(Point point1, Point point2, Point point3)
    {
        if (point1 == point2 || point1 == point3 || point2 == point3)
        {
            Debug.LogError("Points must be distinct!");
            return;
        }

        if (!IsCounterClockwise(point1, point2, point3))
        { // if clockwise
            vertices[0] = point1;
            vertices[1] = point3;
            vertices[2] = point2;
        }
        else
        {
            vertices[0] = point1;
            vertices[1] = point2;
            vertices[2] = point3;
        }

        vertices[0].adjacentTriangles.Add(this);
        vertices[1].adjacentTriangles.Add(this);
        vertices[2].adjacentTriangles.Add(this);
        UpdateCircumcircle();
    }
    private void UpdateCircumcircle()
    {
        // https://codefound.wordpress.com/2013/02/21/how-to-compute-a-circumcircle/#more-58
        // https://en.wikipedia.org/wiki/Circumscribed_circle
        var p0 = vertices[0];
        var p1 = vertices[1];
        var p2 = vertices[2];

        var dA = p0.x * p0.x + p0.y * p0.y;
        var dB = p1.x * p1.x + p1.y * p1.y;
        var dC = p2.x * p2.x + p2.y * p2.y;

        var aux1 = (dA * (p2.y - p1.y) + dB * (p0.y - p2.y) + dC * (p1.y - p0.y));
        var aux2 = -(dA * (p2.x - p1.x) + dB * (p0.x - p2.x) + dC * (p1.x - p0.x));
        var div = (2 * (p0.x * (p2.y - p1.y) + p1.x * (p0.y - p2.y) + p2.x * (p1.y - p0.y)));

        if (div == 0)
        {
            Debug.LogError("Division by zero!");
            return;
        }

        var center = new Point(aux1 / div, aux2 / div);
        circumcenter = center;
        radiusSquared = (center.x - p0.x) * (center.x - p0.x) + (center.y - p0.y) * (center.y - p0.y);
    }

    public bool SharesEdgesWith(Triangle triangle)
    {
        return vertices.Where(vertex => triangle.vertices.Contains(vertex)).Count() == 2;
    }

    private bool IsCounterClockwise(Point point1, Point point2, Point point3)
    {
        return ((point2.x - point1.x) * (point3.y - point1.y) - (point3.x - point1.x) * (point2.y - point1.y)) > 0;
    }

    public bool IsPointInsideCircumcircle(Point point)
    {
        return ((point.x - circumcenter.x) * (point.x - circumcenter.x) + (point.y - circumcenter.y) * (point.y - circumcenter.y)) < radiusSquared;
    }

    public bool Contains(Point point) {
        return vertices.Any(vertex => vertex.Equals(point));
    }
}