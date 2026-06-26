using TextRPG.Core.Enums;
using TextRPG.Core.Models;
using Xunit;

namespace TextRPG.Tests;

public sealed class PlayerTests
{
    [Fact]
    public void Create_ShouldSetInitialStats_ByClass()
    {
        var warrior = Player.Create("Conan", CharacterClass.Warrior);
        Assert.Equal(120, warrior.MaxHp);
        Assert.Equal(15, warrior.Attack);
        Assert.Equal(10, warrior.Defense);

        var mage = Player.Create("Gandalf", CharacterClass.Mage);
        Assert.Equal(70, mage.MaxHp);
        Assert.Equal(25, mage.Attack);
        Assert.Equal(5, mage.Defense);

        var rogue = Player.Create("Garrett", CharacterClass.Rogue);
        Assert.Equal(90, rogue.MaxHp);
        Assert.Equal(20, rogue.Attack);
        Assert.Equal(7, rogue.Defense);
    }

    [Fact]
    public void Create_ShouldSetFullHp()
    {
        var player = Player.Create("Hero", CharacterClass.Warrior);
        Assert.Equal(player.MaxHp, player.CurrentHp);
    }

    [Fact]
    public void IsAlive_ShouldBeTrue_WhenHpPositive()
    {
        var player = Player.Create("Hero", CharacterClass.Warrior);
        Assert.True(player.IsAlive);
    }

    [Fact]
    public void IsAlive_ShouldBeFalse_WhenHpZero()
    {
        var player = Player.Create("Hero", CharacterClass.Warrior);
        // Simulate critical damage by reflection or by direct set
        var field = typeof(Player).GetProperty(nameof(Player.CurrentHp));
        Assert.NotNull(field);
        // Use the setter
        player.CurrentHp = 0;
        Assert.False(player.IsAlive);
    }

    [Fact]
    public void IsAlive_ShouldBeFalse_WhenHpNegative()
    {
        var player = Player.Create("Hero", CharacterClass.Warrior);
        player.CurrentHp = -10;
        Assert.False(player.IsAlive);
    }

    [Fact]
    public void GainExperience_ShouldLevelUp_WhenThresholdReached()
    {
        var player = Player.Create("Hero", CharacterClass.Warrior);
        var initialLevel = player.Level;
        var initialMaxHp = player.MaxHp;

        // First level requires: Level * 100 = 1 * 100 = 100 XP
        player.GainExperience(100);

        Assert.Equal(initialLevel + 1, player.Level);
        Assert.Equal(initialMaxHp + 10, player.MaxHp);
    }

    [Fact]
    public void GainExperience_ShouldNotLevelUp_WhenBelowThreshold()
    {
        var player = Player.Create("Hero", CharacterClass.Warrior);
        var initialLevel = player.Level;

        player.GainExperience(50);

        Assert.Equal(initialLevel, player.Level);
    }

    [Fact]
    public void GainExperience_ShouldCarryOverExcess_AfterLevelUp()
    {
        var player = Player.Create("Hero", CharacterClass.Warrior);
        // XP needed: Level 1->2 = 100, Level 2->3 = 200. Total to reach L3 = 300.
        // Give 350: 350 - 100 - 200 = 50 remaining, Level 3
        player.Level = 1;
        player.GainExperience(350);

        Assert.Equal(3, player.Level);
        Assert.Equal(50, player.Experience);
    }

    [Fact]
    public void GainExperience_ShouldHandleMultipleLevelUps()
    {
        var player = Player.Create("Hero", CharacterClass.Warrior);
        player.Level = 1;
        player.Experience = 0;

        // Level 1 -> 2 needs 100
        // Level 2 -> 3 needs 200
        // Level 3 -> 4 needs 300
        // Total: 600 XP needed to go from 1 to 4
        // Give 650 XP -> should be level 4 with 50 XP overflow
        player.GainExperience(650);

        Assert.Equal(4, player.Level);
        Assert.Equal(50, player.Experience);
    }

    [Fact]
    public void LevelUp_ShouldIncreaseStats()
    {
        var player = Player.Create("Hero", CharacterClass.Warrior);
        var initialAtk = player.Attack;
        var initialDef = player.Defense;

        player.GainExperience(100); // Level up to 2

        Assert.Equal(initialAtk + 3, player.Attack);
        Assert.Equal(initialDef + 2, player.Defense);
    }

    [Fact]
    public void LevelUp_ShouldRestoreFullHp()
    {
        var player = Player.Create("Hero", CharacterClass.Warrior);
        player.CurrentHp = 1;

        player.GainExperience(100); // Level up to 2

        Assert.Equal(player.MaxHp, player.CurrentHp);
    }

    [Fact]
    public void Heal_ShouldRestoreUpToMax()
    {
        var player = Player.Create("Hero", CharacterClass.Warrior);
        player.CurrentHp = 50;

        player.Heal(30);

        Assert.Equal(80, player.CurrentHp);
    }

    [Fact]
    public void Heal_ShouldNotExceedMaxHp()
    {
        var player = Player.Create("Hero", CharacterClass.Warrior);
        player.CurrentHp = player.MaxHp - 5;

        player.Heal(50);

        Assert.Equal(player.MaxHp, player.CurrentHp);
    }

    [Fact]
    public void Heal_ShouldDoNothing_WhenAlreadyFull()
    {
        var player = Player.Create("Hero", CharacterClass.Warrior);
        var initialHp = player.CurrentHp;

        player.Heal(10);

        Assert.Equal(initialHp, player.CurrentHp);
    }

    [Fact]
    public void Heal_ShouldHandleZeroAmount()
    {
        var player = Player.Create("Hero", CharacterClass.Warrior);
        player.CurrentHp = 50;

        player.Heal(0);

        Assert.Equal(50, player.CurrentHp);
    }
}
