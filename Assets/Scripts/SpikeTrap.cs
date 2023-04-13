// Authored by: Harley Clark
// Added to by: Finn Davis
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;

public class SpikeTrap : Trap
{
    public Animator spike;
    [SerializeField]
    private float delayTime;
    [SerializeField]
    private float engageTime = 0.1f;
    [SerializeField]
    private float holdTime = 1.5f;
    [SerializeField]
    private float withdrawTime = 1.5f;
    [SerializeField]
    ParticleSystem sparkParticle;
    [SerializeField]
    List<VisualEffect> spikeGlints;
    List<Player> playersOnTrap = new List<Player>();
    bool trapAllreadyActivating = false;

    private void Start()
    {
        // Sets the values in the animation
        spike.SetFloat("engageSpeed", 1 / engageTime);
        spike.SetFloat("holdSpeed", 1 / holdTime);
        spike.SetFloat("withdrawSpeed", 1 / withdrawTime);
    }
    public override void TrapEnterConsequence(Collider other)
    {        
        Player newPlayer = other.GetComponentInParent<Player>();
        // So that the trap doesn't trigger just by an arm/leg going over
        if (newPlayer != null)
        {
            playersOnTrap.Add(newPlayer);
        }
        if (!trapAllreadyActivating) StartCoroutine(SpikesUp());
    }
    public override void TrapExitConsequence(Collider other)
    {
        // Removes the pkayer so that the damage of an already activated trap doesn't 
        Player exitingPlayer = other.GetComponentInParent<Player>();
        if (exitingPlayer != null) playersOnTrap.Remove(exitingPlayer);
    }
    IEnumerator SpikesUp()
    {
        trapAllreadyActivating = true;
        yield return new WaitForSeconds(delayTime);
        trapAllreadyActivating = false;
        AudioManager.Instance.PlaySound("Spike Activation");
        spike.SetTrigger("engageTrigger");

        if (playersOnTrap.Count > 0)
        {
            foreach (Player p in playersOnTrap) p.TakeDamage(damageValue);
            AudioManager.Instance.PlaySound("Chumpkin Damaged");
        } 
       
        yield return new WaitForSeconds(3f);

        if (playersOnTrap.Count != 0) StartCoroutine(SpikesUp());
    }
    public void ToggleSparks()
    {
        if (sparkParticle.isPlaying) sparkParticle.Stop();
        else sparkParticle.Play();
        spikeGlints[Random.Range(0, spikeGlints.Count)].Play();
    }
}

