// Authored by: Harley Clark
using UnityEngine;
// Attached to the actual damaging sections of the trap prefabs to send the Trigger up the hierachy
public class TrapTrigger : MonoBehaviour
{
    [SerializeField]
    private Trap trap;
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("HitBox"))
        {            
            trap.TrapEnterConsequence(other); 
        }
    }
    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("HitBox"))
        {
            trap.TrapExitConsequence(other);
        }
    }
}
