using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using System.IO;
using System.Collections.Generic;

public class CreateNextStageSetup : EditorWindow
{
    [MenuItem("Tools/次のステージ（2面）を自動で準備する")]
    public static void AutoSetupNextStages()
    {
        // 1. dialogue_2.csv を作る
        string csvPath = "Assets/Resources/Dialogues/dialogue_2.csv";
        if (!File.Exists(csvPath))
        {
            File.WriteAllText(csvPath, "name,text,sprite,position\n#DAY#,二日目,,\n麺乃どん子,ここはステージ2だよ！,donko_normal,left\n", System.Text.Encoding.UTF8);
            AssetDatabase.Refresh();
        }

        // 2. DialogueScene_1 をコピーして DialogueScene_2 を作る
        string d1Path = "Assets/Scenes/DialogueScene_1.unity";
        string d2Path = "Assets/Scenes/DialogueScene_2.unity";
        if (!File.Exists(d2Path) && File.Exists(d1Path))
        {
            AssetDatabase.CopyAsset(d1Path, d2Path);
        }

        // 3. SampleScene をコピーして Stage2 を作る
        string s1Path = "Assets/Scenes/SampleScene.unity";
        string s2Path = "Assets/Scenes/Stage2.unity";
        if (!File.Exists(s2Path) && File.Exists(s1Path))
        {
            AssetDatabase.CopyAsset(s1Path, s2Path);
        }

        // 4. DialogueScene_2 を開いて設定を書き換える
        if (File.Exists(d2Path))
        {
            var scene = EditorSceneManager.OpenScene(d2Path, OpenSceneMode.Single);
            var manager = FindObjectOfType<DialogueController>();
            if (manager != null)
            {
                manager.csvFileName = "dialogue_2";
                if (manager.dayTextUI != null) manager.dayTextUI.text = "二日目";
                EditorUtility.SetDirty(manager);
            }
            EditorSceneManager.SaveScene(scene);
        }

        // 5. Build Settings に順番通りに並べる
        var scenes = new List<EditorBuildSettingsScene>();
        if (File.Exists(d1Path)) scenes.Add(new EditorBuildSettingsScene(d1Path, true));
        if (File.Exists(s1Path)) scenes.Add(new EditorBuildSettingsScene(s1Path, true));
        if (File.Exists(d2Path)) scenes.Add(new EditorBuildSettingsScene(d2Path, true));
        if (File.Exists(s2Path)) scenes.Add(new EditorBuildSettingsScene(s2Path, true));

        EditorBuildSettings.scenes = scenes.ToArray();

        // 6. DialogueScene_1 に戻す
        if (File.Exists(d1Path))
        {
            EditorSceneManager.OpenScene(d1Path, OpenSceneMode.Single);
        }

        EditorUtility.DisplayDialog("完了！", "2日目の会話シーンと、Stage2 の準備が完了しました！\n\nPlayボタンを押して、最後までクリアしてみてください。無限ループしなくなっています！", "OK");
    }
}
