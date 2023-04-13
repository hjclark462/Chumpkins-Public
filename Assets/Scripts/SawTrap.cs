//Authored by Harley Clark
using UnityEngine;

public class SawTrap : Trap
{
    [SerializeField]
    private Transform saw;
    [SerializeField]
    private Transform pointA;
    [SerializeField]
    private Transform pointB;
    bool swapDirection;
    [SerializeField]
    private float speed;
    [SerializeField]
    private Animator sawAnim;
    [SerializeField]
    private float maxSpinSpeed = 5f;    
    private float currentSpinUpTime;
    private float spinSpeed;
    public override void TrapEnterConsequence(Collider other)
    {
        Player player = other.GetComponentInParent<Player>();
        player.TakeDamage(damageValue);
        Debug.Log("SawTrapped");

        if (player.invincibool) return;

        AudioManager.Instance.PlaySound("Saw Damage");
        AudioManager.Instance.PlaySound("Chumpkin Damaged");
    }    
    private void Update()
    {
        // Gets the saw blade up and spinning upon activation
        currentSpinUpTime += Time.deltaTime;
        spinSpeed = Mathf.Lerp(0, maxSpinSpeed, SmoothStep(currentSpinUpTime));
        sawAnim.SetFloat("SpinSpeed", spinSpeed);
    }
    private void FixedUpdate()
    {
        // Handles the saw moving along the rail
        if (swapDirection)
        {
            saw.transform.position = Vector3.MoveTowards(saw.transform.position, pointB.position, speed);
            if (saw.transform.position == pointB.position)
            {
                swapDirection = false;
            }
        }
        else
        {
            saw.transform.position = Vector3.MoveTowards(saw.transform.position, pointA.position, speed);
            if (saw.transform.position == pointA.position)
            {
                swapDirection = true;
            }
        }
    }  
    private float SmoothStep(float x)
    {
        if (x < 0)
            return 0;

        if (x >= 1)
            return 1;
               
        return x * x * (3 - 2 * x);
    }
}
