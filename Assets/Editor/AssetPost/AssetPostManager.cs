using UnityEditor;

public class AssetPostManager : AssetPostprocessor
{
    
    /// <summary>
    /// 导入 图片资源自动变成  sprite 模式
    /// </summary>
    private void OnPreprocessTexture()
    {
        TextureImporter importer = (TextureImporter) assetImporter;

        importer.textureType = TextureImporterType.Sprite;
    }

}
