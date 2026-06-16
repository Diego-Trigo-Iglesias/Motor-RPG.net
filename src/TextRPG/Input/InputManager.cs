/// <summary>
/// Gestión de entrada por frame. Cada Update() consulta el estado actual del teclado
/// y expone propiedades bool WasXxxPressedThisFrame para cada tecla relevante.
/// No almacena eventos — solo el estado del frame actual.
/// Convierte teclas físicas a conceptos de juego (WasPressedThisFrame = Enter o Space).
/// </summary>

using Raylib_cs;
using static Raylib_cs.Raylib;

namespace TextRPG.Input;

/// GestiÃ³n de entrada de teclado con detecciÃ³n por frame.
public sealed class InputManager
{
    public bool WasPressedThisFrame { get; private set; }
    public bool WasBPressedThisFrame { get; private set; }
    public bool WasFPressedThisFrame { get; private set; }
    public bool WasTPressedThisFrame { get; private set; }
    public bool WasSPressedThisFrame { get; private set; }
    public bool WasHPressedThisFrame { get; private set; }
    public bool WasIPressedThisFrame { get; private set; }
    private readonly bool[] _nums = new bool[9];

    public void Update()
    {
        WasPressedThisFrame = IsKeyPressed(KeyboardKey.Enter) || IsKeyPressed(KeyboardKey.Space);
        WasBPressedThisFrame = IsKeyPressed(KeyboardKey.B) || IsKeyPressed(KeyboardKey.Escape);
        WasFPressedThisFrame = IsKeyPressed(KeyboardKey.F);
        WasTPressedThisFrame = IsKeyPressed(KeyboardKey.T);
        WasSPressedThisFrame = IsKeyPressed(KeyboardKey.S);
        WasHPressedThisFrame = IsKeyPressed(KeyboardKey.H);
        WasIPressedThisFrame = IsKeyPressed(KeyboardKey.I);
        for (int i = 0; i < 9; i++)
            _nums[i] = IsKeyPressed((KeyboardKey)((int)KeyboardKey.One + i));
    }

    public bool WasNumberPressed(int n) => n >= 1 && n <= 9 && _nums[n - 1];
}

