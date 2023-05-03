using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace EditorUtilities
{
    public static class EditorShortcuts
    {
        public static void DrawCenterLabel(string text, GUIStyle style = null, params GUILayoutOption[] options)
        {
            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (style != null) EditorGUILayout.LabelField(text, style, options);
            else EditorGUILayout.LabelField(text, options);
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();
        }

        public static void DrawTexture(Texture2D reference, string title, float width, float height)
        {
            EditorGUILayout.BeginVertical();
            EditorGUILayout.LabelField(title);
            if (reference == null)
            {
                EditorGUILayout.HelpBox($"Texture {title} not defined!", MessageType.Warning);
                EditorGUILayout.EndVertical();
                return;
            }
            
            GUI.color = Color.clear;
            GUILayout.Box("" ,GUILayout.Width(width), GUILayout.Height(height));

            GUI.color = Color.white;

            //GUILayout.BeginArea(position, tex);
            GUI.DrawTexture(GUILayoutUtility.GetLastRect(), reference, ScaleMode.StretchToFill, true, 0);
            // GUILayout.EndArea();

            GUILayout.Space(EditorGUIUtility.singleLineHeight);

            EditorGUILayout.EndVertical();
        }

    }
}

