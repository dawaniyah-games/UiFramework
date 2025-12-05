using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor;
using UnityEditor.SceneManagement;
using UiFramework.Editor.Config;
using UiFramework.Editor.CodeGeneration;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using UnityEditor.UIElements;
using UnityEngine.SceneManagement;
using System;

namespace UiFramework.Editor.Window.Tabs
{
    public class ElementsTab : BaseVisualElementTab
    {
        public override string TabName
        {
            get
            {
                return "Elements";
            }
        }

        private UiEditorConfig editorConfig;
        private ScrollView sceneListScrollView;
        private bool includeParams = false;
        private bool includeReference = false;

        public override void OnCreateGUI(VisualElement root, UiEditorConfig config)
        {
            editorConfig = config;

            if (root == null)
            {
                return;
            }

            root.style.flexGrow = 1;

            Label header = new Label("UI Elements")
            {
                style =
                {
                    unityFontStyleAndWeight = FontStyle.Bold,
                    fontSize = 18,
                    marginTop = 18,
                    marginBottom = 18
                }
            };
            root.Add(header);

            sceneListScrollView = new ScrollView();
            sceneListScrollView.mode = ScrollViewMode.Vertical;
            sceneListScrollView.style.flexGrow = 1;
            sceneListScrollView.style.minHeight = 0;
            root.Add(sceneListScrollView);
            RefreshSceneList();

            SceneAsset selectedScene = null;

            ObjectField sceneField = new ObjectField("Optional Existing Scene")
            {
                objectType = typeof(SceneAsset),
                allowSceneObjects = false,
                style = { marginBottom = 6 }
            };
            sceneField.RegisterValueChangedCallback(evt =>
            {
                selectedScene = evt.newValue as SceneAsset;
            });
            root.Add(sceneField);

            TextField nameField = new TextField("New Element Name")
            {
                style = { marginTop = 10, marginBottom = 4 }
            };
            root.Add(nameField);

            Toggle paramsToggle = new Toggle("Include Parameters Class") { value = false };

            paramsToggle.RegisterValueChangedCallback(evt =>
            {
                includeParams = evt.newValue;
            });
            root.Add(paramsToggle);

            Toggle referenceToggle = new Toggle("Include Reference Class") { value = false };

            referenceToggle.RegisterValueChangedCallback(evt =>
            {
                includeReference = evt.newValue;
            });

            root.Add(referenceToggle);

            Button createButton = new Button(() =>
            {
                string manualName = nameField.value != null ? nameField.value.Trim() : string.Empty;
                string sceneName = string.Empty;

                if (selectedScene != null)
                {
                    string selectedPath = AssetDatabase.GetAssetPath(selectedScene);
                    sceneName = Path.GetFileNameWithoutExtension(selectedPath);
                }
                else
                {
                    sceneName = manualName;
                }

                if (!string.IsNullOrEmpty(sceneName))
                {
                    CreateUiElementFromScene(sceneName, selectedScene);
                    RefreshSceneList();
                }
            })
            {
                text = "\u2795 Create UI Element"
            };
            root.Add(createButton);
        }

        private void RefreshSceneList()
        {
            if (sceneListScrollView == null)
            {
                return;
            }

            sceneListScrollView.Clear();

            List<string> sceneList = GetElementScenes();
            if (sceneList.Count == 0)
            {
                Label noSceneLabel = new Label("\u26a0\ufe0f No UI element scenes found.")
                {
                    style = { color = Color.yellow, marginBottom = 6 }
                };
                sceneListScrollView.Add(noSceneLabel);
            }
            else
            {
                for (int i = 0; i < sceneList.Count; i++)
                {
                    string scene = sceneList[i];

                    Label label = new Label(scene)
                    {
                        style =
                        {
                            fontSize = 13,
                            unityFontStyleAndWeight = FontStyle.Bold,
                            marginBottom = 6,
                            paddingLeft = 8,
                            paddingTop = 4,
                            paddingBottom = 4,
                            backgroundColor = new Color(0.2f, 0.2f, 0.2f, 0.15f),
                            borderBottomWidth = 1,
                            borderBottomColor = Color.gray
                        }
                    };
                    sceneListScrollView.Add(label);
                }
            }
        }

        private void CreateUiElementFromScene(string elementName, SceneAsset sceneAsset)
        {
            string scriptPath = editorConfig != null ? editorConfig.ElementsScriptPath : string.Empty;
            string scenePath = editorConfig != null ? editorConfig.ElementsScenePath : string.Empty;
            string elementNamespace = editorConfig != null ? editorConfig.ElementNamespace : string.Empty;

            UiElementGenerator.Generate(elementName, scriptPath, elementNamespace, includeParams, includeReference);
            AssetDatabase.Refresh();

            EditorPrefs.SetString("UiElementAutoBind_Name", elementName);
            EditorPrefs.SetString("UiElementAutoBind_ScriptPath", scriptPath);

            Scene openedScene;

            if (sceneAsset != null)
            {
                string assetPath = AssetDatabase.GetAssetPath(sceneAsset);
                openedScene = EditorSceneManager.OpenScene(assetPath, OpenSceneMode.Single);
            }
            else
            {
                openedScene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene);
            }

            string rootName = elementName + "Root";
            GameObject rootGameObject = new GameObject(rootName);

            GameObject[] roots = openedScene.GetRootGameObjects();
            for (int i = 0; i < roots.Length; i++)
            {
                GameObject go = roots[i];
                if (go != rootGameObject)
                {
                    go.transform.SetParent(rootGameObject.transform);
                }
            }

            string expectedScriptFile = Path.Combine(scriptPath, elementName + ".cs");
            string[] guids = AssetDatabase.FindAssets(elementName + " t:MonoScript", new string[] { scriptPath });

            for (int i = 0; i < guids.Length; i++)
            {
                string guid = guids[i];
                string assetPath = AssetDatabase.GUIDToAssetPath(guid);
                if (Path.GetFileNameWithoutExtension(assetPath) == elementName)
                {
                    MonoScript monoScript = AssetDatabase.LoadAssetAtPath<MonoScript>(assetPath);
                    Type type = monoScript != null ? monoScript.GetClass() : null;

                    if (type != null && typeof(MonoBehaviour).IsAssignableFrom(type))
                    {
                        if (rootGameObject.GetComponent(type) == null)
                        {
                            rootGameObject.AddComponent(type);
                            EditorSceneManager.MarkSceneDirty(openedScene);
                            Debug.Log("✅ Attached script '" + type.Name + "' to '" + rootName + "'");
                        }
                    }
                }
            }

            string finalScenePath = Path.Combine(scenePath, elementName + ".unity");
            Directory.CreateDirectory(scenePath);
            EditorSceneManager.SaveScene(openedScene, finalScenePath);
            AssetDatabase.Refresh();

            Debug.Log("\u2705 Created/updated scene: " + finalScenePath + ". Script will be attached to '" + rootName + "' after compilation.");
        }

        private List<string> GetElementScenes()
        {
            if (editorConfig == null || string.IsNullOrEmpty(editorConfig.ElementsScenePath))
            {
                return new List<string>();
            }

            List<string> scenes = AssetDatabase.FindAssets("t:Scene", new string[] { editorConfig.ElementsScenePath })
                .Select(guid => AssetDatabase.GUIDToAssetPath(guid))
                .Where(path => path.EndsWith(".unity"))
                .Select(path => Path.GetFileNameWithoutExtension(path))
                .Distinct()
                .ToList();

            return scenes ?? new List<string>();
        }
    }
}
