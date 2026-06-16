/// <summary>
/// Sistema de efectos visuales con estado propio (partículas, textos flotantes).
/// CLASES (no structs) para permitir mutación directa en Update() — evitando el bug
/// clásico de copia por valor en List&lt;T&gt; con structs.
/// Cada efecto tiene su propio temporizador y se auto-gestiona:
/// las partículas expiradas se eliminan en el Update().
/// </summary>

using Raylib_cs;
using TextRPG.Core.Engine;
using static Raylib_cs.Raylib;

namespace TextRPG.Drawing;

/// <summary>Efectos visuales: partÃ­culas, floating text, flash, shake, animaciones de ataque.
public sealed class Effects
{
    private readonly List<Particle> _parts = new();
    private readonly List<FloatText> _floats = new();
    private float _shakeI, _shakeT, _flashA;
    private Color _flashCol;
    public float AtkAnim { get; private set; }
    public bool AtkIsCrit { get; private set; }
    public float HitAnim { get; private set; }
    public int CombatRound { get; set; }

    private const float AtkDur = 0.3f, HitDur = 0.5f;

    private static Color C(int r, int g, int b, int a = 255) => new((byte)r, (byte)g, (byte)b, (byte)a);

    public void PlayAttack(CombatRound r)
    {
        _shakeI = r.IsCritical ? 10f : 4f; _shakeT = r.IsCritical ? 0.35f : 0.18f;
        _flashA = r.IsCritical ? 0.55f : 0.22f;
        _flashCol = r.IsCritical ? C(255, 200, 50) : C(255, 255, 255);
        AtkAnim = AtkDur;
        HitAnim = HitDur;
        AtkIsCrit = r.IsCritical;
        CombatRound++;
        Spawn(540, 200, r.IsCritical ? C(255, 160, 40) : C(225, 230, 240), r.IsCritical ? 30 : 10);
        AddFloat(r.PlayerDamage.ToString(), r.IsCritical, 540, 170);
    }

    public void PlayVictory()
    {
        _flashA = 1.2f; _flashCol = C(255, 210, 50);
        _shakeI = 6f; _shakeT = 0.5f;
        Spawn(480, 200, C(255, 200, 50), 40);
        for (int i = 0; i < 6; i++)
            AddFloat("+", false, 480 + (i - 3) * 35, 150 - Math.Abs(i - 3) * 15);
    }

    public void Update(float dt)
    {
        _shakeT -= dt; if (_shakeT < 0) _shakeT = 0;
        _flashA -= dt * 3; if (_flashA < 0) _flashA = 0;
        AtkAnim -= dt; if (AtkAnim < 0) AtkAnim = 0;
        HitAnim -= dt; if (HitAnim < 0) HitAnim = 0;

        // Clases mutables: modificamos directamente los objetos de la lista
        for (int i = _parts.Count - 1; i >= 0; i--)
        {
            var p = _parts[i];
            p.Life -= dt;
            if (p.Life <= 0) { _parts.RemoveAt(i); continue; }
            p.X += p.Vx * dt; p.Y += p.Vy * dt; p.Vy += 400 * dt;
        }
        for (int i = _floats.Count - 1; i >= 0; i--)
        {
            var f = _floats[i];
            f.Life -= dt;
            if (f.Life <= 0) { _floats.RemoveAt(i); continue; }
            f.Y -= dt * 55;
        }
    }

    public void Draw(int sw, int sh)
    {
        if (_flashA > 0)
            DrawRectangle(0, 0, sw, sh, C(_flashCol.R, _flashCol.G, _flashCol.B, (byte)(_flashA * 180)));

        for (int i = 0; i < _parts.Count; i++)
        { var p = _parts[i]; float a = Math.Clamp(p.Life / p.MLife, 0, 1); DrawCircle((int)p.X, (int)p.Y, p.Sz * a, C(p.Col.R, p.Col.G, p.Col.B, (byte)(a * 200))); }

        foreach (var f in _floats)
        { float a = Math.Clamp(f.Life / f.MaxLife, 0, 1); int sz = f.Critical ? 24 : 18; var c = f.Critical ? C(255, 160, 40) : C(225, 230, 240); DrawTC(f.Text, (int)f.X, (int)f.Y, sz, C(c.R, c.G, c.B, (byte)(a * 255))); }
    }

    public float ShakeX => _shakeT > 0 ? MathF.Sin(_shakeT * 130) * _shakeI : 0;
    public float ShakeY => _shakeT > 0 ? MathF.Cos(_shakeT * 150) * _shakeI * 0.6f : 0;

    private void Spawn(float x, float y, Color c, int n)
    {
        var rng = new Random();
        for (int i = 0; i < n; i++)
            _parts.Add(new Particle
            {
                X = x,
                Y = y,
                Vx = (float)(rng.NextDouble() - 0.5) * 350,
                Vy = (float)(rng.NextDouble() - 0.5) * 350 - 180,
                Life = 0.3f + (float)rng.NextDouble() * 0.5f,
                MLife = 0.3f + (float)rng.NextDouble() * 0.5f,
                Col = c,
                Sz = 2 + rng.Next(4)
            });
    }

    private void AddFloat(string t, bool crit, float x, float y)
    { _floats.Add(new FloatText { Text = t, Critical = crit, X = x, Y = y, Life = crit ? 1.4f : 0.9f, MaxLife = crit ? 1.4f : 0.9f }); }

    private static void DrawTC(string t, int x, int y, int fs, Color c)
    { int tw = MeasureText(t, fs); DrawText(t, x - tw / 2, y, fs, c); }

    // CLASES (no structs) para mutaciÃ³n directa en Update()
    private class Particle { public float X, Y, Vx, Vy, Life, MLife; public Color Col; public int Sz; }
    private class FloatText { public string Text = ""; public bool Critical; public float X, Y, Life, MaxLife; }
}

