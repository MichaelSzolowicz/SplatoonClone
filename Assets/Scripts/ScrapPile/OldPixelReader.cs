using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OldPixelReader : MonoBehaviour
{
    private Vector2 _textureCoords;

    public Texture2D dst;

    Vector2 mouseScreenPos;
    Ray mouseRay;
    RaycastHit hit;

    public RenderTexture splatmap;

 

    private void Update()
    {
        
        ReadPixelUpdate();
    }

    private void ReadPixelUpdate()
    {

        bool isValidHit = GetSplatmapCoords(ref _textureCoords);

        if(isValidHit)
        {
            Rect copy = new Rect((int)_textureCoords.x, 1024 - (int)_textureCoords.y, 1, 1);   // new Rect(0, 0, 1024, 1024);

            print(copy);

            dst = new Texture2D(1, 1, TextureFormat.RGBAFloat, false);

            RenderTexture.active = splatmap;
            dst.ReadPixels(copy, 0, 0);
            dst.Apply();

            print(dst.GetPixel(0, 0));

            RenderTexture.active = null;
        }
        
    }

    private IEnumerator ReadPixel()
    {
        while(true)
        {

            yield return new WaitForSeconds(0);

            bool isValidHit = GetSplatmapCoords(ref _textureCoords);

            Rect copy = new Rect((int)_textureCoords.x, (int)_textureCoords.y, 1, 1);

            print(copy);

            RenderTexture.active = splatmap;
            dst.ReadPixels(copy, 0, 0);
            dst.Apply();

            print(dst.GetPixel(0, 0));

            RenderTexture.active = null;
        }
    }

    bool GetSplatmapCoords(ref Vector2 textureCoords)
    {
        mouseScreenPos = Input.mousePosition;
        mouseRay = Camera.main.ScreenPointToRay(mouseScreenPos);

        bool bHit = Physics.Raycast(mouseRay, out hit, Mathf.Infinity);

        Debug.DrawRay(mouseRay.origin, mouseRay.direction * 9999, Color.red, .1f);

        if (!bHit) return false;

        if (!hit.transform.GetComponent<SplatableObject>()) return false;


        splatmap = hit.transform.GetComponent<SplatableObject>().Splatmap;

        if (!splatmap) return false;

        textureCoords = hit.textureCoord;
        textureCoords.x *= splatmap.width;
        textureCoords.y *= splatmap.height;

        return true;
    }
}
