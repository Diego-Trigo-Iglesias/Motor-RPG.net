/// <summary>
/// Componentes de UI reutilizables. TODOS son métodos estáticos sin estado.
/// Cada función dibuja un elemento concreto: panel, barra de HP, log, menú.
/// Se usa composición en lugar de herencia — GameRenderer llama a estos métodos
/// en el orden correcto para construir cada pantalla.
/// </summary>

using Raylib_cs;
using static Raylib_cs.Raylib;

namespace TextRPG.Drawing;

/// Elementos de UI reutilizables: paneles, barras, log, menÃº.
public static class UIElements
{
    private static Color C(int r, int g, int b, int a = 255) => new((byte)r, (byte)g, (byte)b, (byte)a);

    public static void DrawPanel(int x, int y, int w, int h, Color border)
    {
        DrawRectangle(x, y, w, h, C(18, 24, 42));
        DrawRectangle(x + 1, y + 1, w - 2, h - 2, C(14, 18, 35));
        DrawRectangleLines(x, y, w, h, border);
    }

    public static void DrawPlayerInfo(int x, int y, int w, string name, int lvl, string cls,
        float hp, int maxHp, float exp, int expN, int atk, int def, int gold)
    {
        DrawRectangle(x, y, w, 130, C(18, 24, 42));
        DrawRectangleLines(x, y, w, 130, C(60, 75, 110));
        DrawLine(x + 2, y + 18, x + w - 2, y + 18, C(160, 80, 220));
        DrawText("PERSONAJE", x + 8, y + 3, 12, C(160, 80, 220));

        int yy = y + 22;
        DrawText($"{name}  Nv.{lvl}  {cls}", x + 10, yy, 14, C(255, 200, 50));
        DrawHP(x + 10, yy + 22, w - 20, 14, hp, maxHp, C(60, 210, 70));
        DrawEXP(x + 10, yy + 40, w - 20, 6, exp, expN);
        DrawText($"ATK: {atk}  DEF: {def}  Oro: {gold}", x + 10, yy + 52, 12, C(225, 230, 240));
        DrawText($"EXP: {(int)exp}/{expN}", x + 10, yy + 68, 12, C(130, 140, 165));
    }

    public static void DrawLocationInfo(int x, int y, int w, string name, string desc)
    {
        DrawRectangle(x, y, w, 130, C(18, 24, 42));
        DrawRectangleLines(x, y, w, 130, C(60, 75, 110));
        DrawLine(x + 2, y + 18, x + w - 2, y + 18, C(255, 200, 50));
        DrawText(name, x + 8, y + 3, 12, C(255, 200, 50));
        DrawText(desc, x + 10, y + 22, 12, C(130, 140, 165));
    }

    public static void DrawHP(int x, int y, int w, int h, float cur, int max, Color good)
    {
        float pct = max > 0 ? cur / max : 0;
        DrawRectangle(x, y, w, h, C(25, 25, 38));
        var fill = pct > 0.5f ? good : pct > 0.25f ? C(255, 160, 40) : C(240, 70, 70);
        int fw = Math.Max(0, (int)((w - 2) * pct));
        if (fw > 0) DrawRectangle(x + 1, y + 1, fw, h - 2, fill);
        DrawRectangleLines(x, y, w, h, C(60, 75, 110));
        string lbl = $"HP {(int)cur}/{max}";
        DrawText(lbl, x + w / 2 - MeasureText(lbl, 12) / 2, y + 1, 12, C(225, 230, 240));
    }

    public static void DrawEXP(int x, int y, int w, int h, float cur, int max)
    {
        float pct = max > 0 ? cur / max : 0;
        DrawRectangle(x, y, w, h, C(25, 25, 38));
        int fw = Math.Max(0, (int)((w - 2) * pct));
        if (fw > 0) DrawRectangle(x + 1, y + 1, fw, h - 2, C(100, 80, 200));
        DrawRectangleLines(x, y, w, h, C(60, 75, 110));
    }

    public static void DrawLog(List<string> log, int x, int y, int w, int h)
    {
        DrawRectangle(x, y, w, h, C(18, 24, 42));
        DrawRectangle(x + 1, y + 1, w - 2, h - 2, C(14, 18, 35));
        DrawRectangleLines(x, y, w, h, C(60, 75, 110));

        int maxL = (h - 24) / 18;
        int start = Math.Max(0, log.Count - maxL);
        int ly = y + 6;
        for (int i = start; i < log.Count; i++)
        {
            DrawText(log[i], x + 6, ly, 12, C(225, 230, 240));
            ly += 18;
        }
    }

    public static void DrawMenu(string[] items, int x, int y, int w)
    {
        if (items.Length == 0) return;
        int h = Math.Min(items.Length * 20 + 12, 100);
        DrawRectangle(x, y, w, h, C(18, 24, 42));
        DrawRectangle(x + 1, y + 1, w - 2, h - 2, C(22, 28, 50));
        DrawRectangleLines(x, y, w, h, C(255, 200, 50));
        DrawLineEx(new System.Numerics.Vector2(x + 2, y + 2), new System.Numerics.Vector2(x + w - 2, y + 2), 1f, C(255, 200, 50));

        int my = y + 8;
        foreach (var item in items)
        { DrawText(item, x + 8, my, 12, C(225, 230, 240)); my += 20; }
    }

    public static void DrawTC(string text, int x, int y, int fs, Color c)
    { int tw = MeasureText(text, fs); DrawText(text, x - tw / 2, y, fs, c); }

    public static void DrawGradient(int x, int y, int w, int h, Color top, Color bot)
    { DrawRectangleGradientV(x, y, w, h, top, bot); }

    private static void DrawLine(int x1, int y1, int x2, int y2, Color c)
    { DrawLineEx(new System.Numerics.Vector2(x1, y1), new System.Numerics.Vector2(x2, y2), 1f, c); }
}

