using System;
using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>Repeatable local QA packaging. Never publishes, overwrites a prior build or creates game state.</summary>
public static class ForgeborneWindowsBuild
{
    [Serializable]
    private sealed class Evidence
    {
        public string unityVersion;
        public string productVersion;
        public string output;
        public string backend;
        public string options;
        public string result;
        public int errors;
        public int warnings;
        public ulong bytes;
        public double seconds;
        public string exception;
    }

    [MenuItem("Forgeborne/Build Windows QA Player")]
    public static void Request()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode || EditorApplication.isCompiling)
            throw new InvalidOperationException("Stop Play and finish imports before building.");
        for (int index = 0; index < SceneManager.sceneCount; index++)
            if (SceneManager.GetSceneAt(index).isDirty)
                throw new InvalidOperationException("Save authored scenes before building.");
        if (!BuildPipeline.IsBuildTargetSupported(BuildTargetGroup.Standalone, BuildTarget.StandaloneWindows64))
            throw new InvalidOperationException("Install Unity Windows x64 build support first.");
        string folder = Path.GetFullPath(Path.Combine(Application.dataPath, "../Builds/Windows-QA", DateTime.UtcNow.ToString("yyyyMMddTHHmmssZ")));
        if (Directory.Exists(folder)) throw new InvalidOperationException("Build directory already exists; prior evidence was kept.");
        Directory.CreateDirectory(folder);
        File.WriteAllText(Path.GetFullPath(Path.Combine(Application.dataPath, "../Library/forgeborne-last-build.txt")), folder);
        EditorApplication.delayCall += () => Build(folder);
        Debug.Log("Forgeborne Windows QA build requested: " + folder);
    }

    static void Build(string folder)
    {
        var evidence = new Evidence
        {
            unityVersion = Application.unityVersion,
            productVersion = PlayerSettings.bundleVersion,
            output = Path.Combine(folder, "Forgeborne.exe"),
            backend = PlayerSettings.GetScriptingBackend(UnityEditor.Build.NamedBuildTarget.Standalone).ToString(),
            options = BuildOptions.None.ToString(),
            result = "Failed",
        };
        try
        {
            var report = BuildPipeline.BuildPlayer(new BuildPlayerOptions
            {
                scenes = new[] { ForgeborneSceneBuilder.MainPath },
                locationPathName = evidence.output,
                target = BuildTarget.StandaloneWindows64,
                // The single-player QA executable needs no development player
                // connection or network listener. Profile release behavior separately.
                options = BuildOptions.None,
            });
            evidence.result = report.summary.result.ToString();
            evidence.errors = report.summary.totalErrors;
            evidence.warnings = report.summary.totalWarnings;
            evidence.bytes = report.summary.totalSize;
            evidence.seconds = report.summary.totalTime.TotalSeconds;
        }
        catch (Exception error)
        {
            evidence.exception = error.ToString();
            Debug.LogException(error);
        }
        finally
        {
            File.WriteAllText(Path.Combine(folder, "build-report.json"), JsonUtility.ToJson(evidence, true));
        }
        if (evidence.result != "Succeeded") Debug.LogError("Forgeborne Windows QA build failed; inspect its build-report.json.");
    }
}
