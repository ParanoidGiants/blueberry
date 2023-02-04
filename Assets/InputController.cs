using UnityEngine;

namespace Creeper
{
    public class InputController : MonoBehaviour
    {
        public HeadController Head;

        private void Update()
        {
            var inpuHorizontal = Input.GetAxis("Horizontal");
            var inputVertical = Input.GetAxis("Vertical");
            var direction = new Vector3(inpuHorizontal, 0, inputVertical);

            if (Mathf.Abs(direction.x) > 0 || Mathf.Abs(direction.z) > 0)
            {
                if (direction.magnitude > 1f)
                {
                    direction.Normalize();
                }
                Head.UpdateHead(direction);
                Head.IsMoving = true;
            }
            else
            {
                Head.IsMoving = false;
            }
        }
    }
}
