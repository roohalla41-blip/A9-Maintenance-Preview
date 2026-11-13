using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;

public static class BuildScript
{
    public static void BuildAndroid()
    {
        const string outputDirectory = "Build/Android";
        Directory.CreateDirectory(outputDirectory);

        var scenes = new[] { "Assets/Scenes/AfghanistanVillage.unity" };
        var options = new BuildPlayerOptions
        {
            scenes = scenes,
            locationPathName = Path.Combine(outputDirectory, "AfghanistanOld.apk"),
            target = BuildTarget.Android,
            options = BuildOptions.None
        };

        PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64;
        PlayerSettings.SetScriptingBackend(BuildTargetGroup.Android, ScriptingImplementation.IL2CPP);

        BuildReport report = BuildPipeline.BuildPlayer(options);
        if (report.summary.result != BuildResult.Succeeded)
            throw new System.Exception("Android build failed: " + report.summary.result);
    }
}