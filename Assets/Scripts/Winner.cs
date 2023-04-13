// Authored by: Harley Clark
using UnityEngine;
// Broken down version of the character model that has it's colour and headpiece copied from the winner of the round
public class Winner : MonoBehaviour
{
    public Transform headJoint;
    public Animator animator;
    public SkinnedMeshRenderer fur;

    public void SetWinnerAppearance(GameObject head, Material col, float volume)
    {
        // To make sure that the headpiece is only one and no duplicates happen.
        if (headJoint.transform.childCount > 0)
        {
            for (int i = 0; i < headJoint.transform.childCount; i++)
                Destroy(headJoint.transform.GetChild(i).gameObject);
        }
        Instantiate(head, headJoint);
        fur.material = col;
        fur.SetBlendShapeWeight(0, 100 - volume);
        animator.SetTrigger("Victory");    
    }
}
