#if UNITY_EDITOR
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

[InitializeOnLoad]
public static class AncientAfghanistanProjectBootstrap
{
    private const string ScenePath = "Assets/Scenes/AncientAfghanistan_Prototype.unity";

    static AncientAfghanistanProjectBootstrap()
    {
        EditorApplication.delayCall += EnsureProjectScene;
    }

    private static void EnsureProjectScene()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode)
            return;

        if (!File.Exists(ScenePath))
        {
            if (!AssetDatabase.IsValidFolder("Assets/Scenes"))
                AssetDatabase.CreateFolder("Assets", "Scenes");

            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            var bootstrap = new GameObject("GameBootstrap");
            bootstrap.AddComponent<PrototypeBootstrap>();
            EditorSceneManager.SaveScene(scene, ScenePath);
            Debug.Log("Ancient Afghanistan: generated the prototype scene.");
        }

        var scenes = EditorBuildSettings.scenes;
        if (!scenes.Any(s => s.path == ScenePath))
        {
            EditorBuildSettings.scenes = scenes
                .Concat(new[] { new EditorBuildSettingsScene(ScenePath, true) })
                .ToArray();
        }

        AssetDatabase.SaveAssets();
    }
}
#endif
