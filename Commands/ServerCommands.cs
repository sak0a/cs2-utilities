using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Cvars;
using CS2Utilities.Core;
using CS2Utilities.Utils;
using System;
using System.Linq;

namespace CS2Utilities.Commands
{
    /// <summary>
    /// Implements console command-based server administration commands
    /// </summary>
    public class ServerCommands
    {
        private readonly CommandManager _commandManager;
        private readonly StateManager _stateManager;

        public ServerCommands(CommandManager commandManager, StateManager stateManager)
        {
            _commandManager = commandManager;
            _stateManager = stateManager;
        }

        /// <summary>
        /// Register all server commands with the command manager
        /// </summary>
        public void RegisterCommands()
        {
            // Money-related commands
            _commandManager.RegisterCommand("maxmoney", HandleMaxMoneyCommand,
                "Set maximum money players can have", "<amount>", "@css/root", 1, 1);
            
            _commandManager.RegisterCommand("startmoney", HandleStartMoneyCommand,
                "Set starting money for players", "<amount>", "@css/root", 1, 1);
            
            _commandManager.RegisterCommand("afterroundmoney", HandleAfterRoundMoneyCommand,
                "Set money awarded after round end", "<amount>", "@css/root", 1, 1);

            // Time-related commands
            _commandManager.RegisterCommand("warmuptime", HandleWarmupTimeCommand,
                "Set warmup duration", "<seconds|default>", "@css/root", 1, 1);
            
            _commandManager.RegisterCommand("roundtime", HandleRoundTimeCommand,
                "Set round time in minutes", "<minutes>", "@css/root", 1, 1);
            
            _commandManager.RegisterCommand("roundfreezetime", HandleRoundFreezeTimeCommand,
                "Set freeze time at round start", "<seconds>", "@css/root", 1, 1);
            
            _commandManager.RegisterCommand("roundrestartdelay", HandleRoundRestartDelayCommand,
                "Set delay before round restart", "<seconds>", "@css/root", 1, 1);
            
            _commandManager.RegisterCommand("buytime", HandleBuyTimeCommand,
                "Set buy time duration", "<seconds>", "@css/root", 1, 1);

            // Team and bot commands
            _commandManager.RegisterCommand("autoteambalance", HandleAutoTeamBalanceCommand,
                "Toggle automatic team balancing", "", "@css/root", 0, 0);
            
            _commandManager.RegisterCommand("limitteams", HandleLimitTeamsCommand,
                "Set team size difference limit", "<value>", "@css/root", 1, 1);
            
            _commandManager.RegisterCommand("botdifficulty", HandleBotDifficultyCommand,
                "Set bot difficulty level", "<0-3>", "@css/root", 1, 1);
            
            _commandManager.RegisterCommand("botquota", HandleBotQuotaCommand,
                "Set number of bots", "<count>", "@css/root", 1, 1);

            // Utility commands
            _commandManager.RegisterCommand("buyanywhere", HandleBuyAnywhereCommand,
                "Toggle buy anywhere on the map", "", "@css/root", 0, 0);
            
            _commandManager.RegisterCommand("changemap", HandleChangeMapCommand,
                "Change to specified map (supports workshop maps)", "<mapname|workshop/ID|workshopID|steamURL>", "@css/root", 1, 1);

            _commandManager.RegisterCommand("listmaps", HandleListMapsCommand,
                "List available workshop maps", "", "@css/root", 0, 0);
        }

        #region Money Commands

        /// <summary>
        /// Handle !maxmoney command
        /// </summary>
        private bool HandleMaxMoneyCommand(CCSPlayerController? player, string[] args)
        {
            if (!int.TryParse(args[0], out var amount) || amount < 0)
            {
                if (player != null)
                    ChatUtils.PrintToPlayer(player, "Invalid amount. Please enter a positive number.", MessageType.Error);
                return false;
            }

            return ExecuteConsoleCommand(player, "mp_maxmoney", amount, "Maximum money");
        }

        /// <summary>
        /// Handle !startmoney command
        /// </summary>
        private bool HandleStartMoneyCommand(CCSPlayerController? player, string[] args)
        {
            if (!int.TryParse(args[0], out var amount) || amount < 0)
            {
                if (player != null)
                    ChatUtils.PrintToPlayer(player, "Invalid amount. Please enter a positive number.", MessageType.Error);
                return false;
            }

            return ExecuteConsoleCommand(player, "mp_startmoney", amount, "Starting money");
        }

        /// <summary>
        /// Handle !afterroundmoney command
        /// </summary>
        private bool HandleAfterRoundMoneyCommand(CCSPlayerController? player, string[] args)
        {
            if (!int.TryParse(args[0], out var amount) || amount < 0)
            {
                if (player != null)
                    ChatUtils.PrintToPlayer(player, "Invalid amount. Please enter a positive number.", MessageType.Error);
                return false;
            }

            return ExecuteConsoleCommand(player, "mp_afterroundmoney", amount, "After-round money");
        }

        #endregion

        #region Time Commands

        /// <summary>
        /// Handle !warmuptime command
        /// </summary>
        private bool HandleWarmupTimeCommand(CCSPlayerController? player, string[] args)
        {
            var input = args[0].ToLower();
            
            if (input == "default")
            {
                return ExecuteConsoleCommand(player, "mp_warmuptime", 30, "Warmup time");
            }
            
            if (!int.TryParse(input, out var seconds) || seconds < 0)
            {
                if (player != null)
                    ChatUtils.PrintToPlayer(player, "Invalid time. Use 'default' or enter seconds (0 or higher).", MessageType.Error);
                return false;
            }

            return ExecuteConsoleCommand(player, "mp_warmuptime", seconds, "Warmup time");
        }

        /// <summary>
        /// Handle !roundtime command
        /// </summary>
        private bool HandleRoundTimeCommand(CCSPlayerController? player, string[] args)
        {
            if (!float.TryParse(args[0], out var minutes) || minutes <= 0)
            {
                if (player != null)
                    ChatUtils.PrintToPlayer(player, "Invalid time. Please enter minutes (greater than 0).", MessageType.Error);
                return false;
            }

            // CS2 uses separate commands for defuse and hostage maps
            var success1 = ExecuteConsoleCommand(player, "mp_roundtime_defuse", minutes, null, false);
            var success2 = ExecuteConsoleCommand(player, "mp_roundtime_hostage", minutes, null, false);
            
            if (success1 && success2)
            {
                if (player != null)
                    ChatUtils.PrintToPlayer(player, $"Round time set to {minutes} minutes", MessageType.Success);
                else
                    Console.WriteLine($"[CS2Utils] Round time set to {minutes} minutes");
                return true;
            }
            
            return false;
        }

        /// <summary>
        /// Handle !roundfreezetime command
        /// </summary>
        private bool HandleRoundFreezeTimeCommand(CCSPlayerController? player, string[] args)
        {
            if (!int.TryParse(args[0], out var seconds) || seconds < 0)
            {
                if (player != null)
                    ChatUtils.PrintToPlayer(player, "Invalid time. Please enter seconds (0 or higher).", MessageType.Error);
                return false;
            }

            return ExecuteConsoleCommand(player, "mp_freezetime", seconds, "Round freeze time");
        }

        /// <summary>
        /// Handle !roundrestartdelay command
        /// </summary>
        private bool HandleRoundRestartDelayCommand(CCSPlayerController? player, string[] args)
        {
            if (!int.TryParse(args[0], out var seconds) || seconds < 0)
            {
                if (player != null)
                    ChatUtils.PrintToPlayer(player, "Invalid time. Please enter seconds (0 or higher).", MessageType.Error);
                return false;
            }

            return ExecuteConsoleCommand(player, "mp_round_restart_delay", seconds, "Round restart delay");
        }

        /// <summary>
        /// Handle !buytime command
        /// </summary>
        private bool HandleBuyTimeCommand(CCSPlayerController? player, string[] args)
        {
            if (!int.TryParse(args[0], out var seconds) || seconds < 0)
            {
                if (player != null)
                    ChatUtils.PrintToPlayer(player, "Invalid time. Please enter seconds (0 or higher).", MessageType.Error);
                return false;
            }

            return ExecuteConsoleCommand(player, "mp_buytime", seconds, "Buy time");
        }

        #endregion

        #region Team and Bot Commands

        /// <summary>
        /// Handle !autoteambalance command
        /// </summary>
        private bool HandleAutoTeamBalanceCommand(CCSPlayerController? player, string[] args)
        {
            var currentValue = _stateManager.GetState("mp_autoteambalance", 1);
            var newValue = currentValue == 1 ? 0 : 1;
            var statusText = newValue == 1 ? "enabled" : "disabled";
            
            return ExecuteConsoleCommand(player, "mp_autoteambalance", newValue, $"Auto team balance {statusText}");
        }

        /// <summary>
        /// Handle !limitteams command
        /// </summary>
        private bool HandleLimitTeamsCommand(CCSPlayerController? player, string[] args)
        {
            if (!int.TryParse(args[0], out var value) || value < 0)
            {
                if (player != null)
                    ChatUtils.PrintToPlayer(player, "Invalid value. Please enter 0 or higher.", MessageType.Error);
                return false;
            }

            return ExecuteConsoleCommand(player, "mp_limitteams", value, "Team limit");
        }

        /// <summary>
        /// Handle !botdifficulty command
        /// </summary>
        private bool HandleBotDifficultyCommand(CCSPlayerController? player, string[] args)
        {
            if (!int.TryParse(args[0], out var difficulty) || difficulty < 0 || difficulty > 3)
            {
                if (player != null)
                    ChatUtils.PrintToPlayer(player, "Invalid difficulty. Use 0 (Easy), 1 (Normal), 2 (Hard), or 3 (Expert).", MessageType.Error);
                return false;
            }

            var difficultyNames = new[] { "Easy", "Normal", "Hard", "Expert" };
            return ExecuteConsoleCommand(player, "bot_difficulty", difficulty, $"Bot difficulty ({difficultyNames[difficulty]})");
        }

        /// <summary>
        /// Handle !botquota command
        /// </summary>
        private bool HandleBotQuotaCommand(CCSPlayerController? player, string[] args)
        {
            if (!int.TryParse(args[0], out var count) || count < 0)
            {
                if (player != null)
                    ChatUtils.PrintToPlayer(player, "Invalid count. Please enter 0 or higher.", MessageType.Error);
                return false;
            }

            return ExecuteConsoleCommand(player, "bot_quota", count, "Bot quota");
        }

        #endregion

        #region Utility Commands

        /// <summary>
        /// Handle !buyanywhere command
        /// </summary>
        private bool HandleBuyAnywhereCommand(CCSPlayerController? player, string[] args)
        {
            var currentValue = _stateManager.GetState("mp_buy_anywhere", 0);
            var newValue = currentValue == 1 ? 0 : 1;
            var statusText = newValue == 1 ? "enabled" : "disabled";

            return ExecuteConsoleCommand(player, "mp_buy_anywhere", newValue, $"Buy anywhere {statusText}");
        }

        /// <summary>
        /// Handle !changemap command with workshop map support
        /// </summary>
        private bool HandleChangeMapCommand(CCSPlayerController? player, string[] args)
        {
            var input = args[0].Trim();

            if (string.IsNullOrWhiteSpace(input))
            {
                if (player != null)
                    ChatUtils.PrintToPlayer(player, "Usage: !changemap <mapname|workshop/ID|workshopID|steamURL>", MessageType.Error);
                return false;
            }

            try
            {
                var mapCommand = ProcessMapInput(input);
                if (mapCommand == null)
                {
                    if (player != null)
                        ChatUtils.PrintToPlayer(player, "Invalid map format. Use: mapname, workshop/123456789, or Steam Workshop URL", MessageType.Error);
                    return false;
                }

                // Simple approach - let CS2's built-in map configs handle settings
                // Users can create csgo/cfg/{mapname}.cfg files for per-map settings

                // Provide simple feedback
                var mapName = IsWorkshopMap(mapCommand) ? ExtractWorkshopId(mapCommand) : mapCommand;
                var message = $"Changing to map: {mapName}";

                if (player != null)
                    ChatUtils.PrintToPlayer(player, message, MessageType.Info);
                else
                    Console.WriteLine($"[CS2Utils] {message}");

                // Use the appropriate command based on map type
                if (IsWorkshopMap(mapCommand))
                {
                    // For workshop maps, use ds_workshop_changelevel
                    var workshopId = ExtractWorkshopId(mapCommand);
                    Server.ExecuteCommand($"ds_workshop_changelevel {workshopId}");
                }
                else
                {
                    // For regular maps, use changelevel
                    Server.ExecuteCommand($"changelevel {mapCommand}");
                }

                // Note about map configs
                Console.WriteLine($"[CS2Utils] Map change initiated. CS2 will automatically load csgo/cfg/{mapName}.cfg if it exists.");

                // Don't save map changes to state as they're one-time actions
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[CS2Utils] Error changing map: {ex.Message}");
                if (player != null)
                    ChatUtils.PrintToPlayer(player, "Failed to change map.", MessageType.Error);
                return false;
            }
        }

        /// <summary>
        /// Process various map input formats and return the proper changelevel command
        /// </summary>
        private string? ProcessMapInput(string input)
        {
            // Already in workshop format (workshop/123456789)
            if (input.StartsWith("workshop/", StringComparison.OrdinalIgnoreCase))
            {
                var parts = input.Split('/');
                if (parts.Length == 2 && IsValidWorkshopId(parts[1]))
                    return input;
                return null;
            }

            // Steam Workshop URL
            if (input.Contains("steamcommunity.com") && input.Contains("filedetails"))
            {
                var workshopId = ExtractWorkshopIdFromUrl(input);
                return workshopId != null ? $"workshop/{workshopId}" : null;
            }

            // Direct workshop ID (just numbers)
            if (IsValidWorkshopId(input))
            {
                return $"workshop/{input}";
            }

            // Check if it's a regular CS2 map (starts with known prefixes)
            var regularMapPrefixes = new[] { "de_", "cs_", "ar_", "dz_", "gd_", "training1", "lobby_" };
            var isRegularMap = regularMapPrefixes.Any(prefix => input.StartsWith(prefix, StringComparison.OrdinalIgnoreCase));

            if (isRegularMap)
            {
                // Regular map name - validate it doesn't contain invalid characters
                if (IsValidMapName(input))
                {
                    return input;
                }
            }
            else
            {
                // Likely a workshop map name (like "1v1v1v1", "aim_botz", etc.)
                if (IsValidMapName(input))
                {
                    return $"workshop/{input}";
                }
            }

            return null;
        }

        /// <summary>
        /// Check if input is a valid workshop ID (numeric and reasonable length)
        /// </summary>
        private bool IsValidWorkshopId(string input)
        {
            return long.TryParse(input, out var id) && id > 0 && input.Length >= 8 && input.Length <= 12;
        }

        /// <summary>
        /// Extract workshop ID from Steam Workshop URL
        /// </summary>
        private string? ExtractWorkshopIdFromUrl(string url)
        {
            try
            {
                // Simple regex approach to extract ID from Steam Workshop URL
                var match = System.Text.RegularExpressions.Regex.Match(url, @"[?&]id=(\d+)");
                if (match.Success)
                {
                    var id = match.Groups[1].Value;
                    return IsValidWorkshopId(id) ? id : null;
                }
                return null;
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Check if map command is for a workshop map
        /// </summary>
        private bool IsWorkshopMap(string mapCommand)
        {
            return mapCommand.StartsWith("workshop/", StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Extract workshop ID from workshop map command
        /// </summary>
        private string ExtractWorkshopId(string mapCommand)
        {
            if (IsWorkshopMap(mapCommand))
            {
                var parts = mapCommand.Split('/');
                return parts.Length == 2 ? parts[1] : mapCommand;
            }
            return mapCommand;
        }

        /// <summary>
        /// Validate regular map name format
        /// </summary>
        private bool IsValidMapName(string mapName)
        {
            // Basic validation - no spaces, reasonable length, alphanumeric + underscore/dash
            return !string.IsNullOrWhiteSpace(mapName) &&
                   mapName.Length <= 64 &&
                   mapName.All(c => char.IsLetterOrDigit(c) || c == '_' || c == '-') &&
                   !mapName.StartsWith("-") && !mapName.EndsWith("-");
        }





        #endregion

        #region Helper Methods

        /// <summary>
        /// Execute a console command with state persistence and user feedback
        /// </summary>
        /// <param name="player">Player who executed the command</param>
        /// <param name="consoleCommand">Console command to execute</param>
        /// <param name="value">Value to set</param>
        /// <param name="displayName">Human-readable name for feedback</param>
        /// <param name="showFeedback">Whether to show feedback message</param>
        /// <returns>True if successful</returns>
        private bool ExecuteConsoleCommand(CCSPlayerController? player, string consoleCommand, object value,
            string? displayName = null, bool showFeedback = true)
        {
            try
            {
                // Execute the console command
                Server.ExecuteCommand($"{consoleCommand} {value}");

                // Save state for persistence
                _stateManager.SaveState(consoleCommand, value);

                // Provide feedback if requested
                if (showFeedback && displayName != null)
                {
                    var message = $"{displayName} set to {value}";
                    if (player != null)
                        ChatUtils.PrintToPlayer(player, message, MessageType.Success);
                    else
                        Console.WriteLine($"[CS2Utils] {message}");
                }

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[CS2Utils] Error executing command {consoleCommand}: {ex.Message}");
                if (player != null && showFeedback)
                    ChatUtils.PrintToPlayer(player, $"Failed to execute command.", MessageType.Error);
                return false;
            }
        }

        #endregion

        #region Public Command Execution Methods

        /// <summary>
        /// Execute max money command (for console command support)
        /// </summary>
        public bool ExecuteMaxMoneyCommand(CCSPlayerController? player, string[] args)
        {
            return HandleMaxMoneyCommand(player, args);
        }

        /// <summary>
        /// Execute start money command (for console command support)
        /// </summary>
        public bool ExecuteStartMoneyCommand(CCSPlayerController? player, string[] args)
        {
            return HandleStartMoneyCommand(player, args);
        }

        /// <summary>
        /// Handle !listmaps command - lists available workshop maps
        /// </summary>
        private bool HandleListMapsCommand(CCSPlayerController? player, string[] args)
        {
            try
            {
                // Execute the console command to get workshop maps
                Server.ExecuteCommand("ds_workshop_listmaps");

                var message = "Workshop maps list requested. Check server console for detailed output.";

                if (player != null)
                    ChatUtils.PrintToPlayer(player, message, MessageType.Info);
                else
                    Console.WriteLine($"[CS2Utils] {message}");

                // Also show configured favorites if any
                var config = _stateManager.GetConfig();
                if (config.WorkshopMapFavorites.Count > 0)
                {
                    var favoritesList = new List<string>();
                    favoritesList.Add("=== Configured Favorite Workshop Maps ===");

                    foreach (var favorite in config.WorkshopMapFavorites)
                    {
                        var mapInfo = favorite.Value;
                        favoritesList.Add($"• {mapInfo.Name} (ID: {favorite.Key}) - {mapInfo.Category}");
                        favoritesList.Add($"  Command: !changemap {favorite.Key}");
                    }

                    // Output favorites list
                    foreach (var line in favoritesList)
                    {
                        if (player != null)
                            ChatUtils.PrintToPlayer(player, line, MessageType.Info);
                        else
                            Console.WriteLine($"[CS2Utils] {line}");
                    }
                }
                else
                {
                    var noFavoritesMsg = "No favorite workshop maps configured. Add them to your config for quick access.";
                    if (player != null)
                        ChatUtils.PrintToPlayer(player, noFavoritesMsg, MessageType.Info);
                    else
                        Console.WriteLine($"[CS2Utils] {noFavoritesMsg}");
                }

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[CS2Utils] Error listing workshop maps: {ex.Message}");
                if (player != null)
                    ChatUtils.PrintToPlayer(player, "Failed to list workshop maps.", MessageType.Error);
                return false;
            }
        }

        #endregion
    }
}
