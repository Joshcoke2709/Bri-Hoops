using System;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class GameTesterWebGLBuild
{
    private const string ScenePath = "Assets/Scenes/SampleScene.unity";

    public static void Build()
    {
        string outputPath = Environment.GetEnvironmentVariable("GAMETESTER_WEBGL_OUTPUT");

        if (string.IsNullOrWhiteSpace(outputPath))
        {
            outputPath = "Builds/WebGL/BriHoops";
        }

        PlayerSettings.productName = "Bri Hoops";
        EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.WebGL, BuildTarget.WebGL);
        EditorSceneManager.OpenScene(ScenePath);

        var options = new BuildPlayerOptions
        {
            scenes = new[] { ScenePath },
            locationPathName = outputPath,
            target = BuildTarget.WebGL,
            options = BuildOptions.None
        };

        BuildReport report = BuildPipeline.BuildPlayer(options);

        if (report.summary.result != BuildResult.Succeeded)
        {
            throw new Exception($"Bri Hoops WebGL build failed: {report.summary.result}");
        }

        Debug.Log($"Bri Hoops WebGL build completed at {outputPath}");
    }
}
