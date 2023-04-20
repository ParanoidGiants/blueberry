using System;
using UnityEngine;

// TODO: create own namespace
[Serializable]
public class IvyNode {
    Vector3 position;
    Vector3 normal;

    public IvyNode(Vector3 position, Vector3 normal) {
        this.position = position;
        this.normal = normal;
    }

    public Vector3 getPosition() => position;
    public Vector3 getNormal() => normal;

    public Vector3 Position { set { position = value; } }
}