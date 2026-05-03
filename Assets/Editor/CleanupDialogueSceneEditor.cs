using UnityEngine;
using UnityEditor;

public class CleanupDialogueSceneEditor : EditorWindow
{
    [MenuItem("Tools/古い会話システムを削除して掃除する")]
    public static void CleanupOldDialogueSystem()
    {
        // DialogueController をすべて探す
        var controllers = FindObjectsOfType<DialogueController>();
        
        if (controllers.Length <= 1)
        {
            EditorUtility.DisplayDialog("確認", "重複している会話システムは見つかりませんでした。", "OK");
            return;
        }

        int deletedCount = 0;

        foreach (var controller in controllers)
        {
            // DialogueUIController (スキップ・オート・ログ機能) が付いていない古いものを判定
            var uiController = controller.GetComponent<DialogueUIController>();
            if (uiController == null)
            {
                // 古いシステムの親Canvas等も含めて削除したいが、構造が不明な場合は
                // 紐づいているCanvasごと消すのが安全
                var canvas = controller.GetComponentInParent<Canvas>();
                if (canvas != null && canvas.gameObject.name != "DialogueCanvas")
                {
                    DestroyImmediate(canvas.gameObject);
                }
                else if (controller.characterImage != null)
                {
                    var parentCanvas = controller.characterImage.GetComponentInParent<Canvas>();
                    if (parentCanvas != null)
                    {
                        DestroyImmediate(parentCanvas.gameObject);
                    }
                }
                
                // DialogueManager自体を削除
                if (controller != null && controller.gameObject != null)
                {
                    DestroyImmediate(controller.gameObject);
                }
                
                deletedCount++;
            }
        }

        if (deletedCount > 0)
        {
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());
            Debug.Log($"✅ 古い会話システムを {deletedCount} 個削除しました。");
            EditorUtility.DisplayDialog("掃除完了", "UIがない古い会話システムを削除しました！\nCtrl+Sで保存してください。", "OK");
        }
        else
        {
            EditorUtility.DisplayDialog("確認", "スキップUIが付いていない古いシステムが見つかりませんでした。\nもし手動で消す場合は、Hierarchyから不要なCanvasを削除してください。", "OK");
        }
    }
}
