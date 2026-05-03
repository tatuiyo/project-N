using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;

public class UpdateDialogueSceneEditor : EditorWindow
{
    [MenuItem("Tools/現在の会話シーンを最新にアップデート（ボタン等追加）")]
    public static void UpdateCurrentScene()
    {
        var manager = FindObjectOfType<DialogueController>();
        if (manager == null)
        {
            EditorUtility.DisplayDialog("エラー", "このシーンに DialogueManager が見つかりません。\n会話シーンを開いた状態で実行してください。", "OK");
            return;
        }

        var canvas = FindObjectOfType<Canvas>();
        if (canvas == null)
        {
            EditorUtility.DisplayDialog("エラー", "Canvasが見つかりません。", "OK");
            return;
        }

        // 日本語フォント取得
        TMP_FontAsset jpFont = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/Fonts/meiryo SDF.asset");

        // 1. FlashPanel の追加
        Transform flashTrans = canvas.transform.Find("FlashPanel");
        Image flashImage = null;
        if (flashTrans == null)
        {
            var flashObj = new GameObject("FlashPanel");
            flashObj.transform.SetParent(canvas.transform, false);
            flashObj.transform.SetSiblingIndex(2); // 後ろの方に配置
            flashImage = flashObj.AddComponent<Image>();
            flashImage.color = new Color(1, 1, 1, 0);
            flashImage.raycastTarget = false;
            var flashRect = flashObj.GetComponent<RectTransform>();
            flashRect.anchorMin = Vector2.zero;
            flashRect.anchorMax = Vector2.one;
            flashRect.sizeDelta = Vector2.zero;
            flashObj.SetActive(false);
            manager.flashPanel = flashImage;
        }

        // 2. ButtonBar の追加
        Transform barTrans = canvas.transform.Find("ButtonBar");
        GameObject buttonBar = null;
        if (barTrans == null)
        {
            buttonBar = new GameObject("ButtonBar");
            buttonBar.transform.SetParent(canvas.transform, false);
            var barRect = buttonBar.AddComponent<RectTransform>();
            barRect.anchorMin = new Vector2(0.55f, 0.92f);
            barRect.anchorMax = new Vector2(0.98f, 0.98f);
            barRect.offsetMin = Vector2.zero;
            barRect.offsetMax = Vector2.zero;
            var barLayout = buttonBar.AddComponent<HorizontalLayoutGroup>();
            barLayout.spacing = 10;
            barLayout.childAlignment = TextAnchor.MiddleRight;
            barLayout.childForceExpandWidth = true;
            barLayout.childForceExpandHeight = true;
        }
        else
        {
            buttonBar = barTrans.gameObject;
        }

        // ボタンの生成
        Button skipBtn = null, autoBtn = null, logBtn = null;
        TextMeshProUGUI skipTxt = null, autoTxt = null;

        if (buttonBar.transform.Find("SkipButton") == null)
        {
            var btnObj = CreateUIButton(buttonBar.transform, "SkipButton", "スキップ", jpFont);
            skipBtn = btnObj.GetComponent<Button>();
            skipTxt = btnObj.GetComponentInChildren<TextMeshProUGUI>();
        }
        else
        {
            skipBtn = buttonBar.transform.Find("SkipButton").GetComponent<Button>();
            skipTxt = skipBtn.GetComponentInChildren<TextMeshProUGUI>();
        }

        if (buttonBar.transform.Find("AutoButton") == null)
        {
            var btnObj = CreateUIButton(buttonBar.transform, "AutoButton", "オート", jpFont);
            autoBtn = btnObj.GetComponent<Button>();
            autoTxt = btnObj.GetComponentInChildren<TextMeshProUGUI>();
        }
        else
        {
            autoBtn = buttonBar.transform.Find("AutoButton").GetComponent<Button>();
            autoTxt = autoBtn.GetComponentInChildren<TextMeshProUGUI>();
        }

        if (buttonBar.transform.Find("LogButton") == null)
        {
            var btnObj = CreateUIButton(buttonBar.transform, "LogButton", "ログ", jpFont);
            logBtn = btnObj.GetComponent<Button>();
        }
        else
        {
            logBtn = buttonBar.transform.Find("LogButton").GetComponent<Button>();
        }

        // 3. LogPanel の追加
        Transform logTrans = canvas.transform.Find("LogPanel");
        GameObject logPanel = null;
        Transform contentTrans = null;
        GameObject logTemplate = null;

        if (logTrans == null)
        {
            logPanel = new GameObject("LogPanel");
            logPanel.transform.SetParent(canvas.transform, false);
            var logPanelImg = logPanel.AddComponent<Image>();
            logPanelImg.color = new Color(0, 0, 0, 0.95f); // 少し濃く
            var logPanelRect = logPanel.GetComponent<RectTransform>();
            logPanelRect.anchorMin = new Vector2(0.1f, 0.1f);
            logPanelRect.anchorMax = new Vector2(0.9f, 0.9f);
            logPanelRect.offsetMin = Vector2.zero;
            logPanelRect.offsetMax = Vector2.zero;

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

            var content = new GameObject("Content");
            contentTrans = content.transform;
            content.transform.SetParent(viewport.transform, false);
            var contentRect = content.AddComponent<RectTransform>();
            contentRect.anchorMin = new Vector2(0, 1);
            contentRect.anchorMax = new Vector2(1, 1);
            contentRect.pivot = new Vector2(0.5f, 1);
            contentRect.sizeDelta = new Vector2(0, 0);
            var contentLayout = content.AddComponent<VerticalLayoutGroup>();
            contentLayout.spacing = 15;
            contentLayout.padding = new RectOffset(20, 20, 20, 20);
            
            var contentFitter = content.AddComponent<ContentSizeFitter>();
            contentFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            var scrollRect = logPanel.AddComponent<ScrollRect>();
            scrollRect.content = contentRect;
            scrollRect.viewport = vpRect;
            scrollRect.horizontal = false;
            scrollRect.vertical = true;
            scrollRect.movementType = ScrollRect.MovementType.Clamped;
            // スクロール感度をアップ
            scrollRect.scrollSensitivity = 25f;

            logTemplate = new GameObject("LogEntryTemplate");
            logTemplate.transform.SetParent(logPanel.transform, false);
            var leRect = logTemplate.AddComponent<RectTransform>();
            leRect.sizeDelta = new Vector2(0, 80);
            var leTmp = logTemplate.AddComponent<TextMeshProUGUI>();
            leTmp.text = "テンプレート";
            leTmp.fontSize = 32;
            leTmp.color = Color.white;
            leTmp.alignment = TextAlignmentOptions.TopLeft;
            if (jpFont != null) leTmp.font = jpFont;
            var leFitter = logTemplate.AddComponent<LayoutElement>();
            logTemplate.SetActive(false);

            logPanel.SetActive(false);
        }
        else
        {
            logPanel = logTrans.gameObject;
            contentTrans = logPanel.transform.Find("Viewport/Content");
            logTemplate = logPanel.transform.Find("LogEntryTemplate").gameObject;
        }

        // --- ログの文字が潰れない・消えないように強制修正 ---
        if (contentTrans != null)
        {
            var contentLayout = contentTrans.GetComponent<VerticalLayoutGroup>();
            if (contentLayout != null)
            {
                contentLayout.childControlWidth = true;  
                contentLayout.childControlHeight = true; 
                contentLayout.childForceExpandWidth = true;
                contentLayout.childForceExpandHeight = false;
                EditorUtility.SetDirty(contentLayout);
            }

            // ViewportのMaskをRectMask2Dに修正（文字が透明になるバグ回避）
            var vp = contentTrans.parent.gameObject;
            var oldMask = vp.GetComponent<Mask>();
            if (oldMask != null) DestroyImmediate(oldMask);
            var vpImg = vp.GetComponent<Image>();
            if (vpImg != null) DestroyImmediate(vpImg);
            
            if (vp.GetComponent<UnityEngine.UI.RectMask2D>() == null)
            {
                vp.AddComponent<UnityEngine.UI.RectMask2D>();
                EditorUtility.SetDirty(vp);
            }
        }
        if (logTemplate != null)
        {
            var leFitter = logTemplate.GetComponent<LayoutElement>();
            if (leFitter != null)
            {
                leFitter.minHeight = 60;
                leFitter.preferredHeight = -1; // -1にすることで文字量に合わせて自動拡張される
                EditorUtility.SetDirty(leFitter);
            }
        }
        // ----------------------------------------------------

        // --- 5. DayTextUI の追加 ---
        Transform dayTrans = canvas.transform.Find("DayTextUI");
        TextMeshProUGUI dayTxt = null;
        if (dayTrans == null)
        {
            var dayObj = new GameObject("DayTextUI");
            dayObj.transform.SetParent(canvas.transform, false);
            // FlashPanelより手前（前面）に描画されるようにSiblingIndexを調整
            dayObj.transform.SetSiblingIndex(3);
            dayTxt = dayObj.AddComponent<TextMeshProUGUI>();
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
        }
        else
        {
            dayTxt = dayTrans.GetComponent<TextMeshProUGUI>();
        }

        // 4. UIController のアタッチ
        var uiController = manager.gameObject.GetComponent<DialogueUIController>();
        if (uiController == null) uiController = manager.gameObject.AddComponent<DialogueUIController>();

        uiController.skipButton = skipBtn;
        uiController.autoButton = autoBtn;
        uiController.logButton = logBtn;
        uiController.skipButtonText = skipTxt;
        uiController.autoButtonText = autoTxt;
        uiController.logPanel = logPanel;
        uiController.logContentParent = contentTrans;
        uiController.logEntryPrefab = logTemplate;

        manager.canvasRect = canvas.GetComponent<RectTransform>();
        manager.dayTextUI = dayTxt; // ← 日にちUIをセット

        // 全体を保存済みにする
        EditorUtility.SetDirty(manager);
        EditorUtility.SetDirty(uiController);
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(manager.gameObject.scene);

        Debug.Log("✅ 現在の会話シーンをアップデートしました！");
        EditorUtility.DisplayDialog("完了", "現在の会話シーンにスキップ・オート・ログUIを追加しました。\nCtrl+S でシーンを保存してPlayしてください。", "OK");
    }

    static GameObject CreateUIButton(Transform parent, string name, string label, TMP_FontAsset font)
    {
        var btnObj = new GameObject(name);
        btnObj.transform.SetParent(parent, false);

        var btnImg = btnObj.AddComponent<Image>();
        btnImg.color = new Color(0.2f, 0.2f, 0.2f, 0.9f);
        var btn = btnObj.AddComponent<Button>();
        var colors = btn.colors;
        colors.highlightedColor = new Color(0.4f, 0.4f, 0.4f, 1f);
        colors.pressedColor = new Color(0.5f, 0.5f, 0.5f, 1f);
        btn.colors = colors;

        var le = btnObj.AddComponent<LayoutElement>();
        le.minWidth = 140;
        le.preferredWidth = 160;

        var textObj = new GameObject("Text");
        textObj.transform.SetParent(btnObj.transform, false);
        var tmp = textObj.AddComponent<TextMeshProUGUI>();
        tmp.text = label;
        tmp.fontSize = 28;
        tmp.color = Color.white;
        tmp.alignment = TextAlignmentOptions.Center;
        if (font != null) tmp.font = font;

        var textRect = textObj.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;

        return btnObj;
    }
}
