using System;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace CacheCache.Editor
{
    public static class ProjectSetup
    {
        [MenuItem("Cache-Cache/Prepare project")]
        public static void Prepare()
        {
            const string scenePath = "Assets/CacheCache/Scenes/Main.unity";
            if (!File.Exists("Assets/TextMesh Pro/Resources/TMP Settings.asset"))
            {
                var package = UnityEditor.PackageManager.PackageInfo.FindForAssembly(typeof(TMPro.TMP_Settings).Assembly);
                if (package == null) throw new InvalidOperationException("TextMeshPro package missing.");
                AssetDatabase.ImportPackage(Path.Combine(package.resolvedPath,
                    "Package Resources/TMP Essential Resources.unitypackage"), false);
            }
            if (!File.Exists(scenePath))
            {
                if (!Application.isBatchMode && !EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
                Directory.CreateDirectory("Assets/CacheCache/Scenes");
                var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
                new GameObject("CacheCache").AddComponent<Bootstrap>();
                if (!EditorSceneManager.SaveScene(scene, scenePath)) throw new IOException("Cannot save Main scene.");
            }
            EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(scenePath, true) };
            PlayerSettings.defaultInterfaceOrientation = UIOrientation.LandscapeLeft;
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        public static void BuildLinux()
        {
            Prepare();
            Directory.CreateDirectory("Builds/Linux");
            var report = BuildPipeline.BuildPlayer(new BuildPlayerOptions {
                scenes = new[] { "Assets/CacheCache/Scenes/Main.unity" },
                locationPathName = "Builds/Linux/CacheCache.x86_64",
                target = BuildTarget.StandaloneLinux64,
                options = BuildOptions.None
            });
            if (report.summary.result != UnityEditor.Build.Reporting.BuildResult.Succeeded)
                throw new InvalidOperationException("Unity build failed: " + report.summary.result);
        }
    }
}
