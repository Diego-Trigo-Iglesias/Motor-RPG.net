using TextRPG.Core.Enums;

namespace TextRPG.Core.PixelArt;

public static class Animations
{
    private const int IdleMs = 600;
    private const int AtkMs = 80;
    private const int HitMs = 100;
    private const int EffMs = 400;

    //  Idle 
    public static Animation WarriorIdle { get; } = new("warrior_idle", true,
        new(PlayerSprites.Warrior, IdleMs),
        new(PlayerSprites.Warrior, IdleMs / 2));

    public static Animation MageIdle { get; } = new("mage_idle", true,
        new(PlayerSprites.Mage, IdleMs),
        new(PlayerSprites.Mage, IdleMs / 2));

    public static Animation RogueIdle { get; } = new("rogue_idle", true,
        new(PlayerSprites.Rogue, IdleMs),
        new(PlayerSprites.Rogue, IdleMs / 2));

    public static Animation IdleForClass(CharacterClass cls) => cls switch
    {
        CharacterClass.Warrior => WarriorIdle,
        CharacterClass.Mage => MageIdle,
        CharacterClass.Rogue => RogueIdle,
        _ => WarriorIdle
    };

    public static Animation EnemyIdle(string enemyName)
    {
        var sprite = EnemySprites.ForName(enemyName);
        return new Animation($"enemy_idle_{enemyName}", true,
            new(sprite, IdleMs),
            new(sprite, IdleMs / 2));
    }

    //  Efectos (tamaños actualizados) 
    public static Animation AttackSlash { get; } = new("attack_slash", false,
        new(EffectSprites.Slash, AtkMs),
        new(PixelSprite.Create(16, 8), AtkMs));

    public static Animation CriticalExplosion { get; } = new("crit_explosion", false,
        new(EffectSprites.CriticalHit, HitMs),
        new(PixelSprite.Create(16, 12), HitMs * 2));

    public static Animation HealEffect { get; } = new("heal_effect", false,
        new(EffectSprites.Heal, EffMs),
        new(PixelSprite.Create(12, 10), EffMs / 2));

    public static Animation DodgeEffect { get; } = new("dodge_effect", false,
        new(EffectSprites.Dodge, EffMs),
        new(PixelSprite.Create(12, 8), EffMs / 2));

    public static Animation VictoryEffect { get; } = new("victory_effect", true,
        new(EffectSprites.Victory, 300),
        new(PixelSprite.Create(14, 10), 200));

    public static Animation DefeatEffect { get; } = new("defeat_effect", false,
        new(EffectSprites.CriticalHit, 200),
        new(PixelSprite.Create(16, 12), 600));

    //  Transiciones 
    public static Animation TravelTransition { get; } = new("travel", false,
        new(PixelSprite.Create(8, 8), 200),
        new(PixelSprite.Create(8, 8), 200));

    //  Título 
    public static Animation TitleScreen { get; } = new("title", true,
        new(PlayerSprites.Warrior, 500),
        new(PlayerSprites.Mage, 500),
        new(PlayerSprites.Rogue, 500));
}
