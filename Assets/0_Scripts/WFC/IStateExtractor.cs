using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IStateExtractor<StateType, ReferenceType>
{
    public Dictionary<Vector3Int, StateType> ExtractState(ReferenceType reference);
}
