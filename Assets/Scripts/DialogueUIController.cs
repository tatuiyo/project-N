using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// 会話UI制御：スキップ・オート・ログの各ボタンとログパネルを管理
/// </summary>
public class DialogueUIController : MonoBehaviour
{
    [Header("ボタン参照")]
    public Button skipButton;
    public Button autoButton;
    public Button logButton;

    [Header("ログパネル")]
    public GameObject logPanel;
    public Transform logContentParent;  // ScrollView > Content
    public GameObject logEntryPrefab;   // ログ1行のテンプレート

    [Header("ボタンテキスト")]
    public TextMeshProUGUI skipButtonText;
    public TextMeshProUGUI autoButtonText;

    private DialogueController controller;
    private bool logVisible = false;

    void Start()
    {
        controller = FindObjectOfType<DialogueController>();
        if (controller == null)
        {
            Debug.LogError("❌ DialogueController が見つかりません");
            return;
        }

        // ボタンのリスナー登録
        if (skipButton != null)
            skipButton.onClick.AddListener(OnSkipClicked);
        if (autoButton != null)
            autoButton.onClick.AddListener(OnAutoClicked);
        if (logButton != null)
            logButton.onClick.AddListener(OnLogClicked);

        // ログ更新イベントの購読
        controller.OnLogUpdated += RefreshLogDisplay;

        // ログパネルは最初非表示
        if (logPanel != null)
            logPanel.SetActive(false);

        UpdateButtonLabels();
    }

    void OnDestroy()
    {
        if (controller != null)
        {
            controller.OnLogUpdated -= RefreshLogDisplay;
        }
    }

    // ============================
    // ボタンハンドラー
    // ============================

    void OnSkipClicked()
    {
        if (controller == null) return;
        controller.SetSkipping(!controller.IsSkipping);
        UpdateButtonLabels();
    }

    void OnAutoClicked()
    {
        if (controller == null) return;
        controller.SetAuto(!controller.IsAuto);
        UpdateButtonLabels();
    }

    void OnLogClicked()
    {
        logVisible = !logVisible;
        if (logPanel != null)
        {
            logPanel.SetActive(logVisible);
            if (logVisible) RefreshLogDisplay();
        }
    }

    // ============================
    // ボタンラベル更新
    // ============================

    void UpdateButtonLabels()
    {
        if (skipButtonText != null)
        {
            skipButtonText.text = controller.IsSkipping ? "■ 停止" : "▶▶ スキップ";
        }
        if (autoButtonText != null)
        {
            autoButtonText.text = controller.IsAuto ? "■ 停止" : "▶ オート";
        }
    }

    void Update()
    {
        // ボタンラベルをリアルタイムに反映
        UpdateButtonLabels();
    }

    // ============================
    // ログ表示の更新
    // ============================

    void RefreshLogDisplay()
    {
        if (logContentParent == null || logEntryPrefab == null) return;
        if (!logVisible) return;

        // 既存のログエントリを即座に削除
        int childCount = logContentParent.childCount;
        for (int i = childCount - 1; i >= 0; i--)
        {
            DestroyImmediate(logContentParent.GetChild(i).gameObject);
        }

        // ログを生成
        foreach (var entry in controller.logHistory)
        {
            // 第3引数をfalseにすることで、UIのサイズや位置がおかしくなるのを防ぐ
            GameObject obj = Instantiate(logEntryPrefab, logContentParent, false);
            obj.SetActive(true);

            TextMeshProUGUI tmp = obj.GetComponentInChildren<TextMeshProUGUI>();
            if (tmp != null)
            {
                // 文字がはみ出ないように設定
                tmp.enableWordWrapping = true;
                tmp.text = $"<color=#FFD700>{entry.speakerName}</color>\n{entry.text}";
            }
        }
    }
}
