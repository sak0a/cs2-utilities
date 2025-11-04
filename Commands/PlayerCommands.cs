using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;
using CS2Utilities.Core;
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
        
        // Player state tracking
        private readonly Dictionary<ulong, bool> _frozenPlayers;

        public PlayerCommands(CommandManager commandManager, StateManager stateManager, PlayerTargetResolver targetResolver)
        {
            _commandManager = commandManager;
            _stateManager = stateManager;
            _targetResolver = targetResolver;
            _frozenPlayers = new Dictionary<ulong, bool>();
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

            // Kick command
            _commandManager.RegisterCommand("kick", HandleKickCommand,
                "Kick players", "<player|team|all>", "@css/root", 1, 1);

            // Freeze/Unfreeze commands
            _commandManager.RegisterCommand("freeze", HandleFreezeCommand,
                "Freeze players", "<player|team|all>", "@css/root", 1, 1);
            
            _commandManager.RegisterCommand("unfreeze", HandleUnfreezeCommand,
                "Unfreeze players", "<player|team|all>", "@css/root", 1, 1);
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
                    if (!int.TryParse(args[0], out health) || health < 0 || health > 1000)
                    {
                        if (player != null)
                            ChatUtils.PrintToPlayer(player, "Invalid health value. Use 0-1000.", MessageType.Error);
                        return false;
                    }

                    if (player == null)
                    {
                        Console.WriteLine("[CS2Utils] Health command requires a player when no target specified");
                        return false;
                    }

                    targets = new PlayerTarget
                    {
                        TargetType = TargetType.Individual,
                        Players = new List<CCSPlayerController> { player }
                    };
                }
                else
                {
                    // !health <target> <value>
                    if (!int.TryParse(args[1], out health) || health < 0 || health > 1000)
                    {
                        if (player != null)
                            ChatUtils.PrintToPlayer(player, "Invalid health value. Use 0-1000.", MessageType.Error);
                        return false;
                    }

                    targets = _targetResolver.ResolveTarget(args[0], player);
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
                    if (target.IsValid && !target.IsBot && target.PawnIsAlive)
                    {
                        target.PlayerPawn.Value!.Health = health;
                        Utilities.SetStateChanged(target.PlayerPawn.Value, "CBaseEntity", "m_iHealth");
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
                        TargetType = TargetType.Self,
                        Players = new List<CCSPlayerController> { player }
                    };
                }
                else
                {
                    // !kill <target>
                    targets = _targetResolver.ResolveTarget(args[0], player);
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
                    if (target.IsValid && !target.IsBot && target.PawnIsAlive)
                    {
                        target.CommitSuicide(false, true);
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

            var target = _targetResolver.ResolveTarget(targetName, player);
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
            var fromPlayers = _targetResolver.ResolveTarget(fromTarget, player);
            if (fromPlayers == null || !fromPlayers.IsValid)
            {
                if (player != null)
                    ChatUtils.PrintToPlayer(player, fromPlayers?.ErrorMessage ?? "Invalid source target.", MessageType.Error);
                return false;
            }

            var toPlayer = _targetResolver.ResolveTarget(toTarget, player);
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
