using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


namespace Utils
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
    }

    public static class Helper
    {
        
        private static int _MESH_FACE_COUNT = 8;
        public static int MESH_FACE_COUNT => _MESH_FACE_COUNT;
        
        private static float _V_STEP = (2f * Mathf.PI) / _MESH_FACE_COUNT;
        public static float V_STEP => _V_STEP;
        
        public static bool AreDirectionsConsideredEqual(Vector3 direction1, Vector3 direction2)
        {
            return Vector3.Dot(direction1, direction2) > 0.99f;
        }

        public static float Remap(float input, float oldLow, float oldHigh, float newLow, float newHigh)
        {
            float t = Mathf.InverseLerp(oldLow, oldHigh, input);
            return Mathf.Lerp(newLow, newHigh, t);
        }
        
        // This method was generated with ChatGPT
        private static Vector3 IntersectingLine(
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

        // this method is derived from https://github.com/ToughNutToCrack/ProceduralIvy
        public static void SetVertex(
            VineNode vineNode,
            Vector3 offset,
            int i,
            int v,
            float radius,
            int meshFaceCount,
            float vStep,
            ref Vector3[] vertices,
            ref Vector3[] normals,
            ref Vector2[] uv,
            float uvValueY = 0f
        ) {
            var vertexIndex = i * meshFaceCount + v;
            var position = vineNode.position;
            position += vineNode.rotation * Vector3.up * (radius * Mathf.Sin(v * vStep));
            position += vineNode.rotation * Vector3.right * (radius * Mathf.Cos(v * vStep));
            vertices[vertexIndex] = position + offset;
            var diff = position - vineNode.position;
            normals[vertexIndex] = diff / diff.magnitude;
            uv[vertexIndex] = new Vector2(0, uvValueY);
        }
    
        // this method is derived from https://github.com/ToughNutToCrack/ProceduralIvy
        public static void SetTriangles(int i, int v, int meshFaceCount, ref int[] triangles)
        {
            var baseTriangleIndex = i * meshFaceCount * 6 + v * 6;
            triangles[baseTriangleIndex] = ((v + 1) % meshFaceCount) + i * meshFaceCount;
            triangles[baseTriangleIndex + 1] = triangles[baseTriangleIndex + 4] = v + i * meshFaceCount;
            triangles[baseTriangleIndex + 2] = triangles[baseTriangleIndex + 3]
                = ((v + 1) % meshFaceCount + meshFaceCount) + i * meshFaceCount;
            triangles[baseTriangleIndex + 5] = (meshFaceCount + v % meshFaceCount) + i * meshFaceCount;
        }
        
        // this method is copied from https://github.com/ToughNutToCrack/ProceduralIvy
        public static List<T> Join<T>(this List<T> first, List<T> second) {
            if (first == null) {
                return second;
            }
            if (second == null) {
                return first;
            }
            return first.Concat(second).ToList();
        }
        
        public static Axis CreateMovementAxis(Transform head, Transform camera, Vector3 groundDirection)
        {
            var wallDirection = groundDirection;
            var position = head.position;
            var cameraPosition = camera.position;
            
            var up = IntersectingLine(position, camera.up, cameraPosition, wallDirection);
            var right = IntersectingLine(position, camera.right, cameraPosition, wallDirection);
            return new Axis(up, right);
        }
    }
}