using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
using System.Collections.ObjectModel;

#if UNITY_EDITOR
public class RulesetInspector<StateType>
{
    private Action<StateType> _stateDisplay;
    private bool _isOpen;
    private bool _showRatios;
    private bool _showEdges;
    private Dictionary<Direction, bool> _showDirectionRule;

    public RulesetInspector(Action<StateType> stateDisplay)
    {
        _stateDisplay = stateDisplay;
        _isOpen = false;
        _showDirectionRule = new Dictionary<Direction, bool>();
        foreach(Direction direction in Enum.GetValues(typeof(Direction))) _showDirectionRule[direction] = false;
    }

    public void Draw(Ruleset<StateType> rules, string name = "Ruleset")
    {
        // Popup
        if (rules == null) return;

        EditorGUILayout.BeginHorizontal();
        _isOpen = EditorGUILayout.Foldout(_isOpen, name);
        EditorGUILayout.EndHorizontal();

        if (!_isOpen) return;
        
        EditorGUILayout.BeginVertical("Box");
        
        EditorGUILayout.BeginHorizontal("Box");
        EditorGUILayout.IntField("States", rules.NumStates);
        EditorGUILayout.IntField("Rules", rules.NumRules);
        EditorGUILayout.EndHorizontal();

        _showRatios = EditorGUILayout.Foldout(_showRatios, "Ratios");

        if (_showRatios) ShowRatios(rules);

        _showEdges = EditorGUILayout.Foldout(_showEdges, "Rules");
        
        if (_showEdges) ShowEdges(rules);

        EditorGUILayout.EndVertical();
    }

    private void ShowRatios(Ruleset<StateType> rules)
    {
        if (rules.Ratios == null) return;
        foreach (var ratio in rules.Ratios)
        {
            DrawRatioInspector(ratio.Key, ratio.Value);
        }
    }

    private void ShowEdges(Ruleset<StateType> rules)
    {
        List<StateType> states = rules.GetStates();
        foreach (Direction dir in Enum.GetValues(typeof(Direction)))
        {
            _showDirectionRule[dir] = EditorGUILayout.Foldout(_showDirectionRule[dir], Enum.GetName(typeof(Direction), dir));

            if (_showDirectionRule[dir] == false) continue;

            foreach(StateType state in states)
            {
                ReadOnlyDictionary<StateType, float> stateEdges = rules.GetRule(dir, state);
                foreach(var edge in stateEdges)
                {
                    DrawEdgeInspector(state, edge.Key, edge.Value);
                }
            }
        }
    }

    private void DrawRatioInspector(StateType state, float ratio)
    {
        EditorGUILayout.BeginHorizontal("Box");
        EditorGUILayout.LabelField("State", GUILayout.MaxWidth(40f));
        _stateDisplay(state);
        GUILayout.FlexibleSpace();
        EditorGUILayout.FloatField("Ratio", ratio);
        EditorGUILayout.EndHorizontal();
    }
    private void DrawEdgeInspector(StateType from, StateType to, float weight)
    {
        EditorGUILayout.BeginHorizontal("Box");
        EditorGUILayout.LabelField("From", GUILayout.MaxWidth(30f));
        _stateDisplay(from);
        GUILayout.FlexibleSpace();
        EditorGUILayout.LabelField("To", GUILayout.MaxWidth(40f));
        _stateDisplay(to);
        GUILayout.FlexibleSpace();
        EditorGUILayout.FloatField("Weight", weight);
        EditorGUILayout.EndHorizontal();
    }
}
#endif
