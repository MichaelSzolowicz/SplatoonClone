using UnityEngine;
using UnityEngine.Rendering;
using System.IO;
using System.Collections;

public class AsyncCapture : MonoBehaviour
{
    public Texture2D brush;
    public RenderTexture splatmap;


    // Start
    private WaitForSeconds waitForSeconds = new WaitForSeconds(0.032f);
    private WaitForEndOfFrame waitForEndOfFrame = new WaitForEndOfFrame();

    private Vector2 textureCoords;
    bool isValidHit;

    // Get splatmap coords
    private Vector3 mouseScreenPos;
    private Ray mouseRay;
    private RaycastHit hit;

    IEnumerator Start()
    {
        Application.targetFrameRate = -1;
        BlitBrush();

        while (true)
        {
            yield return waitForSeconds;
            yield return waitForEndOfFrame;

            isValidHit = GetSplatmapCoords(ref textureCoords);

            if(isValidHit)
            {
                for(int i = 0; i < 8; i++)
                {
                    var rt = RenderTexture.GetTemporary(1, 1, 0, RenderTextureFormat.ARGB32);

                    Graphics.CopyTexture(splatmap, 0, 0, (int)textureCoords.x, (int)textureCoords.y, 1, 1, rt, 0, 0, 0, 0);

                    AsyncGPUReadback.Request(rt, 0, TextureFormat.ARGB32, OnCompleteReadback);

                    RenderTexture.ReleaseTemporary(rt);
                }
            }
        }
    }

    void BlitBrush()
    {
        Graphics.Blit(brush, splatmap);
    }

    bool GetSplatmapCoords(ref Vector2 textureCoords)
    {
        mouseScreenPos = Input.mousePosition;
        mouseRay = Camera.main.ScreenPointToRay(mouseScreenPos);

        bool bHit = Physics.Raycast(mouseRay, out hit, Mathf.Infinity);


        if (bHit)
        {
            textureCoords = hit.textureCoord;
            textureCoords.x *= splatmap.width;
            textureCoords.y *= splatmap.height;

            return true;
        }

        return false;
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

        ReadPixel(tex);

        //File.WriteAllBytes("C:\\Users\\szolo\\OneDrive\\Desktop\\test.png", ImageConversion.EncodeToPNG(tex));
        Destroy(tex);
    }

    void ReadPixel(Texture2D tex)
    {
        Color color = tex.GetPixel(0, 0);
        print("Color: " + color);
    }
}
