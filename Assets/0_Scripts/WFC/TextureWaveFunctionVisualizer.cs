using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TextureWaveFunctionVisualizer : IWaveFunctionVisualizer<Color, Texture2D>
{
    private Color _superpositionColor = Color.clear;
    private Color _contradictionColor = Color.magenta;

    public TextureWaveFunctionVisualizer(Color? superpositionColor = null, Color? contradictionColor = null)
    {
        if (superpositionColor != null) _superpositionColor = (Color)superpositionColor;
        if (contradictionColor != null) _contradictionColor = (Color)contradictionColor;
    }

    public Texture2D Visualize(WaveFunction<Color> waveFunction)
    {
        Texture2D visualization = new Texture2D(waveFunction.Bounds.x, waveFunction.Bounds.y);
        // Debug.Log($"Visualized Wave Function --> Bounds {waveFunction.Bounds.x} by {waveFunction.Bounds.y}");
        visualization.filterMode = FilterMode.Point;
        DrawSuperpositions(waveFunction, visualization);
        return visualization;
    }

    private void DrawSuperpositions(WaveFunction<Color> waveFunction, Texture2D texture)
    {
        foreach(KeyValuePair<Vector3Int, Superposition<Color>> superposition in waveFunction.Particles)
        {
            (int, int) coord = (superposition.Key.x, superposition.Key.y);
            if (texture.InBounds(coord) == false) continue;

            Color pixelColor;
            switch(superposition.Value.GetStatus())
            {
                case SuperpositionStatus.Indeterminate:
                    if (superposition.Value.States.Count == waveFunction.Rules.NumStates)
                    {
                        pixelColor = _superpositionColor;
                        break;
                    }
                    // Slightly collapsed -- lets see
                    pixelColor = Color.black;
                    foreach (Color c in superposition.Value.States)
                    {
                        pixelColor += c;
                    }
                    // pixelColor /= Mathf.Sqrt(pixelColor.r * pixelColor.r + pixelColor.g + pixelColor.g + pixelColor.b + pixelColor.b);
                    // pixelColor = _constrainedColor;
                    pixelColor /= superposition.Value.States.Count;

                    break;
                case SuperpositionStatus.Contradiction:
                    pixelColor = _contradictionColor;
                    break;
                case SuperpositionStatus.Collapsed:
                    pixelColor = superposition.Value.GetCollapsedState();
                    break;
                default:
                    pixelColor = _contradictionColor;
                    break;
            }

            texture.SetPixel(coord.Item1, coord.Item2, pixelColor);
        }

        texture.Apply();
    }
}
