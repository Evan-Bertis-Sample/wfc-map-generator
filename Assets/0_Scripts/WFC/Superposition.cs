using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using System.Linq;


public class Superposition<State>
{
    public HashSet<State> States {get; private set;}
    private State _collapsedState;

    public Superposition(State state)
    {
        States = new HashSet<State>(){state}; 
        _collapsedState = state;
    }
    public Superposition(List<State> states) => States = new HashSet<State>(states);
    public Superposition(Superposition<State> superposition)
    {
        // Copy constructor
        States = superposition.States;
        _collapsedState = superposition._collapsedState;
    }

    public bool Constrain(HashSet<State> valid)
    {
        if (valid.Count == 0) return false;
        int oldCount = States.Count;
        States = States.Where(s => valid.Contains(s)).ToHashSet();
        // States.RemoveItemsNotInList(valid);
        return States.Count != oldCount;
    }

    public SuperpositionStatus GetStatus()
    {
        switch (States.Count)
        {
            case 0:
                return SuperpositionStatus.Contradiction;
            case 1:
                if (_collapsedState != null) _collapsedState = States.First();
                return SuperpositionStatus.Collapsed;
            default:
                return SuperpositionStatus.Indeterminate;
        }
    }

    public float CalculateEntropy(Dictionary<State, float> stateEntropies)
    {
        if (States.Count == 1 || States.Count == 0) return 0;


        float entropy = 0;
        foreach(State s in States)
        {
            entropy += stateEntropies[s];
        }
        // Debug.Log(entropy);
        return entropy;
    }

    // Collapses the superposition given the state of the wavefunction
    public State Collapse(Ruleset<State> ruleset, Dictionary<Direction, State> collapsedNeighbors, Dictionary<State, float> probabilities)
    {
        switch (GetStatus())
        {
            case SuperpositionStatus.Collapsed:
                return States.First();
            case SuperpositionStatus.Contradiction:
                throw new System.Exception("Unable to collapse. Contradiction reached.");
            default:
                _collapsedState = CollapseToRandomState(ruleset, collapsedNeighbors, probabilities);
                // Set state to only one state
                States = new HashSet<State>(){_collapsedState};
                return _collapsedState;
        }
    }

    // Collapses the superposition given the state of the wavefunction
    public State Collapse(Ruleset<State> ruleset, Dictionary<Direction, Superposition<State>> neighbors, Dictionary<State, float> probabilities)
    {
        switch (GetStatus())
        {
            case SuperpositionStatus.Collapsed:
                return States.First();
            case SuperpositionStatus.Contradiction:
                throw new System.Exception("Unable to collapse. Contradiction reached.");
            default:
                _collapsedState = CollapseToRandomState(ruleset, neighbors, probabilities);
                // Set state to only one state
                States = new HashSet<State>(){_collapsedState};
                return _collapsedState;
        }
    }

    public State GetCollapsedState()
    {
        if (GetStatus() != SuperpositionStatus.Collapsed) throw new System.Exception("Cannot grab collapsed state! The superposition is not collapsed.");
        return _collapsedState;
    }

    private State CollapseToRandomState(Ruleset<State> ruleset, Dictionary<Direction, State> collapsedNeighbors, Dictionary<State, float> probabilities)
    {
        if (collapsedNeighbors == null) collapsedNeighbors = new Dictionary<Direction, State>();

        Dictionary<State, float> weights = new Dictionary<State, float>();

        // Calculate the weights of the state based on the collapsedNeighbors
        foreach (State s in States)
        {
            float weight = probabilities[s];

            foreach (var neighbor in collapsedNeighbors)
            {
                weight *= ruleset.GetRatio(Compass.GetOpposite(neighbor.Key), neighbor.Value, s);
            }

            weights[s] = weight;
        }

        // Choose a random state
        float totalWeight = 0;
        foreach (float weight in weights.Values) totalWeight += weight;

        float randomNumber = Random.Range(0f, totalWeight);
        foreach(KeyValuePair<State, float> possibility in weights)
        {
            randomNumber -= possibility.Value;
            if (randomNumber < 0) return possibility.Key;
        }
        // It should never reach this point but
        return States.First();
    }

    private State CollapseToRandomState(Ruleset<State> ruleset, Dictionary<Direction, Superposition<State>> neighbors, Dictionary<State, float> probabilities)
    {
        if (neighbors == null) neighbors = new Dictionary<Direction, Superposition<State>>();

        Dictionary<State, float> weights = new Dictionary<State, float>();

        // Calculate the weights of the state based on the collapsedNeighbors
        foreach (State s in States)
        {
            float weight = probabilities[s];

            foreach (var neighbor in neighbors)
            {
                foreach (State ns in neighbor.Value.States)
                {
                    weight *= ruleset.GetRatio(Compass.GetOpposite(neighbor.Key), ns, s);
                }
            }

            weights[s] = weight;
        }

        // Choose a random state
        float totalWeight = 0;
        foreach (float weight in weights.Values) totalWeight += weight;

        float randomNumber = Random.Range(0f, totalWeight);
        foreach(KeyValuePair<State, float> possibility in weights)
        {
            randomNumber -= possibility.Value;
            if (randomNumber < 0) return possibility.Key;
        }
        // It should never reach this point but
        return States.First();
    }
}
