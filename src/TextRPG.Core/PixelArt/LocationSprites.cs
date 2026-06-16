namespace TextRPG.Core.PixelArt;

/// Paisajes 24×12 para localizaciones.
public static class LocationSprites
{
    // Aldea: s=cielo, r=tejado, h=casa, g=hierba
    private static readonly PixelColor LS = new(80, 140, 220);
    private static readonly PixelColor LR = new(180, 75, 45);
    private static readonly PixelColor LH = new(190, 155, 100);
    private static readonly PixelColor LG = new(60, 165, 55);

    // Bosque: o=hojas osc, f=hojas clr, t tronco
    private static readonly PixelColor FO = new(25, 110, 25);
    private static readonly PixelColor FF = new(45, 150, 40);
    private static readonly PixelColor FT = new(90, 60, 30);

    // Cueva: s=piedra, k=oscuro, b=negro
    private static readonly PixelColor CS = new(100, 90, 80);
    private static readonly PixelColor CK = new(45, 40, 35);
    private static readonly PixelColor CB = new(25, 20, 15);

    // Ruinas: b=bloque, r=ruina, g=hierba
    private static readonly PixelColor RS = new(150, 140, 120);
    private static readonly PixelColor RR = new(110, 100, 85);
    private static readonly PixelColor RG = new(65, 130, 50);

    // Mazmorra: d=oscuro, w=pared, f=fuego
    private static readonly PixelColor DD = new(35, 30, 40);
    private static readonly PixelColor DW = new(65, 60, 70);
    private static readonly PixelColor DF = new(210, 100, 30);

    public static PixelSprite Village => _village.Value;
    private static readonly Lazy<PixelSprite> _village = new(() => PixelSprite.FromAscii([
        "   ssss   ssss    ",
        "  ssssss ssssss   ",
        "  srrrss srrrss   ",
        "  shhhss shhhss   ",
        "  shhhhs shhhhs   ",
        "  shhhss shhhss   ",
        "  ssssss ssssss   ",
        " gggggggggggggggg ",
        " gggggggggggggggg ",
        " gggggggggggggggg ",
        " gggggggggggggggg ",
        " gggggggggggggggg ",
    ], new() { ['s'] = LS, ['r'] = LR, ['h'] = LH, ['g'] = LG }));

    public static PixelSprite Forest => _forest.Value;
    private static readonly Lazy<PixelSprite> _forest = new(() => PixelSprite.FromAscii([
        "  fff fff fff fff ",
        " ffffffffffffffff ",
        " ffffffffffffffff ",
        " fffoffffoffffoff ",
        "  tff fff fff ttt ",
        "  tt   t   tt  tt ",
        "  tt   t   tt  tt ",
        "  tt   t   tt  tt ",
        "                tt ",
        "                tt ",
        "               tt  ",
        "              tt   ",
    ], new() { ['f'] = FF, ['o'] = FO, ['t'] = FT }));

    public static PixelSprite Cave => _cave.Value;
    private static readonly Lazy<PixelSprite> _cave = new(() => PixelSprite.FromAscii([
        "  ssssssssssssssss ",
        " ssssssssssssssssss",
        "ssssssssssssssssss ",
        "ss  sssssssss  ss  ",
        "s  kkkkk  kkkk  s ",
        "s  kkkkk  kkkk  s ",
        "ssssssssssssssssss ",
        " ssssssssssssssss  ",
        " ssss    sssssss s ",
        " ssss    sssssss s ",
        "  ss     ssssssss  ",
        "        ssssssss   ",
    ], new() { ['s'] = CS, ['k'] = CK, ['b'] = CB }));

    public static PixelSprite Ruins => _ruins.Value;
    private static readonly Lazy<PixelSprite> _ruins = new(() => PixelSprite.FromAscii([
        "  rs  rs  rs      ",
        "  rs  rs  rs      ",
        "  rr  rr  rr      ",
        "  rr  rr  rr      ",
        "  rs  rs  rs      ",
        "  rs  rs  rs      ",
        "  rr  rr  rr      ",
        " gggggggggggggg   ",
        " gggggggggggggg   ",
        " gggggggggggggg   ",
        "gggggggggggggggg  ",
        "gggggggggggggggg  ",
    ], new() { ['r'] = RR, ['s'] = RS, ['g'] = RG }));

    public static PixelSprite Dungeon => _dungeon.Value;
    private static readonly Lazy<PixelSprite> _dungeon = new(() => PixelSprite.FromAscii([
        "dddddddddddddddddd",
        "dwwwwwwwwwwwwwwwwd",
        "dwwwwwwwwwwwwwwwwd",
        "dwwfwwwwwwwwfwwwwd",
        "dwwwwwwwwwwwwwwwwd",
        "dwwwwwwwwwwwwwwwwd",
        "dwwwwwwwwwwwwwwwwd",
        "dddddddddddddddddd",
        "dddddddddddddddddd",
        "dwwwwwwwwwwwwwwwwd",
        "dwwwwwwwwwwwwwwwwd",
        "dwwwwwwwwwwwwwwwwd",
    ], new() { ['d'] = DD, ['w'] = DW, ['f'] = DF }));

    public static PixelSprite ForId(string id) => id switch
    {
        "village" => Village,
        "forest" => Forest,
        "cave" => Cave,
        "ruins" => Ruins,
        "dungeon" => Dungeon,
        _ => Village
    };
}
