using UnityEngine;
using UnityEngine.SceneManagement;

namespace Creeper
{
    public class InputController : MonoBehaviour
    {
        public HeadController Head;
        public BranchController Branch;

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
                if (!Head.IsRotating)
                {
                    Branch.UpdateBranch();
                }
                Head.IsMoving = true;
            }
            else
            {
                Head.IsMoving = false;
            }

            if (Input.GetKeyDown(KeyCode.R))
            {
                SceneManager.LoadScene(0);
            }
        }
    }
}
