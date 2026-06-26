namespace TextRPG.Core.PixelArt;

/// Paisajes 32x16 pixel-art detallados para cada localización.
/// Cada sprite usa 5-7 colores para profundidad y atmósfera.
public static class LocationSprites
{
    // ─── ALDEA: atardecer, casas con tejados rojos, ventanas iluminadas, árboles ─────
    private static readonly PixelColor Village_Sky    = new(70, 130, 220);
    private static readonly PixelColor Village_SkyD   = new(50, 100, 190);
    private static readonly PixelColor Village_Roof   = new(190, 75, 40);
    private static readonly PixelColor Village_Wall   = new(200, 165, 105);
    private static readonly PixelColor Village_Window = new(255, 210, 60);
    private static readonly PixelColor Village_Grass  = new(55, 155, 50);
    private static readonly PixelColor Village_GrassD = new(35, 120, 35);
    private static readonly PixelColor Village_Path   = new(120, 90, 55);
    private static readonly PixelColor Village_Trunk  = new(85, 55, 30);

    // ─── BOSQUE: follaje denso, troncos, claroscuros, hongos ─────────────────────────
    private static readonly PixelColor Forest_FoliageL = new(50, 160, 45);
    private static readonly PixelColor Forest_FoliageM = new(35, 125, 35);
    private static readonly PixelColor Forest_FoliageD = new(20, 90, 20);
    private static readonly PixelColor Forest_Trunk    = new(85, 55, 30);
    private static readonly PixelColor Forest_TrunkD   = new(55, 35, 20);
    private static readonly PixelColor Forest_Ground   = new(25, 65, 25);
    private static readonly PixelColor Forest_GroundL  = new(40, 90, 40);
    private static readonly PixelColor Forest_MushC    = new(210, 110, 60);
    private static readonly PixelColor Forest_MushS    = new(230, 200, 160);

    // ─── CUEVA: roca, estalactitas, cristales brillantes, agua ───────────────────────
    private static readonly PixelColor Cave_StoneL  = new(105, 95, 85);
    private static readonly PixelColor Cave_StoneM  = new(75, 68, 58);
    private static readonly PixelColor Cave_Dark    = new(40, 35, 30);
    private static readonly PixelColor Cave_Darker  = new(22, 18, 15);
    private static readonly PixelColor Cave_Crystal = new(55, 135, 230);
    private static readonly PixelColor Cave_CryGlow = new(120, 200, 255);
    private static readonly PixelColor Cave_Water   = new(12, 22, 45);
    private static readonly PixelColor Cave_Tip     = new(85, 78, 68);

    // ─── RUINAS: columnas caídas, piedra, atardecer, vegetación ──────────────────────
    private static readonly PixelColor Ruins_Stone    = new(115, 105, 90);
    private static readonly PixelColor Ruins_StoneL   = new(155, 145, 125);
    private static readonly PixelColor Ruins_Brick    = new(140, 120, 95);
    private static readonly PixelColor Ruins_Sunset   = new(230, 170, 70);
    private static readonly PixelColor Ruins_SunsetD  = new(180, 120, 45);
    private static readonly PixelColor Ruins_Grass    = new(70, 135, 55);
    private static readonly PixelColor Ruins_GrassD   = new(45, 100, 35);
    private static readonly PixelColor Ruins_Debris   = new(90, 80, 70);

    // ─── MAZMORRA: muros oscuros, rejas, fuego, ambiente opresivo ────────────────────
    private static readonly PixelColor Dungeon_DarkW   = new(35, 28, 42);
    private static readonly PixelColor Dungeon_MidW    = new(50, 42, 58);
    private static readonly PixelColor Dungeon_Stone   = new(68, 60, 75);
    private static readonly PixelColor Dungeon_Fire    = new(215, 105, 30);
    private static readonly PixelColor Dungeon_FireGlow = new(255, 180, 40);
    private static readonly PixelColor Dungeon_Bar     = new(85, 78, 85);
    private static readonly PixelColor Dungeon_BarL    = new(125, 115, 125);

    // ─── DELTA: pantano, agua oscura, juncos, árboles retorcidos, neblina ────────────
    private static readonly PixelColor Delta_WaterD  = new(12, 32, 28);
    private static readonly PixelColor Delta_WaterL  = new(22, 52, 40);
    private static readonly PixelColor Delta_Grass   = new(32, 72, 38);
    private static readonly PixelColor Delta_GrassL  = new(55, 105, 52);
    private static readonly PixelColor Delta_Trunk   = new(72, 50, 35);
    private static readonly PixelColor Delta_TrunkD  = new(45, 30, 20);
    private static readonly PixelColor Delta_Reed    = new(105, 85, 45);
    private static readonly PixelColor Delta_ReedG   = new(60, 100, 40);
    private static readonly PixelColor Delta_Firefly = new(210, 205, 80);
    private static readonly PixelColor Delta_Mist    = new(130, 175, 165);
    private static readonly PixelColor Delta_Sky     = new(32, 22, 45);

    // ═══════════════════════════════════════════════════════════════════════════════
    //  SPRITES 32x16
    // ═══════════════════════════════════════════════════════════════════════════════

    // ─── ALDEA ────────────────────────────────────────────────────────────────────
    public static PixelSprite Village => _village.Value;
    private static readonly Lazy<PixelSprite> _village = new(() => PixelSprite.FromAscii([
        "                                ",
        "        sssss   sssss          ",
        "      sssssssss sssssssss      ",
        "      ssrrrssss ssrrrssss      ",
        "      sshhhssss sshhhssss      ",
        "      sswwhssss sswwhssss      ",
        "      sshhhssss sshhhssss      ",
        "      sssssssss sssssssss      ",
        "     gggggggggggggggggggg      ",
        "    gggggggggggggggggggggg     ",
        "   ggggpggggggggggggpgggggg    ",
        "   ggggpggggggggggggpgggggg    ",
        "  gggggpggggtttttggpggggggg    ",
        "  ttttggggtttttttggggttttt     ",
        "  ttttttttttttttttttttttt      ",
        "   ttttttttttttttttttttt       ",
    ], new() {
        ['s'] = Village_Sky, ['S'] = Village_SkyD,
        ['r'] = Village_Roof, ['h'] = Village_Wall, ['w'] = Village_Window,
        ['g'] = Village_Grass, ['G'] = Village_GrassD, ['p'] = Village_Path,
        ['t'] = Village_Trunk
    }));

    // ─── BOSQUE ───────────────────────────────────────────────────────────────────
    public static PixelSprite Forest => _forest.Value;
    private static readonly Lazy<PixelSprite> _forest = new(() => PixelSprite.FromAscii([
        "   fff fff fff fff fff ffff   ",
        "  ffffffffffffffffffffffffff  ",
        "  ffffffffffffffffffffffffff  ",
        " fffoffffoffffoffffoffffoffff ",
        " fffoffffoffffoffffoffffoffff ",
        " ffffffffffffffffffffffffffff ",
        "  tff fff fff fff fff    tt  ",
        "  tt   t   t   t   t    tt   ",
        "  tt   t   t   t   t   tt    ",
        "  ttt m t   t   t m t tt     ",
        "   tttt    t   t    tttt     ",
        "   ttttt  ttt ttt  ttttt     ",
        "    tttttttttttttttttttt     ",
        "    gggggggggggggggggggg     ",
        "   gggggggggggggggggggggg    ",
        "   gggggggggggggggggggggg    ",
    ], new() {
        ['f'] = Forest_FoliageL, ['F'] = Forest_FoliageM, ['o'] = Forest_FoliageD,
        ['t'] = Forest_Trunk, ['T'] = Forest_TrunkD,
        ['g'] = Forest_Ground, ['G'] = Forest_GroundL,
        ['m'] = Forest_MushC, ['M'] = Forest_MushS
    }));

    // ─── CUEVA ────────────────────────────────────────────────────────────────────
    public static PixelSprite Cave => _cave.Value;
    private static readonly Lazy<PixelSprite> _cave = new(() => PixelSprite.FromAscii([
        "  SSS  SSS  SSS  SSS  SSSS   ",
        "  SSSSSSSSSSSSSSSSSSSSSSSS   ",
        " SSSSSSttSSSSSSttSSSSSSSSSS  ",
        " SSSStttSSSSSStttSSSSSSSSSS  ",
        " SSSkkkSSSSSSkkkSSSSSSkkkSS  ",
        " SSSkkkSSSSSSkkkSSSSSSkkkSS  ",
        "  SSSSSSSSSSSSSSSSSSSSSSSS  ",
        "  SSSSSSSSSSSSSSSSSSSSSSSS  ",
        "   SSSSSSSkSSkSSkSSSSSSSS   ",
        "   SSSSkkccckkccckkkSSSSS   ",
        "   SSSkccCccccCccckkkSSSS   ",
        "    SSSSkkcccccckkkSSSSS    ",
        "    SSSSSSkkkkkkkSSSSSSS    ",
        "    wwwwwwwwwwwwwwwwwwww    ",
        "   wwwwwwwwwwwwwwwwwwwwww   ",
        "   wwwwwwwwwwwwwwwwwwwwww   ",
    ], new() {
        ['s'] = Cave_StoneL, ['S'] = Cave_StoneM, ['t'] = Cave_Tip,
        ['k'] = Cave_Dark, ['K'] = Cave_Darker,
        ['c'] = Cave_Crystal, ['C'] = Cave_CryGlow,
        ['w'] = Cave_Water
    }));

    // ─── RUINAS ───────────────────────────────────────────────────────────────────
    public static PixelSprite Ruins => _ruins.Value;
    private static readonly Lazy<PixelSprite> _ruins = new(() => PixelSprite.FromAscii([
        "       ssssssssssssssssss     ",
        "     sssssssssssssssssssss   ",
        "    rrrrr  sssss  rrrrr rr   ",
        "    rrrrr  sssss  rrrrr rr   ",
        "    rrrrr  sssss  rrrrr      ",
        "    rrrrrrrrrrrrrrrrrrr      ",
        "     rrrrrrrrrrrrrrrrrr      ",
        "    gggggrrdgggrddrggggg     ",
        "    gggggggggggggggggggg     ",
        "   gggggggggggggggggggggg    ",
        "  ggggrrggggggggggggrrggg   ",
        "  ggggrrggggggggggggrrggg   ",
        "  gggggggggrrrggggggggggg   ",
        "  ggggggggrrrrrgggggggggg   ",
        "   gggggggrrrrrggggggggg    ",
        "    ggggggggggggggggggg     ",
    ], new() {
        ['r'] = Ruins_Stone, ['R'] = Ruins_StoneL, ['b'] = Ruins_Brick,
        ['s'] = Ruins_Sunset, ['S'] = Ruins_SunsetD,
        ['g'] = Ruins_Grass, ['G'] = Ruins_GrassD,
        ['d'] = Ruins_Debris
    }));

    // ─── MAZMORRA ────────────────────────────────────────────────────────────────
    public static PixelSprite Dungeon => _dungeon.Value;
    private static readonly Lazy<PixelSprite> _dungeon = new(() => PixelSprite.FromAscii([
        "dddddddddddddddddddddddddddddd",
        "dDDDDDDDDDDDDDDDDDDDDDDDDDDdd",
        "dDDDwwwwwwwwwwwwwwwwwwwwDDDdd",
        "dDDDwwwwwwwwwwwwwwwwwwwwDDDdd",
        "dDDDwwffwwwwwwwwwwffwwwwDDDdd",
        "dDDDwwwwwwwwwwwwwwwwwwwwDDDdd",
        "dDDDwwwwwwwwwwwwwwwwwwwwDDDdd",
        "dDDDwwwwwwBwwBwwwwwwwwDDDDdd",
        "ddDDDDDDDDDDDDDDDDDDDDDDDDdd",
        "ddDDDDDDDDDDDDDDDDDDDDDDDDdd",
        " ddDDDwwwwwwwwwwwwwwwwDDDdd  ",
        " ddDDDwwwwwwwwwwwwwwwwDDDdd  ",
        "  ddDDDwwwwffwwwwffwwDDDdd  ",
        "  ddDDDwwwwwwwwwwwwwwDDDdd  ",
        "   dddDDDDDDDDDDDDDDDDddd   ",
        "   dddddddddddddddddddddd   ",
    ], new() {
        ['d'] = Dungeon_DarkW, ['D'] = Dungeon_MidW, ['w'] = Dungeon_Stone,
        ['f'] = Dungeon_Fire, ['F'] = Dungeon_FireGlow,
        ['b'] = Dungeon_Bar, ['B'] = Dungeon_BarL
    }));

    // ─── DELTA (pantano) ─────────────────────────────────────────────────────────
    public static PixelSprite Delta => _delta.Value;
    private static readonly Lazy<PixelSprite> _delta = new(() => PixelSprite.FromAscii([
        "   sssssssssssssssssssssssss  ",
        "  ssssssssssssssssssssssssss ",
        " sssssssssssssssssssssssssss ",
        " sssssssssssssssssssssssssss ",
        "    fff      rrrr   rrrr     ",
        "   tttt    rrrrrr rrrrrr     ",
        "   tr rt   rrrrrr rrrrrr     ",
        "   t  tt   rrrrrr rrrrrr     ",
        "  t    t   ggg    ggg       ",
        "  t   tt  ggggg  ggggg     ",
        "  tttt   gggggggggggggg     ",
        "   tt   ggggggggggggggggg   ",
        "       wwwwwwwwwwwwwwwwww   ",
        "      wwwwwwwwwwwwwwwwwww   ",
        "     wwwwwwwwwwwwwwwwwwww   ",
        "    mmmmm  wwwwwww  mmmmm   ",
    ], new() {
        ['s'] = Delta_Sky,
        ['w'] = Delta_WaterD, ['W'] = Delta_WaterL,
        ['g'] = Delta_Grass, ['G'] = Delta_GrassL,
        ['t'] = Delta_Trunk, ['T'] = Delta_TrunkD,
        ['r'] = Delta_Reed, ['R'] = Delta_ReedG,
        ['f'] = Delta_Firefly,
        ['m'] = Delta_Mist
    }));

    // ═══════════════════════════════════════════════════════════════════════════════
    //  DISPATCHER
    // ═══════════════════════════════════════════════════════════════════════════════

    public static PixelSprite ForId(string id) => id switch
    {
        "village" => Village,
        "forest"  => Forest,
        "cave"    => Cave,
        "ruins"   => Ruins,
        "delta"   => Delta,
        "dungeon" => Dungeon,
        _ => Village
    };
}
