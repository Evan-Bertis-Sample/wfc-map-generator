using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathDrawer : IDrawer
{
    private IPathGenerator _generator;
    private Vector3[] _path;
    private Color _pathColor = Color.white;
    private Color _backgroundColor = Color.black;
    private int _pixelsPerUnit;
    private Vector3 _borders;

    public PathDrawer(IPathGenerator pathGenerator)
    {
        SetGenerator(pathGenerator);
    }
    
    public PathDrawer(IPathGenerator pathGenerator, Vector3 start, Vector3 end, Vector2Int borders, int pixelsPerUnit, out Texture2D texture)
    {
        SetGenerator(pathGenerator);
        SetPixelsPerUnit(pixelsPerUnit);
        SetBorders(borders);
        texture = CreateTexture(start, end, borders);
    }

    // Creates a texture perfectly sized to fit a generated path
    public Texture2D CreateTexture(Vector3 start, Vector3 end, Vector2Int borders)
    {
        GeneratePath(start, end);
        // Find bounds of the path
        Vector2 min = _path[0];
        Vector2 max = _path[0];
        foreach(Vector3 vertex in _path)
        {
            min = Vector2.Min(min, vertex);
            max = Vector2.Max(max, vertex);
        }
        // Add borders and create texture
        int width = Mathf.CeilToInt((max - min).x * _pixelsPerUnit + borders.x * 2);
        int height = Mathf.CeilToInt((max - min).y * _pixelsPerUnit + borders.y * 2);

        return new Texture2D(width, height);
    }

    public void SetGenerator(IPathGenerator pathGenerator) => _generator = pathGenerator;

    public void SetPathColor(Color color) => _pathColor = color;

    public void SetBackgroundColor(Color color) => _backgroundColor = color;

    public void SetPixelsPerUnit(int ppu) => _pixelsPerUnit = ppu;

    public void SetBorders(Vector2Int borders) => _borders = (Vector3Int)borders;

    public void GeneratePath(Vector3 start, Vector3 end)
    {
        if (_generator == null)
        {
            Debug.LogWarning("Please set the path generator before generation.");
            return;
        }

        _path = _generator.GeneratePath(start, end);

        // Fix path such that the bottom left corder is drawing
        Vector2 min = _path[0];
        foreach(Vector3 vertex in _path)
        {
            min = Vector2.Min(min, vertex);
        }

        for(int i = 0; i < _path.Length; i++)
        {
            _path[i] -= (Vector3)min;
        }
    }

    public Vector3[] GetPath()
    {
        if (_path == null || _path.Length == 0)
        {
            Debug.LogWarning($"Must generate path before getting path!");
            return null;
        }

        return _path;
    }
    
    public void Draw(Texture2D tex)
    {
        if (_path == null || _path.Length == 0)
        {
            Debug.LogWarning($"Must generate path before drawing on texture: {tex.name}!");
            return;
        }

        tex.ModifyPixels(c => _backgroundColor);
        LineDrawer lineDrawer = new LineDrawer();
        lineDrawer.SetLineColor(_pathColor);

        for(int i = 0; i < _path.Length - 1; i++)
        {
            lineDrawer.SetLine(_path[i] * _pixelsPerUnit + _borders, _path[i + 1] * _pixelsPerUnit + _borders);
            lineDrawer.Draw(tex);
        }
    }
}
