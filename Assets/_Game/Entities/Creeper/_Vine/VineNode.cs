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
    
    public VineNode(Vector3 position, Quaternion rotation, bool draw)
    {
        this.position = position;
        this.rotation = rotation;
        
        Debug.DrawRay(this.position, rotation * Vector3.right, Color.red, 3f);
        Debug.DrawRay(this.position, rotation * Vector3.up, Color.green, 3f);
        Debug.DrawRay(this.position, rotation * Vector3.forward, Color.blue, 3f);
    }
}