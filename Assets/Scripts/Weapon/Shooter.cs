using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shooter : MonoBehaviour
{
    [SerializeField]
    protected GameObject projectilePrefab;

    [SerializeField]
    protected float fireRate;
    [SerializeField]
    protected float launchStrength;
    [SerializeField]
    protected float xVariance, yVariance;

    public void StartShooting()
    {
        StartCoroutine(ShootOnLoop());
    }

    public void StopShooting()
    {
        StopAllCoroutines();
    }

    public IEnumerator ShootOnLoop()
    {
        while(true)
        {
            Shoot();
            yield return new WaitForSeconds(fireRate);
        }
    }

    public void Shoot()
    {
        GameObject obj = Instantiate(projectilePrefab, transform.position, transform.rotation);

        Rigidbody rigidbody = obj.GetComponent<Rigidbody>();
        if(rigidbody)
        {
            Vector3 direction = transform.forward;
            direction.x += Random.Range(0f, xVariance);
            direction.y += Random.Range(0f, yVariance);
            rigidbody.AddForce(direction.normalized * launchStrength, ForceMode.Impulse);
        }
    }
}
