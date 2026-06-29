using TextRPG.Core.Constants;
using TextRPG.Core.Enums;

namespace TextRPG.Core.Models;

/// <summary>Jugador con stats, inventario, equipamiento y efectos de estado.</summary>
public class Player
{
    public string Name { get; set; } = string.Empty;
    public CharacterClass Class { get; set; }
    public int Level { get; set; } = 1;
    public int Experience { get; set; } = 0;
    public int ExperienceToNextLevel => Level * GameConstants.ExpPerLevel;

    public int MaxHp { get; set; }
    public int CurrentHp { get; set; }
    public int Attack { get; set; }
    public int Defense { get; set; }
    public int Gold { get; set; } = GameConstants.StartingGold;

    public List<Item> Inventory { get; set; } = [];
    public List<string> CompletedQuests { get; set; } = [];

    //  Equipamiento 
    public Item? EquippedWeapon { get; set; }
    public Item? EquippedArmor { get; set; }
    public Item? EquippedAmulet { get; set; }

    //  Efectos de estado (combate) 
    /// <summary>Turnos restantes de veneno. 0 = sin veneno.</summary>
    public int PoisonTurnsRemaining { get; set; }

    /// <summary>Daño por turno del veneno activo.</summary>
    public int PoisonDamagePerTurn { get; set; }

    /// <summary>True si el jugador usó Defensa este turno (reduce daño recibido). Se resetea cada ronda.</summary>
    public bool IsDefending { get; set; }

    public List<Item> GetEquippedItems()
    {
        var items = new List<Item>(3);
        if (EquippedWeapon != null) items.Add(EquippedWeapon);
        if (EquippedArmor != null) items.Add(EquippedArmor);
        if (EquippedAmulet != null) items.Add(EquippedAmulet);
        return items;
    }

    public int TotalAttack => Attack + (EquippedWeapon?.AttackBonus ?? 0) + (EquippedAmulet?.AttackBonus ?? 0);
    public int TotalDefense => Defense + (EquippedArmor?.DefenseBonus ?? 0);

    public static EquipmentSlot SlotFor(ItemType type) => type switch
    {
        ItemType.Weapon => EquipmentSlot.Weapon,
        ItemType.Armor => EquipmentSlot.Armor,
        ItemType.Amulet => EquipmentSlot.Amulet,
        _ => EquipmentSlot.None
    };

    public void Equip(Item item)
    {
        var slot = SlotFor(item.Type);
        if (slot == EquipmentSlot.None) return;

        Item? prev = slot switch
        {
            EquipmentSlot.Weapon => EquippedWeapon,
            EquipmentSlot.Armor => EquippedArmor,
            EquipmentSlot.Amulet => EquippedAmulet,
            _ => null
        };
        if (prev != null) Inventory.Add(prev);

        switch (slot)
        {
            case EquipmentSlot.Weapon: EquippedWeapon = item; break;
            case EquipmentSlot.Armor: EquippedArmor = item; break;
            case EquipmentSlot.Amulet: EquippedAmulet = item; break;
        }
        Inventory.Remove(item);
    }

    public void Unequip(EquipmentSlot slot)
    {
        Item? item = slot switch
        {
            EquipmentSlot.Weapon => EquippedWeapon,
            EquipmentSlot.Armor => EquippedArmor,
            EquipmentSlot.Amulet => EquippedAmulet,
            _ => null
        };
        if (item == null) return;

        switch (slot)
        {
            case EquipmentSlot.Weapon: EquippedWeapon = null; break;
            case EquipmentSlot.Armor: EquippedArmor = null; break;
            case EquipmentSlot.Amulet: EquippedAmulet = null; break;
        }
        Inventory.Add(item);
    }

    /// <summary>Limpia efectos de estado al terminar el combate.</summary>
    public void ClearStatusEffects()
    {
        PoisonTurnsRemaining = 0;
        PoisonDamagePerTurn = 0;
        IsDefending = false;
    }

    /// <summary>Aplica daño de veneno acumulado. Devuelve el daño infligido.</summary>
    public int ApplyPoisonDamage()
    {
        if (PoisonTurnsRemaining <= 0) return 0;

        int dmg = PoisonDamagePerTurn;
        CurrentHp = Math.Max(0, CurrentHp - dmg);
        PoisonTurnsRemaining--;
        return dmg;
    }

    public static Player Create(string name, CharacterClass characterClass)
    {
        var (hp, atk, def) = characterClass switch
        {
            CharacterClass.Warrior => GameConstants.WarriorStats,
            CharacterClass.Mage    => GameConstants.MageStats,
            CharacterClass.Rogue   => GameConstants.RogueStats,
            _                      => GameConstants.DefaultStats
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
        MaxHp    += GameConstants.HpPerLevel;
        CurrentHp = MaxHp; // Curar al subir de nivel
        Attack   += GameConstants.AtkPerLevel;
        Defense  += GameConstants.DefPerLevel;
    }

    public void Heal(int amount) =>
        CurrentHp = Math.Min(CurrentHp + amount, MaxHp);
}
