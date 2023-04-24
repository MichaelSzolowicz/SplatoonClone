using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExampleTest : MonoBehaviour
{ // Copies aTexture to rTex and displays it in all cameras.

    public Texture aTexture;
    public RenderTexture rTex;
    public Material mat;

    public Texture2D texture = new Texture2D(1, 1);

    public Texture2D writeTex;
    bool mousePressed = false;

    private WaitForSeconds waitForSeconds = new WaitForSeconds(0.032f);

    void Start()
    {
        Application.targetFrameRate = -1;

        Graphics.Blit(aTexture, rTex);

        if (!aTexture || !rTex)
        {
            Debug.LogError("A texture or a render texture are missing, assign them.");
        }

        StartCoroutine(GetRtColorCoroutine());
    }

    IEnumerator GetRtColorCoroutine()
    {
        while(true)
        {
            yield return waitForSeconds;

            for (int i = 0; i < 8; i++)
            {
                Vector3 screenPos = Input.mousePosition;

                Ray ray = Camera.main.ScreenPointToRay(screenPos);
                Vector3 worldPos = Camera.main.ScreenToWorldPoint(screenPos);

                RaycastHit hit;
                bool bHit = Physics.Raycast(ray, out hit, Mathf.Infinity);


                if (bHit)
                {
                    print("Mouse hit" + hit.transform.gameObject.name);

                    Vector2 texCoord = hit.textureCoord;

                    print(texCoord);

                    RenderTexture.active = rTex;

                    texture = new Texture2D(1, 1);

                    texture.ReadPixels(new Rect(texCoord.x * rTex.width, texCoord.y * rTex.height, 1, 1), 0, 0);
                }
            }
        }
    }
}

/*
 
*/ 
