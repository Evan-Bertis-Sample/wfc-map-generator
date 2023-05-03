using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using EditorUtilities;

public class PatternWFCEditor : WFCEditor<Pattern<Color>, Texture2D, Texture2D>
{
    private int _patternSize;
    private Dictionary<Pattern<Color>, Texture2D> _memPreviewTextures = new Dictionary<Pattern<Color>, Texture2D>();

    [MenuItem("Window/WFC/Pattern")]
    private static void ShowWindow() {
        var window = GetWindow<PatternWFCEditor>();
        window.titleContent = new GUIContent("Pattern WFC");
        window.Show();
    }

    protected override void ExtendLeftPanel()
    {
        _patternSize = EditorGUILayout.DelayedIntField("Pattern Size",_patternSize);
    }

    protected override Pattern<Color> CreateNewState() => new Pattern<Color>(3);

    protected override IRulesetBuilder<Pattern<Color>, Texture2D> CreateRulesetBuilder() => new TexturePatternRulesetBuilder(_patternSize);

    protected override IWaveFunctionVisualizer<Pattern<Color>, Texture2D> CreateVisualizer() => new PatternWaveFunctionVisualizer(_patternSize);

    protected override Texture2D DrawReferenceTypeInput(Texture2D reference, string title = "")
    {
        Texture2D tex = (Texture2D)EditorGUILayout.ObjectField(title, reference, typeof(Texture2D), false, GUILayout.Height(EditorGUIUtility.singleLineHeight));
        if(tex != null && tex.isReadable == false) EditorGUILayout.HelpBox("Cannot build ruleset from texture. Set texture to read/write", MessageType.Warning);
        return tex;
    }

    protected override Pattern<Color> DrawStateType(Pattern<Color> state)
    {
        EditorGUILayout.HelpBox("Starting State is not Supported!", MessageType.Warning);
        return state;
    }

    protected override Pattern<Color> DrawStateTypeMini(Pattern<Color> state)
    {
        if (_memPreviewTextures.ContainsKey(state) == false) GenerateOutputTexture(state); // If not generated, create
        Texture2D output = _memPreviewTextures[state]; // Grab from dictionary

        //GUILayout.Label(output, GUILayout.MaxWidth(EditorGUIUtility.singleLineHeight), GUILayout.MaxHeight(EditorGUIUtility.singleLineHeight));
        EditorGUILayout.ObjectField(output, typeof(Texture2D), false, GUILayout.MaxWidth(EditorGUIUtility.singleLineHeight), GUILayout.MaxHeight(EditorGUIUtility.singleLineHeight));
        //EditorShortcuts.DrawTexture(output, "", 20f, EditorGUIUtility.singleLineHeight);
        // Debug.Log("Drawing pattern...");
        return state;
    }

    protected override void DrawVisualizationTypeGUI(Texture2D visualization, string title, float width, float height) => EditorShortcuts.DrawTexture(visualization, title, width, height);

    protected override Dictionary<Vector3Int, Pattern<Color>> ExtractState(Texture2D reference, List<Pattern<Color>> collapsed)
    {
        throw new System.NotImplementedException();
    }

    protected override void VisualizeReferenceTypeGUI(Texture2D reference, string title, float width, float height) => EditorShortcuts.DrawTexture(reference, title, width, height);

    private void GenerateOutputTexture(Pattern<Color> state)
    {
        Texture2D output = new Texture2D(state.Size, state.Size);
        for (int i = 0; i < state.Size; i++)
        {
            for (int j = 0; j < state.Size; j++)
            {
                state.GetContents(i, j, out Color contents);
                output.SetPixel(i, j, contents);
            }
        }
        output.filterMode = FilterMode.Point;
        output.Apply();

        // Save to cache
        _memPreviewTextures[state] = output;
    }
}
