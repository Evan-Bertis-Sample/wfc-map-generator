using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;
using System.Linq;

// Graph representation, can be bidirectional
[System.Serializable]
public class Graph<VertexType, WeightType>
{
    [SerializeField] public Dictionary<VertexType, Dictionary<VertexType, WeightType>> Edges {get; private set;}
    public bool Bidirectional {get; private set;}
    public int NumVerts => Edges.Count;
    public int NumEdges;

    public Graph(bool bidirectional = false)
    {
        Edges = new Dictionary<VertexType, Dictionary<VertexType, WeightType>>();
        NumEdges = 0;
        Bidirectional = bidirectional;
    }

    public bool AddVertex(VertexType state)
    {
        if (Edges.ContainsKey(state)) return false; // State already exists!
        Edges[state] = new Dictionary<VertexType, WeightType>();
        return true;
    }

    public bool AddEdge(VertexType s1, VertexType s2, WeightType ratio)
    {
        if (!(Edges.ContainsKey(s1) && Edges.ContainsKey(s2))) return false; // One or more states do not exist
        if (Edges[s1].ContainsKey(s2) == false) NumEdges++; // Only add to rule count when this is a novel rule, and not overwriting
        // Add edges
        Edges[s1][s2] = ratio;
        if (Bidirectional) Edges[s2][s1] = ratio;
        return true;
    }

    public ReadOnlyDictionary<VertexType, WeightType> GetEdges(VertexType cur)
    {
        if (!Edges.ContainsKey(cur)) return null; // Rule does not exist
        else return new ReadOnlyDictionary<VertexType, WeightType>(Edges[cur]);
    }

    public bool GetWeight(VertexType from, VertexType to, ref WeightType weight)
    {
        if (!(Edges.ContainsKey(from) && Edges.ContainsKey(to))) return false;
        if (!Edges[from].ContainsKey(to)) return false;
        weight = Edges[from][to];
        return true;
    }

    public List<VertexType> GetVertices()
    {
        return Edges.Keys.ToList();
    }
}
