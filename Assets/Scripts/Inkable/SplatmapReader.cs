using UnityEngine;
using UnityEngine.Rendering;
using System.IO;
using System.Collections;

public class SplatmapReader : MonoBehaviour
{
    private RenderTexture splatmap;
    private Color color;

    // Start
    private WaitForSeconds waitForSeconds = new WaitForSeconds(0.032f);
    private WaitForEndOfFrame waitForEndOfFrame = new WaitForEndOfFrame();

    private Vector2 textureCoords;
    bool isValidHit;

    // Get splatmap coords
    private Vector3 mouseScreenPos;
    private Ray mouseRay;
    private RaycastHit hit;

    public Color ReadPixel(RenderTexture target, Vector2 uv)
    {
        //isValidHit = GetSplatmapCoords(ref inCoordinates);

        //if (isValidHit)
        {
            var rt = RenderTexture.GetTemporary(1, 1, 0, RenderTextureFormat.ARGBFloat);

            Graphics.CopyTexture(target, 0, 0, (int)(uv.x * target.width), (int)(uv.y * target.height), 1, 1, rt, 0, 0, 0, 0);

            //print("SplatmapReader: " + (int)(uv.x * target.width) + ", " + (int)(uv.y * target.height));

            AsyncGPUReadback.Request(rt, 0, TextureFormat.ARGB32, OnCompleteReadback);

            RenderTexture.ReleaseTemporary(rt);
        }

        return color;
    }

    /// <summary>
    /// Projects mouse position to splatmap uv space.
    /// </summary>
    /// <param name="textureCoords">Store the ouput coordinates.</param>
    /// <returns>Returns true if the mouse hit an object.</returns>
    bool MouseToUv(ref Vector2 textureCoords)
    {
        mouseScreenPos = Input.mousePosition;
        mouseRay = Camera.main.ScreenPointToRay(mouseScreenPos);

        bool bHit = Physics.Raycast(mouseRay, out hit, Mathf.Infinity);


        if (!bHit) return false;

        if (!hit.transform.GetComponent<SplatableObject>()) return false;


        splatmap = hit.transform.GetComponent<SplatableObject>().Splatmap;

        if (!splatmap) return false;

        textureCoords = hit.textureCoord;
        textureCoords.x *= splatmap.width;
        textureCoords.y *= splatmap.height;

        return true;
    }

    void OnCompleteReadback(AsyncGPUReadbackRequest request)
    {
        if (request.hasError)
        {
            Debug.Log("GPU readback error detected.");
            return;
        }

        var tex = new Texture2D(1, 1, TextureFormat.ARGB32, false);
        tex.LoadRawTextureData(request.GetData<uint>());
        tex.Apply();

        Color c = tex.GetPixel(0, 0);
        color = c;

        //print("Splatmap: " + color);

        Destroy(tex);
    }
}