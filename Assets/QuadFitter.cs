using System;
using UnityEngine;

public class QuadFitter : MonoBehaviour
{
    [Header("References")]
    [SerializeField] Camera targetCamera;
    [SerializeField] MeshRenderer targetMesh;
    [SerializeField] float distanceFromCamera = 1000;
    
    int _width;
    int _height;
    
    void FitQuad()
    {
        if (!targetCamera.orthographic) throw new ArgumentException("The camera must be orthographic.");
        float h = 2f * targetCamera.orthographicSize;
        float w = h * targetCamera.aspect;
        targetMesh.transform.position = targetCamera.transform.position + targetCamera.transform.forward * distanceFromCamera;
        targetMesh.transform.rotation = Quaternion.identity;
        targetMesh.transform.localScale = new(w, h, 1);
    }
    
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
