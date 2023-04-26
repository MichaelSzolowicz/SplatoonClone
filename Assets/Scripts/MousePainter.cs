using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MousePainter : MonoBehaviour
{
    public Color inkColor;
    public float radius, hardness, strength;

    void Update()
    {
        if(Input.GetMouseButton(0))
        {
            Draw();
        }
    }

    protected void Draw()
    {
        //print("Mouse is drawing");

        Vector3 mousePos = Input.mousePosition;
        Ray ray = Camera.main.ScreenPointToRay(mousePos);

        RaycastHit hit;
        bool isValidHit = Physics.Raycast(ray, out hit, Mathf.Infinity);

        if (!isValidHit) return;

        SplatableObject splatObject = hit.transform.GetComponent<SplatableObject>();
        if (!splatObject) return;

        splatObject.DrawSplat(hit.textureCoord, radius, hardness, strength, inkColor);

        
    }
}
