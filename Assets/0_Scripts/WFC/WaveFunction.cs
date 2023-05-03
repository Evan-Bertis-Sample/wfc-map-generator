using System.Collections.ObjectModel;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
using System.Collections.Concurrent;
using System.Threading.Tasks;

public class WaveFunction<State>
{
    protected struct WaveFunctionState
    {
        public Dictionary<Vector3Int, Superposition<State>> StateParticles;
        public List<Vector3Int> ParticlePositionsByEntropy;
        public Dictionary<State, int> StateCount;
        public Dictionary<State, float> StateProbability;
        public Dictionary<State, float> StateEntropy;
        public int NumCollapsed;
        
        public WaveFunctionState(WaveFunction<State> wf, List<Vector3Int> collapsableParticles)
        {
            // Copy state of superposition
            StateParticles = new Dictionary<Vector3Int, Superposition<State>>();
            foreach(var particle in wf.Particles)
            {
                StateParticles[particle.Key] = new Superposition<State>(particle.Value);
            }

            ParticlePositionsByEntropy = collapsableParticles;
            StateCount = new Dictionary<State, int>(wf._stateCount);
            StateProbability = new Dictionary<State, float>(wf._stateProbability);
            StateEntropy = new Dictionary<State, float>(wf._stateEntropy);
            NumCollapsed = wf._numCollapsed;
        }
    }

    public Ruleset<State> Rules {get; private set;}
    public Dictionary<Vector3Int, Superposition<State>> Particles {get; private set;} = new Dictionary<Vector3Int, Superposition<State>>();
    public Vector3Int Bounds {get; private set;}
    private Stack<WaveFunctionState> _checkpoints = new Stack<WaveFunctionState>();
    private Dictionary<State, int> _stateCount = new Dictionary<State, int>();
    private Dictionary<State, float> _stateProbability = new Dictionary<State, float>();
    private Dictionary<State, float> _stateEntropy = new Dictionary<State, float>();
    private List<Vector3Int> _particlePositionsByEntropy = new List<Vector3Int>();
    private int _numCollapsed;

    public WaveFunction(Ruleset<State> rules, int width, int height, int depth)
    {
        Rules = rules;
        Bounds = new Vector3Int(width, height, depth);
        _numCollapsed = 0;
        List<State> allStates = rules.GetStates();
        ForeachPosition(p => 
        {
            Superposition<State> particle = new Superposition<State>(allStates);
            Particles[p] = particle;
            _particlePositionsByEntropy.Add(p);
        });
        _stateCount = new Dictionary<State, int>();
        _stateProbability = new Dictionary<State, float>();

        foreach(State state in rules.GetStates()) 
        {
            _stateCount[state] = 0; 
            float probability = Rules.GetProbability(state);
            _stateProbability[state] = probability;
            _stateEntropy[state] = probability * Mathf.Log(probability);
        }

        _particlePositionsByEntropy.Shuffle();
    }

    public void SetStartingState(Dictionary<Vector3Int, State> states)
    {
        foreach(var state in states)
        {
            if (!InBounds(state.Key)) continue;

            Particles[state.Key] = new Superposition<State>(state.Value);
            _stateCount[state.Value] += 1;
            _numCollapsed++;
        }
    }

    public bool IsCollapsed()
    {
        return _particlePositionsByEntropy.Count == 0;
    }

    public float GetStateProbability(State state)
    {
        if (Rules.GetStates().Contains(state) == false) return 0;
        return _stateProbability[state];
    }
    
    public float GetStateEntropy(State state)
    {
        if (Rules.GetStates().Contains(state) == false) return 0;
        return _stateEntropy[state];
    }
    public void Collapse()
    {
        while (IsCollapsed() == false)
        {
            if (Iterate() == false) break;
        }

        Debug.Log($"Collapsed Wave Function. Created {_checkpoints.Count} Checkpoints in the process");
    }

    public bool Iterate()
    {
        Vector3Int particlePosition = GetLowestEntropyPosition();
        Superposition<State> particle = Particles[particlePosition];
        // Debug.Log(particle.States.Count);

        // If there are multiple options, save a checkpoint
        if (_particlePositionsByEntropy.Count > 2 || _particlePositionsByEntropy.Count < 10) SaveCheckpoint(particlePosition);

        if (particle.GetStatus() == SuperpositionStatus.Contradiction)
        {
            bool handled = HandleContradiction();
            if (!handled) return false; // Couldn't restore wave function --> unsolvable
            else return true; // Could restore wave function --> try again
        }

        // Collapse the superposition
        particle.Collapse(Rules, GetCollapsedNeighborsStateByDirection(particlePosition), _stateProbability);
        HandleCollapse(particle, particlePosition);

        SuperpositionStatus status = Propogate(particlePosition);
        // Handle contradictions
        if (status == SuperpositionStatus.Contradiction)
        {
            bool handled = HandleContradiction();
            if (!handled) return false; // Couldn't restore wave function --> unsolvable
            else return true; // Could restore wave function --> try again
        }

        return true;
    }

    private SuperpositionStatus Propogate(Vector3Int position)
    {
        Stack<Vector3Int> stack = new Stack<Vector3Int>();
        stack.Push(position);
        // Recalculate the probabilities for the next iteration
        while (stack.Count > 0)
        {
            Vector3Int currentPos = stack.Pop();
            Superposition<State> currentParticle = Particles[currentPos];
            Dictionary<Direction, Vector3Int> adjacent = GrabAdjacentByDirections(currentPos);

            foreach (var neighborDirPosPair in adjacent)
            {
                Superposition<State> neighbor = Particles[neighborDirPosPair.Value];
                bool narrowedSuperpositon = false;
                // Can speed up
                // * Single Threaded Approach
                // HashSet<State> validStates = new HashSet<State>();
                // foreach (State s in currentParticle.States)
                // {
                //     ReadOnlyDictionary<State, float>.KeyCollection rule = Rules.GetRule(neighborDirPosPair.Key, s).Keys;
                //     List<State> ruleList = rule.ToList();
                //     foreach(State r in ruleList) validStates.Add(r);
                // }
                // narrowedSuperpositon = neighbor.Constrain(validStates);

                // * Multithreaded Approach
                // int numThreads = Mathf.Min(Environment.ProcessorCount, currentParticle.States.Count);
                // int batchsize = currentParticle.States.Count / numThreads;
                // ConcurrentDictionary<State, byte> validStates = new ConcurrentDictionary<State, byte>(); // There is no concurrent hashset, so we improvise
                // List<Task> tasks = new List<Task>();  
                
                // for (int i = 0; i < numThreads; i++)
                // {
                //     int startIndex = i * batchsize;
                //     int endIndex = (i == numThreads - 1) ? currentParticle.States.Count : (i + 1) * batchsize;
                //     // Debug.Log($"Created thread {i} -- Domain [{startIndex}, {endIndex}]");
                //     Task task = Task.Run(() =>
                //     {
                //         List<State> states = currentParticle.States.ToList().GetRange(startIndex, endIndex - startIndex);
                //         foreach(State state in states)
                //         {
                //             ReadOnlyDictionary<State, float>.KeyCollection rule = Rules.GetRule(neighborDirPosPair.Key, state).Keys;
                //             List<State> ruleList = rule.ToList();
                //             foreach (State r in ruleList)
                //             {
                //                 validStates[r] = 1;
                //             }
                //         }
                //     });

                //     tasks.Add(task);
                // }
                // Task.WaitAll(tasks.ToArray());
                // narrowedSuperpositon = neighbor.Constrain(validStates.Keys.ToHashSet());

                // * Parallel Approach
                int numThreads = Mathf.Min(Environment.ProcessorCount, currentParticle.States.Count);
                int batchsize = currentParticle.States.Count / numThreads;
                ConcurrentDictionary<State, byte> validStates = new ConcurrentDictionary<State, byte>(); // There is no concurrent hashset, so we improvise
                List<Action> tasks = new List<Action>();  
                
                for (int i = 0; i < numThreads; i++)
                {
                    int startIndex = i * batchsize;
                    int endIndex = (i == numThreads - 1) ? currentParticle.States.Count : (i + 1) * batchsize;
                    // Debug.Log($"Created thread {i} -- Domain [{startIndex}, {endIndex}]");
                    tasks.Add(() =>
                    {
                        List<State> states = currentParticle.States.ToList().GetRange(startIndex, endIndex - startIndex);
                        foreach(State state in states)
                        {
                            ReadOnlyDictionary<State, float>.KeyCollection rule = Rules.GetRule(neighborDirPosPair.Key, state).Keys;
                            List<State> ruleList = rule.ToList();
                            foreach (State r in ruleList)
                            {
                                validStates[r] = 1;
                            }
                        }
                    });
                }
                Parallel.Invoke(tasks.ToArray());
                narrowedSuperpositon = neighbor.Constrain(validStates.Keys.ToHashSet());

                
                switch (neighbor.GetStatus())
                {
                    case SuperpositionStatus.Indeterminate:
                        break;
                    case SuperpositionStatus.Collapsed:
                        HandleCollapse(neighbor, neighborDirPosPair.Value);
                        break;
                    case SuperpositionStatus.Contradiction:
                        return SuperpositionStatus.Contradiction;
                }

                if (narrowedSuperpositon == true && !stack.Contains(neighborDirPosPair.Value))
                {
                    stack.Push(neighborDirPosPair.Value);
                    // Debug.Log($"Adding {neighborDirPosPair.Value} to stack");
                }
            }
            // Debug.Log($"Propogating -- Stack Count:{stack.Count}");
        }
        
        SortEntropy();

        return SuperpositionStatus.Indeterminate;
    }

    private void SaveCheckpoint(Vector3Int chosenParticlePosition)
    {
        List<Vector3Int> particles = new List<Vector3Int>(_particlePositionsByEntropy);
        particles.Add(particles[0]);
        particles.RemoveAt(0);
        _checkpoints.Push(new WaveFunctionState(this, particles));
        // Debug.Log("saved a checkpoint");
    }

    // Handles a contradiction by restoring state
    // If the wave function can be restored, return true, otherwise, return false
    private bool HandleContradiction()
    {
        // Debug.Log("Reached Contradiction!");
        if (_checkpoints.Count == 0)
        {
            Debug.Log("Could not collapse the Wave Function"); // Worst case scenario
            return false;
        }

        RestoreState(_checkpoints.Pop());
        return true;
    }

    private void HandleCollapse(Superposition<State> particle, Vector3Int position)
    {
        // Update state of wave function
        State collapsedState = particle.GetCollapsedState();
        _stateCount[collapsedState]++;
        _numCollapsed++;
        _particlePositionsByEntropy.Remove(position);
        CalculateProbabilities();
        //_stateCount.Print();
    }

    #region Utility

    private void CalculateProbabilities()
    {
        // * Sinlge-threaded Approach
        // foreach (State s in Rules.GetStates())
        // {
        //     float currentRatio = (float)_stateCount[s] / _numCollapsed;
        //     float desiredRatio = Rules.GetProbability(s);

        //     float proability = Mathf.Clamp(1f - (currentRatio / desiredRatio), 0f, 1f);
        //     _stateProbability[s] = proability;

        //     float entropy;
        //     if (proability == 0) entropy = 0f;
        //     else entropy = proability * Mathf.Log(proability, 2);

        //     _stateEntropy[s] = entropy;
        // }

        // * Multithreaded Approach
        List<State> states = Rules.GetStates();

        // Divide the states into smaller chunks
        int chunkSize = states.Count / Environment.ProcessorCount;
        var chunks = Enumerable.Range(0, Environment.ProcessorCount)
                            .Select(i => states.Skip(i * chunkSize).Take(chunkSize))
                            .Where(c => c.Any());

        List<Task> tasks = new List<Task>();

        foreach (var chunk in chunks)
        {
            // Create a new task to process the chunk in parallel
            Task task = Task.Factory.StartNew(() =>
            {
                foreach (State s in chunk)
                {
                    float currentRatio = (float)_stateCount[s] / _numCollapsed;
                    float desiredRatio = Rules.GetProbability(s);

                    float proability = Mathf.Clamp(1f - (currentRatio / desiredRatio), 0f, 1f);
                    _stateProbability[s] = proability;

                    float entropy;
                    if (proability == 0) entropy = 0f;
                    else entropy = proability * Mathf.Log(proability, 2);

                    _stateEntropy[s] = entropy;
                }
            });

            tasks.Add(task);
        }

        // Wait for all tasks to complete
        Task.WaitAll(tasks.ToArray());
        //_stateProbability.Print();
    }

    private Dictionary<Direction, State> GetCollapsedNeighborsStateByDirection(Vector3Int position)
    {
        Dictionary<Direction, Vector3Int> adjacentByDirection = GrabAdjacentByDirections(position);
        Dictionary<Direction, State> collapsedStatesByDirection = new Dictionary<Direction, State>();

        foreach(var coordinateDirectionPair in adjacentByDirection)
        {
            Superposition<State> particle = Particles[coordinateDirectionPair.Value];
            if(particle.GetStatus() == SuperpositionStatus.Collapsed) collapsedStatesByDirection[coordinateDirectionPair.Key] = particle.GetCollapsedState();
        }

        return collapsedStatesByDirection;
    }

    private void SortEntropy()
    {
        _particlePositionsByEntropy = _particlePositionsByEntropy.OrderBy(pos => Particles[pos].CalculateEntropy(_stateEntropy)).ToList();
    }

    private Superposition<State> GetLowestEntropyParticle()
    {
        return Particles[_particlePositionsByEntropy[0]];
    }

    private Vector3Int GetLowestEntropyPosition()
    {
        return _particlePositionsByEntropy[0];
    }

    private void RestoreState(WaveFunctionState checkpoint)
    {
        // Copy state of superpositions from checkpoint
        foreach(var particle in checkpoint.StateParticles)
        {
            Particles[particle.Key] = new Superposition<State>(particle.Value);
        }
        _numCollapsed = checkpoint.NumCollapsed;
        _stateCount = checkpoint.StateCount;
        _stateProbability = checkpoint.StateProbability;
        _stateEntropy = checkpoint.StateEntropy;
        _particlePositionsByEntropy = checkpoint.ParticlePositionsByEntropy;

        // Now collapse the lowest entropy particle from 
    }

    private void ForeachPosition(Action<Vector3Int> function)
    {
        for(int x = 0; x < Bounds.x; x++)
        {
            for(int y = 0; y < Bounds.y; y++)
            {
                for(int z = 0; z < Bounds.z; z++)
                {
                    function(new Vector3Int(x, y, z));
                }
            }
        }
    }
    
    private void ForeachParticle(Action<Superposition<State>> function)
    {
        for(int x = 0; x < Bounds.x; x++)
        {
            for(int y = 0; y < Bounds.y; y++)
            {
                for(int z = 0; z < Bounds.z; z++)
                {
                    function(Particles[new Vector3Int(x, y, z)]);
                }
            }
        }
    }

    private Dictionary<Direction, Vector3Int> GrabAdjacentByDirections(Vector3Int particlePosition)
    {
        Dictionary<Direction, Vector3Int> collapsedPositionsByDirection = new Dictionary<Direction, Vector3Int>();

        foreach(Direction dir in Enum.GetValues(typeof(Direction)))
        {
            Vector3Int neighborPosition = particlePosition + Compass.GetDirectionVector(dir);
            if (InBounds(neighborPosition))
            {
                collapsedPositionsByDirection[dir] = neighborPosition;
            }
        }

        return collapsedPositionsByDirection;
    }

    private Dictionary<Direction, Superposition<State>> GrabAdjacentParticlesByDirection(Vector3Int particlePosition)
    {
        Dictionary<Direction, Superposition<State>> collapsedParticlesByDirection = new Dictionary<Direction, Superposition<State>>();

        foreach(Direction dir in Enum.GetValues(typeof(Direction)))
        {
            Vector3Int neighborPosition = particlePosition + Compass.GetDirectionVector(dir);
            if (InBounds(neighborPosition))
            {
                collapsedParticlesByDirection[dir] = Particles[neighborPosition];
            }
        }

        return collapsedParticlesByDirection;
    }

    private List<Vector3Int> GrabAdjacent(Vector3Int particlePosition)
    {
        List<Vector3Int> neighbors = new List<Vector3Int>();

        foreach(Direction dir in Enum.GetValues(typeof(Direction)))
        {
            Vector3Int neighborPosition = particlePosition + Compass.GetDirectionVector(dir);
            if (InBounds(neighborPosition))
            {
                neighbors.Add(neighborPosition);
            }
        }

        return neighbors;
    }

    private bool InBounds(Vector3Int coordinate)
    {
        return (coordinate.x >= 0 && coordinate.x < Bounds.x &&
                coordinate.y >= 0 && coordinate.y < Bounds.y &
                coordinate.z >= 0 && coordinate.z < Bounds.z);
    }
    #endregion
}
