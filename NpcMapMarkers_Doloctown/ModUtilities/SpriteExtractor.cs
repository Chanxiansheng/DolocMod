using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;
using Newtonsoft.Json; // ✅ 引入 JSON.NET

public class SpriteExtractor
{
    public class SpriteInfo
    {
        public string name;
        public string textureName;
        public float x, y, width, height;
        public float pixelsPerUnit;
    }

    public static async Task ExtractAllSpritesAsync(string outputPath)
    {
        List<SpriteInfo> spriteList = new List<SpriteInfo>();

        foreach (var sprite in Resources.FindObjectsOfTypeAll<Sprite>())
        {
            var rect = sprite.rect;

            var info = new SpriteInfo
            {
                name = sprite.name,
                textureName = sprite.texture != null ? sprite.texture.name : "null",
                x = rect.x,
                y = rect.y,
                width = rect.width,
                height = rect.height,
                pixelsPerUnit = sprite.pixelsPerUnit
            };

            spriteList.Add(info);
        }

        try
        {
            string json = JsonConvert.SerializeObject(spriteList, Formatting.Indented); // ✅ 使用 JSON.NET 序列化
            File.WriteAllText(outputPath, json);
            Debug.Log($"[SpriteExtractor] Exported {spriteList.Count} sprites to {outputPath}");
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[SpriteExtractor] Failed to write JSON: {ex.Message}");
        }

        await Task.CompletedTask;
    }
}