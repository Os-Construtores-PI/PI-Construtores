using UnityEngine;
public class MiniJumpGroundScript : MonoBehaviour
{
    public bool jumped = false;

    private void OnTriggerEnter(Collider collider)
    {
        MakeJump(collider);
    }
    private void OnTriggerStay(Collider collider)
    {
        MakeJump(collider);
    }
    private void MakeJump(Collider collider)
    {
        if (collider.gameObject.CompareTag("Player"))
        {
            MiniPlayerControl MPC = collider.gameObject.GetComponent<MiniPlayerControl>();
            if (MPC.IsGrounded && !jumped && MPC.can_jump)
            {
                MPC.Jump();
                jumped = true;
            }

        }
    }
}
