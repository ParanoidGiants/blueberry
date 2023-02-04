using UnityEngine;

namespace Creeper
{
    public class HeadController : MonoBehaviour
    {
        public float MoveSpeed;
        public void UpdateHead(Vector3 directionLocalSpace)
        {
            var directionWorldSpace = transform.right * directionLocalSpace.x + transform.forward * directionLocalSpace.z;
            transform.position += MoveSpeed * directionWorldSpace;
        }
    }
}
