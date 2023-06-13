using System;
using UnityEngine;

[Serializable]
public class VineNode
{
    public Vector3 position;
    public Vector3 normal;
    public VineNode(Vector3 position, Vector3 normal)
    {
        this.position = position;
        this.normal = normal;
    }
}