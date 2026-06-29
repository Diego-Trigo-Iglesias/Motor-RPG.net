/// <summary>
/// Carga texturas desde PixelSprites. Necesita que Raylib esté inicializado
/// (InitWindow llamado) ANTES de llamar a LoadAll() - de lo contrario
/// LoadTextureFromImage lanza AccessViolationException.
/// El caller debe gestionar el ciclo de vida: LoadAll() al iniciar, UnloadAll() al cerrar.
/// </summary>

using Raylib_cs;
using TextRPG.Core.PixelArt;
using static Raylib_cs.Raylib;

namespace TextRPG.Drawing;

/// <summary>Carga texturas desde PixelSprites para renderizar con Raylib.
public static class SpriteLoader
{
    private static Color C(int r, int g, int b, int a = 255) => new((byte)r, (byte)g, (byte)b, (byte)a);

    public static Dictionary<string, Texture2D> LoadAll()
    {
        var dict = new Dictionary<string, Texture2D>();
        void L(string k, PixelSprite s) { if (s.Width > 0) dict[k] = Load(s); }
        L("warrior", PlayerSprites.Warrior); L("mage", PlayerSprites.Mage); L("rogue", PlayerSprites.Rogue);
        L("rat", EnemySprites.Rat); L("goblin", EnemySprites.Goblin); L("skeleton", EnemySprites.Skeleton);
        L("orc", EnemySprites.Orc); L("dragon", EnemySprites.Dragon);
        L("slash", EffectSprites.Slash); L("crit", EffectSprites.CriticalHit);
        L("victory", EffectSprites.Victory);
        // Location sprites (for main scene display)
        L("loc_village", LocationSprites.Village); L("loc_forest", LocationSprites.Forest);
        L("loc_cave", LocationSprites.Cave); L("loc_ruins", LocationSprites.Ruins);
        L("loc_delta", LocationSprites.Delta); L("loc_dungeon", LocationSprites.Dungeon);
        return dict;
    }

    private static Texture2D Load(PixelSprite s)
    {
        var img = GenImageColor(s.Width, s.Height, C(0, 0, 0, 0));
        for (int y = 0; y < s.Height; y++)
            for (int x = 0; x < s.Width; x++)
            {
                var px = s.GetPixel(x, y);
                if (!px.IsTransparent) ImageDrawPixel(ref img, x, y, C(px.R, px.G, px.B));
            }
        var tex = LoadTextureFromImage(img);
        UnloadImage(img);
        return tex;
    }

    public static void UnloadAll(Dictionary<string, Texture2D> dict)
    {
        foreach (var t in dict.Values) UnloadTexture(t);
        dict.Clear();
    }
}
