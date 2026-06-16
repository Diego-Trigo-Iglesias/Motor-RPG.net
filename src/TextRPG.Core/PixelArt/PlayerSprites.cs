namespace TextRPG.Core.PixelArt;

/// Sprites 48×48 con shapes avanzadas y detalles finos.
public static class PlayerSprites
{
    private static readonly PixelColor
        Skin = new(230, 190, 150), SkinD = new(200, 165, 130), SkinS = new(245, 210, 175),
        EyeW = new(255, 255, 255), EyeP = new(40, 40, 50),
        // Guerrero
        HR = new(195, 40, 40), HDR = new(150, 28, 28), HG = new(215, 180, 45),
        AS = new(150, 165, 190), AM = new(120, 135, 160), AD = new(70, 80, 100),
        CP = new(175, 35, 35), CD = new(130, 20, 20),
        // Mago
        HB = new(42, 60, 195), HD = new(28, 42, 145), RP = new(130, 48, 170),
        RL = new(160, 58, 200), RD = new(80, 28, 110), SY = new(255, 218, 58),
        SW = new(178, 138, 78), SB = new(140, 100, 50),
        // Picaro
        HG2 = new(38, 152, 48), HD2 = new(22, 108, 32), CK = new(58, 66, 76),
        CKD = new(38, 44, 52), LT = new(138, 93, 48), LTD = new(98, 53, 28),
        SS = new(155, 145, 140);

    private static void Fill(PixelSprite s, int x, int y, int w, int h, PixelColor c)
    { for (int dy = 0; dy < h; dy++) for (int dx = 0; dx < w; dx++) { int px = x + dx, py = y + dy; if (px >= 0 && px < 48 && py >= 0 && py < 48) s.SetPixel(px, py, c); } }

    private static void Set(PixelSprite s, int x, int y, PixelColor c) { if (x >= 0 && x < 48 && y >= 0 && y < 48) s.SetPixel(x, y, c); }

    private static void FillM(PixelSprite s, int cx, int y, int w, int h, PixelColor c)
    { for (int dy = 0; dy < h; dy++) for (int dx = 0; dx < w; dx++) { Set(s, cx - dx, y + dy, c); Set(s, cx + dx, y + dy, c); } }

    private static void DrawLine(PixelSprite s, int x0, int y0, int x1, int y1, PixelColor c)
    {
        int dx = Math.Abs(x1 - x0), dy = Math.Abs(y1 - y0), sx = x0 < x1 ? 1 : -1, sy = y0 < y1 ? 1 : -1, err = dx - dy;
        while (true) { Set(s, x0, y0, c); if (x0 == x1 && y0 == y1) break; int e2 = 2 * err; if (e2 > -dy) { err -= dy; x0 += sx; } if (e2 < dx) { err += dx; y0 += sy; } }
    }

    private static void Circle(PixelSprite s, int cx, int cy, int r, PixelColor c)
    { for (int y = -r; y <= r; y++) for (int x = -r; x <= r; x++) if (x * x + y * y <= r * r) Set(s, cx + x, cy + y, c); }

    private static void Tri(PixelSprite s, int x0, int y0, int x1, int y1, int x2, int y2, PixelColor c)
    {
        int minX = Math.Max(0, Math.Min(x0, Math.Min(x1, x2))), maxX = Math.Min(47, Math.Max(x0, Math.Max(x1, x2)));
        int minY = Math.Max(0, Math.Min(y0, Math.Min(y1, y2))), maxY = Math.Min(47, Math.Max(y0, Math.Max(y1, y2)));
        for (int y = minY; y <= maxY; y++) for (int x = minX; x <= maxX; x++)
        {
            float d0 = (x1 - x0) * (y - y0) - (y1 - y0) * (x - x0);
            float d1 = (x2 - x1) * (y - y1) - (y2 - y1) * (x - x1);
            float d2 = (x0 - x2) * (y - y2) - (y0 - y2) * (x - x2);
            bool neg = (d0 < 0) || (d1 < 0) || (d2 < 0);
            bool pos = (d0 > 0) || (d1 > 0) || (d2 > 0);
            if (!(neg && pos)) Set(s, x, y, c);
        }
    }

    //  GUERRERO 48×48 
    public static PixelSprite Warrior => _w.Value;
    private static readonly Lazy<PixelSprite> _w = new(() =>
    {
        var s = PixelSprite.Create(48, 48);
        int CX = 24;
        // Capa
        Fill(s, 10, 6, 28, 34, CP);
        Fill(s, 12, 8, 24, 32, CD);
        // Cabeza
        Circle(s, CX, 10, 8, Skin);
        // Casco
        Fill(s, 14, 2, 20, 5, HR);
        FillM(s, CX, 2, 8, 2, HDR);
        Set(s, CX, 2, HG); Set(s, CX - 1, 2, HG); Set(s, CX + 1, 2, HG); // cresta
        Fill(s, 15, 6, 18, 2, HR);
        Fill(s, 16, 7, 16, 1, HDR);
        // Visera
        FillM(s, CX, 7, 3, 2, AD);
        // Ojos
        Set(s, CX - 4, 8, EyeW); Set(s, CX + 4, 8, EyeW);
        Set(s, CX - 4, 9, EyeP); Set(s, CX + 4, 9, EyeP);
        Set(s, CX - 4, 8, EyeW); Set(s, CX + 4, 8, EyeW); // brillo
        // Boca
        FillM(s, CX, 13, 2, 1, SkinD);
        // Barbilla
        Fill(s, 20, 14, 8, 1, Skin);
        // Cuello
        Fill(s, 20, 15, 8, 2, SkinD);
        // Hombros
        Fill(s, 4, 16, 40, 5, AS);
        Fill(s, 6, 17, 36, 4, AM);
        Fill(s, 3, 17, 3, 5, AS); Fill(s, 42, 17, 3, 5, AS); // hombreras extendidas
        // Peto
        Fill(s, 10, 21, 28, 8, AS);
        Fill(s, 12, 22, 24, 6, AM);
        // Emblema
        FillM(s, CX, 24, 4, 3, HG);
        FillM(s, CX, 25, 2, 1, HG);
        // Brazos
        Fill(s, 4, 21, 4, 10, AS);
        Fill(s, 40, 21, 4, 10, AS);
        Fill(s, 5, 22, 3, 8, AM);
        Fill(s, 40, 22, 3, 8, AM);
        // Manos
        Circle(s, 8, 33, 3, Skin);
        Circle(s, 40, 33, 3, Skin);
        // Cinturón
        Fill(s, 10, 29, 28, 3, AD);
        FillM(s, CX, 29, 3, 3, HG);
        // Faldón armor
        Fill(s, 11, 32, 26, 2, AM);
        Fill(s, 12, 33, 24, 1, AD);
        // Piernas
        Fill(s, 13, 34, 8, 8, AM);
        Fill(s, 27, 34, 8, 8, AM);
        Fill(s, 14, 35, 6, 7, AS);
        Fill(s, 28, 35, 6, 7, AS);
        // Rodilleras
        Fill(s, 12, 40, 9, 3, AD);
        Fill(s, 27, 40, 9, 3, AD);
        // Grebas
        Fill(s, 13, 43, 8, 4, AM);
        Fill(s, 27, 43, 8, 4, AM);
        // Botas
        Fill(s, 10, 44, 12, 4, AD);
        Fill(s, 26, 44, 12, 4, AD);
        // Espada
        Fill(s, 42, 20, 3, 3, AD);  // Empuñadura
        Set(s, 41, 19, HG); Set(s, 43, 19, HG);
        Fill(s, 43, 15, 2, 4, AS);   // Hoja
        Set(s, 44, 14, AS); Set(s, 44, 18, AS);
        Set(s, 42, 14, AS); Set(s, 42, 18, AS);
        return s;
    });

    //  MAGO 48×48 
    public static PixelSprite Mage => _m.Value;
    private static readonly Lazy<PixelSprite> _m = new(() =>
    {
        var s = PixelSprite.Create(48, 48);
        int CX = 24;
        // Cuerpo
        Fill(s, 8, 22, 32, 22, RP);
        // Cabeza
        Circle(s, CX, 12, 7, Skin);
        // Sombrero
        Tri(s, CX, 0, 18, 6, 30, 6, HB);
        Tri(s, CX, 0, 19, 5, 29, 5, HD);
        // Ala del sombrero
        Fill(s, 14, 6, 20, 2, HB);
        Fill(s, 15, 7, 18, 1, HD);
        // Ojos
        Set(s, CX - 3, 10, EyeW); Set(s, CX + 3, 10, EyeW);
        Set(s, CX - 3, 11, EyeP); Set(s, CX + 3, 11, EyeP);
        // Barba
        FillM(s, CX, 14, 3, 3, Skin);
        FillM(s, CX, 16, 2, 2, SkinD);
        FillM(s, CX, 17, 1, 2, SkinD);
        // Túnica superior
        Fill(s, 10, 19, 28, 6, RL);
        Fill(s, 12, 20, 24, 4, RP);
        // Estrellas
        Set(s, 14, 23, SY); Set(s, 34, 21, SY);
        Set(s, 18, 28, SY); Set(s, 30, 30, SY);
        Set(s, 12, 33, SY); Set(s, 36, 35, SY);
        // Cinturón
        Fill(s, 10, 31, 28, 2, RD);
        FillM(s, CX, 31, 2, 2, SY);
        // Túnica inferior
        Fill(s, 8, 33, 32, 9, RP);
        Fill(s, 9, 34, 30, 8, RD);
        // Borde túnica
        Fill(s, 7, 42, 34, 2, RL);
        // Brazos (mangas)
        Fill(s, 4, 21, 5, 10, RL);
        Fill(s, 39, 21, 5, 10, RL);
        Fill(s, 5, 22, 4, 8, RP);
        Fill(s, 39, 22, 4, 8, RP);
        // Manos
        Circle(s, 7, 34, 3, Skin);
        Circle(s, 41, 34, 3, Skin);
        // Bastón
        Fill(s, 42, 16, 3, 24, SW);
        Fill(s, 43, 17, 1, 22, SB);
        // Gema del bastón
        Circle(s, CX + 20, 14, 3, SY);
        Set(s, CX + 20, 13, SY); Set(s, CX + 20, 15, SY);
        Set(s, CX + 19, 14, SY); Set(s, CX + 21, 14, SY);
        // Cuello
        Fill(s, 20, 16, 8, 3, SkinD);
        return s;
    });

    //  PICARO 48×48 
    public static PixelSprite Rogue => _r.Value;
    private static readonly Lazy<PixelSprite> _r = new(() =>
    {
        var s = PixelSprite.Create(48, 48);
        int CX = 24;
        // Capa
        Fill(s, 8, 8, 32, 32, CK);
        Fill(s, 10, 10, 28, 30, CKD);
        // Cuerpo
        Fill(s, 10, 20, 28, 22, LT);
        // Cabeza
        Circle(s, CX, 12, 7, Skin);
        // Capucha
        Fill(s, 14, 4, 20, 5, HG2);
        FillM(s, CX, 4, 9, 3, HD2);
        Fill(s, 15, 8, 18, 3, HG2);
        Fill(s, 16, 10, 16, 2, HG2);
        // Ojos (asomando)
        Set(s, CX - 5, 8, EyeW); Set(s, CX + 5, 8, EyeW);
        Set(s, CX - 5, 9, EyeP); Set(s, CX + 5, 9, EyeP);
        // Boca
        FillM(s, CX, 15, 2, 1, SkinD);
        // Cuello
        Fill(s, 20, 17, 8, 3, SkinD);
        // Armadura cuero superior
        Fill(s, 12, 20, 24, 6, LT);
        Fill(s, 14, 21, 20, 4, LTD);
        // Cinturón
        Fill(s, 10, 30, 28, 2, LTD);
        FillM(s, CX, 30, 3, 2, HG2);
        // Armadura cuero inferior
        Fill(s, 11, 32, 26, 8, LT);
        Fill(s, 12, 33, 24, 7, LTD);
        // Brazos
        Fill(s, 6, 21, 4, 10, LT);
        Fill(s, 38, 21, 4, 10, LT);
        Fill(s, 5, 22, 3, 8, CKD);
        Fill(s, 40, 22, 3, 8, CKD);
        // Manos
        Circle(s, 9, 34, 3, Skin);
        Circle(s, 39, 34, 3, Skin);
        // Piernas
        Fill(s, 13, 40, 8, 6, LT);
        Fill(s, 27, 40, 8, 6, LT);
        Fill(s, 14, 41, 6, 5, LTD);
        Fill(s, 28, 41, 6, 5, LTD);
        // Botas
        Fill(s, 11, 44, 10, 4, LTD);
        Fill(s, 27, 44, 10, 4, LTD);
        Fill(s, 10, 45, 11, 3, CKD);
        Fill(s, 27, 45, 11, 3, CKD);
        // Daga
        Fill(s, 42, 22, 2, 3, LTD);   // Empuñadura
        Set(s, 41, 23, HG2); Set(s, 43, 23, HG2);
        Fill(s, 42, 17, 2, 5, SS);    // Hoja
        Set(s, 41, 18, SS); Set(s, 43, 18, SS);
        Set(s, 41, 16, SS); Set(s, 43, 16, SS);
        return s;
    });

    public static PixelSprite ForClass(Enums.CharacterClass cls) => cls switch
    {
        Enums.CharacterClass.Warrior => Warrior,
        Enums.CharacterClass.Mage => Mage,
        Enums.CharacterClass.Rogue => Rogue,
        _ => Warrior
    };
}
