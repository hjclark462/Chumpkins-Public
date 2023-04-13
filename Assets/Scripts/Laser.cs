// Authored by: Finn Davis
using UnityEngine;

public class Laser : Trap
{
    [Header("Properties")]
    public float laserWidth;
    public float laserLength;
    public float scanLeft;
    public float scanRight;
    public float scanSpeed;
    public StartDirection startDirection;

    [Header("Dependencies")]
    public GameObject cylinder;
    public Material laserBeam;

    private bool disableGizmoUpdate;
    Vector3 left;
    Vector3 right;
    Vector3 target;
    #region Enum
    [System.Serializable]
    public enum StartDirection
    {
        Left,
        Right
    }
    #endregion
    public override void TrapEnterConsequence(Collider other)
    {
        Player player = other.GetComponentInParent<Player>();
        player.TakeDamage(damageValue);
        Debug.Log("LaserTrapped");
    }
    private void Awake()
    {
        left = transform.position + Vector3.left * scanLeft;
        right = transform.position + Vector3.right * scanRight;

        if (startDirection == StartDirection.Left) target = left;
        else target = right;

        disableGizmoUpdate = true;
    }
    private void FixedUpdate()
    {
        if (transform.position == target) target = right;
        if (transform.position == target) target = left;
        transform.position = Vector3.MoveTowards(transform.position, target, scanSpeed * Time.deltaTime);
    }
    private void OnValidate()
    {
        if (laserLength < 0) laserLength = 0;
        laserBeam.SetFloat("_Length", laserLength);
        Vector3 endPosition = transform.position + transform.forward * laserLength;
        cylinder.transform.position = transform.position + transform.forward * (Vector3.Distance(transform.position, endPosition) / 2);
        cylinder.transform.localScale = new Vector3(laserWidth, Vector3.Distance(transform.position, endPosition) / 2, laserWidth);
    }
    private void OnDrawGizmos()
    {
        if (scanLeft < 0) scanLeft = 0;
        if (scanRight < 0) scanRight = 0;

        if (!disableGizmoUpdate)
        {
            left = transform.position + Vector3.left * scanLeft;
            right = transform.position + Vector3.right * scanRight;
        }

        Gizmos.color = Color.cyan;
        Gizmos.DrawLine(left, left + Vector3.forward * laserLength);

        Gizmos.color = Color.magenta;
        Gizmos.DrawLine(right, right + Vector3.forward * laserLength);
    }
}