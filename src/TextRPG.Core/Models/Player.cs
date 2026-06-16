using TextRPG.Core.Enums;

namespace TextRPG.Core.Models;

public class Player
{
    public string Name { get; set; } = string.Empty;
    public CharacterClass Class { get; set; }
    public int Level { get; set; } = 1;
    public int Experience { get; set; } = 0;
    public int ExperienceToNextLevel => Level * 100;

    public int MaxHp { get; set; }
    public int CurrentHp { get; set; }
    public int Attack { get; set; }
    public int Defense { get; set; }
    public int Gold { get; set; } = 10;

    public List<string> Inventory { get; set; } = [];
    public List<string> CompletedQuests { get; set; } = [];

    public static Player Create(string name, CharacterClass characterClass)
    {
        var (hp, atk, def) = characterClass switch
        {
            CharacterClass.Warrior => (120, 15, 10),
            CharacterClass.Mage    => (70,  25,  5),
            CharacterClass.Rogue   => (90,  20,  7),
            _                      => (100, 15,  8)
        };

        return new Player
        {
            Name    = name,
            Class   = characterClass,
            MaxHp   = hp,
            CurrentHp = hp,
            Attack  = atk,
            Defense = def
        };
    }

    public bool IsAlive => CurrentHp > 0;

    public void GainExperience(int amount)
    {
        Experience += amount;
        while (Experience >= ExperienceToNextLevel)
        {
            Experience -= ExperienceToNextLevel;
            LevelUp();
        }
    }

    private void LevelUp()
    {
        Level++;
        MaxHp    += 10;
        CurrentHp = MaxHp;
        Attack   += 3;
        Defense  += 2;
    }

    public void Heal(int amount) =>
        CurrentHp = Math.Min(CurrentHp + amount, MaxHp);
}
