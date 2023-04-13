// Authored by: Finn Davis
using UnityEngine;

public class GroundedCheck : MonoBehaviour
{
    public Player player;

    private void OnCollisionStay(Collision collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("Ground") || collision.gameObject.layer == LayerMask.NameToLayer("Trap")) player.TimeOfLastGrounded = Time.time;
    }
}
