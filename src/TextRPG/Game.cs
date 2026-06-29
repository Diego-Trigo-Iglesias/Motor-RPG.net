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
        Travel, TravelAnim, Shop, Inv, GameOver, Save, Load,
        // Nuevas pantallas
        DungeonEntrance, DungeonFloor, Victory
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
                HandleCreateCharInput();
                break;

            case Scr.Main:
                HandleMainInput();
                break;

            case Scr.Combat:
                HandleCombatInput();
                break;

            case Scr.CombatAnim:
                if (_stimer > GameConstants.AttackAnimDuration) { _scr = Scr.Combat; _stimer = 0; if (_enemy != null) Menus.ShowCombat(_ren, _enemy); }
                break;

            case Scr.CombatResult:
                if (_stimer > GameConstants.CombatResultDuration)
                {
                    _stimer = 0;

                    // Limpiar el enemigo ya sea que esté vivo o muerto
                    _enemy = null;
                    _ren.ClearEnemy();

                    if (_gs != null && _gs.IsInDungeon)
                    {
                        // Verificar si es victoria (jefe derrotado = piso final superado)
                        if (_gs.DungeonFloor >= GameConstants.DungeonFloorCount)
                        {
                            // El jefe fue derrotado → VICTORIA!
                            _scr = Scr.Victory;
                            Actions.WinGame(_ren, _gs);
                            Menus.ShowVictory(_ren, _gs);
                        }
                        else
                        {
                            // Piso superado, mostrar transición
                            _scr = Scr.DungeonFloor;
                            Menus.ShowDungeonFloorTransition(_ren, _gs);
                        }
                    }
                    else
                    {
                        IrMain();
                    }
                }
                break;

            case Scr.Travel:
                HandleTravelInput();
                break;

            case Scr.TravelAnim:
                const float travelDuration = 1.8f;
                if (_stimer >= travelDuration)
                {
                    Actions.TravelTo(_ren, ref _gs!, ref _loc!, _pendingTravelDest);
                    if (_loc?.IsDungeon == true)
                    {
                        // Mostrar pantalla de entrada a la mazmorra
                        _scr = Scr.DungeonEntrance;
                    }
                    else
                    {
                        IrMain();
                    }
                }
                break;

            case Scr.Shop:
                HandleShopInput();
                break;

            case Scr.Inv:
                HandleInventoryInput();
                break;

            case Scr.Save:
                HandleSaveInput();
                break;

            case Scr.Load:
                HandleLoadInput();
                break;

            case Scr.GameOver:
                HandleGameOverInput();
                break;

            //  Nuevas pantallas 

            case Scr.DungeonEntrance:
                HandleDungeonEntranceInput();
                break;

            case Scr.DungeonFloor:
                HandleDungeonFloorInput();
                break;

            case Scr.Victory:
                if (_stimer > 1.0f && _inp.WasPressedThisFrame)
                {
                    _gs = null;
                    _scr = Scr.Title;
                    _ren.ShowTitle();
                    Menus.ShowTitle(_ren);
                }
                break;
        }
    }

    // ══════════════════════════════════════════════════
    //   DRAW
    // ══════════════════════════════════════════════════

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
            // Nuevas
            case Scr.DungeonEntrance: _ren.DrawDungeonEntrance(); break;
            case Scr.DungeonFloor: if (_pl != null) _ren.DrawDungeonFloor(_pl, _gs); break;
            case Scr.Victory: _ren.DrawVictory(_gs); break;
        }
    }

    // ══════════════════════════════════════════════════
    //   MANEJADORES DE INPUT
    // ══════════════════════════════════════════════════

    private void HandleCreateCharInput()
    {
        int key = GetCharPressed();
        while (key > 0) { if (_pName.Length < GameConstants.MaxNameLength && key >= 32 && key <= 126) _pName += (char)key; key = GetCharPressed(); }
        if (IsKeyPressed(KeyboardKey.Backspace) && _pName.Length > 0) _pName = _pName[..^1];
        if (_inp.WasNumberPressed(1)) Crear(CharacterClass.Warrior);
        else if (_inp.WasNumberPressed(2)) Crear(CharacterClass.Mage);
        else if (_inp.WasNumberPressed(3)) Crear(CharacterClass.Rogue);
    }

    private void HandleMainInput()
    {
        if (_inp.WasNumberPressed(1) && (_loc?.HasEnemies ?? false) && !_gs!.IsInDungeon) IniciarCombate();
        else if (_inp.WasNumberPressed(2)) IniciarViaje();
        else if (_inp.WasNumberPressed(3) && (_loc?.HasShop ?? false)) IrTienda();
        else if (_inp.WasNumberPressed(4) && (_loc?.HasHealer ?? false)) Curar();
        else if (_inp.WasNumberPressed(5)) MostrarInventario();
        else if (_inp.WasNumberPressed(6) && (_pl != null && GameActions.HasUsablePotion(_pl))) UsarPocion();
        else if (_inp.WasSPressedThisFrame) { _scr = Scr.Save; _saveMsg = ""; _saveSlots = []; CargarSlotsGuardado(); Menus.ShowSave(_ren, _saveSlots, _saveMsg); }
        else if (_inp.WasLPressedThisFrame) { _scr = Scr.Load; _saveMsg = ""; _saveSlots = []; CargarSlotsGuardado(); Menus.ShowLoad(_ren, _saveSlots, _saveMsg); }
    }

    private void HandleCombatInput()
    {
        if (_inp.WasNumberPressed(1)) EjecutarAccion(PlayerAction.Attack);
        else if (_inp.WasNumberPressed(2)) EjecutarAccion(PlayerAction.HeavyAttack);
        else if (_inp.WasNumberPressed(3)) EjecutarAccion(PlayerAction.Defend);
        else if (_inp.WasNumberPressed(4) && _pl != null && GameActions.HasUsablePotion(_pl)) EjecutarAccion(PlayerAction.UsePotion);
        else if (_inp.WasNumberPressed(5)) EjecutarAccion(PlayerAction.Flee);
    }

    private void HandleTravelInput()
    {
        var dests = World.Locations.Where(l => l.Id != _gs?.CurrentLocationId).ToList();
        for (int i = 0; i < dests.Count; i++)
            if (_inp.WasNumberPressed(i + 1))
            {
                string fromId = _gs!.CurrentLocationId;
                _ren.PlayTravel(fromId, dests[i].Id);
                _stimer = 0;
                _scr = Scr.TravelAnim;
                _pendingTravelDest = dests[i].Id;
                return;
            }
        if (_inp.WasBPressedThisFrame) IrMain();
    }

    private void HandleShopInput()
    {
        for (int i = 0; i < ShopCatalog.Items.Length; i++)
            if (_inp.WasNumberPressed(i + 1)) { Comprar(i); return; }
        if (_inp.WasBPressedThisFrame) IrMain();
    }

    private void HandleInventoryInput()
    {
        if (_inp.WasUPressedThisFrame && (_pl != null && GameActions.HasUsablePotion(_pl))) { UsarPocion(); MostrarInventario(); }
        else if (_inp.WasPressedThisFrame)
        {
            if (_gs != null && _gs.IsInDungeon)
                IrDungeonFloor();
            else
                IrMain();
        }
    }

    private void HandleSaveInput()
    {
        for (int i = 0; i < _saveSlots.Length; i++)
            if (_inp.WasNumberPressed(i + 1))
            { EjecutarGuardado(i); return; }
        if (_inp.WasBPressedThisFrame) IrMain();
    }

    private void HandleLoadInput()
    {
        for (int i = 0; i < _saveSlots.Length; i++)
            if (_inp.WasNumberPressed(i + 1))
            { EjecutarCarga(i); return; }
        if (_inp.WasBPressedThisFrame) IrMain();
    }

    private void HandleGameOverInput()
    {
        if (_stimer > GameConstants.GameOverDuration && _inp.WasPressedThisFrame)
        {
            if (_gs != null && _gs.IsInDungeon)
            {
                // Morir en la mazmorra = perder progreso y volver a la aldea
                _gs.ResetDungeon();
                _gs.CurrentLocationId = "village";
                _loc = _gs.CurrentLocation;
                _ren.SetLocation(_gs.CurrentLocationId, _loc.Name, _loc.Description);
                _ren.AddMsg("Has muerto en la mazmorra... Tu progreso se ha perdido.");
                _ren.UpdateState(_gs.Player, null);
                IrMain();
            }
            else
            {
                _gs = null;
                _scr = Scr.Title;
                Menus.ShowTitle(_ren);
                _ren.ShowTitle();
            }
        }
    }

    // ══════════════════════════════════════════════════
    //   MANEJADORES DE MAZMORRA
    // ══════════════════════════════════════════════════

    private void HandleDungeonEntranceInput()
    {
        if (_inp.WasNumberPressed(1))
        {
            // Entrar a la mazmorra
            Actions.EnterDungeon(_ren, _gs!);
            _enemy = null;
            _ren.ClearEnemy();
            // Ir directamente al primer combate
            IniciarCombateDungeon();
        }
        else if (_inp.WasNumberPressed(2) || _inp.WasBPressedThisFrame)
        {
            IrMain();
        }
    }

    private void HandleDungeonFloorInput()
    {
        if (_inp.WasNumberPressed(1))
        {
            // Continuar al siguiente piso / enfrentar jefe
            if (_gs!.DungeonFloor >= GameConstants.DungeonFloorCount)
            {
                // Jefe final
                IniciarCombateDungeon();
            }
            else
            {
                Actions.AdvanceDungeonFloor(_ren, _gs);
                IniciarCombateDungeon();
            }
        }
        else if (_inp.WasNumberPressed(2) && _pl != null && GameActions.HasUsablePotion(_pl))
        {
            UsarPocion();
            Menus.ShowDungeonFloorTransition(_ren, _gs!);
        }
        else if (_inp.WasNumberPressed(5))
        {
            // Huir de la mazmorra
            _gs!.ResetDungeon();
            _gs.CurrentLocationId = "village";
            _loc = _gs.CurrentLocation;
            _ren.SetLocation("village", _loc.Name, _loc.Description);
            _ren.ClearMsgs();
            _ren.AddMsg("Escapas de la mazmorra, cubierto de vergüenza...");
            _ren.UpdateState(_gs.Player, null);
            IrMain();
        }
    }

    // ══════════════════════════════════════════════════
    //   ACCIONES
    // ══════════════════════════════════════════════════

    private void Crear(CharacterClass cls) { Actions.CreatePlayer(_ren, ref _gs, ref _loc, _pName, cls); _scr = Scr.Main; }

    private void IniciarCombate()
    {
        _scr = Scr.Combat;
        _stimer = 0;
        Actions.StartCombat(_ren, _gs!, ref _enemy, _combat);
    }

    private void IniciarCombateDungeon()
    {
        _scr = Scr.Combat;
        _stimer = 0;
        Actions.StartDungeonCombat(_ren, _gs!, ref _enemy, _combat);
    }

    private void IniciarViaje() { _scr = Scr.Travel; Menus.ShowTravel(_ren, _gs); }
    private void IrTienda() { _scr = Scr.Shop; Menus.ShowShop(_ren, _pl?.Gold ?? 0); }

    private void EjecutarAccion(PlayerAction action)
    {
        if (_pl == null || _enemy == null) return;

        if (action == PlayerAction.Flee)
        {
            bool victoria = Actions.ExecuteAction(_ren, _gs!, ref _enemy, _combat, action);
            _enemy = null;
            _ren.ClearEnemy();
            if (_gs!.IsInDungeon)
            {
                // Huir de combate en mazmorra = perder progreso
                _gs.ResetDungeon();
                _gs.CurrentLocationId = "village";
                _loc = _gs.CurrentLocation;
                _ren.SetLocation("village", _loc.Name, _loc.Description);
                _ren.AddMsg("Has huido de la mazmorra. Progreso perdido.");
                IrMain();
            }
            else
            {
                IrMain();
            }
            return;
        }

        if (action == PlayerAction.UsePotion)
        {
            Actions.ExecuteAction(_ren, _gs!, ref _enemy, _combat, action);
            _ren.ClearMenu();
            Menus.ShowCombat(_ren, _enemy);
            return;
        }

        bool result = Actions.ExecuteAction(_ren, _gs!, ref _enemy, _combat, action);
        if (result)
        {
            // Victoria — el enemigo tiene 0 HP pero el objeto sigue existiendo
            _scr = Scr.CombatResult;
            _stimer = 0;
        }
        else if (!_pl.IsAlive)
        {
            _scr = Scr.GameOver;
            _stimer = 0;
            _ren.ResetGameOver();
        }
        else
        {
            _scr = Scr.CombatAnim;
            _stimer = 0;
        }
    }

    private void Curar() { if (_pl == null) return; Actions.Heal(_ren, _pl); }
    private void Comprar(int idx) { if (_pl == null) return; Actions.BuyItem(_ren, _pl, idx); Menus.ShowShop(_ren, _pl.Gold); }
    private void UsarPocion() { if (_pl == null) return; Actions.UsePotion(_ren, _pl); }

    private void MostrarInventario()
    {
        if (_pl != null) Menus.ShowInventory(_ren, _pl, _gs);
        _scr = Scr.Inv;
    }

    private void IrMain()
    {
        _scr = Scr.Main;
        _ren.ShowGame();
        Menus.ShowMain(_ren, _pl, _loc);
    }

    private void IrDungeonFloor()
    {
        _scr = Scr.DungeonFloor;
        Menus.ShowDungeonFloorTransition(_ren, _gs!);
    }

    // ══════════════════════════════════════════════════
    //   GUARDADO / CARGA
    // ══════════════════════════════════════════════════

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
