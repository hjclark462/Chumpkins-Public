// Authored by Harley Clark
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[RequireComponent(typeof(Camera))]
public class CameraControl : MonoBehaviour
{
    [HideInInspector]
    public List<Transform> targetList;

    [Header("Position, Move Speed and FOV")]
    public Vector3 offset;
    public float fieldOfView;
    public float smoothTime;
    public float minZoom;
    public float maxZoom;

    [Header("Rotation")]
    public bool automaticallyLookAtCenter = false;
    public float pitch;
    public float yaw;
    public float roll;

    private Vector3 velocity;
    private Vector3 velocity1;
    private Vector3 lastCenter;
    private Camera m_camera;
    public GameObject dolly;

    void Start()
    {
        m_camera = GetComponent<Camera>();
        m_camera.fieldOfView = fieldOfView;
    }

    // Updates camera after the targets have moved
    void LateUpdate()
    {
        if (targetList.Count == 0)
        {
            return;
        }
        MoveAndRotate();
    }
    // Centers based on the targets
    void MoveAndRotate()
    {
        Vector3 centerPoint = GetCenterPoint();
        dolly.transform.position = Vector3.SmoothDamp(dolly.transform.position, centerPoint, ref velocity, smoothTime);
        transform.localPosition = Vector3.SmoothDamp(transform.localPosition, CalculateOffset(), ref velocity1, smoothTime);
        if (automaticallyLookAtCenter)
        {
            m_camera.transform.LookAt(new Vector3(m_camera.transform.position.x, centerPoint.y, centerPoint.z));
        }
        else
        {
            Quaternion rotation = Quaternion.Euler(pitch, yaw, roll);
            transform.localRotation = rotation;
        }
        lastCenter = centerPoint;
    }

    Vector3 CalculateOffset()
    {
        Vector3 newOffset = offset + (offset.normalized * GetGreatestDistance());
        return newOffset;
    }

    // Creates an AABB around the targets to get the center focus point
    Vector3 GetCenterPoint()
    {
        if (targetList.Count == 1)
        {
            return targetList[0].position;
        }
        Bounds bounds = new Bounds(targetList[0].position, Vector3.zero);
        for (int i = 0; i < targetList.Count; i++)
        {
            bounds.Encapsulate(targetList[i].position);
        }
        return bounds.center;
    }
    // Returns the width of the AABB based on the furthest apart targets
    float GetGreatestDistance()
    {
        Bounds bounds = new Bounds(targetList[0].position, Vector3.zero);
        for (int i = 0; i < targetList.Count; i++)
        {
            bounds.Encapsulate(targetList[i].position);
        }
        if (bounds.size.z < minZoom && bounds.size.x < minZoom)
        {
            return minZoom;
        }
        else if (bounds.size.z > bounds.size.x && bounds.size.z < maxZoom && bounds.size.z > minZoom)
        {
            return bounds.size.z;
        }
        else if (bounds.size.z < bounds.size.x && bounds.size.x < maxZoom && bounds.size.x > minZoom)
        {
            return bounds.size.x;
        }
        else
        {
            return maxZoom;
        }
    }
    public void AddTarget(Transform target)
    {
        targetList.Add(target);
    }
    public void RemoveTarget(Transform target)
    {
        targetList.Remove(target);
    }
    private void ResetTargets()
    {
        targetList = new List<Transform>();
    }
}
