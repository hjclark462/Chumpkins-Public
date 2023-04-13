// Authored by: Todd Fanning
using UnityEngine;

public class RotationDummy : MonoBehaviour {
  [SerializeField]
  private Transform lookTarget;
  [SerializeField]
  private Transform ragdollRoot;
  private Quaternion baseRot;

  void Start() {
    baseRot = transform.localRotation;
  }

  void FixedUpdate() {
    Vector2 target = new Vector2(lookTarget.position.x, lookTarget.position.z);
    Vector2 root = new Vector2(ragdollRoot.position.x, ragdollRoot.position.z);
    Vector2 offset = target - root;
    Vector3 facing3 = ragdollRoot.InverseTransformDirection(Vector3.forward);
    Vector2 facing = new Vector2(-facing3.x, facing3.z).normalized;
    float angle = -Vector2.SignedAngle(facing, offset);
    transform.localRotation = Quaternion.AngleAxis(angle/4, transform.parent.InverseTransformDirection(Vector3.up)) * baseRot;
  }
}
