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
            // Basic bot management commands
            _commandManager.RegisterCommand("addbot", HandleAddBotCommand,
                "Add bot(s) to specified team", "<t|ct> [amount]", "@css/root", 1, 2);

            _commandManager.RegisterCommand("kickbot", HandleKickBotCommand,
                "Kick bot(s) from specified team or all", "[t|ct|all] [amount]", "@css/root", 0, 2);

            _commandManager.RegisterCommand("kickbots", HandleKickAllBotsCommand,
                "Kick all bots from the server", "", "@css/root", 0, 0);

            // Place bot command (existing)
            _commandManager.RegisterCommand("placebot", HandlePlaceBotCommand,
                "Spawn a bot with team assignment", "<team> [position]", "@css/root", 1, 2);
        }

        #region Basic Bot Commands

        /// <summary>
        /// Handle !addbot command
        /// </summary>
        private bool HandleAddBotCommand(CCSPlayerController? player, string[] args)
        {
            try
            {
                // Parse team
                var teamStr = args[0].ToLower();
                CsTeam team;

                switch (teamStr)
                {
                    case "t":
                    case "terrorist":
                    case "terrorists":
                        team = CsTeam.Terrorist;
                        break;
                    case "ct":
                    case "counterterrorist":
                    case "counter-terrorist":
                    case "counterterrorists":
                        team = CsTeam.CounterTerrorist;
                        break;
                    default:
                        if (player != null)
                            ChatUtils.PrintToPlayer(player, "Usage: !addbot <t|ct> [amount]", MessageType.Error);
                        return false;
                }

                // Parse amount (default to 1)
                int amount = 1;
                if (args.Length > 1)
                {
                    if (!int.TryParse(args[1], out amount) || amount < 1 || amount > 10)
                    {
                        if (player != null)
                            ChatUtils.PrintToPlayer(player, "Amount must be between 1 and 10", MessageType.Error);
                        return false;
                    }
                }

                // Add bots
                var teamName = team == CsTeam.CounterTerrorist ? "Counter-Terrorist" : "Terrorist";
                for (int i = 0; i < amount; i++)
                {
                    Server.ExecuteCommand("bot_add");

                    // Wait a moment for bot to be added, then assign team
                    var currentIndex = i;
                    new CounterStrikeSharp.API.Modules.Timers.Timer(0.5f + (currentIndex * 0.1f), () =>
                    {
                        AssignBotToTeam(team, null);
                    });
                }

                var message = amount == 1
                    ? $"Adding 1 bot to {teamName} team"
                    : $"Adding {amount} bots to {teamName} team";

                if (player != null)
                    ChatUtils.PrintToPlayer(player, message, MessageType.Success);
                else
                    Console.WriteLine($"[CS2Utils] {message}");

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[CS2Utils] Error in addbot command: {ex.Message}");
                if (player != null)
                    ChatUtils.PrintToPlayer(player, "Error adding bot", MessageType.Error);
                return false;
            }
        }

        /// <summary>
        /// Handle !kickbot command
        /// </summary>
        private bool HandleKickBotCommand(CCSPlayerController? player, string[] args)
        {
            try
            {
                CsTeam? targetTeam = null;
                int amount = 1;

                // Parse arguments
                if (args.Length > 0)
                {
                    var teamStr = args[0].ToLower();
                    switch (teamStr)
                    {
                        case "t":
                        case "terrorist":
                        case "terrorists":
                            targetTeam = CsTeam.Terrorist;
                            break;
                        case "ct":
                        case "counterterrorist":
                        case "counter-terrorist":
                        case "counterterrorists":
                            targetTeam = CsTeam.CounterTerrorist;
                            break;
                        case "all":
                            targetTeam = null; // Kick from any team
                            break;
                        default:
                            if (player != null)
                                ChatUtils.PrintToPlayer(player, "Usage: !kickbot [t|ct|all] [amount]", MessageType.Error);
                            return false;
                    }

                    // Parse amount
                    if (args.Length > 1)
                    {
                        if (!int.TryParse(args[1], out amount) || amount < 1 || amount > 10)
                        {
                            if (player != null)
                                ChatUtils.PrintToPlayer(player, "Amount must be between 1 and 10", MessageType.Error);
                            return false;
                        }
                    }
                }

                // Get bots to kick
                var players = Utilities.GetPlayers();
                var bots = players.Where(p => p.IsValid && p.IsBot).ToList();

                if (targetTeam.HasValue)
                {
                    bots = bots.Where(b => b.Team == targetTeam.Value).ToList();
                }

                if (bots.Count == 0)
                {
                    var teamName = targetTeam?.ToString() ?? "any team";
                    if (player != null)
                        ChatUtils.PrintToPlayer(player, $"No bots found on {teamName}", MessageType.Warning);
                    return false;
                }

                // Kick the specified amount of bots
                var botsToKick = bots.Take(amount).ToList();
                foreach (var bot in botsToKick)
                {
                    Server.ExecuteCommand($"bot_kick {bot.PlayerName}");
                }

                var teamName2 = targetTeam.HasValue
                    ? (targetTeam == CsTeam.CounterTerrorist ? "Counter-Terrorist" : "Terrorist")
                    : "all";

                var message = botsToKick.Count == 1
                    ? $"Kicked 1 bot from {teamName2} team"
                    : $"Kicked {botsToKick.Count} bots from {teamName2} team";

                if (player != null)
                    ChatUtils.PrintToPlayer(player, message, MessageType.Success);
                else
                    Console.WriteLine($"[CS2Utils] {message}");

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[CS2Utils] Error in kickbot command: {ex.Message}");
                if (player != null)
                    ChatUtils.PrintToPlayer(player, "Error kicking bot", MessageType.Error);
                return false;
            }
        }

        /// <summary>
        /// Handle !kickbots command (kick all bots)
        /// </summary>
        private bool HandleKickAllBotsCommand(CCSPlayerController? player, string[] args)
        {
            try
            {
                Server.ExecuteCommand("bot_kick all");

                var message = "Kicked all bots from the server";

                if (player != null)
                    ChatUtils.PrintToPlayer(player, message, MessageType.Success);
                else
                    Console.WriteLine($"[CS2Utils] {message}");

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[CS2Utils] Error in kickbots command: {ex.Message}");
                if (player != null)
                    ChatUtils.PrintToPlayer(player, "Error kicking bots", MessageType.Error);
                return false;
            }
        }

        #endregion

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
