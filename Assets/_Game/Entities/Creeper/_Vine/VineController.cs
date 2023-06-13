using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;

public class VineController : MonoBehaviour
{
    private const string AMOUNT = "_Amount";
    private const string RADIUS = "_Radius";
    private const int MESH_FACE_COUNT = 8;
    
    [Header("References")]
    public Transform head;
    public Material material;
    public GameObject livingVineGameObject;
    public GameObject deadVineGameObject;
    
    [Header("Settings")]
    [SerializeField] private int maxLivingNodeCount;
    [SerializeField] private float vineLength = 2f;
    [SerializeField] private float defaultRadius;
    
    [Space(10)]
    [Header("Watchers")]
    private int _maxDeadNodeCount = 3;
    private Vine _livingVine;
    private Vine _deadVine;
    [SerializeField] private List<VineNode> _livingVineNodes;
    [SerializeField] private List<VineNode> _deadVineNodes;
    private int _currentNodeCount = 0;
    
    private Vector2 _inputDirection;
    public Vector2 InputDirection { set => _inputDirection = value; }
    
    private LineRenderer _debugLine;
    private Vector3 _lastPosition;

    private void Start()
    {
        _debugLine = GetComponent<LineRenderer>();
        _livingVineNodes = new List<VineNode>();
        _deadVineNodes = new List<VineNode>();
        
        var position = head.position;
        _lastPosition = position;
        var localScale = head.localScale;
        _debugLine.startWidth = localScale.x;
        _debugLine.endWidth = localScale.x;
        
        InitMeshCreation(position, head.up);
    }
    
    private void InitMeshCreation(Vector3 position, Vector3 normal)
    {
        _currentNodeCount = 0;
        var livingMeshFilter = livingVineGameObject.GetComponent<MeshFilter>();
        var livingMeshRenderer = livingVineGameObject.GetComponent<MeshRenderer>();
        _livingVine = new Vine()
        {
            meshFilter = livingMeshFilter,
            meshRenderer = livingMeshRenderer
        };
        AddNodeToVine(new VineNode(position, normal));
        AddNodeToVine(new VineNode(position, normal));
        
        var deadMeshFilter = deadVineGameObject.GetComponent<MeshFilter>();
        var deadMeshRenderer = deadVineGameObject.GetComponent<MeshRenderer>();
        _deadVine = new Vine()
        {
            meshFilter = deadMeshFilter,
            meshRenderer = deadMeshRenderer
        };
        _deadVineNodes.Add(_livingVineNodes[0]);
    }
    
    public void AddNodeToVine(VineNode vineNode)
    {
        _currentNodeCount++;
        _livingVineNodes.Add(vineNode);
        _lastPosition = vineNode.position;
        _debugLine.positionCount = _currentNodeCount;
        _debugLine.SetPosition(_currentNodeCount - 1, vineNode.position);
        
        if (_currentNodeCount > maxLivingNodeCount)
        {
            _deadVineNodes.Add(_livingVineNodes[1]);
            if (_deadVineNodes.Count > _maxDeadNodeCount)
            {
                _deadVineNodes.RemoveAt(0);
            }
            RefreshDeadMesh();
            
            _livingVineNodes.RemoveAt(0);
        }
        
        Mesh mesh = _livingVine.meshFilter.mesh;
        var livingNodeCount = _livingVineNodes.Count;
        mesh.vertices = new Vector3[livingNodeCount * MESH_FACE_COUNT * 4];
        mesh.normals = new Vector3[livingNodeCount * MESH_FACE_COUNT * 4];
        mesh.uv = new Vector2[livingNodeCount * MESH_FACE_COUNT * 4];
        mesh.triangles = new int[(livingNodeCount - 1) * MESH_FACE_COUNT * 6];
        _livingVine.SetMesh(mesh);
    }
    
    private void Update()
    {
        if (_inputDirection.magnitude < 0.1f) return;
        
        var position = head.position;
        var distance = Vector3.Distance(_lastPosition, position);
        if (distance > vineLength)
        {
            AddNodeToVine(new VineNode(position, head.up));
        }
        
        var end = Mathf.Min(_currentNodeCount, maxLivingNodeCount);
        var moveDirection = position - _livingVineNodes[end-1].position;
        for (var i = 0; i < end; i++)
        {
            var moveFactor = (float)i / (end - 1);
            _livingVineNodes[i].position += moveDirection * moveFactor;
            _livingVineNodes[i].normal = transform.up;
        }
        UpdateLivingMesh();
        UpdateDeadMesh();
    }

    private void UpdateLivingMesh()
    {
        Mesh livingVineMesh = _livingVine.meshFilter.mesh;
        Vector3[] vertices = livingVineMesh.vertices;
        Vector3[] normals = livingVineMesh.normals;
        Vector2[] uv = livingVineMesh.uv;
        int[] triangles = livingVineMesh.triangles;
        var nodeCount = _livingVineNodes.Count;
        for (int i = 0; i < nodeCount; i++)
        {
            var radius = defaultRadius;
            if (i > nodeCount - maxLivingNodeCount)
            {
                radius -= ((float)(i - (nodeCount - 1 - maxLivingNodeCount)) / maxLivingNodeCount) * defaultRadius;
            }

            float vStep = (2f * Mathf.PI) / MESH_FACE_COUNT;
            var forward = Vector3.zero;
            if (i > 0) {
                forward = _livingVineNodes[i - 1].position - _livingVineNodes[i].position;
            }

            if (i < nodeCount - 1) {
                forward += _livingVineNodes[i].position - _livingVineNodes[i + 1].position;
            }

            if (forward == Vector3.zero) {
                forward = Vector3.forward;
            }

            forward.Normalize();

            var up = _livingVineNodes[i].normal;
            up.Normalize();

            for (int v = 0; v < MESH_FACE_COUNT; v++)
            {
                var orientation = Quaternion.LookRotation(forward, up);
                Vector3 xAxis = Vector3.up;
                Vector3 yAxis = Vector3.right;
                Vector3 position = _livingVineNodes[i].position - head.up * (head.localScale.y * 0.5f) + _livingVineNodes[i].normal * radius;
                position += orientation * xAxis * (radius * Mathf.Sin(v * vStep));
                position += orientation * yAxis * (radius * Mathf.Cos(v * vStep));

                vertices[i * MESH_FACE_COUNT + v] = position;

                var diff = position - _livingVineNodes[i].position;
                normals[i * MESH_FACE_COUNT + v] = diff / diff.magnitude;

                var valueToMap = _currentNodeCount - maxLivingNodeCount + nodeCount;
                float uvID = Remap(valueToMap, 0,  _currentNodeCount, 0, 1);
                uv[i * MESH_FACE_COUNT + v] = new Vector2((float)v / MESH_FACE_COUNT, uvID);
            }

            if (i + 1 < nodeCount)
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
            
            _debugLine.SetPosition(i, _livingVineNodes[i].position);
        }

        livingVineMesh.vertices = vertices;
        livingVineMesh.triangles = triangles;
        livingVineMesh.normals = normals;
        livingVineMesh.uv = uv;
        
        _livingVine.SetMesh(livingVineMesh);
    }
    
    private void UpdateDeadMesh()
    {
        if (_deadVineNodes.Count <= 1) return;
        Mesh deadVineMesh = _deadVine.meshFilter.mesh;
        
        Vector3[] vertices = deadVineMesh.vertices;
        Vector3[] normals = deadVineMesh.normals;
        Vector2[] uv = deadVineMesh.uv;

        var lastNodeIndex = _deadVineNodes.Count - 1;
        var lastMeshIndex = _currentNodeCount - maxLivingNodeCount;
        lastMeshIndex = Mathf.Max(0, lastMeshIndex);
        
        float vStep = (2f * Mathf.PI) / MESH_FACE_COUNT;
        var forward = _deadVineNodes[lastNodeIndex].position - _livingVineNodes[1].position;
        forward.Normalize();

        var up = _deadVineNodes[lastNodeIndex].normal;
        up.Normalize();

        for (int v = 0; v < MESH_FACE_COUNT; v++)
        {
            var orientation = Quaternion.LookRotation(forward, up);
            Vector3 xAxis = Vector3.up;
            Vector3 yAxis = Vector3.right;
            Vector3 pos = _deadVineNodes[lastNodeIndex].position - head.up * (head.localScale.y * 0.5f) + _deadVineNodes[lastNodeIndex].normal * defaultRadius;
            pos += orientation * xAxis * (defaultRadius * Mathf.Sin(v * vStep));
            pos += orientation * yAxis * (defaultRadius * Mathf.Cos(v * vStep));

            vertices[lastMeshIndex * MESH_FACE_COUNT + v] = pos;

            var diff = pos - _deadVineNodes[lastNodeIndex].position;
            normals[lastMeshIndex * MESH_FACE_COUNT + v] = diff / diff.magnitude;
            float uvID = Remap(lastNodeIndex, 0, _currentNodeCount, 0, 1);
            uv[lastMeshIndex * MESH_FACE_COUNT + v] = new Vector2((float)v / MESH_FACE_COUNT, uvID);
        }

        deadVineMesh.vertices = vertices;
        deadVineMesh.normals = normals;
        deadVineMesh.uv = uv;

        _deadVine.SetMesh(deadVineMesh);
    }
    
    private void RefreshDeadMesh()
    {
        Mesh deadVineMesh = _deadVine.meshFilter.mesh;
        
        var deadMeshNodeCount = _currentNodeCount - maxLivingNodeCount + 1;
        var vertexCount = deadMeshNodeCount * MESH_FACE_COUNT * 4;
        var triangleCount = (deadMeshNodeCount - 1) * MESH_FACE_COUNT * 6;
        
        Vector3[] vertices = ExtendArray(deadVineMesh.vertices, vertexCount);
        Vector3[] normals = ExtendArray(deadVineMesh.normals, vertexCount);
        Vector2[] uv = ExtendArray(deadVineMesh.uv, vertexCount);
        int[] triangles = ExtendArray(deadVineMesh.triangles, triangleCount);

        var meshIndex = Mathf.Max(0, deadMeshNodeCount - _maxDeadNodeCount);
        var nodeIndex = 0;
        for (int i = meshIndex; i < deadMeshNodeCount; i++, nodeIndex++)
        {
            float vStep = (2f * Mathf.PI) / MESH_FACE_COUNT;
            var forward = Vector3.zero;
            if (nodeIndex - 1 >= 0)
            {
                forward = _deadVineNodes[nodeIndex - 1].position - _deadVineNodes[nodeIndex].position;
            }
            
            if (nodeIndex + 1 < _deadVineNodes.Count)
            {
                forward += _deadVineNodes[nodeIndex].position - _deadVineNodes[nodeIndex + 1].position;
            }

            if (forward == Vector3.zero)
            {
                forward = Vector3.forward;
            }

            forward.Normalize();

            var up = _deadVineNodes[nodeIndex].normal;
            up.Normalize();

            for (int v = 0; v < MESH_FACE_COUNT; v++)
            {
                var orientation = Quaternion.LookRotation(forward, up);
                Vector3 xAxis = Vector3.up;
                Vector3 yAxis = Vector3.right;
                Vector3 pos = _deadVineNodes[nodeIndex].position - head.up * (head.localScale.y * 0.5f) + _deadVineNodes[nodeIndex].normal * defaultRadius;
                pos += orientation * xAxis * (defaultRadius * Mathf.Sin(v * vStep));
                pos += orientation * yAxis * (defaultRadius * Mathf.Cos(v * vStep));

                vertices[i * MESH_FACE_COUNT + v] = pos;

                var diff = pos - _deadVineNodes[nodeIndex].position;
                normals[i * MESH_FACE_COUNT + v] = diff / diff.magnitude;

                float uvID = Remap(nodeIndex, 0, _currentNodeCount, 0, 1);
                uv[i * MESH_FACE_COUNT + v] = new Vector2((float)v / MESH_FACE_COUNT, uvID);
            }

            if (i + 1 < deadMeshNodeCount)
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
        
        deadVineMesh.vertices = vertices;
        deadVineMesh.triangles = triangles;
        deadVineMesh.normals = normals;
        deadVineMesh.uv = uv;

        _deadVine.SetMesh(deadVineMesh);
    }
    
    private T[] ExtendArray<T>(T[] array, int newSize)
    {
        T[] newArray = new T[newSize];
        for (int i = 0; i < array.Length; i++)
        {
            newArray[i] = array[i];
        }

        return newArray;
    }
    
    private float Remap(float input, float oldLow, float oldHigh, float newLow, float newHigh)
    {
        float t = Mathf.InverseLerp(oldLow, oldHigh, input);
        return Mathf.Lerp(newLow, newHigh, t);
    }
}