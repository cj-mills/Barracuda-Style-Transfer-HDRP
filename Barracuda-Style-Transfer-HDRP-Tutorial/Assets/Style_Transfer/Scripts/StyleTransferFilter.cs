using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;
using System;
using Unity.Barracuda;

[Serializable, VolumeComponentMenu("Post-processing/Custom/StyleTransferFilter")]
public sealed class StyleTransferFilter : CustomPostProcessVolumeComponent, IPostProcessComponent
{
    [Tooltip("")]
    public ComputeShaderParameter FilterComputeShaderParameter = new ComputeShaderParameter(null);
    [Tooltip("")]
    public NNModelParameter ModelAssetParameter = new NNModelParameter(null);
    [Tooltip("")]
    public WorkerFactoryTypeParameter WorkerTypeParameter = new WorkerFactoryTypeParameter(WorkerFactory.Type.Auto);
    [Tooltip("")]
    public Vector2IntParameter InputResolutionParameter = new Vector2IntParameter(new Vector2Int(960, 540));

    public bool IsActive() => FilterComputeShaderParameter.value != null;

    // Do not forget to add this post process in the Custom Post Process Orders list (Project Settings > HDRP Default Settings).
    public override CustomPostProcessInjectionPoint injectionPoint => CustomPostProcessInjectionPoint.AfterPostProcess;

    RTHandle rtHandle;
    RenderTexture rTex;

    // The compiled model used for performing inference
    private Model m_RuntimeModel;

    // The interface used to execute the neural network
    private IWorker engine;


    public override void Setup()
    {

        // Compile the model asset into an object oriented representation
        m_RuntimeModel = ModelLoader.Load(ModelAssetParameter.value);

        // Create a worker that will execute the model with the selected backend
        engine = WorkerFactory.CreateWorker(WorkerTypeParameter.value, m_RuntimeModel);


        float scale = InputResolutionParameter.value.y / (float)Screen.height;

        rtHandle = RTHandles.Alloc(
            scaleFactor: Vector2.one * scale,
            filterMode: FilterMode.Point,
            wrapMode: TextureWrapMode.Clamp,
            dimension: TextureDimension.Tex2D,
            enableRandomWrite: true
            );

        // Assign a temporary RenderTexture with the new dimensions
        rTex = RenderTexture.GetTemporary(rtHandle.rt.width, rtHandle.rt.height, 24, RenderTextureFormat.ARGBHalf);
    }



    public override void Render(CommandBuffer cmd, HDCamera camera, RTHandle source, RTHandle destination)
    {
        cmd.Blit(source, rtHandle, 0, 0);

        // Create a Tensor of shape [1, rTex.height, rTex.width, 3]
        Tensor input = new Tensor(rtHandle, channels: 3);

        // Execute neural network with the provided input
        engine.Execute(input);
        // Get the raw model output
        Tensor prediction = engine.PeekOutput();
        // Release GPU resources allocated for the Tensor
        input.Dispose();
        // Copy the model output to rTex
        prediction.ToRenderTexture(rTex);
        // Release GPU resources allocated for the Tensor
        prediction.Dispose();

        cmd.Blit(rTex, source, 0, 0);


        // Postprocessing
        var filterComputeShader = FilterComputeShaderParameter.value;
        var mainkernel2 = filterComputeShader.FindKernel("ProcessOutput");

        filterComputeShader.GetKernelThreadGroupSizes(mainkernel2, out uint xGroupSize, out uint yGroupSize, out _);

        cmd.SetComputeTextureParam(filterComputeShader, mainkernel2, "InputImage", source.nameID);
        cmd.SetComputeTextureParam(filterComputeShader, mainkernel2, "Result", destination.nameID);
        cmd.DispatchCompute(filterComputeShader, mainkernel2,
            Mathf.CeilToInt(destination.rt.width / xGroupSize),
            Mathf.CeilToInt(destination.rt.width / yGroupSize),
            1);
    }

    public override void Cleanup()
    {
        // Release the resources allocated for the inference engine
        engine.Dispose();
        rtHandle.Release();
        rTex.Release();
    }
}
