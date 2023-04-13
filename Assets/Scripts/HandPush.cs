// Authored by: Finn Davis
using UnityEngine;

public class HandPush : MonoBehaviour
{
    Player player;

    private void Awake()
    {
        player = GetComponentInParent<Player>();
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("HitBox"))
        {
            Player otherPlayer = other.gameObject.GetComponentInParent<Player>();
            if (player.pushing && otherPlayer != player)
            {
                player.pushing = false;
                AudioManager.Instance.PlaySound("Chumpkin is Pushed");
                if (otherPlayer.blocking)
                {
                    otherPlayer.PushBlocked();
                    otherPlayer.hipRB.AddForce((player.orientation.forward * player.pushStrength) * otherPlayer.blockStrength, ForceMode.Impulse);
                }
                else
                {
                    otherPlayer.hipRB.AddForce(player.orientation.forward * player.pushStrength, ForceMode.Impulse);
                }
                otherPlayer.GetEffect("Pushed").Play();
            }
        }
    }
}
