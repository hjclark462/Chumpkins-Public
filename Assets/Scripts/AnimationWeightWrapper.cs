// Authored by: Todd Fanning
using UnityEngine;

public class AnimationWeightWrapper : MonoBehaviour {
  // Start is called before the first frame update
  public float weight = 0;

  void FixedUpdate() {
    if (weight > 1) weight = 1;
    if (weight < 0) weight = 0;
  }
}
