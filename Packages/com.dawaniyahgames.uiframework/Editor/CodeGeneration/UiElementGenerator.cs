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
                return System.IO.Path.GetDirectoryName(path).Replace("\\", "/") + "/";
            }
            return "Packages/com.dawaniyahgames.uiframework/Editor/Templates/";
        }

        public static void Generate(string name, string outputPath, string ns, bool includeParams = false, bool includeReference = false)
        {
            if (!Directory.Exists(outputPath))
                Directory.CreateDirectory(outputPath);

            WriteFromTemplate("UiElementTemplate.txt", name, ns, outputPath, name);

            if (includeReference)
                WriteFromTemplate("UiElementReferenceTemplate.txt", name, ns, outputPath, name + "Reference");

            if (includeParams)
                WriteFromTemplate("UiPopulationParameterTemplate.txt", name, ns, outputPath, name + "Params");

            AssetDatabase.Refresh();
        }

        private static void WriteFromTemplate(string templateFile, string name, string ns, string outputPath, string className)
        {
            string templatePath = Path.Combine(GetTemplateFolder(), templateFile);
            Debug.Log($"🔍 Looking for template at: {templatePath}");

            if (!File.Exists(templatePath))
            {
                Debug.LogError($"❌ Template not found: {templatePath}");
                return;
            }

            string template = File.ReadAllText(templatePath);
            if (string.IsNullOrWhiteSpace(template))
            {
                Debug.LogWarning($"⚠️ Template '{templateFile}' is empty.");
                return;
            }

            string code = template
                .Replace("[UiElementName]", name)
                .Replace("[UiReferenceName]", name + "Reference")
                .Replace("[UiParamName]", name + "Params")
                .Replace("[UiElementNamespace]", ns)
                .Replace("[UiReferenceNamespace]", ns)
                .Replace("[UiParamNamespace]", ns);

            string outputFile = Path.Combine(outputPath, $"{className}.cs");
            Debug.Log($"📄 Writing script to: {outputFile}");

            try
            {
                File.WriteAllText(outputFile, code);
                Debug.Log($"✅ Successfully generated: {outputFile}");
            }
            catch (IOException ex)
            {
                Debug.LogError($"❌ Failed to write file: {ex.Message}");
            }
        }
    }
}
