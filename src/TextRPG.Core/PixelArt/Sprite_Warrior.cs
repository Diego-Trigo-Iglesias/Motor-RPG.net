namespace TextRPG.Core.PixelArt;

using static SpriteHelpers;

//  GUERRERO 48×48
public static partial class PlayerSprites
{
    public static PixelSprite Warrior => _w.Value;
    private static readonly Lazy<PixelSprite> _w = new(() =>
    {
        var s = PixelSprite.Create(48, 48);
        int CX = 24;
        Fill(s, 10, 6, 28, 34, CP);
        Fill(s, 12, 8, 24, 32, CD);
        Circle(s, CX, 10, 8, Skin);
        Fill(s, 14, 2, 20, 5, HR);
        FillM(s, CX, 2, 8, 2, HDR);
        Set(s, CX, 2, HG); Set(s, CX - 1, 2, HG); Set(s, CX + 1, 2, HG);
        Fill(s, 15, 6, 18, 2, HR);
        Fill(s, 16, 7, 16, 1, HDR);
        FillM(s, CX, 7, 3, 2, AD);
        Set(s, CX - 4, 8, EyeW); Set(s, CX + 4, 8, EyeW);
        Set(s, CX - 4, 9, EyeP); Set(s, CX + 4, 9, EyeP);
        FillM(s, CX, 13, 2, 1, SkinD);
        Fill(s, 20, 14, 8, 1, Skin);
        Fill(s, 20, 15, 8, 2, SkinD);
        Fill(s, 4, 16, 40, 5, AS);
        Fill(s, 6, 17, 36, 4, AM);
        Fill(s, 3, 17, 3, 5, AS); Fill(s, 42, 17, 3, 5, AS);
        Fill(s, 10, 21, 28, 8, AS);
        Fill(s, 12, 22, 24, 6, AM);
        FillM(s, CX, 24, 4, 3, HG);
        FillM(s, CX, 25, 2, 1, HG);
        Fill(s, 4, 21, 4, 10, AS);
        Fill(s, 40, 21, 4, 10, AS);
        Fill(s, 5, 22, 3, 8, AM);
        Fill(s, 40, 22, 3, 8, AM);
        Circle(s, 8, 33, 3, Skin);
        Circle(s, 40, 33, 3, Skin);
        Fill(s, 10, 29, 28, 3, AD);
        FillM(s, CX, 29, 3, 3, HG);
        Fill(s, 11, 32, 26, 2, AM);
        Fill(s, 12, 33, 24, 1, AD);
        Fill(s, 13, 34, 8, 8, AM);
        Fill(s, 27, 34, 8, 8, AM);
        Fill(s, 14, 35, 6, 7, AS);
        Fill(s, 28, 35, 6, 7, AS);
        Fill(s, 12, 40, 9, 3, AD);
        Fill(s, 27, 40, 9, 3, AD);
        Fill(s, 13, 43, 8, 4, AM);
        Fill(s, 27, 43, 8, 4, AM);
        Fill(s, 10, 44, 12, 4, AD);
        Fill(s, 26, 44, 12, 4, AD);
        Fill(s, 42, 20, 3, 3, AD);
        Set(s, 41, 19, HG); Set(s, 43, 19, HG);
        Fill(s, 43, 15, 2, 4, AS);
        Set(s, 44, 14, AS); Set(s, 44, 18, AS);
        Set(s, 42, 14, AS); Set(s, 42, 18, AS);
        return s;
    });
}
