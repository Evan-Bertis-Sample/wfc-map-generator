using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class RandomPathTester : MonoBehaviour
{
    [field: SerializeField] public Vector3 StartPoint {get; private set;}
    [field: SerializeField] public Vector3 EndPoint {get; private set;}
    [field: SerializeField] public float PathLength {get; private set;}
    [field: SerializeField] public float DampingFactor{get; private set;}

    private LineRenderer _lr;

    public void GeneratePath()
    {
        Vector3[] path = BuildPath();
        DisplayPath(path);
        RunTests(path);
    }

    private void DisplayPath(Vector3[] path)
    {
        if (_lr == null) _lr = GetComponent<LineRenderer>();
        _lr.positionCount = path.Length;
        _lr.SetPositions(path);
    }
    
    private Vector3[] BuildPath()
    {
        RandomPathGenerator pathGenerator = new RandomPathGenerator(PathLength, DampingFactor);
        return pathGenerator.GeneratePath(StartPoint, EndPoint);
    }

    #region Unit Tests
    private void RunTests(Vector3[] path)
    {
        // Test
        TestRunner runner = new TestRunner("RandomPathGenerator");
        runner.Queue("Length_Test", () => Test_PathLength(path));
        runner.Queue("Bounds_Test", () => Test_EndPoints(path));
        runner.Run();
    }

    private bool Test_PathLength(Vector3[] path)
    {
        float length = 0;
        for (int i = 0; i < path.Length - 1; i++)
        {
            length += (path[i] - path[i + 1]).magnitude;
        }
        Debug.Log($"Path Length: {length}");
        return FastApproximately(length, PathLength, 0.01f);
    }

    private bool Test_EndPoints(Vector3[] path)
    {
        return path[0] == StartPoint && path[path.Length - 1] == EndPoint;
    }
    #endregion

    public bool FastApproximately(float a, float b, float threshold)
    {
        return ((a - b) < 0 ? ((a - b) * -1) : (a - b)) <= threshold;
    }
}
