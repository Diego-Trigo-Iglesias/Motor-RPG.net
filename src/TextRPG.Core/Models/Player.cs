using TextRPG.Core.Constants;
using TextRPG.Core.Enums;

namespace TextRPG.Core.Models;

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

    // Equipamiento
    public Item? EquippedWeapon { get; set; }
    public Item? EquippedArmor { get; set; }
    public Item? EquippedAmulet { get; set; }

    /// <summary>Lista de items equipados (no nulos).</summary>
    public List<Item> GetEquippedItems()
    {
        var items = new List<Item>(3);
        if (EquippedWeapon != null) items.Add(EquippedWeapon);
        if (EquippedArmor != null) items.Add(EquippedArmor);
        if (EquippedAmulet != null) items.Add(EquippedAmulet);
        return items;
    }

    /// <summary>Ataque total incluyendo bono de equipo.</summary>
    public int TotalAttack => Attack + (EquippedWeapon?.AttackBonus ?? 0) + (EquippedAmulet?.AttackBonus ?? 0);

    /// <summary>Defensa total incluyendo bono de equipo.</summary>
    public int TotalDefense => Defense + (EquippedArmor?.DefenseBonus ?? 0);

    /// <summary>Slot que ocupa un item según su tipo.</summary>
    public static EquipmentSlot SlotFor(ItemType type) => type switch
    {
        ItemType.Weapon => EquipmentSlot.Weapon,
        ItemType.Armor => EquipmentSlot.Armor,
        ItemType.Amulet => EquipmentSlot.Amulet,
        _ => EquipmentSlot.None
    };

    /// <summary>Equipa un item. Si ya hay uno equipado en ese slot, lo devuelve al inventario.</summary>
    public void Equip(Item item)
    {
        var slot = SlotFor(item.Type);
        if (slot == EquipmentSlot.None) return; // No equipable

        // Devolver item previo al inventario
        Item? prev = slot switch
        {
            EquipmentSlot.Weapon => EquippedWeapon,
            EquipmentSlot.Armor => EquippedArmor,
            EquipmentSlot.Amulet => EquippedAmulet,
            _ => null
        };
        if (prev != null) Inventory.Add(prev);

        // Asignar nuevo item
        switch (slot)
        {
            case EquipmentSlot.Weapon: EquippedWeapon = item; break;
            case EquipmentSlot.Armor: EquippedArmor = item; break;
            case EquipmentSlot.Amulet: EquippedAmulet = item; break;
        }

        // Quitar del inventario si estaba allí
        Inventory.Remove(item);
    }

    /// <summary>Desequipa un slot. El item vuelve al inventario.</summary>
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
        CurrentHp = MaxHp;
        Attack   += GameConstants.AtkPerLevel;
        Defense  += GameConstants.DefPerLevel;
    }

    public void Heal(int amount) =>
        CurrentHp = Math.Min(CurrentHp + amount, MaxHp);
}
