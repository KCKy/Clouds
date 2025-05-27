using UnityEngine;

public class QuadFitter : MonoBehaviour
{
    [Header("References")]
    [SerializeField] Camera targetCamera;
    [SerializeField] MeshRenderer targetMesh;
    
    int _width;
    int _height;
    //[SerializeField] RenderTexture texture;
    
    void FitQuad()
    {
        float h = 2f * targetCamera.orthographicSize;
        float w = h * targetCamera.aspect;
        targetMesh.transform.position = targetCamera.transform.position + targetCamera.transform.forward * 1f;
        targetMesh.transform.rotation = Quaternion.identity;
        targetMesh.transform.localScale = new(w, h, 1);
    }
    
    /*
    void CreateTexture()
    {
        _width = Screen.width;
        _height = Screen.height;

        if (!targetCamera)
            targetCamera = Camera.main;
        
        if (texture)
        {
            texture.Release();
            Destroy(texture);
        }
        
        texture = new(_width, _height, 32, RenderTextureFormat.ARGB32)
        {
            antiAliasing = antiAliasing
        };
        
        texture.Create();
        targetMesh.material.mainTexture = texture;
    }*/
    
    void Start()
    {
        FitQuad();
    }

    void Update()
    {
        if (Screen.width == _width && Screen.height == _height)
            return;
        
        FitQuad();
    }
    
    /*void OnDestroy()
    {
        /*
        if (texture)
        {
            texture.Release();
            Destroy(texture);
        }
    }*/
}
