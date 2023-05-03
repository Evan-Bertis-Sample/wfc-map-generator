 using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomPathDrawerBehaviour : MonoBehaviour
{
    [field: Header("Path Generation")]
    [field: SerializeField] public Vector3 StartPoint {get; private set;}
    [field: SerializeField] public Vector3 EndPoint {get; private set;}
    [field: SerializeField] public float PathLength {get; private set;}
    [field: SerializeField] public float DampingFactor {get; private set;}

    [field: Header("Expansion")]
    [field: SerializeField] public int ExpansionPasses {get; private set;}
    [field: SerializeField] public int KernelSize {get; private set;}
    [field: SerializeField] public int NeighborThreshold {get; private set;}
    [field: SerializeField] public float TransformationChance {get; private set;}

    [field: Header("Texture Settings")]
    [field: SerializeField] public Vector2Int Borders {get; private set;}
    [field: SerializeField] public Texture2D Texture {get; private set;}
    [field: SerializeField] public Color PathColor {get; private set;}
    [field: SerializeField] public Color BackgroundColor {get; private set;}
    [field: SerializeField] public int PixelsPerUnit {get; private set;}

    private RandomPathGenerator _generator;

    public void CreateTexture()
    {
        _generator = new RandomPathGenerator(PathLength, DampingFactor);

        PathDrawer pathDrawer = new PathDrawer(_generator, StartPoint, EndPoint, Borders, PixelsPerUnit, out Texture2D tex);
        Texture = tex;

        pathDrawer.SetPathColor(PathColor);
        pathDrawer.SetBackgroundColor(BackgroundColor);
        pathDrawer.Draw(Texture);

        Texture.Apply();
        Texture.filterMode = FilterMode.Point;

        PixelExtenderDrawer extender = new PixelExtenderDrawer(BackgroundColor, PathColor, NeighborThreshold, TransformationChance, KernelSize);

        for (int i = 0; i < ExpansionPasses; i++)
        {
            extender.Draw(Texture);
            Texture.Apply();
        }
    }
}
