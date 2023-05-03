using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomPathGenerator : IPathGenerator
{
    private float _length;
    private float _dampingFactor;
    private Dictionary<Vector3, Vector3> _segments;

    public     RandomPathGenerator(float length, float dampingFactor)
    {
        _length = length;
        _dampingFactor = dampingFactor;
        _segments = new Dictionary<Vector3, Vector3>();
    }

    public Vector3[] GeneratePath(Vector3 start, Vector3 end)
    {
        Vector3[] path = SubdivideRecursively(start, end, _dampingFactor * _length);
        // Clear up memory to prevent overflow
        _segments.Clear();
        _segments.TrimExcess();
        return path;
    }

    private Vector3[] SubdivideRecursively(Vector3 start, Vector3 end, float targetLength, int level = 1, float ratio = 1)
    {
        // float error = _length - targetLength * Mathf.Pow(2, level - 1);
        float error = _length - (1/ratio) * targetLength;
        //Debug.Log($"Target Length: {targetLength} Error: {error}");

        // Base case - reached final level or small error --> correct error in final step
        if (error < 0.1f || level > 10)
        {
            Vector3[] final = Subdivide(start, end, targetLength + error * ratio);
            // Update relationships in dictionary
            _segments[start] = final[1];
            _segments[final[1]] = end;
            return final;
        }

        Vector3[] subdivided = Subdivide(start, end, targetLength);

        // Unable to subdivide -- return
        if (subdivided.Length == 2)
        {
            // Update relationships in the dictionary
            _segments[start] = end;
            return subdivided;
        }

        // keep fixing subdivision until the subdivision does not intersect path
        // Now check if the line segments generated intersect withi the line segments currently in the path
        while (IntersectsPath(subdivided[0], subdivided[1]) || (IntersectsPath(subdivided[1], subdivided[2])))
        {
            subdivided = Subdivide(start, end, targetLength); // Redo subdivision
        }

        // Update relationships in the dictionary
        _segments[start] = subdivided[1];
        _segments[subdivided[1]] = end;

        // Progress towards final destination length by moving forward by _dampingFactor * error
        // Progression in this manner will converge until the final destination length
        // We subdivide our subdivisons with target lengths that will guarantee a longer overall path
        float nextLength = (targetLength + (error * _dampingFactor) / Mathf.Pow(2, level - 1)); // This should be the sum of the lengths of the next two subdivisions
        // Distribute length proportionally
        float left = (subdivided[1] - subdivided[0]).magnitude / targetLength; // The ratio of this segment to the total length of both segments
        float right = (subdivided[2] - subdivided[1]).magnitude / targetLength;

        // Debug.Log($"Next Left: {nextLength * left} Next Right: {nextLength * right}");
        // Recursively subdivide
        List<Vector3> next = new List<Vector3>();
        next.AddRange(SubdivideRecursively(subdivided[0], subdivided[1], nextLength * left, level + 1, ratio * left));
        next.AddRange(SubdivideRecursively(subdivided[1], subdivided[2], nextLength * right, level + 1, ratio * right));
        // Debug.Log(string.Join('\n', _segments));
        return next.ToArray();
    }

    private float CalculateNextStep(int resolution)
    {
        float sumMu = 0;
        for (int i = 1; i <= resolution + 1; i++) 
        {
            sumMu += Mathf.Pow(_dampingFactor, i) * Mathf.Pow(-1, i + 1);
        }

        return sumMu;
    }

    private bool IntersectsPath(Vector3 start, Vector3 end)
    {
        foreach(KeyValuePair<Vector3, Vector3> segment in _segments)
        {
            if (start == segment.Key || end == segment.Key || start == segment.Value || end == segment.Value) continue; // Same segment
            if (Intersects(start, end, segment.Key, segment.Value))
            {
                return true; // Intersects a segment
            }
        }
        return false;
    }

    private bool Intersects(Vector3 p1, Vector3 q1, Vector3 p2, Vector3 q2)
    {
        // Find the four orientations needed for general and
        // special cases
        int o1 = Orientation(p1, q1, p2);
        int o2 = Orientation(p1, q1, q2);
        int o3 = Orientation(p2, q2, p1);
        int o4 = Orientation(p2, q2, q1);
    
        // General case
        if (o1 != o2 && o3 != o4)
            return true;
    
        // Special Cases
        // p1, q1 and p2 are collinear and p2 lies on segment p1q1
        if (o1 == 0 && OnSegment(p1, p2, q1)) return true;
    
        // p1, q1 and q2 are collinear and q2 lies on segment p1q1
        if (o2 == 0 && OnSegment(p1, q2, q1)) return true;
    
        // p2, q2 and p1 are collinear and p1 lies on segment p2q2
        if (o3 == 0 && OnSegment(p2, p1, q2)) return true;
    
        // p2, q2 and q1 are collinear and q1 lies on segment p2q2
        if (o4 == 0 && OnSegment(p2, q1, q2)) return true;
    
    return false; // Doesn't fall in any of the above cases
    }

    // To find orientation of ordered triplet (p, q, r).
    // The function returns following values
    // 0 --> p, q and r are collinear
    // 1 --> Clockwise
    // 2 --> Counterclockwise
    private int Orientation(Vector3 p, Vector3 q, Vector3 r)
    {
        // See https://www.geeksforgeeks.org/orientation-3-ordered-points/
        // for details of below formula.
        float val = (q.y - p.y) * (r.x - q.x) -
                (q.x - p.x) * (r.y - q.y);
    
        if (val == 0) return 0; // collinear
    
        return (val > 0)? 1: 2; // clock or counterclock wise
    }

    // Given three collinear points p, q, r, the function checks if
    // point q lies on line segment 'pr'
    private bool OnSegment(Vector3 p, Vector3 q, Vector3 r)
    {
        if (q.x <= Mathf.Max(p.x, r.x) && q.x >= Mathf.Min(p.x, r.x) &&
            q.y <= Mathf.Max(p.y, r.y) && q.y >= Mathf.Min(p.y, r.y))
        return true;
    
        return false;
    }

    private Vector3[] Subdivide(Vector3 start, Vector3 end, float length)
    {
        // Pick a randome theta
        float theta = Mathf.Acos(1 - Random.Range(0, 2f)) - Mathf.PI / 2;
        // Rotate vector v by this theta to get P'
        Vector3 v = end - start;

        if (length <= v.magnitude) 
        {
            // Debug.Log("Impossible to Subdivide");
            return new Vector3[]{start, end}; // It is impossible to add a third point
        }

        Vector3 pPrime = new Vector3(
            v.x * Mathf.Cos(theta) - v.y * Mathf.Sin(theta),
            v.x * Mathf.Sin(theta) + v.y * Mathf.Cos(theta)
        );

        // Now Solve for the projection onto v to get pHat
        Vector3 pHat = (Vector3.Dot(v, pPrime) / Vector3.Dot(v, v)) * v;
        // Use pHat and pPrime to create an orthographic basis for our new coordinate system defined by (v, u)
        Vector3 u = pPrime - pHat;

        // Now solve for the distance d to move along u to get to P
        float a = pHat.magnitude;
        float b = v.magnitude - a;

        // This is painful
        float b4 = Mathf.Pow(b, 4);
        float b2 = Mathf.Pow(b, 2);
        float a4 = Mathf.Pow(a, 4);
        float a2 = Mathf.Pow(a, 2);
        float l4 = Mathf.Pow(length, 4);
        float l2 = Mathf.Pow(length, 2);

        float d = Mathf.Sqrt(b4 - 2*l2*b2 - 2*a2*b2 + l4 + a4 - 2*l2*a2) / (2 * length);

        // Now find point P
        Vector3 p = a * v.normalized + d * u.normalized;
        // Translate
        p += start;

        return new Vector3[] {start, p, end};
    }
}
