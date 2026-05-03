using UnityEngine;
using UnityEditor;

public class RenameEbifryerEditor : EditorWindow
{
    [MenuItem("Tools/Rename Ebifryer")]
    public static void RenameEbi()
    {
        string oldPath = "Assets/Resources/Sprites/1099055_0.png";
        string newName = "ebifryer_normal"; // 拡張子は省く

        // ファイル名を変更
        string error = AssetDatabase.RenameAsset(oldPath, newName);
        
        if (string.IsNullOrEmpty(error))
        {
            Debug.Log("✅ エビフライヤーの画像の名前を変更しました！");
            
            // Texture TypeをSpriteに変更
            TextureImporter importer = (TextureImporter)AssetImporter.GetAtPath("Assets/Resources/Sprites/ebifryer_normal.png");
            if (importer != null && importer.textureType != TextureImporterType.Sprite)
            {
                importer.textureType = TextureImporterType.Sprite;
                importer.spriteImportMode = SpriteImportMode.Single;
                importer.alphaIsTransparency = true;
                importer.SaveAndReimport();
                Debug.Log("✅ Texture Type を Sprite に自動設定しました！");
            }
            
            EditorUtility.DisplayDialog("成功", "画像の準備が完了しました！\nPlayして確認してください！", "OK");
        }
        else
        {
            Debug.LogError("リネーム失敗: " + error);
        }
    }
}
