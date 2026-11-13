#if UNITY_EDITOR
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class AncientAfghanistanBuild
{
    private const string ScenePath = "Assets/Scenes/AncientAfghanistan_Prototype.unity";
    private const string OutputPath = "Builds/Android/AncientAfghanistan.apk";

    public static void BuildAndroid()
    {
        EnsurePrototypeScene();

        EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Android, BuildTarget.Android);
        PlayerSettings.productName = "افغانستان قدیم";
        PlayerSettings.companyName = "Ancient Afghanistan Studio";
        PlayerSettings.applicationIdentifier = "com.ancientafghanistan.game";
        PlayerSettings.bundleVersion = "0.1.0";
        PlayerSettings.Android.bundleVersionCode = 1;
        PlayerSettings.defaultInterfaceOrientation = UIOrientation.Portrait;
        PlayerSettings.allowedAutorotateToPortrait = true;
        PlayerSettings.allowedAutorotateToPortraitUpsideDown = false;
        PlayerSettings.allowedAutorotateToLandscapeLeft = false;
        PlayerSettings.allowedAutorotateToLandscapeRight = false;
        PlayerSettings.Android.minSdkVersion = AndroidSdkVersions.AndroidApiLevel26;
        PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64;
        PlayerSettings.SetScriptingBackend(BuildTargetGroup.Android, ScriptingImplementation.IL2CPP);

        Directory.CreateDirectory(Path.GetDirectoryName(OutputPath));
        var options = new BuildPlayerOptions
        {
            scenes = new[] { ScenePath },
            locationPathName = OutputPath,
            target = BuildTarget.Android,
            options = BuildOptions.None
        };

        BuildReport report = BuildPipeline.BuildPlayer(options);
        if (report.summary.result != BuildResult.Succeeded)
            throw new System.Exception("Android build failed: " + report.summary.result +
                " | errors: " + report.summary.totalErrors);

        Debug.Log("ANDROID_APK_READY=" + Path.GetFullPath(OutputPath) +
            " | bytes=" + report.summary.totalSize);
    }

    private static void EnsurePrototypeScene()
    {
        if (!AssetDatabase.IsValidFolder("Assets/Scenes"))
            AssetDatabase.CreateFolder("Assets", "Scenes");

        if (!File.Exists(ScenePath))
        {
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            var bootstrap = new GameObject("GameBootstrap");
            bootstrap.AddComponent<PrototypeBootstrap>();
            if (!EditorSceneManager.SaveScene(scene, ScenePath))
                throw new System.Exception("Could not save prototype scene.");
        }

        var enabledScenes = EditorBuildSettings.scenes
            .Where(scene => scene.path != ScenePath)
            .ToList();
        enabledScenes.Insert(0, new EditorBuildSettingsScene(ScenePath, true));
        EditorBuildSettings.scenes = enabledScenes.ToArray();
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }
}
#endif
