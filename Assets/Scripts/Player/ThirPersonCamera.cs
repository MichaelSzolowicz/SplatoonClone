using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThirPersonCamera : MonoBehaviour
{
    public Transform camera;
    public Transform followTransform;
    [SerializeField]
    protected float followDistance = 6.0f;
    protected float _offsetX, _offsetY;
    [SerializeField]
    protected float Y_MAX = 1.0f;
    [SerializeField]
    protected float Y_MIN = .50f;
    [SerializeField]
    protected float xSensitivity = 1.0f, ySensitivity = 1.0f;

    public float yRotation;

    protected void Awake()
    {
        camera.transform.position = followTransform.position + (followTransform.forward * -followDistance);

        Debug.DrawLine(camera.transform.position, followTransform.position, Color.red, 100.0f);
    }

    public void CameraUpdate(float offsetX, float offsetY)
    {
        _offsetX += offsetX * xSensitivity;
        _offsetY += offsetY * ySensitivity;

        _offsetY = Mathf.Clamp(_offsetY, Y_MIN, Y_MAX);

        Vector3 direction = new Vector3(0, 0, -followDistance);
        Quaternion rot = Quaternion.Euler(_offsetY, _offsetX, 0);
        camera.transform.position = (followTransform.position + rot * direction);
        camera.transform.LookAt(followTransform.position);


        yRotation = camera.transform.rotation.eulerAngles.y;
    }
}
