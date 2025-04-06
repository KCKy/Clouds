#if UNITY_EDITOR && EDITOR_PLAY_FROM_FIRST
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Useful.Hacks
{
    [InitializeOnLoad]
    public class EditorPlayFromFirstHack : MonoBehaviour
    {
        static EditorPlayFromFirstHack()
        {
            var sceneAsset = AssetDatabase.LoadAssetAtPath<SceneAsset>(EditorBuildSettings.scenes[0].path);
            EditorSceneManager.playModeStartScene = sceneAsset;
        }
    }
}

#endif
