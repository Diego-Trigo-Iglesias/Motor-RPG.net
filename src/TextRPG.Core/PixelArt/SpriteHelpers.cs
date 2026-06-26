namespace TextRPG.Core.PixelArt;

/// <summary>
/// Helpers de dibujo compartidos entre todos los generadores de sprites.
/// Operan sobre PixelSprite con bounds checking integrado (48×48).
/// </summary>
internal static class SpriteHelpers
{
    /// <summary>Rellena un rectángulo con bounds checking.</summary>
    public static void Fill(PixelSprite s, int x, int y, int w, int h, PixelColor c)
    {
        for (int dy = 0; dy < h; dy++)
            for (int dx = 0; dx < w; dx++)
            {
                int px = x + dx, py = y + dy;
                if (px >= 0 && px < 48 && py >= 0 && py < 48)
                    s.SetPixel(px, py, c);
            }
    }

    /// <summary>Fija un píxel con bounds checking.</summary>
    public static void Set(PixelSprite s, int x, int y, PixelColor c)
    {
        if (x >= 0 && x < 48 && y >= 0 && y < 48)
            s.SetPixel(x, y, c);
    }

    /// <summary>Rellena simétricamente desde el centro hacia los lados.</summary>
    public static void FillM(PixelSprite s, int cx, int y, int w, int h, PixelColor c)
    {
        for (int dy = 0; dy < h; dy++)
            for (int dx = 0; dx < w; dx++)
            {
                Set(s, cx - dx, y + dy, c);
                Set(s, cx + dx, y + dy, c);
            }
    }

    /// <summary>Dibuja una línea con algoritmo de Bresenham.</summary>
    public static void DrawLine(PixelSprite s, int x0, int y0, int x1, int y1, PixelColor c)
    {
        int dx = Math.Abs(x1 - x0), dy = Math.Abs(y1 - y0);
        int sx = x0 < x1 ? 1 : -1, sy = y0 < y1 ? 1 : -1, err = dx - dy;
        while (true)
        {
            Set(s, x0, y0, c);
            if (x0 == x1 && y0 == y1) break;
            int e2 = 2 * err;
            if (e2 > -dy) { err -= dy; x0 += sx; }
            if (e2 < dx) { err += dx; y0 += sy; }
        }
    }

    /// <summary>Dibuja un círculo relleno.</summary>
    public static void Circle(PixelSprite s, int cx, int cy, int r, PixelColor c)
    {
        for (int y = -r; y <= r; y++)
            for (int x = -r; x <= r; x++)
                if (x * x + y * y <= r * r)
                    Set(s, cx + x, cy + y, c);
    }

    /// <summary>Dibuja un triángulo relleno.</summary>
    public static void Tri(PixelSprite s, int x0, int y0, int x1, int y1, int x2, int y2, PixelColor c)
    {
        int minX = Math.Max(0, Math.Min(x0, Math.Min(x1, x2)));
        int maxX = Math.Min(47, Math.Max(x0, Math.Max(x1, x2)));
        int minY = Math.Max(0, Math.Min(y0, Math.Min(y1, y2)));
        int maxY = Math.Min(47, Math.Max(y0, Math.Max(y1, y2)));
        for (int y = minY; y <= maxY; y++)
            for (int x = minX; x <= maxX; x++)
            {
                float d0 = (x1 - x0) * (y - y0) - (y1 - y0) * (x - x0);
                float d1 = (x2 - x1) * (y - y1) - (y2 - y1) * (x - x1);
                float d2 = (x0 - x2) * (y - y2) - (y0 - y2) * (x - x2);
                bool neg = (d0 < 0) || (d1 < 0) || (d2 < 0);
                bool pos = (d0 > 0) || (d1 > 0) || (d2 > 0);
                if (!(neg && pos)) Set(s, x, y, c);
            }
    }

    /// <summary>Crea un PixelColor desde bytes.</summary>
    public static PixelColor C(byte r, byte g, byte b) => new(r, g, b);
}
