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
<<<<<<< HEAD
        if (GUILayout.Button("Collect Data"))
        {
            rapidLib.ToggleCollectingData();
        }

        if (GUILayout.Button("Run"))
        {
            rapidLib.ToggleRunning();
        }
        if (GUILayout.Button("train"))
=======
		if (GUILayout.Button("collect data"))
		{
			rapidLib.ToggleRecording();
		}
		if (GUILayout.Button("train"))
>>>>>>> 1b13825aa829121056a8cb039cae3f001e4bc9ad
        {
            rapidLib.Train();
        }
    }
}