using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Code.Editor
{
    [InitializeOnLoad]
    public static class PlayModeStartSceneSetup
    {
        private const string InitialScenePath = "Assets/Scenes/Bootstrap.unity";

        static PlayModeStartSceneSetup()
        {
            EditorApplication.delayCall += SetStartScene;
        }

        private static void SetStartScene()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
                return;

            SceneAsset scene = AssetDatabase.LoadAssetAtPath<SceneAsset>(InitialScenePath);

            if (scene == null)
            {
                Debug.LogWarning($"Стартовая сцена не найдена: {InitialScenePath}");
                return;
            }

            EditorSceneManager.playModeStartScene = scene;
        }
    }
}