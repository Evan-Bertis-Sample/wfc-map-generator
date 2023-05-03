using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;
using System.Linq;
using System;

// A wrapper for the graph class to maintain the abstraction of a ruleset
// This is a wrapper for a Graph<State, float> that is not bidirectional
// Contains additional data, like the probability of each state, and directional information
[System.Serializable]
public class Ruleset<State>
{
    public Dictionary<State, float> Ratios {get; private set;}
    public int NumStates => _directionRules[Direction.North].NumVerts;
    public int NumRules => GetNumRules();


    private Dictionary<Direction, Graph<State, float>> _directionRules;

    public Ruleset()
    {
        _directionRules = new Dictionary<Direction, Graph<State, float>>();

        foreach(Direction direction in Enum.GetValues(typeof(Direction)))
        {
            _directionRules[direction] = new Graph<State, float>(false);
        }
    }

    public bool AddState(State state)
    {
        bool added = false;
        ForeachDirection(rule =>
        {
            bool result = rule.AddVertex(state);
            if (result == true) added = true;
        });

        return added;
        // return _graph.AddVertex(state);
    }

    public bool AddRule(Direction dir, State s1, State s2, float ratio)
    {
        return _directionRules[dir].AddEdge(s1, s2, ratio);
        // return _graph.AddEdge(s1, s2, ratio);
    }

    public ReadOnlyDictionary<State, float> GetRule(Direction dir, State cur)
    {
        return _directionRules[dir].GetEdges(cur);
    }

    public Graph<State, float> GetRule(Direction dir)
    {
        return _directionRules[dir];
    }

    public float GetRatio(Direction dir, State from, State to)
    {
        float weight = 0;
        _directionRules[dir].GetWeight(from, to, ref weight);
        return weight;
    }

    public List<State> GetStates()
    {
        return _directionRules[Direction.East].GetVertices();
    }

    public void SetRatios(Dictionary<State, float> ratios)
    {
        Ratios = new Dictionary<State, float>();
        List<State> states = GetStates();

        foreach(var ratio in ratios)
        {
            if (states.Contains(ratio.Key))
            {
                Ratios[ratio.Key] = ratio.Value;
            }
        }
    }

    public float GetProbability(State state)
    {
        if (Ratios.ContainsKey(state) == false) return 0;
        return Ratios[state];
    }


    private void ForeachDirection(Action<Graph<State, float>> function)
    {
        foreach(Direction dir in Enum.GetValues(typeof(Direction)))
        {
            function(_directionRules[dir]);
        }
    }

    private int GetNumRules()
    {
        int numRules = 0;

        foreach(Direction dir in Enum.GetValues(typeof(Direction)))
        {
            numRules += _directionRules[dir].NumEdges;
        }

        return numRules;
    }
}
