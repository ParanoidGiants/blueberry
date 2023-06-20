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
    private int _currentNodeCount = 0;
    
    private Vector2 _inputDirection;
    public Vector2 InputDirection { set => _inputDirection = value; }
    
    private Vector3 _lastPosition;

    private void Start()
    {
        _livingVineNodes = new List<VineNode>();
        
        var position = head.position;
        _lastPosition = position;
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
    }
    
    public void AddNodeToVine(VineNode vineNode)
    {
        _currentNodeCount++;
        _livingVineNodes.Add(vineNode);
        _lastPosition = vineNode.position;
        
        if (_currentNodeCount > maxLivingNodeCount)
        {
            RefreshDeadMesh();
            _livingVineNodes.RemoveAt(0);
        }
        
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
        Mesh deadVineMesh = _deadVine.meshFilter.mesh;
        var nextNodeToDie = _livingVineNodes[0];
        var oldestLivingNode = _livingVineNodes[1];
        
        var deadMeshNodeCount = (int) Mathf.Max(0f,_currentNodeCount - maxLivingNodeCount + 1);
        var vertexCount = (deadMeshNodeCount+1) * MESH_FACE_COUNT * 4;
        var triangleCount = deadMeshNodeCount * MESH_FACE_COUNT * 6;
        Vector3[] vertices = ExtendArray(deadVineMesh.vertices, vertexCount);
        Vector3[] normals = ExtendArray(deadVineMesh.normals, vertexCount);
        Vector2[] uv = ExtendArray(deadVineMesh.uv, vertexCount);
        int[] triangles = ExtendArray(deadVineMesh.triangles, triangleCount);
        
        float vStep = (2f * Mathf.PI) / MESH_FACE_COUNT;
        
        var meshIndex = deadMeshNodeCount;
        var oldMeshIndex = meshIndex - 1;
        
        var forward = (oldestLivingNode.position - nextNodeToDie.position).normalized;
        forward.Normalize();

        var up = oldestLivingNode.up;
        up.Normalize();

        for (int v = 0; v < MESH_FACE_COUNT; v++)
        {
            var orientation = Quaternion.LookRotation(forward, up);
            Vector3 xAxis = Vector3.up;
            Vector3 yAxis = Vector3.right;
            Vector3 offset = nextNodeToDie.up * (defaultRadius - head.localScale.y * 0.5f);
            Vector3 pos = nextNodeToDie.position + offset;
            Debug.DrawRay(nextNodeToDie.position, offset, Color.red, 1f);
            pos += orientation * xAxis * (defaultRadius * Mathf.Sin(v * vStep));
            pos += orientation * yAxis * (defaultRadius * Mathf.Cos(v * vStep));

            vertices[oldMeshIndex * MESH_FACE_COUNT + v] = pos;

            var diff = pos - nextNodeToDie.position;
            normals[oldMeshIndex * MESH_FACE_COUNT + v] = diff / diff.magnitude;

            float uvID = Remap(1, 0, 1, 0, 1);
            uv[oldMeshIndex * MESH_FACE_COUNT + v] = new Vector2((float)v / MESH_FACE_COUNT, uvID);
        }
        
        for (int v = 0; v < MESH_FACE_COUNT; v++)
        {
            triangles[oldMeshIndex * MESH_FACE_COUNT * 6 + v * 6] = ((v + 1) % MESH_FACE_COUNT) + oldMeshIndex * MESH_FACE_COUNT;
            triangles[oldMeshIndex * MESH_FACE_COUNT * 6 + v * 6 + 1] =
                triangles[oldMeshIndex * MESH_FACE_COUNT * 6 + v * 6 + 4] = v + oldMeshIndex * MESH_FACE_COUNT;
            triangles[oldMeshIndex * MESH_FACE_COUNT * 6 + v * 6 + 2] = triangles[oldMeshIndex * MESH_FACE_COUNT * 6 + v * 6 + 3] =
                ((v + 1) % MESH_FACE_COUNT + MESH_FACE_COUNT) + oldMeshIndex * MESH_FACE_COUNT;
            triangles[oldMeshIndex * MESH_FACE_COUNT * 6 + v * 6 + 5] = (MESH_FACE_COUNT + v % MESH_FACE_COUNT) + oldMeshIndex * MESH_FACE_COUNT;
        }
        
        for (int v = 0; v < MESH_FACE_COUNT; v++)
        {
            var orientation = Quaternion.LookRotation(forward, up);
            Vector3 xAxis = Vector3.up;
            Vector3 yAxis = Vector3.right;
            Vector3 offset = oldestLivingNode.up * (defaultRadius - head.localScale.y * 0.5f);
            Vector3 pos = oldestLivingNode.position + offset;
            Debug.DrawRay(oldestLivingNode.position, offset, Color.red, 1f);
            pos += orientation * xAxis * (defaultRadius * Mathf.Sin(v * vStep));
            pos += orientation * yAxis * (defaultRadius * Mathf.Cos(v * vStep));

            vertices[meshIndex * MESH_FACE_COUNT + v] = pos;

            var diff = pos - oldestLivingNode.position;
            normals[meshIndex * MESH_FACE_COUNT + v] = diff / diff.magnitude;

            float uvID = Remap(1, 0, 1, 0, 1);
            uv[meshIndex * MESH_FACE_COUNT + v] = new Vector2((float)v / MESH_FACE_COUNT, uvID);
        }
        
        deadVineMesh.vertices = vertices;
        deadVineMesh.triangles = triangles;
        deadVineMesh.normals = normals;
        deadVineMesh.uv = uv;

        _deadVine.SetMesh(deadVineMesh);
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
        }
        UpdateLivingMesh();
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
            if (i > 0)
            {
                forward = _livingVineNodes[i - 1].position - _livingVineNodes[i].position;
            }

            if (i < nodeCount - 1)
            {
                forward += _livingVineNodes[i].position - _livingVineNodes[i + 1].position;
            }

            if (forward == Vector3.zero)
            {
                forward = Vector3.forward;
            }

            forward.Normalize();

            var up = _livingVineNodes[i].up;
            up.Normalize();

            for (int v = 0; v < MESH_FACE_COUNT; v++)
            {
                var orientation = Quaternion.LookRotation(forward, up);
                Vector3 xAxis = Vector3.up;
                Vector3 yAxis = Vector3.right;
                Vector3 offset = _livingVineNodes[i].up * (radius - head.localScale.y * 0.5f);
                Vector3 position = _livingVineNodes[i].position + offset;
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
}