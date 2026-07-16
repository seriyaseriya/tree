using System.IO;
using TMPro;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using WoodClicker.Application;
using WoodClicker.Presentation.MainScreen;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem.UI;
#endif

namespace WoodClicker.Editor
{
    public static class MainSceneSetupTool
    {
        private const string SceneDirectory = "Assets/Scenes";
        private const string ScenePath = SceneDirectory + "/MainScene.unity";

        [MenuItem("Tools/WoodClicker/Create Basic Chopping Scene")]
        public static void CreateBasicChoppingScene()
        {
            if (File.Exists(ScenePath) && !EditorUtility.DisplayDialog(
                    "Overwrite MainScene?",
                    "Assets/Scenes/MainScene.unity already exists. Overwrite it?",
                    "Overwrite",
                    "Cancel"))
            {
                return;
            }

            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            {
                return;
            }

            EnsureSceneDirectoryExists();

            Scene scene = EditorSceneManager.NewScene(
                NewSceneSetup.EmptyScene,
                NewSceneMode.Single);

            CreateMainCamera();

            var gameRoot = new GameObject("GameRoot");
            ChoppingController choppingController =
                gameRoot.AddComponent<ChoppingController>();

            CreateEventSystem();

            Canvas canvas = CreateCanvas();
            RectTransform canvasTransform = canvas.GetComponent<RectTransform>();

            RectTransform safeArea = CreateUiObject("SafeArea", canvasTransform);
            SetStretch(safeArea);

            RectTransform header = CreateUiObject("Header", safeArea);
            header.anchorMin = new Vector2(0f, 1f);
            header.anchorMax = new Vector2(1f, 1f);
            header.pivot = new Vector2(0.5f, 1f);
            header.anchoredPosition = Vector2.zero;
            header.sizeDelta = new Vector2(0f, 180f);

            TextMeshProUGUI ownedLogsText = CreateText(
                "OwnedLogsText",
                header,
                "Logs: 0",
                64f);
            SetStretch(ownedLogsText.rectTransform, 40f, 40f, 20f, 20f);

            RectTransform mainArea = CreateUiObject("MainArea", safeArea);
            mainArea.anchorMin = Vector2.zero;
            mainArea.anchorMax = Vector2.one;
            mainArea.offsetMin = new Vector2(0f, 120f);
            mainArea.offsetMax = new Vector2(0f, -180f);

            Button treeButton = CreateTreeButton(mainArea);
            Image cooldownGauge = CreateCooldownGauge(mainArea);

            RectTransform viewTransform = CreateUiObject(
                "MainScreenView",
                canvasTransform);
            SetStretch(viewTransform);
            MainScreenView mainScreenView =
                viewTransform.gameObject.AddComponent<MainScreenView>();

            ConnectSerializedReferences(
                choppingController,
                mainScreenView,
                ownedLogsText,
                cooldownGauge);

            UnityEventTools.AddPersistentListener(
                treeButton.onClick,
                mainScreenView.OnTreeButtonClicked);

            EditorUtility.SetDirty(treeButton);
            EditorSceneManager.MarkSceneDirty(scene);

            if (!EditorSceneManager.SaveScene(scene, ScenePath))
            {
                Debug.LogError($"Failed to save scene at {ScenePath}.");
                return;
            }

            Selection.activeGameObject = gameRoot;
            Debug.Log($"Created and opened {ScenePath}.");
        }

        private static void EnsureSceneDirectoryExists()
        {
            if (!AssetDatabase.IsValidFolder(SceneDirectory))
            {
                AssetDatabase.CreateFolder("Assets", "Scenes");
            }
        }

        private static void CreateMainCamera()
        {
            var cameraObject = new GameObject("Main Camera");
            cameraObject.tag = "MainCamera";
            cameraObject.transform.position = new Vector3(0f, 0f, -10f);

            Camera camera = cameraObject.AddComponent<Camera>();
            camera.orthographic = true;
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.12f, 0.18f, 0.12f);

            cameraObject.AddComponent<AudioListener>();
        }

        private static void CreateEventSystem()
        {
            var eventSystemObject = new GameObject("EventSystem");
            eventSystemObject.AddComponent<EventSystem>();

#if ENABLE_INPUT_SYSTEM
            InputSystemUIInputModule inputModule =
                eventSystemObject.AddComponent<InputSystemUIInputModule>();
            inputModule.AssignDefaultActions();
#else
            eventSystemObject.AddComponent<StandaloneInputModule>();
#endif
        }

        private static Canvas CreateCanvas()
        {
            var canvasObject = new GameObject(
                "Canvas",
                typeof(RectTransform),
                typeof(Canvas),
                typeof(CanvasScaler),
                typeof(GraphicRaycaster));

            Canvas canvas = canvasObject.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;

            CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080f, 1920f);
            scaler.matchWidthOrHeight = 0.5f;

            return canvas;
        }

        private static Button CreateTreeButton(RectTransform parent)
        {
            RectTransform buttonTransform = CreateUiObject("TreeButton", parent);
            buttonTransform.anchorMin = new Vector2(0.5f, 0.5f);
            buttonTransform.anchorMax = new Vector2(0.5f, 0.5f);
            buttonTransform.pivot = new Vector2(0.5f, 0.5f);
            buttonTransform.anchoredPosition = new Vector2(0f, 120f);
            buttonTransform.sizeDelta = new Vector2(620f, 620f);

            Image image = buttonTransform.gameObject.AddComponent<Image>();
            image.color = new Color(0.35f, 0.65f, 0.25f);

            Button button = buttonTransform.gameObject.AddComponent<Button>();
            button.targetGraphic = image;

            TextMeshProUGUI label = CreateText(
                "Label",
                buttonTransform,
                "CHOP",
                72f);
            SetStretch(label.rectTransform, 20f, 20f, 20f, 20f);
            label.raycastTarget = false;

            return button;
        }

        private static Image CreateCooldownGauge(RectTransform parent)
        {
            RectTransform gaugeTransform = CreateUiObject("CooldownGauge", parent);
            gaugeTransform.anchorMin = new Vector2(0.5f, 0.5f);
            gaugeTransform.anchorMax = new Vector2(0.5f, 0.5f);
            gaugeTransform.pivot = new Vector2(0.5f, 0.5f);
            gaugeTransform.anchoredPosition = new Vector2(0f, -280f);
            gaugeTransform.sizeDelta = new Vector2(620f, 64f);

            Image image = gaugeTransform.gameObject.AddComponent<Image>();
            image.color = new Color(1f, 0.75f, 0.15f);
            image.type = Image.Type.Filled;
            image.fillMethod = Image.FillMethod.Horizontal;
            image.fillOrigin = (int)Image.OriginHorizontal.Left;
            image.fillAmount = 0f;

            return image;
        }

        private static TextMeshProUGUI CreateText(
            string name,
            RectTransform parent,
            string text,
            float fontSize)
        {
            RectTransform textTransform = CreateUiObject(name, parent);
            TextMeshProUGUI textComponent =
                textTransform.gameObject.AddComponent<TextMeshProUGUI>();
            textComponent.text = text;
            textComponent.fontSize = fontSize;
            textComponent.alignment = TextAlignmentOptions.Center;
            textComponent.color = Color.white;
            return textComponent;
        }

        private static RectTransform CreateUiObject(
            string name,
            RectTransform parent)
        {
            var gameObject = new GameObject(name, typeof(RectTransform));
            RectTransform rectTransform = gameObject.GetComponent<RectTransform>();
            rectTransform.SetParent(parent, false);
            return rectTransform;
        }

        private static void SetStretch(
            RectTransform rectTransform,
            float left = 0f,
            float right = 0f,
            float top = 0f,
            float bottom = 0f)
        {
            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchorMax = Vector2.one;
            rectTransform.offsetMin = new Vector2(left, bottom);
            rectTransform.offsetMax = new Vector2(-right, -top);
        }

        private static void ConnectSerializedReferences(
            ChoppingController choppingController,
            MainScreenView mainScreenView,
            TMP_Text ownedLogsText,
            Image cooldownGauge)
        {
            var controllerObject = new SerializedObject(choppingController);
            controllerObject.FindProperty("_mainScreenView").objectReferenceValue =
                mainScreenView;
            controllerObject.ApplyModifiedPropertiesWithoutUndo();

            var viewObject = new SerializedObject(mainScreenView);
            viewObject.FindProperty("_ownedLogsText").objectReferenceValue =
                ownedLogsText;
            viewObject.FindProperty("_cooldownGauge").objectReferenceValue =
                cooldownGauge;
            viewObject.ApplyModifiedPropertiesWithoutUndo();
        }
    }
}
