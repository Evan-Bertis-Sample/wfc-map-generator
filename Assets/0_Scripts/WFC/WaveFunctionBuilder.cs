using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// A class devoted to building a Wave Function from reference data
public class WaveFunctionBuilder<StateType, ReferenceType>
{
    private Ruleset<StateType> _rules;
    private ReferenceType _reference;

    public WaveFunctionBuilder(IRulesetBuilder<StateType, ReferenceType> rulesetBuilder, ReferenceType reference)
    {
        _reference = reference;
        _rules = rulesetBuilder.BuildRuleset(_reference);
    }

    public WaveFunctionBuilder(Ruleset<StateType> rules, ReferenceType reference)
    {
        _reference = reference;
        _rules = rules;
    }

    public WaveFunction<StateType> BuildWaveFunction(int width, int height, int depth, IStateExtractor<StateType, ReferenceType> stateExtractor = null)
    {
        WaveFunction<StateType> waveFunction = new WaveFunction<StateType>(_rules, width, height, depth);
        if (stateExtractor != null && _reference != null) waveFunction.SetStartingState(stateExtractor.ExtractState(_reference));                 
        return waveFunction;
    }
}