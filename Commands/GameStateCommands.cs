using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Events;
using CounterStrikeSharp.API.Modules.Timers;
using CS2Utilities.Core;
using CS2Utilities.Utils;

namespace CS2Utilities.Commands
{
    /// <summary>
    /// Implements game state manipulation commands using GameRules and event hooks
    /// </summary>
    public class GameStateCommands
    {
        private readonly CommandManager _commandManager;
        private readonly StateManager _stateManager;
        
        // Timer management for temporary states
        private readonly Dictionary<string, CounterStrikeSharp.API.Modules.Timers.Timer> _activeTimers;
        
        // Game state tracking
        private bool _damageDisabled = false;
        private bool _gamePaused = false;

        public GameStateCommands(CommandManager commandManager, StateManager stateManager)
        {
            _commandManager = commandManager;
            _stateManager = stateManager;
            _activeTimers = new Dictionary<string, CounterStrikeSharp.API.Modules.Timers.Timer>();
        }

        /// <summary>
        /// Register all game state commands with the command manager
        /// </summary>
        public void RegisterCommands()
        {
            // Pause/unpause commands
            _commandManager.RegisterCommand("pause", HandlePauseCommand,
                "Pause the game", "", "@css/root", 0, 0);
            
            _commandManager.RegisterCommand("unpause", HandleUnpauseCommand,
                "Unpause the game", "", "@css/root", 0, 0);

            // Warmup control commands
            _commandManager.RegisterCommand("endwarmup", HandleEndWarmupCommand,
                "End warmup period", "[seconds]", "@css/root", 0, 1);
            
            _commandManager.RegisterCommand("startwarmup", HandleStartWarmupCommand,
                "Start warmup period", "[seconds]", "@css/root", 0, 1);

            // Game rules commands
            _commandManager.RegisterCommand("allowknifedrop", HandleAllowKnifeDropCommand,
                "Toggle knife dropping", "", "@css/root", 0, 0);
            
            _commandManager.RegisterCommand("friendlyfire", HandleFriendlyFireCommand,
                "Toggle friendly fire", "[team]", "@css/root", 0, 1);
            
            _commandManager.RegisterCommand("disabledamage", HandleDisableDamageCommand,
                "Toggle damage on/off", "", "@css/root", 0, 0);

            // Visual debugging commands
            _commandManager.RegisterCommand("showimpacts", HandleShowImpactsCommand,
                "Show bullet impacts", "[seconds]", "@css/root", 0, 1);
            
            _commandManager.RegisterCommand("grenadeview", HandleGrenadeViewCommand,
                "Show grenade trajectories", "[seconds]", "@css/root", 0, 1);

            // Grenade limit command
            _commandManager.RegisterCommand("maxgrenades", HandleMaxGrenadesCommand,
                "Set maximum grenades per player", "<value>", "@css/root", 1, 1);
        }

        #region Pause/Unpause Commands

        /// <summary>
        /// Handle !pause command
        /// </summary>
        private bool HandlePauseCommand(CCSPlayerController? player, string[] args)
        {
            try
            {
                if (_gamePaused)
                {
                    if (player != null)
                        ChatUtils.PrintToPlayer(player, "Game is already paused.", MessageType.Warning);
                    return false;
                }

                // Use console command for reliable pause
                Server.ExecuteCommand("mp_pause_match");
                _gamePaused = true;
                _stateManager.SaveState("game_paused", true);

                var message = "Game paused";
                if (player != null)
                {
                    ChatUtils.PrintToAll($"{player.PlayerName} paused the game", MessageType.Info);
                }
                else
                {
                    ChatUtils.PrintToAll(message, MessageType.Info);
                    Console.WriteLine($"[CS2Utils] {message}");
                }

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[CS2Utils] Error pausing game: {ex.Message}");
                if (player != null)
                    ChatUtils.PrintToPlayer(player, "Failed to pause game.", MessageType.Error);
                return false;
            }
        }

        /// <summary>
        /// Handle !unpause command
        /// </summary>
        private bool HandleUnpauseCommand(CCSPlayerController? player, string[] args)
        {
            try
            {
                if (!_gamePaused)
                {
                    if (player != null)
                        ChatUtils.PrintToPlayer(player, "Game is not paused.", MessageType.Warning);
                    return false;
                }

                // Use console command for reliable unpause
                Server.ExecuteCommand("mp_unpause_match");
                _gamePaused = false;
                _stateManager.SaveState("game_paused", false);

                var message = "Game unpaused";
                if (player != null)
                {
                    ChatUtils.PrintToAll($"{player.PlayerName} unpaused the game", MessageType.Success);
                }
                else
                {
                    ChatUtils.PrintToAll(message, MessageType.Success);
                    Console.WriteLine($"[CS2Utils] {message}");
                }

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[CS2Utils] Error unpausing game: {ex.Message}");
                if (player != null)
                    ChatUtils.PrintToPlayer(player, "Failed to unpause game.", MessageType.Error);
                return false;
            }
        }

        #endregion

        #region Warmup Control Commands

        /// <summary>
        /// Handle !endwarmup command
        /// </summary>
        private bool HandleEndWarmupCommand(CCSPlayerController? player, string[] args)
        {
            try
            {
                int delay = 0;
                if (args.Length > 0 && !int.TryParse(args[0], out delay))
                {
                    if (player != null)
                        ChatUtils.PrintToPlayer(player, "Invalid delay. Please enter seconds (0 or higher).", MessageType.Error);
                    return false;
                }

                if (delay > 0)
                {
                    var message = $"Ending warmup in {delay} seconds...";
                    if (player != null)
                        ChatUtils.PrintToAll($"{player.PlayerName} is ending warmup in {delay} seconds", MessageType.Info);
                    else
                        ChatUtils.PrintToAll(message, MessageType.Info);

                    // Create timer to end warmup after delay
                    var timer = new CounterStrikeSharp.API.Modules.Timers.Timer(delay, () =>
                    {
                        Server.ExecuteCommand("mp_warmup_end");
                        ChatUtils.PrintToAll("Warmup ended", MessageType.Success);
                    });
                    
                    return true;
                }
                else
                {
                    // End warmup immediately
                    Server.ExecuteCommand("mp_warmup_end");
                    
                    var message = "Warmup ended";
                    if (player != null)
                        ChatUtils.PrintToAll($"{player.PlayerName} ended warmup", MessageType.Success);
                    else
                        ChatUtils.PrintToAll(message, MessageType.Success);
                    
                    return true;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[CS2Utils] Error ending warmup: {ex.Message}");
                if (player != null)
                    ChatUtils.PrintToPlayer(player, "Failed to end warmup.", MessageType.Error);
                return false;
            }
        }

        /// <summary>
        /// Handle !startwarmup command
        /// </summary>
        private bool HandleStartWarmupCommand(CCSPlayerController? player, string[] args)
        {
            try
            {
                int duration = 0;
                if (args.Length > 0 && !int.TryParse(args[0], out duration))
                {
                    if (player != null)
                        ChatUtils.PrintToPlayer(player, "Invalid duration. Please enter seconds (0 or higher).", MessageType.Error);
                    return false;
                }

                // Set warmup time if specified
                if (duration > 0)
                {
                    Server.ExecuteCommand($"mp_warmuptime {duration}");
                }

                // Start warmup
                Server.ExecuteCommand("mp_warmup_start");
                
                var message = duration > 0 ? $"Warmup started ({duration} seconds)" : "Warmup started";
                if (player != null)
                    ChatUtils.PrintToAll($"{player.PlayerName} started warmup" + (duration > 0 ? $" ({duration}s)" : ""), MessageType.Success);
                else
                    ChatUtils.PrintToAll(message, MessageType.Success);

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[CS2Utils] Error starting warmup: {ex.Message}");
                if (player != null)
                    ChatUtils.PrintToPlayer(player, "Failed to start warmup.", MessageType.Error);
                return false;
            }
        }

        #endregion

        #region Game Rules Commands

        /// <summary>
        /// Handle !allowknifedrop command
        /// </summary>
        private bool HandleAllowKnifeDropCommand(CCSPlayerController? player, string[] args)
        {
            try
            {
                var currentValue = _stateManager.GetState("mp_drop_knife_enable", 0);
                var newValue = currentValue == 1 ? 0 : 1;
                var statusText = newValue == 1 ? "enabled" : "disabled";

                Server.ExecuteCommand($"mp_drop_knife_enable {newValue}");
                _stateManager.SaveState("mp_drop_knife_enable", newValue);

                var message = $"Knife dropping {statusText}";
                if (player != null)
                    ChatUtils.PrintToPlayer(player, message, MessageType.Success);
                else
                    Console.WriteLine($"[CS2Utils] {message}");

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[CS2Utils] Error toggling knife drop: {ex.Message}");
                if (player != null)
                    ChatUtils.PrintToPlayer(player, "Failed to toggle knife dropping.", MessageType.Error);
                return false;
            }
        }

        /// <summary>
        /// Handle !friendlyfire command
        /// </summary>
        private bool HandleFriendlyFireCommand(CCSPlayerController? player, string[] args)
        {
            try
            {
                var currentValue = _stateManager.GetState("mp_friendlyfire", 0);
                var newValue = currentValue == 1 ? 0 : 1;
                var statusText = newValue == 1 ? "enabled" : "disabled";

                Server.ExecuteCommand($"mp_friendlyfire {newValue}");
                _stateManager.SaveState("mp_friendlyfire", newValue);

                var message = $"Friendly fire {statusText}";
                if (player != null)
                    ChatUtils.PrintToPlayer(player, message, MessageType.Success);
                else
                    Console.WriteLine($"[CS2Utils] {message}");

                // TODO: Implement team-specific friendly fire logic if args[0] specifies team
                if (args.Length > 0)
                {
                    var team = args[0].ToLower();
                    if (team == "ct" || team == "t")
                    {
                        if (player != null)
                            ChatUtils.PrintToPlayer(player, "Team-specific friendly fire not yet implemented.", MessageType.Warning);
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[CS2Utils] Error toggling friendly fire: {ex.Message}");
                if (player != null)
                    ChatUtils.PrintToPlayer(player, "Failed to toggle friendly fire.", MessageType.Error);
                return false;
            }
        }

        #endregion
