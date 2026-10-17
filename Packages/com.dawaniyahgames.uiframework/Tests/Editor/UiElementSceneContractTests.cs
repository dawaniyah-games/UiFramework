using System.Collections.Generic;
using NUnit.Framework;
using UiFramework.Editor.Config;
using UiFramework.Editor.Scenes;
using UiFramework.Tests.Support;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace UiFramework.Editor.Tests
{
    // Rebuilds the consumer-project migration cases in memory and checks the reported issue codes and fixes.
    public class UiElementSceneContractTests
    {
        private const string tempFolder = "Assets/UiFrameworkContractTestTemp";

        private Scene scene;
        private UiGlobalCanvasSettings settings;

        [SetUp]
        public void SetUp()
        {
            scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            settings = ScriptableObject.CreateInstance<UiGlobalCanvasSettings>();

            // Single-mode scene loads unload unreferenced assets; keep the transient settings alive.
            settings.hideFlags = HideFlags.DontSave;
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(settings);
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            if (AssetDatabase.IsValidFolder(tempFolder))
            {
                AssetDatabase.DeleteAsset(tempFolder);
            }
        }

        // ---------- Scenes that must pass ----------

        [TestCase("Root")]
        [TestCase("IngameRoot")]
        [TestCase("ClubsUiParentGroup")]
        public void ContractHierarchy_WithAnyContentRootName_HasNoIssues(string contentRootName)
        {
            Canvas canvas = CreateCanvas(CreateElementRoot("LudoGameUiElementRoot", true));
            CreateStretched(contentRootName, canvas.transform);

            AssertCodes(Validate());
        }

        [Test]
        public void StretchedImageAsOnlyChild_HasNoIssues()
        {
            Canvas canvas = CreateCanvas(CreateElementRoot("LoadingUiElementRoot", true));
            RectTransform image = CreateStretched("Image", canvas.transform);
            image.gameObject.AddComponent<Image>();

            AssertCodes(Validate());
        }

        [Test]
        public void BuildContractHierarchy_ProducesValidScene()
        {
            GameObject elementRoot = CreateElementRoot("NewUiElementRoot", true);

            RectTransform contentRoot = UiElementStructureFixer.BuildContractHierarchy(elementRoot, settings);

            Assert.AreEqual("Root", contentRoot.name);
            Assert.AreEqual("Canvas", elementRoot.transform.GetChild(0).name);
            Assert.IsFalse(elementRoot.transform is RectTransform, "Element root must stay a plain Transform.");
            AssertCodes(Validate());
            AssertCanvasMatchesDefaults(elementRoot.transform.GetChild(0).GetComponent<Canvas>());
        }

        // ---------- Migration cases ----------

        [Test]
        public void ElementRootWithoutChildren_ReportsFixableNoCanvas()
        {
            CreateElementRoot("InboxUiElementRoot", true);

            UiElementStructureReport report = Validate();

            AssertCodes(report, UiElementStructureIssueCode.NoCanvas);
            Assert.IsTrue(report.Issues[0].IsFixable);
        }

        [Test]
        public void CanvasFirstChildIsPrefabInstanceWithSiblings_ReportsNoContentRoot()
        {
            Canvas canvas = CreateCanvas(CreateElementRoot("PlayWithFriendsUiElementRoot", true));
            InstantiatePrefab(CreateRectPrefab("FriendsPanel"), canvas.transform);
            CreateRect("Title", canvas.transform);
            CreateRect("CloseButton", canvas.transform);

            UiElementStructureReport report = Validate();

            AssertCodes(report, UiElementStructureIssueCode.NoContentRoot);
            Assert.IsTrue(report.HasFixableIssues);
        }

        [Test]
        public void OnlyCanvasChildIsPrefabInstance_ReportsNoContentRoot()
        {
            Canvas canvas = CreateCanvas(CreateElementRoot("WaitingForFriendToJoinUiElementRoot", true));
            InstantiatePrefab(CreateRectPrefab("WaitingPanel"), canvas.transform);

            AssertCodes(Validate(), UiElementStructureIssueCode.NoContentRoot);
        }

        [TestCase("Root", "TopPanel")]
        [TestCase("ManinMenuRoot", "refrence")]
        [TestCase("Overlay", "Content", "CoinFlyLayer")]
        public void CanvasWithSeveralDirectChildren_ReportsNoContentRoot(params string[] childNames)
        {
            Canvas canvas = CreateCanvas(CreateElementRoot("GameResultUiElementRoot", true));
            for (int i = 0; i < childNames.Length; i++)
            {
                CreateStretched(childNames[i], canvas.transform);
            }

            AssertCodes(Validate(), UiElementStructureIssueCode.NoContentRoot);
        }

        [Test]
        public void FixedSizeCenteredRoot_ReportsContentRootNotStretched()
        {
            Canvas canvas = CreateCanvas(CreateElementRoot("GameSettingsUiElementRoot", true));
            RectTransform root = CreateRect("Root", canvas.transform);
            root.sizeDelta = new Vector2(1080.0f, 1920.0f);

            UiElementStructureReport report = Validate();

            AssertCodes(report, UiElementStructureIssueCode.ContentRootNotStretched);
            StringAssert.Contains("anchors", report.Issues[0].Message);
        }

        [Test]
        public void TopAnchoredStrip_ReportsContentRootNotStretched()
        {
            Canvas canvas = CreateCanvas(CreateElementRoot("TopBarUiElementRoot", true));
            CreateTopStrip("Bar_bg", canvas.transform);

            AssertCodes(Validate(), UiElementStructureIssueCode.ContentRootNotStretched);
        }

        [Test]
        public void MissingUiElementAndSeveralCanvasChildren_ReportsNoUiElementAndNoContentRoot()
        {
            Canvas canvas = CreateCanvas(CreateElementRoot("ChooseLudoPlayerColorUIElementRoot", false));
            CreateStretched("Background", canvas.transform);
            CreateStretched("Colors", canvas.transform);

            UiElementStructureReport report = Validate();

            AssertCodes(report, UiElementStructureIssueCode.NoUiElement, UiElementStructureIssueCode.NoContentRoot);
            Assert.IsTrue(report.HasUnfixableIssues);
        }

        [Test]
        public void ExtraRootObjects_AreReportedPerObjectWithActiveState()
        {
            Canvas canvas = CreateCanvas(CreateElementRoot("EightBallGameUiElementRoot", true));
            CreateStretched("Root", canvas.transform);

            GameObject testCamera = new GameObject("testCamera", typeof(Camera));
            testCamera.SetActive(false);
            new GameObject("Directional Light", typeof(Light));

            UiElementStructureReport report = Validate();

            AssertCodes(report, UiElementStructureIssueCode.ExtraRootObjects, UiElementStructureIssueCode.ExtraRootObjects);
            StringAssert.Contains("'testCamera' (inactive)", report.Issues[0].Message);
            StringAssert.Contains("'Directional Light' (active)", report.Issues[1].Message);
            Assert.IsFalse(report.HasFixableIssues, "Extra root objects must never be auto-fixed.");
        }

        [Test]
        public void ContentRootWithOwnCanvas_ReportsNoContentRoot()
        {
            Canvas canvas = CreateCanvas(CreateElementRoot("NestedUiElementRoot", true));
            RectTransform nested = CreateStretched("Root", canvas.transform);
            nested.gameObject.AddComponent<Canvas>();

            AssertCodes(Validate(), UiElementStructureIssueCode.NoContentRoot);
        }

        [Test]
        public void InactiveContentRoot_IsWarningOnly()
        {
            Canvas canvas = CreateCanvas(CreateElementRoot("HiddenUiElementRoot", true));
            CreateStretched("Root", canvas.transform).gameObject.SetActive(false);

            UiElementStructureReport report = Validate();

            AssertCodes(report, UiElementStructureIssueCode.ContentRootInactive);
            Assert.IsTrue(report.Issues[0].IsWarning);
            Assert.IsFalse(report.HasUnfixableIssues);
        }

        [Test]
        public void CanvasMissingComponents_IsFixable()
        {
            Canvas canvas = CreateCanvas(CreateElementRoot("BareCanvasUiElementRoot", true), false, false);
            CreateStretched("Root", canvas.transform);

            UiElementStructureReport report = Validate();

            AssertCodes(report, UiElementStructureIssueCode.CanvasMissingComponents);
            Assert.IsTrue(report.Issues[0].IsFixable);
        }

        [Test]
        public void CanvasInsidePrefabInstance_IsReportedAndNotRestructured()
        {
            GameObject elementRoot = CreateElementRoot("PrefabCanvasUiElementRoot", true);
            GameObject canvasPrefab = CreateCanvasPrefabWithTwoChildren();
            InstantiatePrefab(canvasPrefab, elementRoot.transform);

            UiElementStructureReport report = Validate();

            AssertCodes(report, UiElementStructureIssueCode.CanvasInsidePrefab, UiElementStructureIssueCode.NoContentRoot);
            Assert.IsFalse(report.Find(UiElementStructureIssueCode.NoContentRoot).IsFixable);
        }

        // ---------- Fixes ----------

        [Test]
        public void Fix_WrapsCanvasChildrenInOriginalOrder_KeepingObjectsAndLocalRects()
        {
            Canvas canvas = CreateCanvas(CreateElementRoot("PlayWithFriendsUiElementRoot", true));
            GameObject prefabChild = InstantiatePrefab(CreateRectPrefab("FriendsPanel"), canvas.transform);
            RectTransform title = CreateTopStrip("Title", canvas.transform);
            RectTransform close = CreateRect("CloseButton", canvas.transform);
            close.anchoredPosition = new Vector2(-40.0f, 25.0f);
            close.localScale = new Vector3(1.5f, 1.5f, 1.0f);

            List<Transform> originalOrder = new List<Transform> { prefabChild.transform, title, close };
            Vector2 titleSize = title.sizeDelta;
            Vector2 titleAnchorMin = title.anchorMin;
            Vector2 closePosition = close.anchoredPosition;

            UiElementStructureReport remaining = UiElementStructureFixer.FixStructure(scene, settings);

            AssertCodes(remaining);
            Assert.AreEqual(1, canvas.transform.childCount);
            Transform root = canvas.transform.GetChild(0);
            Assert.AreEqual("Root", root.name);
            Assert.AreEqual(originalOrder.Count, root.childCount);

            for (int i = 0; i < originalOrder.Count; i++)
            {
                Assert.AreSame(originalOrder[i], root.GetChild(i), "Child order or identity changed at index " + i);
            }

            Assert.AreEqual(titleSize, title.sizeDelta);
            Assert.AreEqual(titleAnchorMin, title.anchorMin);
            Assert.AreEqual(closePosition, close.anchoredPosition);
            Assert.AreEqual(new Vector3(1.5f, 1.5f, 1.0f), close.localScale);
            Assert.IsTrue(PrefabUtility.IsAnyPrefabInstanceRoot(prefabChild), "Prefab instance must stay connected.");
        }

        [Test]
        public void Fix_WhenCanvasAlreadyHasChildNamedRoot_UsesContentRootName()
        {
            Canvas canvas = CreateCanvas(CreateElementRoot("SendGiftsMenuUiElementRoot", true));
            RectTransform existingRoot = CreateStretched("Root", canvas.transform);
            CreateTopStrip("TopPanel", canvas.transform);

            UiElementStructureReport remaining = UiElementStructureFixer.FixStructure(scene, settings);

            AssertCodes(remaining);
            Assert.AreEqual("ContentRoot", canvas.transform.GetChild(0).name);
            Assert.AreSame(existingRoot, canvas.transform.GetChild(0).GetChild(0));
        }

        [Test]
        public void Fix_NoChildren_CreatesCanvasAndRootWithSettings()
        {
            GameObject elementRoot = CreateElementRoot("EventsUiElenetsRoot", true);

            UiElementStructureReport remaining = UiElementStructureFixer.FixStructure(scene, settings);

            AssertCodes(remaining);
            Canvas canvas = elementRoot.transform.GetChild(0).GetComponent<Canvas>();
            Assert.IsNotNull(canvas);
            Assert.AreEqual("Root", canvas.transform.GetChild(0).name);
            AssertCanvasMatchesDefaults(canvas);
        }

        [Test]
        public void Fix_UiChildrenWithoutCanvas_MovesThemUnderCanvasAndRoot()
        {
            GameObject elementRoot = CreateElementRoot("ShopMenuUiElementRoot", true);
            RectTransform header = CreateTopStrip("Header", elementRoot.transform);
            RectTransform list = CreateStretched("List", elementRoot.transform);

            UiElementStructureReport remaining = UiElementStructureFixer.FixStructure(scene, settings);

            AssertCodes(remaining);
            Transform canvas = elementRoot.transform.GetChild(0);
            Transform root = canvas.GetChild(0);
            Assert.AreSame(header, root.GetChild(0));
            Assert.AreSame(list, root.GetChild(1));
        }

        [Test]
        public void Fix_MisplacedCanvas_IsMovedToFirstChild()
        {
            GameObject elementRoot = CreateElementRoot("MisplacedUiElementRoot", true);
            new GameObject("Helper").transform.SetParent(elementRoot.transform, false);
            Canvas canvas = CreateCanvas(elementRoot);
            CreateStretched("Root", canvas.transform);

            UiElementStructureReport report = Validate();
            AssertCodes(report, UiElementStructureIssueCode.NoCanvas);

            UiElementStructureReport remaining = UiElementStructureFixer.FixStructure(scene, settings);

            AssertCodes(remaining);
            Assert.AreSame(canvas.transform, elementRoot.transform.GetChild(0));
        }

        [Test]
        public void Fix_WrapMode_KeepsNonStretchedContentRootLayout()
        {
            Canvas canvas = CreateCanvas(CreateElementRoot("TopBarUiElementRoot", true));
            RectTransform bar = CreateTopStrip("Bar_bg", canvas.transform);
            Vector2 size = bar.sizeDelta;

            UiElementStructureReport remaining = UiElementStructureFixer.FixStructure(scene, settings, UiElementStructureFixMode.WrapContent);

            AssertCodes(remaining);
            Assert.AreSame(bar, canvas.transform.GetChild(0).GetChild(0));
            Assert.AreEqual(new Vector2(0.0f, 1.0f), bar.anchorMin);
            Assert.AreEqual(size, bar.sizeDelta);
        }

        [Test]
        public void Fix_ResetMode_StretchesExistingContentRoot()
        {
            Canvas canvas = CreateCanvas(CreateElementRoot("GameSettingsUiElementRoot", true));
            RectTransform root = CreateRect("Root", canvas.transform);
            root.sizeDelta = new Vector2(1080.0f, 1920.0f);

            UiElementStructureReport remaining = UiElementStructureFixer.FixStructure(scene, settings, UiElementStructureFixMode.ResetContentRoot);

            AssertCodes(remaining);
            Assert.AreEqual(1, canvas.transform.childCount);
            Assert.AreSame(root, canvas.transform.GetChild(0));
        }

        [Test]
        public void Fix_NeverDeletesObjects_AndLeavesUnfixableIssues()
        {
            Canvas canvas = CreateCanvas(CreateElementRoot("EightBallGameUiElementRoot", true));
            CreateStretched("A", canvas.transform);
            CreateStretched("B", canvas.transform);
            GameObject testCamera = new GameObject("testCamera", typeof(Camera));
            testCamera.SetActive(false);

            int objectCountBefore = CountObjects();

            UiElementStructureReport remaining = UiElementStructureFixer.FixStructure(scene, settings);

            AssertCodes(remaining, UiElementStructureIssueCode.ExtraRootObjects);
            Assert.AreEqual(objectCountBefore + 1, CountObjects(), "Only the new Root should have been added.");
            Assert.IsTrue(testCamera != null);
        }

        [Test]
        public void Fix_AddsMissingCanvasComponents()
        {
            Canvas canvas = CreateCanvas(CreateElementRoot("BareCanvasUiElementRoot", true), false, false);
            CreateStretched("Root", canvas.transform);

            UiElementStructureReport remaining = UiElementStructureFixer.FixStructure(scene, settings);

            AssertCodes(remaining);
            Assert.IsNotNull(canvas.GetComponent<GraphicRaycaster>());
            Assert.AreEqual(new Vector2(1080.0f, 1920.0f), canvas.GetComponent<CanvasScaler>().referenceResolution);
        }

        [Test]
        public void Fix_CanBeUndone()
        {
            Canvas canvas = CreateCanvas(CreateElementRoot("UndoUiElementRoot", true));
            RectTransform a = CreateStretched("A", canvas.transform);
            CreateStretched("B", canvas.transform);

            UiElementStructureFixer.FixStructure(scene, settings);
            Undo.PerformUndo();

            Assert.AreEqual(2, canvas.transform.childCount);
            Assert.AreSame(a, canvas.transform.GetChild(0));
        }

        // ---------- Canvas settings ----------

        [Test]
        public void ApplyCanvasSettings_ChangesOnlyAllowedFields()
        {
            Canvas canvas = CreateCanvas(CreateElementRoot("SortedUiElementRoot", true));
            CanvasScaler scaler = canvas.GetComponent<CanvasScaler>();
            canvas.sortingOrder = 7;
            canvas.planeDistance = 42.0f;
            canvas.additionalShaderChannels = AdditionalCanvasShaderChannels.TexCoord1;
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(800.0f, 600.0f);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.25f;
            scaler.referencePixelsPerUnit = 1.0f;

            UiElementSceneUtility.ApplyCanvasSettings(canvas, scaler, settings);

            Assert.AreEqual(7, canvas.sortingOrder);
            Assert.AreEqual(42.0f, canvas.planeDistance);
            Assert.AreEqual(AdditionalCanvasShaderChannels.TexCoord1, canvas.additionalShaderChannels);
            Assert.AreEqual(0.25f, scaler.matchWidthOrHeight, "matchWidthOrHeight is only written in MatchWidthOrHeight mode.");
            AssertCanvasMatchesDefaults(canvas);
        }

        [Test]
        public void CanvasSettingsMatch_ProducesReadableDiff()
        {
            Canvas canvas = CreateCanvas(CreateElementRoot("DiffUiElementRoot", true));
            CanvasScaler scaler = canvas.GetComponent<CanvasScaler>();
            UiElementSceneUtility.ApplyCanvasSettings(canvas, scaler, settings);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.referenceResolution = new Vector2(800.0f, 600.0f);

            string diff;
            bool matches = UiElementSceneUtility.CanvasSettingsMatch(canvas, scaler, settings, out diff);

            Assert.IsFalse(matches);
            StringAssert.Contains("ScreenMatchMode: MatchWidthOrHeight → Expand", diff);
            StringAssert.Contains("ReferenceResolution: 800x600 → 1080x1920", diff);
        }

        [Test]
        public void CanvasSettingsMatch_IgnoresMatchWidthOrHeight_UnlessMatchMode()
        {
            Canvas canvas = CreateCanvas(CreateElementRoot("MatchUiElementRoot", true));
            CanvasScaler scaler = canvas.GetComponent<CanvasScaler>();
            UiElementSceneUtility.ApplyCanvasSettings(canvas, scaler, settings);
            scaler.matchWidthOrHeight = 0.9f;

            string diff;
            Assert.IsTrue(UiElementSceneUtility.CanvasSettingsMatch(canvas, scaler, settings, out diff), diff);

            SerializedObject serializedSettings = new SerializedObject(settings);
            serializedSettings.FindProperty("screenMatchMode").enumValueIndex = (int)CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            serializedSettings.ApplyModifiedPropertiesWithoutUndo();
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;

            Assert.IsFalse(UiElementSceneUtility.CanvasSettingsMatch(canvas, scaler, settings, out diff));
            StringAssert.Contains("MatchWidthOrHeight: 0.9 → 0.5", diff);
        }

        // ---------- Scene access ----------

        [Test]
        public void WithScene_OpenScene_IsEditedInPlaceAndNotSavedOrClosed()
        {
            CreateElementRoot("OpenUiElementRoot", true);
            string path = SaveActiveScene("OpenUiElement");
            string fileBefore = System.IO.File.ReadAllText(path);

            bool ran = UiElementSceneUtility.WithScene(path, true, target =>
            {
                UiElementStructureFixer.FixStructure(target, settings);
            });

            Scene open = SceneManager.GetSceneByPath(path);
            Assert.IsTrue(ran);
            Assert.IsTrue(open.isLoaded, "Scene must stay open.");
            Assert.IsTrue(open.isDirty, "Scene must be left dirty for the user to save.");
            Assert.AreEqual(fileBefore, System.IO.File.ReadAllText(path), "Scene must not be saved.");
        }

        [Test]
        public void WithScene_ClosedScene_IsOpenedSavedAndClosed()
        {
            CreateElementRoot("ClosedUiElementRoot", true);
            string path = SaveActiveScene("ClosedUiElement");
            scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            bool ran = UiElementSceneUtility.WithScene(path, true, target =>
            {
                UiElementStructureFixer.FixStructure(target, settings);
            });

            Assert.IsTrue(ran);
            Assert.IsFalse(SceneManager.GetSceneByPath(path).IsValid(), "Scene must be closed again.");

            UiElementSceneInfo info = UiElementSceneUtility.InspectScene(path, settings);
            Assert.IsTrue(info.IsAvailable);
            Assert.IsTrue(info.HasUiElement);
            AssertCodes(info.Structure);
            Assert.AreEqual(UiCanvasStatus.Matches, info.CanvasStatus, info.CanvasDiff);
            Assert.IsFalse(SceneManager.GetSceneByPath(path).IsValid(), "InspectScene must close the scene.");
        }

        // ---------- Helpers ----------

        private UiElementStructureReport Validate()
        {
            UiElementStructureReport report;
            UiElementSceneUtility.ValidateStructure(scene, out report);
            return report;
        }

        private static void AssertCodes(UiElementStructureReport report, params UiElementStructureIssueCode[] expected)
        {
            List<UiElementStructureIssueCode> actual = new List<UiElementStructureIssueCode>();
            for (int i = 0; i < report.Issues.Count; i++)
            {
                actual.Add(report.Issues[i].Code);
            }

            CollectionAssert.AreEqual(expected, actual, "Issues were:\n" + report.GetIssuesText());
        }

        private static void AssertCanvasMatchesDefaults(Canvas canvas)
        {
            CanvasScaler scaler = canvas.GetComponent<CanvasScaler>();
            Assert.AreEqual(RenderMode.ScreenSpaceOverlay, canvas.renderMode);
            Assert.IsFalse(canvas.pixelPerfect);
            Assert.AreEqual(CanvasScaler.ScaleMode.ScaleWithScreenSize, scaler.uiScaleMode);
            Assert.AreEqual(new Vector2(1080.0f, 1920.0f), scaler.referenceResolution);
            Assert.AreEqual(CanvasScaler.ScreenMatchMode.Expand, scaler.screenMatchMode);
            Assert.AreEqual(100.0f, scaler.referencePixelsPerUnit);
        }

        private static GameObject CreateElementRoot(string name, bool withUiElement)
        {
            GameObject root = new GameObject(name);
            if (withUiElement)
            {
                root.AddComponent<TestUiElement>();
            }

            return root;
        }

        private static Canvas CreateCanvas(GameObject parent)
        {
            return CreateCanvas(parent, true, true);
        }

        private static Canvas CreateCanvas(GameObject parent, bool withScaler, bool withRaycaster)
        {
            GameObject canvasObject = new GameObject("Canvas", typeof(RectTransform), typeof(Canvas));
            if (withScaler)
            {
                canvasObject.AddComponent<CanvasScaler>();
            }

            if (withRaycaster)
            {
                canvasObject.AddComponent<GraphicRaycaster>();
            }

            canvasObject.transform.SetParent(parent.transform, false);
            return canvasObject.GetComponent<Canvas>();
        }

        private static RectTransform CreateRect(string name, Transform parent)
        {
            GameObject child = new GameObject(name, typeof(RectTransform));
            child.transform.SetParent(parent, false);
            return child.GetComponent<RectTransform>();
        }

        private static RectTransform CreateStretched(string name, Transform parent)
        {
            RectTransform rectTransform = CreateRect(name, parent);
            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchorMax = Vector2.one;
            rectTransform.offsetMin = Vector2.zero;
            rectTransform.offsetMax = Vector2.zero;
            return rectTransform;
        }

        private static RectTransform CreateTopStrip(string name, Transform parent)
        {
            RectTransform rectTransform = CreateRect(name, parent);
            rectTransform.anchorMin = new Vector2(0.0f, 1.0f);
            rectTransform.anchorMax = Vector2.one;
            rectTransform.pivot = new Vector2(0.5f, 1.0f);
            rectTransform.sizeDelta = new Vector2(0.0f, 200.0f);
            rectTransform.anchoredPosition = Vector2.zero;
            return rectTransform;
        }

        private static GameObject CreateRectPrefab(string name)
        {
            EnsureTempFolder();
            GameObject source = new GameObject(name, typeof(RectTransform));
            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(source, tempFolder + "/" + name + ".prefab");
            Object.DestroyImmediate(source);
            return prefab;
        }

        private static GameObject CreateCanvasPrefabWithTwoChildren()
        {
            EnsureTempFolder();
            GameObject source = new GameObject("Canvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            CreateStretched("A", source.transform);
            CreateStretched("B", source.transform);
            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(source, tempFolder + "/PrefabCanvas.prefab");
            Object.DestroyImmediate(source);
            return prefab;
        }

        private static GameObject InstantiatePrefab(GameObject prefab, Transform parent)
        {
            GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
            instance.transform.SetParent(parent, false);
            return instance;
        }

        private string SaveActiveScene(string name)
        {
            EnsureTempFolder();
            string path = tempFolder + "/" + name + ".unity";
            Assert.IsTrue(EditorSceneManager.SaveScene(scene, path));
            return path;
        }

        private int CountObjects()
        {
            int count = 0;
            GameObject[] roots = scene.GetRootGameObjects();
            for (int i = 0; i < roots.Length; i++)
            {
                count += roots[i].GetComponentsInChildren<Transform>(true).Length;
            }

            return count;
        }

        private static void EnsureTempFolder()
        {
            if (!AssetDatabase.IsValidFolder(tempFolder))
            {
                AssetDatabase.CreateFolder("Assets", "UiFrameworkContractTestTemp");
            }
        }
    }
}
