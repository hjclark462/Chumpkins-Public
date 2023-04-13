// Authored by: Todd Fanning
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArmIKController : MonoBehaviour {
  [SerializeField]
  private Transform IKTarget;
  [SerializeField]
  private Transform pole;
  [SerializeField]
  private Transform ragJoint0, ragJoint1;
  [SerializeField]
  private Transform animJoint0, animJoint1, animJoint2;
  private LogicalBone bone1, bone2;
  private float boneLength1, boneLength2;
  private Vector3 oldStart, oldPole, oldTarget;
  private Vector3 joint0pos, joint1pos, joint2pos;
  [SerializeField]
  private AnimationWeightWrapper IKweightW;
  private Quaternion elbowDefaultRot;
  // Start is called before the first frame update
  void Start() {
    joint0pos = animJoint0.position;
    joint1pos = animJoint1.position;
    joint2pos = animJoint2.position;
    bone1 = new LogicalBone(joint0pos, joint1pos);
    bone2 = new LogicalBone(joint1pos, joint2pos);
    boneLength1 = bone1.getLength();
    boneLength2 = bone2.getLength();
  }

  void LateUpdate() {
    elbowDefaultRot = animJoint1.localRotation;
    if ((animJoint0.position != oldStart) || (pole.position != oldPole) || (IKTarget.position != oldTarget)) {
      bone1.snapStart(animJoint0.position);
      bone2.snapStart(bone1.getEnd());
      ikSolve();
      oldStart = animJoint0.position;
      oldPole = pole.position;
      oldTarget = IKTarget.position;
    }
    bone1.drawVis();
    bone2.drawVis();
    bool leftBone = name[name.Length-1] == 'L';
    Vector3 downBone = leftBone?Vector3.up:Vector3.down;
    Vector3 joint0Pointing = bone1.getEnd()-bone1.getStart();
    Debug.DrawRay(bone2.getStart(), bone2.getEnd()-bone2.getStart(), Color.white, 0);
    float weight = IKweightW.weight;
    animJoint0.rotation = Quaternion.Slerp(animJoint0.rotation, Quaternion.LookRotation(joint0Pointing, Vector3.up) * Quaternion.Euler(90 * (leftBone?-1:1), 90 * (leftBone?1:-1), 0), weight);
    Vector3 joint1Pointing = bone2.getEnd()-bone2.getStart();
    animJoint1.localRotation = Quaternion.Slerp(elbowDefaultRot, Quaternion.LookRotation(animJoint0.InverseTransformDirection(joint1Pointing), -downBone) * Quaternion.Euler(-90 * (leftBone?1:-1), 90 * (leftBone?1:-1), 0), weight);
  }

  void Update() {
  }

  void ikSolve() {
    Vector3 IKpos = IKTarget.position;
    Vector3 polePos = pole.position;
    Vector3 ikTargetOffset = IKpos - animJoint0.position;
    float offsetLength = ikTargetOffset.magnitude;
    if ((boneLength1 + boneLength2) <= offsetLength) {
      bone1.lookAt(IKpos);
      bone2.snapStart(bone1.getEnd());
      bone2.lookAt(IKpos);
    }
    else if ((boneLength2 - boneLength1) >= offsetLength){
      bone1.lookAt(animJoint0.position - ikTargetOffset);
      bone2.snapStart(bone1.getEnd());
      bone2.lookAt(IKpos);
    }
    else {
      float boneRotateTheta = Mathf.Acos((((boneLength1*boneLength1)+(offsetLength*offsetLength))-(boneLength2*boneLength2))/(2*boneLength1*offsetLength));
      Vector3 firstBoneLookat = Quaternion.AngleAxis(boneRotateTheta * Mathf.Rad2Deg, Vector3.Cross(ikTargetOffset, polePos-animJoint0.position)) * ikTargetOffset;
      bone1.lookAt(firstBoneLookat + animJoint0.position);
      bone2.snapStart(bone1.getEnd());
      bone2.lookAt(IKpos);
    }
  }
}

public class LogicalBone {
  private Vector3 start, end;

  public LogicalBone(Vector3 point1, Vector3 point2) {
    start = point1;
    end = point2;
  }
  public void drawVis() {
    Debug.DrawLine(start, end, Color.black, 0, false);
  }
  public void setStart(Vector3 point) {
    start = point;
  }
  public void setEnd(Vector3 point) {
    end = point;
  }
  public void lookAt(Vector3 point) {
    Vector3 currentdiff = end - start;
    Vector3 newdiff = point - start;
    setEnd(start + (newdiff.normalized*currentdiff.magnitude));
  }

  public void snapEnd(Vector3 point) {
    Vector3 newdiff = point - end;
    setStart(start + newdiff);
    setEnd(point);
  }
  public void snapStart(Vector3 point) {
    Vector3 newdiff = point - start;
    setStart(point);
    setEnd(end + newdiff);
  }
  public Vector3 getStart() {
    return start;
  }
  public Vector3 getEnd() {
    return end;
  }

  public float getLength() {
    return (end - start).magnitude;
  }
}