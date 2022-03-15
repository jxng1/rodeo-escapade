using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(TileAutomata))]
public class CustomInspector : Editor
{
    public TileAutomata tileAutomataScript;

   public override void OnInspectorGUI() {
       DrawDefaultInspector();

       TileAutomata script = (TileAutomata) target;
       if (GUILayout.Button("Simulate Map")) {
           script.simulate(script.iterations);
       } else if (GUILayout.Button("Clear Simulation Map")) {
           script.clearMaps(true);
       } else if (GUILayout.Button("Save Simulation Map")) {
           script.saveAssetMap();
       }
   }
}
