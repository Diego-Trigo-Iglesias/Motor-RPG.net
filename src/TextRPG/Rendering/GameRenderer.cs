using Raylib_cs;
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
/// y los sub-renderers lo consumen por parámetro o desde sus campos compartidos.
/// </summary>
public sealed class GameRenderer : IDisposable
{
    private const int SW = 960, SH = 600, CX = 480, SP = 160;
    private static Color C(int r, int g, int b, int a = 255) => new((byte)r, (byte)g, (byte)b, (byte)a);

    private Dictionary<string, Texture2D> _tex = null!;
    private readonly Effects _fx = new();
    private string _pClass = "warrior", _eKey = "goblin", _pName = "", _pCls = "";
    private int _pLv, _pHp, _pMxHp, _pAtk, _pDef, _pGold, _pExp, _pExpN;
    private float _dHP, _dEXP, _deHP;
    private string _eName = "", _locId = "", _locName = "", _locDesc = "";
    private int _eHp, _eMxHp, _eAtk, _eDef;
    private string[] _menu = [];
    private bool _showTitle, _showCreate;
    private float _goA;
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
        if (_pHp == 0) { _dHP = p.CurrentHp; _dEXP = p.Experience; }
        _pName = p.Name; _pCls = p.Class.ToString(); _pLv = p.Level;
        _pHp = p.CurrentHp; _pMxHp = p.MaxHp; _pAtk = p.Attack; _pDef = p.Defense;
        _pGold = p.Gold; _pExp = p.Experience; _pExpN = p.ExperienceToNextLevel;
        _pClass = p.Class switch { CharacterClass.Warrior => "warrior", CharacterClass.Mage => "mage", CharacterClass.Rogue => "rogue", _ => "warrior" };
    }

    public void SetEnemy(Enemy e)
    {
        if (_eHp == 0) _deHP = e.CurrentHp;
        _eName = e.Name; _eHp = e.CurrentHp; _eMxHp = e.MaxHp; _eAtk = e.Attack; _eDef = e.Defense;
        _eKey = e.Name.Contains("Rata") ? "rat" : e.Name.Contains("Goblin") ? "goblin"
            : e.Name.Contains("Esqueleto") ? "skeleton" : e.Name.Contains("Orco") ? "orc" : "dragon";
    }

    public void ClearEnemy() { _eName = ""; _eHp = 0; _eMxHp = 0; _deHP = 0; }
    public void SetLocation(string id, string n, string d) { _locId = id; _locName = n; _locDesc = d; }
    public void ShowTitle() { _showTitle = true; _showCreate = false; ClearMsgs(); ClearMenu(); ClearEnemy(); }
    public void ShowCreate() { _showTitle = false; _showCreate = true; ClearMsgs(); ClearMenu(); ClearEnemy(); }
    public void ShowGame() { _showTitle = false; _showCreate = false; }
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
        DrawTC("TEXTRPG", CX, 120, 40, C(255, 200, 50));
        DrawTC("RPG de Texto Premium", CX, 165, 18, C(130, 140, 165));
        int x = 200;
        foreach (var k in new[] { "warrior", "mage", "rogue" })
        { if (_tex.TryGetValue(k, out var t)) DrawTexturePro(t, new(0, 0, t.Width, t.Height), new(x, 210, 120, 120), default, 0, C(225, 230, 240)); x += 220; }
        DrawTC("Pulsa [ENTER] para comenzar", CX, 430, 14, C(60, 75, 110));
        _fx.Draw(SW, SH);
    }

    public void DrawCreate(string name)
    {
        UIElements.DrawGradient(0, 0, SW, SH, C(8, 10, 22), C(20, 16, 35));
        DrawTC("CREA TU PERSONAJE", CX, 25, 26, C(255, 200, 50));
        DrawTC("Nombre: " + name + (name.Length < 16 ? "_" : ""), CX, 60, 16, C(225, 230, 240));
        int px = 120;
        foreach (var k in new[] { "warrior", "mage", "rogue" })
        { if (_tex.TryGetValue(k, out var t)) { DrawTexturePro(t, new(0, 0, t.Width, t.Height), new(px - 64, 85, 128, 128), default, 0, C(225, 230, 240)); px += 360; } }
        DrawTC("[1] Guerrero  Tanque y resistencia", CX, 240, 14, C(200, 120, 80));
        DrawTC("[2] Mago  Alto dano, fragil", CX, 266, 14, C(100, 120, 220));
        DrawTC("[3] Picaro  Equilibrado, evasion", CX, 292, 14, C(80, 180, 80));
    }

    public void DrawMain(Player p, Location loc)
    {
        Tick(GetFrameTime());
        Backgrounds.Draw(SW, SH, _locId);
        UIElements.DrawPlayerInfo(15, 15, 390, _pName, _pLv, _pCls, _dHP, _pMxHp, _dEXP, _pExpN, _pAtk, _pDef, _pGold);
        UIElements.DrawLocationInfo(420, 15, 525, _locName, _locDesc);
        UIElements.DrawLog(_log, 12, 155, 936, _menu.Length > 0 ? 255 : 330);
        UIElements.DrawMenu(_menu, 12, 500, 936);
        _fx.Draw(SW, SH);
    }

    public void DrawCombat(Player p, Enemy e)
    {
        Tick(GetFrameTime());
        float t = (float)GetTime();
        Backgrounds.Draw(SW, SH, _locId, combat: true);
        float sx = _fx.ShakeX, sy = _fx.ShakeY;
        float atkP = _fx.AtkAnim > 0 ? MathF.Sin(_fx.AtkAnim / 0.3f * MathF.PI) : 0;
        float aOff = atkP * 40f, hOff = _fx.HitAnim > 0 ? MathF.Sin(_fx.HitAnim / 0.5f * MathF.PI * 1.5f) * 20f : 0;

        int ex = 570, ey = 40;
        if (_tex.TryGetValue(_eKey, out var et))
        {
            float hf = _fx.HitAnim > 0 ? MathF.Sin(_fx.HitAnim * 30) * 0.4f + 0.6f : 1f;
            DrawTexturePro(et, new(0, 0, et.Width, et.Height), new(ex - 80 + (int)(sx / 2 + hOff), ey + (int)(sy / 2), SP, SP), default, 0, C((byte)(255 * hf), (byte)(60 * hf), (byte)(60 * hf)));
        }
        DrawTC(_eName, ex + (int)(sx / 2), ey + SP + 6 + (int)(sy / 2), 14, C(240, 70, 70));
        UIElements.DrawHP(ex - 100 + (int)(sx / 2), ey + SP + 28 + (int)(sy / 2), 200, 14, _deHP, _eMxHp, C(240, 70, 70));

        if (_fx.AtkAnim > 0.12f && _tex.TryGetValue(_fx.AtkIsCrit ? "crit" : "slash", out var ef))
        {
            float aa = Math.Clamp((_fx.AtkAnim - 0.12f) / 0.18f, 0, 1);
            DrawTexturePro(ef, new(0, 0, ef.Width, ef.Height), new(ex - 45 + (int)hOff, ey + 80 - 45, 90, 90), default, MathF.Sin(t * 30) * 25, C(255, 255, 255, (byte)(aa * 255)));
        }

        int px = 100, py = 120;
        float breath = _fx.AtkAnim > 0 ? 0 : MathF.Sin(t * 2.5f) * 2f;
        if (_tex.TryGetValue(_pClass, out var pt))
            DrawTexturePro(pt, new(0, 0, pt.Width, pt.Height), new(px - 80 - (int)(sx / 2 - aOff), py + (int)(breath - sy / 2), SP, SP), default, 0, C(225, 230, 240));
        float np = 1f + MathF.Sin(t * 2f) * 0.05f;
        DrawTC(_pName + " Nv." + _pLv + " " + _pCls, px - (int)(sx / 2), py + SP + 6 - (int)(sy / 2), 14, C((byte)(255 * np), (byte)(200 * np), (byte)(50 * np)));
        UIElements.DrawHP(px - 100 - (int)(sx / 2), py + SP + 28 - (int)(sy / 2), 200, 14, _dHP, _pMxHp, C(60, 210, 70));
        DrawTC("Ronda " + _fx.CombatRound, CX, 195 + (int)sy, 12, C(130, 140, 165));
        UIElements.DrawLog(_log, 12, 290, 936, 160);
        UIElements.DrawMenu(_menu, 12, 510, 936);
        _fx.Draw(SW, SH);
    }

    public void DrawTravel() { UIElements.DrawGradient(0, 0, SW, SH, C(10, 12, 25), C(18, 22, 40)); DrawTC("VIAJAR", CX, 20, 26, C(255, 200, 50)); UIElements.DrawLog(_log, 12, 55, 936, 390); UIElements.DrawMenu(_menu, 12, 500, 936); }
    public void DrawShop() { UIElements.DrawGradient(0, 0, SW, SH, C(12, 10, 8), C(25, 20, 15)); DrawTC("TIENDA  Oro: " + _pGold, CX, 20, 18, C(255, 200, 50)); UIElements.DrawLog(_log, 12, 50, 936, 395); UIElements.DrawMenu(_menu, 12, 500, 936); }

    public void DrawGameOver()
    {
        _goA += GetFrameTime() * 0.5f;
        float a = Math.Clamp(_goA, 0, 1);
        UIElements.DrawGradient(0, 0, SW, SH, C(0, 0, 0, (byte)(a * 255)), C(30, 5, 5, (byte)(a * 255)));
        DrawTC("HAS CAIDO EN COMBATE", CX, 230, 26, C(240, 50, 50, (byte)(a * 255)));
        if (a > 0.8f) { float p = 0.7f + MathF.Sin((float)GetTime() * 3f) * 0.3f; DrawTC("Pulsa [ENTER] para continuar", CX, 290, 14, C(130, 130, 150, (byte)(a * 255 * p))); }
        _fx.Draw(SW, SH);
    }

    private static void DrawTC(string t, int x, int y, int fs, Color c) { int tw = MeasureText(t, fs); DrawText(t, x - tw / 2, y, fs, c); }
}
