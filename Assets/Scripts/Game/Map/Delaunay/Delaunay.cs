using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Delaunator
{
    public class Delaunay
    {
        public int maxX { get; set; }
        public int maxY { get; set; }

        private IEnumerable<Triangle> border;

        public IEnumerable<Triangle> BowyerWatson(IEnumerable<Point> points)
        {
            var triangulation = new HashSet<Triangle>();
            Triangle superTriangle = GenerateSuperTriangle(points);

            triangulation.Add(superTriangle);

            foreach (Point point in points)
            {
                var badTriangles = FindBadTriangles(point, triangulation);
                var polygon = FindHoleBoundaries(badTriangles);

                foreach (Triangle triangle in badTriangles)
                {
                    foreach (Point vertex in triangle.vertices)
                    {
                        vertex.adjacentTriangles.Remove(triangle);
                    }
                }
                triangulation.RemoveWhere(triangle => badTriangles.Contains(triangle));

                foreach (Edge edge in polygon.Where(possibleEdge => possibleEdge.point1 != point && possibleEdge.point2 != point))
                {
                    Triangle triangle = new Triangle(point, edge.point1, edge.point2);
                    triangulation.Add(triangle);
                }
            }

            triangulation.RemoveWhere(triangle => triangle.vertices.Any(vertex => superTriangle.vertices.Contains(vertex)));
            return triangulation;
        }

        private List<Edge> FindHoleBoundaries(ISet<Triangle> badTriangles)
        {
            var edges = new List<Edge>();

            foreach (Triangle triangle in badTriangles)
            {
                edges.Add(new Edge(triangle.vertices[0], triangle.vertices[1]));
                edges.Add(new Edge(triangle.vertices[1], triangle.vertices[2]));
                edges.Add(new Edge(triangle.vertices[2], triangle.vertices[0]));
            }

            var grouped = edges.GroupBy(edge => edge);
            var boundaryEdges = edges.GroupBy(edge => edge).Where(edge => edge.Count() == 1).Select(edge => edge.First());

            return boundaryEdges.ToList();
        }

        private ISet<Triangle> FindBadTriangles(Point point, HashSet<Triangle> triangles)
        {
            var badTriangles = triangles.Where(triangle => triangle.IsPointInsideCircumcircle(point));

            return new HashSet<Triangle>(badTriangles);
        }

        public Triangle GenerateSuperTriangle(IEnumerable<Point> points)
        {
            Point point1 = new Point(2 * maxX, -maxY);
            Point point2 = new Point(0, 2 * maxY);
            Point point3 = new Point(-2 * maxX, -maxY);

            return new Triangle(point1, point2, point3);
        }


        public IEnumerable<Point> TurnCentersToPoints(List<Room> rooms, int maxX, int maxY)
        {
            this.maxX = maxX;
            this.maxY = maxY;

            //  Commented code is for border generation instead of a super triangle.

            // Point point0 = new Point(-maxX, -maxY);
            // Point point1 = new Point(-maxX, maxY);
            // Point point2 = new Point(maxX, maxY);
            // Point point3 = new Point(maxX, -maxY);

            List<Point> points = new List<Point>();

            // Triangle triangle1 = new Triangle(point0, point1, point2);
            // Triangle triangle2 = new Triangle(point0, point2, point3);

            border = new List<Triangle>();

            foreach (var room in rooms)
            {
                points.Add(new Point(room.center.x, room.center.y));
                Debug.Log("Room center coords: " + room.center.x + "," + room.center.y);
            }

            return points;
        }
    }
}