using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public enum RoomType
{
    Key, Normal
}

public class Room
{
    public Vector2 center { get; set; }
    public int length { get; set; }
    public int width { get; set; }

    public int area { get; }
    public RoomType roomType { get; set; }

    private int id { get; }

    private Tile floorTile;
    private Tile wallTile;

    public Room(int id, int x, int y, int length, int width, Tile floorTile, Tile wallTile)
    {
        this.id = id;
        this.roomType = RoomType.Normal;

        this.center = new Vector2(x, y);
        this.length = length;
        this.width = width;
        this.area = width * length;

        this.wallTile = wallTile;
        this.floorTile = floorTile;
    }

    public void Straighten()
    {
        center = new Vector2(Mathf.Round(center.x), Mathf.Round(center.y));
    }

    public void Spawn(Tilemap map, Tile tile)
    {
        for (float x = center.x - length / 2; x < center.x + length / 2; x++)
        {
            for (float y = center.y - width / 2; y < center.y + width / 2; y++)
            {
                Vector3Int pos = new Vector3Int((int)x, (int)y, 0);
                Matrix4x4 matrix = Matrix4x4.TRS(Vector3.zero, Quaternion.Euler(0f, 0f, 0f), Vector3.one);

                switch (Random.Range(0, 4))
                {
                    case 0:
                        matrix = Matrix4x4.TRS(Vector3.zero, Quaternion.Euler(0f, 0f, 90f), Vector3.one);
                        break;
                    case 1:
                        matrix = Matrix4x4.TRS(Vector3.zero, Quaternion.Euler(0f, 0f, 180f), Vector3.one);
                        break;
                    case 2:
                        matrix = Matrix4x4.TRS(Vector3.zero, Quaternion.Euler(0f, 0f, 270f), Vector3.one);
                        break;
                    case 3:
                        matrix = Matrix4x4.TRS(Vector3.zero, Quaternion.Euler(0f, 0f, 360f), Vector3.one);
                        break;
                    default:
                        break;
                }

                map.SetTile(pos, tile);
                map.SetTransformMatrix(pos, matrix);
            }
        }
    }

    public bool IsOverlapping(Room room, int adjustment)
    {
        return this.center.x + this.length / 2 + (adjustment / 2) >= room.center.x - room.length / 2 // xmax1 >= xmin2
        && room.center.x + room.length / 2 >= this.center.x - this.length / 2 - (adjustment / 2)// xmax2 >= xmin2
        && this.center.y + this.width / 2 + (adjustment / 2) >= room.center.y - room.width / 2 // ymax1 >= ymin2
        && room.center.y + room.width / 2 >= this.center.y - this.width / 2 - (adjustment / 2); // ymax2 >= ymin2
    }

    public bool IsOverlapping(Edge edge)
    {
        /*
          center.x-length/2, center.y+width/2 -> ----- <- center.x+length/2, center.y+width/2
                                                 |   |
                                                 |   |
          center.x-length/2, center.y-width/2 -> ----- <- center.x+length/2, center.y-width/2

          Credit to Jeffrey Thompson: http://jeffreythompson.org/collision-detection/line-rect.php
        */

        bool left = edge.IsOverlapping(new Edge(new Point(this.center.x - length / 2, this.center.y + width / 2)
        , new Point(this.center.x - length / 2, this.center.y - width / 2))); // left edge of a rectangle

        bool right = edge.IsOverlapping(new Edge(new Point(center.x + length / 2, center.y + width / 2)
        , new Point(center.x + length / 2, center.y - width / 2))); // right edge of a rectangle

        bool top = edge.IsOverlapping(new Edge(new Point(center.x - length / 2, center.y + width / 2)
        , new Point(center.x + length / 2, center.y + width / 2)));

        bool bottom = edge.IsOverlapping(new Edge(new Point(center.x - length / 2, center.y - width / 2)
        , new Point(center.x + length / 2, center.y - width / 2)));

        if (left || right || top || bottom)
        {
            return true;
        }

        return false;
    }

    public bool IsOverlapping(Point point, int adjustment)
    {
        return this.center.x + this.length / 2 + (adjustment / 2) >= point.x
        && this.center.x - this.length / 2 - (adjustment / 2) <= point.x
        && this.center.y + this.width / 2 + (adjustment / 2) >= point.y
        && this.center.x - this.width / 2 - (adjustment / 2) <= point.y;
    }
}
