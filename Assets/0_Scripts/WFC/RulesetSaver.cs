using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;
using Newtonsoft.Json;

public class RulesetSaver<State> : ISaver<Ruleset<State>>
{
    [System.Serializable]
    public struct Rule<T>
    {
        public Direction Direction;
        public T From;
        public T To;
        public float Weight;

        public Rule(Direction dir, T from, T to, float weight)
        {
            Direction = dir;
            From = from;
            To = to;
            Weight = weight;
        }
    }

    [System.Serializable]
    public struct StateRatio<T>
    {
        public T State;
        public float Ratio;

        public StateRatio(T state, float ratio)
        {
            State = state;
            Ratio = ratio;
        }
    }

    [System.Serializable]
    public struct RulesetData<T>
    {
        public int NumRules;
        public List<Rule<T>> Rules;
        public List<StateRatio<T>> Ratios;

        public RulesetData(int numRules, Dictionary<T, float> ratios)
        {
            NumRules = numRules;
            Rules = new List<Rule<T>>();
            Ratios = new List<StateRatio<T>>();
            foreach(var ratio in ratios)
            {
                Ratios.Add(new StateRatio<T>(ratio.Key, ratio.Value));
            }
        }

        public void AddRule(Direction dir, T from, T to, float weight)
        {
            Rules.Add(new Rule<T>(dir, from, to, weight));
        }
    }

    public void Save(Ruleset<State> data, string path)
    {
        RulesetData<State> flattenedData = FlattenRuleset(data);
        string json = JsonUtility.ToJson(flattenedData, true);
        File.WriteAllText(path, json);
    }

    public Ruleset<State> Load(string path)
    {
       string json = File.ReadAllText(path);
       RulesetData<State> data = JsonUtility.FromJson<RulesetData<State>>(json);
       return BuildRuleset(data);
    }

    private RulesetData<State> FlattenRuleset(Ruleset<State> rules)
    {
        RulesetData<State> data = new RulesetData<State>(rules.NumRules, rules.Ratios);

        foreach (Direction dir in Enum.GetValues(typeof(Direction)))
        {
            foreach(State state in rules.GetStates())
            {
                foreach (var edge in rules.GetRule(dir, state))
                {
                    data.AddRule(dir, state, edge.Key, edge.Value);
                }
            }
        }

        return data;
    }

    private Ruleset<State> BuildRuleset(RulesetData<State> data)
    {
        Ruleset<State> ruleset = new Ruleset<State>();

        foreach (Rule<State> rule in data.Rules)
        {
            ruleset.AddState(rule.From);
            ruleset.AddState(rule.To);
            ruleset.AddRule(rule.Direction, rule.From, rule.To, rule.Weight);
        }

        Dictionary<State, float> ratios = new Dictionary<State, float>();
        foreach (StateRatio<State> ratio in data.Ratios)
        {
            ratios[ratio.State] = ratio.Ratio;
        }

        ruleset.SetRatios(ratios);
        return ruleset;
    }
}
