using UnityEditor;
using UnityEngine;

public class BuildScript {
    [MenuItem("Build/Build Windows")]
    public static void BuildWindows() {
        string[] scenes = System.Array.ConvertAll(
            System.Array.FindAll(EditorBuildSettings.scenes, s => s.enabled),
            s => s.path
        );

        BuildPlayerOptions options = new BuildPlayerOptions {
            scenes = scenes,
            locationPathName = "Builds/Windows/Carpinotchi.exe",
            target = BuildTarget.StandaloneWindows64,
            options = BuildOptions.None
        };

        UnityEditor.Build.Reporting.BuildReport report = BuildPipeline.BuildPlayer(options);
        Debug.Log("Build result: " + report.summary.result);
        Debug.Log("Total errors: " + report.summary.totalErrors);
        Debug.Log("Total warnings: " + report.summary.totalWarnings);

        if (report.summary.result != UnityEditor.Build.Reporting.BuildResult.Succeeded) {
            EditorApplication.Exit(1);
        }
    }
}
