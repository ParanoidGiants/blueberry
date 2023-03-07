using UnityEngine;
using UnityEngine.SceneManagement;

namespace Creeper
{
    public class InputController : MonoBehaviour
    {
        public HeadController Head;
        public BranchController Branch;
        public FollowTarget Camera;

        private void Update()
        {
            var inputHorizontal = Input.GetAxis("Horizontal");
            var inputVertical = Input.GetAxis("Vertical");
            var direction = new Vector2(inputHorizontal, inputVertical);

            if (Mathf.Abs(direction.x) > 0 || Mathf.Abs(direction.y) > 0)
            {
                if (direction.magnitude > 1f)
                {
                    direction.Normalize();
                }
                if (!Camera.IsRotating)
                {
                    Vector3 directionWorldSpace = Head.GetWorldDirection(direction);
                    Head.UpdateHead(directionWorldSpace);
                    Branch.UpdateBranch(directionWorldSpace);
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
