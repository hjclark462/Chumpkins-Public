// Authored by: Finn Davis
using UnityEngine;

public class SpikeTrapParticleToggle : MonoBehaviour
{
    SpikeTrap spikeTrap;
    private void Awake()
    {
        spikeTrap = GetComponentInParent<SpikeTrap>();
    }
    public void ToggleSparks() => spikeTrap.ToggleSparks();
}
