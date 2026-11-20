using System;
using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ATP.Editor
{
    /// <summary>
    /// One-click project preparation and Android APK build commands.
    /// Preparation is safe to run without Android build modules; APK build requires them.
    /// </summary>
    public static class AndroidBuildPipeline
    {
        private const string ScenePath = "Assets/Scenes/PrototypeDriving.unity";
        private const string OutputPath = "build/Android/BUS-COACH-SIMULATOR-AFRICA.apk";
        private const string BundleId = "com.buscoachsimulator.africa";

        [MenuItem("BUS & COACH SIMULATOR AFRICA/1 - Prepare Unity project")]
        public static void PrepareProject()
        {
            Directory.CreateDirectory("Assets/Scenes");

            SceneAsset existingScene = AssetDatabase.LoadAssetAtPath<SceneAsset>(ScenePath);
            if (existingScene == null)
            {
                Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
                GameObject bootstrap = new GameObject("Game Bootstrap");
                bootstrap.AddComponent<GameBootstrap>();
                EditorSceneManager.SaveScene(scene, ScenePath);
            }

            EditorBuildSettings.scenes = new[]
            {
                new EditorBuildSettingsScene(ScenePath, true)
            };

            PlayerSettings.companyName = "BUS & COACH SIMULATOR AFRICA";
            PlayerSettings.productName = "BUS & COACH SIMULATOR AFRICA";
            PlayerSettings.bundleVersion = "0.1.0";
            PlayerSettings.Android.bundleVersionCode = 1;
            PlayerSettings.SetApplicationIdentifier(BuildTargetGroup.Android, BundleId);
            PlayerSettings.defaultInterfaceOrientation = UIOrientation.LandscapeLeft;
            PlayerSettings.allowedAutorotateToLandscapeLeft = true;
            PlayerSettings.allowedAutorotateToLandscapeRight = true;
            PlayerSettings.allowedAutorotateToPortrait = false;
            PlayerSettings.allowedAutorotateToPortraitUpsideDown = false;
            PlayerSettings.Android.minSdkVersion = AndroidSdkVersions.AndroidApiLevel26;
            PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64;
            PlayerSettings.runInBackground = false;
            PlayerSettings.stripEngineCode = true;

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("Projet préparé : scène de conduite, scène de démarrage et paramètres Android/ paysage configurés.");
            EditorUtility.DisplayDialog("Projet préparé",
                "La scène de démarrage et les paramètres Android sont configurés.\n\nAucun APK n'a été compilé.",
                "OK");
        }

        [MenuItem("BUS & COACH SIMULATOR AFRICA/2 - Build Android APK")]
        public static void BuildAndroid()
        {
            PrepareProject();
            Directory.CreateDirectory("build/Android");

            bool switched = EditorUserBuildSettings.SwitchActiveBuildTarget(
                BuildTargetGroup.Android, BuildTarget.Android);
            if (!switched)
                throw new InvalidOperationException(
                    "Impossible d'activer Android. Vérifiez que Android Build Support, SDK, NDK et OpenJDK sont installés dans Unity Hub.");

            BuildPlayerOptions options = new BuildPlayerOptions
            {
                scenes = new[] { ScenePath },
                locationPathName = OutputPath,
                target = BuildTarget.Android,
                options = BuildOptions.None
            };

            BuildReport report = BuildPipeline.BuildPlayer(options);
            if (report.summary.result != BuildResult.Succeeded)
                throw new Exception("Android build failed: " + report.summary.result +
                    " — " + report.summary.totalErrors + " error(s).");

            Debug.Log("APK Android créé : " + Path.GetFullPath(OutputPath));
            EditorUtility.DisplayDialog("Compilation terminée",
                "APK créé :\n" + Path.GetFullPath(OutputPath), "OK");
        }
    }
}
