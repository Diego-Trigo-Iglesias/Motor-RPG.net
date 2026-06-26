using System.Runtime.CompilerServices;
using TextRPG.Core.PixelArt;

namespace TextRPG.Web.Rendering;

public class SpriteData
{
    public int Width { get; set; }
    public int Height { get; set; }
    public PixelData[] Pixels { get; set; } = [];

    // Cache por referencia de PixelSprite — asocia SpriteData a la instancia sin impedir GC
    private static readonly ConditionalWeakTable<PixelSprite, SpriteData> _cache = new();

    public static SpriteData FromPixelSprite(PixelSprite sprite)
    {
        if (_cache.TryGetValue(sprite, out var cached))
            return cached;

        var pixels = new PixelData[sprite.Width * sprite.Height];
        for (int y = 0; y < sprite.Height; y++)
            for (int x = 0; x < sprite.Width; x++)
            {
                pixels[y * sprite.Width + x] = sprite.IsTransparent(x, y)
                    ? new PixelData { A = 0 }
                    : new PixelData
                    {
                        R = sprite.GetPixel(x, y).R,
                        G = sprite.GetPixel(x, y).G,
                        B = sprite.GetPixel(x, y).B,
                        A = 255
                    };
            }
        var result = new SpriteData { Width = sprite.Width, Height = sprite.Height, Pixels = pixels };
        _cache.AddOrUpdate(sprite, result);
        return result;
    }

    /// <summary>Limpia la caché de sprites (útil al reiniciar).</summary>
    public static void ClearCache() { _cache.Clear(); }
}
