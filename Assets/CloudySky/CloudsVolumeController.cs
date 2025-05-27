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
        if (_cloudsVolumeComponent == null) return;
        //_cloudsVolumeComponent.testColor.overrideState = true;
        //_cloudsVolumeComponent.testColor.value = _palette[Time.time];
    }
}