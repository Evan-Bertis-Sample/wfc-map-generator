using UnityEngine;
using NUnit.Framework;

public class PatternTests
{
    [Test]
    public void GetContents_ReturnsTrueForInBoundsCoords()
    {
        Pattern<int> pattern = new Pattern<int>(3);
        pattern.SetContents(1, 1, 42);

        int value;
        bool result = pattern.GetContents(1, 1, out value);

        Assert.IsTrue(result);
        Assert.AreEqual(42, value);
    }

    [Test]
    public void GetContents_ReturnsFalseForOutOfBoundsCoords()
    {
        Pattern<int> pattern = new Pattern<int>(3);

        int value;
        bool result = pattern.GetContents(4, 4, out value);

        Assert.IsFalse(result);
    }

    [Test]
    public void SetContents_ReturnsFalseForOutOfBoundsCoords()
    {
        Pattern<int> pattern = new Pattern<int>(3);

        bool result = pattern.SetContents(4, 4, 42);

        Assert.IsFalse(result);
    }

    [Test]
    public void Rotate_RotatesClockwise()
    {
        Pattern<int> pattern = new Pattern<int>(2);
        // 1 2
        // 4 3
        pattern.SetContents(0, 0, 1);
        pattern.SetContents(1, 0, 2);
        pattern.SetContents(1, 1, 3);
        pattern.SetContents(0, 1, 4);

        // pattern.Print();
        Pattern<int> rotated = pattern.Rotate();
        // srotated.Print();
        int[] expectedEdge = {4, 1};
        int[] actualEdge = rotated.GetEdge(Direction.Up);

        Assert.AreEqual(2, rotated.Size);
        Assert.AreEqual(2, actualEdge.Length);
        Assert.AreEqual(expectedEdge, actualEdge);
    }

    [Test]
    public void GetEdge_ReturnsCorrectEdge()
    {
        Pattern<int> pattern = new Pattern<int>(3);

        // 1 2 3
        // 8 ? 4
        // 7 6 5
        pattern.SetContents(0, 0, 1);
        pattern.SetContents(1, 0, 2);
        pattern.SetContents(2, 0, 3);
        pattern.SetContents(2, 1, 4);
        pattern.SetContents(2, 2, 5);
        pattern.SetContents(1, 2, 6);
        pattern.SetContents(0, 2, 7);
        pattern.SetContents(0, 1, 8);

        int[] expectedUp = {1, 2, 3};
        int[] actualUp = pattern.GetEdge(Direction.Up);
        int[] expectedEast = {3, 4, 5};
        int[] actualEast = pattern.GetEdge(Direction.East);
        int[] expectedSouth = {5, 6, 7};
        int[] actualSouth = pattern.GetEdge(Direction.South);
        int[] expectedWest = {7, 8, 1};
        int[] actualWest = pattern.GetEdge(Direction.West);

        Assert.AreEqual(expectedUp, actualUp);
        Assert.AreEqual(expectedEast, actualEast);
        Assert.AreEqual(expectedSouth, actualSouth);
        Assert.AreEqual(expectedWest, actualWest);
    }

    [Test]
    public void Equality()
    {
        Pattern<int> p1 = new Pattern<int>(2);
        // 1 2
        // 4 3
        p1.SetContents(0, 0, 1);
        p1.SetContents(1, 0, 2);
        p1.SetContents(1, 1, 3);
        p1.SetContents(0, 1, 4);

        Pattern<int> p2 = new Pattern<int>(2);
        // 1 2
        // 4 3
        p2.SetContents(0, 0, 1);
        p2.SetContents(1, 0, 2);
        p2.SetContents(1, 1, 3);
        p2.SetContents(0, 1, 4);

        Assert.IsTrue(p1 == p2);
    }
}