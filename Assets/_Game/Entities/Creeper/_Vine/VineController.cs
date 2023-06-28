using System.Collections.Generic;
using RootMath;
using UnityEngine;

public class VineController : MonoBehaviour
{
    private const int MESH_FACE_COUNT = 8;
    
    [Header("References")]
    public Transform head;
    public GameObject livingVineGameObject;
    
    public GameObject deadVinesParent;
    public GameObject deadVinePrefab;
    
    public GameObject branchesParent;
    public GameObject branchPrefab;

    public GameObject flowerParent;
    public GameObject flowerPrefab;
    
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
    [SerializeField] private int maxDeadNodeCount;
    [SerializeField] private int _deadNodeCount = 0;

    
    
    private Vector2 _inputDirection;
    public Vector2 InputDirection { set => _inputDirection = value; }
    
    private Vector3 _lastPosition;

    [SerializeField] private VineBranch currentBranch;
    [SerializeField] private float createBranchAfterSeconds = 4f;
    [SerializeField] private float createBranchInSecondes = 1f;
    [SerializeField] private float createBranchTime = 0f;

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
        
        InitNewDeadVineMesh();
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

        TryCreateBranch();
        
        _deadNodeCount++;
        if (_deadNodeCount > maxDeadNodeCount)
        {
            InitNewDeadVineMesh();
            _deadNodeCount = 0;
        }
        
        var newDeadNode = _livingVineNodes[0];
        _livingVineNodes.RemoveAt(0);
        var survivingLivingNode = _livingVineNodes[0];

        var deadMeshNodeCount = _currentNodeCount - _livingVineNodes.Count + 1;

        var vertexCount = deadMeshNodeCount * MESH_FACE_COUNT;
        var triangleCount = (deadMeshNodeCount - 1) * MESH_FACE_COUNT * 6;
        
        Mesh deadVineMesh = _deadVine.meshFilter.mesh;
        Vector3[] vertices = RMath.ExtendArray(deadVineMesh.vertices, vertexCount);
        int[] triangles = RMath.ExtendArray(deadVineMesh.triangles, triangleCount);
        Vector3[] normals = RMath.ExtendArray(deadVineMesh.normals, vertexCount);
        Vector2[] uv = RMath.ExtendArray(deadVineMesh.uv, vertexCount);
        
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

    private void TryCreateBranch()
    {
        if (currentBranch == null || currentBranch.isDone && createBranchTime >= createBranchAfterSeconds)
        {
            createBranchTime = 0f;
            currentBranch = Instantiate(branchPrefab, branchesParent.transform).GetComponent<VineBranch>();
            var position = _livingVineNodes[0].position;
            var up = _livingVineNodes[0].rotation * Vector3.up;
            var rotation = Random.Range(0f, 1f) > 0.5f 
                ? Quaternion.AngleAxis(Random.Range(270f, 340f), up)
                : Quaternion.AngleAxis(Random.Range(20f, 90.0f), up);
            rotation *= _livingVineNodes[0].rotation;
            var forward = rotation * Vector3.forward;
            currentBranch.CreateBranch(position, Quaternion.LookRotation(forward, up), defaultRadius, vineLength);
        }
    }

    private void InitNewDeadVineMesh()
    {
        var newDeadVine = Instantiate(deadVinePrefab, deadVinesParent.transform);
        _deadVine = new Vine()
        {
            meshFilter = newDeadVine.GetComponent<MeshFilter>(),
            meshRenderer = newDeadVine.GetComponent<MeshRenderer>()
        };
    }

    private void FixedUpdate()
    {
        if (_inputDirection.magnitude < 0.1f) return;
        
        var position = head.position;
        var distance = Vector3.Distance(_lastPosition, position);
        if (distance > vineLength)
        {
            AddNodeToVine(new VineNode(position, head.rotation));
            UpdateLivingMesh();
            UpdateFlowers();
        }
        else
        {
            var lastNode = _livingVineNodes[^1];
            var end = _livingVineNodes.Count - 1;
            var moveDirection = position - lastNode.position;

            UpdateBranch();
            for (var i = 1; i < end; i++)
            {
                if (_livingVineNodes[i].isFixed) continue;
            
                var moveFactor = (float)i / end;
                _livingVineNodes[i].position += moveDirection * moveFactor;
            }

            for (var i = 1; i < end; i++)
            {
                if (_livingVineNodes[i].isFixed && _livingVineNodes[i + 1].isFixed) continue;

                var nextForward = (_livingVineNodes[i + 1].position - _livingVineNodes[i].position).normalized;
                if (nextForward.sqrMagnitude == 0f) continue;
            
                _livingVineNodes[i].rotation = Quaternion.LookRotation(nextForward, _livingVineNodes[i].rotation * Vector3.up);
            }

            lastNode.position = head.position;
            lastNode.rotation = head.rotation;
        
            UpdateLivingMesh();
        }
    }

    private void UpdateBranch()
    {
        createBranchTime += Time.deltaTime;
        if (currentBranch != null && !currentBranch.isDone)
        {
            var value = createBranchTime / createBranchInSecondes;
            currentBranch.UpdateMesh(value);
        }
    }

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
            radius -= ((float)(i - (nodeCount - 1 - maxLivingNodeCount)) / maxLivingNodeCount) * defaultRadius;

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

                float uvID = RMath.Remap(i, 0, _livingVineNodes.Count-1, 0, 1);
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

    public Vector3 GetTipPosition()
    {
        return _livingVineNodes[^2].position;
    }
    
    public void FixAllNodes()
    {
        foreach (var node in _livingVineNodes)
        {
            node.isFixed = true;
        }
    }


    public List<Flower> _flowers = new List<Flower>();
    public void AddFlower()
    {
        var flower = Instantiate(flowerPrefab, flowerParent.transform).GetComponent<Flower>();
        flower.vineIndex = _livingVineNodes.Count;
        _flowers.Add(flower);
        UpdateFlowers();
    }

    private void UpdateFlowers()
    {
        for (int i = _flowers.Count - 1; i >= 0; i--)
        {
            var flower = _flowers[i];
            flower.vineIndex--;
            var flowerIndex = flower.vineIndex;
            if (flower.vineIndex >= 0)
            {
                var radius = defaultRadius * (1f - (float) flowerIndex / (_livingVineNodes.Count - 1));
                var offset = _livingVineNodes[flowerIndex].rotation * Vector3.up * (2f * radius - head.localScale.y * 0.5f);
                flower.transform.position = _livingVineNodes[flowerIndex].position + offset;
            }
            else
            {
                _flowers.RemoveAt(i);
            }
        }
    }
}
