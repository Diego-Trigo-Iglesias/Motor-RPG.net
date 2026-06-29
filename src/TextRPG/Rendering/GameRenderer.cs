using Raylib_cs;
using TextRPG.Core.Constants;
using TextRPG.Core.Enums;
using TextRPG.Core.Models;
using TextRPG.Core.Engine;
using TextRPG.Drawing;
using static Raylib_cs.Raylib;

namespace TextRPG.Rendering;

/// <summary>
/// Coordinador de renderizado. NO dibuja nada directamente. Delega en:
///   • Backgrounds  — fondos multi-capa por localización
///   • UIElements   — paneles, barras HP, log, menús
///   • Effects      — partículas, floating text, flash, shake
///   • SpriteLoader — carga de texturas desde PixelSprite
/// El estado del juego (player, enemy, location) se mantiene aquí como campos
/// y los sub-renderizers lo consumen por parámetro o desde sus campos compartidos.
/// </summary>
public sealed class GameRenderer : IDisposable
{
    private const int SW = GameConstants.ScreenWidth, SH = GameConstants.ScreenHeight, CX = SW / 2;
    private const int M = 12; // margen global
    private static Color C(int r, int g, int b, int a = 255) => new((byte)r, (byte)g, (byte)b, (byte)a);

    private Dictionary<string, Texture2D> _tex = null!;
    private readonly Effects _fx = new();
    private string _pClass = "warrior", _eKey = "goblin", _pName = "", _pCls = "";
    private int _pLv, _pHp, _pMxHp, _pAtk, _pDef, _pGold, _pExp, _pExpN;
    private float _dHP, _dEXP, _deHP;
    private string _eName = "", _locId = "", _locName = "", _locDesc = "";
    private int _eHp, _eMxHp, _eAtk, _eDef;
    private string[] _menu = [];
    private float _goA;
    private bool _hpInit, _enemyInit;
    public int CombatRound { get => _fx.CombatRound; set => _fx.CombatRound = value; }
    public void SetMenu(string[] i) => _menu = i;
    public void ClearMenu() => _menu = [];
    public void ClearMsgs() => _log.Clear();
    public void AddMsg(string m) => _log.Add(m);
    private readonly List<string> _log = new();
    public void Dispose() { if (_tex != null) SpriteLoader.UnloadAll(_tex); }

    public void Init(int sw, int sh) { if (_tex == null) _tex = SpriteLoader.LoadAll(); }

    public void SetPlayer(Player p)
    {
        if (!_hpInit) { _dHP = p.CurrentHp; _dEXP = p.Experience; _hpInit = true; }
        _pName = p.Name; _pCls = p.Class.ToString(); _pLv = p.Level;
        _pHp = p.CurrentHp; _pMxHp = p.MaxHp; _pAtk = p.TotalAttack; _pDef = p.TotalDefense;
        _pGold = p.Gold; _pExp = p.Experience; _pExpN = p.ExperienceToNextLevel;
        _pClass = p.Class switch { CharacterClass.Warrior => "warrior", CharacterClass.Mage => "mage", CharacterClass.Rogue => "rogue", _ => "warrior" };
    }

    public void SetEnemy(Enemy e)
    {
        if (!_enemyInit) { _deHP = e.CurrentHp; _enemyInit = true; }
        _eName = e.Name; _eHp = e.CurrentHp; _eMxHp = e.MaxHp; _eAtk = e.Attack; _eDef = e.Defense;
        _eKey = e.Name.Contains("Rata") ? "rat" : e.Name.Contains("Goblin") ? "goblin"
            : e.Name.Contains("Esqueleto") ? "skeleton" : e.Name.Contains("Orco") ? "orc" : "dragon";
    }

    public void ClearEnemy() { _eName = ""; _eHp = 0; _eMxHp = 0; _deHP = 0; _enemyInit = false; }
    public void SetLocation(string id, string n, string d) { _locId = id; _locName = n; _locDesc = d; }
    public void PlayTravel(string fromId, string toId)
    {
        _fx.PlayTravel(fromId, toId);
        _travelFromId = fromId;
        _travelToId = toId;
    }
    private string _travelFromId = "", _travelToId = "";
    public void ShowTitle() { ClearMsgs(); ClearMenu(); ClearEnemy(); }
    public void ShowCreate() { ClearMsgs(); ClearMenu(); ClearEnemy(); }
    public void ShowGame() { }
    public void ResetGameOver() { _goA = 0; }
    public void PlayAtk(CombatRound r) => _fx.PlayAttack(r);
    public void PlayVictory() => _fx.PlayVictory();
    public void UpdateState(Player p, Enemy? e) { SetPlayer(p); if (e != null) SetEnemy(e); }

    private void Tick(float dt)
    {
        float s = 9f;
        _dHP += (_pHp - _dHP) * Math.Clamp(dt * s, 0, 1);
        _deHP += (_eHp - _deHP) * Math.Clamp(dt * s, 0, 1);
        _dEXP += (_pExp - _dEXP) * Math.Clamp(dt * s, 0, 1);
        _fx.Update(dt);
    }

    //  DRAW 
    public void DrawTitle()
    {
        UIElements.DrawGradient(0, 0, SW, SH, C(8, 10, 22), C(18, 22, 40));

        // Panel decorativo central
        int pw = 520, ph = 360, px = CX - pw / 2, py = 80;
        UIElements.DrawPanel(px, py, pw, ph, C(60, 75, 110));
        DrawLineH(px + 20, py + 60, pw - 40, C(255, 200, 50));
        DrawLineH(px + 20, py + 300, pw - 40, C(60, 75, 110));

        DrawTC("TEXTRPG", CX, 110, 48, C(255, 200, 50));
        DrawTC("RPG de Texto Premium", CX, 165, 18, C(130, 140, 165));

        // Sprites de clases centrados
        int spriteW = 100, gap = 40;
        int totalW = 3 * spriteW + 2 * gap;
        int sx = CX - totalW / 2;
        foreach (var k in new[] { "warrior", "mage", "rogue" })
        {
            if (_tex.TryGetValue(k, out var t))
                DrawTexturePro(t, new(0, 0, t.Width, t.Height), new(sx, 200, spriteW, spriteW), default, 0, C(225, 230, 240));
            sx += spriteW + gap;
        }

        float pulse = 0.7f + MathF.Sin((float)GetTime() * 3f) * 0.3f;
        DrawTC("Pulsa [ENTER] para comenzar", CX, 420, 14, C(100, 150, 220, (byte)(255 * pulse)));
        _fx.Draw(SW, SH);
    }

    public void DrawCreate(string name)
    {
        UIElements.DrawGradient(0, 0, SW, SH, C(8, 10, 22), C(20, 16, 35));

        int pw = 560, ph = 480, px = CX - pw / 2, py = 40;
        UIElements.DrawPanelWithTitle(px, py, pw, ph, "CREA TU PERSONAJE", C(255, 200, 50), C(60, 75, 110));

        // Nombre
        DrawTL("NOMBRE DEL HEROE", px + 24, py + 34, 12, C(130, 140, 165));
        int nw = pw - 48;
        UIElements.DrawPanel(px + 24, py + 52, nw, 30, C(60, 75, 110));
        DrawTL(name + (name.Length < 16 ? "_" : ""), px + 34, py + 58, 16, C(225, 230, 240));

        // Sprites
        int spriteW = 96, gap = 36;
        int totalW = 3 * spriteW + 2 * gap;
        int sx = CX - totalW / 2;
        foreach (var k in new[] { "warrior", "mage", "rogue" })
        {
            if (_tex.TryGetValue(k, out var t))
                DrawTexturePro(t, new(0, 0, t.Width, t.Height), new(sx, py + 110, spriteW, spriteW), default, 0, C(225, 230, 240));
            sx += spriteW + gap;
        }

        // Clases
        int cy = py + 230;
        DrawTC("Selecciona una clase:", CX, cy, 14, C(130, 140, 165));
        DrawClassRow(px + 40, cy + 30, "[1] Guerrero", "Tanque y resistencia", C(200, 120, 80));
        DrawClassRow(px + 40, cy + 58, "[2] Mago", "Alto daño, frágil", C(100, 120, 220));
        DrawClassRow(px + 40, cy + 86, "[3] Pícaro", "Equilibrado, evasión", C(80, 180, 80));

        DrawTC("[ENTER] Confirmar", CX, py + ph - 30, 12, C(130, 140, 165));
    }

    public void DrawMain(Player p, Location loc)
    {
        Tick(GetFrameTime());
        Backgrounds.Draw(SW, SH, _locId);

        // Sprite de localización (esquina superior derecha)
        string locKey = "loc_" + _locId;
        if (_tex.TryGetValue(locKey, out var lt))
            DrawTexturePro(lt, new(0, 0, lt.Width, lt.Height), new(SW - 148, 20, 128, 64), default, 0, C(225, 230, 240));

        // Paneles superiores
        int topY = 20;
        int leftW = 380, rightX = SW - M - 540, rightW = 540;
        UIElements.DrawPlayerInfo(M, topY, leftW, _pName, _pLv, _pCls, _dHP, _pMxHp, _dEXP, _pExpN, _pAtk, _pDef, _pGold);
        UIElements.DrawLocationInfo(rightX, topY, rightW, _locName, _locDesc);

        // Log + Menú: altura dinámica para nunca solaparse ni salirse de pantalla
        int logY = 160;
        int menuH = _menu.Length > 0 ? Math.Min(_menu.Length * 22 + 28, 200) : 0;
        int logH = _menu.Length > 0 ? SH - logY - menuH - 16 : SH - logY - 16;
        UIElements.DrawLog(_log, M, logY, SW - 2 * M, logH);
        if (_menu.Length > 0)
            UIElements.DrawMenu(_menu, M, logY + logH + 8, SW - 2 * M);

        _fx.Draw(SW, SH);
    }

    public void DrawCombat(Player p, Enemy e)
    {
        Tick(GetFrameTime());
        float t = (float)GetTime();
        Backgrounds.Draw(SW, SH, _locId, combat: true);
        float sx = _fx.ShakeX, sy = _fx.ShakeY;
        float atkP = _fx.AtkAnim > 0 ? MathF.Sin(_fx.AtkAnim / 0.3f * MathF.PI) : 0;
        float aOff = atkP * 50f, hOff = _fx.HitAnim > 0 ? MathF.Sin(_fx.HitAnim / 0.5f * MathF.PI * 1.5f) * 20f : 0;

        // Barra superior: ronda
        UIElements.DrawPanel(M, 10, SW - 2 * M, 34, C(60, 75, 110));
        DrawTC("RONDA " + (_fx.CombatRound + 1), CX, 18, 16, C(255, 200, 50));

        int arenaTop = 54;
        int arenaH = 230;
        UIElements.DrawPanel(M, arenaTop, SW - 2 * M, arenaH, C(60, 75, 110));

        // Enemigo (derecha)
        int ex = SW - M - 180, ey = arenaTop + 20;
        int spriteSize = 120;
        if (_tex.TryGetValue(_eKey, out var et))
        {
            float hf = _fx.HitAnim > 0 ? MathF.Sin(_fx.HitAnim * 30) * 0.4f + 0.6f : 1f;
            DrawTexturePro(et, new(0, 0, et.Width, et.Height), new(ex - spriteSize / 2 + (int)(sx / 2 + hOff), ey + (int)(sy / 2), spriteSize, spriteSize), default, 0, C((byte)(255 * hf), (byte)(60 * hf), (byte)(60 * hf)));
        }
        DrawTR(_eName, ex + spriteSize / 2 - 10, ey + spriteSize + 6, 14, C(240, 70, 70));
        UIElements.DrawHP(ex - spriteSize / 2 - 10, ey + spriteSize + 26, spriteSize + 20, 14, _deHP, _eMxHp, C(240, 70, 70));

        // Efecto de ataque
        if (_fx.AtkAnim > 0.12f && _tex.TryGetValue(_fx.AtkIsCrit ? "crit" : "slash", out var ef))
        {
            float aa = Math.Clamp((_fx.AtkAnim - 0.12f) / 0.18f, 0, 1);
            DrawTexturePro(ef, new(0, 0, ef.Width, ef.Height), new(ex - 35 + (int)hOff, ey + 40, 80, 80), default, MathF.Sin(t * 30) * 25, C(255, 255, 255, (byte)(aa * 255)));
        }

        // Jugador (izquierda)
        int px = M + 180, py = arenaTop + 20;
        float breath = _fx.AtkAnim > 0 ? 0 : MathF.Sin(t * 2.5f) * 2f;
        if (_tex.TryGetValue(_pClass, out var pt))
            DrawTexturePro(pt, new(0, 0, pt.Width, pt.Height), new(px - spriteSize / 2 - (int)(sx / 2 - aOff), py + (int)breath, spriteSize, spriteSize), default, 0, C(225, 230, 240));
        float np = 1f + MathF.Sin(t * 2f) * 0.05f;
        DrawTL(_pName + " Nv." + _pLv, px - spriteSize / 2, py + spriteSize + 6, 14, C((byte)(255 * np), (byte)(200 * np), (byte)(50 * np)));
        UIElements.DrawHP(px - spriteSize / 2, py + spriteSize + 26, spriteSize + 20, 14, _dHP, _pMxHp, C(60, 210, 70));

        // VS
        DrawTC("VS", CX, arenaTop + arenaH / 2 - 10, 24, C(130, 140, 165));

        // Log y menú
        int logY = arenaTop + arenaH + 10;
        int logH = 150;
        UIElements.DrawLog(_log, M, logY, SW - 2 * M, logH);
        UIElements.DrawMenu(_menu, M, logY + logH + 8, SW - 2 * M);

        _fx.Draw(SW, SH);
    }

    public void DrawTravel()
    {
        UIElements.DrawGradient(0, 0, SW, SH, C(10, 12, 25), C(18, 22, 40));
        DrawHeader("VIAJAR", C(255, 200, 50));
        UIElements.DrawLog(_log, M, 60, SW - 2 * M, 300, "DESTINOS");
        UIElements.DrawMenu(_menu, M, 370, SW - 2 * M);
        DrawHelpBar("[B] Volver");
    }

    public void DrawTravelAnim(int sw, int sh)
    {
        float dt = GetFrameTime();
        _fx.Update(dt);
        _fx.DrawTravel(sw, sh);

        float p = _fx.TravelProgress;
        DrawGradientV(0, 0, sw, sh, C(8, 10, 22), C(16, 18, 36));

        string fromKey = "loc_" + _travelFromId;
        string toKey = "loc_" + _travelToId;

        float fromX, fromY, fromScale, fromAlpha;
        float toX, toY, toScale, toAlpha;

        if (p < 0.5f)
        {
            float phase = p / 0.5f;
            fromX = CX - 100 - phase * 80;
            fromY = SH / 2 - 40;
            fromScale = 1.0f - phase * 0.2f;
            fromAlpha = 1.0f - phase * 0.6f;
            toX = CX + 20 - (1 - phase) * 120;
            toY = SH / 2 - 40 + (1 - phase) * 30;
            toScale = 0.6f + phase * 0.4f;
            toAlpha = phase * 0.8f;
        }
        else
        {
            float phase = (p - 0.5f) / 0.5f;
            fromX = CX - 180 - phase * 60;
            fromY = SH / 2 - 40 + phase * 20;
            fromScale = 0.8f - phase * 0.6f;
            fromAlpha = 0.4f * (1 - phase);
            toX = CX - 60 + phase * 60;
            toY = SH / 2 - 40;
            toScale = 1.0f;
            toAlpha = 0.8f + phase * 0.2f;
        }

        if (_tex.TryGetValue(fromKey, out var fromTex) && fromAlpha > 0)
        {
            int fw = (int)(fromTex.Width * fromScale * 4);
            int fh = (int)(fromTex.Height * fromScale * 4);
            DrawTexturePro(fromTex, new(0, 0, fromTex.Width, fromTex.Height), new((int)fromX, (int)fromY, fw, fh), default, 0, C(225, 230, 240, (byte)(fromAlpha * 255)));
        }

        if (_tex.TryGetValue(toKey, out var toTex) && toAlpha > 0)
        {
            int tw = (int)(toTex.Width * toScale * 4);
            int th = (int)(toTex.Height * toScale * 4);
            DrawTexturePro(toTex, new(0, 0, toTex.Width, toTex.Height), new((int)toX, (int)toY, tw, th), default, 0, C(225, 230, 240, (byte)(toAlpha * 255)));
        }

        int lineY = SH / 2 + 40;
        int lineW = (int)(200 + MathF.Sin(p * MathF.PI) * 100);
        DrawRectangle(CX - lineW / 2, lineY, lineW, 2, C(255, 200, 50, (byte)(100 + MathF.Sin(p * MathF.PI) * 80)));

        float textAlpha = Math.Clamp((p - 0.3f) * 3, 0, 1);
        if (textAlpha > 0)
        {
            var toLoc = World.TryGet(_travelToId);
            if (toLoc != null)
            {
                int ty = lineY + 20;
                float glow = 0.8f + MathF.Sin((float)GetTime() * 4 + p * 2) * 0.2f;
                DrawTC("→ " + toLoc.Name + " ←", CX, ty, 22, C(255, 220, 50, (byte)(255 * textAlpha * glow)));
            }
        }

        _fx.Draw(sw, sh);
    }

    public void DrawShop()
    {
        UIElements.DrawGradient(0, 0, SW, SH, C(12, 10, 8), C(25, 20, 15));
        DrawHeader("TIENDA  |  Oro: " + _pGold, C(255, 200, 50));
        UIElements.DrawLog(_log, M, 60, SW - 2 * M, 300, "MERCANCIA");
        UIElements.DrawMenu(_menu, M, 370, SW - 2 * M, "COMPRAR");
        DrawHelpBar("[1-5] Comprar  [B] Salir");
    }

    public void DrawSave(string[] slots, string msg)
    {
        UIElements.DrawGradient(0, 0, SW, SH, C(8, 10, 22), C(18, 22, 40));
        DrawHeader("GUARDAR PARTIDA", C(255, 200, 50));
        if (!string.IsNullOrEmpty(msg))
        {
            UIElements.DrawPanelWithTitle(M, 55, SW - 2 * M, 34, "", C(80, 220, 80), C(80, 220, 80));
            DrawTC(msg, CX, 64, 14, C(80, 220, 80));
        }
        int logY = string.IsNullOrEmpty(msg) ? 55 : 95;
        UIElements.DrawLog(_log, M, logY, SW - 2 * M, 260, "SLOTS");
        UIElements.DrawMenu(_menu, M, logY + 270, SW - 2 * M);
        DrawHelpBar("[1-9] Seleccionar slot  [B] Volver");
    }

    public void DrawLoad(string[] slots, string msg)
    {
        UIElements.DrawGradient(0, 0, SW, SH, C(8, 10, 22), C(18, 22, 40));
        DrawHeader("CARGAR PARTIDA", C(255, 200, 50));
        if (!string.IsNullOrEmpty(msg))
        {
            UIElements.DrawPanelWithTitle(M, 55, SW - 2 * M, 34, "", C(255, 210, 60), C(255, 210, 60));
            DrawTC(msg, CX, 64, 14, C(255, 210, 60));
        }
        int logY = string.IsNullOrEmpty(msg) ? 55 : 95;
        UIElements.DrawLog(_log, M, logY, SW - 2 * M, 260, "SLOTS");
        UIElements.DrawMenu(_menu, M, logY + 270, SW - 2 * M);
        DrawHelpBar("[1-9] Cargar slot  [B] Volver");
    }

    public void DrawGameOver()
    {
        _goA += GetFrameTime() * 0.5f;
        float a = Math.Clamp(_goA, 0, 1);
        UIElements.DrawGradient(0, 0, SW, SH, C(0, 0, 0, (byte)(a * 255)), C(30, 5, 5, (byte)(a * 255)));

        int pw = 520, ph = 160, px = CX - pw / 2, py = SH / 2 - ph / 2 - 20;
        UIElements.DrawPanel(px, py, pw, ph, C(120, 40, 40, (byte)(a * 255)));
        DrawTC("HAS CAIDO EN COMBATE", CX, py + 40, 28, C(240, 50, 50, (byte)(a * 255)));
        if (a > 0.8f)
        {
            float p = 0.7f + MathF.Sin((float)GetTime() * 3f) * 0.3f;
            DrawTC("Pulsa [ENTER] para continuar", CX, py + 95, 14, C(130, 130, 150, (byte)(a * 255 * p)));
        }
        _fx.Draw(SW, SH);
    }

    // Helpers de dibujo
    private static void DrawTC(string t, int x, int y, int fs, Color c) { int tw = MeasureText(t, fs); DrawText(t, x - tw / 2, y, fs, c); }
    private static void DrawTL(string t, int x, int y, int fs, Color c) { DrawText(t, x, y, fs, c); }
    private static void DrawTR(string t, int x, int y, int fs, Color c) { int tw = MeasureText(t, fs); DrawText(t, x - tw, y, fs, c); }

    private static void DrawLineH(int x, int y, int w, Color c)
    {
        DrawLineEx(new System.Numerics.Vector2(x, y), new System.Numerics.Vector2(x + w, y), 1f, c);
    }

    private static void DrawGradientV(int x, int y, int w, int h, Color t, Color b)
    {
        if (h > 0 && w > 0)
            Raylib_cs.Raylib.DrawRectangleGradientV(x, y, w, h, t, b);
    }

    private static void DrawHeader(string title, Color color)
    {
        UIElements.DrawPanel(M, 10, SW - 2 * M, 40, C(60, 75, 110));
        DrawTC(title, CX, 20, 20, color);
    }

    private static void DrawHelpBar(string text)
    {
        int y = SH - 28;
        UIElements.DrawPanel(M, y, SW - 2 * M, 22, C(40, 50, 75));
        DrawTC(text, CX, y + 5, 11, C(130, 140, 165));
    }

    private static void DrawClassRow(int x, int y, string key, string desc, Color keyColor)
    {
        DrawText(key, x, y, 14, keyColor);
        int kw = MeasureText(key, 14);
        DrawText(" — " + desc, x + kw + 4, y, 12, C(130, 140, 165));
    }
}
