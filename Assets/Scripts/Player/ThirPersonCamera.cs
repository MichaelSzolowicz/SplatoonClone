using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThirPersonCamera : MonoBehaviour
{
    public Camera cam;
    public Transform target;
    public float distance = 6.0f;

    protected Transform useTransform;

    protected float _offsetX, _offsetY;

    protected void Awake()
    {
        cam.transform.position = target.position + (target.forward * -distance);

        Debug.DrawLine(cam.transform.position, target.position, Color.red, 100.0f);
    }

    protected void Start()
    {

    }

    public void CameraUpdate(float offsetX, float offsetY)
    {
        _offsetX += offsetX;
        _offsetY += offsetY;
        Vector3 direction = new Vector3(0, 0, -distance);
        Quaternion rot = Quaternion.Euler(_offsetY, _offsetX, 0);
        cam.transform.position = (target.position + rot * direction);
        cam.transform.LookAt(target.position);
    }
}
