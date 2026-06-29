/// <summary>
/// Componentes de UI reutilizables. TODOS son métodos estáticos sin estado.
/// Cada función dibuja un elemento concreto: panel, barra de HP, log, menú.
/// Se usa composición en lugar de herencia — GameRenderer llama a estos métodos
/// en el orden correcto para construir cada pantalla.
/// </summary>

using Raylib_cs;
using static Raylib_cs.Raylib;

namespace TextRPG.Drawing;

/// Elementos de UI reutilizables: paneles, barras, log, menú.
public static class UIElements
{
    private static Color C(int r, int g, int b, int a = 255) => new((byte)r, (byte)g, (byte)b, (byte)a);

    // Colores temáticos consistentes
    public static readonly Color PanelBg = new(18, 24, 42, 255);
    public static readonly Color PanelInner = new(14, 18, 35, 255);
    public static readonly Color PanelBorder = new(60, 75, 110, 255);
    public static readonly Color TitleGold = new(255, 200, 50, 255);
    public static readonly Color TextMain = new(225, 230, 240, 255);
    public static readonly Color TextMuted = new(130, 140, 165, 255);
    public static readonly Color HpGood = new(60, 210, 70, 255);
    public static readonly Color HpWarn = new(255, 160, 40, 255);
    public static readonly Color HpBad = new(240, 70, 70, 255);

    public static void DrawPanel(int x, int y, int w, int h, Color border)
    {
        DrawRectangle(x, y, w, h, PanelBg);
        DrawRectangle(x + 1, y + 1, w - 2, h - 2, PanelInner);
        DrawRectangleLines(x, y, w, h, border);
    }

    public static void DrawPanelWithTitle(int x, int y, int w, int h, string title, Color titleColor, Color border)
    {
        DrawPanel(x, y, w, h, border);
        // Línea decorativa bajo el título
        DrawRectangle(x + 2, y + 20, w - 4, 1, titleColor);
        DrawText(title, x + 8, y + 4, 12, titleColor);
    }

    public static void DrawPlayerInfo(int x, int y, int w, string name, int lvl, string cls,
        float hp, int maxHp, float exp, int expN, int atk, int def, int gold)
    {
        const int rowH = 16;
        int h = 128;
        DrawPanelWithTitle(x, y, w, h, "PERSONAJE", new Color(160, 80, 220, 255), PanelBorder);

        int yy = y + 26;
        DrawText($"{name}", x + 10, yy, 14, TitleGold);
        DrawText($"Nv.{lvl}  {cls}", x + w - 10 - MeasureText($"Nv.{lvl}  {cls}", 12), yy + 2, 12, TextMuted);

        yy += rowH + 2;
        DrawHP(x + 10, yy, w - 20, 14, hp, maxHp, HpGood);
        yy += 20;
        DrawEXP(x + 10, yy, w - 20, 6, exp, expN);

        yy += 14;
        DrawLabelValue(x + 10, yy, 70, "ATK:", atk.ToString(), TextMain);
        DrawLabelValue(x + 90, yy, 70, "DEF:", def.ToString(), TextMain);
        DrawLabelValue(x + 170, yy, w - 180, "Oro:", gold.ToString(), TitleGold);

        yy += rowH;
        DrawLabelValue(x + 10, yy, 120, "EXP:", $"{(int)exp}/{expN}", TextMuted);
    }

    public static void DrawLocationInfo(int x, int y, int w, string name, string desc)
    {
        DrawPanelWithTitle(x, y, w, 130, "UBICACION", TitleGold, PanelBorder);
        DrawText(name, x + 10, y + 26, 14, TitleGold);
        DrawWrappedText(desc, x + 10, y + 48, w - 20, 12, TextMuted, 4);
    }

    public static void DrawHP(int x, int y, int w, int h, float cur, int max, Color good)
    {
        float pct = max > 0 ? cur / max : 0;
        DrawRectangle(x, y, w, h, C(25, 25, 38));
        var fill = pct > 0.5f ? good : pct > 0.25f ? HpWarn : HpBad;
        int fw = Math.Max(0, (int)((w - 2) * pct));
        if (fw > 0) DrawRectangle(x + 1, y + 1, fw, h - 2, fill);
        DrawRectangleLines(x, y, w, h, PanelBorder);
        string lbl = $"HP {(int)cur}/{max}";
        int ty = y + (h - 12) / 2;
        DrawText(lbl, x + w / 2 - MeasureText(lbl, 12) / 2, ty, 12, TextMain);
    }

    public static void DrawEXP(int x, int y, int w, int h, float cur, int max)
    {
        float pct = max > 0 ? cur / max : 0;
        DrawRectangle(x, y, w, h, C(25, 25, 38));
        int fw = Math.Max(0, (int)((w - 2) * pct));
        if (fw > 0) DrawRectangle(x + 1, y + 1, fw, h - 2, new Color(100, 80, 200, 255));
        DrawRectangleLines(x, y, w, h, PanelBorder);
    }

    public static void DrawLog(List<string> log, int x, int y, int w, int h, string title = "REGISTRO")
    {
        DrawPanelWithTitle(x, y, w, h, title, TextMuted, PanelBorder);

        int maxL = (h - 30) / 18;
        int start = Math.Max(0, log.Count - maxL);
        int ly = y + 26;
        for (int i = start; i < log.Count; i++)
        {
            DrawText(log[i], x + 8, ly, 12, TextMain);
            ly += 18;
        }
    }

    public static void DrawMenu(string[] items, int x, int y, int w, string title = "MENU")
    {
        if (items.Length == 0) return;
        int h = Math.Min(items.Length * 22 + 28, 200);
        DrawPanelWithTitle(x, y, w, h, title, TitleGold, TitleGold);

        int my = y + 28;
        foreach (var item in items)
        {
            DrawText(item, x + 12, my, 12, TextMain);
            my += 22;
        }
    }

    public static void DrawLabelValue(int x, int y, int w, string label, string value, Color valueColor)
    {
        DrawText(label, x, y, 12, TextMuted);
        int lw = MeasureText(label, 12);
        DrawText(value, Math.Min(x + w - MeasureText(value, 12), x + lw + 4), y, 12, valueColor);
    }

    public static void DrawWrappedText(string text, int x, int y, int w, int fs, Color c, int maxLines)
    {
        if (string.IsNullOrWhiteSpace(text)) return;
        var words = text.Split(' ');
        var lines = new List<string>();
        var current = "";
        foreach (var word in words)
        {
            var test = current.Length == 0 ? word : current + " " + word;
            if (MeasureText(test, fs) <= w)
            {
                current = test;
            }
            else
            {
                if (current.Length > 0) lines.Add(current);
                current = word;
            }
        }
        if (current.Length > 0) lines.Add(current);

        int ly = y;
        for (int i = 0; i < Math.Min(lines.Count, maxLines); i++)
        {
            DrawText(lines[i], x, ly, fs, c);
            ly += fs + 2;
        }
    }

    public static void DrawTC(string text, int x, int y, int fs, Color c)
    { int tw = MeasureText(text, fs); DrawText(text, x - tw / 2, y, fs, c); }

    public static void DrawTL(string text, int x, int y, int fs, Color c)
    { DrawText(text, x, y, fs, c); }

    public static void DrawTR(string text, int x, int y, int fs, Color c)
    { int tw = MeasureText(text, fs); DrawText(text, x - tw, y, fs, c); }

    public static void DrawGradient(int x, int y, int w, int h, Color top, Color bot)
    { DrawRectangleGradientV(x, y, w, h, top, bot); }

    private static void DrawLine(int x1, int y1, int x2, int y2, Color c)
    { DrawLineEx(new System.Numerics.Vector2(x1, y1), new System.Numerics.Vector2(x2, y2), 1f, c); }
}
