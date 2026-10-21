using System.IO;
using UnityEditor;
using UnityEngine;

namespace UiFramework.Editor.CodeGeneration
{
    public static class UiElementGenerator
    {
        private static string GetTemplateFolder()
        {
            string[] guids = AssetDatabase.FindAssets("UiElementTemplate t:TextAsset");

            if (guids.Length > 0)
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[0]);
                return Path.GetDirectoryName(path).Replace("\\", "/") + "/";
            }

            return "Packages/com.dawaniyahgames.uiframework/Editor/Templates/";
        }

        public static void Generate(string name, string outputPath, string ns, bool includeParams = false, bool includeReference = false)
        {
            if (!Directory.Exists(outputPath))
            {
                Directory.CreateDirectory(outputPath);
            }

            WriteFromTemplate("UiElementTemplate.txt", name, ns, outputPath, name);

            if (includeReference)
            {
                WriteFromTemplate("UiElementReferenceTemplate.txt", name, ns, outputPath, name + "Reference");
            }

            if (includeParams)
            {
                WriteFromTemplate("UiPopulationParameterTemplate.txt", name, ns, outputPath, name + "Params");
            }

            AssetDatabase.Refresh();
        }

        private static void WriteFromTemplate(string templateFile, string name, string ns, string outputPath, string className)
        {
            string outputFile = Path.Combine(outputPath, $"{className}.cs");
            if (File.Exists(outputFile))
            {
                Debug.LogWarning($"[UiFramework] {outputFile} already exists, so it was not generated again. Rename or delete it first if you want a fresh copy.");
                return;
            }

            string templatePath = Path.Combine(GetTemplateFolder(), templateFile);
            if (!File.Exists(templatePath))
            {
                Debug.LogError($"[UiFramework] Template not found at {templatePath}. Restore Editor/Templates from the package.");
                return;
            }

            string template = File.ReadAllText(templatePath);
            if (string.IsNullOrWhiteSpace(template))
            {
                Debug.LogWarning($"[UiFramework] Template '{templateFile}' is empty, so {className}.cs was not generated. Restore it from the package.");
                return;
            }

            string code = template
                .Replace("[UiElementName]", name)
                .Replace("[UiReferenceName]", name + "Reference")
                .Replace("[UiParamName]", name + "Params")
                .Replace("[UiElementNamespace]", ns)
                .Replace("[UiReferenceNamespace]", ns)
                .Replace("[UiParamNamespace]", ns);

            try
            {
                File.WriteAllText(outputFile, code);
                Debug.Log($"[UiFramework] Generated {outputFile}.");
            }
            catch (IOException exception)
            {
                Debug.LogError($"[UiFramework] Could not write {outputFile}: {exception.Message}");
            }
        }
    }
}
