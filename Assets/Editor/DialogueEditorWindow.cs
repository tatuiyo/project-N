using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;
using System.Linq;

/// <summary>
/// 会話エディタ：プルダウンでキャラ・表情・立ち位置を選び、テキストを入力するだけでCSVが完成するツール
/// メニュー: Tools > 会話エディタ
/// </summary>
public class DialogueEditorWindow : EditorWindow
{
    // ====== キャラクター定義 ======
    private static readonly CharacterDef[] characters = new CharacterDef[]
    {
        new CharacterDef("麺乃どん子", "donko", new string[] { "normal", "happy", "sad", "angry", "surprise" }),
        new CharacterDef("エビフライヤー", "ebifryer", new string[] { "normal" }),
        // ↓ 新キャラを追加するときはここに足すだけ！
        // new CharacterDef("新キャラ名", "newchara", new string[] { "normal", "smile" }),
    };

    // ====== 立ち位置定義 ======
    private static readonly string[] positionOptions = { "left", "center", "right" };
    private static readonly string[] positionLabels = { "← 左", "◉ 中央", "→ 右" };

    // ====== 演出タグ一覧 ======
    private static readonly string[] effectTags = { "[shake]", "[flash]", "[wait]", "[slow]", "[fast]" };
    private static readonly string[] effectLabels = { "💥 揺れ", "⚡ 光", "⏸ 待つ", "🐢 遅く", "🐇 速く" };

    // ====== 内部データ ======
    private List<DialogueEntry> entries = new List<DialogueEntry>();
    private Vector2 scrollPos;
    private string fileName = "dialogue_1";

    // ====== 日にち設定 ======
    private string[] dayOptions = { "非表示", "一日目", "二日目", "三日目", "四日目", "五日目", "六日目", "最終日" };
    private int selectedDayIndex = 0;

    [MenuItem("Tools/会話エディタ")]
    public static void ShowWindow()
    {
        var win = GetWindow<DialogueEditorWindow>("会話エディタ");
        win.minSize = new Vector2(550, 450);
    }

    void OnEnable()
    {
        TryLoadExistingCSV();
    }

    void OnGUI()
    {
        // ===== ヘッダー =====
        EditorGUILayout.Space(5);
        DrawHeader();
        EditorGUILayout.Space(5);

        // ===== ファイル名 =====
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("📄 ファイル名", GUILayout.Width(80));
        fileName = EditorGUILayout.TextField(fileName);
        if (GUILayout.Button("読み込み", GUILayout.Width(60)))
        {
            TryLoadExistingCSV();
        }
        EditorGUILayout.EndHorizontal();

        // ===== 日にち設定 =====
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("📅 日にち", GUILayout.Width(80));
        selectedDayIndex = EditorGUILayout.Popup(selectedDayIndex, dayOptions);
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space(10);// ===== 演出タグ凡例 =====
        EditorGUILayout.Space(3);
        DrawTagLegend();

        EditorGUILayout.Space(5);
        DrawSeparator();

        // ===== 会話行リスト =====
        scrollPos = EditorGUILayout.BeginScrollView(scrollPos);

        for (int i = 0; i < entries.Count; i++)
        {
            DrawEntryBox(i);
        }

        EditorGUILayout.EndScrollView();

        // ===== 下部ボタン =====
        EditorGUILayout.Space(5);
        DrawSeparator();
        DrawBottomButtons();
    }

    // ============================
    // 描画系メソッド
    // ============================

    void DrawHeader()
    {
        GUIStyle titleStyle = new GUIStyle(EditorStyles.boldLabel)
        {
            fontSize = 16,
            alignment = TextAnchor.MiddleCenter
        };
        EditorGUILayout.LabelField("🎭 会話エディタ", titleStyle);

        GUIStyle subStyle = new GUIStyle(EditorStyles.miniLabel)
        {
            alignment = TextAnchor.MiddleCenter
        };
        EditorGUILayout.LabelField("キャラ・表情・位置を選んでテキストを入力 → CSVを保存！", subStyle);
    }

    void DrawTagLegend()
    {
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("演出タグ:", EditorStyles.miniLabel, GUILayout.Width(55));
        GUIStyle tagStyle = new GUIStyle(EditorStyles.miniLabel) { richText = true };
        EditorGUILayout.LabelField(
            "<b>[shake]</b>=揺れ  <b>[flash]</b>=光  <b>[wait]</b>=待つ  <b>[slow]</b>=遅く  <b>[fast]</b>=速く",
            tagStyle);
        EditorGUILayout.EndHorizontal();
    }

    void DrawEntryBox(int index)
    {
        DialogueEntry entry = entries[index];

        // --- ボックス開始 ---
        EditorGUILayout.BeginVertical("box");

        // 行番号 + 操作ボタン
        EditorGUILayout.BeginHorizontal();

        GUIStyle numStyle = new GUIStyle(EditorStyles.boldLabel) { fontSize = 14 };
        EditorGUILayout.LabelField($"#{index + 1}", numStyle, GUILayout.Width(35));

        GUILayout.FlexibleSpace();

        // 上下移動ボタン
        EditorGUI.BeginDisabledGroup(index == 0);
        if (GUILayout.Button("▲", GUILayout.Width(25)))
        {
            SwapEntries(index, index - 1);
            return;
        }
        EditorGUI.EndDisabledGroup();

        EditorGUI.BeginDisabledGroup(index == entries.Count - 1);
        if (GUILayout.Button("▼", GUILayout.Width(25)))
        {
            SwapEntries(index, index + 1);
            return;
        }
        EditorGUI.EndDisabledGroup();

        // 複製ボタン
        if (GUILayout.Button("📋", GUILayout.Width(25)))
        {
            DuplicateEntry(index);
            return;
        }

        // 削除ボタン
        GUI.backgroundColor = new Color(1f, 0.5f, 0.5f);
        if (GUILayout.Button("✕", GUILayout.Width(25)))
        {
            entries.RemoveAt(index);
            return;
        }
        GUI.backgroundColor = Color.white;

        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space(2);

        // --- キャラ + 表情 + 位置 選択 ---
        EditorGUILayout.BeginHorizontal();

        EditorGUILayout.LabelField("キャラ", GUILayout.Width(35));
        string[] charNames = characters.Select(c => c.displayName).ToArray();
        entry.characterIndex = EditorGUILayout.Popup(entry.characterIndex, charNames);

        EditorGUILayout.LabelField("表情", GUILayout.Width(28));
        CharacterDef charDef = characters[Mathf.Clamp(entry.characterIndex, 0, characters.Length - 1)];
        string[] expressionLabels = charDef.expressions.Select(e => GetExpressionLabel(e)).ToArray();
        entry.expressionIndex = Mathf.Clamp(entry.expressionIndex, 0, charDef.expressions.Length - 1);
        entry.expressionIndex = EditorGUILayout.Popup(entry.expressionIndex, expressionLabels);

        EditorGUILayout.LabelField("位置", GUILayout.Width(28));
        entry.positionIndex = EditorGUILayout.Popup(entry.positionIndex, positionLabels, GUILayout.Width(70));

        EditorGUILayout.EndHorizontal();

        // --- スプライトプレビュー ---
        string spriteName = charDef.spritePrefix + "_" + charDef.expressions[entry.expressionIndex];
        string spritePath = $"Assets/Resources/Sprites/{spriteName}.png";
        bool spriteExists = File.Exists(spritePath);

        if (spriteExists)
        {
            EditorGUILayout.LabelField($"  ✅ {spriteName}  |  📍 {positionLabels[entry.positionIndex]}", EditorStyles.miniLabel);
        }
        else
        {
            EditorGUILayout.LabelField($"  ⚠ {spriteName} (画像なし)  |  📍 {positionLabels[entry.positionIndex]}", EditorStyles.miniLabel);
        }

        // --- テキスト入力 ---
        EditorGUILayout.LabelField("セリフ:");
        entry.text = EditorGUILayout.TextArea(entry.text, GUILayout.MinHeight(40));

        // --- 演出タグ挿入ボタン ---
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("演出タグ挿入:", EditorStyles.miniLabel, GUILayout.Width(80));
        for (int t = 0; t < effectTags.Length; t++)
        {
            if (GUILayout.Button(effectLabels[t], EditorStyles.miniButton, GUILayout.Width(55)))
            {
                entry.text += effectTags[t];
                GUI.FocusControl(null); // フォーカスをリフレッシュ
            }
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.EndVertical();
        EditorGUILayout.Space(3);
    }

    void DrawBottomButtons()
    {
        EditorGUILayout.BeginHorizontal();

        // 行追加ボタン
        GUI.backgroundColor = new Color(0.5f, 1f, 0.5f);
        if (GUILayout.Button("＋ 行を追加", GUILayout.Height(30)))
        {
            entries.Add(new DialogueEntry());
        }
        GUI.backgroundColor = Color.white;

        // 保存ボタン
        GUI.backgroundColor = new Color(0.5f, 0.7f, 1f);
        if (GUILayout.Button("💾 CSVを保存", GUILayout.Height(30)))
        {
            SaveCSV();
        }
        GUI.backgroundColor = Color.white;

        EditorGUILayout.EndHorizontal();

        // 情報表示
        EditorGUILayout.Space(3);
        EditorGUILayout.LabelField($"合計 {entries.Count} 行 | 保存先: Resources/Dialogues/{fileName}.csv", EditorStyles.miniLabel);
    }

    void DrawSeparator()
    {
        EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
    }

    // ============================
    // ロジック系メソッド
    // ============================

    void SaveCSV()
    {
        if (entries.Count == 0)
        {
            EditorUtility.DisplayDialog("エラー", "会話行が0件です。\n「＋行を追加」で追加してください。", "OK");
            return;
        }

        string dirPath = Path.Combine(Application.dataPath, "Resources", "Dialogues");
        if (!Directory.Exists(dirPath))
        {
            Directory.CreateDirectory(dirPath);
        }

        string filePath = Path.Combine(dirPath, fileName + ".csv");
        using (StreamWriter writer = new StreamWriter(filePath, false, System.Text.Encoding.UTF8))
        {
            // ヘッダー
            writer.WriteLine("name,text,sprite,position");

            // 日にち設定があれば、先頭に特殊な行として追加
            if (selectedDayIndex > 0)
            {
                writer.WriteLine($"#DAY#,{dayOptions[selectedDayIndex]},,");
            }

            // データ
            foreach (var entry in entries)
            {
                CharacterDef charDef = characters[Mathf.Clamp(entry.characterIndex, 0, characters.Length - 1)];
                string name = charDef.displayName;
                string spriteName = charDef.spritePrefix + "_" + charDef.expressions[entry.expressionIndex];
                string pos = positionOptions[Mathf.Clamp(entry.positionIndex, 0, positionOptions.Length - 1)];
                // CSVのカンマをエスケープ（テキスト中のカンマは全角に変換）
                string safeText = entry.text.Replace(",", "、").Replace("\n", " ");

                writer.WriteLine($"{name},{safeText},{spriteName},{pos}");
            }
        }
        AssetDatabase.Refresh();

        Debug.Log($"✅ CSV保存完了: {filePath} ({entries.Count} 行)");
        EditorUtility.DisplayDialog("保存完了", $"{fileName}.csv を保存しました！\n{entries.Count} 行の会話データ", "OK");
    }

    void TryLoadExistingCSV()
    {
        string filePath = Path.Combine(Application.dataPath, "Resources", "Dialogues", fileName + ".csv");
        if (!File.Exists(filePath)) return;

        entries.Clear();
        string[] lines = File.ReadAllLines(filePath, System.Text.Encoding.UTF8);

        bool isFirstLine = true;

        for (int i = 1; i < lines.Length; i++)
        {
            if (string.IsNullOrWhiteSpace(lines[i])) continue;

            string[] cols = lines[i].Split(',');
            if (cols.Length < 2) continue;

            string charName = cols[0];
            string text = cols[1];
            string spriteName = cols.Length > 2 ? cols[2] : "";
            string pos = cols.Length > 3 ? cols[3] : "left";

            // 日にち設定の特殊行ならプルダウンに反映してスキップ
            if (isFirstLine && charName == "#DAY#")
            {
                for (int d = 0; d < dayOptions.Length; d++)
                {
                    if (dayOptions[d] == text)
                    {
                        selectedDayIndex = d;
                        break;
                    }
                }
                isFirstLine = false;
                continue;
            }
            isFirstLine = false;

            // キャラクターの特定
            DialogueEntry entry = new DialogueEntry();
            entry.text = text;

            // 立ち位置の特定
            for (int p = 0; p < positionOptions.Length; p++)
            {
                if (positionOptions[p] == pos)
                {
                    entry.positionIndex = p;
                    break;
                }
            }

            // キャラクターを特定
            for (int c = 0; c < characters.Length; c++)
            {
                if (characters[c].displayName == name)
                {
                    entry.characterIndex = c;

                    // スプライト名から表情を特定
                    if (!string.IsNullOrEmpty(spriteName))
                    {
                        string prefix = characters[c].spritePrefix + "_";
                        if (spriteName.StartsWith(prefix))
                        {
                            string expr = spriteName.Substring(prefix.Length);
                            int exprIdx = System.Array.IndexOf(characters[c].expressions, expr);
                            if (exprIdx >= 0) entry.expressionIndex = exprIdx;
                        }
                    }
                    break;
                }
            }

            entries.Add(entry);
        }

        Debug.Log($"📂 CSV読み込み完了: {entries.Count} 行");
    }

    void SwapEntries(int a, int b)
    {
        var temp = entries[a];
        entries[a] = entries[b];
        entries[b] = temp;
    }

    void DuplicateEntry(int index)
    {
        var original = entries[index];
        var copy = new DialogueEntry
        {
            characterIndex = original.characterIndex,
            expressionIndex = original.expressionIndex,
            positionIndex = original.positionIndex,
            text = original.text
        };
        entries.Insert(index + 1, copy);
    }

    string GetExpressionLabel(string expr)
    {
        switch (expr)
        {
            case "normal":   return "😐 普通";
            case "happy":    return "😊 嬉しい";
            case "sad":      return "😢 悲しい";
            case "angry":    return "😠 怒り";
            case "surprise": return "😲 驚き";
            default:         return expr;
        }
    }

    // ============================
    // データクラス
    // ============================

    private class DialogueEntry
    {
        public int characterIndex = 0;
        public int expressionIndex = 0;
        public int positionIndex = 0; // 0=left, 1=center, 2=right
        public string text = "";
    }

    private class CharacterDef
    {
        public string displayName;
        public string spritePrefix;
        public string[] expressions;

        public CharacterDef(string name, string prefix, string[] exprs)
        {
            displayName = name;
            spritePrefix = prefix;
            expressions = exprs;
        }
    }
}
