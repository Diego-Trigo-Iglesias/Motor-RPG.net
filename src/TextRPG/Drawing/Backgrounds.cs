/// <summary>
/// Fondos multi-capa para cada localizaci�n. Cada fondo combina:
///   � 2-3 gradientes superpuestos para profundidad y atm�sfera
///   � Part�culas ambientales animadas (luci�rnagas, polvo, cristales, llamas)
///   � Efectos atmosf�ricos (niebla, rayos de luna, sombras)
///   � Tinte rojo sutil en combate
/// El m�todo Draw() es el punto de entrada �nico � decide el fondo seg�n el locationId.
/// </summary>

using Raylib_cs;
using static Raylib_cs.Raylib;

namespace TextRPG.Drawing;

/// <summary>FONDOS ÉPICOS con capas dramáticas, luces, partículas y atmósfera.</summary>
public static class Backgrounds
{
    private static Color C(int r, int g, int b, int a = 255) => new((byte)r, (byte)g, (byte)b, (byte)a);

    public static void Draw(int sw, int sh, string locId, bool combat = false)
    {
        switch (locId)
        {
            case "village": Village(sw, sh); break;
            case "forest":  Forest(sw, sh);  break;
            case "cave":    Cave(sw, sh);    break;
            case "ruins":   Ruins(sw, sh);   break;
            case "dungeon": Dungeon(sw, sh); break;
            default:        DefaultBg(sw, sh); break;
        }
        // Oscuridad sutil en bordes
        DrawGradientV(0, 0, sw, 60, C(0, 0, 0, 120), C(0, 0, 0, 0));
        DrawGradientV(0, sh - 60, sw, 60, C(0, 0, 0, 0), C(0, 0, 0, 120));
        if (combat) DrawRectangle(0, 0, sw, sh, C(200, 30, 30, 45));
    }

    // ─── ALDEA: marrones, naranjas, luciérnagas ───────────────────────
    private static void Village(int sw, int sh)
    {
        DrawGradientV(0, 0, sw, sh, C(55, 38, 22), C(75, 55, 30));
        DrawGradientV(0, 0, sw, sh, C(100, 75, 45, 80), C(0, 0, 0, 0));
        DrawGradientV(0, sh/2, sw, sh/2, C(160, 100, 50, 60), C(0, 0, 0, 0));
        // Luciérnagas más grandes y brillantes
        float t = (float)GetTime();
        for (int i = 0; i < 12; i++)
        {
            float px = sw * 0.1f + (MathF.Sin(t * 0.7f + i * 2.1f) * 0.5f + 0.5f) * sw * 0.8f;
            float py = sh * 0.15f + (MathF.Cos(t * 0.5f + i * 2.7f) * 0.5f + 0.5f) * sh * 0.6f;
            float sz = 2 + MathF.Sin(t * 2f + i * 3f) * 1;
            int a = 150 + (int)(MathF.Sin(t * 2.5f + i * 3f) * 80);
            DrawCircle((int)px, (int)py, sz, C(255, 210, 80, a));
            DrawCircle((int)px, (int)py, sz * 2, C(255, 210, 80, a / 3));
        }
    }

    // ─── BOSQUE: verdes oscuros, rayos luna, niebla ───────────────────
    private static void Forest(int sw, int sh)
    {
        DrawGradientV(0, 0, sw, sh, C(4, 18, 8), C(10, 22, 6));
        DrawGradientV(0, 0, sw, sh, C(25, 60, 30, 70), C(0, 0, 0, 0));
        DrawGradientV(0, sh/3, sw, sh/3, C(40, 100, 50, 40), C(0, 0, 0, 0));
        // Rayos de luna más visibles
        float t = (float)GetTime();
        for (int i = 0; i < 6; i++)
        {
            float rx = sw * 0.05f + i * sw * 0.18f;
            int alpha = 40 + (int)(MathF.Sin(t * 0.8f + i * 1.3f) * 25);
            DrawGradientV((int)rx, 0, 5 + (i % 2) * 3, sh, C(200, 230, 210, alpha), C(200, 230, 210, 0));
        }
        // Niebla
        DrawGradientV(0, sh * 2/3, sw, sh/3, C(50, 80, 55, 60), C(0, 0, 0, 0));
        // Bruma flotante
        for (int i = 0; i < 18; i++)
        {
            float px = (MathF.Sin(t * 0.3f + i * 2.5f) * 0.5f + 0.5f) * sw;
            float py = (MathF.Cos(t * 0.2f + i * 3.3f) * 0.5f + 0.5f) * sh;
            DrawCircle((int)px, (int)py, 2, C(120, 200, 140, 60));
        }
    }

    // ─── CUEVA: oscura, azul profundo, cristales ──────────────────────
    private static void Cave(int sw, int sh)
    {
        DrawGradientV(0, 0, sw, sh, C(4, 4, 8), C(8, 6, 4));
        DrawGradientV(0, 0, sw, sh, C(12, 12, 24, 80), C(0, 0, 0, 0));
        DrawGradientV(0, sh/4, sw, sh/4, C(20, 20, 40, 60), C(0, 0, 0, 0));
        // Cristales brillantes
        float t = (float)GetTime();
        (int x, int y)[] gems = [(60, 100), (160, 220), (740, 70), (840, 190), (400, 340), (680, 280), (900, 350), (50, 380)];
        foreach (var (gx, gy) in gems)
        {
            float alpha = 160 + MathF.Sin(t * 2f + gx * 0.1f) * 80;
            float sz = 3 + MathF.Sin(t * 2.5f + gy * 0.1f) * 1.5f;
            DrawCircle(gx, gy, sz, C(60, 140, 230, (int)alpha));
            DrawCircle(gx, gy, sz * 2.5f, C(60, 140, 230, (int)(alpha / 4)));
        }
        // Sombra abajo
        DrawGradientV(0, sh/2, sw, sh/2, C(0, 0, 0, 60), C(0, 0, 0, 180));
    }

    // ─── RUINAS: atardecer, polvo, dorados ────────────────────────────
    private static void Ruins(int sw, int sh)
    {
        DrawGradientV(0, 0, sw, sh, C(32, 24, 16), C(48, 35, 18));
        DrawGradientV(0, 0, sw, sh, C(70, 55, 30, 80), C(0, 0, 0, 0));
        // Sol poniente
        DrawGradientV(0, 0, sw, sh/3, C(240, 170, 70, 60), C(0, 0, 0, 0));
        DrawCircle(sw/2, 40, 80, C(255, 180, 60, 30));
        // Polvo dorado
        float t = (float)GetTime();
        for (int i = 0; i < 16; i++)
        {
            float px = (MathF.Sin(t * 0.4f + i * 1.7f) * 0.5f + 0.5f) * sw;
            float py = (MathF.Cos(t * 0.3f + i * 2.9f) * 0.5f + 0.5f) * sh * 0.6f + sh * 0.2f;
            DrawCircle((int)px, (int)py, 2, C(220, 180, 120, 60));
        }
    }

    // ─── MAZMORRA: púrpura, rojo fuego, llamas ───────────────────────
    private static void Dungeon(int sw, int sh)
    {
        DrawGradientV(0, 0, sw, sh, C(5, 2, 10), C(12, 3, 6));
        DrawGradientV(0, 0, sw, sh, C(25, 5, 18, 80), C(0, 0, 0, 0));
        // Fondo rojo infernal
        DrawGradientV(0, sh/2, sw, sh/2, C(180, 40, 15, 60), C(0, 0, 0, 0));
        // Llamas grandes y brillantes
        float t = (float)GetTime();
        for (int i = 0; i < 6; i++)
        {
            float fx = sw * 0.08f + i * sw * 0.17f + MathF.Sin(t * 4f + i * 2f) * 15;
            int fy = sh - 15 + (int)(MathF.Sin(t * 6f + i * 2.5f) * 8);
            // Llama exterior
            DrawCircle((int)fx, fy, 12 + MathF.Sin(t * 4f + i * 1.5f) * 4, C(255, 100, 20, 120));
            DrawCircle((int)fx, fy - 5, 7 + MathF.Sin(t * 5f + i * 1.8f) * 3, C(255, 180, 40, 100));
            DrawCircle((int)fx, fy - 10, 3 + MathF.Sin(t * 6f + i * 2f) * 2, C(255, 220, 80, 80));
        }
        // Techo opresivo
        DrawGradientV(0, 0, sw, sh/3, C(0, 0, 0, 160), C(0, 0, 0, 0));
    }

    private static void DefaultBg(int sw, int sh) => DrawGradientV(0, 0, sw, sh, C(10, 14, 28), C(16, 20, 38));

    private static void DrawGradientV(int x, int y, int w, int h, Color t, Color b)
    { if (h > 0 && w > 0) DrawRectangleGradientV(x, y, w, h, t, b); }
}

