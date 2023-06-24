using System;
using UnityEngine;
using UnityEngine.Serialization;

[Serializable]
public class VineNode
{
    public Vector3 position;
    public Quaternion rotation;
    public bool isFixed = false;

    public VineNode(Vector3 position, Quaternion rotation)
    {
        this.position = position;
        this.rotation = rotation;
    }
}