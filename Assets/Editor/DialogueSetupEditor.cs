using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.UI;
using TMPro;
using UnityEngine.InputSystem.UI;

public class DialogueSetupEditor : EditorWindow
{
    [MenuItem("Tools/Create Dialogue Scene")]
    public static void CreateDialogueScene()
    {
        // 0. 日本語フォントを自動検索
        TMP_FontAsset jpFont = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/Fonts/meiryo SDF.asset");
        if (jpFont != null)
        {
            Debug.Log($"✅ 日本語フォント検出: {jpFont.name}");
        }
        else
        {
            Debug.LogWarning("⚠ Assets/Fonts/meiryo SDF.asset が見つかりません。デフォルトフォントを使用します。");
        }

        // 1. 新しいシーンを作成
        var newScene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);

        // 2. EventSystemの作成 (UIのクリック判定に必須)
        var eventSystem = new GameObject("EventSystem");
        eventSystem.AddComponent<UnityEngine.EventSystems.EventSystem>();
        eventSystem.AddComponent<InputSystemUIInputModule>(); // 新Input System対応

        // 3. Canvasの作成
        var canvasObj = new GameObject("DialogueCanvas");
        var canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        var canvasScaler = canvasObj.AddComponent<CanvasScaler>();
        canvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        canvasScaler.referenceResolution = new Vector2(1920, 1080);
        canvasObj.AddComponent<GraphicRaycaster>();

        // 4. 背景画像の作成 (暗めのオーバーレイ)
        var bgObj = new GameObject("Background");
        bgObj.transform.SetParent(canvasObj.transform, false);
        var bgImage = bgObj.AddComponent<Image>();
        bgImage.color = new Color(0.1f, 0.1f, 0.1f, 0.5f);
        var bgRect = bgObj.GetComponent<RectTransform>();
        bgRect.anchorMin = Vector2.zero;
        bgRect.anchorMax = Vector2.one;
        bgRect.sizeDelta = Vector2.zero;

        // 5. 立ち絵 (Character Image) の作成
        var charObj = new GameObject("CharacterImage");
        charObj.transform.SetParent(canvasObj.transform, false);
        var charImage = charObj.AddComponent<Image>();
        charImage.color = Color.white;
        charImage.preserveAspect = true;
        var charRect = charObj.GetComponent<RectTransform>();
        charRect.anchorMin = new Vector2(0.15f, 0.1f);
        charRect.anchorMax = new Vector2(0.45f, 0.8f);
        charRect.offsetMin = Vector2.zero;
        charRect.offsetMax = Vector2.zero;

        // 6. テキストボックス背景の作成
        var textBoxObj = new GameObject("TextBoxBg");
        textBoxObj.transform.SetParent(canvasObj.transform, false);
        var textBoxImage = textBoxObj.AddComponent<Image>();
        textBoxImage.color = new Color(0, 0, 0, 0.8f);
        var tbRect = textBoxObj.GetComponent<RectTransform>();
        tbRect.anchorMin = new Vector2(0.1f, 0.05f);
        tbRect.anchorMax = new Vector2(0.9f, 0.3f);
        tbRect.offsetMin = Vector2.zero;
        tbRect.offsetMax = Vector2.zero;

        // 7. 名前テキスト (TextMeshPro)
        var nameObj = new GameObject("SpeakerName");
        nameObj.transform.SetParent(textBoxObj.transform, false);
        var nameText = nameObj.AddComponent<TextMeshProUGUI>();
        nameText.text = "名前";
        nameText.fontSize = 48;
        nameText.color = Color.white;
        nameText.alignment = TextAlignmentOptions.BottomLeft;
        if (jpFont != null)
        {
            nameText.font = jpFont;
            EditorUtility.SetDirty(nameText);
        }
        var nameRect = nameObj.GetComponent<RectTransform>();
        nameRect.anchorMin = new Vector2(0.02f, 1.0f);
        nameRect.anchorMax = new Vector2(0.3f, 1.3f);
        nameRect.offsetMin = Vector2.zero;
        nameRect.offsetMax = Vector2.zero;

        // 8. セリフテキスト (TextMeshPro)
        var dialogueObj = new GameObject("DialogueText");
        dialogueObj.transform.SetParent(textBoxObj.transform, false);
        var dialText = dialogueObj.AddComponent<TextMeshProUGUI>();
        dialText.text = "セリフがここに表示されます。";
        dialText.fontSize = 42;
        dialText.color = Color.white;
        dialText.alignment = TextAlignmentOptions.TopLeft;
        if (jpFont != null)
        {
            dialText.font = jpFont;
            EditorUtility.SetDirty(dialText);
        }
        var dialRect = dialogueObj.GetComponent<RectTransform>();
        dialRect.anchorMin = new Vector2(0.05f, 0.1f);
        dialRect.anchorMax = new Vector2(0.95f, 0.9f);
        dialRect.offsetMin = Vector2.zero;
        dialRect.offsetMax = Vector2.zero;

        // ============================
        // 9. FlashPanel (画面フラッシュ用)
        // ============================
        var flashObj = new GameObject("FlashPanel");
        flashObj.transform.SetParent(canvasObj.transform, false);
        var flashImage = flashObj.AddComponent<Image>();
        flashImage.color = new Color(1, 1, 1, 0);
        flashImage.raycastTarget = false; // クリックを貫通させる
        var flashRect = flashObj.GetComponent<RectTransform>();
        flashRect.anchorMin = Vector2.zero;
        flashRect.anchorMax = Vector2.one;
        flashRect.sizeDelta = Vector2.zero;
        flashObj.SetActive(false);

        // ============================
        // 10. 日にち表示テキスト (DayTextUI)
        // ============================
        var dayObj = new GameObject("DayTextUI");
        dayObj.transform.SetParent(canvasObj.transform, false);
        var dayTxt = dayObj.AddComponent<TextMeshProUGUI>();
        dayTxt.text = "一日目";
        dayTxt.fontSize = 42;
        dayTxt.color = new Color(1f, 0.9f, 0.6f, 1f); // 少しゴールドっぽい色
        dayTxt.alignment = TextAlignmentOptions.TopLeft;
        dayTxt.enableWordWrapping = false;
        if (jpFont != null) dayTxt.font = jpFont;

        var dayRect = dayObj.GetComponent<RectTransform>();
        dayRect.anchorMin = new Vector2(0.02f, 0.9f);
        dayRect.anchorMax = new Vector2(0.3f, 0.98f);
        dayRect.offsetMin = Vector2.zero;
        dayRect.offsetMax = Vector2.zero;

        // ============================
        // 11. 上部ボタンバー (スキップ/オート/ログ)
        // ============================
        var buttonBar = new GameObject("ButtonBar");
        buttonBar.transform.SetParent(canvasObj.transform, false);
        var barRect = buttonBar.GetComponent<RectTransform>();
        if (barRect == null) barRect = buttonBar.AddComponent<RectTransform>();
        barRect.anchorMin = new Vector2(0.55f, 0.92f);
        barRect.anchorMax = new Vector2(0.98f, 0.98f);
        barRect.offsetMin = Vector2.zero;
        barRect.offsetMax = Vector2.zero;
        var barLayout = buttonBar.AddComponent<HorizontalLayoutGroup>();
        barLayout.spacing = 10;
        barLayout.childAlignment = TextAnchor.MiddleRight;
        barLayout.childForceExpandWidth = true;
        barLayout.childForceExpandHeight = true;

        // スキップボタン
        var skipBtnObj = CreateUIButton(buttonBar.transform, "SkipButton", "スキップ", jpFont);
        var skipBtnText = skipBtnObj.GetComponentInChildren<TextMeshProUGUI>();

        // オートボタン
        var autoBtnObj = CreateUIButton(buttonBar.transform, "AutoButton", "オート", jpFont);
        var autoBtnText = autoBtnObj.GetComponentInChildren<TextMeshProUGUI>();

        // ログボタン
        var logBtnObj = CreateUIButton(buttonBar.transform, "LogButton", "ログ", jpFont);

        // ============================
        // 12. ログパネル (ScrollView)
        // ============================
        var logPanel = new GameObject("LogPanel");
        logPanel.transform.SetParent(canvasObj.transform, false);
        var logPanelImg = logPanel.AddComponent<Image>();
        logPanelImg.color = new Color(0, 0, 0, 0.9f);
        var logPanelRect = logPanel.GetComponent<RectTransform>();
        logPanelRect.anchorMin = new Vector2(0.1f, 0.1f);
        logPanelRect.anchorMax = new Vector2(0.9f, 0.9f);
        logPanelRect.offsetMin = Vector2.zero;
        logPanelRect.offsetMax = Vector2.zero;

        // ScrollView用のViewport
        var viewport = new GameObject("Viewport");
        viewport.transform.SetParent(logPanel.transform, false);
        var vpImg = viewport.AddComponent<Image>();
        vpImg.color = new Color(0, 0, 0, 0);
        var vpMask = viewport.AddComponent<Mask>();
        vpMask.showMaskGraphic = false;
        var vpRect = viewport.GetComponent<RectTransform>();
        vpRect.anchorMin = new Vector2(0.02f, 0.02f);
        vpRect.anchorMax = new Vector2(0.98f, 0.98f);
        vpRect.offsetMin = Vector2.zero;
        vpRect.offsetMax = Vector2.zero;

        // Content (ログエントリの親)
        var content = new GameObject("Content");
        content.transform.SetParent(viewport.transform, false);
        var contentRect = content.AddComponent<RectTransform>();
        contentRect.anchorMin = new Vector2(0, 1);
        contentRect.anchorMax = new Vector2(1, 1);
        contentRect.pivot = new Vector2(0.5f, 1);
        contentRect.sizeDelta = new Vector2(0, 0);
        var contentLayout = content.AddComponent<VerticalLayoutGroup>();
        contentLayout.spacing = 10;
        contentLayout.padding = new RectOffset(10, 10, 10, 10);
        contentLayout.childAlignment = TextAnchor.UpperLeft;
        contentLayout.childForceExpandWidth = true;
        contentLayout.childForceExpandHeight = false;
        var contentFitter = content.AddComponent<ContentSizeFitter>();
        contentFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        // ScrollRect設定
        var scrollRect = logPanel.AddComponent<ScrollRect>();
        scrollRect.content = contentRect;
        scrollRect.viewport = vpRect;
        scrollRect.horizontal = false;
        scrollRect.vertical = true;
        scrollRect.movementType = ScrollRect.MovementType.Clamped;

        // ログエントリのテンプレート（プレファブ代わり）
        var logEntryTemplate = new GameObject("LogEntryTemplate");
        logEntryTemplate.transform.SetParent(logPanel.transform, false);
        var leRect = logEntryTemplate.AddComponent<RectTransform>();
        leRect.sizeDelta = new Vector2(0, 80);
        var leTmp = logEntryTemplate.AddComponent<TextMeshProUGUI>();
        leTmp.text = "テンプレート";
        leTmp.fontSize = 28;
        leTmp.color = Color.white;
        leTmp.alignment = TextAlignmentOptions.TopLeft;
        if (jpFont != null)
        {
            leTmp.font = jpFont;
            EditorUtility.SetDirty(leTmp);
        }
        var leFitter = logEntryTemplate.AddComponent<LayoutElement>();
        leFitter.minHeight = 60;
        leFitter.preferredHeight = 80;
        logEntryTemplate.SetActive(false); // テンプレートは非表示

        logPanel.SetActive(false); // ログパネルは最初非表示

        // ============================
        // 13. DialogueController のセットアップ
        // ============================
        var managerObj = new GameObject("DialogueManager");
        var controller = managerObj.AddComponent<DialogueController>();
        controller.characterImage = charImage;
        controller.speakerNameText = nameText;
        controller.dialogueText = dialText;
        controller.flashPanel = flashImage;
        controller.dayTextUI = dayTxt;
        controller.canvasRect = canvasObj.GetComponent<RectTransform>();

        // ============================
        // 14. DialogueUIController のセットアップ
        // ============================
        var uiController = managerObj.AddComponent<DialogueUIController>();
        uiController.skipButton = skipBtnObj.GetComponent<Button>();
        uiController.autoButton = autoBtnObj.GetComponent<Button>();
        uiController.logButton = logBtnObj.GetComponent<Button>();
        uiController.logPanel = logPanel;
        uiController.logContentParent = content.transform;
        uiController.logEntryPrefab = logEntryTemplate;
        uiController.skipButtonText = skipBtnText;
        uiController.autoButtonText = autoBtnText;

        // キャラのImageをデフォルトでは非アクティブに
        charObj.SetActive(false);

        Debug.Log("【完了】会話シーンのセットアップが完了しました。");
        EditorUtility.DisplayDialog("成功", "会話シーンのセットアップが完了しました。\n\n新機能:\n・演出タグ対応 ([shake] [flash] [wait] [slow] [fast])\n・立ち位置対応 (left / center / right)\n・スキップ / オート / ログボタン\n\nシーンを保存してPlayボタンを押してみてください。", "OK");
    }

    /// <summary>UIボタンを生成するヘルパー</summary>
    static GameObject CreateUIButton(Transform parent, string name, string label, TMP_FontAsset font = null)
    {
        var btnObj = new GameObject(name);
        btnObj.transform.SetParent(parent, false);

        var btnImg = btnObj.AddComponent<Image>();
        btnImg.color = new Color(0.2f, 0.2f, 0.2f, 0.8f);
        var btn = btnObj.AddComponent<Button>();
        var colors = btn.colors;
        colors.highlightedColor = new Color(0.4f, 0.4f, 0.4f, 0.9f);
        colors.pressedColor = new Color(0.5f, 0.5f, 0.5f, 1f);
        btn.colors = colors;

        // LayoutElement で最小幅を設定
        var le = btnObj.AddComponent<LayoutElement>();
        le.minWidth = 120;
        le.preferredWidth = 150;

        // テキスト
        var textObj = new GameObject("Text");
        textObj.transform.SetParent(btnObj.transform, false);
        var tmp = textObj.AddComponent<TextMeshProUGUI>();
        tmp.text = label;
        tmp.fontSize = 28;
        tmp.color = Color.white;
        tmp.alignment = TextAlignmentOptions.Center;
        if (font != null)
        {
            tmp.font = font;
            EditorUtility.SetDirty(tmp);
        }
        var textRect = textObj.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;

        return btnObj;
    }
}
