using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class RainbowScript : MonoBehaviour
{
    double[] x = { 0.934, 0.95, 0.95, 0.885, 0.885, 0.587, 0.673, 0.719, 0.673, 0.587, 0.339, 0.375, 0.375, 0.339, 6.12E-17, 4.40E-17, 3.75E-33, 4.40E-17, 6.12E-17, -0.339, -0.375, -0.375, -0.339, -0.587, -0.673, -0.719, -0.673, -0.587, -0.809, -0.885, -0.885, -0.809, -0.999 };
    double[] y = { 0, 0.309, -0.309, 0.376, -0.376, 0.809, 0.545, 0, -0.545, -0.809, 0.883, 0.375, -0.375, -0.883, 0.999, 0.719, -6.12E-17, -0.719, -0.999, 0.883, 0.375, -0.375, -0.883, 0.809, 0.545, -8.81E-17, -0.545, -0.809, 0.587, 0.376, -0.376, -0.587, -1.22E-16 };
    double[] z = { 0.358, -0.0349, -0.0349, 0.276, 0.276, -0.0349, 0.5, 0.695, 0.5, -0.0349, 0.326, 0.848, 0.848, 0.326, -0.0349, 0.695, 1, 0.695, -0.0349, 0.326, 0.848, 0.848, 0.326, -0.0349, 0.5, 0.695, 0.5, -0.0349, -0.0349, 0.276, 0.276, -0.0349, -0.0349 };
    Color[] colors;
    float[] values;

    MeshFilter[] meshFilters;
    Vector3[] nautilusVertices;
    Mesh mesh;

    // Use this for initialization
    void Start()
    {
        meshFilters = GetComponentsInChildren<MeshFilter>();
        mesh = meshFilters[0].mesh;

        // Change Vertices To Nautilus Electrode Locations
        nautilusVertices = new Vector3[x.Length];
        for (int i = 0; i < x.Length; i++)
        {
            Vector3 v = new Vector3((float)x[i], (float)y[i], (float)z[i]);
            nautilusVertices[i] = v;
        }

        List<int> triangles = new List<int>();
        for (int i = 0; i < nautilusVertices.Length; i++)
        {
            for (int j = i + 1; j < nautilusVertices.Length; j++)
            {
                for (int k = j + 1; k < nautilusVertices.Length; k++)
                {
                    triangles.Add(i);
                    triangles.Add(j);
                    triangles.Add(k);
                }
            }
        }

        //mesh.Clear();
        mesh.triangles = triangles.ToArray();
        mesh.vertices = nautilusVertices;
        mesh.RecalculateBounds();

        Vector3[] vertices = mesh.vertices;
        values = new float[vertices.Length];
        colors = new Color[vertices.Length];
        for (int j = 0; j < vertices.Length; j++)
        {
            //values[j] = 0.0f;
            values[j] = Random.value;
        }
    }


    Transform Vertices;
    Vector3[] tri;
    // Update is called once per frame
    void Update()
    {
        for (int j = 0; j < colors.Length; j++)
        {
            values[j] = (values[j] + Random.value / 1000f) % 1.0f;
            colors[j] = heatMapColorforValue(values[j]);
        }
        mesh.colors = colors;

    }

    public static Color heatMapColorforValue(float value)
    {
        float h = (1.0f - value) * (240.0f / 360.0f);
        return Color.HSVToRGB(h, 1.0f, 0.5f);
    }

}
