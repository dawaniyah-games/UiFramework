using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace UiFramework.Editor.CodeGeneration
{
    public static class UiStateGenerator
    {
        private const string TemplatePath = "Assets/Scripts/UiFramework/Editor/Templates/UiStateTemplate.txt";

        public static void Generate(string name, string outputPath, string namespaceName)
        {
            Debug.Log($"⚠️ Output directory does not exist, creating: {outputPath}");


            if (!Directory.Exists(outputPath))
            {
                UnityEngine.Debug.LogWarning($"⚠️ Output directory does not exist, creating: {outputPath}");
                Directory.CreateDirectory(outputPath);
            }

            string className = name;
            string filePath = Path.Combine(outputPath, className + ".cs");

            if (File.Exists(filePath))
            {
                UnityEngine.Debug.LogWarning($"⚠️ File already exists: {filePath}");
                return;
            }

            if (!File.Exists(TemplatePath))
            {
                UnityEngine.Debug.LogError($"❌ Template file not found: {TemplatePath}");
                return;
            }

            string template = File.ReadAllText(TemplatePath);

            template = template.Replace("[UiStateNamespace]", namespaceName);
            template = template.Replace("[UiStateName]", className);

            File.WriteAllText(filePath, template);
            AssetDatabase.ImportAsset(filePath);
            UnityEngine.Debug.Log($"✅ Generated UI State: {filePath}");
        }
    }
}
