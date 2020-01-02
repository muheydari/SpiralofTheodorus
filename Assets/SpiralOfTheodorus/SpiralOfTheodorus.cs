using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static UnityEngine.Camera;
using static UnityEngine.Mathf;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class SpiralOfTheodorus : MonoBehaviour
{
    public enum Ink
    {
        Grayscale,
        Rainbow,
        Gradient
    }
    
    [Header("Colors")] 
    public Ink ColorInk = Ink.Grayscale;
    public bool ReversColor;
    public Gradient ColorGradient;
    
    [Header("Spiral")] public float height = 0.001f;

    private Mesh _mesh;
    private MeshFilter _meshFilter;
    
    private void Awake()
    {
        _meshFilter = GetComponent<MeshFilter>();
        _mesh = new Mesh();
        _meshFilter.mesh = _mesh;
    }
    
    public void drawTriangles(Slider slider)
    {
        if (main) main.orthographicSize = 10.0f;

        DrawSpiralOfTheodorus((int) slider.value);
    }
    private void DrawSpiralOfTheodorus(int c)
    {
        _mesh.Clear();
        var edge_points = FindTheodorusPoints(c);

        var verticesArray = new Vector3[c * 3];
        var trianglesArray = new int[c * 3];
        float perviousHeight = 0;
        for (var i = 1; i < edge_points.Count - 1; i++)
        {
            verticesArray[i * 3 + 0] = new Vector3(edge_points[i].x, edge_points[i].y, i * height); //;)
            verticesArray[i * 3 + 1] = new Vector3(edge_points[i - 1].x, edge_points[i - 1].y, perviousHeight);
            verticesArray[i * 3 + 2] = new Vector3(0, 0, 0);
            trianglesArray[i * 3 + 0] = i * 3 + 0; // ;)
            trianglesArray[i * 3 + 1] = i * 3 + 1;
            trianglesArray[i * 3 + 2] = i * 3 + 2;

            perviousHeight = i * height;
            if (main) main.orthographicSize += 0.02f;
        }

        //add these two triangles to the mesh
        _mesh.vertices = verticesArray;
        _mesh.triangles = trianglesArray;

        //   SplitMesh(m);
        SetColors(_mesh);

        _mesh.RecalculateBounds();
        _mesh.RecalculateNormals();
    }
    private List<Vector2> FindTheodorusPoints(int num_triangles)
    {
        // Find the edge points.
        var edge_points = new List<Vector2>();

        // Add the first point.
        float theta = 0;
        float radius = 1;
        for (var i = 1; i <= num_triangles + 1; i++)
        {
            radius = (float) Math.Sqrt(i);
            edge_points.Add(new Vector2(
                radius * Cos(theta),
                radius * Sin(theta)));
            theta -= Atan2(1, radius);
        }

        return edge_points;
    }
    private Color[] RainbowColors()
    {
        return new[]
        {
            new Color(255, 0, 0),
            new Color(255, 255, 0),
            new Color(255, 128, 0),
            new Color(0, 255, 0),
            new Color(0, 255, 255),
            new Color(0, 0, 255),
            new Color(255, 0, 255)
        };
    }
    private void SetColors(Mesh mesh)
    {
        var AvailableColors = RainbowColors();
        var colors = new Color[mesh.vertexCount];
        for (var i = 0; i < colors.Length; i += 3)
            switch (ColorInk)
            {
                case Ink.Grayscale:
                    if (ReversColor)
                        colors[i] = colors[i + 1] = colors[i + 2] = new Color(1 - (float) i / colors.Length,
                            1 - (float) i / colors.Length, 1 - (float) i / colors.Length);
                    else
                        colors[i] = colors[i + 1] = colors[i + 2] = new Color((float) i / colors.Length,
                            (float) i / colors.Length, (float) i / colors.Length);

                    break;
                case Ink.Rainbow:
                    colors[i] = colors[i + 1] = colors[i + 2] = AvailableColors[i % AvailableColors.Length];
                    break;
                case Ink.Gradient:
                    if (ReversColor)
                        colors[i] = colors[i + 1] = colors[i + 2] = ColorGradient.Evaluate((float) i / colors.Length);
                    else
                        colors[i] = colors[i + 1] = colors[i + 2] =
                            ColorGradient.Evaluate((colors.Length - (float) i) / colors.Length);

                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

        mesh.colors = colors;
    }

    private void SplitMesh(Mesh mesh)
    {
        var triangles = mesh.triangles;
        var verts = mesh.vertices;
        var normals = mesh.normals;
        var uvs = mesh.uv;

        Vector3[] newVerts;
        Vector3[] newNormals;
        Vector2[] newUvs;

        var n = triangles.Length;
        newVerts = new Vector3[n];
        newNormals = new Vector3[n];
        newUvs = new Vector2[n];

        for (var i = 0; i < n; i++)
        {
            newVerts[i] = verts[triangles[i]];
            newNormals[i] = normals[triangles[i]];
            if (uvs.Length > 0) newUvs[i] = uvs[triangles[i]];

            triangles[i] = i;
        }

        mesh.vertices = newVerts;
        mesh.normals = newNormals;
        mesh.uv = newUvs;
        mesh.triangles = triangles;
    }
}