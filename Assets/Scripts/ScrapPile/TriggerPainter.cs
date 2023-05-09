using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TriggerPainter : MonoBehaviour
{
    private void OnTriggerStay(Collider other)
    {
        print("trigger");

      
        if (!other.transform.GetComponent<SplatableObject>()) return;

        other.transform.GetComponent<SplatableObject>().DrawSplat(transform.position, .5f, .7f, .9f, new Color(1, 0, 0, 1));
    }
}
