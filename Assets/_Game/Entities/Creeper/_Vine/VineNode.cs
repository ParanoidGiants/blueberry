using System;
using UnityEngine;
using UnityEngine.Serialization;

[Serializable]
public class VineNode
{
    public Vector3 position;
    public Vector3 up;
    public VineNode(Vector3 position, Vector3 up)
    {
        this.position = position;
        this.up = up;
    }
}