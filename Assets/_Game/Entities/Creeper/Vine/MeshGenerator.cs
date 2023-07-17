using System.Collections.Generic;
using UnityEngine;

namespace Creeper
{
    public class MeshGenerator : MonoBehaviour
    {
        private const int MAX_DEAD_NODE_COUNT = 200;
        private const float CREATE_BRANCH_IN_SECONDS = 0.8f;
        
        [Header("References")]
        [SerializeField] private Transform _head;
        [SerializeField] private GameObject _livingVineGameObject;
        [SerializeField] private GameObject _deadVinesParent;
        [SerializeField] private GameObject _deadVinePrefab;
        [SerializeField] private GameObject _branchesParent;
        [SerializeField] private GameObject _branchPrefab;
        [SerializeField] private GameObject _flowerParent;
        [SerializeField] private GameObject _flowerPrefab;

        [Header("Settings")]
        [SerializeField] private int _maxLivingNodeCount = 30;
        [SerializeField] private float _vineLength = 0.1f;
        [SerializeField] private float _defaultRadius = 0.25f;

        [Space(10)]
        [Header("Watchers")]
        
        private List<VineNode> _livingVineNodes;
        private Vine _livingVine;
        private Vine _deadVine;
        [SerializeField] private int _currentNodeCount;
        [SerializeField] private int _deadNodeCount;

        private List<Flower> _flowers;
        
        private VineBranch _currentBranch;
        private float _createBranchTime;

        private Vector3 _lastPosition;
        private Vector2 _inputDirection;

        public Vector2 InputDirection
        {
            set => _inputDirection = value;
        }

        private void Start()
        {
            _livingVineNodes = new List<VineNode>();
            _flowers = new List<Flower>();

            var position = _head.position;
            _lastPosition = position;
            InitMeshCreation(position, _head.rotation);
        }

        private void InitMeshCreation(Vector3 position, Quaternion rotation)
        {
            _currentNodeCount = 0;
            var livingMeshFilter = _livingVineGameObject.GetComponent<MeshFilter>();
            var livingMeshRenderer = _livingVineGameObject.GetComponent<MeshRenderer>();
            _livingVine = new Vine(livingMeshFilter);
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
            Mesh livingMesh = _livingVine.Mesh;
            var livingNodeCount = _livingVineNodes.Count;
            livingMesh.vertices = new Vector3[livingNodeCount * Utils.Helper.MESH_FACE_COUNT * 4];
            livingMesh.normals = new Vector3[livingNodeCount * Utils.Helper.MESH_FACE_COUNT * 4];
            livingMesh.uv = new Vector2[livingNodeCount * Utils.Helper.MESH_FACE_COUNT * 4];
            livingMesh.triangles = new int[(livingNodeCount - 1) * Utils.Helper.MESH_FACE_COUNT * 6];
            _livingVine.SetMesh(livingMesh);
        }

        private void RefreshDeadMesh()
        {
            if (_livingVineNodes.Count <= _maxLivingNodeCount) return;

            TryCreateBranch();

            _deadNodeCount++;
            if (_deadNodeCount > MAX_DEAD_NODE_COUNT)
            {
                InitNewDeadVineMesh();
                _deadNodeCount = 0;
            }

            var newDeadNode = _livingVineNodes[0];
            _livingVineNodes.RemoveAt(0);
            var survivingLivingNode = _livingVineNodes[0];
            var deadMeshNodeCount = _currentNodeCount - _livingVineNodes.Count + 1;

            var vertexCount = deadMeshNodeCount * Utils.Helper.MESH_FACE_COUNT;
            var triangleCount = (deadMeshNodeCount - 1) * Utils.Helper.MESH_FACE_COUNT * 6;

            Mesh deadVineMesh = _deadVine.Mesh;
            Vector3[] vertices = Utils.Helper.ExtendArray(deadVineMesh.vertices, vertexCount);
            int[] triangles = Utils.Helper.ExtendArray(deadVineMesh.triangles, triangleCount);
            Vector3[] normals = Utils.Helper.ExtendArray(deadVineMesh.normals, vertexCount);
            Vector2[] uv = Utils.Helper.ExtendArray(deadVineMesh.uv, vertexCount);

            var newDeadMeshNodeIndex = deadMeshNodeCount - 2;
            var orientation = newDeadNode.rotation;
            Vector3 offset = orientation * Vector3.up * (_defaultRadius - _head.localScale.y * 0.5f);
            for (int v = 0; v < Utils.Helper.MESH_FACE_COUNT; v++)
            {
                Utils.Helper.SetVertex(
                    newDeadNode,
                    offset,
                    newDeadMeshNodeIndex,
                    v,
                    _defaultRadius,
                    Utils.Helper.MESH_FACE_COUNT,
                    Utils.Helper.V_STEP,
                    ref vertices,
                    ref normals,
                    ref uv
                );
            }

            for (int v = 0; v < Utils.Helper.MESH_FACE_COUNT; v++)
            {
                Utils.Helper.SetTriangles(newDeadMeshNodeIndex, v, Utils.Helper.MESH_FACE_COUNT, ref triangles);
            }

            var survivingLivingNodeMeshIndex = deadMeshNodeCount - 1;
            orientation = survivingLivingNode.rotation;
            offset = orientation * Vector3.up * (_defaultRadius - _head.localScale.y * 0.5f);
            for (int v = 0; v < Utils.Helper.MESH_FACE_COUNT; v++)
            {
                Utils.Helper.SetVertex(
                    survivingLivingNode,
                    offset,
                    survivingLivingNodeMeshIndex,
                    v,
                    _defaultRadius,
                    Utils.Helper.MESH_FACE_COUNT,
                    Utils.Helper.V_STEP,
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
            if (_currentBranch != null && !_currentBranch.isDone) return;

            _createBranchTime = 0f;
            _currentBranch = Instantiate(_branchPrefab, _branchesParent.transform).GetComponent<VineBranch>();
            var position = _livingVineNodes[0].position;
            var up = _livingVineNodes[0].rotation * Vector3.up;
            var rotation = Random.Range(0f, 1f) > 0.5f
                ? Quaternion.AngleAxis(Random.Range(270f, 340f), up)
                : Quaternion.AngleAxis(Random.Range(20f, 90.0f), up);
            rotation *= _livingVineNodes[0].rotation;
            var forward = rotation * Vector3.forward;
            _currentBranch.CreateBranch(position, Quaternion.LookRotation(forward, up), _defaultRadius);
        }

        private void InitNewDeadVineMesh()
        {
            var newDeadVine = Instantiate(_deadVinePrefab, _deadVinesParent.transform);
            _deadVine = new Vine(newDeadVine.GetComponent<MeshFilter>());
        }

        private void FixedUpdate()
        {
            if (_inputDirection.magnitude < 0.1f) return;

            var position = _head.position;
            var distance = Vector3.Distance(_lastPosition, position);
            if (distance > _vineLength)
            {
                AddNodeToVine(new VineNode(position, _head.rotation));
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

                    _livingVineNodes[i].rotation =
                        Quaternion.LookRotation(nextForward, _livingVineNodes[i].rotation * Vector3.up);
                }

                lastNode.position = _head.position;
                lastNode.rotation = _head.rotation;
                UpdateLivingMesh();
            }
        }

        private void UpdateBranch()
        {
            _createBranchTime += Time.deltaTime;
            if (_currentBranch != null && !_currentBranch.isDone)
            {
                var value = _createBranchTime / CREATE_BRANCH_IN_SECONDS;
                _currentBranch.UpdateMesh(value);
            }
        }

        private void UpdateLivingMesh()
        {
            Mesh livingVineMesh = _livingVine.Mesh;
            Vector3[] vertices = livingVineMesh.vertices;
            Vector3[] normals = livingVineMesh.normals;
            Vector2[] uv = livingVineMesh.uv;
            int[] triangles = livingVineMesh.triangles;
            var nodeCount = _livingVineNodes.Count;

            float vStep = (2f * Mathf.PI) / Utils.Helper.MESH_FACE_COUNT;
            for (int i = 0; i < nodeCount; i++)
            {
                var livingVineNode = _livingVineNodes[i];
                var input = 1f - (float)i / nodeCount;
                var radius = Utils.Helper.Remap(input, 0f, 1f, 0f, _defaultRadius);

                var offset = livingVineNode.rotation * Vector3.up * (radius - _head.localScale.y * 0.5f);
                var uvValue = (float)i / nodeCount;

                Debug.DrawRay(livingVineNode.position, offset, Color.blue, 1f);
                for (int v = 0; v < Utils.Helper.MESH_FACE_COUNT; v++)
                {
                    Utils.Helper.SetVertex(
                        livingVineNode,
                        offset,
                        i,
                        v,
                        radius,
                        Utils.Helper.MESH_FACE_COUNT,
                        Utils.Helper.V_STEP,
                        ref vertices,
                        ref normals,
                        ref uv,
                        uvValue
                    );
                }

                if (i + 1 < nodeCount)
                {
                    for (int v = 0; v < Utils.Helper.MESH_FACE_COUNT; v++)
                    {
                        Utils.Helper.SetTriangles(i, v, Utils.Helper.MESH_FACE_COUNT, ref triangles);
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
            var flower = Instantiate(_flowerPrefab, _flowerParent.transform).GetComponent<Flower>();
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
                    var radius = _defaultRadius * (1f - (float)flowerVineIndex / (_livingVineNodes.Count - 1));
                    var offset = rotation * Vector3.up * (2f * radius - _head.localScale.y * 0.5f);
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
}