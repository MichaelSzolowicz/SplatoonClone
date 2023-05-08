using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class SplatableObject : MonoBehaviour
{
    [SerializeField]
    protected Texture sourceMap;
    [SerializeField]
    protected int textureSize = 1024;
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
        if(sourceMap)
        {
            print(sourceMap.graphicsFormat);
            splatmap = new RenderTexture(sourceMap.width, sourceMap.height, 0, RenderTextureFormat.ARGBFloat);
            //splatmap.enableRandomWrite = true;
            //RenderTexture.active = splatmap;
            Graphics.Blit(sourceMap, splatmap, alphaCombiner);
        }
        else
        {
            splatmap = new RenderTexture(textureSize, textureSize, 0, RenderTextureFormat.ARGBFloat);
        }
        tempM = new RenderTexture(splatmap.width, splatmap.height, 0, RenderTextureFormat.ARGBFloat);
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
