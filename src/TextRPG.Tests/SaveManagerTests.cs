using TextRPG.Core.Enums;
using TextRPG.Core.Models;
using TextRPG.Core.SaveSystem;
using Xunit;

namespace TextRPG.Tests;

public sealed class SaveManagerTests : IDisposable
{
    private readonly string _tempDir;
    private readonly SaveManager _saveManager;
    private readonly GameState _testState;

    public SaveManagerTests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), "TextRPG_Tests_" + Guid.NewGuid());
        Directory.CreateDirectory(_tempDir);
        _saveManager = new SaveManager(_tempDir);

        _testState = new GameState
        {
            Player = Player.Create("TestHero", CharacterClass.Warrior),
            CurrentLocationId = "village",
            TotalBattles = 5,
            TotalVictories = 3,
            CreatedAt = DateTime.UtcNow,
            LastSavedAt = DateTime.UtcNow
        };
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempDir))
            Directory.Delete(_tempDir, recursive: true);
    }

    [Fact]
    public void Save_ShouldCreateFile_InSpecifiedDirectory()
    {
        _saveManager.Save(_testState, "test_slot");

        var filePath = Path.Combine(_tempDir, "test_slot.json");
        Assert.True(File.Exists(filePath));
    }

    [Fact]
    public void Save_ShouldThrow_WhenSlotNameEmpty()
    {
        Assert.Throws<ArgumentException>(() => _saveManager.Save(_testState, ""));
    }

    [Fact]
    public void Save_ShouldThrow_WhenSlotNameNull()
    {
        Assert.Throws<ArgumentNullException>(() => _saveManager.Save(_testState, null!));
    }

    [Fact]
    public void SaveAndLoad_Roundtrip_ShouldPreserveState()
    {
        _saveManager.Save(_testState, "roundtrip");
        var loaded = _saveManager.Load("roundtrip");

        Assert.NotNull(loaded);
        Assert.Equal(_testState.Player.Name, loaded.Player.Name);
        Assert.Equal(_testState.Player.Class, loaded.Player.Class);
        Assert.Equal(_testState.Player.Level, loaded.Player.Level);
        Assert.Equal(_testState.CurrentLocationId, loaded.CurrentLocationId);
        Assert.Equal(_testState.TotalBattles, loaded.TotalBattles);
        Assert.Equal(_testState.TotalVictories, loaded.TotalVictories);
    }

    [Fact]
    public void SaveAndLoad_Roundtrip_ShouldRestorePlayerStats()
    {
        _saveManager.Save(_testState, "stats_test");
        var loaded = _saveManager.Load("stats_test");

        Assert.NotNull(loaded);
        Assert.Equal(_testState.Player.MaxHp, loaded.Player.MaxHp);
        Assert.Equal(_testState.Player.CurrentHp, loaded.Player.CurrentHp);
        Assert.Equal(_testState.Player.Attack, loaded.Player.Attack);
        Assert.Equal(_testState.Player.Defense, loaded.Player.Defense);
        Assert.Equal(_testState.Player.Gold, loaded.Player.Gold);
        Assert.Equal(_testState.Player.Experience, loaded.Player.Experience);
    }

    [Fact]
    public void Load_ShouldReturnNull_WhenSaveNotExists()
    {
        var result = _saveManager.Load("non_existent_slot");
        Assert.Null(result);
    }

    [Fact]
    public void Load_ShouldThrow_WhenSlotNameEmpty()
    {
        Assert.Throws<ArgumentException>(() => _saveManager.Load(""));
    }

    [Fact]
    public void SaveExists_ShouldReturnTrue_AfterSave()
    {
        _saveManager.Save(_testState, "exists_test");
        Assert.True(_saveManager.SaveExists("exists_test"));
    }

    [Fact]
    public void SaveExists_ShouldReturnFalse_WhenNotSaved()
    {
        Assert.False(_saveManager.SaveExists("ghost_slot"));
    }

    [Fact]
    public void SaveExists_ShouldThrow_WhenSlotNameEmpty()
    {
        Assert.Throws<ArgumentException>(() => _saveManager.SaveExists(""));
    }

    [Fact]
    public void Delete_ShouldRemoveFile()
    {
        _saveManager.Save(_testState, "to_delete");
        Assert.True(_saveManager.SaveExists("to_delete"));

        _saveManager.Delete("to_delete");
        Assert.False(_saveManager.SaveExists("to_delete"));
    }

    [Fact]
    public void Delete_ShouldNotThrow_WhenFileNotExists()
    {
        var exception = Record.Exception(() => _saveManager.Delete("phantom_slot"));
        Assert.Null(exception);
    }

    [Fact]
    public void Delete_ShouldThrow_WhenSlotNameEmpty()
    {
        Assert.Throws<ArgumentException>(() => _saveManager.Delete(""));
    }

    [Fact]
    public void ListSaves_ShouldReturnEmpty_WhenNoSaves()
    {
        var saves = _saveManager.ListSaves();
        Assert.NotNull(saves);
        Assert.Empty(saves);
    }

    [Fact]
    public void ListSaves_ShouldListAllSavedGames()
    {
        _saveManager.Save(_testState, "slot_a");
        _saveManager.Save(_testState, "slot_b");

        var saves = _saveManager.ListSaves();

        Assert.Equal(2, saves.Count);
        Assert.Contains(saves, s => s.Slot == "slot_a");
        Assert.Contains(saves, s => s.Slot == "slot_b");
    }

    [Fact]
    public void ListSaves_ShouldIncludePlayerInfo()
    {
        _saveManager.Save(_testState, "info_test");
        var saves = _saveManager.ListSaves();

        var save = Assert.Single(saves);
        Assert.Contains("TestHero", save.PlayerInfo);
        Assert.Contains("Warrior", save.PlayerInfo);
    }

    [Fact]
    public void ListSaves_ShouldSkipCorruptedFiles()
    {
        // Create a corrupted JSON file
        File.WriteAllText(Path.Combine(_tempDir, "corrupted.json"), "not valid json");

        _saveManager.Save(_testState, "valid_slot");
        var saves = _saveManager.ListSaves();

        Assert.Single(saves);
        Assert.Equal("valid_slot", saves[0].Slot);
    }

    [Fact]
    public void Constructor_ShouldThrow_WhenDirectoryEmpty()
    {
        Assert.Throws<ArgumentException>(() => new SaveManager(""));
    }

    [Fact]
    public void Constructor_ShouldThrow_WhenDirectoryNull()
    {
        Assert.Throws<ArgumentNullException>(() => new SaveManager(null!));
    }

    [Fact]
    public void SanitizeSlotName_ShouldRemoveInvalidChars()
    {
        _saveManager.Save(_testState, "slot:with<invalid>chars?");
        Assert.True(_saveManager.SaveExists("slot:with<invalid>chars?"));
        // The file should exist with sanitized name
        var files = Directory.GetFiles(_tempDir, "*.json");
        var fileName = Path.GetFileNameWithoutExtension(files[0]);
        Assert.DoesNotContain("<", fileName);
        Assert.DoesNotContain(">", fileName);
        Assert.DoesNotContain(":", fileName);
    }
}
