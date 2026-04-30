using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.InputSystem; // ← 新Input System

[System.Serializable]
public class DialogueLine
{
    public string speakerName;
    public string text;
    public string spriteName;
}

public class DialogueController : MonoBehaviour
{
    [Header("UI References")]
    public Image characterImage;
    public TextMeshProUGUI speakerNameText;
    public TextMeshProUGUI dialogueText;

    [Header("Settings")]
    public float typeSpeed = 0.04f;

    [Header("CSV File")]
    public string csvFileName = "dialogue_1";

    private List<DialogueLine> dialogueLines = new List<DialogueLine>();
    private int currentLineIndex = 0;
    private bool isTyping = false;
    private Coroutine typingCoroutine;

    void Start()
    {
        LoadCSV(csvFileName);
        ShowLine();
    }

    void Update()
    {
        // 新Input System対応のクリック検知
        bool clicked = Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame;

        if (clicked)
        {
            if (isTyping)
                CompleteText();
            else
                NextLine();
        }
    }

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
                spriteName  = cols.Length > 2 ? cols[2].Trim() : ""
            });
        }

        Debug.Log($"CSV読み込み完了: {dialogueLines.Count} 行");
    }

    void ShowLine()
    {
        if (currentLineIndex >= dialogueLines.Count)
        {
            SceneLoader.LoadNextScene();
            return;
        }

        DialogueLine line = dialogueLines[currentLineIndex];
        speakerNameText.text = line.speakerName;

        if (!string.IsNullOrEmpty(line.spriteName))
        {
            Sprite sp = Resources.Load<Sprite>("Sprites/" + line.spriteName);
            if (sp != null)
            {
                characterImage.sprite = sp;
                characterImage.preserveAspect = true; // ← 画像の縦横比を維持する設定
                characterImage.gameObject.SetActive(true);
                Debug.Log($"✅ 立ち絵ロード成功: {line.spriteName}");
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

        if (typingCoroutine != null) StopCoroutine(typingCoroutine);
        typingCoroutine = StartCoroutine(TypeLine(line.text));
    }

    IEnumerator TypeLine(string textToType)
    {
        isTyping = true;
        dialogueText.text = "";
        foreach (char c in textToType)
        {
            dialogueText.text += c;
            yield return new WaitForSeconds(typeSpeed);
        }
        isTyping = false;
    }

    void CompleteText()
    {
        if (typingCoroutine != null) StopCoroutine(typingCoroutine);
        dialogueText.text = dialogueLines[currentLineIndex].text;
        isTyping = false;
    }

    void NextLine()
    {
        currentLineIndex++;
        ShowLine();
    }
}
