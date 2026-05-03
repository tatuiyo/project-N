using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using System.IO;
using System.Collections.Generic;
using System.Linq;

public class AutoCreateNextStage : EditorWindow
{
    [MenuItem("Tools/次のステージ（N+1日目）を自動で追加する")]
    public static void CreateNext()
    {
        // 1. 現在の最大ステージ数(N)を調べる
        int maxN = 0;
        string[] sceneGuids = AssetDatabase.FindAssets("t:Scene", new[] { "Assets/Scenes" });
        foreach (string guid in sceneGuids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            string name = Path.GetFileNameWithoutExtension(path);
            if (name.StartsWith("DialogueScene_"))
            {
                string numStr = name.Replace("DialogueScene_", "");
                if (int.TryParse(numStr, out int num))
                {
                    if (num > maxN) maxN = num;
                }
            }
        }

        if (maxN == 0)
        {
            EditorUtility.DisplayDialog("エラー", "DialogueScene_1 が見つかりません！", "OK");
            return;
        }

        int nextN = maxN + 1;
        string[] dayStrs = { "非表示", "一日目", "二日目", "三日目", "四日目", "五日目", "六日目", "七日目", "八日目", "九日目", "十日目" };
        string dayStr = nextN < dayStrs.Length ? dayStrs[nextN] : $"{nextN}日目";

        // 2. CSVを作成
        string csvPath = $"Assets/Resources/Dialogues/dialogue_{nextN}.csv";
        if (!File.Exists(csvPath))
        {
            File.WriteAllText(csvPath, $"name,text,sprite,position\n#DAY#,{dayStr},,\n麺乃どん子,ここはステージ{nextN}だよ！,donko_normal,left\n", System.Text.Encoding.UTF8);
        }

        // 3. DialogueSceneをコピー
        string prevDPath = $"Assets/Scenes/DialogueScene_{maxN}.unity";
        string nextDPath = $"Assets/Scenes/DialogueScene_{nextN}.unity";
        if (File.Exists(prevDPath))
        {
            AssetDatabase.CopyAsset(prevDPath, nextDPath);
        }

        // 4. Stageをコピー（前のステージがStage[maxN]かSampleScene）
        string prevSPath = $"Assets/Scenes/Stage{maxN}.unity";
        if (!File.Exists(prevSPath) && maxN == 1) prevSPath = "Assets/Scenes/SampleScene.unity";
        
        string nextSPath = $"Assets/Scenes/Stage{nextN}.unity";
        if (File.Exists(prevSPath))
        {
            AssetDatabase.CopyAsset(prevSPath, nextSPath);
        }

        AssetDatabase.Refresh();

        // 5. 新しいDialogueSceneを開いて設定変更
        string originalScene = EditorSceneManager.GetActiveScene().path;
        if (File.Exists(nextDPath))
        {
            var scene = EditorSceneManager.OpenScene(nextDPath, OpenSceneMode.Single);
            var manager = FindObjectOfType<DialogueController>();
            if (manager != null)
            {
                manager.csvFileName = $"dialogue_{nextN}";
                if (manager.dayTextUI != null) manager.dayTextUI.text = dayStr;
                EditorUtility.SetDirty(manager);
            }
            EditorSceneManager.SaveScene(scene);
        }

        // 6. Build Settings の末尾に追加
        var scenes = new List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
        
        // 重複チェックして追加
        if (!scenes.Any(s => s.path == nextDPath) && File.Exists(nextDPath))
        {
            scenes.Add(new EditorBuildSettingsScene(nextDPath, true));
        }
        if (!scenes.Any(s => s.path == nextSPath) && File.Exists(nextSPath))
        {
            scenes.Add(new EditorBuildSettingsScene(nextSPath, true));
        }

        EditorBuildSettings.scenes = scenes.ToArray();

        // 元のシーンに戻る
        if (!string.IsNullOrEmpty(originalScene) && File.Exists(originalScene))
        {
            EditorSceneManager.OpenScene(originalScene, OpenSceneMode.Single);
        }

        EditorUtility.DisplayDialog("大成功！", $"{nextN}日目の会話シーンと Stage{nextN} が新しく追加されました！\nBuild Settingsにも自動で登録されています。", "OK");
    }
}
