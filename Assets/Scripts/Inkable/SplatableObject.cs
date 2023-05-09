using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class SplatableObject : MonoBehaviour
{
    private RenderTexture splatmap;
    private RenderTexture tempM;

    [SerializeField, Tooltip("Used to blit newly drawn ink on top of the existing inking.")]
    protected Material alphaCombiner;

    [SerializeField, Tooltip("If left blank a blank render texture of size textureSize will be automatically generated.")]
    protected Texture sourceMap;
    [SerializeField]
    protected int textureSize = 1024;

    public RenderTexture Splatmap
    {
        get { return splatmap; }
    }

    private Material splatMaterial;
    private Material thisMaterial;
    private CommandBuffer cmd;

    void Start()
    {
        if(sourceMap)
        {
            print(sourceMap.graphicsFormat);
            splatmap = new RenderTexture(sourceMap.width, sourceMap.height, 0, RenderTextureFormat.ARGBFloat);
            Graphics.Blit(sourceMap, splatmap, alphaCombiner);
        }
        else
        {
            splatmap = new RenderTexture(textureSize, textureSize, 0, RenderTextureFormat.ARGBFloat);
        }
        tempM = new RenderTexture(splatmap.width, splatmap.height, 0, RenderTextureFormat.ARGBFloat);

        splatMaterial = new Material(Shader.Find("Unlit/SplatMask"));       
        thisMaterial = GetComponent<Renderer>().material;
        thisMaterial.SetTexture("_Splatmap", splatmap);

         cmd = new CommandBuffer();
    }

    public void DrawSplat(Vector3 worldPos, float radius, float hardness, float strength, Color inkColor)
    {
        splatMaterial.SetFloat(Shader.PropertyToID("_Radius"), radius);
        splatMaterial.SetFloat(Shader.PropertyToID("_Hardness"), hardness);
        splatMaterial.SetFloat(Shader.PropertyToID("_Strength"), strength);
        splatMaterial.SetVector(Shader.PropertyToID("_SplatPos"), worldPos);
        splatMaterial.SetVector(Shader.PropertyToID("_InkColor"), inkColor);

        cmd.SetRenderTarget(tempM);
        cmd.DrawRenderer(GetComponent<Renderer>(), splatMaterial, 0);

        cmd.SetRenderTarget(splatmap);
        cmd.Blit(tempM, splatmap, alphaCombiner);
        
        Graphics.ExecuteCommandBuffer(cmd);
        cmd.Clear();
    }
}
