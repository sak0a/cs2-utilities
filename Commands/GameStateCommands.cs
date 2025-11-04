using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Events;
using CounterStrikeSharp.API.Modules.Timers;
using CounterStrikeSharp.API.Modules.Cvars;
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
        private string _infiniteAmmoMode = "off"; // "off", "clip", "reserve", "both"
        private string _lastInfiniteAmmoMode = "clip"; // Remember last used mode for toggling

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

            // Warmup aliases
            _commandManager.RegisterCommand("warmupend", HandleEndWarmupCommand,
                "End warmup period", "[seconds]", "@css/root", 0, 1);

            _commandManager.RegisterCommand("warmupstart", HandleStartWarmupCommand,
                "Start warmup period", "[seconds]", "@css/root", 0, 1);

            // Unified warmup command
            _commandManager.RegisterCommand("warmup", HandleWarmupCommand,
                "Control warmup period", "<start|end|0|1> [seconds]", "@css/root", 1, 2);

            // Game rules commands
            _commandManager.RegisterCommand("knifedrop", HandleAllowKnifeDropCommand,
                "Toggle knife dropping", "", "@css/root", 0, 0);
            
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
                "Show grenade trajectories", "[seconds|toggle|off]", "@css/root", 0, 1);

            // Grenade limit command
            _commandManager.RegisterCommand("maxgrenades", HandleMaxGrenadesCommand,
                "Set maximum grenades per player", "<value>", "@css/root", 1, 1);

            // Infinite ammo command
            _commandManager.RegisterCommand("infiniteammo", HandleInfiniteAmmoCommand,
                "Set infinite ammo mode", "[off|clip|reserve|both]", "@css/root", 0, 1);
            _commandManager.RegisterCommand("iammo", HandleInfiniteAmmoCommand,
                "Set infinite ammo mode", "[off|clip|reserve|both]", "@css/root", 0, 1);
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

        /// <summary>
        /// Handle !warmup command (unified warmup control)
        /// </summary>
        private bool HandleWarmupCommand(CCSPlayerController? player, string[] args)
        {
            try
            {
                var action = args[0].ToLower();

                // Parse action parameter
                bool startWarmup;
                switch (action)
                {
                    case "start":
                    case "1":
                        startWarmup = true;
                        break;
                    case "end":
                    case "0":
                        startWarmup = false;
                        break;
                    default:
                        if (player != null)
                            ChatUtils.PrintToPlayer(player, "Invalid action. Use: start, end, 0, or 1", MessageType.Error);
                        return false;
                }

                // Get optional duration parameter
                string[] durationArgs = args.Length > 1 ? new[] { args[1] } : new string[0];

                // Call the appropriate handler
                if (startWarmup)
                {
                    return HandleStartWarmupCommand(player, durationArgs);
                }
                else
                {
                    return HandleEndWarmupCommand(player, durationArgs);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[CS2Utils] Error in warmup command: {ex.Message}");
                if (player != null)
                    ChatUtils.PrintToPlayer(player, "Failed to control warmup.", MessageType.Error);
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

        /// <summary>
        /// Handle !disabledamage command
        /// </summary>
        private bool HandleDisableDamageCommand(CCSPlayerController? player, string[] args)
        {
            try
            {
                _damageDisabled = !_damageDisabled;
                _stateManager.SaveState("damage_disabled", _damageDisabled);

                var statusText = _damageDisabled ? "disabled" : "enabled";
                var message = $"Damage {statusText}";

                if (player != null)
                    ChatUtils.PrintToAll($"{player.PlayerName} {statusText} damage", MessageType.Info);
                else
                    ChatUtils.PrintToAll(message, MessageType.Info);

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[CS2Utils] Error toggling damage: {ex.Message}");
                if (player != null)
                    ChatUtils.PrintToPlayer(player, "Failed to toggle damage.", MessageType.Error);
                return false;
            }
        }

        #endregion

        #region Visual Debugging Commands

        /// <summary>
        /// Handle !showimpacts command
        /// </summary>
        private bool HandleShowImpactsCommand(CCSPlayerController? player, string[] args)
        {
            try
            {
                int duration = 10; // Default 10 seconds
                if (args.Length > 0 && !int.TryParse(args[0], out duration))
                {
                    if (player != null)
                        ChatUtils.PrintToPlayer(player, "Invalid duration. Please enter seconds.", MessageType.Error);
                    return false;
                }

                // Remove cheat flag from sv_showimpacts so it works without sv_cheats
                RemoveCheatFlagFromConVar("sv_showimpacts");

                // Enable impact visualization
                Server.ExecuteCommand("sv_showimpacts 1");

                var message = $"Bullet impacts enabled for {duration} seconds";
                if (player != null)
                    ChatUtils.PrintToPlayer(player, message, MessageType.Success);
                else
                    Console.WriteLine($"[CS2Utils] {message}");

                // Set timer to disable after duration
                ClearTimer("showimpacts");
                _activeTimers["showimpacts"] = new CounterStrikeSharp.API.Modules.Timers.Timer(duration, () =>
                {
                    Server.ExecuteCommand("sv_showimpacts 0");
                    ChatUtils.PrintToAll("Bullet impacts disabled", MessageType.Info);
                    _activeTimers.Remove("showimpacts");
                });

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[CS2Utils] Error enabling show impacts: {ex.Message}");
                if (player != null)
                    ChatUtils.PrintToPlayer(player, "Failed to enable bullet impacts.", MessageType.Error);
                return false;
            }
        }

        /// <summary>
        /// Handle !grenadeview command
        /// </summary>
        private bool HandleGrenadeViewCommand(CCSPlayerController? player, string[] args)
        {
            try
            {
                // Check if it's a toggle command (no arguments or "toggle")
                if (args.Length == 0 || (args.Length == 1 && args[0].ToLower() == "toggle"))
                {
                    return ToggleGrenadeView(player);
                }

                // Check for "off" or "disable" to turn off
                if (args.Length == 1 && (args[0].ToLower() == "off" || args[0].ToLower() == "disable"))
                {
                    return DisableGrenadeView(player);
                }

                // Parse duration - handle large numbers properly
                if (!int.TryParse(args[0], out int duration) || duration < 0)
                {
                    if (player != null)
                        ChatUtils.PrintToPlayer(player, "Invalid duration. Use: !grenadeview [seconds|toggle|off]", MessageType.Error);
                    return false;
                }

                // Handle very large numbers (treat as permanent)
                if (duration > 86400) // More than 24 hours = permanent
                {
                    return EnableGrenadeViewPermanent(player);
                }

                // Remove cheat flag from sv_grenade_trajectory so it works without sv_cheats
                RemoveCheatFlagFromConVar("sv_grenade_trajectory");

                // Enable grenade trajectory visualization with timer
                Server.ExecuteCommand("sv_grenade_trajectory 1");
                _stateManager.SaveState("sv_grenade_trajectory", 1);

                var message = $"Grenade trajectories enabled for {duration} seconds";
                if (player != null)
                    ChatUtils.PrintToPlayer(player, message, MessageType.Success);
                else
                    Console.WriteLine($"[CS2Utils] {message}");

                // Set timer to disable after duration
                ClearTimer("grenadeview");
                _activeTimers["grenadeview"] = new CounterStrikeSharp.API.Modules.Timers.Timer(duration, () =>
                {
                    Server.ExecuteCommand("sv_grenade_trajectory 0");
                    _stateManager.SaveState("sv_grenade_trajectory", 0);
                    ChatUtils.PrintToAll("Grenade trajectories disabled", MessageType.Info);
                    _activeTimers.Remove("grenadeview");
                });

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[CS2Utils] Error with grenade view command: {ex.Message}");
                if (player != null)
                    ChatUtils.PrintToPlayer(player, "Failed to execute grenade view command.", MessageType.Error);
                return false;
            }
        }

        /// <summary>
        /// Toggle grenade view on/off
        /// </summary>
        private bool ToggleGrenadeView(CCSPlayerController? player)
        {
            try
            {
                // Check current state
                var currentState = _stateManager.HasState("sv_grenade_trajectory")
                    ? _stateManager.GetState<int>("sv_grenade_trajectory")
                    : 0;

                if (currentState == 1)
                {
                    return DisableGrenadeView(player);
                }
                else
                {
                    return EnableGrenadeViewPermanent(player);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[CS2Utils] Error toggling grenade view: {ex.Message}");
                if (player != null)
                    ChatUtils.PrintToPlayer(player, "Failed to toggle grenade trajectories.", MessageType.Error);
                return false;
            }
        }

        /// <summary>
        /// Enable grenade view permanently (until manually disabled)
        /// </summary>
        private bool EnableGrenadeViewPermanent(CCSPlayerController? player)
        {
            try
            {
                // Clear any existing timer
                ClearTimer("grenadeview");

                // Remove cheat flag from sv_grenade_trajectory so it works without sv_cheats
                RemoveCheatFlagFromConVar("sv_grenade_trajectory");

                // Enable grenade trajectory visualization
                Server.ExecuteCommand("sv_grenade_trajectory 1");
                _stateManager.SaveState("sv_grenade_trajectory", 1);

                var message = "Grenade trajectories enabled (permanent)";
                if (player != null)
                    ChatUtils.PrintToPlayer(player, message, MessageType.Success);
                else
                    Console.WriteLine($"[CS2Utils] {message}");

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[CS2Utils] Error enabling permanent grenade view: {ex.Message}");
                if (player != null)
                    ChatUtils.PrintToPlayer(player, "Failed to enable grenade trajectories.", MessageType.Error);
                return false;
            }
        }

        /// <summary>
        /// Disable grenade view
        /// </summary>
        private bool DisableGrenadeView(CCSPlayerController? player)
        {
            try
            {
                // Clear any existing timer
                ClearTimer("grenadeview");

                // Disable grenade trajectory visualization
                Server.ExecuteCommand("sv_grenade_trajectory 0");
                _stateManager.SaveState("sv_grenade_trajectory", 0);

                var message = "Grenade trajectories disabled";
                if (player != null)
                    ChatUtils.PrintToPlayer(player, message, MessageType.Success);
                else
                    Console.WriteLine($"[CS2Utils] {message}");

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[CS2Utils] Error disabling grenade view: {ex.Message}");
                if (player != null)
                    ChatUtils.PrintToPlayer(player, "Failed to disable grenade trajectories.", MessageType.Error);
                return false;
            }
        }

        #endregion

        #region Grenade Limit Commands

        /// <summary>
        /// Handle !maxgrenades command
        /// </summary>
        private bool HandleMaxGrenadesCommand(CCSPlayerController? player, string[] args)
        {
            try
            {
                if (!int.TryParse(args[0], out var count) || count < 0)
                {
                    if (player != null)
                        ChatUtils.PrintToPlayer(player, "Invalid count. Please enter 0 or higher.", MessageType.Error);
                    return false;
                }

                // Set limits for all grenade types
                var grenadeCommands = new[]
                {
                    "ammo_grenade_limit_flashbang",
                    "ammo_grenade_limit_he",
                    "ammo_grenade_limit_smoke",
                    "ammo_grenade_limit_decoy",
                    "ammo_grenade_limit_molotov"
                };

                foreach (var command in grenadeCommands)
                {
                    Server.ExecuteCommand($"{command} {count}");
                    _stateManager.SaveState(command, count);
                }

                var message = $"Maximum grenades set to {count}";
                if (player != null)
                    ChatUtils.PrintToPlayer(player, message, MessageType.Success);
                else
                    Console.WriteLine($"[CS2Utils] {message}");

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[CS2Utils] Error setting grenade limits: {ex.Message}");
                if (player != null)
                    ChatUtils.PrintToPlayer(player, "Failed to set grenade limits.", MessageType.Error);
                return false;
            }
        }

        #endregion

        #region Infinite Ammo Command

        /// <summary>
        /// Handle !infiniteammo command
        /// </summary>
        private bool HandleInfiniteAmmoCommand(CCSPlayerController? player, string[] args)
        {
            try
            {
                string mode;

                if (args.Length == 0)
                {
                    // Toggle mode: if off, use last mode; if on, turn off
                    mode = _infiniteAmmoMode == "off" ? _lastInfiniteAmmoMode : "off";
                }
                else
                {
                    mode = args[0].ToLower();

                    // Validate mode
                    if (mode != "off" && mode != "clip" && mode != "reserve" && mode != "both")
                    {
                        if (player != null)
                            ChatUtils.PrintToPlayer(player, "Invalid mode. Use: off, clip, reserve, both, or no argument to toggle", MessageType.Error);
                        return false;
                    }
                }

                // Remember the last non-off mode for toggling
                if (mode != "off")
                {
                    _lastInfiniteAmmoMode = mode;
                    _stateManager.SaveState("last_infinite_ammo_mode", _lastInfiniteAmmoMode);
                }

                // Apply the infinite ammo mode
                ApplyInfiniteAmmoMode(mode);

                _infiniteAmmoMode = mode;
                _stateManager.SaveState("infinite_ammo_mode", _infiniteAmmoMode);

                // Provide feedback
                var message = $"Infinite ammo mode set to: {mode}";
                if (player != null)
                    ChatUtils.PrintToPlayer(player, message, MessageType.Success);
                else
                    Console.WriteLine($"[CS2Utils] {message}");

                // Notify all players
                var players = Utilities.GetPlayers();
                foreach (var p in players)
                {
                    if (p.IsValid && !p.IsBot && p != player)
                    {
                        ChatUtils.PrintToPlayer(p, $"Server infinite ammo mode: {mode}", MessageType.Info);
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[CS2Utils] Error setting infinite ammo mode: {ex.Message}");
                if (player != null)
                    ChatUtils.PrintToPlayer(player, "Failed to set infinite ammo mode.", MessageType.Error);
                return false;
            }
        }

        /// <summary>
        /// Remove cheat flag from a console variable
        /// </summary>
        private void RemoveCheatFlagFromConVar(string convar_name)
        {
            try
            {
                // Try to find and modify the convar using CounterStrikeSharp API
                var convar = ConVar.Find(convar_name);
                if (convar != null)
                {
                    convar.Flags &= ~ConVarFlags.FCVAR_CHEAT;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[CS2Utils] Error removing cheat flag from {convar_name}: {ex.Message}");
            }
        }

        /// <summary>
        /// Apply infinite ammo mode to the server
        /// </summary>
        private void ApplyInfiniteAmmoMode(string mode)
        {
            try
            {
                // Remove cheat flags from ammo-related convars so we don't need sv_cheats 1
                RemoveCheatFlagFromConVar("sv_infinite_ammo");

                switch (mode.ToLower())
                {
                    case "off":
                        // Disable infinite ammo - restore default settings
                        Server.ExecuteCommand("sv_infinite_ammo 0");
                        Console.WriteLine("[CS2Utils] Infinite ammo disabled - normal ammo system restored");
                        break;

                    case "clip":
                        // Infinite ammo in clip (no reload needed)
                        Server.ExecuteCommand("sv_infinite_ammo 1");
                        Console.WriteLine("[CS2Utils] Infinite ammo enabled - clip mode (no reload needed)");
                        break;

                    case "reserve":
                        // Infinite reserve ammo (reload required)
                        Server.ExecuteCommand("sv_infinite_ammo 2");
                        Console.WriteLine("[CS2Utils] Infinite ammo enabled - reserve mode (reload required)");
                        break;

                    case "both":
                        // Both infinite clip and reserve (best of both worlds)
                        Server.ExecuteCommand("sv_infinite_ammo 1");
                        Console.WriteLine("[CS2Utils] Infinite ammo enabled - both clip and reserve");
                        break;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[CS2Utils] Error applying infinite ammo mode: {ex.Message}");
            }
        }

        #endregion

        #region Helper Methods and Event Hooks

        /// <summary>
        /// Clear an active timer
        /// </summary>
        private void ClearTimer(string timerName)
        {
            if (_activeTimers.TryGetValue(timerName, out var timer))
            {
                timer?.Kill();
                _activeTimers.Remove(timerName);
            }
        }

        /// <summary>
        /// Event hook for damage blocking
        /// </summary>
        public HookResult OnPlayerHurt(EventPlayerHurt @event, GameEventInfo info)
        {
            if (_damageDisabled)
            {
                // Block damage by setting it to 0
                @event.DmgHealth = 0;
                @event.DmgArmor = 0;
                return HookResult.Changed;
            }

            return HookResult.Continue;
        }

        /// <summary>
        /// Check if damage is currently disabled
        /// </summary>
        public bool IsDamageDisabled()
        {
            return _damageDisabled;
        }

        /// <summary>
        /// Check if game is currently paused
        /// </summary>
        public bool IsGamePaused()
        {
            return _gamePaused;
        }

        /// <summary>
        /// Cleanup method for plugin unload
        /// </summary>
        public void Cleanup()
        {
            try
            {
                // Clear all active timers
                foreach (var timer in _activeTimers.Values)
                {
                    timer?.Kill();
                }
                _activeTimers.Clear();

                // Reset infinite ammo to off on cleanup
                if (_infiniteAmmoMode != "off")
                {
                    ApplyInfiniteAmmoMode("off");
                }

                Console.WriteLine("[CS2Utils] Game state commands cleaned up");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[CS2Utils] Error during game state commands cleanup: {ex.Message}");
            }
        }

        #endregion
    }
}
