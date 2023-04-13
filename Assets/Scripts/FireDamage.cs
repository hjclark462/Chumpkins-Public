// Authored by Harley Clark
using System.Collections;
using UnityEngine;

public class FireDamage : MonoBehaviour
{
    public Player player;
    public int damage;    
    public float delay;
    public float applyDamageNTimes;
    public float applyEveryNSeconds;    
    private int appliedTimes = 0;

    void Start()
    {
        StartCoroutine(Dps());       
    }
    private void OnTriggerEnter(Collider other)
    {
        // Checks to see if when the other character touches another if it can spread the fire further
        if (other.gameObject.CompareTag("Player"))
        {
            if (other.gameObject.layer == this.gameObject.layer) return;
                Player player = other.gameObject.GetComponentInParent<Player>();
            player.onFire = true;
            Collider[] hbs = player.gameObject.GetComponentsInChildren<Collider>();
            Collider hitBox = null;
            foreach (Collider hb in hbs)
            {
                if (hb.gameObject.tag == "HitBox")
                {
                    hitBox = hb;
                    break;
                }
            }

            if (hitBox.gameObject.GetComponent<FireDamage>() == null)
            {
                var transfer = hitBox.gameObject.AddComponent<FireDamage>();
                transfer.player = player;
                transfer.damage = damage;
                transfer.delay = 0;
                transfer.applyDamageNTimes = applyDamageNTimes - appliedTimes;
                transfer.applyEveryNSeconds = applyEveryNSeconds;                
            }
        }
    }    
    // Coroutine that either applies damage over time or if the correct amount of damage has been applied destroys the script.
    public IEnumerator Dps()
    {
        yield return new WaitForSeconds(delay);

        while (appliedTimes < applyDamageNTimes && GameManager.Instance.State != GameManager.GameState.EndGame)
        {   
            if(GameManager.Instance.State != GameManager.GameState.EndGame)
            AudioManager.Instance.PlaySound("Chumpkin Damaged");
            player.TakeDamage(damage);
            yield return new WaitForSeconds(applyEveryNSeconds);            
            appliedTimes++;
        }
        player.onFire = false;
        Destroy(this);
    }
}
