using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PatternWaveFunctionVisualizer : IWaveFunctionVisualizer<Pattern<Color>, Texture2D>
{
    private Color _superpositionColor = Color.clear;
    private Color _contradictionColor = Color.magenta;
    private Color _unknownPatternColor = Color.cyan;
    private int _patternSize;

    public PatternWaveFunctionVisualizer(int patternSize, Color? superpositionColor = null, Color? contradictionColor = null, Color? unknownPatternColor = null)
    {
        if (superpositionColor != null) _superpositionColor = (Color)superpositionColor;
        if (contradictionColor != null) _contradictionColor = (Color)contradictionColor;
        if (unknownPatternColor != null) _unknownPatternColor = (Color)unknownPatternColor;
        _patternSize = patternSize;
    }

    public Texture2D Visualize(WaveFunction<Pattern<Color>> waveFunction)
    {
        int size = _patternSize - 1;
        Texture2D output = new Texture2D(waveFunction.Bounds.x * (size), waveFunction.Bounds.y * (size));
        output.filterMode = FilterMode.Point;
        foreach(KeyValuePair<Vector3Int, Superposition<Pattern<Color>>> patternParticlePair in waveFunction.Particles)
        {
            Vector3Int topLeft = patternParticlePair.Key * (size);
            for(int i = 0; i < size; i++)
            {
                for(int j = 0; j < size; j++)
                {
                    Color contents;
                    switch (patternParticlePair.Value.GetStatus())
                    {
                        case SuperpositionStatus.Collapsed:
                            Pattern<Color> pattern = patternParticlePair.Value.GetCollapsedState();
                            bool found = pattern.GetContents(i, j, out contents);
                            if (found == false) contents = _unknownPatternColor;
                            break;
                        case SuperpositionStatus.Indeterminate:
                            if (patternParticlePair.Value.States.Count == waveFunction.Rules.NumStates)
                            {
                                contents = _superpositionColor;
                                break;
                            }

                            contents = Color.black;
                            foreach(Pattern<Color> p in patternParticlePair.Value.States)
                            {
                                Color c = Color.white;
                                p.GetContents(i, j, out c);
                                contents += c;
                            }
                            contents /= patternParticlePair.Value.States.Count;
                            // contents = _superpositionColor;
                            break;
                        case SuperpositionStatus.Contradiction:
                            contents = _contradictionColor;
                            break;
                        default:
                            contents = _unknownPatternColor;
                            break;
                    }

                    output.SetPixel(topLeft.x + i, topLeft.y + j, contents);
                }
            }

            output.Apply();
        }
        output.Apply();
        return output;
    }
}
