using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class SplatableObject : MonoBehaviour
{
    private RenderTexture splatmap;

    public RenderTexture Splatmap
    {
        get { return splatmap; }
    }

    private Material splatMaterial;
    private Material thisMaterial;

    void Start()
    {
        splatmap = new RenderTexture(1024, 1024, 0);
        splatMaterial = new Material(Shader.Find("Unlit/SplatMask"));
        //splatmap.useMipMap = true;
        //splatmap.GenerateMips();

        thisMaterial = GetComponent<Renderer>().material;

        RenderTexture temp = RenderTexture.GetTemporary(1024, 1024);
        Graphics.CopyTexture(temp, splatmap);
        RenderTexture.ReleaseTemporary(temp);
        thisMaterial.SetTexture("splatmap", splatmap);

        
    }

    public void DrawSplat(Vector2 uvPos, float radius, float hardness, float strength, Color inkColor)
    {
        splatMaterial.SetFloat(Shader.PropertyToID("_Radius"), radius);
        splatMaterial.SetFloat(Shader.PropertyToID("_Hardness"), hardness);
        splatMaterial.SetFloat(Shader.PropertyToID("_Strength"), strength);
        splatMaterial.SetVector(Shader.PropertyToID("_SplatPos"), uvPos);
        splatMaterial.SetVector(Shader.PropertyToID("_InkColor"), inkColor);

        CommandBuffer cmd = new CommandBuffer();

        RenderTexture temp = RenderTexture.GetTemporary(splatmap.width, splatmap.height);
        cmd.Blit(splatmap, temp);
        cmd.Blit(temp, splatmap, splatMaterial);

        Graphics.ExecuteCommandBuffer(cmd);

        RenderTexture.ReleaseTemporary(temp);
    }
}
