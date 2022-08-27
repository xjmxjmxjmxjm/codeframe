using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.UI;

[CustomEditor(typeof(CircleImage))]
public class CircleImageEditor : ImageEditor
{
    private SerializedProperty _fillPercent;
    private SerializedProperty _segement;
    private SerializedProperty _alpha;

    protected override void OnEnable()
    {
        base.OnEnable();
        _segement = serializedObject.FindProperty("segements");
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        
        serializedObject.Update();

        EditorGUILayout.PropertyField(_segement);
        
        serializedObject.ApplyModifiedProperties();

        if (GUI.changed)
        {
            EditorUtility.SetDirty(target);
        }
    }
}


public class CustomImageEditor : Editor
{
    public const int uilayer = 5;

    [MenuItem("GameObject/UI/CircleImage", priority = 0)]
    public static void AddImage()
    {
        Canvas canvas = GameObject.FindObjectOfType<Canvas>();
      
        Transform trans = CreateImage();
        if (Selection.activeGameObject != null && Selection.activeGameObject.layer == uilayer)
        {
            trans.SetParent(Selection.activeGameObject.transform);
        }
        else
        {
            trans.SetParent(canvas.transform);
        }
        trans.localScale = Vector3.one;
        trans.localPosition = Vector3.zero;
    }

    public static Transform CreateImage()
    {
        GameObject img = new GameObject("CircleImage");
        img.AddComponent<RectTransform>();
        img.AddComponent<CircleImage>();
        return img.transform;
    }
}
