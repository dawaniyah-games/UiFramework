using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace UiFramework.Editor.CodeGeneration
{
    public static class UiStateGenerator
    {
        private static string GetTemplatePath()
        {
            string[] guids = AssetDatabase.FindAssets("UiStateTemplate t:TextAsset");

            if (guids.Length > 0)
            {
                return AssetDatabase.GUIDToAssetPath(guids[0]);
            }

            return "Packages/com.dawaniyahgames.uiframework/Editor/Templates/UiStateTemplate.txt";
        }

        public static void Generate(string name, string outputPath, string namespaceName)
        {
            if (!Directory.Exists(outputPath))
            {
                Directory.CreateDirectory(outputPath);
            }

            string className = name;
            string filePath = Path.Combine(outputPath, className + ".cs");

            if (File.Exists(filePath))
            {
                Debug.LogWarning($"[UiFramework] {filePath} already exists, so it was not generated again. Rename or delete it first if you want a fresh copy.");
                return;
            }

            string templatePath = GetTemplatePath();
            if (!File.Exists(templatePath))
            {
                Debug.LogError($"[UiFramework] Template not found at {templatePath}. Restore Editor/Templates from the package.");
                return;
            }

            string template = File.ReadAllText(templatePath);

            template = template.Replace("[UiStateNamespace]", namespaceName);
            template = template.Replace("[UiStateName]", className);

            File.WriteAllText(filePath, template);
            AssetDatabase.ImportAsset(filePath);
            Debug.Log($"[UiFramework] Generated UI state {filePath}.");
        }
    }
}
