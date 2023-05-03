using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IWaveFunctionVisualizer<StateType, VisualizationType>
{
    public VisualizationType Visualize(WaveFunction<StateType> waveFunction);
}
