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

    private void Start()
    {
        /**TESTONLY**/
        //StartShooting();
    }

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
            rigidbody.AddForce(transform.forward * launchStrength, ForceMode.Impulse);
        }
    }
}