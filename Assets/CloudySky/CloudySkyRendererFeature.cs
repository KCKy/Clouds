using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.RenderGraphModule.Util;
using UnityEngine.Rendering.Universal;

public class CloudySkyRenderPass : ScriptableRenderPass
{
    static readonly int CloudMapOffset = Shader.PropertyToID("_CloudMapOffset");
    readonly Material _material;

    public CloudySkyRenderPass(Material material)
    {
        _material = material;
        requiresIntermediateTexture = true;
        renderPassEvent = RenderPassEvent.BeforeRenderingPostProcessing;
    }

    void UpdateSettings()
    {
        var volumeComponent = VolumeManager.instance.stack.GetComponent<CloudsVolumeComponent>();
        FloatParameter cloudMapOffset = volumeComponent.cloudMapOffset;
        if (!cloudMapOffset.overrideState) return;
        _material.SetFloat(CloudMapOffset, cloudMapOffset.value);
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
        _pass = new(material);
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        if (renderingData.cameraData.cameraType == CameraType.Game)
            renderer.EnqueuePass(_pass);
    }
}