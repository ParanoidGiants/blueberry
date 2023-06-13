using System;
using UnityEngine;

[Serializable]
public class Vine
{
    public MeshFilter meshFilter; 
    public MeshRenderer meshRenderer;
    
    public void SetMesh(Mesh mesh)
    {
        meshFilter.mesh = mesh;
        meshFilter.sharedMesh.RecalculateBounds();
        meshFilter.sharedMesh.RecalculateNormals();
    }
}