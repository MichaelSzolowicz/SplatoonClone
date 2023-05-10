using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    [SerializeField]
    protected float maxLifetime, stickTime;

    protected void Start()
    {
        Invoke("DestroySelf", maxLifetime);
    }

    private void OnCollisionEnter(Collision collision)
    {
        Invoke("DestroySelf", stickTime);
        GetComponent<MeshRenderer>().enabled = false;
    }

    private void DestroySelf()
    {
        if(this)
        {
            Destroy(this.gameObject);
        }
    }
}
