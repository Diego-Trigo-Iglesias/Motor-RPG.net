using TextRPG.Core.Constants;
using TextRPG.Core.Models;

namespace TextRPG.Core.Engine;

public sealed class CombatEngine : ICombatEngine
{
    private static readonly Random Rng = Random.Shared;

    //   SIMULACIÓN POR RONDA (nuevo: con PlayerAction)
    /// <summary>Simula una ronda completa aplicando la acción del jugador,
    /// el comportamiento del enemigo y los efectos de estado.</summary>
    public CombatRound SimulateRound(Player player, Enemy enemy, PlayerAction action)
    {
        //  1. Preparar estados 
        player.IsDefending = action == PlayerAction.Defend;
        bool isHeavy = action == PlayerAction.HeavyAttack;

        //  2. Calcular daño del jugador 
        var rawPlayerDamage = Math.Max(1, player.TotalAttack - enemy.Defense + Rng.Next(-2, 4));
        if (isHeavy)
            rawPlayerDamage = (int)(rawPlayerDamage * GameConstants.HeavyAttackMultiplier);
        if (player.IsDefending)
            rawPlayerDamage = (int)(rawPlayerDamage * GameConstants.DefendAttackReduction);

        var isCritical  = Rng.Next(100) < GameConstants.CritChance;
        var enemyDodge  = Rng.Next(100) < GameConstants.EnemyEvasionChance;
        var playerDodge = Rng.Next(100) < GameConstants.PlayerEvasionChance;

        // Aplicar defensa del enemigo (escudo)
        int playerDamage = enemyDodge ? 0 : (isCritical ? rawPlayerDamage * 2 : rawPlayerDamage);
        playerDamage = enemy.ApplyDefense(playerDamage);

        //  3. Calcular daño del enemigo 
        var rawEnemyDamage = Math.Max(1, enemy.Attack - player.TotalDefense + Rng.Next(-2, 4));

        // Aplicar ofensa del enemigo (berserker, boss charge)
        rawEnemyDamage = enemy.ApplyOffense(rawEnemyDamage);

        // Aplicar defensa del jugador
        if (player.IsDefending)
            rawEnemyDamage = (int)(rawEnemyDamage * GameConstants.DefendDamageReduction);

        int enemyDamage = playerDodge ? 0 : rawEnemyDamage;

        //  4. Veneno del enemigo (Venomous) 
        enemy.AppliedPoisonThisTurn = false;
        if (enemy.Behavior == EnemyBehavior.Venomous && !playerDodge)
        {
            player.PoisonTurnsRemaining = GameConstants.PoisonDuration;
            player.PoisonDamagePerTurn = GameConstants.PoisonDamagePerTurn;
            enemy.AppliedPoisonThisTurn = true;
        }

        //  5. Avanzar estado del enemigo 
        enemy.AdvanceBossCharge();

        //  6. Verificar enfurecimiento 
        // (se verifica ANTES de aplicar daño para que el mensaje aparezca este turno)
        enemy.CheckEnrage();

        //  7. Veneno activo del jugador (daño por turno) 
        int poisonDamage = player.ApplyPoisonDamage();

        //  8. Construir mensaje de comportamiento 
        string behaviorMsg = BuildBehaviorMessage(enemy, player);

        //  9. Icono de comportamiento 
        string behaviorIcon = GetBehaviorIcon(enemy);

        return new CombatRound(
            PlayerDamage: playerDamage,
            EnemyDamage: enemyDamage,
            PlayerDodged: playerDodge,
            EnemyDodged: enemyDodge,
            IsCritical: isCritical,
            PoisonDamage: poisonDamage,
            ShieldActive: enemy.Behavior == EnemyBehavior.Shielded && enemy.ShieldTurns > 0,
            BossChargeCounter: enemy.Behavior == EnemyBehavior.Boss ? enemy.BossChargeCounter : 0,
            BossIsUnleashing: enemy.Behavior == EnemyBehavior.Boss && enemy.BossChargeCounter == 0,
            BehaviorMessage: behaviorMsg
        )
        {
            PlayerDefended = player.IsDefending,
            WasHeavyAttack = isHeavy,
            EnemyIsEnraged = enemy.IsEnraged,
            EnemyBehaviorIcon = behaviorIcon
        };
    }

    // ══════════════════════════════════════════════════
    //   APLICAR RONDA
    // ══════════════════════════════════════════════════

    public void ApplyRound(Player player, Enemy enemy, CombatRound round)
    {
        enemy.CurrentHp  -= round.PlayerDamage;
        player.CurrentHp -= round.EnemyDamage;

        // Asegurar que no baje de 0
        if (enemy.CurrentHp < 0) enemy.CurrentHp = 0;
        if (player.CurrentHp < 0) player.CurrentHp = 0;
    }

    // ══════════════════════════════════════════════════
    //   SIMULACIÓN COMPLETA (usado por Web legacy)
    // ══════════════════════════════════════════════════

    public CombatResult SimulateFull(Player player, Enemy enemy)
    {
        int rounds = 0;
        while (player.IsAlive && enemy.IsAlive)
        {
            var round = SimulateRound(player, enemy, PlayerAction.Attack);
            ApplyRound(player, enemy, round);
            rounds++;
            if (rounds > GameConstants.MaxCombatRounds)
                return new CombatResult(false, 0, 0, rounds);
        }

        return player.IsAlive
            ? new CombatResult(true,  enemy.ExpReward,  enemy.GoldReward,  rounds)
            : new CombatResult(false, enemy.ExpReward / 4, 0, rounds);
    }

    // ══════════════════════════════════════════════════
    //   HELPERS
    // ══════════════════════════════════════════════════

    private static string BuildBehaviorMessage(Enemy enemy, Player player)
    {
        if (enemy.Behavior == EnemyBehavior.Shielded && enemy.ShieldTurns > 0)
            return enemy.ShieldTurns == 2
                ? "El escudo del enemigo brilla con energía oscura..."
                : "El escundo del enemigo comienza a agrietarse...";

        if (enemy.Behavior == EnemyBehavior.Venomous && enemy.AppliedPoisonThisTurn)
            return "El veneno corre por tus venas! (-" + GameConstants.PoisonDamagePerTurn + " HP por " + GameConstants.PoisonDuration + " turnos)";

        if (enemy.Behavior == EnemyBehavior.Berserker && enemy.IsEnraged)
            return "╔══ EL BERSERKER ENFURECE! ══╗";

        if (enemy.Behavior == EnemyBehavior.Boss)
        {
            if (enemy.BossChargeCounter == 2)
                return "El Dragón Ancestral comienza a cargar su aliento...";
            if (enemy.BossChargeCounter == 1)
                return "El Dragón acumula energía...  PREPARA TU DEFENSA!";
            if (enemy.BossChargeCounter == 0)
                return "╔══ ¡¡ALIENTO DE DRAGÓN ANCESTRAL!! ══╗";
            if (enemy.IsEnraged)
                return "╔══ EL DRAGÓN ESTÁ ENFURECIDO! ══╗";
        }

        return "";
    }

    private static string GetBehaviorIcon(Enemy enemy) => enemy.Behavior switch
    {
        EnemyBehavior.Shielded  => "🛡",
        EnemyBehavior.Venomous  => "☠",
        EnemyBehavior.Berserker => "⚡",
        EnemyBehavior.Boss      => "👑",
        _                       => ""
    };
}
