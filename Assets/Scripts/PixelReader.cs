using UnityEngine;
using UnityEngine.Rendering;
using System.IO;
using System.Collections;

public class PixelReader : MonoBehaviour
{
    private RenderTexture splatmap;


    // Start
    private WaitForSeconds waitForSeconds = new WaitForSeconds(0.032f);
    private WaitForEndOfFrame waitForEndOfFrame = new WaitForEndOfFrame();

    private Vector2 textureCoords;
    bool isValidHit;

    // Get splatmap coords
    private Vector3 mouseScreenPos;
    private Ray mouseRay;
    private RaycastHit hit;

    protected void Start()
    {
        Application.targetFrameRate = -1;

        StartCoroutine(ReadPixelContinous());
    }

    protected IEnumerator ReadPixelContinous()
    {
        while (true)
        {
            yield return waitForSeconds;
            yield return waitForEndOfFrame;

            isValidHit = GetSplatmapCoords(ref textureCoords);

            if (isValidHit)
            {
                var rt = RenderTexture.GetTemporary(1, 1, 0, RenderTextureFormat.ARGBFloat);

                Graphics.CopyTexture(splatmap, 0, 0, (int)textureCoords.x, (int)textureCoords.y, 1, 1, rt, 0, 0, 0, 0);

                AsyncGPUReadback.Request(rt, 0, TextureFormat.ARGB32, OnCompleteReadback);

                RenderTexture.ReleaseTemporary(rt);
            }
        }
    }

    /// <summary>
    /// Projects mouse position to splatmap uv space.
    /// </summary>
    /// <param name="textureCoords">Store the ouput coordinates.</param>
    /// <returns>Returns true if the mouse hit an object.</returns>
    bool GetSplatmapCoords(ref Vector2 textureCoords)
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

        Color color = tex.GetPixel(0, 0);
        print("Color: " + color);

        Destroy(tex);
    }
}