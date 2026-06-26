/// <summary>
/// Orquestador principal del juego. Bucle 60fps con state machine (Scr enum).
/// NO contiene lógica de juego - delega en:
///   - Menus    : construcción de menús de texto
///   - Actions  : ejecución de operaciones (combate, viaje, etc.)
///   - Renderer : dibujado de todo
/// El estado se pasa por referencia a las acciones que lo modifican.
/// </summary>

using Raylib_cs;
using TextRPG.Core;
using TextRPG.Core.Constants;
using TextRPG.Core.Enums;
using TextRPG.Core.Models;
using TextRPG.Core.Engine;
using TextRPG.Core.SaveSystem;
using TextRPG.Rendering;
using TextRPG.Input;
using TextRPG.Screens;
using static Raylib_cs.Raylib;

namespace TextRPG;

/// <summary>Orquestador principal. Bucle 60fps, state machine, delega en Menus y Actions.
public sealed class Game : IDisposable
{
    private const int SW = GameConstants.ScreenWidth, SH = GameConstants.ScreenHeight;

    private enum Scr
    {
        Title, CreateChar, Main, Combat, CombatAnim, CombatResult,
        Travel, TravelAnim, Shop, Inv, GameOver, Save, Load
    }

    private readonly GameRenderer _ren = new();
    private readonly InputManager _inp = new();
    private readonly CombatEngine _combat = new();
    private readonly SaveManager _saveManager;
    private GameState? _gs;
    private Player? _pl => _gs?.Player;
    private Location? _loc;
    private Enemy? _enemy;
    private Scr _scr = Scr.Title;
    private float _stimer;
    private bool _running = true;
    private string _pName = "";
    private string _saveMsg = "";
    private string[] _saveSlots = [];
    private string _pendingTravelDest = "";

    public Game()
    {
        var saveDir = System.IO.Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "TextRPG", "saves");
        _saveManager = new SaveManager(saveDir);
    }

    public void Run()
    {
        SetConfigFlags(ConfigFlags.Msaa4xHint | ConfigFlags.VSyncHint);
        InitWindow(SW, SH, "TextRPG - Terminal RPG");
        SetTargetFPS(60);
        _ren.Init(SW, SH);
        Menus.ShowTitle(_ren);
        _ren.ShowTitle();

        while (!WindowShouldClose() && _running)
        {
            _inp.Update();
            Update(GetFrameTime());
            BeginDrawing(); ClearBackground(new Color(12, 16, 30, 255)); Draw(); EndDrawing();
            if (IsKeyPressed(KeyboardKey.Escape)) _running = false;
        }
        CloseWindow();
    }

    private void Update(float dt)
    {
        _stimer += dt;
        switch (_scr)
        {
            case Scr.Title:
                if (_inp.WasPressedThisFrame) { _pName = ""; _scr = Scr.CreateChar; _ren.ShowCreate(); Menus.ShowCreate(_ren); }
                break;

            case Scr.CreateChar:
                int key = GetCharPressed();
                while (key > 0) { if (_pName.Length < GameConstants.MaxNameLength && key >= 32 && key <= 126) _pName += (char)key; key = GetCharPressed(); }
                if (IsKeyPressed(KeyboardKey.Backspace) && _pName.Length > 0) _pName = _pName[..^1];
                if (_inp.WasNumberPressed(1)) Crear(CharacterClass.Warrior);
                else if (_inp.WasNumberPressed(2)) Crear(CharacterClass.Mage);
                else if (_inp.WasNumberPressed(3)) Crear(CharacterClass.Rogue);
                break;

            case Scr.Main:
                if (_inp.WasNumberPressed(1) && (_loc?.HasEnemies ?? false)) IniciarCombate();
                else if (_inp.WasNumberPressed(2)) IniciarViaje();
                else if (_inp.WasNumberPressed(3) && (_loc?.HasShop ?? false)) IrTienda();
                else if (_inp.WasNumberPressed(4) && (_loc?.HasHealer ?? false)) Curar();
                else if (_inp.WasNumberPressed(5)) MostrarInventario();
                else if (_inp.WasSPressedThisFrame) { _scr = Scr.Save; _saveMsg = ""; _saveSlots = []; CargarSlotsGuardado(); Menus.ShowSave(_ren, _saveSlots, _saveMsg); }
                else if (_inp.WasLPressedThisFrame) { _scr = Scr.Load; _saveMsg = ""; _saveSlots = []; CargarSlotsGuardado(); Menus.ShowLoad(_ren, _saveSlots, _saveMsg); }
                break;

            case Scr.Combat:
                if (_inp.WasNumberPressed(1)) EjecutarAtaque();
                else if (_inp.WasNumberPressed(2)) { _ren.AddMsg("Huyes del combate."); _enemy = null; _ren.ClearEnemy(); IrMain(); }
                break;

            case Scr.CombatAnim:
                if (_stimer > GameConstants.AttackAnimDuration) { _scr = Scr.Combat; _stimer = 0; Menus.ShowCombat(_ren); }
                break;

            case Scr.CombatResult:
                if (_stimer > GameConstants.CombatResultDuration) { _enemy = null; _ren.ClearEnemy(); _stimer = 0; IrMain(); }
                break;

            case Scr.Travel:
                var dests = World.Locations.Where(l => l.Id != _gs?.CurrentLocationId).ToList();
                for (int i = 0; i < dests.Count; i++)
                    if (_inp.WasNumberPressed(i + 1))
                    {
                        string fromId = _gs!.CurrentLocationId;
                        _ren.PlayTravel(fromId, dests[i].Id);
                        _stimer = 0;
                        _scr = Scr.TravelAnim;
                        // Guardamos el destino para aplicarlo al terminar la animación
                        _pendingTravelDest = dests[i].Id;
                        return;
                    }
                if (_inp.WasBPressedThisFrame) IrMain();
                break;

            case Scr.TravelAnim:
                const float travelDuration = 1.8f;
                if (_stimer >= travelDuration)
                {
                    Actions.TravelTo(_ren, ref _gs!, ref _loc!, _pendingTravelDest);
                    IrMain();
                }
                break;

            case Scr.Shop:
                for (int i = 0; i < ShopCatalog.Items.Length; i++)
                    if (_inp.WasNumberPressed(i + 1)) { Comprar(i); return; }
                if (_inp.WasBPressedThisFrame) IrMain();
                break;

            case Scr.Inv:
                if (_inp.WasPressedThisFrame) IrMain();
                break;

            case Scr.Save:
                for (int i = 0; i < _saveSlots.Length; i++)
                    if (_inp.WasNumberPressed(i + 1))
                    { EjecutarGuardado(i); return; }
                if (_inp.WasBPressedThisFrame) IrMain();
                break;

            case Scr.Load:
                for (int i = 0; i < _saveSlots.Length; i++)
                    if (_inp.WasNumberPressed(i + 1))
                    { EjecutarCarga(i); return; }
                if (_inp.WasBPressedThisFrame) IrMain();
                break;

            case Scr.GameOver:
                if (_stimer > GameConstants.GameOverDuration && _inp.WasPressedThisFrame) { _gs = null; _scr = Scr.Title; Menus.ShowTitle(_ren); _ren.ShowTitle(); }
                break;
        }
    }

    private void Draw()
    {
        switch (_scr)
        {
            case Scr.Title: _ren.DrawTitle(); break;
            case Scr.CreateChar: _ren.DrawCreate(_pName); break;
            case Scr.Main: case Scr.Inv: if (_pl != null && _loc != null) _ren.DrawMain(_pl, _loc); break;
            case Scr.Combat:
            case Scr.CombatAnim:
            case Scr.CombatResult:
                if (_pl != null && _enemy != null) _ren.DrawCombat(_pl, _enemy); break;
            case Scr.Travel: _ren.DrawTravel(); break;
            case Scr.TravelAnim: _ren.DrawTravelAnim(SW, SH); break;
            case Scr.Shop: _ren.DrawShop(); break;
            case Scr.Save: _ren.DrawSave(_saveSlots, _saveMsg); break;
            case Scr.Load: _ren.DrawLoad(_saveSlots, _saveMsg); break;
            case Scr.GameOver: _ren.DrawGameOver(); break;
        }
    }

    //  Acciones
    private void Crear(CharacterClass cls) { Actions.CreatePlayer(_ren, ref _gs, ref _loc, _pName, cls); _scr = Scr.Main; }
    private void IniciarCombate() { _scr = Scr.Combat; _stimer = 0; Actions.StartCombat(_ren, _gs!, ref _enemy, _combat); }
    private void IniciarViaje() { _scr = Scr.Travel; Menus.ShowTravel(_ren, _gs); }
    private void IrTienda() { _scr = Scr.Shop; Menus.ShowShop(_ren, _pl?.Gold ?? 0); }

    private void EjecutarAtaque()
    {
        bool victoria = Actions.ExecuteAttack(_ren, _gs!, ref _enemy, _combat);
        if (victoria) { _scr = Scr.CombatResult; _stimer = 0; }
        else if (!_pl!.IsAlive) { _scr = Scr.GameOver; _stimer = 0; _ren.ResetGameOver(); }
        else { _scr = Scr.CombatAnim; _stimer = 0; }
    }

    private void Curar() { if (_pl == null) return; Actions.Heal(_ren, _pl); }
    private void Comprar(int idx) { if (_pl == null) return; Actions.BuyItem(_ren, _pl, idx); Menus.ShowShop(_ren, _pl.Gold); }

    private void MostrarInventario()
    {
        if (_pl != null) Menus.ShowInventory(_ren, _pl, _gs);
        _scr = Scr.Inv;
    }

    private void IrMain()
    {
        _scr = Scr.Main; _ren.ShowGame();
        Menus.ShowMain(_ren, _pl, _loc);
    }

    private void CargarSlotsGuardado()
    {
        var saves = _saveManager.ListSaves();
        _saveSlots = saves.Count > 0
            ? saves.Select((s, i) => $"[{i + 1}] {s.PlayerInfo}  ({s.LastSavedAt:yyyy-MM-dd HH:mm})").ToArray()
            : ["(No hay partidas guardadas)"];
    }

    private void EjecutarGuardado(int index)
    {
        if (_gs == null) return;
        var slotName = $"slot_{index + 1}";
        _saveManager.Save(_gs, slotName);
        _saveMsg = "Partida guardada correctamente.";
        CargarSlotsGuardado();
        Menus.ShowSave(_ren, _saveSlots, _saveMsg);
    }

    private void EjecutarCarga(int index)
    {
        var slotName = $"slot_{index + 1}";
        var loaded = _saveManager.Load(slotName);
        if (loaded == null)
        {
            _saveMsg = "No se pudo cargar la partida.";
            Menus.ShowLoad(_ren, _saveSlots, _saveMsg);
            return;
        }
        _gs = loaded;
        _loc = _gs.CurrentLocation;
        _ren.SetPlayer(_gs.Player);
        _ren.SetLocation(_gs.CurrentLocationId, _loc.Name, _loc.Description);
        _enemy = null;
        _ren.ClearEnemy();
        _ren.AddMsg("Partida cargada. Bienvenido de vuelta, " + _gs.Player.Name + "!");
        IrMain();
    }

    public void Dispose() => _ren.Dispose();
}

