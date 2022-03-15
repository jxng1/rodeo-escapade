using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(RoomManager))]
public class RoomManagerCustomGUI : Editor
{

    private bool roomNumberFoldout = true;
    private bool roomSizeFoldout = true;

    private bool tileMapSizeFoldout = true;

    RoomManager script;

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        RoomManager script = (RoomManager)target;

        tileMapSizeFoldout = EditorGUILayout.Foldout(tileMapSizeFoldout, "Tilemap Size", true);
        if (tileMapSizeFoldout)
        {
            script.tileMapSizeLength = EditorGUILayout.IntField("Length", script.tileMapSizeLength);
            script.tileMapSizeWidth = EditorGUILayout.IntField("Width", script.tileMapSizeWidth);
        }

        roomNumberFoldout = EditorGUILayout.Foldout(roomNumberFoldout, "Number of Rooms", true);
        if (roomNumberFoldout)
        {
            script.minRoomSize = EditorGUILayout.IntField("Min Rooms", script.minRoomSize);
            if (script.minRoomSize > script.maxRoomSize)
            {
                script.minRoomSize = script.maxRoomSize;
            }
            else if (script.minRoomSize < 5)
            {
                script.minRoomSize = 5;
            }

            script.maxRoomSize = EditorGUILayout.IntField("Max Rooms", script.maxRoomSize);
            if (script.maxRoomSize > 50)
            {
                script.maxRoomSize = 50;
            }
            else if (script.maxRoomSize < script.minRoomSize)
            {
                script.maxRoomSize = script.minRoomSize;
            }
        }

        roomSizeFoldout = EditorGUILayout.Foldout(roomSizeFoldout, "Room Settings", true);
        if (roomSizeFoldout)
        {
            script.minRoomLength = EditorGUILayout.IntField("Min Length", script.minRoomLength);
            if (script.minRoomLength > script.maxRoomLength)
            {
                script.minRoomLength = script.maxRoomLength;
            }
            else if (script.minRoomLength < 20)
            {
                script.minRoomLength = 20;
            }

            script.maxRoomLength = EditorGUILayout.IntField("Max Length", script.maxRoomLength);
            if (script.maxRoomLength > 100)
            {
                script.maxRoomLength = 100;
            }
            else if (script.maxRoomLength < script.minRoomLength)
            {
                script.maxRoomLength = script.minRoomLength;
            }

            EditorGUILayout.Space();

            script.minRoomWidth = EditorGUILayout.IntField("Min Width", script.minRoomWidth);
            if (script.minRoomWidth > script.maxRoomWidth)
            {
                script.minRoomWidth = script.maxRoomWidth;
            }
            else if (script.minRoomWidth < 20)
            {
                script.minRoomWidth = 20;
            }

            script.maxRoomWidth = EditorGUILayout.IntField("Max Width", script.maxRoomWidth);
            if (script.maxRoomWidth > 100)
            {
                script.maxRoomWidth = 100;
            }
            else if (script.maxRoomWidth < script.minRoomWidth)
            {
                script.maxRoomWidth = script.minRoomWidth;
            }
        }

        EditorGUILayout.Space();
        if (GUILayout.Button("Generate Rooms"))
        {
            script.generateRooms();
            //Debug.Log(script.roomsNotEmpty().ToString());
        }
        else if (GUILayout.Button("Clear Map"))
        {
            script.clearMap();
        }
        EditorGUILayout.Space();
        if (GUILayout.Button("Separate Rooms") && script.roomsNotEmpty())
        {
            //Debug.Log("Separating rooms...");
            script.separateRooms();
        }
        EditorGUILayout.Space();
        if (GUILayout.Button("Delaunay Triangulation"))
        {
            script.triangulate();
        }
        EditorGUILayout.Space();
        if (GUILayout.Button("Generate Super Triangle"))
        {
            script.generateSuperTriangle();
        }
        EditorGUILayout.Space();
        if (GUILayout.Button("Graphify"))
        {
            script.graphify();
        }
        EditorGUILayout.Space();
        if (GUILayout.Button("Prim's Spanning Tree"))
        {
            script.spanningTree();
        }
        EditorGUILayout.Space();
        if (GUILayout.Button("Path Splitter"))
        {
            script.generateLinearPaths();
        }
        EditorGUILayout.Space();
        if (GUILayout.Button("Add rooms on the path"))
        {
            script.addNormalRoomsToMap();
        }
        EditorGUILayout.Space();
        if (GUILayout.Button("Tile rooms"))
        {
            script.tileRooms();
        }
    }
}
