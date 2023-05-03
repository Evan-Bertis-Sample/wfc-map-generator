using System.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using EditorUtilities;
using System.IO;

public abstract class WFCEditor<StateType, ReferenceType, VisualizationType> : EditorWindow
{
    // Ruleset Config
    private bool _importRuleset;
    private string _dataPath;
    private Ruleset<StateType> _rules;

    // Reference Config
    private ReferenceType _reference;
    private ReferenceType _startingState;
    private VisualizationType _output;
    private bool _useStartingData;
    private List<StateType> _collapsedStates = new List<StateType>();

    // Output Config
    private Vector3Int _outputSize;
    private bool _isVisualizing = false;

    // Cache
    private ListInspector<StateType> _collapsedStateEditor;
    private RulesetInspector<StateType> _ruleInspector;
    private Vector2 _propertiesScrollView = Vector2.zero;
    private Vector2 _texturesScrollView = Vector2.zero;

    protected abstract ReferenceType DrawReferenceTypeInput(ReferenceType reference, string title = "");
    protected abstract void VisualizeReferenceTypeGUI(ReferenceType reference, string title, float width, float height);
    protected abstract void DrawVisualizationTypeGUI(VisualizationType visualization, string title, float width, float height);
    protected abstract StateType DrawStateType(StateType state);
    protected abstract StateType DrawStateTypeMini(StateType state);
    protected abstract StateType CreateNewState();
    protected abstract IRulesetBuilder<StateType, ReferenceType> CreateRulesetBuilder();
    protected abstract IWaveFunctionVisualizer<StateType, VisualizationType> CreateVisualizer();
    protected abstract Dictionary<Vector3Int, StateType> ExtractState(ReferenceType reference, List<StateType> collapsed);

    protected virtual void ExtendLeftPanel() {}
    protected virtual void ExtendRightPanel() {}

    private void OnGUI() {
        EditorShortcuts.DrawCenterLabel("Wave Function Collapse Editor", EditorStyles.whiteLargeLabel);

        EditorGUILayout.BeginHorizontal();

        _propertiesScrollView = EditorGUILayout.BeginScrollView(_propertiesScrollView);
        EditorGUILayout.BeginVertical(GUILayout.Width(EditorGUIUtility.currentViewWidth * 0.5f));
        DrawProperties();
        ExtendLeftPanel();
        EditorGUILayout.EndVertical();
        EditorGUILayout.EndScrollView();

        GUILayout.FlexibleSpace();

        _texturesScrollView = EditorGUILayout.BeginScrollView(_texturesScrollView);
        EditorGUILayout.BeginVertical("Box", GUILayout.Width(EditorGUIUtility.currentViewWidth * 0.5f));
        DrawStateConfig();
        ExtendRightPanel();
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
        if (!_useStartingData && (_outputSize.x <= 0 || _outputSize.y <= 0) || _outputSize.z <= 0) return;

        EditorGUILayout.BeginVertical("Box");
        EditorGUILayout.LabelField("Build Configuration", EditorStyles.boldLabel);
        if (GUILayout.Button("Build Output")) BuildOutput();

        if (!_isVisualizing && GUILayout.Button("Visualize Construction")) BuildOutputWithoutStartingStateAsync();
        if (_isVisualizing && GUILayout.Button("Stop Visualization")) _isVisualizing = false;
        EditorGUILayout.EndVertical();

    }

    private void DrawStateConfig()
    {
        EditorGUILayout.LabelField("Visualization", EditorStyles.boldLabel);

        float sideLength = EditorGUIUtility.currentViewWidth * .2f;
        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        if (!_importRuleset)
        {
            VisualizeReferenceTypeGUI(_reference, "Reference", sideLength, sideLength);
        }

        if (_useStartingData) 
        {
            if (_importRuleset) GUILayout.Space(10f);
            VisualizeReferenceTypeGUI(_startingState, "Starting State", sideLength, sideLength);
        }
        GUILayout.FlexibleSpace();
        EditorGUILayout.EndHorizontal();

        DrawVisualizationTypeGUI(_output, "Output" ,sideLength * 2, sideLength * 2);
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

    private void DrawRuleset()
    {
        if (_ruleInspector == null) _ruleInspector = new RulesetInspector<StateType>(S => DrawStateTypeMini(S));
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

    private void DrawStandardConfig()
    {
        _outputSize = EditorGUILayout.Vector3IntField("Output Size", _outputSize);
    }

    private void DrawReferenceConfig()
    {
        _reference = DrawReferenceTypeInput(_reference, "Reference");

        if (_reference == null) return;
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Build Ruleset")) BuildRuleset();
        if (GUILayout.Button("Save Ruleset")) SaveRuleset();
        EditorGUILayout.EndHorizontal();
    }

    private void DrawStartDataConfig()
    {
        _startingState = DrawReferenceTypeInput(_startingState, "Starting State");

        if (_collapsedStateEditor == null)
        {
            _collapsedStateEditor = new ListInspector<StateType>("Collapsed States", _collapsedStates,
                                                               DrawStateType,
                                                               CreateNewState);
        }

        _collapsedStateEditor.Draw();
    }

    #region Utilities
    private void SaveRuleset()
    {
        string absolute = EditorUtility.SaveFilePanelInProject("Select Ruleset JSON", "", "json", $"Please select a JSON file containing a {typeof(StateType).Name} Ruleset");
        // string path = FileUtil.GetProjectRelativePath(absolute);
        RulesetSaver<StateType> saver = new RulesetSaver<StateType>();
        saver.Save(_rules, absolute);
    }

    private void LoadRuleset()
    {
        RulesetSaver<StateType> loader = new RulesetSaver<StateType>();
        _rules = loader.Load(_dataPath);
    }
    #endregion

    #region Building
    private void BuildRuleset()
    {
        _rules = CreateRulesetBuilder().BuildRuleset(_reference);
    }
        private void BuildOutput()
    {
        if (!_useStartingData) BuildOutputWithoutStartingState();
        else BuildOutputFromStartingState();
    }

    private void BuildOutputWithoutStartingState()
    {
        WaveFunction<StateType> waveFunction = new WaveFunction<StateType>(_rules, _outputSize.x, _outputSize.y, _outputSize.z);
        IWaveFunctionVisualizer<StateType, VisualizationType> visualizer = CreateVisualizer();
        waveFunction.Collapse();
        _output = visualizer.Visualize(waveFunction);
    }

    private void BuildOutputFromStartingState()
    {
        WaveFunctionBuilder<StateType, ReferenceType> wfBuilder = new WaveFunctionBuilder<StateType, ReferenceType>(_rules, _reference);
        WaveFunction<StateType> waveFunction = new WaveFunction<StateType>(_rules, _outputSize.x, _outputSize.y, _outputSize.z);
        waveFunction.SetStartingState(ExtractState(_reference, _collapsedStates));
        IWaveFunctionVisualizer<StateType, VisualizationType> visualizer = CreateVisualizer();
        waveFunction.Collapse();
        _output = visualizer.Visualize(waveFunction);
    }

    private async Task BuildOutputWithoutStartingStateAsync()
    {
        WaveFunction<StateType> waveFunction = new WaveFunction<StateType>(_rules, _outputSize.x, _outputSize.y, _outputSize.z);
        IWaveFunctionVisualizer<StateType, VisualizationType> visualizer = CreateVisualizer();
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

    #endregion
}
