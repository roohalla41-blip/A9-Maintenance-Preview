#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using System.IO;

public static class AncientAfghanistanSetup
{
    [MenuItem("Ancient Afghanistan/Create Prototype Scene")]
    public static void CreatePrototypeScene()
    {
        if (!AssetDatabase.IsValidFolder("Assets/Scenes"))
            AssetDatabase.CreateFolder("Assets", "Scenes");

        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        GameObject bootstrap = new GameObject("GameBootstrap");
        bootstrap.AddComponent<PrototypeBootstrap>();
        EditorSceneManager.SaveScene(scene, "Assets/Scenes/AncientAfghanistan_Prototype.unity");
        AssetDatabase.SaveAssets();
        EditorUtility.DisplayDialog("Ancient Afghanistan",
            "Prototype scene created. Press Play to generate the village.", "OK");
    }
}
#endif