using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CS2Utilities.Core;
using CS2Utilities.Utils;

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
                "Change to specified map", "<mapname>", "@css/root", 1, 1);
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
