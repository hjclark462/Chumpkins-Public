// Authored by Finn Davis
using UnityEngine;

public class AnimationEvents : MonoBehaviour
{
    AnimationWeightWrapper awr;
    Animator animator;
    Player player;
    private void Awake()
    {
        animator = GetComponent<Animator>();
        awr = GetComponentInChildren<AnimationWeightWrapper>();
        player = GetComponentInParent<Player>();
    }
    private void LateUpdate() => animator.SetLayerWeight(2, awr.weight);
    public void OnFootDown() => player.OnFootDown();
    public void ExplodeParticles() => player.ExplodeParticles();
}
