using System.Collections.Generic;
using RootMath;
using UnityEngine;

public class VineController : MonoBehaviour
{
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
    [SerializeField] private int _maxLivingNodeCount = 30;
    [SerializeField] private float _vineLength = 0.1f;
    [SerializeField] private float _defaultRadius = 0.25f;
    
    [Space(10)]
    [Header("Watchers")]
    [SerializeField] private int _currentNodeCount = 0;
    [SerializeField] private List<VineNode> _livingVineNodes;
    [SerializeField] private Vine _livingVine;
    [SerializeField] private Vine _deadVine;
    [SerializeField] private int _maxDeadNodeCount = 200;
    [SerializeField] private int _deadNodeCount = 0;
    
    [SerializeField] private List<Flower> _flowers;

    [SerializeField] private VineBranch _currentBranch;
    [SerializeField] private float _createBranchAfterSeconds = 1f;
    [SerializeField] private float _createBranchInSeconds = 0.2f;
    [SerializeField] private float _createBranchTime = 0f;
    
    private Vector3 _lastPosition;
    private Vector2 _inputDirection;
    public Vector2 InputDirection { set => _inputDirection = value; }

    private void Start()
    {
        _livingVineNodes = new List<VineNode>();
        _flowers = new List<Flower>();
        
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
        livingMesh.vertices = new Vector3[livingNodeCount * Vine.MESH_FACE_COUNT * 4];
        livingMesh.normals = new Vector3[livingNodeCount * Vine.MESH_FACE_COUNT * 4];
        livingMesh.uv = new Vector2[livingNodeCount * Vine.MESH_FACE_COUNT * 4];
        livingMesh.triangles = new int[(livingNodeCount - 1) * Vine.MESH_FACE_COUNT * 6];
        _livingVine.SetMesh(livingMesh);
    }

    private void RefreshDeadMesh()
    {
        if (_livingVineNodes.Count <= _maxLivingNodeCount) return;

        TryCreateBranch();
        
        _deadNodeCount++;
        if (_deadNodeCount > _maxDeadNodeCount)
        {
            InitNewDeadVineMesh();
            _deadNodeCount = 0;
        }
        
        var newDeadNode = _livingVineNodes[0];
        _livingVineNodes.RemoveAt(0);
        var survivingLivingNode = _livingVineNodes[0];
        var deadMeshNodeCount = _currentNodeCount - _livingVineNodes.Count + 1;

        var vertexCount = deadMeshNodeCount * Vine.MESH_FACE_COUNT;
        var triangleCount = (deadMeshNodeCount - 1) * Vine.MESH_FACE_COUNT * 6;
        
        Mesh deadVineMesh = _deadVine.meshFilter.mesh;
        Vector3[] vertices = RMath.ExtendArray(deadVineMesh.vertices, vertexCount);
        int[] triangles = RMath.ExtendArray(deadVineMesh.triangles, triangleCount);
        Vector3[] normals = RMath.ExtendArray(deadVineMesh.normals, vertexCount);
        Vector2[] uv = RMath.ExtendArray(deadVineMesh.uv, vertexCount);
        
        var newDeadMeshNodeIndex = deadMeshNodeCount - 2;
        var orientation = newDeadNode.rotation;
        Vector3 offset = orientation * Vector3.up * (_defaultRadius - head.localScale.y * 0.5f);
        for (int v = 0; v < Vine.MESH_FACE_COUNT; v++)
        {
            RMath.SetVertex(
                newDeadNode,
                offset,
                newDeadMeshNodeIndex,
                v,
                _defaultRadius, 
                Vine.MESH_FACE_COUNT,
                Vine.V_STEP,
                ref vertices,
                ref normals,
                ref uv
            );
        }
        
        for (int v = 0; v < Vine.MESH_FACE_COUNT; v++)
        {
            RMath.SetTriangles(newDeadMeshNodeIndex, v, Vine.MESH_FACE_COUNT, ref triangles);
        }

        var survivingLivingNodeMeshIndex = deadMeshNodeCount - 1;
        orientation = survivingLivingNode.rotation;
        offset = orientation * Vector3.up * (_defaultRadius - head.localScale.y * 0.5f);
        for (int v = 0; v < Vine.MESH_FACE_COUNT; v++)
        {
            RMath.SetVertex(
                survivingLivingNode,
                offset,
                survivingLivingNodeMeshIndex,
                v,
                _defaultRadius, 
                Vine.MESH_FACE_COUNT,
                Vine.V_STEP,
                ref vertices,
                ref normals,
                ref uv
            );
        }
        
        deadVineMesh.vertices = vertices;
        deadVineMesh.normals = normals;
        deadVineMesh.triangles = triangles;
        deadVineMesh.uv = uv;

        _deadVine.SetMesh(deadVineMesh);
    }

    private void TryCreateBranch()
    {
        if (_currentBranch == null || _currentBranch.isDone && _createBranchTime >= _createBranchAfterSeconds)
        {
            _createBranchTime = 0f;
            _currentBranch = Instantiate(branchPrefab, branchesParent.transform).GetComponent<VineBranch>();
            var position = _livingVineNodes[0].position;
            var up = _livingVineNodes[0].rotation * Vector3.up;
            var rotation = Random.Range(0f, 1f) > 0.5f 
                ? Quaternion.AngleAxis(Random.Range(270f, 340f), up)
                : Quaternion.AngleAxis(Random.Range(20f, 90.0f), up);
            rotation *= _livingVineNodes[0].rotation;
            var forward = rotation * Vector3.forward;
            _currentBranch.CreateBranch(position, Quaternion.LookRotation(forward, up), _defaultRadius);
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
        if (distance > _vineLength)
        {
            AddNodeToVine(new VineNode(position, head.rotation));
            UpdateLivingMesh();
            UpdateFlowers();
        }
        else
        {
            UpdateBranch();
            var lastNode = _livingVineNodes[^1];
            var end = _livingVineNodes.Count - 1;
            var moveDirection = position - lastNode.position;

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
        _createBranchTime += Time.deltaTime;
        if (_currentBranch != null && !_currentBranch.isDone)
        {
            var value = _createBranchTime / _createBranchInSeconds;
            _currentBranch.UpdateMesh(value);
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
        
        float vStep = (2f * Mathf.PI) / Vine.MESH_FACE_COUNT;
        for (int i = 0; i < nodeCount; i++)
        {
            var livingVineNode = _livingVineNodes[i];
            var input = 1f - (float)i / nodeCount;
            var radius = RMath.Remap(input, 0f, 1f, 0f, _defaultRadius);
            
;
            // if (i != 0 && i + 1 < nodeCount)
            // {
            //     var forward = _livingVineNodes[i + 1].position - livingVineNode.position;
            //     if (forward.magnitude > 0.001f)
            //     {
            //         livingVineNode.rotation = Quaternion.LookRotation(forward);
            //     }
            // }
            
            var offset = livingVineNode.rotation * Vector3.up * (radius - head.localScale.y * 0.5f);
            var uvValue = (float)i / nodeCount;
            
            Debug.DrawRay(livingVineNode.position, offset, Color.blue, 1f);
            for (int v = 0; v < Vine.MESH_FACE_COUNT; v++)
            {
                RMath.SetVertex(
                    livingVineNode,
                    offset,
                    i,
                    v,
                    radius,
                    Vine.MESH_FACE_COUNT,
                    Vine.V_STEP,
                    ref vertices,
                    ref normals,
                    ref uv,
                    uvValue
                );
            }

            if (i + 1 < nodeCount)
            {
                for (int v = 0; v < Vine.MESH_FACE_COUNT; v++)
                {
                    RMath.SetTriangles(i, v, Vine.MESH_FACE_COUNT, ref triangles);
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
        return _livingVineNodes[^1].position;
    }
    
    public void FixAllNodes()
    {
        foreach (var node in _livingVineNodes)
        {
            node.isFixed = true;
        }
    }
    
    public void AddFlower()
    {
        var flower = Instantiate(flowerPrefab, flowerParent.transform).GetComponent<Flower>();
        flower.Init(_livingVineNodes.Count);
        _flowers.Add(flower);
        UpdateFlowers();
    }

    private void UpdateFlowers()
    {
        for (int i = _flowers.Count - 1; i >= 0; i--)
        {
            var flower = _flowers[i];
            if (flower.vineIndex >= 0)
            {
                var flowerVineIndex = flower.vineIndex;
                var rotation = _livingVineNodes[flowerVineIndex].rotation;
                var radius = _defaultRadius * (1f - (float) flowerVineIndex / (_livingVineNodes.Count - 1));
                var offset = rotation * Vector3.up * (2f * radius - head.localScale.y * 0.5f);
                var position = _livingVineNodes[flower.vineIndex].position + offset;
                flower.UpdateFlower(position, rotation);
            }
            else
            {
                _flowers.RemoveAt(i);
            }
        }
    }
}
