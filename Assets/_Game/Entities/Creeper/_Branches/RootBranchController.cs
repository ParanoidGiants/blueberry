using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

[Serializable]
public class RootBranchMesh
{
    public Mesh mesh;
    public MeshFilter meshFilter; 
    public MeshRenderer meshRenderer;
}

public class RootBranchController : MonoBehaviour
{
    private const string AMOUNT = "_Amount";
    private const string RADIUS = "_Radius";
    private const int MESH_FACE_COUNT = 8;
    
    [Header("References")]
    public Transform head;
    public Material material;
    
    [Space(10)]
    [Header("Watchers")]
    [SerializeField] private RootBranchMesh livingMesh;
    [SerializeField] private List<IvyNode> livingBranchNodes;
    [SerializeField] private float defaultRadius;
    [SerializeField] private float branchLength = 2f;
    [SerializeField] private int maxNodeCount = 10;
    
    private Vector2 _inputDirection;
    public Vector2 InputDirection { set => _inputDirection = value; }
    
    private LineRenderer _line;
    private Vector3 _lastPosition;
    private int _maxLivingNodeCount;
    private int _currentSegmentIndex = -1;

    private void Start()
    {
        _line = GetComponent<LineRenderer>();
        
        _currentSegmentIndex = 1;
        var position = head.position;
        _lastPosition = position;

        var localScale = head.localScale;
        _line.startWidth = localScale.x;
        _line.endWidth = localScale.x;
        _line.positionCount = 2;
        _line.SetPosition(0, position);
        _line.SetPosition(1, position);
        InitBaseMesh(position, head.up, maxNodeCount);
    }

    private void Update()
    {
        if (_inputDirection.magnitude < 0.1f) return;
        UpdateBranch();
    }

    private void UpdateBranch()
    {
        var position = head.position;
        var distance = Vector3.Distance(_lastPosition, position);
        if (distance > branchLength)
        {
            AddIvyNode(position, head.up);
        }
        
        var moveDirection = position - _line.GetPosition(_currentSegmentIndex);
        var linePositions = new Vector3[_currentSegmentIndex+1];
        _line.GetPositions(linePositions);
        var start = Mathf.Clamp(_currentSegmentIndex - maxNodeCount, 0, _currentSegmentIndex);
        for (var i = start; i <= _currentSegmentIndex; i++)
        {
            var moveFactor =  (float) (i - start) / maxNodeCount;
            var linePosition = linePositions[i] + moveDirection * moveFactor;
            _line.SetPosition(i, linePosition);
            livingBranchNodes[i].position = linePosition;
            livingBranchNodes[i].normal = transform.up;
        }
        UpdateIvyNodes();
    }
        
    public void InitBaseMesh(Vector3 position, Vector3 normal, int maxSegmentCount)
    {
        _maxLivingNodeCount = maxSegmentCount;
        livingBranchNodes = new List<IvyNode>
        {
            new IvyNode(position, normal),
            new IvyNode(position + 2f * normal, normal)
        };
        var branchMesh = Instantiate(new GameObject(), transform);
        var meshFilter = branchMesh.AddComponent<MeshFilter>();
        var meshRenderer = branchMesh.AddComponent<MeshRenderer>();
        var mesh = CreateMesh(livingBranchNodes);
        meshRenderer.material = material;
        livingMesh = new RootBranchMesh()
        {
            mesh = mesh,
            meshRenderer = meshRenderer,
            meshFilter = meshFilter
        };
    }

    private Mesh CreateMesh(List<IvyNode> nodes)
    {
        Mesh branchMesh = new Mesh();
        Vector3[] vertices = new Vector3[(nodes.Count) * MESH_FACE_COUNT * 4];
        Vector3[] normals = new Vector3[nodes.Count * MESH_FACE_COUNT * 4];
        Vector2[] uv = new Vector2[nodes.Count * MESH_FACE_COUNT * 4];
        int[] triangles = new int[(nodes.Count - 1) * MESH_FACE_COUNT * 6];

        for (int i = 0; i < nodes.Count; i++)
        {
            var radius = defaultRadius * i / nodes.Count;
            float vStep = (2f * Mathf.PI) / MESH_FACE_COUNT;

            var fw = Vector3.zero;
            if (i > 0) {
                fw = livingBranchNodes[i - 1].getPosition() - livingBranchNodes[i].getPosition();
            }

            if (i < livingBranchNodes.Count - 1) {
                fw += livingBranchNodes[i].getPosition() - livingBranchNodes[i + 1].getPosition();
            }

            if (fw == Vector3.zero) {
                fw = Vector3.forward;
            }

            fw.Normalize();

            var up = livingBranchNodes[i].getNormal();
            up.Normalize();

            for (int v = 0; v < MESH_FACE_COUNT; v++) {
                var orientation = Quaternion.LookRotation(fw, up);
                Vector3 xAxis = Vector3.up;
                Vector3 yAxis = Vector3.right;
                Vector3 pos = livingBranchNodes[i].getPosition();
                pos += orientation * xAxis * (radius * Mathf.Sin(v * vStep));
                pos += orientation * yAxis * (radius * Mathf.Cos(v * vStep));

                vertices[i * MESH_FACE_COUNT + v] = pos;

                var diff = pos - livingBranchNodes[i].getPosition();
                normals[i * MESH_FACE_COUNT + v] = diff / diff.magnitude;

                float uvID = Remap(i, 0, nodes.Count - 1, 0, 1);
                uv[i * MESH_FACE_COUNT + v] = new Vector2((float)v / MESH_FACE_COUNT, uvID);
            }

            if (i + 1 < nodes.Count) {
                for (int v = 0; v < MESH_FACE_COUNT; v++) {
                    triangles[i * MESH_FACE_COUNT * 6 + v * 6] = ((v + 1) % MESH_FACE_COUNT) + i * MESH_FACE_COUNT;
                    triangles[i * MESH_FACE_COUNT * 6 + v * 6 + 1] = triangles[i * MESH_FACE_COUNT * 6 + v * 6 + 4] = v + i * MESH_FACE_COUNT;
                    triangles[i * MESH_FACE_COUNT * 6 + v * 6 + 2] = triangles[i * MESH_FACE_COUNT * 6 + v * 6 + 3] = ((v + 1) % MESH_FACE_COUNT + MESH_FACE_COUNT) + i * MESH_FACE_COUNT;
                    triangles[i * MESH_FACE_COUNT * 6 + v * 6 + 5] = (MESH_FACE_COUNT + v % MESH_FACE_COUNT) + i * MESH_FACE_COUNT;
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
        _currentSegmentIndex++;
        _line.positionCount = _currentSegmentIndex+1;
        _lastPosition = position;
        _line.SetPosition(_currentSegmentIndex, position);
        
        livingBranchNodes.Add(new IvyNode(position, normal));
        Mesh mesh = livingMesh.meshFilter.mesh;
        var nodeCount = livingBranchNodes.Count;
        mesh.vertices = new Vector3[nodeCount * MESH_FACE_COUNT * 4];
        mesh.normals = new Vector3[nodeCount * MESH_FACE_COUNT * 4];
        mesh.uv = new Vector2[nodeCount * MESH_FACE_COUNT * 4];
        mesh.triangles = new int[(nodeCount - 1) * MESH_FACE_COUNT * 6];
        livingMesh.mesh = mesh;
        
        ApplyMesh(mesh);
    }

    public void UpdateIvyNodes()
    {
        Mesh mesh = livingMesh.meshFilter.mesh;
        Vector3[] vertices = mesh.vertices;
        Vector3[] normals = mesh.normals;
        Vector2[] uv = mesh.uv;
        int[] triangles = mesh.triangles;
        var nodeCount = livingBranchNodes.Count;
        for (int i = 0; i < nodeCount; i++)
        {
            var radius = defaultRadius;
            if (i > nodeCount - _maxLivingNodeCount)
            {
                radius -= ((float)(i - (nodeCount - _maxLivingNodeCount)) / _maxLivingNodeCount) * defaultRadius;
            }

            float vStep = (2f * Mathf.PI) / MESH_FACE_COUNT;

            var fw = Vector3.zero;
            if (i > 0)
            {
                fw = livingBranchNodes[i - 1].getPosition() - livingBranchNodes[i].getPosition();
            }

            if (i < livingBranchNodes.Count - 1)
            {
                fw += livingBranchNodes[i].getPosition() - livingBranchNodes[i + 1].getPosition();
            }

            if (fw == Vector3.zero)
            {
                fw = Vector3.forward;
            }

            fw.Normalize();

            var up = livingBranchNodes[i].getNormal();
            up.Normalize();

            for (int v = 0; v < MESH_FACE_COUNT; v++)
            {
                var orientation = Quaternion.LookRotation(fw, up);
                Vector3 xAxis = Vector3.up;
                Vector3 yAxis = Vector3.right;
                Vector3 pos = livingBranchNodes[i].getPosition();
                pos += orientation * xAxis * (radius * Mathf.Sin(v * vStep));
                pos += orientation * yAxis * (radius * Mathf.Cos(v * vStep));

                vertices[i * MESH_FACE_COUNT + v] = pos;

                var diff = pos - livingBranchNodes[i].getPosition();
                normals[i * MESH_FACE_COUNT + v] = diff / diff.magnitude;

                float uvID = Remap(i, i, nodeCount - 1, 0, 1);
                uv[i * MESH_FACE_COUNT + v] = new Vector2((float)v / MESH_FACE_COUNT, uvID);
            }

            if (i + 1 < livingBranchNodes.Count)
            {
                for (int v = 0; v < MESH_FACE_COUNT; v++)
                {
                    triangles[i * MESH_FACE_COUNT * 6 + v * 6] = ((v + 1) % MESH_FACE_COUNT) + i * MESH_FACE_COUNT;
                    triangles[i * MESH_FACE_COUNT * 6 + v * 6 + 1] =
                        triangles[i * MESH_FACE_COUNT * 6 + v * 6 + 4] = v + i * MESH_FACE_COUNT;
                    triangles[i * MESH_FACE_COUNT * 6 + v * 6 + 2] = triangles[i * MESH_FACE_COUNT * 6 + v * 6 + 3] =
                        ((v + 1) % MESH_FACE_COUNT + MESH_FACE_COUNT) + i * MESH_FACE_COUNT;
                    triangles[i * MESH_FACE_COUNT * 6 + v * 6 + 5] = (MESH_FACE_COUNT + v % MESH_FACE_COUNT) + i * MESH_FACE_COUNT;
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
        livingMesh.meshFilter.mesh = mesh;
        livingMesh.meshFilter.sharedMesh.RecalculateBounds();
        livingMesh.meshFilter.sharedMesh.RecalculateNormals();
    }
    
    private float Remap(float input, float oldLow, float oldHigh, float newLow, float newHigh) {
        float t = Mathf.InverseLerp(oldLow, oldHigh, input);
        return Mathf.Lerp(newLow, newHigh, t);
    }
}