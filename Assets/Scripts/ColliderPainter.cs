using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ColliderPainter : MonoBehaviour
{
    private void OnCollisionStay(Collision collision)
    {
        print("collision");

        if (!collision.transform.GetComponent<SplatableObject>()) return;

        collision.transform.GetComponent<SplatableObject>().DrawSplat(collision.GetContact(0).point, .5f, .7f, .9f, new Color(1, 0, 0, 1));
    }
}
