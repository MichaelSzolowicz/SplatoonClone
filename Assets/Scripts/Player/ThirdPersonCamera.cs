using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// [Player Controller]
/// [05-01-23]
/// [Szolowicz, Michael]
/// Handles simplistic control of a third person camera.
/// </summary>
/// 

public class ThirdPersonCamera : MonoBehaviour
{
    public Transform cameraTransform;
    public Transform followTransform;

    protected float _offsetX, _offsetY;

    [SerializeField]
    protected float followDistance = 6.0f;
    [SerializeField]
    protected float yMax = 86f;
    [SerializeField]
    protected float yMin = -6f;
    [SerializeField]
    protected float xSensitivity = 1.0f, ySensitivity = 1.0f;

    protected float _yRotation;
    public float yRotation
    {
        get { return _yRotation; }
    }

    protected void Awake()
    {
        cameraTransform.position = followTransform.position + (followTransform.forward * -followDistance);

        //Debug.DrawLine(cameraTransform.position, followTransform.position, Color.red, 100.0f);
    }

    public void CameraUpdate(float offsetX, float offsetY)
    {
        _offsetX += offsetX * xSensitivity;
        _offsetY += offsetY * ySensitivity;

        _offsetY = Mathf.Clamp(_offsetY, yMin, yMax);

        Vector3 direction = new Vector3(0, 0, -followDistance);
        Quaternion rot = Quaternion.Euler(_offsetY, _offsetX, 0);
        cameraTransform.position = (followTransform.position + rot * direction);
        cameraTransform.LookAt(followTransform.position);


        _yRotation = cameraTransform.rotation.eulerAngles.y;
    }
}

