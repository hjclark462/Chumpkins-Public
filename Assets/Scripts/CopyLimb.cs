// Authored by: Todd Fanning
using UnityEngine;

public class CopyLimb : MonoBehaviour {

    [SerializeField]
    private Transform targetLimb;
    private ConfigurableJoint m_ConfigurableJoint;

    Quaternion targetInitialRotation;
    void Start() {
        this.m_ConfigurableJoint = this.GetComponent<ConfigurableJoint>();
        this.targetInitialRotation = this.targetLimb.transform.localRotation;
    }

    private void FixedUpdate() {
        this.m_ConfigurableJoint.targetRotation = copyRotation();
    }

    private Quaternion copyRotation() {
        return Quaternion.Inverse(this.targetLimb.localRotation) * this.targetInitialRotation;
    }
}
