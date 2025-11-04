using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;
using CS2Utilities.Core;
using CS2Utilities.Models;
using CS2Utilities.Utils;

namespace CS2Utilities.Commands
{
    /// <summary>
    /// Implements comprehensive bot management system with intelligent positioning and team assignment
    /// </summary>
    public class BotCommands
    {
        private readonly CommandManager _commandManager;
        private readonly StateManager _stateManager;
        private readonly PlayerTargetResolver _targetResolver;

        public BotCommands(CommandManager commandManager, StateManager stateManager, PlayerTargetResolver targetResolver)
        {
            _commandManager = commandManager;
            _stateManager = stateManager;
            _targetResolver = targetResolver;
        }

        /// <summary>
        /// Register all bot commands with the command manager
        /// </summary>
        public void RegisterCommands()
        {
            // Place bot command
            _commandManager.RegisterCommand("placebot", HandlePlaceBotCommand,
                "Spawn a bot with team assignment", "<team> [position]", "@css/root", 1, 2);
        }

        #region Place Bot Command

        /// <summary>
        /// Handle !placebot command
        /// </summary>
        private bool HandlePlaceBotCommand(CCSPlayerController? player, string[] args)
        {
            try
            {
                // Parse team
                var teamStr = args[0].ToLower();
                CsTeam team;
                
                switch (teamStr)
                {
                    case "ct":
                    case "counterterrorist":
                    case "2":
                        team = CsTeam.CounterTerrorist;
                        break;
                    case "t":
                    case "terrorist":
                    case "3":
                        team = CsTeam.Terrorist;
                        break;
                    default:
                        if (player != null)
                            ChatUtils.PrintToPlayer(player, "Invalid team. Use: ct, t, counterterrorist, or terrorist", MessageType.Error);
                        return false;
                }

                // Get position if specified
                Vector? position = null;
                if (args.Length > 1 && player != null)
                {
                    // Use player's position as reference
                    var playerPawn = player.PlayerPawn.Value;
                    if (playerPawn != null)
                    {
                        position = playerPawn.CBodyComponent?.SceneNode?.AbsOrigin;
                    }
                }

                // Add bot using server command
                Server.ExecuteCommand("bot_add");
                
                // Wait a moment for bot to be added, then assign team
                new CounterStrikeSharp.API.Modules.Timers.Timer(0.5f, () =>
                {
                    AssignBotToTeam(team, position);
                });

                var teamName = team == CsTeam.CounterTerrorist ? "Counter-Terrorist" : "Terrorist";
                var message = $"Adding bot to {teamName} team";
                
                if (player != null)
                    ChatUtils.PrintToPlayer(player, message, MessageType.Success);
                else
                    Console.WriteLine($"[CS2Utils] {message}");

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[CS2Utils] Error placing bot: {ex.Message}");
                if (player != null)
                    ChatUtils.PrintToPlayer(player, "Failed to place bot.", MessageType.Error);
                return false;
            }
        }

        /// <summary>
        /// Assign the most recently added bot to a specific team
        /// </summary>
        private void AssignBotToTeam(CsTeam targetTeam, Vector? position)
        {
            try
            {
                var players = Utilities.GetPlayers();
                var bots = players.Where(p => p.IsValid && p.IsBot).ToList();
                
                if (bots.Count == 0)
                    return;

                // Find the most recently added bot (likely the last one in the list)
                var bot = bots.LastOrDefault();
                if (bot == null)
                    return;

                // Change team if necessary
                if (bot.Team != targetTeam)
                {
                    bot.ChangeTeam(targetTeam);
                }

                // Teleport to position if specified
                if (position != null && bot.PawnIsAlive)
                {
                    var botPawn = bot.PlayerPawn.Value;
                    if (botPawn != null)
                    {
                        botPawn.Teleport(position, new QAngle(0, 0, 0), new Vector(0, 0, 0));
                    }
                }

                Console.WriteLine($"[CS2Utils] Bot assigned to {targetTeam} team");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[CS2Utils] Error assigning bot to team: {ex.Message}");
            }
        }

        #endregion

        #region Enhanced Bot Management Integration

        /// <summary>
        /// Get current bot statistics
        /// </summary>
        public BotStatistics GetBotStatistics()
        {
            try
            {
                var players = Utilities.GetPlayers();
                var bots = players.Where(p => p.IsValid && p.IsBot).ToList();
                
                return new BotStatistics
                {
                    TotalBots = bots.Count,
                    CTBots = bots.Count(b => b.Team == CsTeam.CounterTerrorist),
                    TBots = bots.Count(b => b.Team == CsTeam.Terrorist),
                    AliveBots = bots.Count(b => b.PawnIsAlive),
                    DeadBots = bots.Count(b => !b.PawnIsAlive)
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[CS2Utils] Error getting bot statistics: {ex.Message}");
                return new BotStatistics();
            }
        }

        /// <summary>
        /// Remove all bots from the server
        /// </summary>
        public bool RemoveAllBots()
        {
            try
            {
                Server.ExecuteCommand("bot_kick");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[CS2Utils] Error removing bots: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Balance bots between teams
        /// </summary>
        public bool BalanceBots()
        {
            try
            {
                var stats = GetBotStatistics();
                var difference = Math.Abs(stats.CTBots - stats.TBots);
                
                if (difference <= 1)
                    return true; // Already balanced

                var players = Utilities.GetPlayers();
                var bots = players.Where(p => p.IsValid && p.IsBot).ToList();
                
                // Move bots to balance teams
                var sourceTeam = stats.CTBots > stats.TBots ? CsTeam.CounterTerrorist : CsTeam.Terrorist;
                var targetTeam = sourceTeam == CsTeam.CounterTerrorist ? CsTeam.Terrorist : CsTeam.CounterTerrorist;
                
                var botsToMove = bots.Where(b => b.Team == sourceTeam).Take(difference / 2).ToList();
                
                foreach (var bot in botsToMove)
                {
                    bot.ChangeTeam(targetTeam);
                }

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[CS2Utils] Error balancing bots: {ex.Message}");
                return false;
            }
        }

        #endregion

        #region State Management

        /// <summary>
        /// Restore saved bot states
        /// </summary>
        public void RestoreStates()
        {
            try
            {
                // Bot commands don't typically need state restoration
                // as bots are managed by the server directly
                Console.WriteLine("[CS2Utils] Bot commands state restoration completed");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[CS2Utils] Error restoring bot states: {ex.Message}");
            }
        }

        /// <summary>
        /// Cleanup resources and state
        /// </summary>
        public void Cleanup()
        {
            try
            {
                // No specific cleanup needed for bot commands
                Console.WriteLine("[CS2Utils] Bot commands cleaned up");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[CS2Utils] Error during bot commands cleanup: {ex.Message}");
            }
        }

        #endregion
    }

    /// <summary>
    /// Bot statistics data structure
    /// </summary>
    public class BotStatistics
    {
        public int TotalBots { get; set; }
        public int CTBots { get; set; }
        public int TBots { get; set; }
        public int AliveBots { get; set; }
        public int DeadBots { get; set; }
    }
}
