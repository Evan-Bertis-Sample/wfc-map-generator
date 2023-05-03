using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[System.Serializable]
public struct Pattern<T>
{
    public int Size {get; private set;}
    public T[,] Contents {get; private set;}

    public Pattern(int size)
    {
        Size = size;
        Contents = new T[Size, Size];
    }

    public bool GetContents(int x, int y, out T value)
    {
        value = default(T);
        if (!InBounds(x, y)) return false;
        value = Contents[x, y];
        return true;
    }

    public bool SetContents(int x, int y, T value)
    {
        if (!InBounds(x, y)) return false;
        Contents[x, y] = value;
        return true;
    }

    // Rotates the pattern clockwise in 90 degree increments via the amount
    public Pattern<T> Rotate(int amount = 1)
    {
        amount = (amount + 4 % 4) % 4;
        T[,] rcontents = MatrixUtilities.CopyMatrix<T>(Size, Contents);

        while (amount > 0)
        {
            amount--;
            MatrixUtilities.RotateMatrix<T>(Size, rcontents);
        }

        // Build the pattern
        Pattern<T> rotated = new Pattern<T>(Size);
        for (int i = 0; i < Size; i++)
        {
            for (int j = 0; j < Size; j++)
            {
                rotated.SetContents(i, j, rcontents[i, j]);
            }
        }

        return rotated;
    }

    // Gets the items edge (in clockwise order) of the pattern based on the direction
    // If Direction.Up, or Direction.Down is passed as an arguement, then an empty array will be returned (there is no defined behaviour)
    public T[] GetEdge(Direction dir)
    {
        T[] edge = new T[Size];
        switch(dir)
        {
            case Direction.North:
            {
                int y = Size - 1;
                for (int x = 0; x < Size; x++)
                {
                    GetContents(x, y, out T value);
                    edge[x] = value;
                }
                return edge;
            }
            case Direction.East:
            {
                int x = Size - 1;
                for (int y = 0; y < Size; y++)
                {
                    GetContents(x, y, out T value);
                    edge[y] = value;
                }
                return edge;
            }
            case Direction.South:
            {
                int y = 0;
                for (int x = Size - 1; x >= 0; x--)
                {
                    GetContents(x, y, out T value);
                    edge[Size - 1 - x] = value;
                }
                return edge;
            }
            case Direction.West:
            {
                int x = 0;
                for (int y = Size - 1; y >= 0; y--)
                {
                    GetContents(x, y, out T value);
                    edge[Size - 1 - y] = value;
                }
                return edge;
            }
            default:
                return edge;
        }
    }

    public void ReflectHorizontal()
    {
        for(int y = 0; y < Size; y++)
        {
            for(int x = 0; x < Size; x++)
            {
                
            }
        }
    }

    public void Print()
    {
        MatrixUtilities.PrintMatrix(Size, Contents);
    }

    private bool InBounds(int x, int y)
    {
        return x >= 0 && x < Size && y >= 0 && y < Size;
    }

    #region Overloads
    public override bool Equals(object obj)
    {
        if (!(obj is Pattern<T> other))
        {
            return false;
        }

        if (Size != other.Size)
        {
            return false;
        }

        for (int i = 0; i < Size; i++)
        {
            for (int j = 0; j < Size; j++)
            {
                if (!Contents[i, j].Equals(other.Contents[i, j]))
                {
                    return false;
                }
            }
        }

        return true;
    }

    public static bool operator ==(Pattern<T> a, Pattern<T> b)
    {
        return a.Equals(b);
    }

    public static bool operator !=(Pattern<T> a, Pattern<T> b)
    {
        return !a.Equals(b);
    }

    public override int GetHashCode()
    {
        int hash = 17;
        hash = hash * 23 + Size.GetHashCode();

        for (int i = 0; i < Size; i++)
        {
            for (int j = 0; j < Size; j++)
            {
                hash = hash * 23 + Contents[i, j].GetHashCode();
            }
        }

        return hash;
    }

    public override string ToString()
    {
        string output = $"Pattern Size : {Size}x{Size}\n";
        output += Contents.ToString();
        return output;
    }
    #endregion
}

