   using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(RandomPathDrawerBehaviour))]
public class RandomPathDrawerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        RandomPathDrawerBehaviour drawer = target as RandomPathDrawerBehaviour;
        if (GUILayout.Button("Create Texture")) drawer.CreateTexture();
        if (drawer.Texture != null && GUILayout.Button("Save Texture"))
        {
            string path = EditorUtility.SaveFilePanel("Select Image Location", Application.dataPath, "Path", "png");
            drawer.Texture.Save(path);
            AssetDatabase.Refresh();
        }
    }
}
