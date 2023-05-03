using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Compass
{
    private static Vector3Int[] _directions =
    {
        new Vector3Int(0 , 1 , 0),
        new Vector3Int(0, -1, 0),
        new Vector3Int(1, 0, 0),
        new Vector3Int(-1, 0, 0),
        new Vector3Int(0, 0, 1),
        new Vector3Int(0, 0, -1)
    };

    public static Vector3Int GetDirectionVector(Direction direction) => _directions[(int)direction];

    public static Direction GetOpposite(Direction direction)
    {
        switch (direction)
        {
            case Direction.North:
                return Direction.South;
            case Direction.South:
                return Direction.North;
            case Direction.East:
                return Direction.West;
            case Direction.West:
                return Direction.East;
            case Direction.Up:
                return Direction.Down;
            case Direction.Down:
                return Direction.Up;
            default:
                throw new System.ArgumentException("Invalid direction specified.");
        }
    }

    public static Vector3Int GetOppositeVector(Direction direction) => _directions[(int)GetOpposite(direction)];
}
