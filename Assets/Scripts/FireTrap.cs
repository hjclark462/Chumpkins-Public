// Authored by: Harley Clark
// Added to by: Finn Davis
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;

public class FireTrap : Trap
{
    [SerializeField]
    private float timeBetweenDamage;
    [SerializeField]
    private float timesDamageDealt;
    [SerializeField]
    private int damageOverTime;
    [SerializeField]
    private List<VisualEffect> fires;
    public ParticleSystem embers;
    private Collider damageCollider;
    public MeshRenderer meshRenderer;
   
    public float warmupDuration;
    float currentWarmupDuration;

    public float activeDuration;
    float currentActiveDuration;

    public float cooldownDuration;
    float currentCooldownDuration;

    bool startWarmup;
    bool active;
    bool startCooldown;

    float heat;
    private void Start()
    {
        // Resets the material so it doesn't overide other materials
        meshRenderer.sharedMaterial = new Material(meshRenderer.sharedMaterial);
        damageCollider = GetComponentInChildren<Collider>();
        damageCollider.enabled = false;
    }
    private void OnEnable()
    {
        startWarmup = true;
        foreach (VisualEffect effect in fires)
        {
            effect.Stop();
        }
    }
    // Loops the on and off of the trap both visually and mechanically
    private void Update()
    {
        if (startWarmup)
        {
            startWarmup = false;
            currentWarmupDuration = warmupDuration;
        }
        if (currentWarmupDuration > 0)
        {
            currentWarmupDuration -= Time.deltaTime;
            heat = Mathf.Lerp(0.6f, 0, currentWarmupDuration / warmupDuration);
        }
        else if (currentActiveDuration == 0 && currentCooldownDuration == 0 && currentWarmupDuration <= 0)
        {
            currentWarmupDuration = 0;
            active = true;
        }

        if (active)
        {
            active = false;
            currentActiveDuration = activeDuration;
            Activate();
        }
        if (currentActiveDuration > 0)
        {
            currentActiveDuration -= Time.deltaTime;
            heat = Mathf.Lerp(1, 0.6f, currentActiveDuration / activeDuration);
        }
        else if (currentWarmupDuration == 0 && currentCooldownDuration == 0 && currentActiveDuration <= 0)
        {
            currentActiveDuration = 0;
            startCooldown = true;
        }

        if (startCooldown)
        {
            startCooldown = false;
            currentCooldownDuration = cooldownDuration;
            Deactivate();
        }
        if (currentCooldownDuration > 0)
        {
            currentCooldownDuration -= Time.deltaTime;
            heat = Mathf.Lerp(0, 1, currentCooldownDuration / cooldownDuration);
        }
        else if (currentWarmupDuration == 0 && currentActiveDuration == 0 && currentCooldownDuration <= 0)
        {
            currentCooldownDuration = 0;
            startWarmup = true;
        }

        if (!embers.isPlaying && heat > 0.4f)
        {
            embers.Play();
        }
        else if (embers.isPlaying && heat < 0.4f)
        {
            embers.Stop();
        }

        meshRenderer.sharedMaterial.SetFloat("_FireGlow", heat);
    }

    private void Activate()
    {
        AudioManager.Instance.PlaySound("Fire Activation");
        foreach (VisualEffect effect in fires)
        {
            effect.Play();
        }
        damageCollider.enabled = true;
    }
    private void Deactivate()
    {
        foreach (VisualEffect effect in fires)
        {
            effect.Stop();
        }
        damageCollider.enabled = false;
    }
    // Gives the initial damage while applying a FireDamage script to the player that acts as a Damage Over Time.
    public override void TrapEnterConsequence(Collider other)
    {
        if (GameManager.Instance.State == GameManager.GameState.EndGame) return;

        Player player = other.GetComponentInParent<Player>();
        player.onFire = true;
        player.TakeDamage(damageValue);
        AudioManager.Instance.PlaySound("On Fire");
        AudioManager.Instance.PlaySound("Chumpkin Damaged");
        if (other.gameObject.GetComponent<FireDamage>() == null)
        {
            var dot = other.gameObject.AddComponent<FireDamage>();
            dot.player = player;
            dot.damage = damageOverTime;
            dot.applyEveryNSeconds = timeBetweenDamage;
            dot.applyDamageNTimes = timesDamageDealt;
            dot.delay = timeBetweenDamage;            
        }
    }
}
