using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ColliderPainter : MonoBehaviour
{
    private void OnCollisionStay(Collision collision)
    {
        print("collision");

        Ray ray = new Ray(transform.position, collision.GetContact(0).point - transform.position);
        RaycastHit hit;
        bool isValidHit = collision.collider.Raycast(ray, out hit, 500);

        if (!isValidHit) return;
        if (!hit.transform.GetComponent<SplatableObject>()) return;

        hit.transform.GetComponent<SplatableObject>().DrawSplat(hit.textureCoord, .05f, .7f, 1, new Color(1, 0, 0, 1));
    }
}
