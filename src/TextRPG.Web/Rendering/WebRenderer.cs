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
/// NOTA: Los métodos síncronos de IRenderer lanzan NotSupportedException.
///       El juego web usa los métodos asíncronos (SelectMenuAsync, etc.).
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
        Paused
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
        Messages.Add("Un RPG pixel art en tu navegador.");
        Messages.Add("Creado con .NET 8 + Blazor WASM.");
    }

    public void PrintStatus(Player p)
    {
        PlayerSprite = SpriteData.FromPixelSprite(PlayerSprites.ForClass(p.Class));
        PlayerHp = new HpBarData { Current = p.CurrentHp, Max = p.MaxHp, Label = "HP" };

        var classIcon = p.Class switch
        {
            CharacterClass.Warrior => IconPalette.Warrior,
            CharacterClass.Mage => IconPalette.Mage,
            CharacterClass.Rogue => IconPalette.Rogue,
            _ => IconPalette.DefaultIcon
        };

        StatusLine = $"{classIcon} {p.Name} ({p.Class} Lv.{p.Level})  " +
                     $"Oro: {p.Gold}  ATK: {p.Attack}  DEF: {p.Defense}";
    }

    public void PrintLocation(Location loc)
    {
        LocationName = $"{loc.IconKey}  {loc.Name}";
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
        Messages.Add($"¡Encuentras a {enemy.Name}!");
    }

    public void PrintCombatRound(Player player, Enemy enemy, CombatRound round, int roundNum)
    {
        LastRound = round;
        LastRoundNum = roundNum;
        PlayerHp = new HpBarData { Current = player.CurrentHp, Max = player.MaxHp, Label = $"{player.Name} HP" };
        EnemyHp = new HpBarData { Current = Math.Max(0, enemy.CurrentHp), Max = enemy.MaxHp, Label = $"{enemy.Name} HP" };

        if (round.EnemyDodged)
            Messages.Add($"{enemy.Name} esquivó tu ataque.");
        else if (round.IsCritical)
            Messages.Add($"¡GOLPE CRÍTICO! {round.PlayerDamage} de daño.");
        else
            Messages.Add($"Atacaste a {enemy.Name} por {round.PlayerDamage} de daño.");

        if (round.PlayerDodged)
            Messages.Add($"Esquivaste el ataque de {enemy.Name}.");
        else
            Messages.Add($"{enemy.Name} te golpeó por {round.EnemyDamage} de daño.");
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
        Messages.Add($"HP: {player.CurrentHp}/{player.MaxHp} | ATK: {player.Attack} | DEF: {player.Defense}");
        Messages.Add($"Oro: {player.Gold} | EXP: {player.Experience}/{player.ExperienceToNextLevel}");
        Messages.Add($"Batallas: {state.TotalBattles} | Victorias: {state.TotalVictories}");
        if (player.Inventory.Count > 0)
        {
            Messages.Add("Objetos:");
            foreach (var item in player.Inventory)
                Messages.Add($"  - {item}");
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
}

// DTOs para el Canvas

public class SpriteData
{
    public int Width { get; set; }
    public int Height { get; set; }
    public PixelData[] Pixels { get; set; } = [];

    public static SpriteData FromPixelSprite(PixelSprite sprite)
    {
        var pixels = new PixelData[sprite.Width * sprite.Height];
        for (int y = 0; y < sprite.Height; y++)
            for (int x = 0; x < sprite.Width; x++)
            {
                pixels[y * sprite.Width + x] = sprite.IsTransparent(x, y)
                    ? new PixelData { A = 0 }
                    : new PixelData
                    {
                        R = sprite.GetPixel(x, y).R,
                        G = sprite.GetPixel(x, y).G,
                        B = sprite.GetPixel(x, y).B,
                        A = 255
                    };
            }
        return new SpriteData { Width = sprite.Width, Height = sprite.Height, Pixels = pixels };
    }
}

public class PixelData
{
    public byte R { get; set; }
    public byte G { get; set; }
    public byte B { get; set; }
    public byte A { get; set; }
}

public class HpBarData
{
    public int Current { get; set; }
    public int Max { get; set; }
    public string Label { get; set; } = "";
}

