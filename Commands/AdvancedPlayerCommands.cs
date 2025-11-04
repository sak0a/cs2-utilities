using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Events;
using CounterStrikeSharp.API.Modules.Timers;
using CounterStrikeSharp.API.Modules.Utils;
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
        private readonly Dictionary<ulong, bool> _bhopPlayers;
        private string _defaultPrimary = "";
        private string _defaultSecondary = "";

        public AdvancedPlayerCommands(CommandManager commandManager, StateManager stateManager, PlayerTargetResolver targetResolver)
        {
            _commandManager = commandManager;
            _stateManager = stateManager;
            _targetResolver = targetResolver;
            _immunityTimers = new Dictionary<ulong, CounterStrikeSharp.API.Modules.Timers.Timer>();
            _immunePlayers = new Dictionary<ulong, bool>();
            _bhopPlayers = new Dictionary<ulong, bool>();
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
                "Toggle bunny hopping", "<player|team|all>", "@css/root", 1, 1);

            // Default weapon commands
            _commandManager.RegisterCommand("defaultprimary", HandleDefaultPrimaryCommand,
                "Set default primary weapon", "<weapon>", "@css/root", 1, 1);
            
            _commandManager.RegisterCommand("defaultsecondary", HandleDefaultSecondaryCommand,
                "Set default secondary weapon", "<weapon>", "@css/root", 1, 1);
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
                        var currentState = _bhopPlayers.ContainsKey(target.SteamID) && _bhopPlayers[target.SteamID];
                        var newState = !currentState;
                        
                        _bhopPlayers[target.SteamID] = newState;
                        
                        var statusText = newState ? "enabled" : "disabled";
                        ChatUtils.PrintToPlayer(target, $"Bunny hopping {statusText}", MessageType.Success);
                        successCount++;
                    }
                }

                // Save bhop states
                _stateManager.SaveState("bhop_players", _bhopPlayers.Where(kvp => kvp.Value).Select(kvp => kvp.Key).ToList());

                // Provide feedback
                var message = $"Toggled bunny hopping for {successCount} player(s)";
                if (player != null)
                    ChatUtils.PrintToPlayer(player, message, MessageType.Success);
                else
                    Console.WriteLine($"[CS2Utils] {message}");

                return successCount > 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[CS2Utils] Error toggling bhop: {ex.Message}");
                if (player != null)
                    ChatUtils.PrintToPlayer(player, "Failed to toggle bunny hopping.", MessageType.Error);
                return false;
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
