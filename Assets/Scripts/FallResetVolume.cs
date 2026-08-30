using UnityEngine;

// Put on a large trigger collider well below the map. Anything that falls through the world
// gets teleported back to its last grounded position instead of falling forever.
public class FallResetVolume : MonoBehaviour
{
    void OnTriggerEnter(Collider other)
    {
        PlayerController player = other.GetComponentInParent<PlayerController>();
        if (player != null)
            player.TeleportToLastGrounded();
    }
}
