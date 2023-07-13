using System;
using UnityEngine;

[Serializable]
public class Vine
{
    private static int _MESH_FACE_COUNT = 8;
    public static int MESH_FACE_COUNT => _MESH_FACE_COUNT;
    
    private static float _V_STEP = (2f * Mathf.PI) / _MESH_FACE_COUNT;
    public static float V_STEP => _V_STEP;

    public MeshFilter meshFilter; 
    public MeshRenderer meshRenderer;
    
    public void SetMesh(Mesh mesh)
    {
        meshFilter.mesh = mesh;
        meshFilter.sharedMesh.RecalculateBounds();
        meshFilter.sharedMesh.RecalculateNormals();
    }
}