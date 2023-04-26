using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TriggerPainter : MonoBehaviour
{
    private void OnTriggerStay(Collider other)
    {
        print("trigger");

        Vector3 direction = other.ClosestPointOnBounds(transform.position) - transform.position;

        if (direction.magnitude <= 0) direction = other.transform.position - transform.position;

        Ray ray = new Ray(transform.position, direction);

        print(direction);

        RaycastHit hit;
        bool isValidHit = other.Raycast(ray, out hit, 500);

        if (!isValidHit) return;
        if (!hit.transform.GetComponent<SplatableObject>()) return;

        hit.transform.GetComponent<SplatableObject>().DrawSplat(hit.textureCoord, .05f, .7f, 1, new Color(1, 0, 0, 1));
    }
}
