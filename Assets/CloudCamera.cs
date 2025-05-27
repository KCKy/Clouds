using UnityEngine;

[RequireComponent(typeof(Camera))]
public class CloudCamera : MonoBehaviour
{
    Camera _camera;
    [SerializeField] RenderTexture texture;
    [SerializeField] MeshRenderer targetBillboard;
    [SerializeField] int width;
    [SerializeField] int height;
    [SerializeField] int antiAliasing;
    
    void Awake() => _camera = GetComponent<Camera>();
    
    void CreateTexture()
    {
        if (texture)
        {
            texture.Release();
            Destroy(texture);
        }

        texture = new(width, height, 32, RenderTextureFormat.ARGB32)
        {
            antiAliasing = antiAliasing,
            useMipMap = false
        };

        texture.Create();
        _camera.targetTexture = texture;
        targetBillboard.material.mainTexture = texture;
    }
    
    void Start() => CreateTexture();
}
