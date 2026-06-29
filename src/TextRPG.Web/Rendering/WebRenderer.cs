using TextRPG.Core;
using TextRPG.Core.Constants;
using TextRPG.Core.Enums;
using TextRPG.Core.Models;
using TextRPG.Core.Engine;
using TextRPG.Core.Rendering;
using TextRPG.Core.PixelArt;

namespace TextRPG.Web.Rendering;

/// 
/// WebRenderer para Blazor WASM.
/// Captura el estado de renderizado para que el componente Blazor lo muestre.
/// Los métodos de entrada son asíncronos y se resuelven mediante la UI.
/// 
public sealed class WebRenderer
{
    // Estado de la UI
    public string Title { get; private set; } = "";
    public string StatusLine { get; private set; } = "";
    public string LocationName { get; private set; } = "";
    public string LocationDesc { get; private set; } = "";
    public string LocationId { get; private set; } = "";
    public string EnemyName { get; private set; } = "";
    public CombatRound? LastRound { get; set; }
    public int LastRoundNum { get; set; }

    // Nuevos campos para behaviors y mazmorra
    public string EnemyBehaviorLabel { get; private set; } = "";
    public string EnemyBehaviorIcon { get; private set; } = "";
    public int DungeonFloor { get; set; } = 0;
    public int DungeonMaxFloor { get; set; } = 5;
    public bool HasPoison { get; set; }

    public SpriteData? PlayerSprite { get; set; }
    public SpriteData? EnemySprite { get; set; }
    public SpriteData? LocationSprite { get; set; }
    public SpriteData? EffectSprite { get; set; }

    public HpBarData? PlayerHp { get; set; }
    public HpBarData? EnemyHp { get; set; }

    public List<string> Messages { get; } = new();
    public Screen CurrentScreen { get; private set; } = Screen.Title;

    // Input pendiente
    private TaskCompletionSource<string>? _inputTcs;
    private string? _inputPrompt;
    private string? _inputDefault;
    private TaskCompletionSource<bool>? _confirmTcs;
    private string? _confirmQuestion;
    private TaskCompletionSource<string>? _menuTcs;
    private Dictionary<string, string>? _menuOptions;
    private string? _menuTitle;

    public enum Screen
    {
        Title,
        CreateCharacter,
        MainMenu,
        Combat,
        Victory,
        Defeat,
        Shop,
        Heal,
        Inventory,
        Travel,
        Save,
        Paused,
        // Nuevas
        DungeonEntrance,
        DungeonFloor
    }

    // Métodos de renderizado

    public void Clear()
    {
        Messages.Clear();
        PlayerSprite = null;
        EnemySprite = null;
        LocationSprite = null;
        EffectSprite = null;
    }

    public void PrintTitle()
    {
        CurrentScreen = Screen.Title;
        Title = "TextRPG";
        Messages.Add("RPG de Texto Premium");
        Messages.Add("Creado con .NET 8 + Blazor WASM.");
    }

    public void PrintStatus(Player p)
    {
        PlayerSprite = SpriteData.FromPixelSprite(PlayerSprites.ForClass(p.Class));
        PlayerHp = new HpBarData { Current = p.CurrentHp, Max = p.MaxHp, Label = "HP" };

        string poison = p.PoisonTurnsRemaining > 0 ? $" ☠ Veneno ({p.PoisonTurnsRemaining}t)" : "";
        StatusLine = $"{p.Name}  ({p.Class} Lv.{p.Level})  " +
                     $"Oro: {p.Gold}  ATK: {p.TotalAttack}  DEF: {p.TotalDefense}{poison}";
    }

    public void PrintLocation(Location loc)
    {
        LocationName = loc.Name;
        LocationDesc = loc.Description;
        LocationId = loc.Id;
        LocationSprite = SpriteData.FromPixelSprite(LocationSprites.ForId(loc.Id));
    }

    public void PrintCombatHeader(Player player, Enemy enemy)
    {
        CurrentScreen = Screen.Combat;
        EnemyName = enemy.Name;
        PlayerSprite = SpriteData.FromPixelSprite(PlayerSprites.ForClass(player.Class));
        EnemySprite = SpriteData.FromPixelSprite(EnemySprites.ForName(enemy.Name));
        PlayerHp = new HpBarData { Current = player.CurrentHp, Max = player.MaxHp, Label = $"{player.Name} HP" };
        EnemyHp = new HpBarData { Current = enemy.CurrentHp, Max = enemy.MaxHp, Label = $"{enemy.Name} HP" };

        // Información de comportamiento
        EnemyBehaviorLabel = GetBehaviorLabel(enemy);
        EnemyBehaviorIcon = GetBehaviorIcon(enemy);
        HasPoison = player.PoisonTurnsRemaining > 0;

        Messages.Add($"¡Encuentras a {enemy.Name}!");
        if (enemy.Behavior != EnemyBehavior.Normal)
            Messages.Add($"⚠ {GetBehaviorDescription(enemy.Behavior)} ⚠");
    }

    public void PrintCombatRound(Player player, Enemy enemy, CombatRound round, int roundNum)
    {
        LastRound = round;
        LastRoundNum = roundNum;
        PlayerHp = new HpBarData { Current = player.CurrentHp, Max = player.MaxHp, Label = $"{player.Name} HP" };
        EnemyHp = new HpBarData { Current = Math.Max(0, enemy.CurrentHp), Max = enemy.MaxHp, Label = $"{enemy.Name} HP" };
        HasPoison = player.PoisonTurnsRemaining > 0;

        // Mensaje de comportamiento
        if (!string.IsNullOrEmpty(round.BehaviorMessage))
            Messages.Add(round.BehaviorMessage);

        // Veneno
        if (round.PoisonDamage > 0)
            Messages.Add($"☠ Veneno: -{round.PoisonDamage} HP ({player.PoisonTurnsRemaining} turnos restantes)");

        // Ataque del jugador
        string actionName = round.WasHeavyAttack ? "Ataque Fuerte" : round.PlayerDefended ? "Defensa" : "Ataque";
        if (round.EnemyDodged)
            Messages.Add($"{enemy.Name} esquivó tu {actionName}.");
        else if (round.ShieldActive)
            Messages.Add($"{actionName} golpea el escudo! Daño reducido: {round.PlayerDamage}");
        else if (round.IsCritical)
            Messages.Add($"¡GOLPE CRÍTICO! {round.PlayerDamage} de daño.");
        else
            Messages.Add($"{actionName}: {round.PlayerDamage} de daño a {enemy.Name}.");

        // Contraataque enemigo
        if (enemy.IsAlive && !round.PlayerDodged && round.EnemyDamage > 0)
        {
            string defSuffix = round.PlayerDefended ? " (mitigado por defensa)" : "";
            Messages.Add($"{enemy.Name} te golpea por {round.EnemyDamage}{defSuffix}.");
        }
        else if (enemy.IsAlive && round.PlayerDodged)
        {
            Messages.Add($"Esquivas el ataque de {enemy.Name}!");
        }

        // Enfurecimiento
        if (round.EnemyIsEnraged)
            Messages.Add($"¡{enemy.Name} está ENFURECIDO!");
    }

    public void PrintVictory(Enemy enemy, int exp, int gold)
    {
        CurrentScreen = Screen.Victory;
        EffectSprite = SpriteData.FromPixelSprite(EffectSprites.Victory);
        Messages.Add($"¡Venciste a {enemy.Name}! +{exp} EXP  +{gold} Oro");
    }

    public void PrintDefeat()
    {
        CurrentScreen = Screen.Defeat;
        Messages.Add("Has sido derrotado...");
    }

    public void PrintLevelUp(Player p)
    {
        Messages.Add($"¡SUBIDA DE NIVEL! Nivel {p.Level} — HP: {p.MaxHp} ATK: {p.Attack} DEF: {p.Defense}");
        EffectSprite = SpriteData.FromPixelSprite(PlayerSprites.ForClass(p.Class));
    }

    public void MarkupLine(string text) => Messages.Add(text);

    public void WritePlayerStats(Player player, GameState state)
    {
        PlayerSprite = SpriteData.FromPixelSprite(PlayerSprites.ForClass(player.Class));
        Messages.Add($"=== {player.Name} ===");
        Messages.Add($"Clase: {player.Class} | Nivel: {player.Level}");
        Messages.Add($"HP: {player.CurrentHp}/{player.MaxHp} | ATK: {player.TotalAttack} | DEF: {player.TotalDefense}");
        Messages.Add($"Oro: {player.Gold} | EXP: {player.Experience}/{player.ExperienceToNextLevel}");
        Messages.Add($"Batallas: {state.TotalBattles} | Victorias: {state.TotalVictories}");
        if (state.HasWon)
            Messages.Add("★ ¡HAS COMPLETADO EL JUEGO! ★");
        if (state.IsInDungeon)
            Messages.Add($"⚠ EN MAZMORRA — Piso {state.DungeonFloor} ⚠");

        var eq = player.GetEquippedItems();
        if (eq.Count > 0)
        {
            Messages.Add("[EQUIPADO]");
            foreach (var item in eq)
                Messages.Add($"  * {item.ToDetailString()}");
        }
        if (player.Inventory.Count > 0)
        {
            Messages.Add("Objetos:");
            foreach (var item in player.Inventory)
                Messages.Add($"  - {item.ToDetailString()}");
        }
    }

    // Animaciones

    public void PlayTravelAnimation(string fromId, string toId)
    {
        LocationSprite = SpriteData.FromPixelSprite(LocationSprites.ForId(toId));
        Messages.Add($"Viajando a {World.Get(toId).Name}...");
    }

    public void PlayHealAnimation()
    {
        EffectSprite = SpriteData.FromPixelSprite(EffectSprites.Heal);
        Messages.Add("✦ HP restaurado!");
    }

    public void PlayExploreAnimation(string locationId)
    {
        LocationSprite = SpriteData.FromPixelSprite(LocationSprites.ForId(locationId));
        Messages.Add("Te adentras en la oscuridad...");
    }

    public void PlayClassSelectionAnimation(CharacterClass cls)
    {
        PlayerSprite = SpriteData.FromPixelSprite(PlayerSprites.ForClass(cls));
        Messages.Add("¡Has elegido a tu héroe!");
    }

    // Input asíncrono

    public async Task<T> SelectMenuAsync<T>(string title, Dictionary<T, string> options) where T : notnull
    {
        _menuTcs = new TaskCompletionSource<string>();
        _menuTitle = title;
        _menuOptions = options.ToDictionary(kv => kv.Key.ToString()!, kv => kv.Value);
        CurrentScreen = Screen.Paused;
        var result = await _menuTcs.Task;
        _menuOptions = null;
        _menuTitle = null;
        CurrentScreen = Screen.MainMenu;
        return options.First(kv => kv.Key.ToString() == result).Key;
    }

    public void SelectMenuComplete(string selectedKey)
    {
        _menuTcs?.TrySetResult(selectedKey);
    }

    public async Task<string> PromptAsync(string question, string defaultValue)
    {
        _inputTcs = new TaskCompletionSource<string>();
        _inputPrompt = question;
        _inputDefault = defaultValue;
        CurrentScreen = Screen.Paused;
        var result = await _inputTcs.Task;
        _inputPrompt = null;
        return result;
    }

    public void PromptComplete(string value)
    {
        _inputTcs?.TrySetResult(value);
    }

    public async Task<bool> ConfirmAsync(string question)
    {
        _confirmTcs = new TaskCompletionSource<bool>();
        _confirmQuestion = question;
        CurrentScreen = Screen.Paused;
        var result = await _confirmTcs.Task;
        _confirmQuestion = null;
        return result;
    }

    public void ConfirmComplete(bool value)
    {
        _confirmTcs?.TrySetResult(value);
    }

    public async Task WaitAsync(int milliseconds)
    {
        await Task.Delay(milliseconds);
    }

    public void Pause(string? message = null)
    {
        Messages.Add(message ?? "---");
    }

    // UI State

    public void SetScreen(Screen screen) => CurrentScreen = screen;

    public bool HasMenu => _menuOptions != null;
    public string? MenuTitleText => _menuTitle;
    public Dictionary<string, string>? MenuOptions => _menuOptions;
    public bool HasInput => _inputTcs != null;
    public string? InputPrompt => _inputPrompt;
    public string? InputDefault => _inputDefault;
    public bool HasConfirm => _confirmTcs != null;
    public string? ConfirmQuestion => _confirmQuestion;
    public bool IsWaitingForInput => _menuTcs != null || _inputTcs != null || _confirmTcs != null;

    public object GetRenderState(Player? player, Enemy? enemy, GameState? gameState)
    {
        var screen = CurrentScreen switch
        {
            Screen.Title => "Title",
            Screen.CreateCharacter => "CreateCharacter",
            Screen.MainMenu => "MainMenu",
            Screen.Combat => "Combat",
            Screen.Victory => "Victory",
            Screen.Defeat => "Defeat",
            Screen.Shop => "Shop",
            Screen.Travel => "Travel",
            Screen.Inventory => "Inventory",
            Screen.DungeonEntrance => "DungeonEntrance",
            Screen.DungeonFloor => "DungeonFloor",
            _ => "MainMenu"
        };

        var menu = CurrentScreen switch
        {
            Screen.MainMenu => GetMainMenu(gameState?.CurrentLocation, player, gameState),
            Screen.Combat => GetCombatMenu(player),
            Screen.Victory => new[] { "[Enter] Continuar" },
            Screen.Defeat => new[] { "[Enter] Volver al inicio" },
            Screen.Title => new[] { "[Enter] Iniciar Aventura" },
            Screen.CreateCharacter => new[] { "[1] Guerrero", "[2] Mago", "[3] Pícaro" },
            Screen.Travel => GetTravelMenu(gameState),
            Screen.Shop => GetShopMenu(player),
            Screen.Inventory => new[] { "[Enter] Volver" },
            Screen.DungeonEntrance => new[] { "[1] Entrar", "[2] Volver" },
            Screen.DungeonFloor => GetDungeonFloorMenu(player),
            _ => Array.Empty<string>()
        };

        return new
        {
            screen,
            location = LocationId,
            playerSprite = PlayerSprite,
            enemySprite = screen == "Combat" || screen == "Victory" ? EnemySprite : null,
            effectSprite = EffectSprite,
            locationSprite = LocationSprite,
            hpBar = player != null ? new { current = player.CurrentHp, max = player.MaxHp } : null,
            playerHpBar = PlayerHp,
            enemyHpBar = EnemyHp,
            playerName = player?.Name ?? "",
            playerClass = player?.Class.ToString() ?? "",
            playerLevel = player?.Level ?? 1,
            playerAtk = player?.TotalAttack ?? 0,
            playerDef = player?.TotalDefense ?? 0,
            playerGold = player?.Gold ?? 0,
            enemyName = EnemyName,
            locationName = LocationName,
            locationDesc = LocationDesc,
            messages = Messages,
            menu = menu,
            combatRound = LastRoundNum,
            animations = new { isCritical = LastRound?.IsCritical ?? false },
            // Nuevos campos
            enemyBehavior = EnemyBehaviorLabel,
            enemyBehaviorIcon = EnemyBehaviorIcon,
            hasPoison = HasPoison,
            dungeonFloor = DungeonFloor,
            dungeonMaxFloor = DungeonMaxFloor,
            hasWon = gameState?.HasWon ?? false,
            isInDungeon = gameState?.IsInDungeon ?? false
        };
    }

    private string[] GetMainMenu(Location? loc, Player? player, GameState? gs)
    {
        var items = new List<string>();
        if (loc?.HasEnemies ?? false) items.Add("[1] Explorar");
        items.Add("[2] Viajar");
        if (loc?.HasShop ?? false) items.Add("[3] Tienda");
        if (loc?.HasHealer ?? false) items.Add($"[4] Curandero ({GameConstants.HealCost} oro)");
        items.Add("[5] Inventario");
        if (player != null && GameActions.HasUsablePotion(player))
            items.Add($"[6] Usar poción ({GameActions.PotionCount(player)} en inventario)");
        return items.ToArray();
    }

    private string[] GetCombatMenu(Player? player)
    {
        var items = new List<string>
        {
            "[1] Atacar",
            "[2] Ataque fuerte",
            "[3] Defender"
        };
        if (player != null && GameActions.HasUsablePotion(player))
            items.Add("[4] Usar poción");
        items.Add("[5] Huir");
        return items.ToArray();
    }

    private string[] GetDungeonFloorMenu(Player? player)
    {
        var items = new List<string> { "[1] Continuar" };
        if (player != null && GameActions.HasUsablePotion(player))
            items.Add("[2] Usar poción");
        items.Add("[5] Huir de la mazmorra");
        return items.ToArray();
    }

    private string[] GetTravelMenu(GameState? gs)
    {
        if (gs == null) return new[] { "[Enter] Volver" };
        var items = new List<string>();
        int i = 1;
        foreach (var loc in World.Locations.Where(l => l.Id != gs.CurrentLocationId))
        {
            string marker = loc.IsDungeon ? " ⚠ MAZMORRA" : "";
            items.Add($"[{i++}] {loc.Name}{marker}");
        }
        items.Add("[B] Volver");
        return items.ToArray();
    }

    private string[] GetShopMenu(Player? player) => ShopCatalog.MenuOptions();

    // Helpers

    private static string GetBehaviorLabel(Enemy e) => e.Behavior switch
    {
        EnemyBehavior.Shielded  => $"🛡 Escudo ({e.ShieldTurns} turnos)",
        EnemyBehavior.Venomous  => "☠ Venenoso",
        EnemyBehavior.Berserker => e.IsEnraged ? "⚡ ENFURECIDO" : "⚡ Berserker",
        EnemyBehavior.Boss      => e.IsEnraged ? "👑 JEFE ⚡ ENFURECIDO" : "👑 JEFE FINAL",
        _                       => ""
    };

    private static string GetBehaviorIcon(Enemy e) => e.Behavior switch
    {
        EnemyBehavior.Shielded  => "🛡",
        EnemyBehavior.Venomous  => "☠",
        EnemyBehavior.Berserker => "⚡",
        EnemyBehavior.Boss      => "👑",
        _                       => "⚔"
    };

    private static string GetBehaviorDescription(EnemyBehavior behavior) => behavior switch
    {
        EnemyBehavior.Shielded  => "Escudo mágico: reduce el daño los primeros 2 turnos",
        EnemyBehavior.Venomous  => "Ataque venenoso: aplica veneno que daña por turno",
        EnemyBehavior.Berserker => "Enfurece al 50% de HP: duplica su ataque",
        EnemyBehavior.Boss      => "JEFE FINAL: carga energía 2 turnos y desata un ataque masivo",
        _                       => ""
    };
}
