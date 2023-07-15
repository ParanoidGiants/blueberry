using System.Collections.Generic;
using UnityEngine;
using Utils;

namespace Creeper
{
    public class VineBranch : MonoBehaviour
    {
        public List<VineNode> nodes;
        public Vine vine;
        public int maxCountOfBranchNodes = 10;
        private float _segmentLength;
        private float _defaultRadius;

        public float currentDistance = 0;
        public bool isDone;
        private int _currentNodeIndex;

        public void CreateBranch(Vector3 position, Quaternion orientation, float radius)
        {
            _segmentLength = radius;
            _defaultRadius = radius;
            nodes = CreateBranchNodes(maxCountOfBranchNodes,position, orientation);
            var mesh = createMesh(nodes);
            vine = new Vine(GetComponent<MeshFilter>());
            vine.SetMesh(mesh);
            isDone = false;
        }

        public void UpdateMesh(float distance)
        {
            currentDistance = distance;
            if (currentDistance >= 1f)
            {
                currentDistance = 1f;
                isDone = true;
            }

            var mesh = vine.Mesh;
            var vertices = mesh.vertices;
            var uv = mesh.uv;
            var normals = mesh.normals;
            var triangles = mesh.triangles;
            
            var percentage = currentDistance;
            var midNodePercentage = percentage * (nodes.Count - 1);
            _currentNodeIndex = (int) Mathf.Floor(midNodePercentage);
            
            for (int i = 0; i < nodes.Count; i++)
            {
                var index = Mathf.Min(i, _currentNodeIndex);
                var factor = _currentNodeIndex == 0 ? 0f : (1f - (float) index / _currentNodeIndex);
                var radius = _defaultRadius * percentage * factor;
                float vStep = (2f * Mathf.PI) / Helper.MESH_FACE_COUNT;
                var offset = nodes[index].rotation * Vector3.up * (radius - _defaultRadius);
                
                for (int v = 0; v < Helper.MESH_FACE_COUNT; v++)
                {
                    Utils.Helper.SetVertex(
                        nodes[index],
                        offset,
                        i,
                        v,
                        radius,
                        Helper.MESH_FACE_COUNT,
                        vStep,
                        ref vertices,
                        ref normals,
                        ref uv
                    );
                }

                if (i + 1 >= nodes.Count) continue;
                
                for (int v = 0; v < Helper.MESH_FACE_COUNT; v++)
                {
                    Utils.Helper.SetTriangles(i, v, Helper.MESH_FACE_COUNT, ref triangles);
                }
            }
            
            mesh.vertices =  vertices;
            mesh.normals =  normals;
            mesh.triangles =  triangles;
            mesh.uv = uv;
            vine.SetMesh(mesh);
        }

        private float EPSILON = 0.001f;
        
        List<VineNode> CreateBranchNodes(int count, Vector3 pos, Quaternion orientation)
        {
            RaycastHit hit;
            Ray ray;
            float rayLength;
            Vector3 forward = orientation * Vector3.forward;
            Vector3 up = orientation * Vector3.up;

            if (count == maxCountOfBranchNodes)
            {
                // Search Below
                ray = new Ray(pos, -up);
                rayLength = _defaultRadius + EPSILON;
                if (Physics.Raycast(ray, out hit, rayLength, Utils.Helper.WHAT_IS_CLIMBABLE))
                {
                    pos = hit.point + up * _defaultRadius;
                }
                
                var rootNode = new VineNode(pos, orientation);
                return new List<VineNode> { rootNode }.Join(CreateBranchNodes(count - 1, pos, orientation));
            }
            
            if (count <= 0 || count > maxCountOfBranchNodes)
            {
                // end of recursion
                return null;
            }

            if (count % 2 == 0)
            {
                // change forward angle
                forward = Quaternion.AngleAxis(Random.Range(-20.0f, 20.0f), up) * forward;
            }
            var newOrientation = Quaternion.LookRotation(forward, up);

            // check in front
            ray = new Ray(pos, forward);
            rayLength = _segmentLength + _defaultRadius;
            Vector3 p1 = pos + forward * _segmentLength;
            if (Physics.Raycast(ray, out hit, rayLength, Utils.Helper.WHAT_IS_CLIMBABLE))
            {
                p1 = hit.point - forward * _defaultRadius;
                newOrientation = Quaternion.LookRotation(up, -forward);
                var middleOrientation = Quaternion.Lerp(orientation, newOrientation, 0.5f);
                VineNode p3Node = new VineNode(p1, middleOrientation);

                return new List<VineNode> { p3Node }.Join(CreateBranchNodes(count - 1, p1, newOrientation));
            }
            
            // check  in front + below
            Vector3 p2;
            ray = new Ray(p1, -up);
            rayLength = 2f * _defaultRadius;
            if (Physics.Raycast(ray, out hit, rayLength, Utils.Helper.WHAT_IS_CLIMBABLE))
            {
                p2 = hit.point + up * _defaultRadius;
                var p2Node = new VineNode(p2, newOrientation);
                return new List<VineNode> { p2Node }.Join(CreateBranchNodes(count - 1, p2, newOrientation));
            }
            
            Vector3 p3;
            Vector3 middle;
            Vector3 m0;
            Vector3 m1;
            VineNode m0Node;
            VineNode m1Node;
            
            // check  in front + below + below
            p2 = p1 + ray.direction * rayLength;
            ray = new Ray(p2, -forward);
            rayLength = _segmentLength;
            if (Physics.Raycast(ray, out hit, rayLength, Utils.Helper.WHAT_IS_CLIMBABLE))
            {
                p3 = hit.point + forward * _defaultRadius;
                newOrientation = Quaternion.LookRotation(-up, forward);
                VineNode p3Node = new VineNode(p3, newOrientation);

                if (!IsOccluded(p3, pos, up))
                {
                    return new List<VineNode> { p3Node }.Join(CreateBranchNodes(count - 1, p3, newOrientation));
                }
                
                middle = CalculateMiddlePoint(p3, pos, (up + forward) / 2);
                m0 = (pos + middle) / 2;
                m1 = (p3 + middle) / 2;
                var middleOrientation = Quaternion.Lerp(orientation, newOrientation, 0.5f);
                m0Node = new VineNode(m0, orientation);
                m1Node = new VineNode(m1, middleOrientation);
                return new List<VineNode> { m0Node, m1Node, p3Node }.Join(CreateBranchNodes(count - 3, p3, newOrientation));
            }
            return null;
        }

        private Vector3 ApplyCorrection(Vector3 p, Vector3 normal)
        {
            return p + normal * 0.01f;
        }

        private bool IsOccluded(Vector3 from, Vector3 to)
        {
            Ray ray = new Ray(from, (to - from) / (to - from).magnitude);
            return Physics.Raycast(ray, (to - from).magnitude);
        }

        private bool IsOccluded(Vector3 from, Vector3 to, Vector3 normal)
        {
            return IsOccluded(ApplyCorrection(from, normal), ApplyCorrection(to, normal));
        }
        
        Vector3 CalculateMiddlePoint(Vector3 p0, Vector3 p1, Vector3 normal)
        {
            Vector3 middle = (p0 + p1) / 2;
            var h = p0 - p1;
            var distance = h.magnitude;
            var dir = h / distance;
            return middle + normal * distance;
        }
        
        Mesh createMesh(List<VineNode> nodes) {
            Mesh branchMesh = new Mesh();
            branchMesh.vertices = new Vector3[(nodes.Count) * Helper.MESH_FACE_COUNT * 4];
            branchMesh.normals = new Vector3[nodes.Count * Helper.MESH_FACE_COUNT * 4];
            branchMesh.uv = new Vector2[nodes.Count * Helper.MESH_FACE_COUNT * 4];
            branchMesh.triangles = new int[(nodes.Count - 1) * Helper.MESH_FACE_COUNT * 6];
            
            return branchMesh;
        }
    }
}