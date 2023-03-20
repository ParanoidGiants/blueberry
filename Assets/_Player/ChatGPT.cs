using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class ChatGPT
{
    public static Vector3 IntersectingLine(
        Vector3 playerPosition,
        Vector3 cameraOffset,
        Vector3 cameraPosition,
        Vector3 collisionNormal
        )
    {
        var cameraWithOffset = cameraPosition + cameraOffset;
        var v = cameraWithOffset - playerPosition;
        var w = cameraPosition - playerPosition;
        // Calculate the normal vector of plane "T"
        Vector3 normalT = Vector3.Cross(v, w).normalized;

        // Calculate the direction vector of the intersecting line
        Vector3 direction = Vector3.Cross(normalT, collisionNormal);

        // Find a point on the intersecting line
        float d = Vector3.Dot(collisionNormal, playerPosition);
        float t = (d - Vector3.Dot(collisionNormal, cameraPosition)) / Vector3.Dot(collisionNormal, direction);
        Vector3 pointOnPlaneW = cameraPosition + t * direction;
        float distance = Vector3.Dot(normalT, pointOnPlaneW - playerPosition) / Vector3.Dot(normalT, normalT);
        Vector3 pointOnLine = playerPosition + distance * normalT;

        return direction;
    }
}
