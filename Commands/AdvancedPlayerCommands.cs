using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Events;
using CounterStrikeSharp.API.Modules.Timers;
using CounterStrikeSharp.API.Modules.Utils;
using CounterStrikeSharp.API.Modules.Cvars;
using CS2Utilities.Core;
using CS2Utilities.Models;
using CS2Utilities.Utils;

namespace CS2Utilities.Commands
{
    /// <summary>
    /// Implements advanced player manipulation commands with complex state tracking and event hooks
    /// </summary>
    public class AdvancedPlayerCommands
    {
        private readonly CommandManager _commandManager;
        private readonly StateManager _stateManager;
        private readonly PlayerTargetResolver _targetResolver;
        
        // Advanced state tracking
        private readonly Dictionary<ulong, CounterStrikeSharp.API.Modules.Timers.Timer> _immunityTimers;
        private readonly Dictionary<ulong, bool> _immunePlayers;
        private readonly Dictionary<ulong, bool> _godModePlayers;
        private string _bhopMode = "off"; // "off", "matchmaking", "supernatural"
        private string _lastBhopMode = "matchmaking"; // Remember last used mode for toggling
        private string _defaultPrimary = "";
        private string _defaultSecondary = "";

        public AdvancedPlayerCommands(CommandManager commandManager, StateManager stateManager, PlayerTargetResolver targetResolver)
        {
            _commandManager = commandManager;
            _stateManager = stateManager;
            _targetResolver = targetResolver;
            _immunityTimers = new Dictionary<ulong, CounterStrikeSharp.API.Modules.Timers.Timer>();
            _immunePlayers = new Dictionary<ulong, bool>();
            _godModePlayers = new Dictionary<ulong, bool>();
        }

        /// <summary>
        /// Register all advanced player commands with the command manager
        /// </summary>
        public void RegisterCommands()
        {
            // Instant respawn command
            _commandManager.RegisterCommand("instantrespawn", HandleInstantRespawnCommand,
                "Instantly respawn players", "<player|team|all>", "@css/root", 1, 1);

            // Respawn immunity command
            _commandManager.RegisterCommand("respawnimmunity", HandleRespawnImmunityCommand,
                "Grant temporary damage immunity", "<player|team|all> [seconds]", "@css/root", 1, 2);

            // Bunny hop command
            _commandManager.RegisterCommand("bhop", HandleBhopCommand,
                "Set bunny hopping mode", "[off|matchmaking|supernatural]", "@css/root", 0, 1);

            // Default weapon commands
            _commandManager.RegisterCommand("defaultprimary", HandleDefaultPrimaryCommand,
                "Set default primary weapon", "<weapon>", "@css/root", 1, 1);

            _commandManager.RegisterCommand("defaultsecondary", HandleDefaultSecondaryCommand,
                "Set default secondary weapon", "<weapon>", "@css/root", 1, 1);

            // God mode command
            _commandManager.RegisterCommand("god", HandleGodModeCommand,
                "Toggle god mode for yourself", "", "@css/root", 0, 0);
        }

        #region Instant Respawn Command

        /// <summary>
        /// Handle !instantrespawn command
        /// </summary>
        private bool HandleInstantRespawnCommand(CCSPlayerController? player, string[] args)
        {
            try
            {
                var targets = _targetResolver.ResolveTargets(args[0], player);
                if (targets == null || !targets.IsValid)
                {
                    if (player != null)
                        ChatUtils.PrintToPlayer(player, targets?.ErrorMessage ?? "Invalid target.", MessageType.Error);
                    return false;
                }

                var successCount = 0;
                foreach (var target in targets.Players)
                {
                    if (target.IsValid && !target.IsBot && !target.PawnIsAlive)
                    {
                        // Respawn the player
                        target.Respawn();
                        successCount++;
                    }
                }

                // Provide feedback
                var message = $"Respawned {successCount} player(s)";
                if (player != null)
                    ChatUtils.PrintToPlayer(player, message, MessageType.Success);
                else
                    Console.WriteLine($"[CS2Utils] {message}");

                return successCount > 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[CS2Utils] Error respawning players: {ex.Message}");
                if (player != null)
                    ChatUtils.PrintToPlayer(player, "Failed to respawn players.", MessageType.Error);
                return false;
            }
        }

        #endregion

        #region Respawn Immunity Command

        /// <summary>
        /// Handle !respawnimmunity command
        /// </summary>
        private bool HandleRespawnImmunityCommand(CCSPlayerController? player, string[] args)
        {
            try
            {
                int duration = 5; // Default 5 seconds
                if (args.Length > 1 && !int.TryParse(args[1], out duration))
                {
                    if (player != null)
                        ChatUtils.PrintToPlayer(player, "Invalid duration. Please enter seconds.", MessageType.Error);
                    return false;
                }

                var targets = _targetResolver.ResolveTargets(args[0], player);
                if (targets == null || !targets.IsValid)
                {
                    if (player != null)
                        ChatUtils.PrintToPlayer(player, targets?.ErrorMessage ?? "Invalid target.", MessageType.Error);
                    return false;
                }

                var successCount = 0;
                foreach (var target in targets.Players)
                {
                    if (target.IsValid && !target.IsBot && target.PawnIsAlive)
                    {
                        // Clear existing immunity timer
                        if (_immunityTimers.ContainsKey(target.SteamID))
                        {
                            _immunityTimers[target.SteamID]?.Kill();
                            _immunityTimers.Remove(target.SteamID);
                        }

                        // Grant immunity
                        _immunePlayers[target.SteamID] = true;

                        // Set timer to remove immunity
                        _immunityTimers[target.SteamID] = new CounterStrikeSharp.API.Modules.Timers.Timer(duration, () =>
                        {
                            _immunePlayers.Remove(target.SteamID);
                            _immunityTimers.Remove(target.SteamID);
                            
                            if (target.IsValid)
                                ChatUtils.PrintToPlayer(target, "Damage immunity expired", MessageType.Info);
                        });

                        ChatUtils.PrintToPlayer(target, $"Damage immunity granted for {duration} seconds", MessageType.Success);
                        successCount++;
                    }
                }

                // Provide feedback
                var message = $"Granted immunity to {successCount} player(s) for {duration} seconds";
                if (player != null)
                    ChatUtils.PrintToPlayer(player, message, MessageType.Success);
                else
                    Console.WriteLine($"[CS2Utils] {message}");

                return successCount > 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[CS2Utils] Error granting immunity: {ex.Message}");
                if (player != null)
                    ChatUtils.PrintToPlayer(player, "Failed to grant immunity.", MessageType.Error);
                return false;
            }
        }

        #endregion

        #region Bunny Hop Command

        /// <summary>
        /// Handle !bhop command
        /// </summary>
        private bool HandleBhopCommand(CCSPlayerController? player, string[] args)
        {
            try
            {
                string mode;

                if (args.Length == 0)
                {
                    // Toggle mode: if off, use last mode; if on, turn off
                    mode = _bhopMode == "off" ? _lastBhopMode : "off";
                }
                else
                {
                    mode = args[0].ToLower();

                    // Validate mode
                    if (mode != "off" && mode != "matchmaking" && mode != "supernatural")
                    {
                        if (player != null)
                            ChatUtils.PrintToPlayer(player, "Invalid mode. Use: off, matchmaking, supernatural, or no argument to toggle", MessageType.Error);
                        return false;
                    }
                }

                // Remember the last non-off mode for toggling
                if (mode != "off")
                {
                    _lastBhopMode = mode;
                    _stateManager.SaveState("last_bhop_mode", _lastBhopMode);
                }

                // Apply the bhop mode
                ApplyBhopMode(mode);

                _bhopMode = mode;
                _stateManager.SaveState("bhop_mode", _bhopMode);

                // Provide feedback
                var message = $"Bunny hopping mode set to: {mode}";
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
                        ChatUtils.PrintToPlayer(p, $"Server bunny hopping mode: {mode}", MessageType.Info);
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[CS2Utils] Error setting bhop mode: {ex.Message}");
                if (player != null)
                    ChatUtils.PrintToPlayer(player, "Failed to set bunny hopping mode.", MessageType.Error);
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
        /// Apply bunny hopping mode to the server
        /// </summary>
        private void ApplyBhopMode(string mode)
        {
            try
            {
                // Remove cheat flags from bhop-related convars so we don't need sv_cheats 1
                RemoveCheatFlagFromConVar("sv_enablebunnyhopping");
                RemoveCheatFlagFromConVar("sv_maxvelocity");
                RemoveCheatFlagFromConVar("sv_autobunnyhopping");
                RemoveCheatFlagFromConVar("sv_airaccelerate");
                RemoveCheatFlagFromConVar("sv_accelerate_use_weapon_speed");

                switch (mode.ToLower())
                {
                    case "off":
                        // Disable bunny hopping - restore default settings
                        Server.ExecuteCommand("sv_enablebunnyhopping 0");
                        Server.ExecuteCommand("sv_maxvelocity 3500");
                        Server.ExecuteCommand("sv_staminamax 80");
                        Server.ExecuteCommand("sv_staminalandcost 0.050");
                        Server.ExecuteCommand("sv_staminajumpcost 0.080");
                        Server.ExecuteCommand("sv_accelerate_use_weapon_speed 1");
                        Server.ExecuteCommand("sv_staminarecoveryrate 60");
                        Server.ExecuteCommand("sv_autobunnyhopping 0");
                        Server.ExecuteCommand("sv_airaccelerate 12");
                        Console.WriteLine("[CS2Utils] Bunny hopping disabled - normal settings restored");
                        break;

                    case "matchmaking":
                        // Normal bunny hopping - competitive settings
                        Server.ExecuteCommand("sv_enablebunnyhopping 1");
                        Server.ExecuteCommand("sv_maxvelocity 3500");
                        Server.ExecuteCommand("sv_staminamax 0");
                        Server.ExecuteCommand("sv_staminalandcost 0.050");
                        Server.ExecuteCommand("sv_staminajumpcost 0.080");
                        Server.ExecuteCommand("sv_accelerate_use_weapon_speed 0");
                        Server.ExecuteCommand("sv_staminarecoveryrate 0");
                        Server.ExecuteCommand("sv_autobunnyhopping 1");
                        Server.ExecuteCommand("sv_airaccelerate 12");
                        Console.WriteLine("[CS2Utils] Bunny hopping enabled - matchmaking mode");
                        break;

                    case "supernatural":
                        // Fast/uncapped bunny hopping - fun mode
                        Server.ExecuteCommand("sv_enablebunnyhopping 1");
                        Server.ExecuteCommand("sv_maxvelocity 7000");
                        Server.ExecuteCommand("sv_staminamax 0");
                        Server.ExecuteCommand("sv_staminalandcost 0");
                        Server.ExecuteCommand("sv_staminajumpcost 0");
                        Server.ExecuteCommand("sv_accelerate_use_weapon_speed 0");
                        Server.ExecuteCommand("sv_staminarecoveryrate 0");
                        Server.ExecuteCommand("sv_autobunnyhopping 1");
                        Server.ExecuteCommand("sv_airaccelerate 2000");
                        Console.WriteLine("[CS2Utils] Bunny hopping enabled - supernatural mode");
                        break;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[CS2Utils] Error applying bhop mode: {ex.Message}");
            }
        }

        #endregion

        #region Default Weapon Commands

        /// <summary>
        /// Handle !defaultprimary command
        /// </summary>
        private bool HandleDefaultPrimaryCommand(CCSPlayerController? player, string[] args)
        {
            try
            {
                var weapon = args[0].ToLower();
                
                // Validate weapon
                if (!IsValidWeapon(weapon, true))
                {
                    if (player != null)
                        ChatUtils.PrintToPlayer(player, "Invalid primary weapon. Use: ak47, m4a4, m4a1, awp, etc.", MessageType.Error);
                    return false;
                }

                _defaultPrimary = weapon;
                _stateManager.SaveState("default_primary", _defaultPrimary);

                var message = $"Default primary weapon set to {weapon}";
                if (player != null)
                    ChatUtils.PrintToPlayer(player, message, MessageType.Success);
                else
                    Console.WriteLine($"[CS2Utils] {message}");

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[CS2Utils] Error setting default primary: {ex.Message}");
                if (player != null)
                    ChatUtils.PrintToPlayer(player, "Failed to set default primary weapon.", MessageType.Error);
                return false;
            }
        }

        /// <summary>
        /// Handle !defaultsecondary command
        /// </summary>
        private bool HandleDefaultSecondaryCommand(CCSPlayerController? player, string[] args)
        {
            try
            {
                var weapon = args[0].ToLower();
                
                // Validate weapon
                if (!IsValidWeapon(weapon, false))
                {
                    if (player != null)
                        ChatUtils.PrintToPlayer(player, "Invalid secondary weapon. Use: glock, usp, p250, deagle, etc.", MessageType.Error);
                    return false;
                }

                _defaultSecondary = weapon;
                _stateManager.SaveState("default_secondary", _defaultSecondary);

                var message = $"Default secondary weapon set to {weapon}";
                if (player != null)
                    ChatUtils.PrintToPlayer(player, message, MessageType.Success);
                else
                    Console.WriteLine($"[CS2Utils] {message}");

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[CS2Utils] Error setting default secondary: {ex.Message}");
                if (player != null)
                    ChatUtils.PrintToPlayer(player, "Failed to set default secondary weapon.", MessageType.Error);
                return false;
            }
        }

        /// <summary>
        /// Validate weapon name
        /// </summary>
        private bool IsValidWeapon(string weapon, bool isPrimary)
        {
            if (isPrimary)
            {
                var primaryWeapons = new[]
                {
                    "ak47", "m4a4", "m4a1", "m4a1_silencer", "awp", "ssg08", "g3sg1", "scar20",
                    "famas", "galil", "aug", "sg556", "p90", "mp7", "mp9", "mac10", "ump45",
                    "bizon", "negev", "m249", "nova", "xm1014", "sawedoff", "mag7"
                };
                return primaryWeapons.Contains(weapon);
            }
            else
            {
                var secondaryWeapons = new[]
                {
                    "glock", "usp", "usp_silencer", "p250", "fiveseven", "tec9", "cz75a",
                    "deagle", "revolver", "dualberettas"
                };
                return secondaryWeapons.Contains(weapon);
            }
        }

        #endregion

        #region God Mode Command

        /// <summary>
        /// Handle !god command - executes the actual god console command for the player
        /// </summary>
        private bool HandleGodModeCommand(CCSPlayerController? player, string[] args)
        {
            try
            {
                // Only works for players, not console
                if (player == null)
                {
                    Console.WriteLine("[CS2Utils] God command can only be used by players");
                    return false;
                }

                // Player must be alive
                if (!player.PawnIsAlive)
                {
                    ChatUtils.PrintToPlayer(player, "You must be alive to use god mode", MessageType.Error);
                    return false;
                }

                // Remove cheat flag from god command so it works without sv_cheats
                RemoveCheatFlagFromConVar("god");

                // Execute the actual god command for the player
                player.ExecuteClientCommand("god");

                // Track god mode state for feedback (toggle tracking)
                var isCurrentlyGod = _godModePlayers.ContainsKey(player.SteamID);

                if (isCurrentlyGod)
                {
                    _godModePlayers.Remove(player.SteamID);
                    ChatUtils.PrintToPlayer(player, "God mode toggled", MessageType.Info);
                }
                else
                {
                    _godModePlayers[player.SteamID] = true;
                    ChatUtils.PrintToPlayer(player, "God mode toggled", MessageType.Info);
                }

                // Save state
                _stateManager.SaveState("god_mode_players", _godModePlayers.Keys.ToList());

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[CS2Utils] Error executing god command: {ex.Message}");
                if (player != null)
                    ChatUtils.PrintToPlayer(player, "Failed to execute god command", MessageType.Error);
                return false;
            }
        }

        #endregion

        #region Event Hooks

        /// <summary>
        /// Event hook for damage immunity and god mode
        /// </summary>
        public HookResult OnPlayerHurt(EventPlayerHurt @event, GameEventInfo info)
        {
            var victim = @event.Userid;
            if (victim != null)
            {
                var steamId = victim.SteamID;

                // Check for temporary immunity or god mode
                if (_immunePlayers.ContainsKey(steamId) ||
                    (_godModePlayers.ContainsKey(steamId) && _godModePlayers[steamId]))
                {
                    // Block damage by setting it to 0
                    @event.DmgHealth = 0;
                    @event.DmgArmor = 0;
                    return HookResult.Changed;
                }
            }

            return HookResult.Continue;
        }

        /// <summary>
        /// Event hook for player spawn to apply default weapons
        /// </summary>
        public HookResult OnPlayerSpawn(EventPlayerSpawn @event, GameEventInfo info)
        {
            var player = @event.Userid;
            if (player != null && player.IsValid && !player.IsBot)
            {
                // Apply default weapons after a short delay
                new CounterStrikeSharp.API.Modules.Timers.Timer(0.1f, () =>
                {
                    ApplyDefaultWeapons(player);
                });
            }

            return HookResult.Continue;
        }

        /// <summary>
        /// Apply default weapons to a player
        /// </summary>
        private void ApplyDefaultWeapons(CCSPlayerController player)
        {
            try
            {
                if (!player.IsValid || !player.PawnIsAlive)
                    return;

                // Give default primary weapon
                if (!string.IsNullOrEmpty(_defaultPrimary))
                {
                    player.GiveNamedItem($"weapon_{_defaultPrimary}");
                }

                // Give default secondary weapon
                if (!string.IsNullOrEmpty(_defaultSecondary))
                {
                    player.GiveNamedItem($"weapon_{_defaultSecondary}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[CS2Utils] Error applying default weapons: {ex.Message}");
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
                // Clear all immunity timers
                foreach (var timer in _immunityTimers.Values)
                {
                    timer?.Kill();
                }
                _immunityTimers.Clear();
                _immunePlayers.Clear();
                _godModePlayers.Clear();

                // Reset bhop mode to off on cleanup
                if (_bhopMode != "off")
                {
                    ApplyBhopMode("off");
                }

                Console.WriteLine("[CS2Utils] Advanced player commands cleaned up");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[CS2Utils] Error during advanced player commands cleanup: {ex.Message}");
            }
        }

        /// <summary>
        /// Cleanup player-specific data when they disconnect
        /// </summary>
        public void CleanupPlayer(ulong steamId)
        {
            try
            {
                // Remove immunity timer if exists
                if (_immunityTimers.ContainsKey(steamId))
                {
                    _immunityTimers[steamId]?.Kill();
                    _immunityTimers.Remove(steamId);
                }

                // Remove immunity status
                _immunePlayers.Remove(steamId);

                // Remove god mode status
                _godModePlayers.Remove(steamId);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[CS2Utils] Error cleaning up player {steamId}: {ex.Message}");
            }
        }

        #endregion
    }
}
