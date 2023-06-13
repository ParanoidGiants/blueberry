using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

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
    
    [FormerlySerializedAs("_maxLivingNodeCount")]
    [Header("Settings")]
    [SerializeField] private int maxLivingNodeCount;
    [SerializeField] private float vineLength = 2f;
    [SerializeField] private float defaultRadius;
    
    [FormerlySerializedAs("deadMesh")]
    [Space(10)]
    [Header("Watchers")]
    public Vine livingVine;
    public Vine deadVine;
    public List<VineNode> livingVineNodes;
    public int currentNodeCount = 0;
    
    private Vector2 _inputDirection;
    public Vector2 InputDirection { set => _inputDirection = value; }
    
    private LineRenderer _debugLine;
    private Vector3 _lastPosition;

    private void Start()
    {
        var position = head.position;
        _lastPosition = position;
        _debugLine = GetComponent<LineRenderer>();
        var localScale = head.localScale;
        _debugLine.startWidth = localScale.x;
        _debugLine.endWidth = localScale.x;
        
        InitMeshCreation(position, head.up);
    }
    
    private void InitMeshCreation(Vector3 position, Vector3 normal)
    {
        currentNodeCount = 0;
        Debug.Log("Instantiated livingVineGO");
        var livingMeshFilter = livingVineGameObject.GetComponent<MeshFilter>();
        var livingMeshRenderer = livingVineGameObject.GetComponent<MeshRenderer>();
        livingVine = new Vine()
        {
            meshFilter = livingMeshFilter,
            meshRenderer = livingMeshRenderer
        };
        AddNodeToVine(new VineNode(position, normal));
        AddNodeToVine(new VineNode(position, normal));
        
        var deadMeshFilter = deadVineGameObject.GetComponent<MeshFilter>();
        var deadMeshRenderer = deadVineGameObject.GetComponent<MeshRenderer>();
        deadVine = new Vine()
        {
            meshFilter = deadMeshFilter,
            meshRenderer = deadMeshRenderer
        };
    }
    
    public void AddNodeToVine(VineNode vineNode)
    {
        currentNodeCount++;
        _lastPosition = vineNode.position;
        _debugLine.positionCount = currentNodeCount;
        _debugLine.SetPosition(currentNodeCount - 1, vineNode.position);
        
        if (currentNodeCount > maxLivingNodeCount)
        {
            var oldestNode = livingVineNodes[0];
            livingVineNodes.RemoveAt(0);
            UpdateDeadMesh(oldestNode);
        }
        
        livingVineNodes.Add(vineNode);
        Mesh mesh = livingVine.meshFilter.mesh;
        var livingNodeCount = livingVineNodes.Count;
        mesh.vertices = new Vector3[livingNodeCount * MESH_FACE_COUNT * 4];
        mesh.normals = new Vector3[livingNodeCount * MESH_FACE_COUNT * 4];
        mesh.uv = new Vector2[livingNodeCount * MESH_FACE_COUNT * 4];
        mesh.triangles = new int[(livingNodeCount - 1) * MESH_FACE_COUNT * 6];
        livingVine.SetMesh(mesh);
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
        
        var end = Mathf.Min(currentNodeCount, maxLivingNodeCount);
        var moveDirection = position - livingVineNodes[end-1].position;
        for (var i = 0; i < end; i++)
        {
            var moveFactor = (float)i / (end - 1);
            livingVineNodes[i].position += moveDirection * moveFactor;
            livingVineNodes[i].normal = transform.up;
        }
        UpdateLivingMesh();
    }

    private void UpdateLivingMesh()
    {
        Mesh livingVineMesh = livingVine.meshFilter.mesh;
        Vector3[] vertices = livingVineMesh.vertices;
        Vector3[] normals = livingVineMesh.normals;
        Vector2[] uv = livingVineMesh.uv;
        int[] triangles = livingVineMesh.triangles;
        var nodeCount = livingVineNodes.Count;
        for (int i = 0; i < nodeCount; i++)
        {
            var radius = defaultRadius;
            if (i > nodeCount - maxLivingNodeCount)
            {
                radius -= ((float)(i - (nodeCount - 1 - maxLivingNodeCount)) / maxLivingNodeCount) * defaultRadius;
            }

            float vStep = (2f * Mathf.PI) / MESH_FACE_COUNT;
            var fw = Vector3.zero;
            if (i > 0) {
                fw = livingVineNodes[i - 1].position - livingVineNodes[i].position;
            }

            if (i < livingVineNodes.Count - 1) {
                fw += livingVineNodes[i].position - livingVineNodes[i + 1].position;
            }

            if (fw == Vector3.zero) {
                fw = Vector3.forward;
            }

            fw.Normalize();

            var up = livingVineNodes[i].normal;
            up.Normalize();

            for (int v = 0; v < MESH_FACE_COUNT; v++)
            {
                var orientation = Quaternion.LookRotation(fw, up);
                Vector3 xAxis = Vector3.up;
                Vector3 yAxis = Vector3.right;
                Vector3 pos = livingVineNodes[i].position;
                pos += orientation * xAxis * (radius * Mathf.Sin(v * vStep));
                pos += orientation * yAxis * (radius * Mathf.Cos(v * vStep));

                vertices[i * MESH_FACE_COUNT + v] = pos;

                var diff = pos - livingVineNodes[i].position;
                normals[i * MESH_FACE_COUNT + v] = diff / diff.magnitude;

                float uvID = Remap(i, i, nodeCount - 1, 0, 1);
                uv[i * MESH_FACE_COUNT + v] = new Vector2((float)v / MESH_FACE_COUNT, uvID);
            }

            if (i + 1 < livingVineNodes.Count)
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
            
            _debugLine.SetPosition(i, livingVineNodes[i].position);
        }

        livingVineMesh.vertices = vertices;
        livingVineMesh.triangles = triangles;
        livingVineMesh.normals = normals;
        livingVineMesh.uv = uv;
        
        livingVine.SetMesh(livingVineMesh);
    }
    
    private void UpdateDeadMesh(VineNode oldestNode)
    {
        Mesh mesh = deadVine.meshFilter.mesh;

        var vertexCount = currentNodeCount * MESH_FACE_COUNT * 4;
        var triangleCount = (currentNodeCount - 1) * MESH_FACE_COUNT * 6;
        
        Vector3[] vertices = ExtendArray(mesh.vertices, vertexCount);
        Vector3[] normals = ExtendArray(mesh.normals, vertexCount);
        Vector2[] uv = ExtendArray(mesh.uv, vertexCount);
        int[] triangles = ExtendArray(mesh.triangles, triangleCount);

        int i = (int) Mathf.Max(0f, currentNodeCount - maxLivingNodeCount);
        float vStep = (2f * Mathf.PI) / MESH_FACE_COUNT;

        var fw = livingVineNodes[0].position - oldestNode.position;
        fw.Normalize();

        var up = oldestNode.normal;
        up.Normalize();

        for (int v = 0; v < MESH_FACE_COUNT; v++)
        {
            var orientation = Quaternion.LookRotation(fw, up);
            Vector3 xAxis = Vector3.up;
            Vector3 yAxis = Vector3.right;
            Vector3 pos = oldestNode.position;
            pos += orientation * xAxis * (defaultRadius * Mathf.Sin(v * vStep));
            pos += orientation * yAxis * (defaultRadius * Mathf.Cos(v * vStep));

            vertices[i * MESH_FACE_COUNT + v] = pos;

            var diff = pos - oldestNode.position;
            normals[i * MESH_FACE_COUNT + v] = diff / diff.magnitude;

            float uvID = Remap(i, i, i, 0, 1);
            uv[i * MESH_FACE_COUNT + v] = new Vector2((float)v / MESH_FACE_COUNT, uvID);
        }
        for (int v = 0; v < MESH_FACE_COUNT; v++)
        {
            triangles[i * MESH_FACE_COUNT * 6 + v * 6] = ((v + 1) % MESH_FACE_COUNT) + i * MESH_FACE_COUNT;
            triangles[i * MESH_FACE_COUNT * 6 + v * 6 + 1] =
                triangles[i * MESH_FACE_COUNT * 6 + v * 6 + 4] = v + i * MESH_FACE_COUNT;
            triangles[i * MESH_FACE_COUNT * 6 + v * 6 + 2] = triangles[i * MESH_FACE_COUNT * 6 + v * 6 + 3] =
                ((v + 1) % MESH_FACE_COUNT + MESH_FACE_COUNT) + i * MESH_FACE_COUNT;
            triangles[i * MESH_FACE_COUNT * 6 + v * 6 + 5] = (MESH_FACE_COUNT + v % MESH_FACE_COUNT) + i * MESH_FACE_COUNT;
        }
        
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.normals = normals;
        mesh.uv = uv;

        deadVine.meshFilter.mesh = mesh;
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