using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(BldgPlacement))]
public class BldgPlacementEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        BldgPlacement BldgPlace = (BldgPlacement)target;

        if (GUILayout.Button("Place Buildings"))
        {
            BldgPlace.PlaceBuildings();
        }
    }
}
