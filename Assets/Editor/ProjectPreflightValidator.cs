using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.Build;
using UnityEngine;

namespace ATP.Editor
{
    /// <summary>Checks project files and Android editor support before the first device build.</summary>
    public static class ProjectPreflightValidator
    {
        [MenuItem("BUS & COACH SIMULATOR AFRICA/3 - Verify project readiness")]
        public static void VerifyProjectReadiness()
        {
            List<string> errors = new List<string>();
            List<string> warnings = new List<string>();

            RequireFile("ProjectSettings/ProjectVersion.txt", errors);
            RequireFile("Packages/manifest.json", errors);
            RequireFile("Assets/Scripts/Core/GameBootstrap.cs", errors);
            RequireFile("Assets/Scripts/Vehicle/CoachVehicleController.cs", errors);
            RequireFile("Assets/Scripts/Core/ActiveCoachSaveSystem.cs", errors);
            RequireFile("Assets/Scripts/Core/CompanySaveSystem.cs", errors);
            RequireFile("Assets/Scripts/UI/RoutePlanningPanel.cs", errors);

            if (!AssetDatabase.IsValidFolder("Assets/Scenes"))
                warnings.Add("Le dossier Assets/Scenes sera créé par la préparation du projet.");

            bool androidSupported = BuildPipeline.IsBuildTargetSupported(
                BuildTargetGroup.Android, BuildTarget.Android);
            if (!androidSupported)
                errors.Add("Module Android non détecté : installer Android Build Support, SDK, NDK et OpenJDK via Unity Hub.");

            string report = "Vérification du projet BUS & COACH SIMULATOR AFRICA\n\n";
            report += "Erreurs : " + errors.Count + "\n";
            for (int i = 0; i < errors.Count; i++) report += "• " + errors[i] + "\n";
            report += "\nAvertissements : " + warnings.Count + "\n";
            for (int i = 0; i < warnings.Count; i++) report += "• " + warnings[i] + "\n";

            if (errors.Count == 0)
            {
                Debug.Log(report);
                EditorUtility.DisplayDialog("Vérification terminée",
                    report + "\nLe projet passe les contrôles de présence des fichiers et de support Android. Une compilation et un test sur appareil restent obligatoires.",
                    "OK");
            }
            else
            {
                Debug.LogError(report);
                EditorUtility.DisplayDialog("Préparation incomplète", report, "OK");
            }
        }

        private static void RequireFile(string path, List<string> errors)
        {
            if (!File.Exists(path)) errors.Add("Fichier manquant : " + path);
        }
    }
}
