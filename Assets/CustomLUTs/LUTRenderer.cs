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


    int targetRT = 0;

    CameraEvent evt = CameraEvent.AfterImageEffects;


    
    void OnEnable()
    {
        if(!compute) compute = Resources.Load<ComputeShader>("LUTCompute");
        if(!_camera) _camera = GetComponent<Camera>();

        InitCommandBuffer();

        if(LUT)
        {
            ConstructLUT(ref tex);
            InitializeCompute();
        }
    }

    
    void InitCommandBuffer()
    {
        if(command!=null)
        {
            _camera.RemoveCommandBuffer(evt, command);
        }
            
        command = new CommandBuffer();
        command.name = "LUT Renderer";
        
        _camera.AddCommandBuffer (evt, command); 
        kernel = compute.FindKernel("CSMain");


        blendBuffer?.Dispose();
        blendBuffer = new ComputeBuffer(1, sizeof(float));
        blendBuffer.SetData(new float[]{Blend});
    }

    void InitializeCompute()
    {
        command.Clear();

        command.GetTemporaryRT(targetRT, -1, -1, 24, FilterMode.Bilinear, 
        UnityEngine.Experimental.Rendering.GraphicsFormat.R32G32B32A32_SFloat, 1, true);

        command.SetComputeTextureParam(compute, kernel, "Result", targetRT);
        command.SetComputeTextureParam(compute, kernel, "_color", BuiltinRenderTextureType.CurrentActive);
        command.SetComputeTextureParam(compute, kernel, "_lut", tex);

        
        command.SetComputeBufferParam(compute, kernel, "_blend", blendBuffer);

        
        command.SetComputeIntParam(compute, "lutSize", LUT.size);
        command.SetComputeIntParam(compute, "width", ScreenWidth);
        command.SetComputeIntParam(compute, "height", ScreenHeight);

        int threadGroupsX = Mathf.CeilToInt(ScreenWidth / 8.0f);
        int threadGroupsY = Mathf.CeilToInt(ScreenHeight / 8.0f);

        command.DispatchCompute(compute, kernel, threadGroupsX, threadGroupsY, 1);

        command.Blit(targetRT, BuiltinRenderTextureType.CameraTarget);
        _camera.Render();
    }

    void ScreenScaled()
    {
        if(ScreenWidth != _camera.pixelWidth || ScreenHeight != _camera.pixelHeight)
        {
            ScreenWidth = _camera.pixelWidth;
            ScreenHeight = _camera.pixelHeight;
            InitializeCompute();
        }
    }
    void OnPreRender()
    {
        UpdateBlend();

        if(command!=null && tex != null)
            ScreenScaled();
    }

    void OnDisable()
    {
        if(command!=null)
            _camera?.RemoveCommandBuffer(evt, command);
        blendBuffer?.Dispose();
    }

    public void UpdateBlend()
    {
        blendBuffer?.SetData(new float[]{Blend});
    }

    Cube prevLUT;
    void Update()
    {
        if(Application.isEditor)
        {
            if(prevLUT!=LUT)
            {
                prevLUT = LUT;
                ConstructLUT(ref tex);
                InitializeCompute();
            }
        }
    }
    void ConstructLUT(ref Texture2D texture)
    {
        if(LUT==null)
            return;
        int s = LUT.size * LUT.size;
        
        texture = new Texture2D(s, LUT.size, TextureFormat.RGBA32, false, true); 
        
        for(int i = 0; i < s; i++)
        {
            for(int j = 0; j < LUT.size; j++)
            {
                texture.SetPixel(i, j, LUT.colorValues[ i + (s*j)]);
            }
        }
        byte[] bytes = texture.EncodeToPNG();
        File.WriteAllBytes(Application.dataPath + "/../" + LUT.name + ".png", bytes);
        texture.Apply(false, false);
    }
}
