using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using System.IO;

[ExecuteAlways]
[ImageEffectAllowedInSceneView]
[AddComponentMenu("LUT Renderer")]
public class LUTRenderer : MonoBehaviour
{
    ComputeShader compute;
	Camera _camera;
    CommandBuffer command;

    //settings
    [Range(0,1)]
    public float Blend = 1.0f;
    public Cube LUT;

    Texture2D tex;
    ComputeBuffer blendBuffer;

    int kernel;
    int ScreenWidth, ScreenHeight;

    void OnEnable()
    {
        if(!compute) compute = Resources.Load<ComputeShader>("LUTCompute");
        if(!_camera) _camera = GetComponent<Camera>();

        if(LUT)
        {
            ConstructLUT();
        }
        setShaderNameIDs();
    }

    void ScreenScaled(RenderTexture src)
    {
        if(ScreenWidth != _camera.pixelWidth || ScreenHeight != _camera.pixelHeight)
        {
            ScreenWidth = _camera.pixelWidth;
            ScreenHeight = _camera.pixelHeight;
        }
    }

    RenderTexture target;
    int colorID, resultID, widthID, heightID;
    void setShaderNameIDs()
    {
        colorID = Shader.PropertyToID("_color");
        resultID = Shader.PropertyToID("Result");
        widthID = Shader.PropertyToID("width");
        heightID = Shader.PropertyToID("height");
    }

    void OnRenderImage(RenderTexture src, RenderTexture dst)
    {
        if(LUT == null)
        {
            Graphics.Blit(src, dst);
            return;
        }
        target = RenderTexture.GetTemporary(src.descriptor);
        target.enableRandomWrite = true;

        ScreenWidth = _camera.pixelWidth;
        ScreenHeight = _camera.pixelHeight;

        compute.SetTexture( kernel, colorID, src);
        compute.SetTexture( kernel, resultID, target);
        compute.SetInt(widthID, ScreenWidth);
        compute.SetInt(heightID, ScreenHeight);

        int threadGroupsX = Mathf.CeilToInt(ScreenWidth / 8.0f);
        int threadGroupsY = Mathf.CeilToInt(ScreenHeight / 8.0f);

        compute.Dispatch(kernel, threadGroupsX, threadGroupsY, 1);

        Graphics.Blit(target, dst);
        target.Release();
    }

    Cube prevLUT;
    void Update()
    {
        if(prevLUT!=LUT)
        {
            prevLUT = LUT;
            ConstructLUT();
        }
        compute.SetFloat( "_blend", Blend);
    }
    void ConstructLUT()
    {
        if(LUT==null)
            return;
        int s = LUT.size * LUT.size;
        
        tex = new Texture2D(s, LUT.size, TextureFormat.RGBA32, false, true); 
        
        for(int i = 0; i < s; i++)
        {
            for(int j = 0; j < LUT.size; j++)
            {
                tex.SetPixel(i, j, LUT.colorValues[ i + (s*j)]);
            }
        }
        // uncomment to save lut as png
        // byte[] bytes = tex.EncodeToPNG();
        // File.WriteAllBytes(Application.dataPath + "/../" + LUT.name + ".png", bytes);
        tex.Apply(false, false);

        compute.SetTexture( kernel, "_lut", tex);
        compute.SetInt("lutSize", LUT.size);
    }
}
