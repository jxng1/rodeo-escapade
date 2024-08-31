using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Edge
{
    public Point point1 { get; }
    public Point point2 { get; }

    public Edge(Point point1, Point point2)
    {
        this.point1 = point1;
        this.point2 = point2;
    }

    public override bool Equals(object obj)
    {
        if (obj == null)
        {
            return false;
        }

        if (obj.GetType() != this.GetType())
        {
            return false;
        }

        Edge edge = (Edge)obj;

        bool same = this.point1 == edge.point1 && this.point2 == edge.point2;
        bool sameReversed = this.point1 == edge.point2 && this.point2 == edge.point1;

        return same || sameReversed;
    }

    public override int GetHashCode()
    {
        int hCode = (int)point1.x ^ (int)point1.y ^ (int)point2.x ^ (int)point2.y;
        return hCode.GetHashCode();
    }

    public bool IsOverlapping(Edge edge)
    {
        float uA = ((edge.point2.x - edge.point1.x) * (this.point1.y - edge.point1.y) - (edge.point2.y - edge.point1.y) * (this.point1.x - edge.point1.x)) / ((edge.point2.y - edge.point1.y) * (this.point2.x - this.point1.x) - (edge.point2.x - edge.point1.x) * (this.point2.y - this.point1.y));
        float uB = ((this.point2.x - this.point1.x) * (this.point1.y - edge.point1.y) - (this.point2.y - this.point1.y) * (this.point1.x - edge.point1.x)) / ((edge.point2.y - edge.point1.y) * (this.point2.x - this.point1.x) - (edge.point2.x - edge.point1.x) * (this.point2.y - this.point1.y));

        if (uA >= 0 && uA <= 1 && uB >= 0 && uB <= 1)
        {
            return true;
        }

        return false;
    }
}