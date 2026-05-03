using UnityEngine;
using UnityEditor;
using TMPro;

public class FindTextEditor : EditorWindow
{
    private string searchWord = "こんにちは"; // 初期キーワード

    [MenuItem("Tools/見つからないテキストを探す")]
    public static void ShowWindow()
    {
        GetWindow<FindTextEditor>("テキスト検索");
    }

    void OnGUI()
    {
        EditorGUILayout.LabelField("🔎 シーン内のテキストコンポーネントを探します");
        
        searchWord = EditorGUILayout.TextField("探したい文字:", searchWord);

        if (GUILayout.Button("検索！"))
        {
            FindTextInScene();
        }
    }

    void FindTextInScene()
    {
        if (string.IsNullOrEmpty(searchWord)) return;

        // 全てのTextMeshProUGUIを探す
        var allTmpTexts = Resources.FindObjectsOfTypeAll<TextMeshProUGUI>();
        
        int count = 0;
        foreach (var tmp in allTmpTexts)
        {
            // プレハブなどのアセット本体は除外（シーン内のオブジェクトのみ対象）
            if (tmp.gameObject.scene.name == null) continue;

            if (tmp.text.Contains(searchWord))
            {
                Debug.Log($"🎯 見つけました！ オブジェクト名: <b>{tmp.gameObject.name}</b> (親: {tmp.transform.parent?.name})", tmp.gameObject);
                
                // Unityエディタ上でそのオブジェクトを選択状態にする
                Selection.activeGameObject = tmp.gameObject;
                EditorGUIUtility.PingObject(tmp.gameObject);
                
                count++;
            }
        }

        // 普通のText (旧UI) も一応探す
        var allNormalTexts = Resources.FindObjectsOfTypeAll<UnityEngine.UI.Text>();
        foreach (var txt in allNormalTexts)
        {
            if (txt.gameObject.scene.name == null) continue;

            if (txt.text.Contains(searchWord))
            {
                Debug.Log($"🎯 旧Textで見つけました！ オブジェクト名: <b>{txt.gameObject.name}</b> (親: {txt.transform.parent?.name})", txt.gameObject);
                Selection.activeGameObject = txt.gameObject;
                EditorGUIUtility.PingObject(txt.gameObject);
                count++;
            }
        }

        if (count == 0)
        {
            Debug.LogWarning($"「{searchWord}」を含むテキストは見つかりませんでした。");
        }
        else
        {
            EditorUtility.DisplayDialog("発見", $"「{searchWord}」を含むテキストを {count} 個見つけました！\nInspectorとHierarchyを見てください。", "OK");
        }
    }
}
