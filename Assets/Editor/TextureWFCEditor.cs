using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using System.Threading.Tasks;

public class TextureWFCEditor : EditorWindow {
    private bool _importRuleset;
    private string _dataPath;

    private Texture2D _reference;
    private Texture2D _startingState;
    private Texture2D _output;

    private bool _useStartingData;
    private List<Color> _collapsedColors = new List<Color>();
    private ListInspector<Color> _colorInspector;
    private Vector2Int _size;

    private Ruleset<Color> _rules;
    private TextureRulesetBuilder _rulsetBuilder;
    private RulesetInspector<Color> _ruleInspector;

    private Vector2 _propertiesScrollView = Vector2.zero;
    private Vector2 _texturesScrollView = Vector2.zero;

    private bool _isVisualizing = false;

    [MenuItem("Window/WFC")]
    private static void ShowWindow() {
        var window = GetWindow<TextureWFCEditor>();
        window.titleContent = new GUIContent("WFC");
        window.Show();
    }

    #region Inspector
    private void OnGUI() {
        DrawCenterLabel("Wave Function Collapse Editor", EditorStyles.whiteLargeLabel);

        EditorGUILayout.BeginHorizontal();

        _propertiesScrollView = EditorGUILayout.BeginScrollView(_propertiesScrollView);
        EditorGUILayout.BeginVertical(GUILayout.Width(EditorGUIUtility.currentViewWidth * 0.5f));
        DrawProperties();
        EditorGUILayout.EndVertical();
        EditorGUILayout.EndScrollView();

        GUILayout.FlexibleSpace();

        _texturesScrollView = EditorGUILayout.BeginScrollView(_texturesScrollView);
        EditorGUILayout.BeginVertical("Box", GUILayout.Width(EditorGUIUtility.currentViewWidth * 0.5f));
        DrawTextures();
        EditorGUILayout.EndVertical();
        EditorGUILayout.EndScrollView();

        EditorGUILayout.EndHorizontal();
    }

    private void DrawProperties()
    {
        EditorGUILayout.LabelField("Wave Function Configuration", EditorStyles.boldLabel);

        EditorGUILayout.Space(EditorGUIUtility.singleLineHeight);

        _importRuleset = GUILayout.Toggle(_importRuleset, "Import Ruleset");
        if (_importRuleset) DrawImportConfig();
        else DrawReferenceConfig();
        if (_rules != null) DrawRuleset();

        EditorGUILayout.Space(EditorGUIUtility.singleLineHeight);

        _useStartingData = GUILayout.Toggle(_useStartingData, "Use Starting State");

        EditorGUILayout.Space(EditorGUIUtility.singleLineHeight);
        if (_useStartingData) DrawStartDataConfig();
        else DrawStandardConfig();

        EditorGUILayout.Space(EditorGUIUtility.singleLineHeight);

        DrawBuildConfig();
    }

    private void DrawBuildConfig()
    {
        // Check that there is enough data to build the wave function
        if (_rules == null) return;
        if (_rules.NumStates == 0 || _rules.NumRules == 0) return;
        if (_useStartingData && _startingState == null) return;
        if (!_useStartingData && (_size.x <= 0 || _size.y <= 0)) return;

        EditorGUILayout.BeginVertical("Box");
        EditorGUILayout.LabelField("Build Configuration", EditorStyles.boldLabel);
        if (GUILayout.Button("Build Output")) BuildOutput();

        if (!_isVisualizing && GUILayout.Button("Visualize Construction")) BuildOutputWithoutStartingStateAsync();
        if (_isVisualizing && GUILayout.Button("Stop Visualization")) _isVisualizing = false;
        EditorGUILayout.EndVertical();

    }

    private void DrawImportConfig()
    {
        GUILayout.BeginHorizontal();
        EditorGUILayout.TextField("JSON Path", _dataPath);
        if (GUILayout.Button(new GUIContent(EditorGUIUtility.IconContent("Search On Icon")), GUILayout.Width(40f), GUILayout.MaxHeight(EditorGUIUtility.singleLineHeight)))
        {
            string startPath = (_dataPath == "") ? Application.dataPath : Directory.GetParent(_dataPath).FullName;
            _dataPath = EditorUtility.OpenFilePanelWithFilters("Select a Ruleset JSON", startPath, new string[] {"JSON", "json"});
        }
        GUILayout.EndHorizontal();
        
        if (_dataPath != "" && GUILayout.Button("Load Ruleset")) LoadRuleset();
    }

    private void DrawReferenceConfig()
    {
        _reference = (Texture2D)EditorGUILayout.ObjectField("Reference Data", _reference, typeof(Texture2D), false,  GUILayout.Height(EditorGUIUtility.singleLineHeight));

        if (_reference == null) return;

        if (!_reference.isReadable)
        {
            EditorGUILayout.HelpBox("Cannot build ruleset. Texture is set to readonly.", MessageType.Warning);
            return;
        }

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Build Ruleset")) BuildRuleset();
        if (GUILayout.Button("Save Ruleset")) SaveRuleset();
        EditorGUILayout.EndHorizontal();
    }

    private void DrawRuleset()
    {
        if (_ruleInspector == null) _ruleInspector = new RulesetInspector<Color>(C => 
            EditorGUILayout.ColorField(new GUIContent(), C, false, false, false, GUILayout.MaxWidth(20f))
        );
        _ruleInspector.Draw(_rules);

        if (_rules.NumStates > 0 && GUILayout.Button("Clear Ruleset"))
        {
            if (EditorUtility.DisplayDialog("Clear Ruleset?",
                                            "Are you sure you wish to clear the currently loaded rulset? If it is unsaved, it may take a while to regenerate.",
                                            "Yes, clear the ruleset.", "No, don't clear the ruleset"))
            {
                _rules = null;
            }
        }
    }

    private void SaveRuleset()
    {
        string absolute = EditorUtility.SaveFilePanelInProject("Select Ruleset JSON", "", "json", "Please select a JSON file containing a Color Ruleset");
        // string path = FileUtil.GetProjectRelativePath(absolute);
        RulesetSaver<Color> saver = new RulesetSaver<Color>();
        saver.Save(_rules, absolute);
    }

    private void LoadRuleset()
    {
        RulesetSaver<Color> loader = new RulesetSaver<Color>();
        _rules = loader.Load(_dataPath);
    }

    private void DrawStandardConfig()
    {
        _size = EditorGUILayout.Vector2IntField("Output Size", _size);
    }

    private void DrawStartDataConfig()
    {
        _startingState = (Texture2D)EditorGUILayout.ObjectField("Starting State", _startingState, typeof(Texture2D), false,  GUILayout.Height(EditorGUIUtility.singleLineHeight));

        if (_colorInspector == null)
        {
            _colorInspector = new ListInspector<Color>("Collapsed Colors", _collapsedColors,
                                                        C => EditorGUILayout.ColorField(new GUIContent(), C, true, false, false),
                                                        () => Color.white);
        }

        _colorInspector.Draw();
    }

    private void DrawTextures()
    {
        EditorGUILayout.LabelField("Visualization", EditorStyles.boldLabel);

        float sideLength = EditorGUIUtility.currentViewWidth * .2f;
        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        if (!_importRuleset)
        {
            DrawTexture("Reference", _reference, sideLength, sideLength);   
        }

        if (_useStartingData) 
        {
            if (_importRuleset) GUILayout.Space(10f);
            DrawTexture("Starting State", _startingState, sideLength, sideLength);
        }
        GUILayout.FlexibleSpace();
        EditorGUILayout.EndHorizontal();

        DrawTexture("Output", _output, sideLength * 2, sideLength * 2);
    }

    private void DrawTexture(string title, Texture2D tex, float width, float height)
    {
        EditorGUILayout.BeginVertical();
        EditorGUILayout.LabelField(title);
        if (tex == null)
        {
            EditorGUILayout.HelpBox($"Texture {title} not defined!", MessageType.Warning);
            EditorGUILayout.EndVertical();
            return;
        }
        
        GUI.color = Color.clear;
        GUILayout.Box("" ,GUILayout.Width(width), GUILayout.Height(height));

        GUI.color = Color.white;

        //GUILayout.BeginArea(position, tex);
        GUI.DrawTexture(GUILayoutUtility.GetLastRect(), tex, ScaleMode.StretchToFill, true, 0);
        // GUILayout.EndArea();

        GUILayout.Space(EditorGUIUtility.singleLineHeight);

        EditorGUILayout.EndVertical();
    }

    private void DrawCenterLabel(string text, GUIStyle style = null, params GUILayoutOption[] options)
    {
        EditorGUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        if (style != null) EditorGUILayout.LabelField(text, style, options);
        else EditorGUILayout.LabelField(text, options);
        GUILayout.FlexibleSpace();
        EditorGUILayout.EndHorizontal();
    }
    #endregion

    private void BuildRuleset()
    {
        if (_rulsetBuilder == null) _rulsetBuilder = new TextureRulesetBuilder();
        _rules = _rulsetBuilder.BuildRuleset(_reference);
    }

    private void BuildOutput()
    {
        if (!_useStartingData) BuildOutputWithoutStartingState();
        else BuildOutputFromStartingState();
    }

    private void BuildOutputWithoutStartingState()
    {
        WaveFunction<Color> waveFunction = new WaveFunction<Color>(_rules, _size.x, _size.y, 1);
        TextureWaveFunctionVisualizer visualizer = new TextureWaveFunctionVisualizer();
        waveFunction.Collapse();
        _output = visualizer.Visualize(waveFunction);
    }

    private void BuildOutputFromStartingState()
    {
        WaveFunctionBuilder<Color, Texture2D> wfBuilder = new WaveFunctionBuilder<Color, Texture2D>(_rules, _reference);
        TextureStateExtractor extractor = new TextureStateExtractor(_collapsedColors);
        WaveFunction<Color> waveFunction = wfBuilder.BuildWaveFunction(_reference.width, _reference.height, 1, extractor);
        TextureWaveFunctionVisualizer visualizer = new TextureWaveFunctionVisualizer();
        waveFunction.Collapse();
        _output = visualizer.Visualize(waveFunction);
    }

    private async Task BuildOutputWithoutStartingStateAsync()
    {
        WaveFunction<Color> waveFunction = new WaveFunction<Color>(_rules, _size.x, _size.y, 1);
        TextureWaveFunctionVisualizer visualizer = new TextureWaveFunctionVisualizer();
        _isVisualizing = true;
        while (waveFunction.IsCollapsed() == false)
        {
            _output = visualizer.Visualize(waveFunction);

            if (_isVisualizing == false) break;
            if (waveFunction.Iterate() == false) break;
            await Task.Delay(1);
            Repaint();
        }

        _isVisualizing = false;
        _output = visualizer.Visualize(waveFunction);        
    }
}

