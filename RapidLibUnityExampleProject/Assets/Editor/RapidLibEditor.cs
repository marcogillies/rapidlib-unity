using UnityEngine;
using UnityEditor;
using System.Collections;

[CustomEditor(typeof(RapidLib))]
[CanEditMultipleObjects]
public class RapidLibEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        RapidLib rapidLib = (RapidLib)target;
        if (GUILayout.Button("add training example"))
        {
            rapidLib.AddTrainingExample();
        }

        if (GUILayout.Button("Collect Data"))
        {
            rapidLib.ToggleCollectingData();
        }

        if (GUILayout.Button("Run"))
        {
            rapidLib.ToggleRunning();
        }
        if (GUILayout.Button("train"))

        {
            rapidLib.Train();
        }
    }
}