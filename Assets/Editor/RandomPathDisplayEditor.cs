using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(RandomPathTester))]
public class RandomPathTestEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        RandomPathTester rd = target as RandomPathTester;
        if (GUILayout.Button("Generate Path")) rd.GeneratePath();
    }
}
