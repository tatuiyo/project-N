using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.InputSystem; // 新Input System

// ============================
// データクラス
// ============================

[System.Serializable]
public class DialogueLine
{
    public string speakerName;
    public string text;        // [shake] 等のタグを含む生テキスト
    public string spriteName;
    public string position;    // "left", "center", "right"
}

/// <summary>
/// ログ1行分のデータ（過去の会話履歴用）
/// </summary>
public class DialogueLogEntry
{
    public string speakerName;
    public string text; // タグを除去した表示用テキスト
}

// ============================
// メインコントローラー
// ============================

public class DialogueController : MonoBehaviour
{
    [Header("UI References")]
    public Image characterImage;
    public TextMeshProUGUI speakerNameText;
    public TextMeshProUGUI dialogueText;
    public TextMeshProUGUI dayTextUI; // ← 日にち表示用のUIを追加

    [Header("演出用")]
    public Image flashPanel;          // 白い画面フラッシュ用 Image
    public RectTransform canvasRect;  // 画面揺れ用の Canvas RectTransform

    [Header("Settings")]
    public float typeSpeed = 0.04f;
    public float autoWaitTime = 1.5f;

    [Header("CSV File")]
    public string csvFileName = "dialogue_1";

    // --- 内部状態 ---
    private List<DialogueLine> dialogueLines = new List<DialogueLine>();
    private int currentLineIndex = 0;
    private bool isTyping = false;
    private Coroutine typingCoroutine;
    private float originalTypeSpeed;

    // --- スキップ/オート ---
    private bool isSkipping = false;
    private bool isAuto = false;

    // --- ログ ---
    public List<DialogueLogEntry> logHistory = new List<DialogueLogEntry>();

    // --- イベント（UIコントローラーとの連携用） ---
    public event System.Action OnLineFinished;    // 1行の表示完了時
    public event System.Action OnDialogueEnd;     // 全行完了時
    public event System.Action OnLogUpdated;       // ログ更新時

    // ============================
    // ライフサイクル
    // ============================

    void Start()
    {
        originalTypeSpeed = typeSpeed;

        if (flashPanel != null)
        {
            flashPanel.gameObject.SetActive(false);
        }

        // --- クリック判定を黒い四角（TextBoxBg）のみに限定する ---
        if (dialogueText != null && dialogueText.transform.parent != null)
        {
            GameObject textBoxBg = dialogueText.transform.parent.gameObject;
            Button bgButton = textBoxBg.GetComponent<Button>();
            if (bgButton == null)
            {
                bgButton = textBoxBg.AddComponent<Button>();
                // 見た目は変えないようにする
                var colors = bgButton.colors;
                colors.normalColor = Color.white;
                colors.highlightedColor = Color.white;
                colors.pressedColor = Color.white;
                colors.selectedColor = Color.white;
                bgButton.colors = colors;
            }
            bgButton.onClick.AddListener(OnTextBoxClicked);
        }

        LoadCSV(csvFileName);
        ShowLine();
    }

    void Update()
    {
        // Escキーでスキップ開始/停止のみUpdateで監視
        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            SetSkipping(!isSkipping);
        }
    }

    // --- テキストボックスがクリックされたときの処理 ---
    public void OnTextBoxClicked()
    {
        if (isSkipping) return; // スキップ中はクリック無視

        if (isTyping)
            CompleteText();
        else
            NextLine();
    }

    // ============================
    // CSV読み込み（position列対応）
    // ============================

    void LoadCSV(string fileName)
    {
        dialogueLines.Clear();

        TextAsset csvFile = Resources.Load<TextAsset>("Dialogues/" + fileName);
        if (csvFile == null)
        {
            Debug.LogError($"CSVが見つかりません: Resources/Dialogues/{fileName}");
            return;
        }

        string[] lines = csvFile.text.Split('\n');
        for (int i = 1; i < lines.Length; i++)
        {
            string line = lines[i].Trim();
            if (string.IsNullOrEmpty(line)) continue;

            string[] cols = line.Split(',');
            if (cols.Length < 2) continue;

            dialogueLines.Add(new DialogueLine
            {
                speakerName = cols[0].Trim(),
                text        = cols[1].Trim(),
                spriteName  = cols.Length > 2 ? cols[2].Trim() : "",
                position    = cols.Length > 3 ? cols[3].Trim() : "left"
            });
        }

        Debug.Log($"CSV読み込み完了: {dialogueLines.Count} 行");
    }

    // ============================
    // 行の表示
    // ============================

    void ShowLine()
    {
        if (currentLineIndex >= dialogueLines.Count)
        {
            OnDialogueEnd?.Invoke();
            SceneLoader.LoadNextScene();
            return;
        }

        DialogueLine line = dialogueLines[currentLineIndex];

        // --- 特殊行（日にち設定）の処理 ---
        if (line.speakerName == "#DAY#")
        {
            if (dayTextUI != null)
            {
                dayTextUI.text = line.text;
                dayTextUI.gameObject.SetActive(line.text != "非表示");
            }
            NextLine(); // すぐに次のセリフへ進む
            return;
        }

        speakerNameText.text = line.speakerName;

        // --- 立ち位置の設定 ---
        ApplyPosition(line.position);

        // --- 立ち絵の設定 ---
        if (!string.IsNullOrEmpty(line.spriteName))
        {
            string spritePath = "Sprites/" + line.spriteName.Trim();
            Sprite sp = Resources.Load<Sprite>(spritePath);
            if (sp != null)
            {
                characterImage.sprite = sp;
                characterImage.preserveAspect = true;
                characterImage.gameObject.SetActive(true);
            }
            else
            {
                Debug.LogError($"❌ 立ち絵が見つかりません: Resources/Sprites/{line.spriteName}");
                characterImage.gameObject.SetActive(false);
            }
        }
        else
        {
            characterImage.gameObject.SetActive(false);
        }

        // --- ログに追加 ---
        string cleanText = StripTags(line.text);
        logHistory.Add(new DialogueLogEntry
        {
            speakerName = line.speakerName,
            text = cleanText
        });
        OnLogUpdated?.Invoke();

        // --- テキスト表示開始 ---
        typeSpeed = originalTypeSpeed; // 速度リセット
        if (typingCoroutine != null) StopCoroutine(typingCoroutine);
        typingCoroutine = StartCoroutine(TypeLineWithEffects(line.text));
    }

    // ============================
    // 立ち位置の適用
    // ============================

    void ApplyPosition(string pos)
    {
        if (characterImage == null) return;

        RectTransform rect = characterImage.GetComponent<RectTransform>();
        if (rect == null) return;

        switch (pos?.ToLower())
        {
            case "right":
                rect.anchorMin = new Vector2(0.55f, 0.1f);
                rect.anchorMax = new Vector2(0.85f, 0.8f);
                break;
            case "center":
                rect.anchorMin = new Vector2(0.30f, 0.1f);
                rect.anchorMax = new Vector2(0.70f, 0.8f);
                break;
            case "left":
            default:
                rect.anchorMin = new Vector2(0.15f, 0.1f);
                rect.anchorMax = new Vector2(0.45f, 0.8f);
                break;
        }
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
    }

    // ============================
    // タグ付きテキスト表示（コア機能）
    // ============================

    IEnumerator TypeLineWithEffects(string rawText)
    {
        isTyping = true;
        dialogueText.text = "";

        int i = 0;
        while (i < rawText.Length)
        {
            // タグ検出: [xxx] のパターン
            if (rawText[i] == '[')
            {
                int closeIdx = rawText.IndexOf(']', i);
                if (closeIdx > i)
                {
                    string tag = rawText.Substring(i + 1, closeIdx - i - 1).ToLower().Trim();
                    yield return StartCoroutine(ExecuteTag(tag));
                    i = closeIdx + 1;
                    continue;
                }
            }

            // スキップ中は即座に全文表示
            if (isSkipping)
            {
                dialogueText.text = StripTags(rawText);
                break;
            }

            // 通常の文字送り
            dialogueText.text += rawText[i];
            i++;
            yield return new WaitForSeconds(typeSpeed);
        }

        isTyping = false;
        OnLineFinished?.Invoke();

        // オートモードなら自動で次へ
        if (isAuto && !isSkipping)
        {
            yield return new WaitForSeconds(autoWaitTime);
            NextLine();
        }

        // スキップモードなら即座に次へ
        if (isSkipping)
        {
            yield return null; // 1フレーム待つ
            NextLine();
        }
    }

    // ============================
    // タグの実行
    // ============================

    IEnumerator ExecuteTag(string tag)
    {
        switch (tag)
        {
            case "shake":
                yield return StartCoroutine(DoShake());
                break;

            case "flash":
                yield return StartCoroutine(DoFlash());
                break;

            case "wait":
                if (!isSkipping)
                    yield return new WaitForSeconds(1f);
                break;

            case "slow":
                typeSpeed = originalTypeSpeed * 3f;
                break;

            case "fast":
                typeSpeed = originalTypeSpeed;
                break;

            default:
                Debug.LogWarning($"⚠ 不明な演出タグ: [{tag}]");
                break;
        }
    }

    // ============================
    // 演出エフェクト
    // ============================

    /// <summary>画面揺れ (0.3秒)</summary>
    IEnumerator DoShake()
    {
        if (isSkipping || canvasRect == null) yield break;

        Vector2 originalPos = canvasRect.anchoredPosition;
        float duration = 0.3f;
        float magnitude = 10f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            float x = Random.Range(-1f, 1f) * magnitude;
            float y = Random.Range(-1f, 1f) * magnitude;
            canvasRect.anchoredPosition = originalPos + new Vector2(x, y);
            elapsed += Time.deltaTime;
            yield return null;
        }

        canvasRect.anchoredPosition = originalPos;
    }

    /// <summary>画面フラッシュ (0.3秒)</summary>
    IEnumerator DoFlash()
    {
        if (isSkipping || flashPanel == null) yield break;

        flashPanel.gameObject.SetActive(true);
        Color c = flashPanel.color;
        c.a = 1f;
        flashPanel.color = c;

        float duration = 0.3f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            c.a = Mathf.Lerp(1f, 0f, elapsed / duration);
            flashPanel.color = c;
            yield return null;
        }

        flashPanel.gameObject.SetActive(false);
    }

    // ============================
    // テキストからタグを除去
    // ============================

    string StripTags(string text)
    {
        return Regex.Replace(text, @"\[.*?\]", "");
    }

    // ============================
    // テキスト即時完了（クリック時）
    // ============================

    void CompleteText()
    {
        if (typingCoroutine != null) StopCoroutine(typingCoroutine);
        dialogueText.text = StripTags(dialogueLines[currentLineIndex].text);
        isTyping = false;
        typeSpeed = originalTypeSpeed;
        OnLineFinished?.Invoke();
    }

    // ============================
    // 次の行へ
    // ============================

    void NextLine()
    {
        currentLineIndex++;
        ShowLine();
    }

    // ============================
    // 公開メソッド（UIコントローラーから呼ばれる）
    // ============================

    public void SetSkipping(bool skip)
    {
        isSkipping = skip;
        if (skip)
        {
            isAuto = false; // スキップ時はオート解除
        }
    }

    public void SetAuto(bool auto)
    {
        isAuto = auto;
        if (auto)
        {
            isSkipping = false; // オート時はスキップ解除
            // 現在タイピング中でなければ即座に次へ
            if (!isTyping)
            {
                NextLine();
            }
        }
    }

    public bool IsSkipping => isSkipping;
    public bool IsAuto => isAuto;
    public bool IsTyping => isTyping;
}
