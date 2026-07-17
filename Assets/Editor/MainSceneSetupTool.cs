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
using WoodClicker.Application.Characters;
using WoodClicker.Application.Gacha;
using WoodClicker.Application.Save;
using WoodClicker.Presentation.Characters;
using WoodClicker.Presentation.Gacha;
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
        private const string BackgroundPath =
            "Assets/Art/Backgrounds/forest-clearing.png";
        private const string TreePath = "Assets/Art/Trees/giant-tree.png";
        private const string JapaneseFontAssetPath =
            "Assets/Art/Fonts/WoodClickerJapanese SDF.asset";
        private const string JapaneseFontResourcePath =
            "Assets/TextMesh Pro/Resources/Fonts & Materials/WoodClickerJapanese SDF.asset";
        private const string FontDirectory = "Assets/Art/Fonts";

        private static TMP_FontAsset _japaneseFontAsset;
        private static bool _missingJapaneseFontWarningIssued;

        private static readonly Color PanelColor =
            new Color(0.18f, 0.11f, 0.06f, 0.82f);
        private static readonly Color ButtonColor =
            new Color(0.38f, 0.24f, 0.12f, 0.95f);

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
            _japaneseFontAsset = GetOrCreateJapaneseFontAsset();
            ConfigureSpriteImporter(BackgroundPath, false);
            ConfigureSpriteImporter(TreePath, true);

            Scene scene = EditorSceneManager.NewScene(
                NewSceneSetup.EmptyScene,
                NewSceneMode.Single);

            CreateMainCamera();
            var gameRoot = new GameObject("GameRoot");
            ChoppingController controller =
                gameRoot.AddComponent<ChoppingController>();
            GachaController gachaController =
                gameRoot.AddComponent<GachaController>();
            CharacterCollectionController characterController =
                gameRoot.AddComponent<CharacterCollectionController>();
            SaveController saveController =
                gameRoot.AddComponent<SaveController>();
            CreateEventSystem();

            Canvas canvas = CreateCanvas();
            RectTransform canvasTransform = canvas.GetComponent<RectTransform>();
            CreateBackground(canvasTransform);

            RectTransform safeArea = CreateUiObject("SafeArea", canvasTransform);
            SetStretch(safeArea);

            HeaderReferences header = CreateCommonHeader(safeArea);
            RectTransform screenRoot = CreateUiObject("ScreenRoot", safeArea);
            SetStretch(screenRoot, 0f, 0f, 180f, 190f);

            ChoppingReferences chopping = CreateChoppingScreen(screenRoot);
            SellReferences sell = CreateSellScreen(screenRoot);
            GachaReferences gacha = CreateGachaScreen(screenRoot);
            CharacterCollectionReferences characters =
                CreateCharacterCollectionScreen(screenRoot);
            GameObject skillScreen = CreatePlaceholderScreen(
                "SkillTreeScreen", screenRoot, "スキル", "共同開発中");
            OptionsReferences options = CreateOptionsScreen(screenRoot);

            NavigationReferences navigation = CreateBottomNavigation(safeArea);

            RectTransform viewTransform = CreateUiObject(
                "MainScreenView", canvasTransform);
            SetStretch(viewTransform);
            MainScreenView mainView =
                viewTransform.gameObject.AddComponent<MainScreenView>();
            MainNavigationView navigationView =
                viewTransform.gameObject.AddComponent<MainNavigationView>();

            ConnectController(
                controller,
                mainView,
                gachaController,
                characterController,
                saveController);
            ConnectGachaController(gachaController, gacha.View);
            ConnectGachaView(gacha);
            ConnectCharacterController(characterController, characters.View);
            ConnectCharacterView(characters);
            ConnectMainView(mainView, header, chopping, sell);
            ConnectNavigationView(
                navigationView,
                chopping.Screen,
                sell.Screen,
                gacha.Screen,
                characters.Screen,
                skillScreen,
                options.Screen,
                navigation.ButtonImages);
            RegisterButtonEvents(
                mainView,
                navigationView,
                header.OptionsButton,
                chopping.TreeButton,
                sell.SellAllButton,
                gacha.View,
                gacha.DrawButton,
                options.CloseButton,
                navigation.Buttons);

            chopping.Screen.SetActive(true);
            sell.Screen.SetActive(false);
            gacha.Screen.SetActive(false);
            characters.Screen.SetActive(false);
            skillScreen.SetActive(false);
            options.Screen.SetActive(false);

            EditorSceneManager.MarkSceneDirty(scene);
            if (!EditorSceneManager.SaveScene(scene, ScenePath))
            {
                Debug.LogError($"Failed to save scene at {ScenePath}.");
                return;
            }

            Selection.activeGameObject = gameRoot;
            Debug.Log($"Created smartphone main UI at {ScenePath}.");
        }

        [MenuItem("Tools/WoodClicker/Apply Japanese Font To Main Scene")]
        public static void ApplyJapaneseFontToMainScene()
        {
            TMP_FontAsset fontAsset = GetOrCreateJapaneseFontAsset();
            if (fontAsset == null)
            {
                return;
            }

            if (!File.Exists(ScenePath))
            {
                Debug.LogWarning($"MainScene was not found at {ScenePath}.");
                return;
            }

            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            {
                return;
            }

            Scene scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            TMP_Text[] texts = Object.FindObjectsByType<TMP_Text>(
                FindObjectsInactive.Include,
                FindObjectsSortMode.None);
            int appliedCount = 0;
            foreach (TMP_Text text in texts)
            {
                if (text.gameObject.scene != scene || text.font == fontAsset)
                {
                    continue;
                }

                Undo.RecordObject(text, "Apply Japanese TMP Font");
                text.font = fontAsset;
                EditorUtility.SetDirty(text);
                appliedCount++;
            }

            EditorSceneManager.MarkSceneDirty(scene);
            if (!EditorSceneManager.SaveScene(scene, ScenePath))
            {
                Debug.LogError($"Failed to save scene at {ScenePath}.");
                return;
            }

            Debug.Log(
                $"Applied {fontAsset.name} to {appliedCount} TMP text components in MainScene.");
        }

        private static TMP_FontAsset GetOrCreateJapaneseFontAsset()
        {
            if (_japaneseFontAsset != null)
            {
                return _japaneseFontAsset;
            }

            TMP_FontAsset existing =
                AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(JapaneseFontAssetPath);
            if (existing == null)
            {
                existing = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(
                    JapaneseFontResourcePath);
            }

            if (existing != null)
            {
                existing.atlasPopulationMode = AtlasPopulationMode.Dynamic;
                existing.isMultiAtlasTexturesEnabled = true;
                EditorUtility.SetDirty(existing);
                AssetDatabase.SaveAssets();
                _japaneseFontAsset = existing;
                return existing;
            }

            Font sourceFont = FindJapaneseSourceFont();

            if (sourceFont == null)
            {
                WarnMissingJapaneseFontOnce();
                return null;
            }

            TMP_FontAsset created = TMP_FontAsset.CreateFontAsset(
                sourceFont,
                90,
                9,
                UnityEngine.TextCore.LowLevel.GlyphRenderMode.SDFAA,
                1024,
                1024,
                AtlasPopulationMode.Dynamic,
                true);
            if (created == null)
            {
                Debug.LogError(
                    $"Failed to create a TMP font asset from {AssetDatabase.GetAssetPath(sourceFont)}.");
                return null;
            }

            created.name = "WoodClickerJapanese SDF";
            created.atlasPopulationMode = AtlasPopulationMode.Dynamic;
            created.isMultiAtlasTexturesEnabled = true;
            created.atlasTexture.name = "WoodClickerJapanese SDF Atlas";
            created.material.name = "WoodClickerJapanese SDF Material";

            AssetDatabase.CreateAsset(created, JapaneseFontAssetPath);
            AssetDatabase.AddObjectToAsset(created.atlasTexture, created);
            AssetDatabase.AddObjectToAsset(created.material, created);
            EditorUtility.SetDirty(created);
            AssetDatabase.SaveAssets();
            AssetDatabase.ImportAsset(JapaneseFontAssetPath);

            _japaneseFontAsset = created;
            Debug.Log(
                $"Created dynamic Japanese TMP font asset at {JapaneseFontAssetPath}.");
            return created;
        }

        private static Font FindJapaneseSourceFont()
        {
            if (!AssetDatabase.IsValidFolder(FontDirectory))
            {
                return null;
            }

            string[] guids = AssetDatabase.FindAssets(
                "t:Font",
                new[] { FontDirectory });
            var fontPaths = new System.Collections.Generic.List<string>();
            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                string extension = Path.GetExtension(path);
                if (!extension.Equals(".ttf", System.StringComparison.OrdinalIgnoreCase) &&
                    !extension.Equals(".otf", System.StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                Font font = AssetDatabase.LoadAssetAtPath<Font>(path);
                if (font != null && SupportsJapanese(font))
                {
                    fontPaths.Add(path);
                }
            }

            fontPaths.Sort(System.StringComparer.OrdinalIgnoreCase);
            if (fontPaths.Count == 0)
            {
                return null;
            }

            if (fontPaths.Count == 1)
            {
                return AssetDatabase.LoadAssetAtPath<Font>(fontPaths[0]);
            }

            string[] preferredNameParts = {
                "Japanese", "JP", "Gothic", "Mincho", "Noto"
            };
            foreach (string path in fontPaths)
            {
                string fileName = Path.GetFileNameWithoutExtension(path);
                foreach (string namePart in preferredNameParts)
                {
                    if (fileName.IndexOf(
                            namePart,
                            System.StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        return AssetDatabase.LoadAssetAtPath<Font>(path);
                    }
                }
            }

            return AssetDatabase.LoadAssetAtPath<Font>(fontPaths[0]);
        }

        private static bool SupportsJapanese(Font font)
        {
            return font.HasCharacter('あ') &&
                   font.HasCharacter('ア') &&
                   font.HasCharacter('日');
        }

        private static void WarnMissingJapaneseFontOnce()
        {
            if (_missingJapaneseFontWarningIssued)
            {
                return;
            }

            _missingJapaneseFontWarningIssued = true;
            Debug.LogWarning(
                "No Japanese-compatible TTF or OTF font was found in " +
                "Assets/Art/Fonts. Place a licensed Japanese font there, then run " +
                "Tools > WoodClicker > Apply Japanese Font To Main Scene or " +
                "recreate MainScene. Existing font references were left unchanged.");
        }

        private static void ConfigureSpriteImporter(string path, bool hasAlpha)
        {
            if (!File.Exists(path))
            {
                Debug.LogWarning(
                    $"UI image was not found at {path}. A placeholder color will be used.");
                return;
            }

            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceSynchronousImport);
            if (!(AssetImporter.GetAtPath(path) is TextureImporter importer))
            {
                Debug.LogWarning($"Could not configure TextureImporter for {path}.");
                return;
            }

            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.alphaIsTransparency = hasAlpha;
            importer.isReadable = false;
            var textureSettings = new TextureImporterSettings();
            importer.ReadTextureSettings(textureSettings);
            textureSettings.spriteMeshType = SpriteMeshType.FullRect;
            importer.SetTextureSettings(textureSettings);
            importer.SaveAndReimport();
        }

        private static void CreateBackground(RectTransform canvas)
        {
            RectTransform root = CreateUiObject("BackgroundRoot", canvas);
            SetStretch(root);
            RectTransform imageTransform = CreateUiObject("ForestBackground", root);
            SetStretch(imageTransform);
            Image image = imageTransform.gameObject.AddComponent<Image>();
            image.color = new Color(0.16f, 0.28f, 0.16f, 1f);
            image.raycastTarget = false;
            Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(BackgroundPath);
            if (sprite != null)
            {
                image.sprite = sprite;
                image.color = Color.white;
                image.preserveAspect = true;
                AspectRatioFitter fitter =
                    imageTransform.gameObject.AddComponent<AspectRatioFitter>();
                fitter.aspectMode = AspectRatioFitter.AspectMode.EnvelopeParent;
                fitter.aspectRatio = sprite.rect.width / sprite.rect.height;
            }
        }

        private static HeaderReferences CreateCommonHeader(RectTransform parent)
        {
            RectTransform header = CreateUiObject("CommonHeader", parent);
            SetTopAnchored(header, 180f);

            RectTransform logsPanel = CreatePanel("OwnedLogsPanel", header, PanelColor);
            SetHorizontalCell(logsPanel, 0f, 0.41f, 20f, 8f);
            TMP_Text logsIcon = CreateText("LogsIcon", logsPanel, "丸太", 34f);
            SetLeftSection(logsIcon.rectTransform, 0.34f);
            TMP_Text logsText = CreateText("OwnedLogsText", logsPanel, "0", 48f);
            SetRightSection(logsText.rectTransform, 0.34f);

            RectTransform moneyPanel = CreatePanel("OwnedMoneyPanel", header, PanelColor);
            SetHorizontalCell(moneyPanel, 0.41f, 0.82f, 8f, 8f);
            TMP_Text moneyIcon = CreateText("MoneyIcon", moneyPanel, "お金", 34f);
            SetLeftSection(moneyIcon.rectTransform, 0.34f);
            TMP_Text moneyText = CreateText("OwnedMoneyText", moneyPanel, "0", 48f);
            SetRightSection(moneyText.rectTransform, 0.34f);

            Button optionsButton = CreateButton(
                "OptionsButton", header, "⚙", 52f, ButtonColor);
            RectTransform optionsTransform = optionsButton.GetComponent<RectTransform>();
            optionsTransform.anchorMin = new Vector2(0.84f, 0.15f);
            optionsTransform.anchorMax = new Vector2(0.98f, 0.85f);
            optionsTransform.offsetMin = Vector2.zero;
            optionsTransform.offsetMax = Vector2.zero;

            return new HeaderReferences(logsText, moneyText, optionsButton);
        }

        private static ChoppingReferences CreateChoppingScreen(RectTransform parent)
        {
            RectTransform screen = CreateUiObject("ChoppingScreen", parent);
            SetStretch(screen);

            RectTransform interaction = CreateUiObject("TreeInteractionArea", screen);
            interaction.anchorMin = new Vector2(0f, 0.30f);
            interaction.anchorMax = Vector2.one;
            interaction.offsetMin = Vector2.zero;
            interaction.offsetMax = Vector2.zero;

            RectTransform treeButtonTransform = CreateUiObject(
                "GiantTreeButton", interaction);
            treeButtonTransform.anchorMin = new Vector2(0.5f, 0.5f);
            treeButtonTransform.anchorMax = new Vector2(0.5f, 0.5f);
            treeButtonTransform.pivot = new Vector2(0.5f, 0.5f);
            treeButtonTransform.anchoredPosition = new Vector2(0f, -20f);
            treeButtonTransform.sizeDelta = new Vector2(920f, 1080f);
            Button treeButton = treeButtonTransform.gameObject.AddComponent<Button>();

            RectTransform treeImageTransform = CreateUiObject(
                "GiantTreeImage", treeButtonTransform);
            SetStretch(treeImageTransform);
            Image treeImage = treeImageTransform.gameObject.AddComponent<Image>();
            treeImage.color = new Color(0.30f, 0.55f, 0.22f, 1f);
            treeImage.preserveAspect = true;
            Sprite treeSprite = AssetDatabase.LoadAssetAtPath<Sprite>(TreePath);
            if (treeSprite != null)
            {
                treeImage.sprite = treeSprite;
                treeImage.color = Color.white;
            }
            treeButton.targetGraphic = treeImage;

            RectTransform woodcutter = CreateUiObject("MainWoodcutterRoot", interaction);
            SetStretch(woodcutter);
            RectTransform tapEffects = CreateUiObject("TapEffectRoot", interaction);
            SetStretch(tapEffects);

            RectTransform info = CreatePanel("ChoppingInfoPanel", screen, PanelColor);
            info.anchorMin = new Vector2(0.03f, 0.08f);
            info.anchorMax = new Vector2(0.97f, 0.30f);
            info.offsetMin = Vector2.zero;
            info.offsetMax = Vector2.zero;

            TMP_Text logsPerTap = CreateInfoRow(info, 0, "LogsPerTapLabel", "1タップ", "LogsPerTapText", "1");
            TMP_Text logsPerSecond = CreateInfoRow(info, 1, "LogsPerSecondLabel", "1秒あたり", "LogsPerSecondText", "0");
            TMP_Text tool = CreateInfoRow(info, 2, "EquippedToolLabel", "装備", "EquippedToolText", "なし");
            TMP_Text toolStatus = CreateInfoRow(info, 3, "ToolStatusLabel", "道具状態", "ToolStatusText", "正常");

            RectTransform cooldownPanel = CreatePanel("CooldownPanel", screen, PanelColor);
            cooldownPanel.anchorMin = new Vector2(0.03f, 0.01f);
            cooldownPanel.anchorMax = new Vector2(0.97f, 0.075f);
            cooldownPanel.offsetMin = Vector2.zero;
            cooldownPanel.offsetMax = Vector2.zero;
            RectTransform gaugeBackground = CreatePanel(
                "CooldownGaugeBackground", cooldownPanel,
                new Color(0.08f, 0.06f, 0.04f, 0.9f));
            SetStretch(gaugeBackground, 20f, 20f, 16f, 16f);
            RectTransform gaugeTransform = CreateUiObject(
                "CooldownGauge", gaugeBackground);
            SetStretch(gaugeTransform);
            Image gauge = gaugeTransform.gameObject.AddComponent<Image>();
            gauge.color = new Color(0.95f, 0.68f, 0.18f, 1f);
            gauge.type = Image.Type.Filled;
            gauge.fillMethod = Image.FillMethod.Horizontal;
            gauge.fillOrigin = (int)Image.OriginHorizontal.Left;
            gauge.fillAmount = 0f;
            TMP_Text cooldownText = CreateText(
                "CooldownText", cooldownPanel, "伐採可能", 30f);
            SetStretch(cooldownText.rectTransform);
            cooldownText.raycastTarget = false;

            return new ChoppingReferences(
                screen.gameObject, treeButton, gauge, cooldownText,
                logsPerTap, logsPerSecond, tool, toolStatus);
        }

        private static SellReferences CreateSellScreen(RectTransform parent)
        {
            RectTransform screen = CreateUiObject("SellScreen", parent);
            SetStretch(screen);
            RectTransform panel = CreatePanel("SellPanel", screen, PanelColor);
            panel.anchorMin = new Vector2(0.08f, 0.16f);
            panel.anchorMax = new Vector2(0.92f, 0.84f);
            panel.offsetMin = Vector2.zero;
            panel.offsetMax = Vector2.zero;

            CreateAnchoredText("Title", panel, "換金", 64f, 0.82f, 0.98f);
            TMP_Text owned = CreateValueLine(panel, "SellOwnedLogs", "所持丸太", "0", 0.66f);
            TMP_Text basePrice = CreateValueLine(panel, "BaseSellPrice", "1本あたり", "1", 0.53f);
            TMP_Text finalAmount = CreateValueLine(panel, "FinalSellAmount", "換金予定額", "0", 0.40f);
            CreateValueLine(panel, "ActiveMerchant", "採用中の商人", "なし", 0.27f);

            Button sellButton = CreateButton(
                "SellAllButton", panel, "すべて換金", 52f,
                new Color(0.76f, 0.45f, 0.16f, 1f));
            RectTransform buttonTransform = sellButton.GetComponent<RectTransform>();
            buttonTransform.anchorMin = new Vector2(0.12f, 0.06f);
            buttonTransform.anchorMax = new Vector2(0.88f, 0.20f);
            buttonTransform.offsetMin = Vector2.zero;
            buttonTransform.offsetMax = Vector2.zero;

            return new SellReferences(
                screen.gameObject, owned, basePrice, finalAmount, sellButton);
        }

        private static GameObject CreatePlaceholderScreen(
            string name,
            RectTransform parent,
            string title,
            string message)
        {
            RectTransform screen = CreateUiObject(name, parent);
            SetStretch(screen);
            RectTransform panel = CreatePanel("ContentPanel", screen, PanelColor);
            panel.anchorMin = new Vector2(0.10f, 0.30f);
            panel.anchorMax = new Vector2(0.90f, 0.70f);
            panel.offsetMin = Vector2.zero;
            panel.offsetMax = Vector2.zero;
            CreateAnchoredText("Title", panel, title, 64f, 0.55f, 0.90f);
            CreateAnchoredText("Message", panel, message, 46f, 0.15f, 0.55f);
            return screen.gameObject;
        }

        private static GachaReferences CreateGachaScreen(RectTransform parent)
        {
            RectTransform screen = CreateUiObject("GachaScreen", parent);
            SetStretch(screen);
            GachaView view = screen.gameObject.AddComponent<GachaView>();

            RectTransform panel = CreatePanel("GachaPanel", screen, PanelColor);
            panel.anchorMin = new Vector2(0.08f, 0.08f);
            panel.anchorMax = new Vector2(0.92f, 0.92f);
            panel.offsetMin = Vector2.zero;
            panel.offsetMax = Vector2.zero;

            CreateAnchoredText("Title", panel, "ガチャ", 64f, 0.86f, 0.98f);
            TMP_Text cost = CreateValueLine(
                panel, "GachaCost", "1回の料金", "100", 0.76f);
            TMP_Text money = CreateValueLine(
                panel, "GachaOwnedMoney", "現在の所持金", "0", 0.65f);

            RectTransform resultPanel = CreatePanel(
                "GachaResultPanel",
                panel,
                new Color(0.10f, 0.07f, 0.04f, 0.86f));
            resultPanel.anchorMin = new Vector2(0.07f, 0.25f);
            resultPanel.anchorMax = new Vector2(0.93f, 0.58f);
            resultPanel.offsetMin = Vector2.zero;
            resultPanel.offsetMax = Vector2.zero;
            TMP_Text characterName = CreateValueLine(
                resultPanel, "GachaCharacterName", "キャラクター", "-", 0.79f);
            TMP_Text rarity = CreateValueLine(
                resultPanel, "GachaRarity", "レアリティ", "-", 0.59f);
            TMP_Text acquisitionType = CreateValueLine(
                resultPanel, "GachaAcquisitionType", "入手区分", "-", 0.39f);
            TMP_Text ownedCount = CreateValueLine(
                resultPanel, "GachaOwnedCount", "現在の所持数", "0", 0.19f);

            TMP_Text error = CreateAnchoredText(
                "GachaErrorText", panel, string.Empty, 36f, 0.17f, 0.24f);
            error.color = new Color(1f, 0.55f, 0.45f, 1f);

            Button drawButton = CreateButton(
                "DrawOnceButton",
                panel,
                "1回引く",
                48f,
                new Color(0.76f, 0.45f, 0.16f, 1f));
            RectTransform buttonTransform = drawButton.GetComponent<RectTransform>();
            buttonTransform.anchorMin = new Vector2(0.15f, 0.04f);
            buttonTransform.anchorMax = new Vector2(0.85f, 0.15f);
            buttonTransform.offsetMin = Vector2.zero;
            buttonTransform.offsetMax = Vector2.zero;

            return new GachaReferences(
                screen.gameObject,
                view,
                cost,
                money,
                characterName,
                rarity,
                acquisitionType,
                ownedCount,
                error,
                drawButton);
        }

        private static CharacterCollectionReferences
            CreateCharacterCollectionScreen(RectTransform parent)
        {
            RectTransform screen = CreateUiObject("CharacterScreen", parent);
            SetStretch(screen);
            CharacterCollectionView view =
                screen.gameObject.AddComponent<CharacterCollectionView>();

            RectTransform panel = CreatePanel(
                "CharacterCollectionPanel", screen, PanelColor);
            panel.anchorMin = new Vector2(0.06f, 0.04f);
            panel.anchorMax = new Vector2(0.94f, 0.96f);
            panel.offsetMin = Vector2.zero;
            panel.offsetMax = Vector2.zero;

            CreateAnchoredText(
                "Title", panel, "キャラクター一覧", 58f, 0.87f, 0.98f);
            TMP_Text ownedTypeCount = CreateAnchoredText(
                "OwnedTypeCountText", panel, "所持種類数：0", 36f,
                0.80f, 0.87f, 0.08f, 0.92f);
            ownedTypeCount.alignment = TextAlignmentOptions.MidlineLeft;

            RectTransform scrollRoot = CreatePanel(
                "CharacterScrollView",
                panel,
                new Color(0.08f, 0.05f, 0.03f, 0.72f));
            scrollRoot.anchorMin = new Vector2(0.06f, 0.06f);
            scrollRoot.anchorMax = new Vector2(0.94f, 0.79f);
            scrollRoot.offsetMin = Vector2.zero;
            scrollRoot.offsetMax = Vector2.zero;
            ScrollRect scrollRect = scrollRoot.gameObject.AddComponent<ScrollRect>();
            scrollRect.horizontal = false;
            scrollRect.vertical = true;

            RectTransform viewport = CreateUiObject("Viewport", scrollRoot);
            SetStretch(viewport, 12f, 12f, 12f, 12f);
            Image viewportImage = viewport.gameObject.AddComponent<Image>();
            viewportImage.color = new Color(0f, 0f, 0f, 0.01f);
            viewport.gameObject.AddComponent<Mask>().showMaskGraphic = false;

            RectTransform content = CreateUiObject("Content", viewport);
            content.anchorMin = new Vector2(0f, 1f);
            content.anchorMax = new Vector2(1f, 1f);
            content.pivot = new Vector2(0.5f, 1f);
            content.anchoredPosition = Vector2.zero;
            content.sizeDelta = Vector2.zero;
            VerticalLayoutGroup layout =
                content.gameObject.AddComponent<VerticalLayoutGroup>();
            layout.spacing = 14f;
            layout.padding = new RectOffset(8, 8, 8, 8);
            layout.childControlHeight = true;
            layout.childControlWidth = true;
            layout.childForceExpandHeight = false;
            layout.childForceExpandWidth = true;
            ContentSizeFitter fitter =
                content.gameObject.AddComponent<ContentSizeFitter>();
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            scrollRect.viewport = viewport;
            scrollRect.content = content;

            TMP_Text emptyState = CreateAnchoredText(
                "EmptyStateText",
                scrollRoot,
                "まだキャラクターを持っていません\n" +
                "ガチャでキャラクターを仲間にしましょう",
                38f,
                0.30f,
                0.70f,
                0.08f,
                0.92f);
            emptyState.raycastTarget = false;

            return new CharacterCollectionReferences(
                screen.gameObject,
                view,
                ownedTypeCount,
                emptyState,
                content);
        }

        private static OptionsReferences CreateOptionsScreen(RectTransform parent)
        {
            RectTransform screen = CreateUiObject("OptionsScreen", parent);
            SetStretch(screen);
            RectTransform panel = CreatePanel("OptionsPanel", screen, PanelColor);
            panel.anchorMin = new Vector2(0.10f, 0.25f);
            panel.anchorMax = new Vector2(0.90f, 0.75f);
            panel.offsetMin = Vector2.zero;
            panel.offsetMax = Vector2.zero;
            CreateAnchoredText("Title", panel, "オプション", 64f, 0.68f, 0.92f);
            CreateAnchoredText("Message", panel, "準備中", 46f, 0.36f, 0.66f);
            Button close = CreateButton("CloseButton", panel, "閉じる", 44f, ButtonColor);
            RectTransform closeTransform = close.GetComponent<RectTransform>();
            closeTransform.anchorMin = new Vector2(0.20f, 0.10f);
            closeTransform.anchorMax = new Vector2(0.80f, 0.30f);
            closeTransform.offsetMin = Vector2.zero;
            closeTransform.offsetMax = Vector2.zero;
            return new OptionsReferences(screen.gameObject, close);
        }

        private static NavigationReferences CreateBottomNavigation(RectTransform parent)
        {
            RectTransform navigation = CreatePanel(
                "BottomNavigation", parent,
                new Color(0.10f, 0.07f, 0.04f, 0.98f));
            navigation.anchorMin = Vector2.zero;
            navigation.anchorMax = new Vector2(1f, 0f);
            navigation.pivot = new Vector2(0.5f, 0f);
            navigation.anchoredPosition = Vector2.zero;
            navigation.sizeDelta = new Vector2(0f, 190f);

            string[] names = {
                "ChoppingNavButton", "SellNavButton", "GachaNavButton",
                "CharacterNavButton", "SkillNavButton"
            };
            string[] labels = { "伐採", "換金", "ガチャ", "キャラ", "スキル" };
            var buttons = new Button[5];
            var images = new Image[5];
            for (int i = 0; i < buttons.Length; i++)
            {
                buttons[i] = CreateButton(names[i], navigation, labels[i], 36f, ButtonColor);
                RectTransform rect = buttons[i].GetComponent<RectTransform>();
                rect.anchorMin = new Vector2(i / 5f, 0f);
                rect.anchorMax = new Vector2((i + 1f) / 5f, 1f);
                rect.offsetMin = new Vector2(4f, 8f);
                rect.offsetMax = new Vector2(-4f, -8f);
                images[i] = buttons[i].GetComponent<Image>();
            }

            return new NavigationReferences(buttons, images);
        }

        private static void RegisterButtonEvents(
            MainScreenView mainView,
            MainNavigationView navigationView,
            Button optionsButton,
            Button treeButton,
            Button sellButton,
            GachaView gachaView,
            Button drawButton,
            Button closeOptionsButton,
            Button[] navigationButtons)
        {
            UnityEventTools.AddPersistentListener(
                treeButton.onClick, mainView.OnTreeButtonClicked);
            UnityEventTools.AddPersistentListener(
                sellButton.onClick, mainView.OnSellAllButtonClicked);
            UnityEventTools.AddPersistentListener(
                drawButton.onClick, gachaView.OnDrawButtonClicked);
            UnityEventTools.AddPersistentListener(
                optionsButton.onClick, navigationView.ShowOptionsScreen);
            UnityEventTools.AddPersistentListener(
                closeOptionsButton.onClick, navigationView.CloseOptionsScreen);
            UnityEventTools.AddPersistentListener(
                navigationButtons[0].onClick, navigationView.ShowChoppingScreen);
            UnityEventTools.AddPersistentListener(
                navigationButtons[1].onClick, navigationView.ShowSellScreen);
            UnityEventTools.AddPersistentListener(
                navigationButtons[2].onClick, navigationView.ShowGachaScreen);
            UnityEventTools.AddPersistentListener(
                navigationButtons[3].onClick, navigationView.ShowCharacterScreen);
            UnityEventTools.AddPersistentListener(
                navigationButtons[4].onClick, navigationView.ShowSkillTreeScreen);
        }

        private static void ConnectController(
            ChoppingController controller,
            MainScreenView view,
            GachaController gachaController,
            CharacterCollectionController characterController,
            SaveController saveController)
        {
            var serialized = new SerializedObject(controller);
            serialized.FindProperty("_mainScreenView").objectReferenceValue = view;
            serialized.FindProperty("_gachaController").objectReferenceValue =
                gachaController;
            serialized.FindProperty("_characterCollectionController")
                .objectReferenceValue = characterController;
            serialized.FindProperty("_saveController").objectReferenceValue =
                saveController;
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void ConnectCharacterController(
            CharacterCollectionController controller,
            CharacterCollectionView view)
        {
            var serialized = new SerializedObject(controller);
            SetObjectReference(serialized, "_view", view);
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void ConnectCharacterView(
            CharacterCollectionReferences characters)
        {
            var serialized = new SerializedObject(characters.View);
            SetObjectReference(
                serialized,
                "_ownedTypeCountText",
                characters.OwnedTypeCountText);
            SetObjectReference(
                serialized,
                "_emptyStateText",
                characters.EmptyStateText);
            SetObjectReference(serialized, "_contentRoot", characters.ContentRoot);
            SetObjectReference(serialized, "_fontAsset", _japaneseFontAsset);
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void ConnectGachaController(
            GachaController controller,
            GachaView view)
        {
            var serialized = new SerializedObject(controller);
            SetObjectReference(serialized, "_view", view);
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void ConnectGachaView(GachaReferences gacha)
        {
            var serialized = new SerializedObject(gacha.View);
            SetObjectReference(serialized, "_costText", gacha.CostText);
            SetObjectReference(serialized, "_ownedMoneyText", gacha.OwnedMoneyText);
            SetObjectReference(
                serialized, "_characterNameText", gacha.CharacterNameText);
            SetObjectReference(serialized, "_rarityText", gacha.RarityText);
            SetObjectReference(
                serialized, "_acquisitionTypeText", gacha.AcquisitionTypeText);
            SetObjectReference(serialized, "_ownedCountText", gacha.OwnedCountText);
            SetObjectReference(serialized, "_errorText", gacha.ErrorText);
            SetObjectReference(serialized, "_drawButton", gacha.DrawButton);
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void ConnectMainView(
            MainScreenView view,
            HeaderReferences header,
            ChoppingReferences chopping,
            SellReferences sell)
        {
            var serialized = new SerializedObject(view);
            SetObjectReference(serialized, "_ownedLogsText", header.OwnedLogsText);
            SetObjectReference(serialized, "_ownedMoneyText", header.OwnedMoneyText);
            SetObjectReference(serialized, "_cooldownGauge", chopping.CooldownGauge);
            SetObjectReference(serialized, "_cooldownText", chopping.CooldownText);
            SetObjectReference(serialized, "_logsPerTapText", chopping.LogsPerTapText);
            SetObjectReference(serialized, "_logsPerSecondText", chopping.LogsPerSecondText);
            SetObjectReference(serialized, "_equippedToolText", chopping.EquippedToolText);
            SetObjectReference(serialized, "_toolStatusText", chopping.ToolStatusText);
            SetObjectReference(serialized, "_sellOwnedLogsText", sell.OwnedLogsText);
            SetObjectReference(serialized, "_baseSellPriceText", sell.BasePriceText);
            SetObjectReference(serialized, "_finalSellAmountText", sell.FinalAmountText);
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void ConnectNavigationView(
            MainNavigationView view,
            GameObject chopping,
            GameObject sell,
            GameObject gacha,
            GameObject character,
            GameObject skill,
            GameObject options,
            Image[] buttonImages)
        {
            var serialized = new SerializedObject(view);
            SetObjectReference(serialized, "_choppingScreen", chopping);
            SetObjectReference(serialized, "_sellScreen", sell);
            SetObjectReference(serialized, "_gachaScreen", gacha);
            SetObjectReference(serialized, "_characterScreen", character);
            SetObjectReference(serialized, "_skillTreeScreen", skill);
            SetObjectReference(serialized, "_optionsScreen", options);
            SerializedProperty images =
                serialized.FindProperty("_navigationButtonImages");
            images.arraySize = buttonImages.Length;
            for (int i = 0; i < buttonImages.Length; i++)
            {
                images.GetArrayElementAtIndex(i).objectReferenceValue = buttonImages[i];
            }
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void SetObjectReference(
            SerializedObject serialized,
            string propertyName,
            Object value)
        {
            serialized.FindProperty(propertyName).objectReferenceValue = value;
        }

        private static TMP_Text CreateInfoRow(
            RectTransform parent,
            int index,
            string labelName,
            string label,
            string valueName,
            string value)
        {
            float rowTop = 1f - index * 0.25f;
            TMP_Text labelText = CreateText(labelName, parent, label, 32f);
            labelText.rectTransform.anchorMin = new Vector2(0.04f, rowTop - 0.23f);
            labelText.rectTransform.anchorMax = new Vector2(0.54f, rowTop);
            labelText.rectTransform.offsetMin = Vector2.zero;
            labelText.rectTransform.offsetMax = Vector2.zero;
            labelText.alignment = TextAlignmentOptions.MidlineLeft;
            TMP_Text valueText = CreateText(valueName, parent, value, 36f);
            valueText.rectTransform.anchorMin = new Vector2(0.56f, rowTop - 0.23f);
            valueText.rectTransform.anchorMax = new Vector2(0.96f, rowTop);
            valueText.rectTransform.offsetMin = Vector2.zero;
            valueText.rectTransform.offsetMax = Vector2.zero;
            valueText.alignment = TextAlignmentOptions.MidlineRight;
            return valueText;
        }

        private static TMP_Text CreateValueLine(
            RectTransform parent,
            string name,
            string label,
            string value,
            float centerY)
        {
            CreateAnchoredText(name + "Label", parent, label, 38f,
                centerY - 0.06f, centerY + 0.06f, 0.06f, 0.56f);
            return CreateAnchoredText(name + "Text", parent, value, 42f,
                centerY - 0.06f, centerY + 0.06f, 0.58f, 0.94f);
        }

        private static TMP_Text CreateAnchoredText(
            string name,
            RectTransform parent,
            string text,
            float size,
            float minY,
            float maxY,
            float minX = 0.05f,
            float maxX = 0.95f)
        {
            TMP_Text component = CreateText(name, parent, text, size);
            component.rectTransform.anchorMin = new Vector2(minX, minY);
            component.rectTransform.anchorMax = new Vector2(maxX, maxY);
            component.rectTransform.offsetMin = Vector2.zero;
            component.rectTransform.offsetMax = Vector2.zero;
            return component;
        }

        private static Button CreateButton(
            string name,
            RectTransform parent,
            string label,
            float fontSize,
            Color color)
        {
            RectTransform rect = CreateUiObject(name, parent);
            Image image = rect.gameObject.AddComponent<Image>();
            image.color = color;
            Button button = rect.gameObject.AddComponent<Button>();
            button.targetGraphic = image;
            TMP_Text text = CreateText("Label", rect, label, fontSize);
            SetStretch(text.rectTransform, 8f, 8f, 8f, 8f);
            text.raycastTarget = false;
            return button;
        }

        private static RectTransform CreatePanel(
            string name,
            RectTransform parent,
            Color color)
        {
            RectTransform rect = CreateUiObject(name, parent);
            Image image = rect.gameObject.AddComponent<Image>();
            image.color = color;
            return rect;
        }

        private static TMP_Text CreateText(
            string name,
            RectTransform parent,
            string text,
            float fontSize)
        {
            RectTransform rect = CreateUiObject(name, parent);
            TextMeshProUGUI component =
                rect.gameObject.AddComponent<TextMeshProUGUI>();
            component.text = text;
            component.fontSize = fontSize;
            component.alignment = TextAlignmentOptions.Center;
            component.color = Color.white;
            component.enableWordWrapping = false;
            TMP_FontAsset japaneseFont =
                _japaneseFontAsset != null
                    ? _japaneseFontAsset
                    : GetOrCreateJapaneseFontAsset();
            if (japaneseFont != null)
            {
                component.font = japaneseFont;
            }
            return component;
        }

        private static RectTransform CreateUiObject(
            string name,
            RectTransform parent)
        {
            var gameObject = new GameObject(name, typeof(RectTransform));
            RectTransform rect = gameObject.GetComponent<RectTransform>();
            rect.SetParent(parent, false);
            return rect;
        }

        private static void SetStretch(
            RectTransform rect,
            float left = 0f,
            float right = 0f,
            float top = 0f,
            float bottom = 0f)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = new Vector2(left, bottom);
            rect.offsetMax = new Vector2(-right, -top);
        }

        private static void SetTopAnchored(RectTransform rect, float height)
        {
            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchorMax = Vector2.one;
            rect.pivot = new Vector2(0.5f, 1f);
            rect.anchoredPosition = Vector2.zero;
            rect.sizeDelta = new Vector2(0f, height);
        }

        private static void SetHorizontalCell(
            RectTransform rect,
            float minX,
            float maxX,
            float horizontalMargin,
            float verticalMargin)
        {
            rect.anchorMin = new Vector2(minX, 0f);
            rect.anchorMax = new Vector2(maxX, 1f);
            rect.offsetMin = new Vector2(horizontalMargin, verticalMargin);
            rect.offsetMax = new Vector2(-horizontalMargin, -verticalMargin);
        }

        private static void SetLeftSection(RectTransform rect, float maxX)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = new Vector2(maxX, 1f);
            rect.offsetMin = new Vector2(12f, 4f);
            rect.offsetMax = new Vector2(-4f, -4f);
        }

        private static void SetRightSection(RectTransform rect, float minX)
        {
            rect.anchorMin = new Vector2(minX, 0f);
            rect.anchorMax = Vector2.one;
            rect.offsetMin = new Vector2(4f, 4f);
            rect.offsetMax = new Vector2(-12f, -4f);
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
            camera.backgroundColor = new Color(0.10f, 0.16f, 0.10f);
            cameraObject.AddComponent<AudioListener>();
        }

        private static void CreateEventSystem()
        {
            var eventSystemObject = new GameObject("EventSystem");
            eventSystemObject.AddComponent<EventSystem>();
#if ENABLE_INPUT_SYSTEM
            InputSystemUIInputModule module =
                eventSystemObject.AddComponent<InputSystemUIInputModule>();
            module.AssignDefaultActions();
#else
            eventSystemObject.AddComponent<StandaloneInputModule>();
#endif
        }

        private static Canvas CreateCanvas()
        {
            var canvasObject = new GameObject(
                "Canvas", typeof(RectTransform), typeof(Canvas),
                typeof(CanvasScaler), typeof(GraphicRaycaster));
            Canvas canvas = canvasObject.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080f, 1920f);
            scaler.matchWidthOrHeight = 0.5f;
            return canvas;
        }

        private sealed class HeaderReferences
        {
            public TMP_Text OwnedLogsText { get; }
            public TMP_Text OwnedMoneyText { get; }
            public Button OptionsButton { get; }
            public HeaderReferences(TMP_Text logs, TMP_Text money, Button options)
            {
                OwnedLogsText = logs;
                OwnedMoneyText = money;
                OptionsButton = options;
            }
        }

        private sealed class ChoppingReferences
        {
            public GameObject Screen { get; }
            public Button TreeButton { get; }
            public Image CooldownGauge { get; }
            public TMP_Text CooldownText { get; }
            public TMP_Text LogsPerTapText { get; }
            public TMP_Text LogsPerSecondText { get; }
            public TMP_Text EquippedToolText { get; }
            public TMP_Text ToolStatusText { get; }
            public ChoppingReferences(
                GameObject screen, Button treeButton, Image gauge,
                TMP_Text cooldown, TMP_Text perTap, TMP_Text perSecond,
                TMP_Text tool, TMP_Text status)
            {
                Screen = screen;
                TreeButton = treeButton;
                CooldownGauge = gauge;
                CooldownText = cooldown;
                LogsPerTapText = perTap;
                LogsPerSecondText = perSecond;
                EquippedToolText = tool;
                ToolStatusText = status;
            }
        }

        private sealed class SellReferences
        {
            public GameObject Screen { get; }
            public TMP_Text OwnedLogsText { get; }
            public TMP_Text BasePriceText { get; }
            public TMP_Text FinalAmountText { get; }
            public Button SellAllButton { get; }
            public SellReferences(
                GameObject screen, TMP_Text logs, TMP_Text price,
                TMP_Text amount, Button button)
            {
                Screen = screen;
                OwnedLogsText = logs;
                BasePriceText = price;
                FinalAmountText = amount;
                SellAllButton = button;
            }
        }

        private sealed class OptionsReferences
        {
            public GameObject Screen { get; }
            public Button CloseButton { get; }
            public OptionsReferences(GameObject screen, Button button)
            {
                Screen = screen;
                CloseButton = button;
            }
        }

        private sealed class GachaReferences
        {
            public GameObject Screen { get; }
            public GachaView View { get; }
            public TMP_Text CostText { get; }
            public TMP_Text OwnedMoneyText { get; }
            public TMP_Text CharacterNameText { get; }
            public TMP_Text RarityText { get; }
            public TMP_Text AcquisitionTypeText { get; }
            public TMP_Text OwnedCountText { get; }
            public TMP_Text ErrorText { get; }
            public Button DrawButton { get; }

            public GachaReferences(
                GameObject screen,
                GachaView view,
                TMP_Text cost,
                TMP_Text money,
                TMP_Text characterName,
                TMP_Text rarity,
                TMP_Text acquisitionType,
                TMP_Text ownedCount,
                TMP_Text error,
                Button drawButton)
            {
                Screen = screen;
                View = view;
                CostText = cost;
                OwnedMoneyText = money;
                CharacterNameText = characterName;
                RarityText = rarity;
                AcquisitionTypeText = acquisitionType;
                OwnedCountText = ownedCount;
                ErrorText = error;
                DrawButton = drawButton;
            }
        }

        private sealed class CharacterCollectionReferences
        {
            public GameObject Screen { get; }
            public CharacterCollectionView View { get; }
            public TMP_Text OwnedTypeCountText { get; }
            public TMP_Text EmptyStateText { get; }
            public RectTransform ContentRoot { get; }

            public CharacterCollectionReferences(
                GameObject screen,
                CharacterCollectionView view,
                TMP_Text ownedTypeCountText,
                TMP_Text emptyStateText,
                RectTransform contentRoot)
            {
                Screen = screen;
                View = view;
                OwnedTypeCountText = ownedTypeCountText;
                EmptyStateText = emptyStateText;
                ContentRoot = contentRoot;
            }
        }

        private sealed class NavigationReferences
        {
            public Button[] Buttons { get; }
            public Image[] ButtonImages { get; }
            public NavigationReferences(Button[] buttons, Image[] images)
            {
                Buttons = buttons;
                ButtonImages = images;
            }
        }
    }
}
