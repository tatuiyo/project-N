using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;

public class BuildSettingsSetup : EditorWindow
{
    [MenuItem("Tools/シーンをBuildに自動登録")]
    public static void AddScenesToBuild()
    {
        // Assets/Scenes フォルダ内の .unity ファイルを全部取得
        string[] guids = AssetDatabase.FindAssets("t:Scene", new[] { "Assets/Scenes" });

        var scenes = new List<EditorBuildSettingsScene>();

        // SampleScene を最初（0番）にしたいので先に追加
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            if (Path.GetFileNameWithoutExtension(path) == "SampleScene")
            {
                scenes.Add(new EditorBuildSettingsScene(path, true));
                Debug.Log($"追加: {path} [0番]");
                break;
            }
        }

        // 次に DialogueScene を追加
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            string name = Path.GetFileNameWithoutExtension(path);
            if (name != "SampleScene")
            {
                scenes.Add(new EditorBuildSettingsScene(path, true));
                Debug.Log($"追加: {path}");
            }
        }

        EditorBuildSettings.scenes = scenes.ToArray();

        string result = "Build Settingsに登録しました！\n\n";
        for (int i = 0; i < scenes.Count; i++)
        {
            result += $"{i}: {Path.GetFileNameWithoutExtension(scenes[i].path)}\n";
        }

        EditorUtility.DisplayDialog("完了", result, "OK");
    }
}
