using UnityEngine;
using UnityEngine.Rendering;
using System.IO;
using System.Collections;

public class SplatmapReader : MonoBehaviour
{
    /// <summary> Most recently read color. </summary>
    private Color color;
    /// <summary> Most recently read color. </summary>
    public Color Color
    {
        get { return color; }
    }

    public delegate void CallbackDelgate(Color color);
    [Tooltip("Callback delegate is used to give information back to the original caller.")]
    private CallbackDelgate callbackDelgate;

    /// <summary>
    /// Read the color of a pixel at uv coordinate on rendertexture using Async GPU Readback.
    /// </summary>
    /// <param name="target"></param>
    /// <param name="uv"></param>
    /// <param name="callbackFunction">Function returning void taking a Color parameter. Used to get information back to the original caller.</param>
    public void ReadPixel(RenderTexture target, Vector2 uv, CallbackDelgate callbackFunction = null)
    {
        callbackDelgate = callbackFunction;

         var rt = RenderTexture.GetTemporary(1, 1, 0, RenderTextureFormat.ARGBFloat);

         Graphics.CopyTexture(target, 0, 0, (int)(uv.x * target.width), (int)(uv.y * target.height), 1, 1, rt, 0, 0, 0, 0);
         AsyncGPUReadback.Request(rt, 0, TextureFormat.ARGB32, OnCompleteReadback);
         RenderTexture.ReleaseTemporary(rt);
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

        color = tex.GetPixel(0, 0);
        callbackDelgate.Invoke(color);

        DestroyImmediate(tex);
    }
}