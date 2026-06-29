/// <summary>
/// Fondos multi-capa con siluetas, partículas animadas y atmósfera profunda.
/// Cada fondo combina:
///   - Gradientes atmosféricos (2-3 capas)
///   - Siluetas de elementos del entorno (casas, árboles, estalactitas, columnas)
///   - Partículas ambientales animadas (luciérnagas, niebla, cristales, llamas, chispas)
///   - Efectos de luz (rayos de luna, resplandor de lava, destellos de cristal)
///   - Viñeta oscura y tinte rojo en combate
/// El método Draw() es el punto de entrada único.
/// </summary>

using Raylib_cs;
using static Raylib_cs.Raylib;

namespace TextRPG.Drawing;

/// <summary>FONDOS ÉPICOS con capas dramáticas, luces, partículas, siluetas y atmósfera.</summary>
public static class Backgrounds
{
    private static Color C(int r, int g, int b, int a = 255) => new((byte)r, (byte)g, (byte)b, (byte)a);

    public static void Draw(int sw, int sh, string locId, bool combat = false)
    {
        // Fondo lejano (cielo / atmósfera)
        switch (locId)
        {
            case "village": Village(sw, sh); break;
            case "forest":  Forest(sw, sh);  break;
            case "cave":    Cave(sw, sh);    break;
            case "ruins":   Ruins(sw, sh);   break;
            case "delta":   Delta(sw, sh);   break;
            case "dungeon": Dungeon(sw, sh); break;
            default:        DefaultBg(sw, sh); break;
        }
        //  Capa de profundidad media (siluetas existentes) 
        // (dibujado dentro de cada función)

        //  Primer plano (foreground) 
        // Vegetación / suelo en primer plano para efecto 3D de profundidad
        DrawForeground(sw, sh, locId);

        // Viñeta oscura en bordes (simula profundidad de campo)
        DrawGradientV(0, 0, sw, 100, C(0, 0, 0, 150), C(0, 0, 0, 0));
        DrawGradientV(0, sh - 100, sw, 100, C(0, 0, 0, 0), C(0, 0, 0, 150));
        // Laterales oscuros (efecto túnel / viñeta)
        DrawGradientH(0, 0, 80, sh, C(0, 0, 0, 100), C(0, 0, 0, 0));
        DrawGradientH(sw - 80, 0, 80, sh, C(0, 0, 0, 0), C(0, 0, 0, 100));

        if (combat) DrawRectangle(0, 0, sw, sh, C(200, 30, 30, 45));
    }

    /// <summary>Dibuja elementos de primer plano (foreground) para efecto de profundidad 3D.</summary>
    private static void DrawForeground(int sw, int sh, string locId)
    {
        float t = (float)GetTime();
        // Base común: suelo oscuro en la parte inferior
        DrawGradientV(0, sh - 30, sw, 30, C(0, 0, 0, 100), C(0, 0, 0, 180));

        // Elementos específicos por localización
        switch (locId)
        {
            case "village":
                // Césped alto en primer plano
                for (int i = 0; i < 20; i++)
                {
                    int gx = (i * 53 + 17) % sw;
                    int gh = 8 + (i % 4) * 3;
                    DrawRectangle(gx, sh - gh, 2, gh, C(20, 40, 15, 160));
                    DrawRectangle(gx + 3, sh - gh + 2, 1, gh - 2, C(30, 55, 20, 120));
                }
                break;

            case "forest":
                // Helechos y arbustos densos
                for (int i = 0; i < 25; i++)
                {
                    int fx = (i * 37 + 11) % sw;
                    int fh = 12 + (i % 5) * 4;
                    int alpha = 120 + (i % 3) * 20;
                    DrawCircle(fx, sh - fh + 4, fh / 2, C(8, 30, 12, alpha));
                    DrawCircle(fx + 5, sh - fh + 6, fh / 3, C(12, 40, 16, alpha - 30));
                }
                break;

            case "cave":
                // Rocas en primer plano
                for (int i = 0; i < 8; i++)
                {
                    int rx = (i * 97 + 31) % sw;
                    int rs = 12 + (i % 3) * 8;
                    DrawCircle(rx, sh + 5, rs, C(15, 12, 10, 180));
                    DrawCircle(rx + 3, sh - 2, rs - 4, C(25, 20, 18, 140));
                }
                break;

            case "ruins":
                // Fragmentos de piedra y hierba
                for (int i = 0; i < 12; i++)
                {
                    int rx = (i * 67 + 23) % sw;
                    DrawRectangle(rx, sh - 10, 6 + (i % 3) * 4, 10, C(30, 25, 20, 160));
                    DrawRectangle(rx + 2, sh - 8, 3 + (i % 2) * 2, 6, C(45, 35, 28, 120));
                }
                break;

            case "delta":
                // Raíces y fango
                for (int i = 0; i < 15; i++)
                {
                    int dx = (i * 43 + 7) % sw;
                    int dh = 6 + (i % 3) * 5;
                    DrawRectangle(dx, sh - dh, 3, dh, C(15, 25, 18, 180));
                    DrawCircle(dx + 1, sh - dh - 2, 3, C(25, 40, 30, 140));
                }
                break;

            case "dungeon":
                // Huesos y escombros
                for (int i = 0; i < 10; i++)
                {
                    int dx = (i * 73 + 13) % sw;
                    DrawRectangle(dx, sh - 6, 8, 6, C(40, 35, 30, 180));
                    DrawRectangle(dx + 2, sh - 8, 4, 4, C(55, 45, 40, 140));
                }
                break;
        }
    }

    // ═══════════════════════════════════════════════════════════════════════════════
    //  SILUETAS — primitivas de dibujo para elementos del entorno
    // ═══════════════════════════════════════════════════════════════════════════════

    /// Casa con tejado a dos aguas y ventana.
    private static void DrawHouse(int x, int y, int w, int h, Color wall, Color roof, Color? window = null)
    {
        // Tejado (triángulo)
        DrawTriangle(
            (x + w / 2, y - h / 3),
            (x - w / 4, y + h / 6),
            (x + w + w / 4, y + h / 6),
            roof);
        // Paredes
        DrawRectangle(x, y, w, h, wall);
        // Ventana (si aplica)
        if (window.HasValue)
        {
            DrawRectangle(x + w / 2 - 3, y + h / 2 - 3, 6, 6, window.Value);
            DrawRectangle(x + w / 2 - 2, y + h / 2 - 5, 4, 2, window.Value with { A = 180 });
        }
    }

    /// Árbol con copa redondeada y tronco.
    private static void DrawTree(int x, int y, int h, Color trunk, Color foliage, Color foliage2)
    {
        // Tronco
        DrawRectangle(x - 2, y, 4, h, trunk);
        // Copa (círculos superpuestos)
        int cy = y - h / 3;
        DrawCircle(x, cy, h / 3, foliage);
        DrawCircle(x - h / 4, cy + h / 6, h / 4, foliage2);
        DrawCircle(x + h / 4, cy + h / 6, h / 4, foliage2);
        DrawCircle(x - h / 6, cy - h / 6, h / 5, foliage);
        DrawCircle(x + h / 6, cy - h / 6, h / 5, foliage);
    }

    /// Árbol pixel-art de pantano (retorcido, inclinado)
    private static void DrawSwampTree(int x, int y, int h, Color trunk, Color foliage)
    {
        // Tronco retorcido (segmentos)
        DrawRectangle(x - 2, y, 4, h / 2, trunk);
        DrawRectangle(x - 1, y + h / 2, 3, h / 4, trunk with { A = 200 });
        // Rama inclinada
        int bx = x + 8, by = y + h / 4;
        for (int i = 0; i < 5; i++)
            DrawRectangle(bx + i * 3, by - i, 2, 3, trunk);
        // Copa rala
        DrawCircle(bx + 5, by - 8, 10, foliage);
        DrawCircle(bx + 12, by - 4, 8, foliage with { A = 180 });
        DrawCircle(x - 3, y - h / 6, 8, foliage);
    }

    /// Juncos del delta
    private static void DrawReed(int x, int y, int h, Color stem, Color tip)
    {
        DrawRectangle(x, y - h, 2, h, stem);
        DrawCircle(x + 1, y - h, 3, tip);
    }

    /// Estalactita
    private static void DrawStalactite(int x, int y, int h, Color stone1, Color stone2, Color tip)
    {
        // Cuerpo
        DrawTriangle(
            (x, y),
            (x - 6, y + h * 2 / 3),
            (x + 6, y + h * 2 / 3),
            stone1);
        // Punta
        DrawTriangle(
            (x, y + h * 2 / 3),
            (x - 3, y + h),
            (x + 3, y + h),
            tip);
        // Sombra lateral
        DrawTriangle(
            (x + 1, y),
            (x - 3, y + h * 2 / 3),
            (x + 5, y + h * 2 / 3),
            stone2);
    }

    /// Columna caída (ruinas)
    private static void DrawColumn(int x, int y, int h, Color stone, Color shadow, bool fallen = false)
    {
        if (fallen)
        {
            // Columna caída horizontal/angular
            DrawRectangle(x - h / 2, y, h, 5, stone);
            DrawRectangle(x - h / 2, y + 2, h - 4, 2, shadow);
            // Base
            DrawRectangle(x - h / 2 - 4, y - 2, 6, 8, stone);
            DrawRectangle(x + h / 2 - 2, y - 1, 6, 6, stone);
        }
        else
        {
            DrawRectangle(x - 3, y - h, 6, h, stone);
            DrawRectangle(x - 2, y - h, 2, h, shadow);
            // Capitel
            DrawRectangle(x - 5, y - h, 10, 4, stone);
            DrawRectangle(x - 5, y, 10, 3, stone);
        }
    }

    /// Bloque de piedra (ruinas, mazmorra)
    private static void DrawBlock(int x, int y, int w, int h, Color stone, Color shadow)
    {
        DrawRectangle(x, y, w, h, stone);
        DrawRectangle(x, y, w, 2, shadow with { A = 80 });
        DrawRectangle(x, y, 2, h, shadow with { A = 60 });
        DrawRectangle(x + w - 2, y + h - 2, 2, 2, shadow);
    }

    /// Círculo relleno (helper rápido)
    private static void DrawCircle(int x, int y, float r, Color color)
    {
        Raylib_cs.Raylib.DrawCircle(x, y, r, color);
    }

    /// Triángulo relleno
    private static void DrawTriangle((int x, int y) p1, (int x, int y) p2, (int x, int y) p3, Color color)
    {
        Raylib_cs.Raylib.DrawTriangle(
            new(p1.x, p1.y),
            new(p2.x, p2.y),
            new(p3.x, p3.y),
            color);
    }

    /// Rectángulo relleno (wrapper)
    private static void DrawRectangle(int x, int y, int w, int h, Color color)
    {
        if (w > 0 && h > 0)
            Raylib_cs.Raylib.DrawRectangle(x, y, w, h, color);
    }

    // ═══════════════════════════════════════════════════════════════════════════════
    //  ALDEA — noche estrellada, casas con luz, árboles, neblina baja
    // ═══════════════════════════════════════════════════════════════════════════════
    private static void Village(int sw, int sh)
    {
        // Cielo nocturno
        DrawGradientV(0, 0, sw, sh, C(18, 14, 35), C(45, 30, 18));
        DrawGradientV(0, 0, sw, sh / 3, C(30, 25, 55, 120), C(0, 0, 0, 0));

        // Estrellas
        float t = (float)GetTime();
        for (int i = 0; i < 25; i++)
        {
            float px = (MathF.Sin(t * 0.05f + i * 7.3f) * 0.5f + 0.5f) * sw * 0.9f + sw * 0.05f;
            float py = (MathF.Sin(t * 0.03f + i * 11.7f) * 0.5f + 0.5f) * sh * 0.3f + 10;
            int alpha = 100 + (int)(MathF.Sin(t * 1.5f + i * 2.3f) * 80);
            DrawCircle((int)px, (int)py, 1 + (i % 3 == 0 ? 1 : 0), C(255, 240, 200, alpha));
        }

        // Luna creciente
        int moonX = sw - 120, moonY = 50;
        DrawCircle(moonX, moonY, 18, C(240, 230, 210, 200));
        DrawCircle(moonX + 5, moonY - 3, 15, C(18, 14, 35)); // recorte
        DrawCircle(moonX, moonY, 25, C(240, 230, 210, 30)); // halo

        // Siluetas de árboles lejanos
        for (int i = 0; i < 5; i++)
        {
            int tx = 20 + i * sw / 5 + (int)(MathF.Sin(i * 2.5f) * 30);
            int th = 60 + (i % 3) * 30;
            DrawTree(tx, sh - 60 - th + 10, th,
                C(15, 12, 20, 160),
                C(10, 18, 10, 120),
                C(8, 14, 8, 100));
        }

        // Casas del pueblo
        DrawHouse(120, sh - 95, 50, 40, C(60, 40, 25), C(130, 50, 30), C(255, 200, 80));
        DrawHouse(260, sh - 90, 45, 35, C(55, 38, 22), C(120, 45, 25), C(255, 210, 90));
        DrawHouse(400, sh - 100, 55, 45, C(65, 45, 28), C(140, 55, 35), C(255, 190, 70));
        DrawHouse(560, sh - 85, 40, 30, C(50, 35, 20), C(110, 40, 20));
        DrawHouse(700, sh - 92, 48, 38, C(58, 40, 24), C(125, 48, 28), C(255, 220, 100));
        DrawHouse(850, sh - 88, 42, 34, C(52, 36, 22), C(115, 42, 24));

        // Valla de madera
        for (int i = 0; i < 12; i++)
        {
            int vx = 60 + i * 75;
            DrawRectangle(vx, sh - 42, 3, 12, C(60, 40, 25));
            DrawRectangle(vx + 5, sh - 38, 50, 2, C(50, 35, 20));
            DrawRectangle(vx + 5, sh - 34, 50, 2, C(50, 35, 20));
        }

        // Camino de tierra
        DrawRectangle(0, sh - 18, sw, 18, C(80, 60, 35));
        DrawRectangle(0, sh - 16, sw, 6, C(60, 45, 25));

        // Luciérnagas (más orgánicas)
        for (int i = 0; i < 15; i++)
        {
            float px = sw * 0.05f + (MathF.Sin(t * 0.6f + i * 2.1f) * 0.5f + 0.5f) * sw * 0.9f;
            float py = sh * 0.3f + (MathF.Cos(t * 0.5f + i * 2.7f) * 0.5f + 0.5f) * sh * 0.5f;
            float sz = 1.5f + MathF.Sin(t * 2.5f + i * 3f) * 0.8f;
            int a = 120 + (int)(MathF.Sin(t * 2f + i * 3f) * 80);
            DrawCircle((int)px, (int)py, sz, C(255, 220, 100, a));
            DrawCircle((int)px, (int)py, sz * 2.5f, C(255, 220, 100, a / 4));
        }

        // Humo de chimeneas
        for (int i = 0; i < 6; i++)
        {
            float hx = 145 + i * 140;
            float hy = sh - 125 - MathF.Sin(t * 0.8f + i * 1.5f) * 10;
            float ha = 40 + (int)(MathF.Sin(t * 0.6f + i * 2.1f) * 20);
            DrawCircle((int)hx, (int)hy, 4 + MathF.Sin(t * 1.2f + i * 1.8f) * 2, C(180, 160, 140, (int)ha));
            DrawCircle((int)hx + 3, (int)hy - 8, 3 + MathF.Sin(t * 1.0f + i * 2.5f) * 1.5f, C(160, 140, 120, (int)(ha * 0.7f)));
        }
    }

    // ═══════════════════════════════════════════════════════════════════════════════
    //  BOSQUE — frondoso, rayos de luna, niebla densa, hongos bioluminiscentes
    // ═══════════════════════════════════════════════════════════════════════════════
    private static void Forest(int sw, int sh)
    {
        // Cielo nocturno boscoso
        DrawGradientV(0, 0, sw, sh, C(2, 14, 6), C(8, 20, 5));
        DrawGradientV(0, 0, sw, sh, C(20, 55, 25, 80), C(0, 0, 0, 0));
        DrawGradientV(0, sh / 3, sw, sh / 3, C(35, 90, 45, 60), C(0, 0, 0, 0));

        float t = (float)GetTime();

        // Siluetas de árboles (fondo)
        int[] treeX = [30, 100, 180, 280, 380, 480, 580, 680, 780, 880, 930];
        foreach (int tx in treeX)
        {
            int th = 120 + (tx * 7 % 5) * 40;
            DrawTree(tx, sh - 30 - th + 15, th,
                C(10, 20, 10, 180),
                C(8, 25, 12, 140),
                C(6, 20, 10, 120));
        }

        // Árboles principales (más cerca, más detalle)
        DrawTree(60, sh - 60, 130, C(40, 25, 15), C(20, 80, 25, 200), C(15, 60, 20, 180));
        DrawTree(180, sh - 55, 120, C(50, 30, 18), C(25, 90, 30, 200), C(18, 70, 25, 180));
        DrawTree(350, sh - 65, 140, C(35, 20, 12), C(22, 85, 28, 200), C(16, 65, 22, 180));
        DrawTree(520, sh - 50, 110, C(45, 28, 16), C(20, 78, 25, 200), C(14, 58, 20, 180));
        DrawTree(720, sh - 58, 135, C(38, 22, 14), C(24, 88, 30, 200), C(18, 68, 24, 180));
        DrawTree(900, sh - 52, 115, C(42, 26, 15), C(22, 82, 28, 200), C(16, 62, 22, 180));

        // Rayos de luna más sutiles y variados
        for (int i = 0; i < 8; i++)
        {
            float rx = sw * 0.03f + i * sw * 0.13f + MathF.Sin(i * 1.7f) * 15;
            int alpha = 25 + (int)(MathF.Sin(t * 0.7f + i * 1.3f) * 18);
            int rw = 3 + (i % 3) * 2;
            DrawGradientV((int)rx, 0, rw, sh, C(180, 220, 190, alpha), C(180, 220, 190, 0));
            // Halo en la base del rayo
            DrawCircle((int)rx + rw / 2, sh, rw * 8, C(180, 220, 190, alpha / 6));
        }

        // Niebla baja (3 capas)
        DrawGradientV(0, sh * 2 / 3, sw, sh / 3, C(40, 80, 50, 80), C(0, 0, 0, 0));
        DrawGradientV(0, sh * 3 / 4, sw, sh / 4, C(50, 90, 55, 60), C(0, 0, 0, 0));
        DrawGradientV(0, sh * 4 / 5, sw, sh / 5, C(30, 60, 35, 40), C(0, 0, 0, 0));

        // Partículas de bruma
        for (int i = 0; i < 24; i++)
        {
            float px = (MathF.Sin(t * 0.25f + i * 2.5f) * 0.5f + 0.5f) * sw * 1.2f - sw * 0.1f;
            float py = sh * 0.5f + (MathF.Cos(t * 0.18f + i * 3.3f) * 0.5f + 0.5f) * sh * 0.4f;
            int a = 30 + (int)(MathF.Sin(t * 0.5f + i * 1.7f) * 20);
            DrawCircle((int)px, (int)py, 3 + MathF.Sin(t * 0.4f + i * 2.1f) * 1.5f, C(120, 200, 140, a));
        }

        // Hongos bioluminiscentes
        (int hx, int hy)[] mushrooms = [(95, 380), (220, 410), (460, 390), (610, 420), (800, 395), (350, 430)];
        foreach (var (hx, hy) in mushrooms)
        {
            int a = 80 + (int)(MathF.Sin(t * 1.8f + hx * 0.05f) * 50);
            DrawCircle(hx, hy, 3, C(200, 120, 60, a));
            DrawCircle(hx + 3, hy - 2, 2, C(230, 180, 140, a));
            DrawCircle(hx, hy, 8, C(200, 120, 60, a / 5));
        }

        // Hojas cayendo
        for (int i = 0; i < 8; i++)
        {
            float lx = (MathF.Sin(t * 1.2f + i * 2.3f) * 0.5f + 0.5f) * sw;
            float ly = (t * 25 + i * 40) % sh;
            float lw = 2 + MathF.Sin(t * 3f + i * 1.5f) * 1;
            DrawCircle((int)lx, (int)ly, lw, C(40, 120, 35, 60));
            DrawCircle((int)lx + 2, (int)ly + 1, lw * 0.5f, C(50, 140, 45, 40));
        }
    }

    // ═══════════════════════════════════════════════════════════════════════════════
    //  CUEVA — estalactitas, cristales brillantes, charcos de agua, profundidad
    // ═══════════════════════════════════════════════════════════════════════════════
    private static void Cave(int sw, int sh)
    {
        // Oscuridad con matiz azul profundo
        DrawGradientV(0, 0, sw, sh, C(2, 2, 6), C(6, 4, 3));
        DrawGradientV(0, 0, sw, sh, C(8, 8, 20, 100), C(0, 0, 0, 0));
        DrawGradientV(0, sh / 4, sw, sh / 4, C(15, 15, 35, 80), C(0, 0, 0, 0));

        float t = (float)GetTime();

        // Estalactitas del techo
        (int sx, int len)[] stalactites = [
            (30, 50), (80, 70), (140, 45), (210, 80), (280, 55),
            (350, 65), (420, 40), (490, 90), (560, 50), (630, 75),
            (700, 55), (770, 85), (840, 45), (910, 60)
        ];
        foreach (var (sx, len) in stalactites)
        {
            DrawStalactite(sx, 0, len,
                C(50, 45, 40, 180),
                C(35, 30, 28, 160),
                C(25, 22, 20, 150));
        }

        // Estalagmitas (suelo)
        (int gx, int glen)[] stalagmites = [
            (50, 25), (120, 35), (190, 20), (260, 40), (340, 30),
            (410, 22), (480, 45), (550, 28), (620, 38), (690, 25),
            (760, 42), (830, 30), (890, 20)
        ];
        foreach (var (gx, glen) in stalagmites)
        {
            DrawTriangle(
                (gx, sh),
                (gx - 4, sh - glen),
                (gx + 4, sh - glen),
                C(35, 30, 28, 160));
        }

        // Cristales brillantes (con diferentes colores)
        (int cx, int cy, int hue)[] gems = [
            (60, 140, 0), (170, 250, 1), (300, 380, 0), (450, 200, 2),
            (540, 350, 1), (660, 120, 0), (750, 280, 2), (840, 390, 1),
            (200, 360, 2), (500, 120, 1)
        ];
        foreach (var (cx, cy, hue) in gems)
        {
            float alpha = 140 + MathF.Sin(t * 1.8f + cx * 0.08f) * 80;
            float sz = 2.5f + MathF.Sin(t * 2.2f + cy * 0.06f) * 1.2f;
            Color col = hue switch
            {
                1 => C(40, 200, 180, (int)alpha),   // turquesa
                2 => C(200, 80, 220, (int)alpha),   // púrpura
                _ => C(60, 140, 230, (int)alpha)    // azul
            };
            Color glow = hue switch
            {
                1 => C(40, 200, 180, (int)(alpha / 4)),
                2 => C(200, 80, 220, (int)(alpha / 4)),
                _ => C(60, 140, 230, (int)(alpha / 4))
            };
            DrawCircle(cx, cy, sz, col);
            DrawCircle(cx, cy, sz * 2.5f, glow);
            // Destello central
            DrawCircle(cx, cy, sz * 0.4f, C(220, 240, 255, (int)(alpha * 0.6f)));
        }

        // Charco de agua con reflejos
        DrawGradientV(0, sh - 40, sw, 40, C(5, 10, 30, 180), C(3, 6, 20, 120));
        for (int i = 0; i < 12; i++)
        {
            float rx = (MathF.Sin(t * 0.3f + i * 2.5f) * 0.5f + 0.5f) * sw;
            float ry = sh - 15 + MathF.Sin(t * 1.5f + i * 1.8f) * 5;
            int ra = 30 + (int)(MathF.Sin(t * 2f + i * 3.1f) * 20);
            DrawCircle((int)rx, (int)ry, 2, C(60, 140, 200, ra));
        }

        // Sombra inferior profunda
        DrawGradientV(0, sh / 2, sw, sh / 2, C(0, 0, 0, 60), C(0, 0, 0, 200));
    }

    // ═══════════════════════════════════════════════════════════════════════════════
    //  RUINAS — atardecer, columnas caídas, polvo dorado, ambiente melancólico
    // ═══════════════════════════════════════════════════════════════════════════════
    private static void Ruins(int sw, int sh)
    {
        // Cielo atardecer
        DrawGradientV(0, 0, sw, sh, C(28, 20, 14), C(45, 30, 16));
        DrawGradientV(0, 0, sw, sh, C(70, 50, 25, 80), C(0, 0, 0, 0));

        float t = (float)GetTime();

        // Sol poniente con halo
        DrawCircle(sw / 2, 30, 100, C(240, 180, 60, 20));
        DrawCircle(sw / 2, 35, 60, C(255, 200, 80, 35));
        DrawCircle(sw / 2, 40, 30, C(255, 220, 100, 50));
        // Disco solar
        DrawCircle(sw / 2, 42, 18, C(255, 200, 70, 200));
        DrawCircle(sw / 2, 42, 14, C(255, 220, 120, 180));

        // Nubes teñidas
        for (int i = 0; i < 6; i++)
        {
            float nx = (MathF.Sin(i * 1.3f + t * 0.02f) * 0.5f + 0.5f) * sw * 0.8f + sw * 0.1f;
            float ny = 20 + i * 12;
            DrawCircle((int)nx, (int)ny, 18 + i * 2, C(200, 120, 60, 30));
            DrawCircle((int)nx + 20, (int)ny + 5, 14 + i, C(210, 140, 70, 25));
        }

        // Columnas en pie (siluetas)
        DrawColumn(120, sh - 30, 130, C(55, 45, 35, 200), C(35, 28, 22, 160));
        DrawColumn(280, sh - 25, 110, C(50, 40, 32, 200), C(32, 26, 20, 160));
        DrawColumn(680, sh - 28, 120, C(55, 45, 35, 200), C(35, 28, 22, 160));
        DrawColumn(850, sh - 22, 100, C(50, 40, 32, 200), C(32, 26, 20, 160));

        // Columnas caídas
        DrawColumn(350, sh - 18, 80, C(45, 38, 30, 180), C(30, 25, 20, 140), fallen: true);
        DrawColumn(550, sh - 15, 70, C(45, 38, 30, 180), C(30, 25, 20, 140), fallen: true);

        // Bloques de piedra dispersos
        DrawBlock(180, sh - 16, 25, 16, C(60, 50, 40, 180), C(40, 32, 25));
        DrawBlock(750, sh - 14, 30, 14, C(55, 45, 35, 180), C(38, 30, 22));
        DrawBlock(440, sh - 12, 20, 12, C(50, 40, 32, 180), C(35, 28, 22));

        // Hierba crecida
        for (int i = 0; i < 30; i++)
        {
            float gx = MathF.Sin(i * 1.7f) * sw * 0.45f + sw * 0.05f + i * sw * 0.03f;
            int gh = 5 + (i % 5) * 3;
            DrawRectangle((int)gx, sh - 6 - gh * 2, 2, gh, C(40, 85, 35, 140));
            DrawRectangle((int)gx + 3, sh - 6 - gh * 2 + gh / 2, 1, gh / 2, C(55, 105, 45, 120));
        }

        // Polvo dorado atmosférico
        for (int i = 0; i < 20; i++)
        {
            float px = (MathF.Sin(t * 0.35f + i * 1.7f) * 0.5f + 0.5f) * sw;
            float py = (MathF.Cos(t * 0.28f + i * 2.9f) * 0.5f + 0.5f) * sh * 0.5f + sh * 0.15f;
            int a = 30 + (int)(MathF.Sin(t * 0.8f + i * 2.3f) * 20);
            DrawCircle((int)px, (int)py, 1.5f + MathF.Sin(t * 0.5f + i * 1.5f) * 0.5f, C(220, 190, 130, a));
        }

        // Luciérnagas doradas
        for (int i = 0; i < 10; i++)
        {
            float px = sw * 0.1f + (MathF.Sin(t * 0.5f + i * 2.5f) * 0.5f + 0.5f) * sw * 0.8f;
            float py = sh * 0.35f + (MathF.Cos(t * 0.45f + i * 3.1f) * 0.5f + 0.5f) * sh * 0.5f;
            int a = 60 + (int)(MathF.Sin(t * 1.8f + i * 2.7f) * 40);
            DrawCircle((int)px, (int)py, 2, C(255, 210, 80, a));
            DrawCircle((int)px, (int)py, 4, C(255, 210, 80, a / 4));
        }

        // Sombra inferior
        DrawGradientV(0, sh - 60, sw, 60, C(0, 0, 0, 80), C(0, 0, 0, 0));
    }

    // ═══════════════════════════════════════════════════════════════════════════════
    //  DELTA — pantano, agua oscura, juncos, árboles retorcidos, niebla, fuegos fatuos
    // ═══════════════════════════════════════════════════════════════════════════════
    private static void Delta(int sw, int sh)
    {
        // Cielo pantanoso (violáceo oscuro)
        DrawGradientV(0, 0, sw, sh, C(16, 10, 28), C(10, 18, 14));
        DrawGradientV(0, 0, sw, sh / 2, C(25, 18, 38, 80), C(0, 0, 0, 0));

        float t = (float)GetTime();

        // Luna brumosa (oculta tras niebla)
        int moonX = sw - 100, moonY = 55;
        DrawCircle(moonX, moonY, 22, C(200, 210, 190, 80));
        DrawCircle(moonX, moonY, 16, C(220, 230, 210, 110));
        DrawCircle(moonX, moonY, 30, C(200, 210, 190, 25));

        // Árboles retorcidos del pantano (siluetas)
        DrawSwampTree(60, sh - 40, 110, C(25, 18, 12, 200), C(15, 35, 20, 150));
        DrawSwampTree(180, sh - 35, 95, C(22, 16, 10, 200), C(12, 30, 18, 150));
        DrawSwampTree(350, sh - 45, 130, C(28, 20, 14, 200), C(18, 38, 22, 150));
        DrawSwampTree(520, sh - 30, 85, C(22, 16, 10, 200), C(12, 30, 18, 150));
        DrawSwampTree(700, sh - 42, 120, C(26, 18, 12, 200), C(16, 36, 20, 150));
        DrawSwampTree(880, sh - 32, 90, C(24, 17, 11, 200), C(14, 32, 19, 150));

        // Juncos cerca del agua
        (int rx, int rl)[] reeds = [
            (40, 30), (90, 25), (150, 35), (230, 28), (290, 32),
            (410, 28), (470, 35), (560, 25), (620, 30), (670, 22),
            (760, 32), (820, 28), (910, 25), (940, 30)
        ];
        foreach (var (rx, rl) in reeds)
        {
            DrawReed(rx, sh - 10, rl, C(45, 65, 35, 180), C(80, 60, 30, 200));
            DrawReed(rx + 3, sh - 5, rl - 5, C(35, 55, 28, 160), C(70, 50, 25, 180));
        }

        // Superficie de agua oscura
        DrawGradientV(0, sh - 40, sw, 40, C(8, 25, 20, 200), C(5, 18, 14, 160));

        // Reflejos en el agua (ondulaciones)
        for (int i = 0; i < 16; i++)
        {
            float rx = (MathF.Sin(t * 0.25f + i * 2.1f) * 0.5f + 0.5f) * sw;
            float ry = sh - 18 + MathF.Sin(t * 1.2f + i * 1.5f) * 8;
            int ra = 20 + (int)(MathF.Sin(t * 1.8f + i * 2.5f) * 15);
            DrawCircle((int)rx, (int)ry, 2, C(100, 160, 140, ra));
            DrawCircle((int)rx + 4, (int)ry + 2, 1.5f, C(120, 180, 160, ra / 2));
        }

        // Reflejos de luna en agua
        for (int i = 0; i < 5; i++)
        {
            float rx = moonX + MathF.Sin(t * 0.5f + i * 1.3f) * 15;
            float ry = sh - 25 + i * 6;
            int ra = 40 - i * 6;
            if (ra > 0)
                DrawCircle((int)rx, (int)ry, 3 - i * 0.4f, C(200, 220, 190, ra));
        }

        // Fuegos fatuos (will-o'-the-wisp) — partículas verdes/azules danzantes
        for (int i = 0; i < 12; i++)
        {
            float px = sw * 0.05f + (MathF.Sin(t * 0.4f + i * 2.8f) * 0.5f + 0.5f) * sw * 0.9f;
            float py = sh * 0.25f + (MathF.Cos(t * 0.35f + i * 3.4f) * 0.5f + 0.5f) * sh * 0.5f;
            float sz = 2 + MathF.Sin(t * 2.5f + i * 3.7f) * 1;
            int a = 70 + (int)(MathF.Sin(t * 2f + i * 2.3f) * 50);
            Color c = i % 3 == 0
                ? C(80, 200, 180, a)    // turquesa
                : i % 3 == 1
                    ? C(180, 220, 80, a)  // verde
                    : C(140, 200, 220, a); // azul claro
            DrawCircle((int)px, (int)py, sz, c);
            DrawCircle((int)px, (int)py, sz * 3, c with { A = (byte)(a / 4) });
        }

        // Niebla baja (densa, 2 capas)
        DrawGradientV(0, sh * 3 / 5, sw, sh * 2 / 5, C(60, 80, 75, 50), C(0, 0, 0, 0));
        DrawGradientV(0, sh * 4 / 5, sw, sh / 5, C(40, 60, 55, 80), C(0, 0, 0, 0));

        // Bruma flotante
        for (int i = 0; i < 20; i++)
        {
            float px = (MathF.Sin(t * 0.15f + i * 2.2f) * 0.5f + 0.5f) * sw * 1.3f - sw * 0.15f;
            float py = sh * 0.4f + (MathF.Cos(t * 0.12f + i * 3.6f) * 0.5f + 0.5f) * sh * 0.4f;
            int a = 15 + (int)(MathF.Sin(t * 0.4f + i * 1.9f) * 12);
            DrawCircle((int)px, (int)py, 4 + MathF.Sin(t * 0.3f + i * 2.5f) * 2, C(100, 140, 130, a));
        }

        // Sombras inferiores
        DrawGradientV(0, sh - 20, sw, 20, C(0, 0, 0, 80), C(0, 0, 0, 0));
    }

    // ═══════════════════════════════════════════════════════════════════════════════
    //  MAZMORRA — piedra oscura, lava/de fuego, rejas, cadenas, opresión
    // ═══════════════════════════════════════════════════════════════════════════════
    private static void Dungeon(int sw, int sh)
    {
        // Oscuridad infernal
        DrawGradientV(0, 0, sw, sh, C(4, 1, 8), C(10, 2, 5));
        DrawGradientV(0, 0, sw, sh, C(20, 3, 15, 100), C(0, 0, 0, 0));
        // Resplandor rojo desde abajo
        DrawGradientV(0, sh / 2, sw, sh / 2, C(180, 30, 10, 60), C(0, 0, 0, 0));
        DrawGradientV(0, sh * 3 / 4, sw, sh / 4, C(220, 40, 15, 40), C(0, 0, 0, 0));

        float t = (float)GetTime();

        // Pilares de piedra
        for (int i = 0; i < 5; i++)
        {
            int px = 40 + i * sw / 4 + 30;
            DrawRectangle(px, 0, 16, sh, C(20, 15, 25, 180));
            DrawRectangle(px + 2, 0, 3, sh, C(30, 22, 35, 120));
            DrawRectangle(px + 11, 0, 3, sh, C(12, 8, 16, 140));
        }

        // Rejas / barrotes
        for (int i = 0; i < 14; i++)
        {
            int bx = 80 + i * 60;
            DrawRectangle(bx, sh / 4, 3, sh / 2, C(60, 50, 60, 160));
            DrawRectangle(bx + 1, sh / 4 + 2, 1, sh / 2 - 4, C(90, 80, 90, 100));
        }
        // Barra horizontal de reja
        DrawRectangle(60, sh / 3, sw - 120, 4, C(55, 45, 55, 160));
        DrawRectangle(60, sh / 2, sw - 120, 4, C(55, 45, 55, 160));

        // Llamas grandes (fuego infernal)
        for (int i = 0; i < 7; i++)
        {
            float fx = sw * 0.06f + i * sw * 0.15f + MathF.Sin(t * 3.5f + i * 2f) * 18;
            float fy = sh - 12 + MathF.Sin(t * 5f + i * 2.5f) * 6;

            // Resplandor en suelo
            DrawCircle((int)fx, sh - 5, 20 + MathF.Sin(t * 2f + i * 1.2f) * 5, C(255, 60, 10, 40));

            // Llama exterior (rojo-anaranjado)
            DrawCircle((int)fx, (int)fy, 14 + MathF.Sin(t * 4f + i * 1.5f) * 4, C(200, 60, 15, 100));
            // Llama media (naranja)
            DrawCircle((int)fx, (int)(fy - 4), 9 + MathF.Sin(t * 4.5f + i * 1.8f) * 3, C(255, 140, 30, 90));
            // Llama interior (amarillo)
            DrawCircle((int)fx, (int)(fy - 8), 5 + MathF.Sin(t * 5f + i * 2f) * 2, C(255, 210, 60, 80));
            // Centro blanco
            DrawCircle((int)fx, (int)(fy - 11), 2 + MathF.Sin(t * 5.5f + i * 2.2f) * 1, C(255, 250, 220, 70));
        }

        // Ascuas / chispas volando
        for (int i = 0; i < 15; i++)
        {
            float sx = (MathF.Sin(t * 2f + i * 1.3f) * 0.5f + 0.5f) * sw;
            float sy = sh - 30 - (t * 35 + i * 20) % (sh * 0.6f);
            int sa = 80 - (int)((sh - sy) / sh * 60);
            if (sa > 0)
                DrawCircle((int)sx, (int)sy, 1.5f + MathF.Sin(t * 3f + i * 2.5f) * 0.5f, C(255, 150, 30, sa));
        }

        // Cadenas colgando
        for (int i = 0; i < 4; i++)
        {
            int cx = 100 + i * (sw - 200) / 3;
            for (int j = 0; j < 8; j++)
            {
                int cy = 40 + j * 18;
                int sway = (int)(MathF.Sin(t * 1.5f + i * 1.7f + j * 0.5f) * 4);
                DrawRectangle(cx + sway, cy, 2, 8, C(60, 55, 60, 120));
                DrawRectangle(cx + sway + 3, cy + 2, 3, 4, C(45, 40, 45, 100));
            }
        }

        // Techo opresivo (más oscuro)
        DrawGradientV(0, 0, sw, sh / 3, C(0, 0, 0, 200), C(0, 0, 0, 0));
        // Esquinas oscuras
        DrawGradientV(0, 0, 60, sh, C(0, 0, 0, 120), C(0, 0, 0, 0));
        DrawGradientV(sw - 60, 0, 60, sh, C(0, 0, 0, 120), C(0, 0, 0, 0));
    }

    // ═══════════════════════════════════════════════════════════════════════════════
    //  DEFAULT
    // ═══════════════════════════════════════════════════════════════════════════════
    private static void DefaultBg(int sw, int sh)
    {
        DrawGradientV(0, 0, sw, sh, C(10, 14, 28), C(16, 20, 38));
    }

    private static void DrawGradientV(int x, int y, int w, int h, Color t, Color b)
    {
        if (h > 0 && w > 0)
            Raylib_cs.Raylib.DrawRectangleGradientV(x, y, w, h, t, b);
    }

    private static void DrawGradientH(int x, int y, int w, int h, Color l, Color r)
    {
        if (h > 0 && w > 0)
            Raylib_cs.Raylib.DrawRectangleGradientH(x, y, w, h, l, r);
    }
}
