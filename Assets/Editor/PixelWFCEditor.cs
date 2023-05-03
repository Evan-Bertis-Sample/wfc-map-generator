using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using EditorUtilities;

public class PixelWFCEditor : WFCEditor<Color, Texture2D, Texture2D>
{
    [MenuItem("Window/WFC/Pixel")]
    private static void ShowWindow() {
        var window = GetWindow<PixelWFCEditor>();
        window.titleContent = new GUIContent("Pixel WFC");
        window.Show();
    }

    protected override Color CreateNewState() => Color.white;

    protected override IRulesetBuilder<Color, Texture2D> CreateRulesetBuilder() => new TextureRulesetBuilder();

    protected override IWaveFunctionVisualizer<Color, Texture2D> CreateVisualizer() => new TextureWaveFunctionVisualizer();

    protected override Texture2D DrawReferenceTypeInput(Texture2D reference, string title = "")
    {
        Texture2D tex = (Texture2D)EditorGUILayout.ObjectField(title, reference, typeof(Texture2D), false, GUILayout.Height(EditorGUIUtility.singleLineHeight));
        if(tex != null && tex.isReadable == false) EditorGUILayout.HelpBox("Cannot build ruleset from texture. Set texture to read/write", MessageType.Warning);
        return tex;
    }

    protected override Color DrawStateType(Color state) => EditorGUILayout.ColorField(new GUIContent(), state, true, false, false);

    protected override Color DrawStateTypeMini(Color state) => EditorGUILayout.ColorField(new GUIContent(), state, false, false, false, GUILayout.MaxWidth(20f));

    protected override void DrawVisualizationTypeGUI(Texture2D visualization, string title, float width, float height) => EditorShortcuts.DrawTexture(visualization, title, width, height);

    protected override Dictionary<Vector3Int, Color> ExtractState(Texture2D reference, List<Color> collapsed) => new TextureStateExtractor(collapsed).ExtractState(reference);

    protected override void VisualizeReferenceTypeGUI(Texture2D reference, string title, float width, float height) => EditorShortcuts.DrawTexture(reference, title, width, height);
}
