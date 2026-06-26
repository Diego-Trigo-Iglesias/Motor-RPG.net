namespace TextRPG.Core.PixelArt;

using static SpriteHelpers;

//  PICARO 48×48
public static partial class PlayerSprites
{
    public static PixelSprite Rogue => _r.Value;
    private static readonly Lazy<PixelSprite> _r = new(() =>
    {
        var s = PixelSprite.Create(48, 48);
        int CX = 24;
        Fill(s, 8, 8, 32, 32, CK);
        Fill(s, 10, 10, 28, 30, CKD);
        Fill(s, 10, 20, 28, 22, LT);
        Circle(s, CX, 12, 7, Skin);
        Fill(s, 14, 4, 20, 5, HG2);
        FillM(s, CX, 4, 9, 3, HD2);
        Fill(s, 15, 8, 18, 3, HG2);
        Fill(s, 16, 10, 16, 2, HG2);
        Set(s, CX - 5, 8, EyeW); Set(s, CX + 5, 8, EyeW);
        Set(s, CX - 5, 9, EyeP); Set(s, CX + 5, 9, EyeP);
        FillM(s, CX, 15, 2, 1, SkinD);
        Fill(s, 20, 17, 8, 3, SkinD);
        Fill(s, 12, 20, 24, 6, LT);
        Fill(s, 14, 21, 20, 4, LTD);
        Fill(s, 10, 30, 28, 2, LTD);
        FillM(s, CX, 30, 3, 2, HG2);
        Fill(s, 11, 32, 26, 8, LT);
        Fill(s, 12, 33, 24, 7, LTD);
        Fill(s, 6, 21, 4, 10, LT);
        Fill(s, 38, 21, 4, 10, LT);
        Fill(s, 5, 22, 3, 8, CKD);
        Fill(s, 40, 22, 3, 8, CKD);
        Circle(s, 9, 34, 3, Skin);
        Circle(s, 39, 34, 3, Skin);
        Fill(s, 13, 40, 8, 6, LT);
        Fill(s, 27, 40, 8, 6, LT);
        Fill(s, 14, 41, 6, 5, LTD);
        Fill(s, 28, 41, 6, 5, LTD);
        Fill(s, 11, 44, 10, 4, LTD);
        Fill(s, 27, 44, 10, 4, LTD);
        Fill(s, 10, 45, 11, 3, CKD);
        Fill(s, 27, 45, 11, 3, CKD);
        Fill(s, 42, 22, 2, 3, LTD);
        Set(s, 41, 23, HG2); Set(s, 43, 23, HG2);
        Fill(s, 42, 17, 2, 5, SS);
        Set(s, 41, 18, SS); Set(s, 43, 18, SS);
        Set(s, 41, 16, SS); Set(s, 43, 16, SS);
        return s;
    });
}
