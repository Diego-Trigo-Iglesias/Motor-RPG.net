using System;
using TextRPG;

try
{
    using var game = new Game();
    game.Run();
}
catch (Exception ex)
{
    Console.Error.WriteLine($"Error fatal: {ex.Message}");
    Console.Error.WriteLine(ex.StackTrace);
    Environment.Exit(1);
}
