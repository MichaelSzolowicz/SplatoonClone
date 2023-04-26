using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class SplatableObject : MonoBehaviour
{
    public RenderTexture splatmap;

    private Material splatMaterial;
    private Material thisMaterial;

    void Awake()
    {
        splatmap = new RenderTexture(1024, 1024, 32, RenderTextureFormat.ARGB32, RenderTextureReadWrite.sRGB);
        splatMaterial = new Material(Shader.Find("Unlit/SplatMask"));
        //splatmap.useMipMap = true;
        //splatmap.GenerateMips();

        thisMaterial = GetComponent<Renderer>().material;
        thisMaterial.SetTexture("_Splatmap", splatmap);
    }

    public void DrawSplat(Vector2 uvPos, float radius, float hardness, float strength, Color inkColor)
    {
        splatMaterial.SetFloat(Shader.PropertyToID("_Radius"), radius);
        splatMaterial.SetFloat(Shader.PropertyToID("_Hardness"), hardness);
        splatMaterial.SetFloat(Shader.PropertyToID("_Strength"), strength);
        splatMaterial.SetVector(Shader.PropertyToID("_SplatPos"), uvPos);
        splatMaterial.SetVector(Shader.PropertyToID("_InkColor"), inkColor);

        RenderTexture temp = RenderTexture.GetTemporary(splatmap.width, splatmap.height);
        Graphics.Blit(splatmap, temp);
        Graphics.Blit(temp, splatmap, splatMaterial);
        RenderTexture.ReleaseTemporary(temp);
    }
}
