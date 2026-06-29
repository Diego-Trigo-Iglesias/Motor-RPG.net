namespace TextRPG.Core.PixelArt;

public static class EffectSprites
{
    private static readonly PixelColor SW = new(230, 230, 240);
    private static readonly PixelColor CY = new(255, 220, 50);
    private static readonly PixelColor CO = new(255, 160, 30);
    private static readonly PixelColor HG = new(80, 220, 80);
    private static readonly PixelColor HL = new(160, 255, 160);
    private static readonly PixelColor VY = new(255, 210, 40);
    private static readonly PixelColor FR = new(255, 60, 20);
    private static readonly PixelColor FO = new(255, 140, 30);
    private static readonly PixelColor DC = new(100, 200, 220);

    /// Corte 16×8.
    public static PixelSprite Slash => _slash.Value;
    private static readonly Lazy<PixelSprite> _slash = new(() => PixelSprite.FromAscii([
        "          sssss   ",
        "       sssssssss  ",
        "   sssssssssssssss",
        " sssssssssssssssss",
        "sssssssssssssssss ",
        " sssssssssssssss  ",
        "   sssssssssss    ",
        "       ssss       ",
    ], new() { ['s'] = SW }));

    /// Explosión crítica 16×12.
    public static PixelSprite CriticalHit => _crit.Value;
    private static readonly Lazy<PixelSprite> _crit = new(() => PixelSprite.FromAscii([
        "    y  y  y       ",
        "   yoyoyoyoy      ",
        "  yyyyyyyyyyy     ",
        "  yoyoyoyoyoy     ",
        " yyyyyyyyyyyyy    ",
        " yyyyyyyyyyyyy    ",
        "  yoyoyoyoyoy     ",
        "  yyyyyyyyyyy     ",
        "   yoyoyoyoy      ",
        "    yyyyyyy       ",
        "     yyyyy        ",
        "      yyy         ",
    ], new() { ['y'] = CY, ['o'] = CO }));

    /// Curación 12×10.
    public static PixelSprite Heal => _heal.Value;
    private static readonly Lazy<PixelSprite> _heal = new(() => PixelSprite.FromAscii([
        "      g           ",
        "     ggg          ",
        "    ggggg         ",
        "   ggggggg        ",
        "  ggggggggg       ",
        "  lllllllll       ",
        "   lllllll        ",
        "    lllll         ",
        "     lll          ",
        "      l           ",
    ], new() { ['g'] = HG, ['l'] = HL }));

    /// Fuego 14×10.
    public static PixelSprite Fire => _fire.Value;
    private static readonly Lazy<PixelSprite> _fire = new(() => PixelSprite.FromAscii([
        "       r          ",
        "      rrr         ",
        "     ororo        ",
        "    rorrro        ",
        "   rorrrrr        ",
        "   rrorrro        ",
        "    rrrrr         ",
        "    rrrrr         ",
        "     rrr          ",
        "      r           ",
    ], new() { ['r'] = FR, ['o'] = FO }));

    /// Esquiva 12×8.
    public static PixelSprite Dodge => _dodge.Value;
    private static readonly Lazy<PixelSprite> _dodge = new(() => PixelSprite.FromAscii([
        "    cccccc        ",
        "   cccccccc       ",
        "  ccc  cccc       ",
        " ccc    cccc      ",
        " ccc    cccc      ",
        "  ccc  cccc       ",
        "   cccccccc       ",
        "    cccccc        ",
    ], new() { ['c'] = DC }));

    /// Victoria 14×10.
    public static PixelSprite Victory => _victory.Value;
    private static readonly Lazy<PixelSprite> _victory = new(() => PixelSprite.FromAscii([
        " y y y y y y      ",
        "yyyyyyyyyyyyy     ",
        " yyyyyyyyyyy      ",
        "  yyyyyyyyy       ",
        " yyyyyyyyyyy      ",
        "yyyyyyyyyyyyy     ",
        " y y y y y y      ",
        " y y y y y y      ",
        "   yyyyyyy        ",
        "    yyyyy         ",
    ], new() { ['y'] = VY }));
}
