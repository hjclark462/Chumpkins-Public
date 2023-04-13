// Authored by: Harley Clark
using UnityEngine;

public class DummyTarget : MonoBehaviour
{
    public Player target;
    public float speed;
    private void Update()
    {
        if(target.targetable) transform.position = Vector3.MoveTowards(transform.position, target.orientation.position, speed * Time.deltaTime);
    }
}
