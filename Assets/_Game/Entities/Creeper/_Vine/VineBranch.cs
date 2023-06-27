using System.Collections.Generic;
using Creeper;
using UnityEngine;
using Random = UnityEngine.Random;

public class VineBranch : MonoBehaviour
{
    private const int meshFaces = 8;
    private static int branchCount = 0;
    public List<VineNode> nodes;
    public Vine vine;
    
    public Material vineMaterial;
    public int maxCountOfBranchNodes = 10;
    public float segmentLength = 0.5f;
    public float defaultRadius;

    public float currentDistance = 0;
    public bool isDone;
    [SerializeField] private int node0;
    [SerializeField] private int node1;

    public void CreateBranch(Vector3 position, Quaternion orientation, float radius, float segmentLength)
    {
        this.segmentLength = radius;
        defaultRadius = radius;
        nodes = CreateBranchNodes(maxCountOfBranchNodes,position, orientation);
        var mesh = createMesh(nodes);
        vine.meshRenderer = GetComponent<MeshRenderer>();
        vine.meshFilter = GetComponent<MeshFilter>();
        vine.meshFilter.mesh = mesh;
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

        var mesh = vine.meshFilter.mesh;
        var vertices = mesh.vertices;
        var uv = mesh.uv;
        var normals = mesh.normals;
        var triangles = mesh.triangles;
        
        var percentage = currentDistance;
        var midNodePercentage = percentage * (nodes.Count - 1);
        node0 = (int) Mathf.Floor(midNodePercentage);
        node1 = (int) Mathf.Ceil(midNodePercentage);
        midNodePercentage %= 1;
        
        for (int i = 0; i < nodes.Count; i++)
        {
            var index = Mathf.Min(i, node0);
            var factor = node0 == 0 ? 0f : (1f - (float) index / node0);
            var radius = defaultRadius * percentage * factor;
            float vStep = (2f * Mathf.PI) / meshFaces;

            var orientation = nodes[index].rotation;
            var forward = orientation * Vector3.forward;
            var offset = orientation * Vector3.up * (radius - defaultRadius);
            Vector3 xAxis = Vector3.up;
            Vector3 yAxis = Vector3.right;
            Vector3 startPosition = index == 0 
                ? nodes[index].position
                : nodes[index - 1].position + forward * (segmentLength * midNodePercentage);
            startPosition += offset;
            
            for (int v = 0; v < meshFaces; v++)
            {
                var vertexIndex = i * meshFaces + v;
                var pos = startPosition;
                pos += orientation * xAxis * (radius * Mathf.Sin(v * vStep));
                pos += orientation * yAxis * (radius * Mathf.Cos(v * vStep));

                vertices[vertexIndex] = pos;

                var diff = pos - nodes[index].position;
                normals[vertexIndex] = diff / diff.magnitude;
                uv[vertexIndex] = new Vector2(0, 0);
            }

            if (i + 1 < nodes.Count)
            {
                if (i > index)
                {
                    for (int v = 0; v < meshFaces; v++)
                    {
                        var triangleIndex = i * meshFaces * 6 + v * 6;
                        triangles[triangleIndex] = ((v + 1) % meshFaces) + index * meshFaces;
                        triangles[triangleIndex + 1]
                            = triangles[triangleIndex + 4]
                            = v + index * meshFaces;
                        triangles[triangleIndex + 2]
                            = triangles[triangleIndex + 3]
                            = ((v + 1) % meshFaces + meshFaces) + index * meshFaces;
                        triangles[triangleIndex + 5] = (meshFaces + v % meshFaces) + index * meshFaces;
                    }
                }
                else
                {
                    for (int v = 0; v < meshFaces; v++)
                    {
                        var triangleIndex = i * meshFaces * 6 + v * 6;
                        triangles[triangleIndex] = ((v + 1) % meshFaces) + index * meshFaces;
                        triangles[triangleIndex + 1]
                            = triangles[triangleIndex + 4]
                            = v + index * meshFaces;
                        triangles[triangleIndex + 2]
                            = triangles[triangleIndex + 3]
                            = ((v + 1) % meshFaces + meshFaces) + index * meshFaces;
                        triangles[triangleIndex + 5] = (meshFaces + v % meshFaces) + index * meshFaces;
                    }
                }
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
            rayLength = defaultRadius + EPSILON;
            if (Physics.Raycast(ray, out hit, rayLength, HeadController.WHAT_IS_CLIMBABLE))
            {
                pos = hit.point + up * defaultRadius;
            }
            
            var rootNode = new VineNode(pos, orientation);
            return new List<VineNode> { rootNode }.join(CreateBranchNodes(count - 1, pos, orientation));
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
        rayLength = segmentLength + defaultRadius;
        Vector3 p1 = pos + forward * segmentLength;
        if (Physics.Raycast(ray, out hit, rayLength, HeadController.WHAT_IS_CLIMBABLE))
        {
            Debug.DrawLine(ray.origin, hit.point, Color.black, 3f);
            p1 = hit.point - forward * defaultRadius;
            newOrientation = Quaternion.LookRotation(up, -forward);
            var middleOrientation = Quaternion.Lerp(orientation, newOrientation, 0.5f);
            VineNode p3Node = new VineNode(p1, middleOrientation, true);

            return new List<VineNode> { p3Node }.join(CreateBranchNodes(count - 1, p1, newOrientation));
        }
        
        // check  in front + below
        Vector3 p2;
        ray = new Ray(p1, -up);
        rayLength = 2f * defaultRadius;
        if (Physics.Raycast(ray, out hit, rayLength, HeadController.WHAT_IS_CLIMBABLE))
        {
            Debug.DrawLine(ray.origin, hit.point, Color.gray, 3f);
            p2 = hit.point + up * defaultRadius;
            var p2Node = new VineNode(p2, newOrientation, true);
            return new List<VineNode> { p2Node }.join(CreateBranchNodes(count - 1, p2, newOrientation));
        }
        else
        {
            Debug.DrawRay(ray.origin, ray.direction * rayLength, Color.gray, 3f);
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
        rayLength = segmentLength;
        if (Physics.Raycast(ray, out hit, rayLength, HeadController.WHAT_IS_CLIMBABLE))
        {
            Debug.DrawLine(ray.origin, hit.point, Color.yellow, 3f);
            p3 = hit.point + forward * defaultRadius;
            newOrientation = Quaternion.LookRotation(-up, forward);
            VineNode p3Node = new VineNode(p3, newOrientation, true);

            if (!IsOccluded(p3, pos, up))
            {
                return new List<VineNode> { p3Node }.join(CreateBranchNodes(count - 1, p3, newOrientation));
            }
            
            middle = CalculateMiddlePoint(p3, pos, (up + forward) / 2);
            m0 = (pos + middle) / 2;
            m1 = (p3 + middle) / 2;
            var middleOrientation = Quaternion.Lerp(orientation, newOrientation, 0.5f);
            m0Node = new VineNode(m0, orientation, true);
            m1Node = new VineNode(m1, middleOrientation, true);
            return new List<VineNode> { m0Node, m1Node, p3Node }.join(CreateBranchNodes(count - 3, p3, newOrientation));
        }
        Debug.DrawRay(ray.origin, ray.direction * rayLength, Color.magenta, 3f);
        return null;
        /*
        
        // check in front + below + behind
        Vector3 p4;
        VineNode p4Node;
        p3 = p2;
        ray = new Ray(p3, -forward);
        rayLength = segmentLength;
        if (Physics.Raycast(ray, out hit, rayLength, HeadController.WHAT_IS_CLIMBABLE))
        {
            Debug.DrawLine(ray.origin, hit.point, Color.magenta, 3f);
            newOrientation = Quaternion.LookRotation(-up, forward);
            p4 = hit.point + forward * defaultRadius;
            p4Node = new VineNode(p4, newOrientation, true);
            if (!IsOccluded(p4, pos, up))
            {
                Debug.Log("4 Count: " + count);
                return new List<VineNode> { p4Node }.join(CreateBranchNodes(count - 1, p4, newOrientation));
            }

            middle = CalculateMiddlePoint(p4, pos, (up + forward) / 2);
            m0 = (pos + middle) / 2;
            m1 = (p4 + middle) / 2;
            // m0Node = new VineNode(m0, Quaternion.LookRotation(forward), true);
            m0Node = new VineNode(m0, Quaternion.LookRotation(forward));
            // m1Node = new VineNode(m1, Quaternion.LookRotation(forward), true);
            m1Node = new VineNode(m1, Quaternion.LookRotation(forward));

            Debug.Log("5 Count: " + count);
            return new List<VineNode> { m0Node, m1Node, p4Node }.join(CreateBranchNodes(count - 3, p4, newOrientation));
        }
        
        p4 = p3 - up * segmentLength;
        // p4Node = new VineNode(p4, Quaternion.LookRotation(-up), true);
        p4Node = new VineNode(p4, Quaternion.LookRotation(-up));
        if (!IsOccluded(p4, pos, up))
        {
            Debug.Log("6 Count: " + count);
            return new List<VineNode> { p4Node }.join(CreateBranchNodes(count - 1, p4, forward, -up));
        }
        
        middle = CalculateMiddlePoint(p4, pos, (up + forward) / 2);
        m0 = (pos + middle) / 2;
        m1 = (p4 + middle) / 2;
        m0Node = new VineNode(m0, Quaternion.LookRotation(-up), true);
        // m0Node = new VineNode(m0, Quaternion.LookRotation(-up));
        m1Node = new VineNode(m1, Quaternion.LookRotation(-up), true);
        // m1Node = new VineNode(m1, Quaternion.LookRotation(-up));

        Debug.Log("7 Count: " + count);
        return new List<VineNode> { m0Node, m1Node, p4Node }.join(CreateBranchNodes(count - 3, p4, forward, -up));
         * 
         */
    }

    Vector3 FindTangentFromArbitraryNormal(Vector3 normal)
    {
        Vector3 t1 = Vector3.Cross(normal, Vector3.forward);
        Vector3 t2 = Vector3.Cross(normal, Vector3.up);
        if (t1.magnitude > t2.magnitude) {
            return t1;
        }
        return t2;
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
        branchMesh.vertices = new Vector3[(nodes.Count) * meshFaces * 4];
        branchMesh.normals = new Vector3[nodes.Count * meshFaces * 4];
        branchMesh.uv = new Vector2[nodes.Count * meshFaces * 4];
        branchMesh.triangles = new int[(nodes.Count - 1) * meshFaces * 6];
        
        return branchMesh;
    }
}
