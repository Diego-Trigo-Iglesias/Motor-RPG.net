namespace TextRPG.Core.Models;

/// <summary>Tipos de objeto en el juego.</summary>
public enum ItemType
{
    Consumable,
    Weapon,
    Armor,
    Amulet,
    Key,
    Quest
}

/// <summary>Slots de equipamiento del personaje.</summary>
public enum EquipmentSlot
{
    None,
    Weapon,
    Armor,
    Amulet
}

/// <summary>Objeto del juego con nombre, tipo y efecto.</summary>
public class Item
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public ItemType Type { get; set; } = ItemType.Consumable;
    public int Value { get; set; }
    public int Price { get; set; }

    // Efectos al usar/aplicar
    public int HealAmount { get; set; }
    public int AttackBonus { get; set; }
    public int DefenseBonus { get; set; }

    public override string ToString() => Name;

    public string ToDetailString()
    {
        var parts = new List<string> { Name };
        if (HealAmount > 0) parts.Add($"+{HealAmount} HP");
        if (AttackBonus > 0) parts.Add($"+{AttackBonus} ATK");
        if (DefenseBonus > 0) parts.Add($"+{DefenseBonus} DEF");
        return string.Join(" | ", parts);
    }

    /// <summary>Texto corto de estadísticas (ej: "+30HP", "+6ATK").</summary>
    public string ToStatString()
    {
        if (HealAmount > 0) return $"+{HealAmount}HP";
        if (AttackBonus > 0) return $"+{AttackBonus}ATK";
        if (DefenseBonus > 0) return $"+{DefenseBonus}DEF";
        return "";
    }
}

/// <summary>Catalogo de objetos predefinidos del juego.</summary>
public static class ItemCatalog
{
    public static Item PotionSmall => new()
    {
        Id = "potion_small", Name = "Poción pequeña",
        Description = "Restaura 30 HP", Type = ItemType.Consumable,
        HealAmount = 30, Price = 10
    };

    public static Item PotionLarge => new()
    {
        Id = "potion_large", Name = "Poción grande",
        Description = "Restaura 70 HP", Type = ItemType.Consumable,
        HealAmount = 70, Price = 25
    };

    public static Item AmuletOfStrength => new()
    {
        Id = "amulet_strength", Name = "Amuleto de fuerza",
        Description = "+5 ATK permanente", Type = ItemType.Amulet,
        AttackBonus = 5, Price = 50
    };

    public static Item LightArmor => new()
    {
        Id = "armor_light", Name = "Armadura ligera",
        Description = "+3 DEF", Type = ItemType.Armor,
        DefenseBonus = 3, Price = 40
    };

    public static Item IronSword => new()
    {
        Id = "sword_iron", Name = "Espada de hierro",
        Description = "+6 ATK", Type = ItemType.Weapon,
        AttackBonus = 6, Price = 60
    };

    public static Item GetById(string id) => id switch
    {
        "potion_small" => PotionSmall,
        "potion_large" => PotionLarge,
        "amulet_strength" => AmuletOfStrength,
        "armor_light" => LightArmor,
        "sword_iron" => IronSword,
        _ => PotionSmall
    };
}
