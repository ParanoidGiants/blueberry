using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;

public class Branch : MonoBehaviour {
    const string AMOUNT = "_Amount";
    const string RADIUS = "_Radius";
    const float MAX = 0.5f;

    public int totalNodeCount = 0;
    public int maxLivelyNodeCount = 15;
    public List<IvyNode> branchNodes;

    Mesh livelyMesh;
    Material material;
    MeshFilter meshFilter; 
    MeshRenderer meshRenderer;

    Material leafMaterial;
    Material flowerMaterial;
    Blossom leafPrefab;
    Blossom flowerPrefab;
    bool wantBlossoms;
    Dictionary<int, Blossom> blossoms;

    public float branchRadius = 0.02f;
    int meshFaces = 8;

    bool animate;
    float growthSpeed = 2;
    float currentAmount = 0.5f;

    CollectableManager manager;
    int blossomCounter = 0;

    public void init(List<IvyNode> branchNodes, float branchRadius, Material material)
    {
        this.branchNodes = branchNodes;
        this.branchRadius = branchRadius;
        this.material = new Material(material);
        livelyMesh = createMesh(branchNodes);
    }

    public void init(List<IvyNode> branchNodes, float branchRadius, Material material, Material leafMaterial, Blossom leafPrefab, Material flowerMaterial, Blossom flowerPrefab, bool isFirst)
    {
        this.branchNodes = branchNodes;
        this.branchRadius = branchRadius;
        this.material = new Material(material);
        livelyMesh = createMesh(branchNodes);

        this.leafMaterial = leafMaterial;
        this.flowerMaterial = flowerMaterial;
        this.leafPrefab = leafPrefab;
        this.flowerPrefab = flowerPrefab;
        this.wantBlossoms = true;

        this.manager = FindObjectOfType<CollectableManager>();
        this.blossomCounter = manager.collectableCounter;

        blossoms = createBlossoms(branchNodes, isFirst);
    }

    public void initBaseMesh(Vector3 position, Vector3 normal, Material material)
    {
        this.meshFilter = GetComponent<MeshFilter>();


        this.branchNodes = new List<IvyNode>
        {
            new IvyNode(position, normal),
            new IvyNode(position + 2f * normal, normal)
        };
        this.material = material;
        this.meshRenderer = GetComponent<MeshRenderer>();
        this.meshRenderer.material = material;
        this.meshFilter.mesh = createMesh(branchNodes);

        material.SetFloat(RADIUS, 0f);
        material.SetFloat(AMOUNT, currentAmount);
    }

    float remap(float input, float oldLow, float oldHigh, float newLow, float newHigh) {
        float t = Mathf.InverseLerp(oldLow, oldHigh, input);
        return Mathf.Lerp(newLow, newHigh, t);
    }

    private Mesh createMesh(List<IvyNode> nodes) {
        Mesh branchMesh = new Mesh();
        Vector3[] vertices = new Vector3[(nodes.Count) * meshFaces * 4];
        Vector3[] normals = new Vector3[nodes.Count * meshFaces * 4];
        Vector2[] uv = new Vector2[nodes.Count * meshFaces * 4];
        int[] triangles = new int[(nodes.Count - 1) * meshFaces * 6];

        for (int i = 0; i < nodes.Count; i++) {
            float vStep = (2f * Mathf.PI) / meshFaces;

            var fw = Vector3.zero;
            if (i > 0) {
                fw = branchNodes[i - 1].getPosition() - branchNodes[i].getPosition();
            }

            if (i < branchNodes.Count - 1) {
                fw += branchNodes[i].getPosition() - branchNodes[i + 1].getPosition();
            }

            if (fw == Vector3.zero) {
                fw = Vector3.forward;
            }

            fw.Normalize();

            var up = branchNodes[i].getNormal();
            up.Normalize();

            for (int v = 0; v < meshFaces; v++) {
                var orientation = Quaternion.LookRotation(fw, up);
                Vector3 xAxis = Vector3.up;
                Vector3 yAxis = Vector3.right;
                Vector3 pos = branchNodes[i].getPosition();
                pos += orientation * xAxis * (branchRadius * Mathf.Sin(v * vStep));
                pos += orientation * yAxis * (branchRadius * Mathf.Cos(v * vStep));

                vertices[i * meshFaces + v] = pos;

                var diff = pos - branchNodes[i].getPosition();
                normals[i * meshFaces + v] = diff / diff.magnitude;

                float uvID = remap(i, 0, nodes.Count - 1, 0, 1);
                uv[i * meshFaces + v] = new Vector2((float)v / meshFaces, uvID);
            }

            if (i + 1 < nodes.Count) {
                for (int v = 0; v < meshFaces; v++) {
                    triangles[i * meshFaces * 6 + v * 6] = ((v + 1) % meshFaces) + i * meshFaces;
                    triangles[i * meshFaces * 6 + v * 6 + 1] = triangles[i * meshFaces * 6 + v * 6 + 4] = v + i * meshFaces;
                    triangles[i * meshFaces * 6 + v * 6 + 2] = triangles[i * meshFaces * 6 + v * 6 + 3] = ((v + 1) % meshFaces + meshFaces) + i * meshFaces;
                    triangles[i * meshFaces * 6 + v * 6 + 5] = (meshFaces + v % meshFaces) + i * meshFaces;
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
        branchNodes.Add(new IvyNode(position, normal));
        totalNodeCount++;
        if (branchNodes.Count >= maxLivelyNodeCount)
        {
            branchNodes.RemoveAt(0);
        }

        var nodeCount = branchNodes.Count;
        Mesh mesh = meshFilter.mesh;
        Vector3[] vertices = new Vector3[nodeCount * meshFaces * 4];
        Vector3[] normals = new Vector3[nodeCount * meshFaces * 4];
        Vector2[] uv = new Vector2[nodeCount * meshFaces * 4];
        int[] triangles = new int[(nodeCount - 1) * meshFaces * 6];

        for (int j = 0; j < mesh.vertices.Length; j++)
        {
            vertices[j] = mesh.vertices[j];
            normals[j] = mesh.normals[j];
            uv[j] = mesh.uv[j];
        }
        for (int j = 0; j < mesh.triangles.Length; j++)
        {
            triangles[j] = mesh.triangles[j];
        }

        int i = nodeCount - 1;
        float vStep = (2f * Mathf.PI) / meshFaces;

        var fw = Vector3.zero;
        if (i > 0)
        {
            fw = branchNodes[i - 1].getPosition() - branchNodes[i].getPosition();
        }

        if (i < branchNodes.Count - 1)
        {
            fw += branchNodes[i].getPosition() - branchNodes[i + 1].getPosition();
        }

        if (fw == Vector3.zero)
        {
            fw = Vector3.forward;
        }

        fw.Normalize();

        var up = branchNodes[i].getNormal();
        up.Normalize();

        for (int v = 0; v < meshFaces; v++)
        {
            var orientation = Quaternion.LookRotation(fw, up);
            Vector3 xAxis = Vector3.up;
            Vector3 yAxis = Vector3.right;
            Vector3 pos = branchNodes[i].getPosition();
            pos += orientation * xAxis * (branchRadius * Mathf.Sin(v * vStep));
            pos += orientation * yAxis * (branchRadius * Mathf.Cos(v * vStep));

            vertices[i * meshFaces + v] = pos;

            var diff = pos - branchNodes[i].getPosition();
            normals[i * meshFaces + v] = diff / diff.magnitude;

            float uvID = remap(i, 0, nodeCount - 1, 0, 1);
            uv[i * meshFaces + v] = new Vector2((float)v / meshFaces, uvID);
        }

        if (i + 1 < nodeCount)
        {
            for (int v = 0; v < meshFaces; v++)
            {
                triangles[i * meshFaces * 6 + v * 6] = ((v + 1) % meshFaces) + i * meshFaces;
                triangles[i * meshFaces * 6 + v * 6 + 1] = triangles[i * meshFaces * 6 + v * 6 + 4] = v + i * meshFaces;
                triangles[i * meshFaces * 6 + v * 6 + 2] = triangles[i * meshFaces * 6 + v * 6 + 3] = ((v + 1) % meshFaces + meshFaces) + i * meshFaces;
                triangles[i * meshFaces * 6 + v * 6 + 5] = (meshFaces + v % meshFaces) + i * meshFaces;
            }
        }

        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.normals = normals;
        mesh.uv = uv;
        this.meshFilter.mesh = mesh;
    }

    public bool HasPosition(Vector3 position)
    {
        return branchNodes.Any(x => x.getPosition() == position);
    }

    public void SetIvyNode(Vector3 branchPosition)
    {
        var segmentIndex = branchNodes.Count - 1;
        branchNodes[segmentIndex].position = branchPosition;
        branchNodes[segmentIndex].normal = transform.up;
    }

    public void UpdateIvyNodes()
    {
        Mesh mesh = this.meshFilter.mesh;
        Vector3[] vertices = mesh.vertices;
        Vector3[] normals = mesh.normals;
        Vector2[] uv = mesh.uv;
        int[] triangles = mesh.triangles;
        //int i = branchNodes.Count - 1;
        //float vStep = (2f * Mathf.PI) / meshFaces;
        //var fw = Vector3.zero;
        //if (i > 0)
        //{
        //    fw = this.branchNodes[i - 1].getPosition() - this.branchNodes[i].getPosition();
        //}

        //if (i < this.branchNodes.Count - 1)
        //{
        //    fw += this.branchNodes[i].getPosition() - this.branchNodes[i + 1].getPosition();
        //}

        //if (fw == Vector3.zero)
        //{
        //    fw = Vector3.forward;
        //}

        //fw.Normalize();

        //var up = this.branchNodes[i].getNormal();
        //up.Normalize();

        //for (int v = 0; v < meshFaces; v++)
        //{
        //    var orientation = Quaternion.LookRotation(fw, up);
        //    Vector3 xAxis = Vector3.up;
        //    Vector3 yAxis = Vector3.right;
        //    Vector3 pos = this.branchNodes[i].getPosition();
        //    pos += orientation * xAxis * (branchRadius * Mathf.Sin(v * vStep));
        //    pos += orientation * yAxis * (branchRadius * Mathf.Cos(v * vStep));

        //    vertices[i * meshFaces + v] = pos;

        //    var diff = pos - this.branchNodes[i].getPosition();
        //    normals[i * meshFaces + v] = diff / diff.magnitude;

        //    float uvID = remap(i, 0, branchNodes.Count - 1, 0, 1);
        //    uv[i * meshFaces + v] = new Vector2((float)v / meshFaces, uvID);
        //}

        //if (i + 1 < branchNodes.Count)
        //{
        //    for (int v = 0; v < meshFaces; v++)
        //    {
        //        triangles[i * meshFaces * 6 + v * 6] = ((v + 1) % meshFaces) + i * meshFaces;
        //        triangles[i * meshFaces * 6 + v * 6 + 1] = triangles[i * meshFaces * 6 + v * 6 + 4] = v + i * meshFaces;
        //        triangles[i * meshFaces * 6 + v * 6 + 2] = triangles[i * meshFaces * 6 + v * 6 + 3] = ((v + 1) % meshFaces + meshFaces) + i * meshFaces;
        //        triangles[i * meshFaces * 6 + v * 6 + 5] = (meshFaces + v % meshFaces) + i * meshFaces;
        //    }
        //}

        var minIndex = Mathf.Min(branchNodes.Count - 3, 0);
        for (int i = 0; i < this.branchNodes.Count; i++)
        {
            float vStep = (2f * Mathf.PI) / meshFaces;

            var fw = Vector3.zero;
            if (i > 0)
            {
                fw = this.branchNodes[i - 1].getPosition() - this.branchNodes[i].getPosition();
            }

            if (i < this.branchNodes.Count - 1)
            {
                fw += this.branchNodes[i].getPosition() - this.branchNodes[i + 1].getPosition();
            }

            if (fw == Vector3.zero)
            {
                fw = Vector3.forward;
            }

            fw.Normalize();

            var up = this.branchNodes[i].getNormal();
            up.Normalize();

            for (int v = 0; v < meshFaces; v++)
            {
                var orientation = Quaternion.LookRotation(fw, up);
                Vector3 xAxis = Vector3.up;
                Vector3 yAxis = Vector3.right;
                Vector3 pos = this.branchNodes[i].getPosition();
                pos += orientation * xAxis * (branchRadius * Mathf.Sin(v * vStep));
                pos += orientation * yAxis * (branchRadius * Mathf.Cos(v * vStep));

                vertices[i * meshFaces + v] = pos;

                var diff = pos - this.branchNodes[i].getPosition();
                normals[i * meshFaces + v] = diff / diff.magnitude;

                float uvID = remap(i, i, this.branchNodes.Count - 1, 0, 1);
                uv[i * meshFaces + v] = new Vector2((float)v / meshFaces, uvID);
            }

            if (i + 1 < this.branchNodes.Count)
            {
                for (int v = 0; v < meshFaces; v++)
                {
                    triangles[i * meshFaces * 6 + v * 6] = ((v + 1) % meshFaces) + i * meshFaces;
                    triangles[i * meshFaces * 6 + v * 6 + 1] = triangles[i * meshFaces * 6 + v * 6 + 4] = v + i * meshFaces;
                    triangles[i * meshFaces * 6 + v * 6 + 2] = triangles[i * meshFaces * 6 + v * 6 + 3] = ((v + 1) % meshFaces + meshFaces) + i * meshFaces;
                    triangles[i * meshFaces * 6 + v * 6 + 5] = (meshFaces + v % meshFaces) + i * meshFaces;
                }
            }
        }
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.normals = normals;
        mesh.uv = uv;
        meshFilter.mesh = mesh;
    }

    Dictionary<int, Blossom> createBlossoms(List<IvyNode> nodes, bool isFirst)
    {
        Dictionary<int, Blossom> bls = new Dictionary<int, Blossom>();
        for (int i = 0; i < nodes.Count; i++) {
            var r = Random.Range(0, 10);
            if (i > 0 
                || 
                isFirst
                &&
                blossomCounter != 0
                ) 
            {

                if (r > 2) {
                    Vector3 n = nodes[i].getNormal();
                    Vector3 otherNormal = Vector3.up;
                    Vector3 fw = Vector3.forward;
                    if (i > 0) {
                        fw = nodes[i - 1].getPosition() - nodes[i].getPosition();
                        otherNormal = nodes[i - 1].getNormal();
                    } else if (i < nodes.Count - 1) {
                        fw = nodes[i].getPosition() - nodes[i + 1].getPosition();
                        otherNormal = nodes[i + 1].getNormal();
                    }
                    // ísFlower
                    var isFlower = (r == 3) && Vector3.Dot(n, otherNormal) >= .95f;

                    var prefab = leafPrefab;
                    var material = leafMaterial;
                    if (isFlower 
                        &&
                        blossomCounter != 0
                        ) {
                        Debug.Log("Before: " + blossomCounter);
                        prefab = flowerPrefab;
                        material = flowerMaterial;
                        blossomCounter--;
                        Debug.Log("After: " + blossomCounter);
                    }

                    Quaternion rotation = Quaternion.LookRotation((fw).normalized, n);
                    float flowerOffset = isFlower ? 0.02f : 0;
                    float uvID = remap(i, 0, nodes.Count - 1, 0, 1);
                    Blossom b = Instantiate(prefab, nodes[i].getPosition() + nodes[i].getNormal() * (branchRadius + flowerOffset), rotation);
                    b.init(material);
                    b.transform.SetParent(transform);
                    bls.Add(i, b);
                }
            }
        }
        return bls;
    }   
}