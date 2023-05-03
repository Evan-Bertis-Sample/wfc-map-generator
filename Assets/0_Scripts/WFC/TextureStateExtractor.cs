using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TextureStateExtractor : IStateExtractor<Color, Texture2D>
{
    private List<Color> _collapsedColors;
    public TextureStateExtractor(List<Color> collapsedColors)
    {
        _collapsedColors = collapsedColors;
    }

    public Dictionary<Vector3Int, Color> ExtractState(Texture2D reference)
    {
        if (_collapsedColors == null) throw new System.Exception("Cannot extract state when Extractor was constructed with a null list of Collapsed Colors!");
        if (_collapsedColors.Count == 0) return new Dictionary<Vector3Int, Color>();
        
        Dictionary<Vector3Int, Color> state = new Dictionary<Vector3Int, Color>();

        reference.ForeachCoord(coord =>
        {
            Color pixelState = reference.GetPixel(coord.Item1, coord.Item2);
            if (_collapsedColors.Contains(pixelState))
            {
                // This is a state that is collapsed --> store it
                Vector3Int pos = new Vector3Int(coord.Item1, coord.Item2, 0);
                state[pos] = pixelState;
            }
        });

        return state;
    }
}
