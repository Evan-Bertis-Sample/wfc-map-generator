using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Collections.ObjectModel;
using System;

public class TextureRulesetBuilder : IRulesetBuilder<Color, Texture2D>
{
    public Ruleset<Color> BuildRuleset(Texture2D texture)
    {
        // Get all possible states and add them to the ruleset
        Ruleset<Color> rules = new Ruleset<Color>();
        List<Color> states = GetColors(texture);
        foreach(Color state in states) rules.AddState(state);

        Dictionary<Direction, Graph<Color, int>> neighborCountGraph = new Dictionary<Direction, Graph<Color, int>>();
        foreach (Direction dir in Enum.GetValues(typeof(Direction)))
        {
            neighborCountGraph[dir] = new Graph<Color, int>();
            foreach(Color state in states) neighborCountGraph[dir].AddVertex(state);
        }

        Dictionary<Color, int> stateCount = new Dictionary<Color, int>();

        // Count the connections from each pixel to the next pixel
        texture.ForeachCoord(coord =>
        {
            Color state = texture.GetPixel(coord.Item1, coord.Item2);
            state = GetClosestColorFromList(state, states);
            if (!states.Contains(state)) return;
            Dictionary<Direction, (int, int)> neighbors  = texture.GrabNeighborsByDirection(coord);

            if (stateCount.ContainsKey(state)) stateCount[state]++;
            else stateCount[state] = 1;

            foreach(var neighbor in neighbors)
            {
                Color neighborState = texture.GetPixel(neighbor.Value.Item1, neighbor.Value.Item2);
                neighborState = GetClosestColorFromList(neighborState, states);
                // Update graph with this connection
                int stateToNeighborCount = 0;
                neighborCountGraph[neighbor.Key].GetWeight(state, neighborState, ref stateToNeighborCount);
                stateToNeighborCount++;
                neighborCountGraph[neighbor.Key].AddEdge(state, neighborState, stateToNeighborCount);
            }
        });

        foreach(Direction dir in Enum.GetValues(typeof(Direction)))
        {
            // Build the ruleset
            foreach(var edge in neighborCountGraph[dir].Edges)
            {
                Color from = edge.Key;
                foreach (var fromToEdge in neighborCountGraph[dir].GetEdges(from))
                {
                    Color to = fromToEdge.Key;
                    rules.AddRule(dir, from, to, CalculateWeight(from, to, neighborCountGraph[dir]));
                }
            }
        }

        // Calculate ratios
        Dictionary<Color, float> ratios = new Dictionary<Color, float>();
        int numPixels = texture.width * texture.height;
        foreach (var count in stateCount)
        {
            ratios[count.Key] = (float)count.Value / numPixels;
        }
        rules.SetRatios(ratios);

        return rules;
    }

    private float CalculateWeight(Color from, Color to, Graph<Color, int> neighborGraph)
    {
        ReadOnlyDictionary<Color, int> edges = neighborGraph.GetEdges(from);
        if (edges == null) return 0; // No connections

        int numNeighbors = 0;
        foreach(var edge in edges)
        {
            numNeighbors += edge.Value;
        }

        if (numNeighbors == 0) throw new System.Exception("No connections found! Something went wrong..."); // This means the dictionary's edges are invalid for this purpose

        int fromToCount = 0;
        neighborGraph.GetWeight(from, to, ref fromToCount);

        return (float)fromToCount/numNeighbors;
    }

    private List<Color> GetColors(Texture2D texture)
    {
        List<Color> colors = new List<Color>();

        texture.Foreach(c => {
            if (!colors.Contains(c)) colors.Add(c);
        });

        return colors;
    }

    private Color GetClosestColorFromList(Color color, List<Color> reference)
    {
        float minDistance = Mathf.Infinity;
        Color closestColor = Color.white;

        foreach (Color refColor in reference)
        {
            Color dif = color - refColor;
            float distance = (dif.r * dif.r + dif.g * dif.g + dif.b * dif.b);

            if (distance < minDistance)
            {
                minDistance = distance;
                closestColor = refColor;
            }
        }

        return closestColor;
    }
}
