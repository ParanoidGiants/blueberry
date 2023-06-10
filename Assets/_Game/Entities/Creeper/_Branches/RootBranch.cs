using System.Collections.Generic;
using UnityEngine;

public class RootBranch : MonoBehaviour {
    private const string AMOUNT = "_Amount";
    private const string RADIUS = "_Radius";

    private int _maxLivingNodeCount;
    public Material material;
    public List<IvyNode> branchNodes;

    private Mesh _livelyMesh;
    private MeshFilter _meshFilter; 
    private MeshRenderer _meshRenderer;

    public float branchRadius = 0.02f;
    private readonly int _meshFaces = 8;
    public float defaultRadius;

    public void InitBaseMesh(Vector3 position, Vector3 normal, int maxSegmentCount)
    {
        _meshFilter = GetComponent<MeshFilter>();
        branchNodes = new List<IvyNode>
        {
            new IvyNode(position, normal),
            new IvyNode(position + 2f * normal, normal)
        };
        _meshRenderer = GetComponent<MeshRenderer>();
        _meshRenderer.material = material;
        ApplyMesh(CreateMesh(branchNodes));
        _maxLivingNodeCount = maxSegmentCount;

        material.SetFloat(RADIUS, 1f);
        material.SetFloat(AMOUNT, 1f);
    }

    private float Remap(float input, float oldLow, float oldHigh, float newLow, float newHigh) {
        float t = Mathf.InverseLerp(oldLow, oldHigh, input);
        return Mathf.Lerp(newLow, newHigh, t);
    }

    private Mesh CreateMesh(List<IvyNode> nodes) {
        Mesh branchMesh = new Mesh();
        Vector3[] vertices = new Vector3[(nodes.Count) * _meshFaces * 4];
        Vector3[] normals = new Vector3[nodes.Count * _meshFaces * 4];
        Vector2[] uv = new Vector2[nodes.Count * _meshFaces * 4];
        int[] triangles = new int[(nodes.Count - 1) * _meshFaces * 6];

        for (int i = 0; i < nodes.Count; i++)
        {
            float vStep = (2f * Mathf.PI) / _meshFaces;

            var fw = Vector3.zero;
            if (i > 0) {
                fw = branchNodes[i - 1].getPosition() - branchNodes[i].getPosition();
            }

            if (i < branchNodes.Count - 1) {
                fw += branchNodes[i].getPosition() - branchNodes[i + 1].getPosition();
            }

            if (fw == Vector3.zero) {
                fw = Vector3.forward;
            }

            fw.Normalize();

            var up = branchNodes[i].getNormal();
            up.Normalize();

            for (int v = 0; v < _meshFaces; v++) {
                var orientation = Quaternion.LookRotation(fw, up);
                Vector3 xAxis = Vector3.up;
                Vector3 yAxis = Vector3.right;
                Vector3 pos = branchNodes[i].getPosition();
                pos += orientation * xAxis * (branchRadius * i / nodes.Count * Mathf.Sin(v * vStep));
                pos += orientation * yAxis * (branchRadius * i / nodes.Count * Mathf.Cos(v * vStep));

                vertices[i * _meshFaces + v] = pos;

                var diff = pos - branchNodes[i].getPosition();
                normals[i * _meshFaces + v] = diff / diff.magnitude;

                float uvID = Remap(i, 0, nodes.Count - 1, 0, 1);
                uv[i * _meshFaces + v] = new Vector2((float)v / _meshFaces, uvID);
            }

            if (i + 1 < nodes.Count) {
                for (int v = 0; v < _meshFaces; v++) {
                    triangles[i * _meshFaces * 6 + v * 6] = ((v + 1) % _meshFaces) + i * _meshFaces;
                    triangles[i * _meshFaces * 6 + v * 6 + 1] = triangles[i * _meshFaces * 6 + v * 6 + 4] = v + i * _meshFaces;
                    triangles[i * _meshFaces * 6 + v * 6 + 2] = triangles[i * _meshFaces * 6 + v * 6 + 3] = ((v + 1) % _meshFaces + _meshFaces) + i * _meshFaces;
                    triangles[i * _meshFaces * 6 + v * 6 + 5] = (_meshFaces + v % _meshFaces) + i * _meshFaces;
                }
            }
        }

        branchMesh.vertices = vertices;
        branchMesh.triangles = triangles;
        branchMesh.normals = normals;
        branchMesh.uv = uv;
        return branchMesh;
    }
    
    public void AddIvyNode(Vector3 position, Vector3 normal)
    {
        branchNodes.Add(new IvyNode(position, normal));
        Mesh mesh = _meshFilter.mesh;
        var nodeCount = branchNodes.Count;
        mesh.vertices = new Vector3[nodeCount * _meshFaces * 4];
        mesh.normals = new Vector3[nodeCount * _meshFaces * 4];
        mesh.uv = new Vector2[nodeCount * _meshFaces * 4];
        mesh.triangles = new int[(nodeCount - 1) * _meshFaces * 6];
        _meshFilter.mesh = mesh;
        ApplyMesh(mesh);
    }
    
    public void SetIvyNode(int index, Vector3 branchPosition)
    {
        branchNodes[index].position = branchPosition;
        branchNodes[index].normal = transform.up;
    }

    public void UpdateIvyNodes()
    {
        Mesh mesh = _meshFilter.mesh;
        Vector3[] vertices = mesh.vertices;
        Vector3[] normals = mesh.normals;
        Vector2[] uv = mesh.uv;
        int[] triangles = mesh.triangles;
        var nodeCount = branchNodes.Count;
        for (int i = 0; i < nodeCount; i++)
        {
            var radius = defaultRadius;
            if (i > nodeCount - _maxLivingNodeCount)
            {
                radius -= ((float)(i - (nodeCount - _maxLivingNodeCount)) / _maxLivingNodeCount) * defaultRadius;
            }

            float vStep = (2f * Mathf.PI) / _meshFaces;

            var fw = Vector3.zero;
            if (i > 0)
            {
                fw = branchNodes[i - 1].getPosition() - branchNodes[i].getPosition();
            }

            if (i < branchNodes.Count - 1)
            {
                fw += branchNodes[i].getPosition() - branchNodes[i + 1].getPosition();
            }

            if (fw == Vector3.zero)
            {
                fw = Vector3.forward;
            }

            fw.Normalize();

            var up = branchNodes[i].getNormal();
            up.Normalize();

            for (int v = 0; v < _meshFaces; v++)
            {
                var orientation = Quaternion.LookRotation(fw, up);
                Vector3 xAxis = Vector3.up;
                Vector3 yAxis = Vector3.right;
                Vector3 pos = branchNodes[i].getPosition();
                pos += orientation * xAxis * (radius * Mathf.Sin(v * vStep));
                pos += orientation * yAxis * (radius * Mathf.Cos(v * vStep));

                vertices[i * _meshFaces + v] = pos;

                var diff = pos - branchNodes[i].getPosition();
                normals[i * _meshFaces + v] = diff / diff.magnitude;

                float uvID = Remap(i, i, nodeCount - 1, 0, 1);
                uv[i * _meshFaces + v] = new Vector2((float)v / _meshFaces, uvID);
            }

            if (i + 1 < branchNodes.Count)
            {
                for (int v = 0; v < _meshFaces; v++)
                {
                    triangles[i * _meshFaces * 6 + v * 6] = ((v + 1) % _meshFaces) + i * _meshFaces;
                    triangles[i * _meshFaces * 6 + v * 6 + 1] =
                        triangles[i * _meshFaces * 6 + v * 6 + 4] = v + i * _meshFaces;
                    triangles[i * _meshFaces * 6 + v * 6 + 2] = triangles[i * _meshFaces * 6 + v * 6 + 3] =
                        ((v + 1) % _meshFaces + _meshFaces) + i * _meshFaces;
                    triangles[i * _meshFaces * 6 + v * 6 + 5] = (_meshFaces + v % _meshFaces) + i * _meshFaces;
                }
            }
        }

        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.normals = normals;
        mesh.uv = uv;
        ApplyMesh(mesh);
    }

    private void ApplyMesh(Mesh mesh)
    {
        _meshFilter.mesh = mesh;
        _meshFilter.sharedMesh.RecalculateBounds();
        _meshFilter.sharedMesh.RecalculateNormals();
    }
}