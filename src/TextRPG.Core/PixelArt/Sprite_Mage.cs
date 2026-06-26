namespace TextRPG.Core.PixelArt;

using static SpriteHelpers;

//  MAGO 48×48
public static partial class PlayerSprites
{
    public static PixelSprite Mage => _m.Value;
    private static readonly Lazy<PixelSprite> _m = new(() =>
    {
        var s = PixelSprite.Create(48, 48);
        int CX = 24;
        Fill(s, 8, 22, 32, 22, RP);
        Circle(s, CX, 12, 7, Skin);
        Tri(s, CX, 0, 18, 6, 30, 6, HB);
        Tri(s, CX, 0, 19, 5, 29, 5, HD);
        Fill(s, 14, 6, 20, 2, HB);
        Fill(s, 15, 7, 18, 1, HD);
        Set(s, CX - 3, 10, EyeW); Set(s, CX + 3, 10, EyeW);
        Set(s, CX - 3, 11, EyeP); Set(s, CX + 3, 11, EyeP);
        FillM(s, CX, 14, 3, 3, Skin);
        FillM(s, CX, 16, 2, 2, SkinD);
        FillM(s, CX, 17, 1, 2, SkinD);
        Fill(s, 10, 19, 28, 6, RL);
        Fill(s, 12, 20, 24, 4, RP);
        Set(s, 14, 23, SY); Set(s, 34, 21, SY);
        Set(s, 18, 28, SY); Set(s, 30, 30, SY);
        Set(s, 12, 33, SY); Set(s, 36, 35, SY);
        Fill(s, 10, 31, 28, 2, RD);
        FillM(s, CX, 31, 2, 2, SY);
        Fill(s, 8, 33, 32, 9, RP);
        Fill(s, 9, 34, 30, 8, RD);
        Fill(s, 7, 42, 34, 2, RL);
        Fill(s, 4, 21, 5, 10, RL);
        Fill(s, 39, 21, 5, 10, RL);
        Fill(s, 5, 22, 4, 8, RP);
        Fill(s, 39, 22, 4, 8, RP);
        Circle(s, 7, 34, 3, Skin);
        Circle(s, 41, 34, 3, Skin);
        Fill(s, 42, 16, 3, 24, SW);
        Fill(s, 43, 17, 1, 22, SB);
        Circle(s, CX + 20, 14, 3, SY);
        Set(s, CX + 20, 13, SY); Set(s, CX + 20, 15, SY);
        Set(s, CX + 19, 14, SY); Set(s, CX + 21, 14, SY);
        Fill(s, 20, 16, 8, 3, SkinD);
        return s;
    });
}
