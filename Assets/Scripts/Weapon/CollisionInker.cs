using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollisionInker : MonoBehaviour
{
    [SerializeField]
    protected Vector3 team;

    [SerializeField]
    protected float radius, hardness, strength;

    private Color color;

    private void Awake()
    {
        color = new Color(team.x, team.y, team.z, 1.0f);
        //radius = GetComponent<SphereCollider>().radius;
    }

    private void OnCollisionStay(Collision collision)
    {
        SplatableObject splatObj = collision.collider.GetComponent<SplatableObject>();
        if(splatObj)
        {
            splatObj.DrawSplat(collision.GetContact(0).point, radius, hardness, strength, color);
        }
    }

}
