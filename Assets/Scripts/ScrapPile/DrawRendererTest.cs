using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class DrawRendererTest : MonoBehaviour
{
    CommandBuffer cmd;

    public Renderer rend;
    public Material mat;
    public Mesh mesh;
    public Matrix4x4 matrix;

    public RenderTexture rt;
    public RenderTexture rt2;

    // Start is called before the first frame update
    void Start()
    {
        cmd = new CommandBuffer();

        rt = new RenderTexture(mat.mainTexture.width, mat.mainTexture.height, 0);
    }

    // Update is called once per frame
    void Update()
    {
        cmd.DrawMesh(mesh, matrix, mat);
        Graphics.ExecuteCommandBuffer(cmd);


        /*  DrawRenderer draws int the render target set for the command buffer.
         *  My Ink Paint material will unfold the mesh, meaning the the UV layout is actually draw to the screen.
         *  This 
         * 
         */
        cmd.SetRenderTarget(rt);
        cmd.DrawRenderer(rend, mat);

        cmd.Blit(rt, rt2);

        Graphics.ExecuteCommandBuffer(cmd);
        cmd.Clear();

        print("hello");
    }
}
