using UnityEngine;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.Rendering.RenderGraphModule.Util;

public class CloudySkyRenderPass : ScriptableRenderPass
{
    readonly Material _material;

    public CloudySkyRenderPass(Material material)
    {
        _material = material;
        requiresIntermediateTexture = true;
        renderPassEvent = RenderPassEvent.BeforeRenderingPostProcessing;
    }

    void UpdateSettings()
    {
        return;
        var volumeComponent = VolumeManager.instance.stack.GetComponent<CloudsVolumeComponent>();
        ColorParameter color = volumeComponent.testColor;
        _material.color = color.overrideState ? color.value : Color.white;
    }

    public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
    {
        var resourceData = frameData.Get<UniversalResourceData>();

        if (resourceData.isActiveTargetBackBuffer)
            return;

        TextureHandle source = resourceData.activeColorTexture;

        TextureDesc destinationDesc = renderGraph.GetTextureDesc(source);
        destinationDesc.name = "CloudySkyCameraColor";
        destinationDesc.clearBuffer = false;

        TextureHandle destination = renderGraph.CreateTexture(destinationDesc);

        UpdateSettings();

        renderGraph.AddBlitPass(new(source, destination, _material, 0));

        resourceData.cameraColor = destination;
    }
}

public class CloudySkyRendererFeature : ScriptableRendererFeature
{    

    [SerializeField] Material material;
    CloudySkyRenderPass _pass;

    public override void Create()
    { 
        _pass = new CloudySkyRenderPass(material);
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        if (renderingData.cameraData.cameraType == CameraType.Game)
            renderer.EnqueuePass(_pass);
    }
}
