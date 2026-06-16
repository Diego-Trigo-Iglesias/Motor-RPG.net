using TextRPG.Core.Enums;
using TextRPG.Core.Models;
using TextRPG.Core.Engine;
using Xunit;

namespace TextRPG.Tests;

public sealed class CombatEngineTests
{
    private readonly ICombatEngine _engine = new CombatEngine();

    [Fact]
    public void SimulateRound_ShouldReturnValidRound()
    {
        var player = Player.Create("Hero", CharacterClass.Warrior);
        var enemy = Enemy.Random(1);
        var round = _engine.SimulateRound(player, enemy);

        Assert.True(round.PlayerDamage >= 0);
        Assert.True(round.EnemyDamage >= 0);
    }

    [Fact]
    public void SimulateRound_MinDamageIsZero_WhenDamageNegative()
    {
        var player = Player.Create("Tank", CharacterClass.Warrior);
        // Create an enemy with 0 attack so the random variation may go negative
        var enemy = new Enemy
        {
            Name = "Slime", MaxHp = 50, CurrentHp = 50,
            Attack = 1, Defense = 999, ExpReward = 5, GoldReward = 1
        };

        for (int i = 0; i < 50; i++)
        {
            var round = _engine.SimulateRound(player, enemy);
            Assert.True(round.PlayerDamage >= 0);
            Assert.True(round.EnemyDamage >= 0);
        }
    }

    [Fact]
    public void ApplyRound_ShouldReduceHealth_WhenDamagePositive()
    {
        var player = Player.Create("Hero", CharacterClass.Warrior);
        var enemy = Enemy.Random(1);
        var initialPlayerHp = player.CurrentHp;
        var initialEnemyHp = enemy.CurrentHp;

        var round = _engine.SimulateRound(player, enemy);
        _engine.ApplyRound(player, enemy, round);

        Assert.True(player.CurrentHp <= initialPlayerHp);
        Assert.True(enemy.CurrentHp <= initialEnemyHp);
    }

    [Fact]
    public void ApplyRound_ShouldNotIncreaseHealth_WhenDamageIsZero()
    {
        var player = Player.Create("Hero", CharacterClass.Warrior);
        var enemy = new Enemy
        {
            Name = "Wall", MaxHp = 100, CurrentHp = 100,
            Attack = 0, Defense = 999, ExpReward = 0, GoldReward = 0
        };

        var initialHp = player.CurrentHp;
        // Force a known round structure where no damage happens
        var round = new CombatRound(0, 0, false, false, false);
        _engine.ApplyRound(player, enemy, round);

        Assert.Equal(initialHp, player.CurrentHp);
        Assert.Equal(100, enemy.CurrentHp);
    }

    [Fact]
    public void SimulateFull_PlayerStronger_ShouldWin()
    {
        var player = Player.Create("Hero", CharacterClass.Warrior);
        var enemy = new Enemy
        {
            Name = "Weakling", MaxHp = 10, CurrentHp = 10,
            Attack = 1, Defense = 0, ExpReward = 10, GoldReward = 5
        };

        var result = _engine.SimulateFull(player, enemy);

        Assert.True(result.PlayerWon);
        Assert.True(result.RoundsPlayed > 0);
        Assert.True(result.ExpGained > 0);
        Assert.True(result.GoldGained > 0);
    }

    [Fact]
    public void SimulateFull_PlayerWeaker_ShouldLose()
    {
        var player = Player.Create("GlassCannon", CharacterClass.Mage);
        var enemy = new Enemy
        {
            Name = "Overlord", MaxHp = 999, CurrentHp = 999,
            Attack = 50, Defense = 20, ExpReward = 0, GoldReward = 0
        };

        var result = _engine.SimulateFull(player, enemy);

        Assert.False(result.PlayerWon);
        Assert.Equal(0, result.GoldGained);
    }

    [Fact]
    public void SimulateFull_ShouldNotExceedMaxRounds()
    {
        var player = Player.Create("Tank", CharacterClass.Warrior);
        var enemy = new Enemy
        {
            Name = "Wall", MaxHp = 1000, CurrentHp = 1000,
            Attack = 1, Defense = 50, ExpReward = 0, GoldReward = 0
        };

        var result = _engine.SimulateFull(player, enemy);

        Assert.True(result.RoundsPlayed <= 200,
            $"Combat exceeded max rounds: {result.RoundsPlayed}");
    }

    [Fact]
    public void SimulateFull_GrantedRewards_OnVictory()
    {
        var player = Player.Create("Hero", CharacterClass.Warrior);
        var enemy = new Enemy
        {
            Name = "Goblin", MaxHp = 30, CurrentHp = 30,
            Attack = 5, Defense = 2, ExpReward = 40, GoldReward = 10
        };

        var result = _engine.SimulateFull(player, enemy);

        Assert.True(result.PlayerWon);
        Assert.Equal(40, result.ExpGained);
        Assert.Equal(10, result.GoldGained);
    }

    [Fact]
    public void SimulateFull_ReducedExperience_OnDefeat()
    {
        var player = Player.Create("Hero", CharacterClass.Warrior);
        var enemy = new Enemy
        {
            Name = "Boss", MaxHp = 999, CurrentHp = 999,
            Attack = 99, Defense = 30, ExpReward = 100, GoldReward = 50
        };

        var result = _engine.SimulateFull(player, enemy);

        Assert.False(result.PlayerWon);
        Assert.Equal(25, result.ExpGained); // 1/4 of 100
        Assert.Equal(0, result.GoldGained);
    }
}
