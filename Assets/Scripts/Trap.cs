// Authored by: Harley Clark
// Added to by: Finn Davis;
using UnityEngine;
// Base class to use for polymorphism with the four trap types
public class Trap : MonoBehaviour
{
    public int damageValue;
    public TrapType type;
    // So that the Audio Manager is aware which sound effect to play
    public enum TrapType
    {
        Spike,
        Saw,
        Fire,
        Laser
    }
    public virtual void ActivateTrap()
    {

    }
    public virtual void TrapEnterConsequence(Collider other)
    {

    }
    public virtual void TrapExitConsequence(Collider other)
    {

    }
}
