using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;
using CounterStrikeSharp.API.Modules.Cvars;
using CS2Utilities.Config;
using CS2Utilities.Core;
using CS2Utilities.Extensions;
using CS2Utilities.Models;
using CS2Utilities.Utils;

namespace CS2Utilities.Commands
{
    /// <summary>
    /// Implements player manipulation commands using CCSPlayerController API
    /// </summary>
    public class PlayerCommands
    {
        private readonly CommandManager _commandManager;
        private readonly StateManager _stateManager;
        private readonly PlayerTargetResolver _targetResolver;
        private readonly CS2UtilitiesConfig _config;

        // Player state tracking
        private readonly Dictionary<ulong, bool> _frozenPlayers;
        private readonly Dictionary<ulong, bool> _infiniteMoneyPlayers;
        private readonly Dictionary<ulong, bool> _noclipPlayers;

        public PlayerCommands(CommandManager commandManager, StateManager stateManager, PlayerTargetResolver targetResolver, CS2UtilitiesConfig config)
        {
            _commandManager = commandManager;
            _stateManager = stateManager;
            _targetResolver = targetResolver;
            _config = config;
            _frozenPlayers = new Dictionary<ulong, bool>();
            _infiniteMoneyPlayers = new Dictionary<ulong, bool>();
            _noclipPlayers = new Dictionary<ulong, bool>();
        }

        /// <summary>
        /// Register all player commands with the command manager
        /// </summary>
        public void RegisterCommands()
        {
            // Health command
            _commandManager.RegisterCommand("health", HandleHealthCommand,
                "Set player health", "<player|team|all> <value> OR <value>", "@css/root", 1, 2);

            // Kill command
            _commandManager.RegisterCommand("kill", HandleKillCommand,
                "Kill players", "<player|team|all> OR (no args for self)", "@css/root", 0, 1);

            // Teleport commands
            _commandManager.RegisterCommand("tp", HandleTeleportCommand,
                "Teleport players", "<fromplayer|fromteam|all> [toplayer] OR <toplayer>", "@css/root", 1, 2);
            
            _commandManager.RegisterCommand("teleport", HandleTeleportCommand,
                "Teleport players", "<fromplayer|fromteam|all> [toplayer] OR <toplayer>", "@css/root", 1, 2);

            // Money command
            _commandManager.RegisterCommand("money", HandleMoneyCommand,
                "Set player money", "<player|team|all> <value> OR <value>", "@css/root", 1, 2);

            // Clear custom money command
            _commandManager.RegisterCommand("clearmoney", HandleClearMoneyCommand,
                "Clear custom money tracking", "<player|team|all>", "@css/root", 1, 1);

            // Kick command
            _commandManager.RegisterCommand("kick", HandleKickCommand,
                "Kick players", "<player|team|all>", "@css/root", 1, 1);

            // Freeze/Unfreeze commands
            _commandManager.RegisterCommand("freeze", HandleFreezeCommand,
                "Freeze players", "<player|team|all>", "@css/root", 1, 1);

            _commandManager.RegisterCommand("unfreeze", HandleUnfreezeCommand,
                "Unfreeze players", "<player|team|all>", "@css/root", 1, 1);

            // Noclip command
            _commandManager.RegisterCommand("noclip", HandleNoclipCommand,
                "Toggle noclip for yourself", "", "@css/root", 0, 0);
        }

        #region Health Command

        /// <summary>
        /// Handle !health command
        /// </summary>
        private bool HandleHealthCommand(CCSPlayerController? player, string[] args)
        {
            try
            {
                int health;
                PlayerTarget? targets;

                // Parse arguments: !health <value> OR !health <target> <value>
                if (args.Length == 1)
                {
                    // !health <value> - apply to self
                    if (!int.TryParse(args[0], out health) || health < 0 || health > 999999)
                    {
                        if (player != null)
                            ChatUtils.PrintToPlayer(player, "Invalid health value. Use 0-999999.", MessageType.Error);
                        return false;
                    }

                    if (player == null)
                    {
                        Console.WriteLine("[CS2Utils] Health command requires a player when no target specified");
                        return false;
                    }

                    targets = new PlayerTarget
                    {
                        Type = TargetType.Individual,
                        Players = new List<CCSPlayerController> { player }
                    };
                }
                else
                {
                    // !health <target> <value>
                    if (!int.TryParse(args[1], out health) || health < 0 || health > 999999)
                    {
                        if (player != null)
                            ChatUtils.PrintToPlayer(player, "Invalid health value. Use 0-999999.", MessageType.Error);
                        return false;
                    }

                    targets = _targetResolver.ResolveTargets(args[0], player);
                    if (targets == null || !targets.IsValid)
                    {
                        if (player != null)
                            ChatUtils.PrintToPlayer(player, targets?.ErrorMessage ?? "Invalid target.", MessageType.Error);
                        return false;
                    }
                }

                // Apply health to all targets
                var successCount = 0;
                foreach (var target in targets.Players)
                {
                    if (target.IsAlive())
                    {
                        target.SetHealth(health);
                        successCount++;
                    }
                }

                // Provide feedback
                var message = $"Set health to {health} for {successCount} player(s)";
                if (player != null)
                    ChatUtils.PrintToPlayer(player, message, MessageType.Success);
                else
                    Console.WriteLine($"[CS2Utils] {message}");

                return successCount > 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[CS2Utils] Error setting health: {ex.Message}");
                if (player != null)
                    ChatUtils.PrintToPlayer(player, "Failed to set health.", MessageType.Error);
                return false;
            }
        }

        #endregion

        #region Kill Command

        /// <summary>
        /// Handle !kill command
        /// </summary>
        private bool HandleKillCommand(CCSPlayerController? player, string[] args)
        {
            try
            {
                PlayerTarget? targets;

                if (args.Length == 0)
                {
                    // !kill - kill self
                    if (player == null)
                    {
                        Console.WriteLine("[CS2Utils] Kill command requires a player when no target specified");
                        return false;
                    }

                    targets = new PlayerTarget
                    {
                        Type = TargetType.Self,
                        Players = new List<CCSPlayerController> { player }
                    };
                }
                else
                {
                    // !kill <target>
                    targets = _targetResolver.ResolveTargets(args[0], player);
                    if (targets == null || !targets.IsValid)
                    {
                        if (player != null)
                            ChatUtils.PrintToPlayer(player, targets?.ErrorMessage ?? "Invalid target.", MessageType.Error);
                        return false;
                    }
                }

                // Kill all targets
                var successCount = 0;
                foreach (var target in targets.Players)
                {
                    if (target.IsAlive())
                    {
                        target.Kill();
                        successCount++;
                    }
                }

                // Provide feedback
                var message = $"Killed {successCount} player(s)";
                if (player != null)
                    ChatUtils.PrintToPlayer(player, message, MessageType.Success);
                else
                    Console.WriteLine($"[CS2Utils] {message}");

                return successCount > 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[CS2Utils] Error killing players: {ex.Message}");
                if (player != null)
                    ChatUtils.PrintToPlayer(player, "Failed to kill players.", MessageType.Error);
                return false;
            }
        }

        #endregion

        #region Teleport Commands

        /// <summary>
        /// Handle !tp/!teleport command
        /// </summary>
        private bool HandleTeleportCommand(CCSPlayerController? player, string[] args)
        {
            try
            {
                if (args.Length == 1)
                {
                    // !tp <toplayer> - teleport self to target
                    return HandleTeleportToPlayer(player, args[0]);
                }
                else if (args.Length == 2)
                {
                    // !tp <fromplayer> <toplayer> - teleport from to to
                    return HandleTeleportFromTo(player, args[0], args[1]);
                }
                else
                {
                    if (player != null)
                        ChatUtils.PrintToPlayer(player, "Usage: !tp <toplayer> OR !tp <fromplayer> <toplayer>", MessageType.Error);
                    return false;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[CS2Utils] Error teleporting players: {ex.Message}");
                if (player != null)
                    ChatUtils.PrintToPlayer(player, "Failed to teleport players.", MessageType.Error);
                return false;
            }
        }

        /// <summary>
        /// Teleport self to target player
        /// </summary>
        private bool HandleTeleportToPlayer(CCSPlayerController? player, string targetName)
        {
            if (player == null)
            {
                Console.WriteLine("[CS2Utils] Teleport command requires a player when no source specified");
                return false;
            }

            var target = _targetResolver.ResolveTargets(targetName, player);
            if (target == null || !target.IsValid || target.Players.Count == 0)
            {
                ChatUtils.PrintToPlayer(player, target?.ErrorMessage ?? "Invalid target.", MessageType.Error);
                return false;
            }

            var destination = target.Players.First();
            if (!destination.IsValid || !destination.PawnIsAlive)
            {
                ChatUtils.PrintToPlayer(player, "Target player is not alive.", MessageType.Error);
                return false;
            }

            return TeleportPlayer(player, destination);
        }

        /// <summary>
        /// Teleport from player(s) to target player
        /// </summary>
        private bool HandleTeleportFromTo(CCSPlayerController? player, string fromTarget, string toTarget)
        {
            var fromPlayers = _targetResolver.ResolveTargets(fromTarget, player);
            if (fromPlayers == null || !fromPlayers.IsValid)
            {
                if (player != null)
                    ChatUtils.PrintToPlayer(player, fromPlayers?.ErrorMessage ?? "Invalid source target.", MessageType.Error);
                return false;
            }

            var toPlayer = _targetResolver.ResolveTargets(toTarget, player);
            if (toPlayer == null || !toPlayer.IsValid || toPlayer.Players.Count == 0)
            {
                if (player != null)
                    ChatUtils.PrintToPlayer(player, toPlayer?.ErrorMessage ?? "Invalid destination target.", MessageType.Error);
                return false;
            }

            var destination = toPlayer.Players.First();
            if (!destination.IsValid || !destination.PawnIsAlive)
            {
                if (player != null)
                    ChatUtils.PrintToPlayer(player, "Destination player is not alive.", MessageType.Error);
                return false;
            }

            var successCount = 0;
            foreach (var source in fromPlayers.Players)
            {
                if (TeleportPlayer(source, destination))
                    successCount++;
            }

            var message = $"Teleported {successCount} player(s)";
            if (player != null)
                ChatUtils.PrintToPlayer(player, message, MessageType.Success);
            else
                Console.WriteLine($"[CS2Utils] {message}");

            return successCount > 0;
        }

        /// <summary>
        /// Teleport one player to another's position
        /// </summary>
        private bool TeleportPlayer(CCSPlayerController source, CCSPlayerController destination)
        {
            if (!source.IsValid || !source.PawnIsAlive || !destination.IsValid || !destination.PawnIsAlive)
                return false;

            var destPawn = destination.PlayerPawn.Value;
            var sourcePawn = source.PlayerPawn.Value;
            
            if (destPawn == null || sourcePawn == null)
                return false;

            // Get destination position and angles
            var destPos = destPawn.CBodyComponent?.SceneNode?.AbsOrigin;
            var destAngles = destPawn.EyeAngles;

            if (destPos == null)
                return false;

            // Teleport source player
            sourcePawn.Teleport(destPos, destAngles, new Vector(0, 0, 0));
            
            return true;
        }

        #endregion

        #region Money Command

        /// <summary>
        /// Handle !money command
        /// </summary>
        private bool HandleMoneyCommand(CCSPlayerController? player, string[] args)
        {
            try
            {
                int money;
                PlayerTarget? targets;
                bool isModifier = false;

                // Parse arguments: !money <value> OR !money <target> <value>
                bool isInfinite = false;
                if (args.Length == 1)
                {
                    // !money <value> - apply to self
                    var moneyStr = args[0];
                    if (!ParseMoneyValue(moneyStr, out money, out isModifier, out isInfinite))
                    {
                        if (player != null)
                            ChatUtils.PrintToPlayer(player, "Invalid money value. Use 0-999999, +/-amount, or -1 for infinite money.", MessageType.Error);
                        return false;
                    }

                    if (player == null)
                    {
                        Console.WriteLine("[CS2Utils] Money command requires a player when no target specified");
                        return false;
                    }

                    targets = new PlayerTarget
                    {
                        Type = TargetType.Individual,
                        Players = new List<CCSPlayerController> { player }
                    };
                }
                else
                {
                    // !money <target> <value>
                    var moneyStr = args[1];
                    if (!ParseMoneyValue(moneyStr, out money, out isModifier, out isInfinite))
                    {
                        if (player != null)
                            ChatUtils.PrintToPlayer(player, "Invalid money value. Use 0-999999, +/-amount, or -1 for infinite money.", MessageType.Error);
                        return false;
                    }

                    targets = _targetResolver.ResolveTargets(args[0], player);
                    if (targets == null || !targets.IsValid)
                    {
                        if (player != null)
                            ChatUtils.PrintToPlayer(player, targets?.ErrorMessage ?? "Invalid target.", MessageType.Error);
                        return false;
                    }
                }

                // Get current server max money limit
                var maxMoney = GetServerMaxMoney();

                // Apply money to all targets
                var successCount = 0;
                foreach (var target in targets.Players)
                {
                    if (target.IsPlayer())
                    {
                        int finalMoney;
                        bool shouldEnableInfinite = false;

                        if (isInfinite)
                        {
                            // Special case: -1 means enable infinite money
                            // Use server's max money (usually enough for the most expensive weapon)
                            finalMoney = maxMoney;
                            shouldEnableInfinite = true;
                        }
                        else if (isModifier)
                        {
                            // Add/subtract money
                            var currentMoney = target.GetMoney();
                            finalMoney = Math.Max(0, Math.Min(999999, currentMoney + money));
                            // Enable infinite if result exceeds server limit
                            shouldEnableInfinite = finalMoney > maxMoney;
                        }
                        else
                        {
                            // Set absolute money
                            finalMoney = Math.Max(0, Math.Min(999999, money));
                            // Enable infinite if value exceeds server limit
                            shouldEnableInfinite = finalMoney > maxMoney;
                        }

                        // Set the money
                        target.SetMoney(finalMoney);

                        // Handle infinite money toggle
                        var wasInfinite = _infiniteMoneyPlayers.ContainsKey(target.SteamID);
                        
                        if (shouldEnableInfinite)
                        {
                            _infiniteMoneyPlayers[target.SteamID] = true;

                            if (!wasInfinite)
                            {
                                var infiniteMessage = isInfinite 
                                    ? $"Infinite money enabled for {target.PlayerName} (money set to ${maxMoney}, will be restored after purchases)"
                                    : $"Money set to ${finalMoney} (exceeds server limit ${maxMoney}). Infinite money enabled.";
                                
                                if (player != null)
                                    ChatUtils.PrintToPlayer(player, infiniteMessage, MessageType.Info);
                                else
                                    Console.WriteLine($"[CS2Utils] {infiniteMessage}");

                                // Also notify the target player
                                ChatUtils.PrintToPlayer(target, "Infinite money enabled! Your money will be restored after purchases.", MessageType.Success);

                                if (_config.EnableDebugLogging)
                                {
                                    Console.WriteLine($"[CS2Utils] Enabled infinite money for {target.PlayerName} (${finalMoney} > server limit ${maxMoney})");
                                }
                            }
                        }
                        else
                        {
                            // Disable infinite money if within normal limits
                            if (_infiniteMoneyPlayers.Remove(target.SteamID))
                            {
                                var normalMessage = $"Money set to ${finalMoney} (within server limit ${maxMoney}). Infinite money disabled.";
                                if (player != null)
                                    ChatUtils.PrintToPlayer(player, normalMessage, MessageType.Info);
                                else
                                    Console.WriteLine($"[CS2Utils] {normalMessage}");

                                // Also notify the target player
                                ChatUtils.PrintToPlayer(target, "Infinite money disabled.", MessageType.Info);

                                if (_config.EnableDebugLogging)
                                {
                                    Console.WriteLine($"[CS2Utils] Disabled infinite money for {target.PlayerName} (${finalMoney} <= server limit ${maxMoney})");
                                }
                            }
                        }

                        successCount++;
                    }
                }

                // Provide feedback
                string message;
                if (isInfinite)
                {
                    message = $"Infinite money enabled for {successCount} player(s)";
                }
                else
                {
                    var actionText = isModifier ? (money >= 0 ? "added" : "removed") : "set";
                    var amountText = isModifier ? Math.Abs(money).ToString() : money.ToString();
                    message = $"Money {actionText} {amountText} for {successCount} player(s)";
                }

                if (player != null)
                    ChatUtils.PrintToPlayer(player, message, MessageType.Success);
                else
                    Console.WriteLine($"[CS2Utils] {message}");

                return successCount > 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[CS2Utils] Error setting money: {ex.Message}");
                if (player != null)
                    ChatUtils.PrintToPlayer(player, "Failed to set money.", MessageType.Error);
                return false;
            }
        }

        /// <summary>
        /// Parse money value with support for +/- modifiers and -1 for infinite money
        /// </summary>
        private bool ParseMoneyValue(string input, out int money, out bool isModifier, out bool isInfinite)
        {
            money = 0;
            isModifier = false;
            isInfinite = false;

            if (string.IsNullOrEmpty(input))
                return false;

            // Special case: -1 means infinite money
            if (input == "-1")
            {
                isInfinite = true;
                return true;
            }

            if (input.StartsWith("+") || input.StartsWith("-"))
            {
                isModifier = true;
                if (!int.TryParse(input, out money))
                    return false;
            }
            else
            {
                if (!int.TryParse(input, out money) || money < 0 || money > 999999)
                    return false;
            }

            return true;
        }

        #endregion

        #region Kick Command

        /// <summary>
        /// Handle !kick command
        /// </summary>
        private bool HandleKickCommand(CCSPlayerController? player, string[] args)
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
                    if (target.IsPlayer())
                    {
                        target.Kick("Kicked by admin");
                        successCount++;
                    }
                }

                // Provide feedback
                var message = $"Kicked {successCount} player(s)";
                if (player != null)
                    ChatUtils.PrintToPlayer(player, message, MessageType.Success);
                else
                    Console.WriteLine($"[CS2Utils] {message}");

                return successCount > 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[CS2Utils] Error kicking players: {ex.Message}");
                if (player != null)
                    ChatUtils.PrintToPlayer(player, "Failed to kick players.", MessageType.Error);
                return false;
            }
        }

        #endregion

        #region Freeze/Unfreeze Commands

        /// <summary>
        /// Handle !freeze command
        /// </summary>
        private bool HandleFreezeCommand(CCSPlayerController? player, string[] args)
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
                    if (target.IsAlive())
                    {
                        target.Freeze();
                        _frozenPlayers[target.SteamID] = true;
                        successCount++;
                    }
                }

                // Save frozen player states
                _stateManager.SaveState("frozen_players", _frozenPlayers.Keys.ToList());

                // Provide feedback
                var message = $"Froze {successCount} player(s)";
                if (player != null)
                    ChatUtils.PrintToPlayer(player, message, MessageType.Success);
                else
                    Console.WriteLine($"[CS2Utils] {message}");

                return successCount > 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[CS2Utils] Error freezing players: {ex.Message}");
                if (player != null)
                    ChatUtils.PrintToPlayer(player, "Failed to freeze players.", MessageType.Error);
                return false;
            }
        }

        /// <summary>
        /// Handle !unfreeze command
        /// </summary>
        private bool HandleUnfreezeCommand(CCSPlayerController? player, string[] args)
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
                    if (target.IsAlive())
                    {
                        target.Unfreeze();
                        _frozenPlayers.Remove(target.SteamID);
                        successCount++;
                    }
                }

                // Save frozen player states
                _stateManager.SaveState("frozen_players", _frozenPlayers.Keys.ToList());

                // Provide feedback
                var message = $"Unfroze {successCount} player(s)";
                if (player != null)
                    ChatUtils.PrintToPlayer(player, message, MessageType.Success);
                else
                    Console.WriteLine($"[CS2Utils] {message}");

                return successCount > 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[CS2Utils] Error unfreezing players: {ex.Message}");
                if (player != null)
                    ChatUtils.PrintToPlayer(player, "Failed to unfreeze players.", MessageType.Error);
                return false;
            }
        }

        /// <summary>
        /// Handle !noclip command - executes the actual noclip console command for the player
        /// </summary>
        private bool HandleNoclipCommand(CCSPlayerController? player, string[] args)
        {
            try
            {
                // Only works for players, not console
                if (player == null)
                {
                    Console.WriteLine("[CS2Utils] Noclip command can only be used by players");
                    return false;
                }

                // Player must be alive
                if (!player.IsAlive())
                {
                    ChatUtils.PrintToPlayer(player, "You must be alive to use noclip", MessageType.Error);
                    return false;
                }

                // Remove cheat flag from noclip command so it works without sv_cheats
                RemoveCheatFlagFromConVar("noclip");

                // Execute the actual noclip command for the player
                // This uses the player's client command execution
                player.ExecuteClientCommand("noclip");

                // Track noclip state for feedback (toggle tracking)
                var isCurrentlyNoclip = _noclipPlayers.ContainsKey(player.SteamID);

                if (isCurrentlyNoclip)
                {
                    _noclipPlayers.Remove(player.SteamID);
                    ChatUtils.PrintToPlayer(player, "Noclip toggled", MessageType.Info);
                }
                else
                {
                    _noclipPlayers[player.SteamID] = true;
                    ChatUtils.PrintToPlayer(player, "Noclip toggled", MessageType.Info);
                }

                // Save state
                _stateManager.SaveState("noclip_players", _noclipPlayers.Keys.ToList());

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[CS2Utils] Error executing noclip: {ex.Message}");
                if (player != null)
                    ChatUtils.PrintToPlayer(player, "Failed to execute noclip", MessageType.Error);
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

        #endregion

        #region State Management

        /// <summary>
        /// Cleanup resources and state
        /// </summary>
        public void Cleanup()
        {
            try
            {
                // Restore normal movement for all affected players
                var players = Utilities.GetPlayers();
                foreach (var player in players)
                {
                    if (player.IsValid && !player.IsBot && player.PawnIsAlive)
                    {
                        var pawn = player.PlayerPawn.Value;
                        if (pawn != null)
                        {
                            // Reset movement for frozen players (noclip is handled by CS2)
                            if (_frozenPlayers.ContainsKey(player.SteamID))
                            {
                                pawn.MoveType = MoveType_t.MOVETYPE_WALK;
                            }
                        }
                    }
                }

                _frozenPlayers.Clear();
                _infiniteMoneyPlayers.Clear();
                _noclipPlayers.Clear();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[CS2Utils] Error during player commands cleanup: {ex.Message}");
            }
        }

        #endregion

        #region Public Command Execution Methods

        /// <summary>
        /// Execute health command (for console command support)
        /// </summary>
        public bool ExecuteHealthCommand(CCSPlayerController? player, string[] args)
        {
            return HandleHealthCommand(player, args);
        }

        /// <summary>
        /// Execute money command (for console command support)
        /// </summary>
        public bool ExecuteMoneyCommand(CCSPlayerController? player, string[] args)
        {
            return HandleMoneyCommand(player, args);
        }

        #endregion

        #region Clear Money Command

        /// <summary>
        /// Handle !clearmoney command
        /// </summary>
        private bool HandleClearMoneyCommand(CCSPlayerController? player, string[] args)
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
                    if (target.IsPlayer() && _infiniteMoneyPlayers.ContainsKey(target.SteamID))
                    {
                        _infiniteMoneyPlayers.Remove(target.SteamID);
                        successCount++;
                    }
                }

                var message = $"Disabled infinite money for {successCount} player(s)";
                if (player != null)
                    ChatUtils.PrintToPlayer(player, message, MessageType.Success);
                else
                    Console.WriteLine($"[CS2Utils] {message}");

                return successCount > 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[CS2Utils] Error clearing custom money: {ex.Message}");
                if (player != null)
                    ChatUtils.PrintToPlayer(player, "Failed to clear custom money.", MessageType.Error);
                return false;
            }
        }

        #endregion

        #region Money Management System

        /// <summary>
        /// Get the server's current mp_maxmoney setting
        /// </summary>
        /// <returns>The server's max money limit</returns>
        private int GetServerMaxMoney()
        {
            try
            {
                // Try to get the convar value
                var maxMoneyConvar = ConVar.Find("mp_maxmoney");
                if (maxMoneyConvar != null)
                {
                    return maxMoneyConvar.GetPrimitiveValue<int>();
                }
            }
            catch (Exception ex)
            {
                if (_config.EnableDebugLogging)
                {
                    Console.WriteLine($"[CS2Utils] Error getting mp_maxmoney convar: {ex.Message}");
                }
            }

            // Default CS2 max money if we can't get the convar
            // Most common values: 16000 (competitive), 8000 (wingman), 12000 (casual)
            return 16000;
        }

        /// <summary>
        /// Check and restore infinite money for a player
        /// </summary>
        /// <param name="player">The player to check</param>
        public void CheckAndRestoreInfiniteMoney(CCSPlayerController player)
        {
            if (!player.IsPlayer() || !_infiniteMoneyPlayers.ContainsKey(player.SteamID))
                return;

            var maxMoney = GetServerMaxMoney();
            var currentMoney = player.GetMoney();

            // If player has infinite money enabled and their money dropped below max, restore to max
            if (currentMoney < maxMoney)
            {
                player.SetMoney(maxMoney);

                if (_config.EnableDebugLogging)
                {
                    Console.WriteLine($"[CS2Utils] Restored infinite money for {player.PlayerName}: ${currentMoney} -> ${maxMoney}");
                }
            }
        }

        /// <summary>
        /// Remove infinite money for a player (when they disconnect)
        /// </summary>
        /// <param name="steamId">The player's Steam ID</param>
        public void RemoveInfiniteMoney(ulong steamId)
        {
            _infiniteMoneyPlayers.Remove(steamId);
        }

        /// <summary>
        /// Check if player has infinite money enabled
        /// </summary>
        /// <param name="steamId">The player's Steam ID</param>
        /// <returns>True if player has infinite money enabled</returns>
        public bool HasInfiniteMoney(ulong steamId)
        {
            return _infiniteMoneyPlayers.ContainsKey(steamId);
        }

        /// <summary>
        /// Check if any players have infinite money enabled
        /// </summary>
        /// <returns>True if any players have infinite money</returns>
        public bool HasAnyInfiniteMoney()
        {
            return _infiniteMoneyPlayers.Count > 0;
        }

        /// <summary>
        /// Get count of players with infinite money
        /// </summary>
        /// <returns>Number of players with infinite money enabled</returns>
        public int GetInfiniteMoneyCount()
        {
            return _infiniteMoneyPlayers.Count;
        }

        #endregion
    }
}
