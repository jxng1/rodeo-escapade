using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using Delaunator;
using Pathfinder;
using UnityEditor;
using System.Linq;

public class RoomManager : Singleton<RoomManager>
{
    [HideInInspector] public int minRoomSize = 5;

    [HideInInspector] public int maxRoomSize = 50;

    [HideInInspector] public int minRoomLength = 20;
    [HideInInspector] public int maxRoomLength = 100;

    [HideInInspector] public int minRoomWidth = 20;
    [HideInInspector] public int maxRoomWidth = 100;

    [HideInInspector] public int tileMapSizeLength = 500;
    [HideInInspector] public int tileMapSizeWidth = 500;
    [HideInInspector] public Vector3 center;

    // map
    public List<Tile> spawnableTiles;
    public Tile floorTile;
    public Tile wallTile;
    public Tilemap map;

    // rooms
    private List<Room> rooms = new List<Room>();
    private List<Room> keyRooms = new List<Room>();

    // delaunay
    private Delaunay delaunator = new Delaunay();
    private IEnumerable<Point> points;
    private IEnumerable<Triangle> triangulation;
    private List<Edge> edges;
    public int separationAdjustment = 2;

    // graph
    private Graph<Point> graph;
    private List<Node<Point>> tree;
    private List<Edge> treeEdges = new List<Edge>();

    // linear path
    private List<Edge> linePath = new List<Edge>();

    // final rooms
    private List<Room> finalRooms = new List<Room>();

    // TESTING ONLY
    Triangle sTriangle;
    private List<Edge> superTEdges = new List<Edge>();

    protected RoomManager()
    { }

    private void Update()
    {
        if (rooms.Count > 0 && roomsHaveOverlap())
        {
            separateRooms();
            if (keyRooms.Count >= 3)
            {
                triangulate();
            }
        }
    }

    public void generateRooms()
    {
        center = transform.position;

        int numOfRooms = Random.Range(minRoomSize, maxRoomSize + 1);

        // Generate rooms
        for (int i = 0; i < numOfRooms; i++)
        {
            int length = Random.Range(minRoomLength, maxRoomLength + 1);
            int width = Random.Range(minRoomWidth, maxRoomWidth + 1);
            Vector2 randomCenter = getRandomPointInCircle(tileMapSizeLength / 4);

            Room newRoom = new Room(i, (int)randomCenter.x, (int)randomCenter.y, length, width, floorTile, wallTile);
            rooms.Add(newRoom);
        }

        identifyKeyRooms();
    }

    public void clearMap() // doesn't work properly as of now, it should reinstantiate, not set to null
    {
        // clear existing tiles off map
        map.ClearAllTiles();

        // clear rooms
        rooms = null;
        keyRooms = null;

        // clear triangulation
        triangulation = null;
        edges = null;
        points = null;

        // clear super triangle
        sTriangle = null;
        superTEdges = null;

        // clear pathfinding
        graph = null;
        tree = null;
    }

    private Vector2 getRandomPointInCircle(int radius)
    {
        float t = 2 * Mathf.PI * Random.Range(0f, 1f);
        float u = Random.Range(0f, 1f) + Random.Range(0f, 1f);
        float r = 0f;

        if (u > 1)
        {
            r = 2 - u;
        }
        else
        {
            r = u;
        }

        return new Vector2(roundm(radius * r * Mathf.Cos(t), 16), roundm(radius * r * Mathf.Sin(t), 16));
    }

    private int roundm(float n, int size)
    {
        return Mathf.FloorToInt(((n + size - 1) / size)) * size;
    }

    public bool roomsNotEmpty()
    {
        return rooms.Count > 0;
    }

    public bool roomsHaveOverlap()
    {
        foreach (Room roomA in rooms)
        {
            foreach (Room roomB in rooms)
            {
                if (roomA == roomB)
                {
                    continue;
                }

                if (roomA.isOverlapping(roomB, separationAdjustment))
                {
                    return true;
                }
            }
        }

        return false;
    }

    public void separateRooms()
    {
        int sumArea = 0;

        foreach (Room room in rooms)
        {
            sumArea += room.length * room.width;
        }

        foreach (Room agent in rooms)
        {
            Vector2 totalForce = Vector2.zero;
            int neighbours = 0;

            foreach (Room room in rooms)
            {
                Vector2 separation = Vector2.zero;

                if (agent == room)
                {
                    continue;
                }

                if (!agent.isOverlapping(room, separationAdjustment))
                {
                    continue;
                }

                Room neighbour = room;
                separation = agent.center - neighbour.center;

                if (separation.x == 0)
                {
                    separation.x = (float)((1 - 2 * Random.Range(0f, 1f)) * 0.5);
                }
                if (separation.y == 0)
                {
                    separation.y = (float)((1 - 2 * Random.Range(0f, 1f)) * 0.5);
                }

                int xDir = separation.x < 0 ? -1 : 1;
                int yDir = separation.y < 0 ? -1 : 1;

                Vector2 pushForce = new Vector2(
                    x:
                        xDir < 0
                            ? separation.x - agent.length
                            : separation.x + agent.length,
                        yDir < 0
                            ? separation.y - agent.width
                            : separation.y + agent.width
                );

                float mass = agent.length * agent.width / separationAdjustment;

                totalForce += (pushForce / mass);
                neighbours++;
            }

            if (neighbours > 0 && (totalForce.x == 0 || totalForce.y == 0))
            {
                Debug.Log("WARNING");
            }

            if (neighbours > 0)
            {
                totalForce /= neighbours;
            }

            if (agent.center.x + totalForce.x + agent.length > center.x + tileMapSizeLength / 2 || agent.center.x - totalForce.x - agent.length < center.x - tileMapSizeLength / 2
            || agent.center.y + totalForce.y + agent.width > center.y + tileMapSizeWidth / 2 || agent.center.y - totalForce.y - agent.width < center.y - tileMapSizeWidth / 2)
            {
                tileMapSizeLength += separationAdjustment;
                tileMapSizeWidth += separationAdjustment;
                //Debug.Log(tileMapSizeLength);
                //Debug.Log(tileMapSizeWidth);
            }

            // if (sumArea < tileMapSizeLength * tileMapSizeWidth
            // && (agent.center.x + totalForce.x + agent.length > center.x + tileMapSizeLength / 2 || agent.center.x - totalForce.x - agent.length < center.x - tileMapSizeLength / 2
            //     || agent.center.y + totalForce.y + agent.width > center.y + tileMapSizeWidth / 2 || agent.center.y - totalForce.y - agent.width < center.y - tileMapSizeWidth / 2))
            // {
            //     totalForce *= -0.9f;
            // }
            // else if (sumArea > tileMapSizeLength * tileMapSizeWidth)
            // {
            //     Debug.Log("Summed area size of rooms is greater than max tilemap size, tilemap size will be increased.");
            // }

            agent.center += totalForce;
        }
    }

    public void identifyKeyRooms()
    {
        keyRooms.Clear();
        rooms.ForEach(room => room.roomType = RoomType.Normal); // reset all rooms to normal

        int keyRoomAreaCutoff = Mathf.RoundToInt(findAverageAreaOfRoom() * 1.4f); // 1.4x area of rooms greater than average room area will be key rooms.

        keyRooms = rooms.FindAll(room => room.area >= keyRoomAreaCutoff);
        keyRooms.ForEach(room => room.roomType = RoomType.Key);
    }

    public void triangulate()
    {
        points = delaunator.turnCentersToPoints(keyRooms, tileMapSizeLength, tileMapSizeWidth);
        generateSuperTriangle();
        triangulation = delaunator.bowyerWatson(points);
        updateEdges();
    }

    public void generateSuperTriangle() // for visualisation
    {
        superTEdges.Clear();
        sTriangle = delaunator.generateSuperTriangle(points);

        superTEdges.Add(new Edge(sTriangle.vertices[0], sTriangle.vertices[1]));
        superTEdges.Add(new Edge(sTriangle.vertices[1], sTriangle.vertices[2]));
        superTEdges.Add(new Edge(sTriangle.vertices[2], sTriangle.vertices[0]));
    }

    public void updateEdges()
    {
        List<Edge> updatedEdges = new List<Edge>();
        foreach (Triangle triangle in triangulation)
        {
            updatedEdges.Add(new Edge(triangle.vertices[0], triangle.vertices[1]));
            updatedEdges.Add(new Edge(triangle.vertices[1], triangle.vertices[2]));
            updatedEdges.Add(new Edge(triangle.vertices[2], triangle.vertices[0]));
        }

        edges = updatedEdges;
    }

    private int findAverageAreaOfRoom()
    {
        int sum = 0;
        rooms.ForEach(room => sum += room.area);

        return sum / rooms.Count;
    }

    public void graphify()
    {
        //rooms.ForEach(room => Debug.Log("BEFORE: Room center: " + room.center.x + "," + room.center.y));

        // rooms.ForEach(room => room.straighten());
        // keyRooms.ForEach(room => room.straighten());

        triangulate();
        rooms.ForEach(room => Debug.Log("AFTER: Room center: " + room.center.x + "," + room.center.y));
        this.graph = new Graph<Point>(keyRooms.Count);
        int currId = 1;

        foreach (Edge edge in edges)
        {
            Node<Point> nodeA = graph.nodes.Find(node => node.data == edge.point1);
            Node<Point> nodeB = graph.nodes.Find(node => node.data == edge.point2);


            if (nodeA == null)
            {
                nodeA = graph.addNode(currId, edge.point1);
                //Debug.Log("Adding node with id: " + currId);

                currId++;
            }

            if (nodeB == null)
            {
                nodeB = graph.addNode(currId, edge.point2);
                //Debug.Log("Adding node with id: " + currId);

                currId++;
            }

            float edgeCost = calculateEdgeCost(nodeA, nodeB);
            if (edgeCost > 0)
            {
                graph.  addUndirectedEdge(nodeA.id, nodeB.id, edgeCost);
            }
            Debug.Log("Node A has center: " + nodeA.data.x + "," + nodeA.data.y);
            Debug.Log("Node B has center: " + nodeB.data.x + "," + nodeB.data.y);
            //Debug.Log("Adding edge with cost of " + edgeCost + " between node (" + nodeA.id + ") and node (" + nodeB.id + ")");
            // drawString(edgeCost.ToString()
            // , new Vector3(edge.point1.x + edge.point2.x / 2, edge.point1.y + edge.point2.y / 2, 0)
            // , Color.cyan);
        }
        //graph.printDebugGraph();
    }

    public void spanningTree()
    {
        //Debug.Log("----------Spanning Tree----------");
        treeEdges.Clear();
        tree = graph.primSpanningTree();

        Dictionary<Node<Point>, List<Node<Point>>> map = new Dictionary<Node<Point>, List<Node<Point>>>(); // from -> to(which can have many) e.g. node 1 to 2, 3, 4

        foreach (var node in tree)
        {
            map.Add(node, new List<Node<Point>>());
        }

        // add edges
        for (int i = 0; i < tree.Count - 1; i++)
        {
            // Debug.Log("Node has center of " + tree[i].data.x + "," + tree[i].data.y);
            if (tree[i].neighbours.Contains(tree[i + 1])) // normal pathing
            {
                treeEdges.Add(new Edge(tree[i].data, tree[i + 1].data));

                map[tree[i]].Add(tree[i + 1]);
                map[tree[i + 1]].Add(tree[i]);
            }
            else
            { // rogue node, choose one of its neighbours to add its path to
                //Debug.Log("Rogue node encountered.");
                var neighbour = tree[i + 1].neighbours[Random.Range(0, tree[i + 1].neighbours.Count)];

                // Debug.Log("Added edge from rogue node: " + tree[i + 1].id + " with it's neighbour: " + neighbour.id);

                treeEdges.Add(new Edge(neighbour.data, tree[i + 1].data));


                map[neighbour].Add(tree[i + 1]);
                map[tree[i + 1]].Add(neighbour);
            }
        }

        //Debug.Log("----------Add loops to tree----------");
        // add some looping
        System.Random random = new System.Random();
        int noOfLoops = random.Next(0, (int)(graph.nodes.Count * 0.3)); // number between 0 - .3 of rooms count of loops
        //Debug.Log("Number of loops to add: " + noOfLoops);

        while (noOfLoops > 0)
        {
            int index = random.Next(0, tree.Count); // random node to pick

            if ((index + 1 > tree.Count - 1) || (index - 1) < 0) // random node picked is last node
            {
                continue;
            }

            var node = tree[index];
            //Debug.Log("Node picked: " + node.id);

            List<Node<Point>> potentials = node.neighbours.FindAll(n => !map[node].Contains(n));

            //potentials.ForEach(node => Debug.Log("Potential Loop Id: " + node.id));

            if (potentials.Count != 0)
            {
                Node<Point> randomNeighbour;

                randomNeighbour = potentials[random.Next(0, potentials.Count)];

                treeEdges.Add(new Edge(new Point(node.data.x, node.data.y)
                , new Point(randomNeighbour.data.x, randomNeighbour.data.y))); // add loop to edges

                map[node].Add(randomNeighbour);
                //Debug.Log("Added new edge from node " + node.id + " to node " + randomNeighbour.id);
                noOfLoops--;
            }
            else
            {
                Debug.Log("There were no potential loops for this node!");
                continue;
            }
        }
    }

    public float calculateEdgeCost(Node<Point> nodeA, Node<Point> nodeB)
    {
        if (nodeA == null || nodeB == null)
        {
            Debug.LogError("One of the nodes are null!");
            return 0;
        }

        float a = Mathf.Pow((nodeA.data.x - nodeB.data.x), 2);
        float b = Mathf.Pow((nodeA.data.y - nodeB.data.y), 2);

        return Mathf.Sqrt(a + b);
    }

    public List<Edge> generateLinearPaths()
    {
        linePath = null;
        /* foreach edge in treeEdges:
        - if the two points are horizontally close enough, create a horizontal line
        - if the two points are vertically close enough, create a vertical line
        - if neither, we split it into an L shape: two lines
        */

        List<Edge> linearPath = new List<Edge>();

        rooms.ForEach(room => Debug.Log("Room center: " + room.center.x + "," + room.center.y));
        foreach (var edge in treeEdges)
        {
            //Debug.Log("Edge Point 1 Coords: " + edge.point1.x + "," + edge.point1.y);
            // Debug.Log("Edge Point 2 Coords: " + edge.point2.x + "," + edge.point2.y);
            if (isWithinBounds(new Point(edge.point2.x, edge.point1.y), edge.point2)) // y positions are similar - horizontal line
            {
                Debug.Log("Edges are close on y axis.");
                linearPath.Add(new Edge(edge.point1, new Point(edge.point2.x, edge.point1.y)));
            }
            else if (isWithinBounds(new Point(edge.point1.x, edge.point2.y), edge.point2)) // x positions are similar - vertical line
            {
                Debug.Log("Edges are close on x axis.");
                linearPath.Add(new Edge(edge.point1, new Point(edge.point1.x, edge.point2.y)));
            }
            else
            { // neither - generate L shape
              // Debug.Log("Edges are not close.");
                switch (Random.Range(0, 2))
                {
                    case 0:
                        //Debug.Log("Selected left down for pathing.");
                        linearPath.Add(new Edge(edge.point1, new Point(edge.point2.x, edge.point1.y)));
                        linearPath.Add(new Edge(edge.point2, new Point(edge.point2.x, edge.point1.y)));
                        break;
                    case 1:
                        // Debug.Log("Selected down left for pathing.");
                        linearPath.Add(new Edge(edge.point1, new Point(edge.point1.x, edge.point2.y)));
                        linearPath.Add(new Edge(edge.point2, new Point(edge.point1.x, edge.point2.y)));
                        break;
                    default:
                        Debug.Log("Issues occured.");
                        break;
                }
            }
        }

        this.linePath = linearPath;

        return linearPath;
    }

    public bool isWithinBounds(Point pointA, Point pointB)
    {
        rooms.ForEach(room => Debug.Log("Room center: " + room.center.x + "," + room.center.y));
        Debug.Log("Point center: " + pointB.x + "," + pointB.y);
        Room room = rooms.First(room => room.center.Equals(new Vector2(pointB.x, pointB.y)));

        if (room == null)
        {
            return false;
        }

        return room.center.x + room.length / 2 >= pointA.x
        && room.center.x - room.length / 2 <= pointA.x
        && room.center.y + room.width / 2 >= pointA.y
        && room.center.y - room.width / 2 <= pointA.y;
    }

    public void addNormalRoomsToMap()
    {
        finalRooms.Clear();
        // foreach normal room, if they intersect with said linear path, then add to them final map

        keyRooms.ForEach(room => finalRooms.Add(room));

        foreach (Room room in rooms)
        {
            if (finalRooms.Contains(room)) continue;

            foreach (Edge edge in linePath)
            {
                if (room.isOverlapping(edge))
                {
                    finalRooms.Add(room);
                    Debug.Log("Total of: " + finalRooms.Count + " rooms.");
                }
            }
        }
    }

    public void tileRooms()
    {
        map.ClearAllTiles();
        foreach (Room room in finalRooms)
        {
            room.spawn(map, floorTile);
        }
    }

    private void OnDrawGizmos()
    {
        // draw border gizmos
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireCube(center, new Vector2(tileMapSizeLength, tileMapSizeWidth));

        // draw room gizmos
        if (rooms != null)
        {
            foreach (Room room in rooms)
            {
                if (room.roomType == RoomType.Normal)
                {
                    Gizmos.color = Color.green;
                }
                else if (room.roomType == RoomType.Key)
                {
                    Gizmos.color = Color.red;
                }

                Gizmos.DrawWireCube(new Vector3(room.center.x, room.center.y, 0), new Vector2(room.length, room.width));
            }
        }

        // draw triangulation gizmos
        if (keyRooms != null && triangulation != null)
        {
            Gizmos.color = Color.blue;
            foreach (Edge edge in edges)
            {
                Gizmos.DrawLine(new Vector3(edge.point1.x, edge.point1.y, 0), new Vector3(edge.point2.x, edge.point2.y, 0));
            }
        }

        // draw sTriangle
        if (sTriangle != null)
        {
            // foreach (Room room in keyRooms)
            // {
            //     Handles.Label(new Vector3(room.center.x, room.center.y, 0),
            //     string.Format("X: {0:0.00} Y: {1:0.00}", room.center.x, room.center.y));
            // }

            // draw sTriangle's vertices
            Gizmos.color = Color.yellow;
            foreach (Point point in sTriangle.vertices)
            {
                Gizmos.DrawSphere(new Vector3(point.x, point.y, 0), 10);
            }

            // draw sTriangle's edges
            Gizmos.color = Color.yellow;
            foreach (Edge edge in superTEdges)
            {
                Gizmos.DrawLine(new Vector3(edge.point1.x, edge.point1.y, 0), new Vector3(edge.point2.x, edge.point2.y, 0));
            }
        }

        // draw tree gizoms
        if (tree != null)
        {
            // draw nodes
            Gizmos.color = Color.green;
            for (int i = 0; i < tree.Count; i++)
            {
                if (tree[i] == null)
                {
                    Debug.Log("Error!");
                }

                Handles.Label(new Vector3(tree[i].data.x, tree[i].data.y - 10, 0), tree[i].id.ToString());

                // red square is start, green square is finish, spheres are intermediate rooms
                Gizmos.color = i == 0 ? Color.red : Color.green;
                if (i == 0 || i == tree.Count - 1)
                {
                    Gizmos.DrawCube(new Vector3(tree[i].data.x, tree[i].data.y, 0), 7.5f * Vector3.one);
                }
                else
                {
                    Gizmos.DrawSphere(new Vector3(tree[i].data.x, tree[i].data.y, 0), 3.5f);
                }
            }

            // draw optimal path
            // Gizmos.color = Color.white;
            // for (int i = 0; i < tree.Count - 1; i++)
            // {
            //     Gizmos.DrawLine(new Vector3(tree[i].data.x, tree[i].data.y),
            //     new Vector3(tree[i + 1].data.x, tree[i + 1].data.y));
            // }

            Gizmos.color = Color.white;
            foreach (var edge in treeEdges)
            {
                if (edge == null)
                {
                    Debug.Log("Error occured.");
                    break;
                }

                Gizmos.DrawLine(new Vector3(edge.point1.x, edge.point1.y, 0)
                , new Vector3(edge.point2.x, edge.point2.y, 0));
            }
        }

        if (linePath != null)
        {
            Gizmos.color = Color.yellow;
            foreach (Edge edge in linePath)
            {
                Gizmos.DrawLine(new Vector3(edge.point1.x, edge.point1.y),
                new Vector3(edge.point2.x, edge.point2.y));
            }
        }

        if (finalRooms != null)
        {
            foreach (Room room in finalRooms)
            {
                if (room.roomType == RoomType.Normal)
                {
                    Gizmos.color = Color.magenta;
                }
                else if (room.roomType == RoomType.Key)
                {
                    Gizmos.color = Color.red;
                }

                Gizmos.DrawWireCube(new Vector3(room.center.x, room.center.y, 0), new Vector2(room.length, room.width));
            }
        }
    }

    static void drawString(string text, Vector3 worldPos, Color? colour = null)
    {
        UnityEditor.Handles.BeginGUI();
        if (colour.HasValue) GUI.color = colour.Value;
        var view = UnityEditor.SceneView.currentDrawingSceneView;
        Vector3 screenPos = view.camera.WorldToScreenPoint(worldPos);
        Vector2 size = GUI.skin.label.CalcSize(new GUIContent(text));
        GUI.Label(new Rect(screenPos.x - (size.x / 2), -screenPos.y + view.position.height + 4, size.x, size.y), text);
        UnityEditor.Handles.EndGUI();
    }
}