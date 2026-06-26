using System.Text.Json;
using TextRPG.Core.Models;

namespace TextRPG.Core.SaveSystem;

public sealed class SaveManager : ISaveManager
{
    private readonly string _saveDirectory;

    public SaveManager(string directoryPath)
    {
        ArgumentException.ThrowIfNullOrEmpty(directoryPath);
        _saveDirectory = directoryPath;
        if (!Directory.Exists(_saveDirectory))
            Directory.CreateDirectory(_saveDirectory);
    }

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true
    };

    public void Save(GameState state, string slotName)
    {
        ArgumentException.ThrowIfNullOrEmpty(slotName);

        state.LastSavedAt = DateTime.UtcNow;
        var filePath = GetFilePath(slotName);
        var json = JsonSerializer.Serialize(state, JsonOptions);
        File.WriteAllText(filePath, json);
    }

    public GameState? Load(string slotName)
    {
        ArgumentException.ThrowIfNullOrEmpty(slotName);

        var filePath = GetFilePath(slotName);
        if (!File.Exists(filePath))
            return null;

        try
        {
            var json = File.ReadAllText(filePath);
            return JsonSerializer.Deserialize<GameState>(json);
        }
        catch
        {
            return null;
        }
    }

    public bool SaveExists(string slotName)
    {
        ArgumentException.ThrowIfNullOrEmpty(slotName);
        return File.Exists(GetFilePath(slotName));
    }

    public void Delete(string slotName)
    {
        ArgumentException.ThrowIfNullOrEmpty(slotName);

        var filePath = GetFilePath(slotName);
        if (File.Exists(filePath))
            File.Delete(filePath);
    }

    public List<SaveInfo> ListSaves()
    {
        if (!Directory.Exists(_saveDirectory))
            return [];

        var saves = new List<SaveInfo>();

        foreach (var filePath in Directory.GetFiles(_saveDirectory, "*.json"))
        {
            try
            {
                var json = File.ReadAllText(filePath);
                var state = JsonSerializer.Deserialize<GameState>(json);
                if (state == null) continue;

                var slotName = Path.GetFileNameWithoutExtension(filePath);
                var playerInfo = $"{state.Player.Name} ({state.Player.Class} Lv.{state.Player.Level})";
                saves.Add(new SaveInfo(slotName, playerInfo, state.LastSavedAt));
            }
            catch
            {
                // Skip corrupted files
                continue;
            }
        }

        return saves;
    }

    private string GetFilePath(string slotName)
    {
        var safeName = SanitizeSlotName(slotName);
        return Path.Combine(_saveDirectory, $"{safeName}.json");
    }

    private static string SanitizeSlotName(string slotName)
    {
        var invalid = Path.GetInvalidFileNameChars();
        return string.Concat(slotName.Where(ch => !invalid.Contains(ch)));
    }
}
