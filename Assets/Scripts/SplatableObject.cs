using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class SplatableObject : MonoBehaviour
{
    private RenderTexture splatmap;
    public RenderTexture tempM;

    // Used to blit newly drawn ink on top of the existing inking. The mobile/particle/additive material works well for this.
    [SerializeField]
    protected Material alphaCombiner;

    public RenderTexture Splatmap
    {
        get { return splatmap; }
    }

    private Material splatMaterial;
    private Material thisMaterial;

    CommandBuffer cmd;

    void Start()
    {
        splatmap = new RenderTexture(1024, 1024, 0, RenderTextureFormat.ARGBFloat);
        //splatmap.filterMode = FilterMode.Trilinear;
        tempM = new RenderTexture(1024, 1024, 0, RenderTextureFormat.ARGBFloat);
        //tempM.filterMode = FilterMode.Trilinear;


        splatMaterial = new Material(Shader.Find("Unlit/SplatMask"));       
        thisMaterial = GetComponent<Renderer>().material;
        thisMaterial.SetTexture("_Splatmap", splatmap);

         cmd = new CommandBuffer();

        


    }

    public void DrawSplat(Vector3 uvPos, float radius, float hardness, float strength, Color inkColor)
    {
        splatMaterial.SetFloat(Shader.PropertyToID("_Radius"), radius);
        splatMaterial.SetFloat(Shader.PropertyToID("_Hardness"), hardness);
        splatMaterial.SetFloat(Shader.PropertyToID("_Strength"), strength);
        splatMaterial.SetVector(Shader.PropertyToID("_SplatPos"), uvPos);
        splatMaterial.SetVector(Shader.PropertyToID("_InkColor"), inkColor);

        

        cmd.SetRenderTarget(tempM);
        cmd.DrawRenderer(GetComponent<Renderer>(), splatMaterial, 0);

        cmd.SetRenderTarget(splatmap);
        cmd.Blit(tempM, splatmap, alphaCombiner);
        
        Graphics.ExecuteCommandBuffer(cmd);
        cmd.Clear();

    }
}
