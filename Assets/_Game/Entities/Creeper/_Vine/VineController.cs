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
    public GameObject livingVineGameObject;
    public GameObject deadVineGameObject;
    
    [Header("Settings")]
    [SerializeField] private int maxLivingNodeCount;
    [SerializeField] private float vineLength = 2f;
    [SerializeField] private float defaultRadius;
    
    [Space(10)]
    [Header("Watchers")]
    private Vine _livingVine;
    private Vine _deadVine;
    [SerializeField] private List<VineNode> _livingVineNodes;
    [SerializeField] private int _currentNodeCount = 0;
    
    private Vector2 _inputDirection;
    public Vector2 InputDirection { set => _inputDirection = value; }
    
    private Vector3 _lastPosition;

    private void Start()
    {
        _livingVineNodes = new List<VineNode>();
        
        var position = head.position;
        _lastPosition = position;
        InitMeshCreation(position, head.rotation);
    }
    
    private void InitMeshCreation(Vector3 position, Quaternion rotation)
    {
        _currentNodeCount = 0;
        var livingMeshFilter = livingVineGameObject.GetComponent<MeshFilter>();
        var livingMeshRenderer = livingVineGameObject.GetComponent<MeshRenderer>();
        _livingVine = new Vine()
        {
            meshFilter = livingMeshFilter,
            meshRenderer = livingMeshRenderer
        };
        AddNodeToVine(new VineNode(position, rotation));
        
        var deadMeshFilter = deadVineGameObject.GetComponent<MeshFilter>();
        var deadMeshRenderer = deadVineGameObject.GetComponent<MeshRenderer>();
        _deadVine = new Vine()
        {
            meshFilter = deadMeshFilter,
            meshRenderer = deadMeshRenderer
        };
    }
    
    public void AddNodeToVine(VineNode vineNode)
    {
        _currentNodeCount++;
        _lastPosition = vineNode.position;
        _livingVineNodes.Add(vineNode);
        RefreshDeadMesh();
        RefreshLivingMesh();
    }

    private void RefreshLivingMesh()
    {
        Mesh livingMesh = _livingVine.meshFilter.mesh;
        var livingNodeCount = _livingVineNodes.Count;
        livingMesh.vertices = new Vector3[livingNodeCount * MESH_FACE_COUNT * 4];
        livingMesh.normals = new Vector3[livingNodeCount * MESH_FACE_COUNT * 4];
        livingMesh.uv = new Vector2[livingNodeCount * MESH_FACE_COUNT * 4];
        livingMesh.triangles = new int[(livingNodeCount - 1) * MESH_FACE_COUNT * 6];
        _livingVine.SetMesh(livingMesh);
    }

    private void RefreshDeadMesh()
    {
        if (_livingVineNodes.Count <= maxLivingNodeCount) return;
        
        var newDeadNode = _livingVineNodes[0];
        _livingVineNodes.RemoveAt(0);
        var survivingLivingNode = _livingVineNodes[0];

        var deadMeshNodeCount = _currentNodeCount - _livingVineNodes.Count + 1;

        var vertexCount = deadMeshNodeCount * MESH_FACE_COUNT;
        var triangleCount = (deadMeshNodeCount - 1) * MESH_FACE_COUNT * 6;
        
        Mesh deadVineMesh = _deadVine.meshFilter.mesh;
        Vector3[] vertices = ExtendArray(deadVineMesh.vertices, vertexCount);
        int[] triangles = ExtendArray(deadVineMesh.triangles, triangleCount);
        Vector3[] normals = ExtendArray(deadVineMesh.normals, vertexCount);
        Vector2[] uv = ExtendArray(deadVineMesh.uv, vertexCount);
        
        float vStep = (2f * Mathf.PI) / MESH_FACE_COUNT;
        var newDeadMeshNodeIndex = deadMeshNodeCount - 2;
        
        var orientation = newDeadNode.rotation;
        Vector3 offset = orientation * Vector3.up * (defaultRadius - head.localScale.y * 0.5f);
        Vector3 xAxis = Vector3.up;
        Vector3 yAxis = Vector3.right;
        for (int v = 0; v < MESH_FACE_COUNT; v++)
        {
            var vertexIndex = newDeadMeshNodeIndex * MESH_FACE_COUNT + v;
            var position = newDeadNode.position;
            position += orientation * xAxis * (defaultRadius * Mathf.Sin(v * vStep));
            position += orientation * yAxis * (defaultRadius * Mathf.Cos(v * vStep));

            vertices[vertexIndex] = position + offset;

            var diff = position - newDeadNode.position;
            normals[vertexIndex] = diff / diff.magnitude;

            uv[vertexIndex] = new Vector2(0, 0);
        }
        
        for (int v = 0; v < MESH_FACE_COUNT; v++)
        {
            var baseTriangleIndex = newDeadMeshNodeIndex * MESH_FACE_COUNT * 6 + v * 6;
            triangles[baseTriangleIndex] 
                = ((v + 1) % MESH_FACE_COUNT) + newDeadMeshNodeIndex * MESH_FACE_COUNT;
            triangles[baseTriangleIndex + 1]
                = triangles[baseTriangleIndex + 4]
                = v + newDeadMeshNodeIndex * MESH_FACE_COUNT;
            
            triangles[baseTriangleIndex + 2]
                = triangles[baseTriangleIndex + 3]
                = ((v + 1) % MESH_FACE_COUNT + MESH_FACE_COUNT) + newDeadMeshNodeIndex * MESH_FACE_COUNT;
            triangles[baseTriangleIndex + 5] = (MESH_FACE_COUNT + v % MESH_FACE_COUNT) + newDeadMeshNodeIndex * MESH_FACE_COUNT;
        }

        var survivingLivingNodeMeshIndex = deadMeshNodeCount - 1;
        orientation = survivingLivingNode.rotation;
        offset = orientation * Vector3.up * (defaultRadius - head.localScale.y * 0.5f);
        for (int v = 0; v < MESH_FACE_COUNT; v++)
        {
            var vertexIndex = survivingLivingNodeMeshIndex * MESH_FACE_COUNT + v;
            var position = survivingLivingNode.position;
            position += orientation * xAxis * (defaultRadius * Mathf.Sin(v * vStep));
            position += orientation * yAxis * (defaultRadius * Mathf.Cos(v * vStep));

            vertices[vertexIndex] = position + offset;

            var diff = position - survivingLivingNode.position;
            normals[vertexIndex] = diff / diff.magnitude;

            uv[vertexIndex] = new Vector2(0, 0);
        }
        
        deadVineMesh.vertices = vertices;
        deadVineMesh.normals = normals;
        deadVineMesh.triangles = triangles;
        deadVineMesh.uv = uv;

        _deadVine.SetMesh(deadVineMesh);
    }

    private void FixedUpdate()
    {
        if (_inputDirection.magnitude < 0.1f) return;
        
        var position = head.position;
        var distance = Vector3.Distance(_lastPosition, position);
        if (distance > vineLength)
        {
            AddNodeToVine(new VineNode(position, head.rotation));
        }

        var lastNode = _livingVineNodes[^1];
        var end = _livingVineNodes.Count - 1;
        var moveDirection = position - lastNode.position;
        for (var i = 1; i < end; i++)
        {
            var moveFactor = (float)i / end;
            _livingVineNodes[i].position += moveDirection * moveFactor;
        }

        for (var i = 1; i < end; i++)
        {
            var nextForward = (_livingVineNodes[i + 1].position - _livingVineNodes[i].position).normalized;
            if (nextForward.sqrMagnitude == 0f) continue;
            
            _livingVineNodes[i].rotation = Quaternion.LookRotation(nextForward, _livingVineNodes[i].rotation * Vector3.up);
        }

        lastNode.position = head.position;
        lastNode.rotation = head.rotation;
        
        UpdateLivingMesh();
    }

    private const double EPSILON = 0.0001f;

    private void UpdateLivingMesh()
    {
        Mesh livingVineMesh = _livingVine.meshFilter.mesh;
        Vector3[] vertices = livingVineMesh.vertices;
        Vector3[] normals = livingVineMesh.normals;
        Vector2[] uv = livingVineMesh.uv;
        int[] triangles = livingVineMesh.triangles;
        var nodeCount = _livingVineNodes.Count;
        
        float vStep = (2f * Mathf.PI) / MESH_FACE_COUNT;
        for (int i = 0; i < nodeCount; i++)
        {
            var livingVineNode = _livingVineNodes[i];
            var radius = defaultRadius;
            if (i > nodeCount - maxLivingNodeCount)
            {
                radius -= ((float)(i - (nodeCount - 1 - maxLivingNodeCount)) / maxLivingNodeCount) * defaultRadius;
            }


            var orientation = livingVineNode.rotation;
            
            if (i != 0 && i + 1 < nodeCount)
            {
                var forward = _livingVineNodes[i + 1].position - livingVineNode.position;
                if (forward.magnitude > 0.001f)
                {
                    orientation = Quaternion.LookRotation(forward);
                }
            }
            var offset = livingVineNode.rotation * Vector3.up * (radius - head.localScale.y * 0.5f);
            var xAxis = Vector3.up;
            var yAxis = Vector3.right;
            var defaultPosition = livingVineNode.position;
            for (int v = 0; v < MESH_FACE_COUNT; v++)
            {
                var vertexIndex = i * MESH_FACE_COUNT + v;
                var position = defaultPosition;
                position += orientation * xAxis * (radius * Mathf.Sin(v * vStep));
                position += orientation * yAxis * (radius * Mathf.Cos(v * vStep));
                vertices[vertexIndex] = position + offset;
                
                var diff = position - _livingVineNodes[i].position;
                normals[vertexIndex] = diff / diff.magnitude;

                float uvID = Remap(i, 0, _livingVineNodes.Count-1, 0, 1);
                uv[vertexIndex] = new Vector2((float)v / MESH_FACE_COUNT, uvID);
            }

            if (i + 1 < nodeCount)
            {
                for (int v = 0; v < MESH_FACE_COUNT; v++)
                {
                    var baseTriangleIndex = i * MESH_FACE_COUNT * 6 + v * 6;
                    triangles[baseTriangleIndex] = ((v + 1) % MESH_FACE_COUNT) + i * MESH_FACE_COUNT;
                    triangles[baseTriangleIndex + 1] = triangles[baseTriangleIndex + 4] = v + i * MESH_FACE_COUNT;
                    triangles[baseTriangleIndex + 2] = triangles[baseTriangleIndex + 3]
                        = ((v + 1) % MESH_FACE_COUNT + MESH_FACE_COUNT) + i * MESH_FACE_COUNT;
                    triangles[baseTriangleIndex + 5] = (MESH_FACE_COUNT + v % MESH_FACE_COUNT) + i * MESH_FACE_COUNT;
                }
            }
        }

        livingVineMesh.vertices = vertices;
        livingVineMesh.triangles = triangles;
        livingVineMesh.normals = normals;
        livingVineMesh.uv = uv;
        
        _livingVine.SetMesh(livingVineMesh);
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

    public Vector3 GetTipPosition()
    {
        return _livingVineNodes[^2].position;
    }
}