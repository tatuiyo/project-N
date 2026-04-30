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
        var charRect = charObj.GetComponent<RectTransform>();
        // 左側に配置
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
        // 画面下部に配置
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
        var dialRect = dialogueObj.GetComponent<RectTransform>();
        dialRect.anchorMin = new Vector2(0.05f, 0.1f);
        dialRect.anchorMax = new Vector2(0.95f, 0.9f);
        dialRect.offsetMin = Vector2.zero;
        dialRect.offsetMax = Vector2.zero;

        // 9. DialogueControllerのアタッチとセットアップ
        var managerObj = new GameObject("DialogueManager");
        var controller = managerObj.AddComponent<DialogueController>();
        controller.characterImage = charImage;
        controller.speakerNameText = nameText;
        controller.dialogueText = dialText;

        // キャラのImageをデフォルトでは非アクティブに（スクリプト側で立ち絵があるときだけActiveにするため）
        charObj.SetActive(false);

        Debug.Log("【完了】会話シーンのセットアップが完了しました。");
        EditorUtility.DisplayDialog("成功", "会話シーンのセットアップが完了しました。\nシーンを保存してPlayボタンを押してみてください。", "OK");
    }
}
