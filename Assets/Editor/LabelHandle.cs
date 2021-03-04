using UnityEngine;
using System.Collections;
using UnityEditor;

// Create a 180 degrees wire arc with a ScaleValueHandle attached to the disc
// lets you visualize some info of the transform

[CustomEditor(typeof(ShowNameInEditor))]
class LabelHandle : Editor
{
    void OnSceneGUI()
    {
        ShowNameInEditor handleExample = (ShowNameInEditor)target;
        if (handleExample == null)
        {
            return;
        }

        Handles.color = Color.black;
        Handles.Label(handleExample.transform.position + Vector3.up * 2,
            handleExample.transform.position.ToString() + "\nName: " +
            handleExample.objectName.ToString());

        Handles.BeginGUI();
        if (GUILayout.Button("Reset Area", GUILayout.Width(100)))
        {
            handleExample.objectName = "Object";
        }
        Handles.EndGUI();

    }
}