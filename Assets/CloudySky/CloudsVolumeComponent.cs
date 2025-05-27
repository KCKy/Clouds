using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.Serialization;

[Serializable]
[SupportedOnRenderPipeline]
[VolumeRequiresRendererFeatures(typeof(CloudySkyRendererFeature))]
[VolumeComponentMenu("Team-Like Team/Cloudy Sky Postprocess")]
public class CloudsVolumeComponent : VolumeComponent, IPostProcessComponent
{
    public bool IsActive() => true;
    public FloatParameter cloudMapOffset = new(0);
}