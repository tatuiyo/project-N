using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;

public class FixEventSystem : EditorWindow
{
    [MenuItem("Tools/EventSystemを自動修正 (Input System対応)")]
    static void FixAllScenes()
    {
        // 現在開いているシーンのEventSystemを修正
        FixCurrentScene();
        Debug.Log("✅ EventSystem の修正が完了しました！");
    }

    static void FixCurrentScene()
    {
        var allEventSystems = FindObjectsByType<EventSystem>(FindObjectsSortMode.None);
        
        if (allEventSystems.Length == 0)
        {
            Debug.Log("EventSystemが見つかりませんでした。");
            return;
        }

        foreach (var es in allEventSystems)
        {
            // StandaloneInputModuleを探して削除
            var standalone = es.GetComponent<StandaloneInputModule>();
            if (standalone != null)
            {
                DestroyImmediate(standalone);
                Debug.Log($"✅ StandaloneInputModule を削除: {es.gameObject.scene.name}");
            }

            // InputSystemUIInputModuleがなければ追加
            var inputSystemModule = es.GetComponent<InputSystemUIInputModule>();
            if (inputSystemModule == null)
            {
                es.gameObject.AddComponent<InputSystemUIInputModule>();
                Debug.Log($"✅ InputSystemUIInputModule を追加: {es.gameObject.scene.name}");
            }
            else
            {
                Debug.Log($"InputSystemUIInputModule はすでに存在: {es.gameObject.scene.name}");
            }
        }

        // シーンを保存
        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        EditorSceneManager.SaveOpenScenes();
    }
}
