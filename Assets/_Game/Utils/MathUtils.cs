using System;
using UnityEngine;


namespace RootMath
{
    [Serializable]
    public class Axis
    {
        public Vector3 up;
        public Vector3 right;
        
        public Axis(Vector3 up, Vector3 right)
        {
            this.up = up;
            this.right = right;
        }
        
        public Axis(Vector3 position, Vector3 cameraPosition, Vector3 cameraRight, Vector3 cameraUp, Vector3 wallDirection)
        {
            right = ChatGPT.IntersectingLine(position, cameraRight, cameraPosition, wallDirection);
            up = ChatGPT.IntersectingLine(position, cameraUp, cameraPosition, wallDirection);
        }
    }

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

    public static class RMath
    {
        public static bool AreDirectionsConsideredEqual(Vector3 _direction1, Vector3 _directions)
        {
            return Vector3.Dot(_direction1, _directions) > 0.99f;
        }
        
        
    
        public static float Remap(float input, float oldLow, float oldHigh, float newLow, float newHigh)
        {
            float t = Mathf.InverseLerp(oldLow, oldHigh, input);
            return Mathf.Lerp(newLow, newHigh, t);
        }
        
        

        public static T[] ExtendArray<T>(T[] array, int newSize)
        {
            T[] newArray = new T[newSize];
            for (int i = 0; i < array.Length; i++)
            {
                newArray[i] = array[i];
            }

            return newArray;
        }
        
        public static int WHAT_IS_CLIMBABLE => LayerMask.GetMask("Climbable");
        public static bool IsLayerClimbable(int layer)
        {
            var colliderLayer = 1 << layer;
            var result = colliderLayer & WHAT_IS_CLIMBABLE;
            return result != 0;
        }

        public static bool IsLayerPlayer(int layer)
        {
            var colliderLayer = 1 << layer;
            var result = colliderLayer & LayerMask.GetMask("Player");
            return result != 0;
        }
        
        public static bool IsLayerPlayerPhysicsCollider(int layer)
        {
            var colliderLayer = 1 << layer;
            var result = colliderLayer & LayerMask.GetMask("PlayerPhysicsCollider");
            return result != 0;
        }
    }
}