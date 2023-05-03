using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LineDrawer : IDrawer
{
    private Vector2 _start;
    private Vector2 _end;
    private Color _lineColor = Color.white;

    public LineDrawer() {}

    public LineDrawer(Vector2 start, Vector2 end)
    {
        SetLine(start, end);
    }

    public void SetLine(Vector2 start, Vector2 end)
    {
        _start = start;
        _end = end;
    }

    public void SetLineColor(Color color) => _lineColor = color;
    
    public void Draw(Texture2D tex)
    {
        if (_start == null || _end == null) return;
        Vector2Int start = new Vector2Int(Mathf.RoundToInt(_start.x), Mathf.RoundToInt(_start.y));
        Vector2Int end = new Vector2Int(Mathf.RoundToInt(_end.x), Mathf.RoundToInt(_end.y));

        int x0 = start.x;
        int y0 = start.y;
        int x1 = end.x;
        int y1 = end.y;

        int dx = Mathf.Abs(x1 - x0);
        int dy = Mathf.Abs(y1 - y0);

        int sx = (x0 < x1) ? 1 : -1;
        int sy = (y0 < y1) ? 1 : -1;

        int err = dx - dy;

        while (true)
        {
            tex.SetPixel(x0, y0, _lineColor);

            if (x0 == x1 && y0 == y1) break;
            int e2 = 2 * err;
            if (e2 > -dy)
            {
                err -= dy;
                x0 += sx;
            }
            if (e2 < dx)
            {
                err += dx;
                y0 += sy;
            }
        }
    }
}
