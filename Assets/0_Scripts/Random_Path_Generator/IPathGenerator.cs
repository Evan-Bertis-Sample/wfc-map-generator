using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IPathGenerator
{
    public Vector3[] GeneratePath(Vector3 start, Vector3 end);
}
