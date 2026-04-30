using UnityEngine;
using UnityEditor;

public class FixSpriteImport
{
    [MenuItem("Tools/どん子の立ち絵を修正する")]
    public static void FixDonkoSprite()
    {
        string path = "Assets/Resources/Sprites/donko_normal.png";
        
        TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
        if (importer == null)
        {
            Debug.LogError($"❌ ファイルが見つかりません: {path}");
            return;
        }
        
        Debug.Log($"現在の設定 - TextureType: {importer.textureType}, SpriteMode: {importer.spriteImportMode}");
        
        importer.textureType = TextureImporterType.Sprite;
        importer.spriteImportMode = SpriteImportMode.Single;
        importer.mipmapEnabled = false;
        importer.alphaIsTransparency = true;
        
        AssetDatabase.WriteImportSettingsIfDirty(path);
        AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
        AssetDatabase.Refresh();
        
        Debug.Log($"✅ 修正完了！TextureType: {importer.textureType}, SpriteMode: {importer.spriteImportMode}");
        EditorUtility.DisplayDialog("完了", "donko_normal の Sprite設定を修正しました！", "OK");
    }
}
