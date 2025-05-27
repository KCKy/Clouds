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
    [SerializeField] float windChange;
    [SerializeField] float windStrength;
    
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
        _zOffset = Random.Range(100, 300);
    }
    
    void Start() => CreateTexture();
    
    float _zOffset;
    float _x;

    void Update()
    {
        _x += Time.deltaTime * windChange;
        float x = Mathf.PerlinNoise1D(_x);
        float z = Mathf.PerlinNoise1D(_x + _zOffset);
        transform.position += new Vector3(x, 0, z) * (Time.deltaTime * windStrength);
    }
}
