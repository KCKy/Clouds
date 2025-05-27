using UnityEngine;
using UnityEngine.Rendering;

public class CloudsVolumeController : MonoBehaviour
{
    public Volume volume;
    CloudsVolumeComponent _cloudsVolumeComponent;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        if (!volume.profile.TryGet(out _cloudsVolumeComponent))
            Debug.LogError("Could not get clouds volume component from given volume.");
    }

    void Update()
    {
        if (!_cloudsVolumeComponent) return;
        _cloudsVolumeComponent.cloudMapOffset.overrideState = true;
        _cloudsVolumeComponent.cloudMapOffset.value = 0.6f * Mathf.Abs(Mathf.Sin(Time.time * 0.2f));
    }
}