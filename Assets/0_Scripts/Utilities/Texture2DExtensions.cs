using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public static class Texture2DExtensions
{
    public static Texture2D ModifyPixels(this Texture2D texture, Func<Color, Color> modifier)
    {
        Color[] pixels = texture.GetPixels();
        for (int i = 0; i < pixels.Length; i++)
        {
            pixels[i] = modifier(pixels[i]);
        }
        texture.SetPixels(pixels);
        texture.Apply();
        return texture;
    }

    public static (int, int)[] GrabPixels(this Texture2D texture, Func<Color, bool> where)
    {
        Color[] pixels = texture.GetPixels();
        List<(int, int)> selected = new List<(int, int)>();
        for (int i = 0; i < pixels.Length; i++)
        {
            if (where(pixels[i]))
            {
                selected.Add(texture.Get2DCoordinate(i));
            }
        }

        return selected.ToArray();
    }

    public static (int, int)[] GrabNeighbors(this Texture2D texture, (int, int) coordinate, int kernelSize)
    {
        List<(int, int)> neighbors = new List<(int, int)>();

        int halfKernelSize = kernelSize / 2;

        for (int x = -halfKernelSize; x <= halfKernelSize; x++)
        {
            for (int y = -halfKernelSize; y <= halfKernelSize; y++)
            {
                int neighborX = coordinate.Item1 + x;
                int neighborY = coordinate.Item2 + y;

                if (neighborX >= 0 && neighborX < texture.width && neighborY >= 0 && neighborY < texture.height)
                {
                    neighbors.Add((neighborX, neighborY));
                }
            }
        }

        return neighbors.ToArray();
    }

    public static Dictionary<Direction, (int, int)> GrabNeighborsByDirection(this Texture2D texture, (int, int) coordinate)
    {
        Dictionary<Direction, (int, int)> neighborsByPosition = new Dictionary<Direction, (int, int)>();

        foreach(Direction dir in Enum.GetValues(typeof(Direction)))
        {
            Vector3Int dirVector = Compass.GetDirectionVector(dir);

            if (dirVector.z != 0) continue;

            (int, int) neighbor = (coordinate.Item1 + dirVector.x, coordinate.Item2 + dirVector.y); 
            neighborsByPosition[dir] = neighbor;
        }

        return neighborsByPosition;
    }

    public static (int, int) Get2DCoordinate(this Texture2D texture, int index)
    {
        return (index % texture.width, index / texture.width);
    }

    public static Texture2D SetPixels(this Texture2D texture, (int, int)[] pixels, Color color)
    {
        for (int i = 0; i < pixels.Length; i++)
        {
            if (texture.InBounds(pixels[i])) texture.SetPixel(pixels[i].Item1, pixels[i].Item2, color);
        }
        return texture;
    }

    public static void Foreach(this Texture2D texture, Action<Color> function)
    {
        Color[] pixels = texture.GetPixels();
        for (int i = 0; i < pixels.Length; i++)
        {
            function(pixels[i]);
        }
    }

    public static void ForeachCoord(this Texture2D texture, Action<(int, int)> function)
    {
        for (int x = 0; x < texture.width; x++)
        {
            for (int y = 0; y < texture.height; y++)
            {
                function((x, y));
            }
        }
    }

    public static bool InBounds(this Texture2D texture, (int, int) pixel)
    {
        return (pixel.Item1 >= 0 && pixel.Item1 < texture.width &&
                pixel.Item2 >= 0 && pixel.Item2 < texture.height);
    }

    public static void Save(this Texture2D texture, string path)
    {
        // string directoryName = System.IO.Directory.GetParent(path).FullName.Replace('\\', '/');
        // if (System.IO.File.Exists(directoryName) == false)
        // {
        //     throw new Exception($"Path {directoryName} does not exist!");
        // }
        byte[] bytes = texture.EncodeToPNG();
        System.IO.File.WriteAllBytes(path, bytes);
    }
}
