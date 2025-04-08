using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

public abstract class Texture3DGeneratorComponent : MonoBehaviour
{
    public abstract void GenerateTexture();
}

public class NoiseGeneratorComponent : Texture3DGeneratorComponent
{
    [SerializeField] protected string textureName;
    [SerializeField] protected int size;
    [SerializeField] protected TextureFormat format;

    protected virtual void StartSampling() { }

    protected virtual Color32 Sample(int x, int y, int z)
    {
        return new Color32((byte)Random.Range(0, 256),
            (byte)Random.Range(0, 256), 
            (byte)Random.Range(0, 256), 
            (byte)Random.Range(0, 256));
    }

    public override void GenerateTexture()
    {
        if (size <= 1)
        {
            Debug.LogError("Invalid texture size!");
        }

        var colors = new Color32[size * size * size];

        StartSampling();
        
        for (int x = 0; x < size; x++)
        for (int y = 0; y < size; y++)
        for (int z = 0; z < size; z++)
        {
            int zOffset = z * size * size;
            int yOffset = y * size;
            colors[x + yOffset + zOffset] = Sample(x, y, z);
        }

        Texture3D texture = new(size, size, size, format, false)
        {
            anisoLevel = 0,
            filterMode = FilterMode.Bilinear,
            wrapMode = TextureWrapMode.Repeat
        };

        texture.SetPixels32(colors);
        texture.Apply();
        AssetDatabase.CreateAsset(texture, $"Assets/{textureName}.asset");
    }
}

#if UNITY_EDITOR

[CustomEditor(typeof(Texture3DGeneratorComponent), true)]
public class Texture3DGeneratorEditorExtension : Editor
{
    void Generate()
    {
        if (target is not Texture3DGeneratorComponent comp)
        {
            Debug.LogError("Invalid target.");
            return;
        }

        comp.GenerateTexture();
    }

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        if (GUILayout.Button("Generate"))
            Generate();

        serializedObject.ApplyModifiedProperties();
    }
}

#endif