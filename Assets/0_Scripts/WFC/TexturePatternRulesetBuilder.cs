using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;

public class TexturePatternRulesetBuilder : IRulesetBuilder<Pattern<Color>, Texture2D>
{
    private int _patternSize;

    public TexturePatternRulesetBuilder(int patternSize) => _patternSize = patternSize;

    public Ruleset<Pattern<Color>> BuildRuleset(Texture2D reference)
    {
        Ruleset<Pattern<Color>> rules = new Ruleset<Pattern<Color>>();
        HashSet<Pattern<Color>> patternHash = GeneratePatterns(reference);
        patternHash = GenerateOrientations(patternHash);

        List<Pattern<Color>> patterns = patternHash.ToList();

        foreach(Pattern<Color> pattern in patterns) rules.AddState(pattern);

        Dictionary<Pattern<Color>, float> patternRatios = new Dictionary<Pattern<Color>, float>();
        foreach(Pattern<Color> pattern in patterns) patternRatios[pattern] = 1f;
        rules.SetRatios(patternRatios);

        Dictionary<Direction, Graph<Pattern<Color>, float>> edges = GenerateEdges(patterns);
        foreach(var graphsByDirection in edges)
        {
            foreach(var edge in graphsByDirection.Value.Edges)
            {
                foreach(var to in edge.Value)
                {
                    rules.AddRule(graphsByDirection.Key, edge.Key, to.Key, to.Value);
                }
            }
        }
        return rules;
    }

    private HashSet<Pattern<Color>> GeneratePatterns(Texture2D reference)
    {
        HashSet<Pattern<Color>> patterns = new HashSet<Pattern<Color>>();
        for(int x = 0; x < reference.width; x++)
        {
            for(int y = 0; y < reference.height; y++)
            {
                //if (reference.InBounds((x + _patternSize - 1, y + _patternSize - 1)) == false) continue;

                // This pixel is the top left of the kernel
                Pattern<Color> pattern = new Pattern<Color>(_patternSize);
                for(int i = 0; i < _patternSize; i++)
                {
                    for(int j = 0; j < _patternSize; j++)
                    {
                        Color pixel = reference.GetPixel((x + i) % (reference.width), (y + j) % (reference.height));
                        pattern.SetContents(i, j, pixel);
                    }
                }

                patterns.Add(pattern);
            }
        }

        return patterns;
    }

    private HashSet<Pattern<Color>> GenerateOrientations(HashSet<Pattern<Color>> baseSet)
    {
        HashSet<Pattern<Color>> newSet = new HashSet<Pattern<Color>>();
        foreach(Pattern<Color> basePattern in baseSet)
        {     
            bool added = newSet.Add(basePattern);
            if (added == false) continue; // this was a permutation of another pattern, no need to recalculate
            for (int i = 1; i < 4; i++)
            {
                newSet.Add(basePattern.Rotate(i));
            }
        }

        return newSet;
    }

    private Dictionary<Direction, Graph<Pattern<Color>, float>> GenerateEdges(List<Pattern<Color>> states)
    {
        Dictionary<Direction, Graph<Pattern<Color>, float>> graphsByDirection = new Dictionary<Direction, Graph<Pattern<Color>, float>>();
        foreach(Direction dir in Enum.GetValues(typeof(Direction)))
        {
            Graph<Pattern<Color>, float> ruleGraph = new Graph<Pattern<Color>, float>();
            // Add states
            foreach(Pattern<Color> p in states) ruleGraph.AddVertex(p);

            foreach(Pattern<Color> from in states)
            {
                foreach(Pattern<Color> to in states)
                {
                    if (HasCompatiableEdges(from, to, dir)) ruleGraph.AddEdge(from, to, 1f);
                }
            }

            graphsByDirection[dir] = ruleGraph;
        }
        return graphsByDirection;
    }

    private bool HasCompatiableEdges(Pattern<Color> from, Pattern<Color> to, Direction dir)
    {
        List<Color> fromEdge = from.GetEdge(dir).ToList();
        List<Color> toEdge = to.GetEdge(Compass.GetOpposite(dir)).ToList();
        toEdge.Reverse(); // Account for orientation of edges

        for(int i = 0; i < fromEdge.Count; i++)
        {
            if (fromEdge[i] != toEdge[i]) return false;
        }

        return true;
    }
}
