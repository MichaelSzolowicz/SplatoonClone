using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    [SerializeField]
    protected float stickTime;

    private void OnCollisionEnter(Collision collision)
    {
        Invoke("DestroySelf", stickTime);
    }

    private void DestroySelf()
    {
        Destroy(this.gameObject);
    }
}
