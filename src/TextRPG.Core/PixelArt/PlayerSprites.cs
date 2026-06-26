namespace TextRPG.Core.PixelArt;

using static SpriteHelpers;

/// Sprites 48×48 con shapes avanzadas y detalles finos.
/// Clase parcial — cada sprite vive en su propio archivo.
public static partial class PlayerSprites
{
    // Colores compartidos
    private static readonly PixelColor
        Skin = new(230, 190, 150), SkinD = new(200, 165, 130), SkinS = new(245, 210, 175),
        EyeW = new(255, 255, 255), EyeP = new(40, 40, 50);

    // Guerrero
    private static readonly PixelColor
        HR = new(195, 40, 40), HDR = new(150, 28, 28), HG = new(215, 180, 45),
        AS = new(150, 165, 190), AM = new(120, 135, 160), AD = new(70, 80, 100),
        CP = new(175, 35, 35), CD = new(130, 20, 20);

    // Mago
    private static readonly PixelColor
        HB = new(42, 60, 195), HD = new(28, 42, 145), RP = new(130, 48, 170),
        RL = new(160, 58, 200), RD = new(80, 28, 110), SY = new(255, 218, 58),
        SW = new(178, 138, 78), SB = new(140, 100, 50);

    // Picaro
    private static readonly PixelColor
        HG2 = new(38, 152, 48), HD2 = new(22, 108, 32), CK = new(58, 66, 76),
        CKD = new(38, 44, 52), LT = new(138, 93, 48), LTD = new(98, 53, 28),
        SS = new(155, 145, 140);

    public static PixelSprite ForClass(Enums.CharacterClass cls) => cls switch
    {
        Enums.CharacterClass.Warrior => Warrior,
        Enums.CharacterClass.Mage => Mage,
        Enums.CharacterClass.Rogue => Rogue,
        _ => Warrior
    };
}
