using System;
using UnityEngine;

namespace Creeper
{
    [Serializable]
    public class Vine
    {
        private MeshFilter _meshFilter;
        public Vine(MeshFilter meshFilter)
        {
            _meshFilter = meshFilter;
        }

        public Mesh Mesh => _meshFilter.mesh;

        public void SetMesh(Mesh mesh)
        {
            _meshFilter.mesh = mesh;
            _meshFilter.sharedMesh.RecalculateBounds();
            _meshFilter.sharedMesh.RecalculateNormals();
        }
    }
}