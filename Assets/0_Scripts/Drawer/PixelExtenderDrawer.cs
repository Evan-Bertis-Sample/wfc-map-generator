using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PixelExtenderDrawer : IDrawer
{
    private int _kernelSize;
    private int _neighborThreshold;
    private Color _targetColor;
    private Color _neighborColor;
    private float _chance;

    public PixelExtenderDrawer(Color target, Color neighbor, int neighborThreshold, float chance, int kernelSize = 2)
    {
        _targetColor = target;
        _neighborColor = neighbor;
        _neighborThreshold = neighborThreshold;
        _chance = chance;
        _kernelSize = kernelSize;
    }
    
    public void Draw(Texture2D tex)
    {
        (int, int)[] target = tex.GrabPixels(c => c == _targetColor);
        List<(int, int)> changeToNeighborColor = new List<(int, int)>();
        
        foreach((int, int) pixel in target)
        {
            (int, int)[] neighbors = tex.GrabNeighbors(pixel, _kernelSize);

            // Count how many neighbors are of a given color
            int activeNeighbors = 0;
            foreach((int, int) neighbor in neighbors)
            {
                if (activeNeighbors >= _neighborThreshold) break;

                if (tex.GetPixel(neighbor.Item1, neighbor.Item2) == _neighborColor)
                {
                    activeNeighbors++;
                }
            }

            // Are there enough neighbors of a given color to warrant a change?
            if (activeNeighbors >= _neighborThreshold)
            {
                // Flip a weighted coin to determine if you should add change it
                if (Random.Range(0f, 1) >= 1 - _chance) changeToNeighborColor.Add(pixel);
            }
        }

        tex.SetPixels(changeToNeighborColor.ToArray(), _neighborColor);
    }
}
