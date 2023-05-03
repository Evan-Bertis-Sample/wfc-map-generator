using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using System.Collections.ObjectModel;
using UnityEngine.TestTools;

public class Ruleset_Tests
{
    [Test]
    public void Ruleset_Construction()
    {
        Ruleset<Color> rules = new Ruleset<Color>();
        Assert.AreEqual(rules.NumRules, 0);
        Assert.AreEqual(rules.NumStates, 0);
    }

    [Test]
    public void Ruleset_AddState()
    {
        Ruleset<float> rules = new Ruleset<float>();
        Assert.AreEqual(rules.AddState(0.1f), true);
        Assert.AreEqual(rules.AddState(0.2f), true);
        Assert.AreEqual(rules.AddState(0.3f), true);
        Assert.AreEqual(rules.AddState(0.4f), true);
        Assert.AreEqual(rules.NumStates, 4);

        Assert.AreEqual(rules.AddState(0.1f), false);
        Assert.AreEqual(rules.NumStates, 4);
    }

    [Test]
    public void Ruleset_AddRule()
    {
        Ruleset<float> rules = new Ruleset<float>();

        for (float i = 0f; i <= 1f; i += 0.1f)
        {
            Assert.AreEqual(rules.AddState(i), true);
        }

        Assert.AreEqual(rules.NumStates, 10);

        Assert.AreEqual(rules.AddRule(Direction.North, 0.1f, 0.2f, 2.2f), true);
        Assert.AreEqual(rules.AddRule(Direction.North, 0.01f, 0.2f, 2.5f), false);
        Assert.AreEqual(rules.AddRule(Direction.North, 0.1f, 0.25f, 2.5f), false);
        Assert.AreEqual(rules.AddRule(Direction.North, .15f, 0.25f, 2.5f), false);

        Assert.AreEqual(rules.NumRules, 1);

        ReadOnlyDictionary<float, float> rule1 = rules.GetRule(Direction.North, 0.1f);
        Assert.AreNotEqual(rule1, null);
        Assert.AreEqual(rule1[0.2f], 2.2f);

        rules.AddRule(Direction.North, .1f, 0.2f, 3f);
        Assert.AreEqual(rules.GetRatio(Direction.North, 0.1f, 0.2f), 3f);
    }
}