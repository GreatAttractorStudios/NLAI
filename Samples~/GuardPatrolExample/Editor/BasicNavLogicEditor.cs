using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(BasicNavLogic))]
public class BasicNavLogicEditor : Editor
{
    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        // Get properties
        var nameProp = serializedObject.FindProperty("_name");
        var targetProp = serializedObject.FindProperty("target");
        var targetPositionProp = serializedObject.FindProperty("targetPosition");
        var returnSuccessImmediatelyProp = serializedObject.FindProperty("returnSuccessImmediately");

        // Draw the main properties
        EditorGUILayout.PropertyField(nameProp);
        EditorGUILayout.PropertyField(targetProp);

        // Only show the targetPosition field if the target transform is not set
        if (targetProp.objectReferenceValue == null)
        {
            EditorGUILayout.PropertyField(targetPositionProp);
        }

        EditorGUILayout.PropertyField(returnSuccessImmediatelyProp);

        serializedObject.ApplyModifiedProperties();
    }
} 