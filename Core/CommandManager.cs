using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using CS2Utilities.Config;
using CS2Utilities.Utils;

namespace CS2Utilities.Core
{
    /// <summary>
    /// Delegate for command handlers
    /// </summary>
    /// <param name="player">Player who executed the command</param>
    /// <param name="args">Command arguments</param>
    /// <returns>True if command executed successfully</returns>
    public delegate bool CommandHandler(CCSPlayerController? player, string[] args);

    /// <summary>
    /// Information about a registered command
    /// </summary>
    public class CommandInfo
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Usage { get; set; } = string.Empty;
        public string Permission { get; set; } = "@css/root";
        public CommandHandler Handler { get; set; } = null!;
        public int MinArgs { get; set; } = 0;
        public int MaxArgs { get; set; } = -1; // -1 means unlimited
        public bool Enabled { get; set; } = true;
        public DateTime LastUsed { get; set; } = DateTime.MinValue;
        public int UsageCount { get; set; } = 0;
    }

    /// <summary>
    /// Manages command registration, execution, and permission checking
    /// </summary>
    public class CommandManager
    {
        private readonly CS2UtilitiesConfig _config;
        private readonly PermissionManager _permissionManager;
        private readonly PlayerTargetResolver _targetResolver;
        private readonly Dictionary<string, CommandInfo> _commands;
        private readonly Dictionary<string, DateTime> _playerCooldowns;

        public CommandManager(CS2UtilitiesConfig config, PermissionManager permissionManager, PlayerTargetResolver targetResolver)
        {
            _config = config;
            _permissionManager = permissionManager;
            _targetResolver = targetResolver;
            _commands = new Dictionary<string, CommandInfo>();
            _playerCooldowns = new Dictionary<string, DateTime>();

            // Initialize default commands if not present
            _config.InitializeDefaultCommands();
        }

        /// <summary>
        /// Get command configuration from config or create default
        /// </summary>
        private Models.CommandConfig GetCommandConfig(string commandName)
        {
            if (_config.Commands.TryGetValue(commandName, out var config))
            {
                return config;
            }

            // Return default configuration
            return new Models.CommandConfig
            {
                Enabled = true,
                Permission = _config.DefaultPermission,
                Description = "",
                Usage = "",
                Aliases = new List<string>(),
                MinArgs = 0,
                MaxArgs = 10
            };
        }

        /// <summary>
        /// Register a new command
        /// </summary>
        /// <param name="name">Command name (without ! prefix)</param>
        /// <param name="handler">Command handler function</param>
        /// <param name="description">Command description</param>
        /// <param name="usage">Usage syntax</param>
        /// <param name="permission">Required permission (defaults to config default)</param>
        /// <param name="minArgs">Minimum number of arguments</param>
        /// <param name="maxArgs">Maximum number of arguments (-1 for unlimited)</param>
        public void RegisterCommand(string name, CommandHandler handler, string description = "",
            string usage = "", string? permission = null, int minArgs = 0, int maxArgs = -1)
        {
            var normalizedName = name.ToLower();

            // Get command configuration from config
            var commandConfig = GetCommandConfig(normalizedName);

            // Check if command is disabled
            if (!commandConfig.Enabled)
            {
                if (_config.EnableDebugLogging)
                {
                    Console.WriteLine($"[CS2Utils] Command '{name}' is disabled in configuration");
                }
                return;
            }

            var commandInfo = new CommandInfo
            {
                Name = name,
                Description = !string.IsNullOrEmpty(commandConfig.Description) ? commandConfig.Description : description,
                Usage = !string.IsNullOrEmpty(commandConfig.Usage) ? commandConfig.Usage : usage,
                Permission = !string.IsNullOrEmpty(commandConfig.Permission) ? commandConfig.Permission :
                            (permission ?? _config.DefaultPermission),
                Handler = handler,
                MinArgs = commandConfig.MinArgs > 0 ? commandConfig.MinArgs : minArgs,
                MaxArgs = commandConfig.MaxArgs > 0 ? commandConfig.MaxArgs : maxArgs,
                Enabled = commandConfig.Enabled
            };

            _commands[normalizedName] = commandInfo;

            // Register aliases
            foreach (var alias in commandConfig.Aliases)
            {
                var normalizedAlias = alias.ToLower();
                if (!_commands.ContainsKey(normalizedAlias))
                {
                    _commands[normalizedAlias] = commandInfo;
                }
            }

            if (_config.EnableDebugLogging)
            {
                Console.WriteLine($"[CS2Utils] Registered command: {name} (permission: {commandInfo.Permission})");
                if (commandConfig.Aliases.Count > 0)
                {
                    Console.WriteLine($"[CS2Utils] Command aliases: {string.Join(", ", commandConfig.Aliases)}");
                }
            }
        }

        /// <summary>
        /// Execute a command
        /// </summary>
        /// <param name="player">Player who executed the command</param>
        /// <param name="commandName">Command name (without ! prefix)</param>
        /// <param name="args">Command arguments</param>
        /// <returns>True if command executed successfully, false if command not found or failed</returns>
        public bool ExecuteCommand(CCSPlayerController? player, string commandName, string[] args)
        {
            var normalizedName = commandName.ToLower();

            // Check if command exists in our plugin
            if (!_commands.TryGetValue(normalizedName, out var commandInfo))
            {
                // Command not found in our plugin - let other plugins handle it
                // Don't show "Unknown command" message as it might be handled elsewhere
                return false;
            }

            // Check if command is disabled
            if (!commandInfo.Enabled)
            {
                if (player != null)
                {
                    ChatUtils.PrintToPlayer(player, $"Command '{commandName}' is disabled.", MessageType.Error);
                }
                return false;
            }

            // Check permissions
            if (!string.IsNullOrEmpty(commandInfo.Permission) && !_permissionManager.HasPermission(player, commandInfo.Permission))
            {
                if (player != null)
                {
                    ChatUtils.PrintPermissionDenied(player, commandName, commandInfo.Permission);
                }
                return false;
            }

            // Check cooldowns
            if (player != null && _config.EnableCommandCooldowns && !CheckCooldown(player, commandName))
            {
                var remainingCooldown = GetRemainingCooldown(player, commandName);
                ChatUtils.PrintToPlayer(player, $"Command on cooldown. Wait {remainingCooldown:F1} seconds.", MessageType.Warning);
                return false;
            }

            // Validate argument count
            if (args.Length < commandInfo.MinArgs)
            {
                if (player != null)
                {
                    ChatUtils.PrintUsage(player, commandName, commandInfo.Usage);
                }
                return false;
            }

            if (commandInfo.MaxArgs >= 0 && args.Length > commandInfo.MaxArgs)
            {
                if (player != null)
                {
                    ChatUtils.PrintUsage(player, commandName, commandInfo.Usage);
                }
                return false;
            }

            try
            {
                // Set cooldown
                if (player != null && _config.EnableCommandCooldowns)
                {
                    SetCooldown(player, commandName);
                }

                // Execute the command
                var success = commandInfo.Handler(player, args);

                // Update usage statistics
                commandInfo.LastUsed = DateTime.UtcNow;
                commandInfo.UsageCount++;

                // Log command execution
                if (_config.LogCommandExecutions)
                {
                    var playerName = player?.PlayerName ?? "Console";
                    var argsString = string.Join(" ", args);
                    Console.WriteLine($"[CS2Utils] Command executed: {playerName} -> !{commandName} {argsString}");
                }

                return success;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[CS2Utils] Error executing command {commandName}: {ex.Message}");
                if (player != null)
                {
                    ChatUtils.PrintToPlayer(player, "An error occurred while executing the command.", MessageType.Error);
                }
                return false;
            }
        }

        /// <summary>
        /// Check if a player can execute a command (cooldown check)
        /// </summary>
        private bool CheckCooldown(CCSPlayerController player, string commandName)
        {
            var key = $"{player.SteamID}_{commandName}";
            
            if (!_playerCooldowns.TryGetValue(key, out var lastUsed))
                return true;

            var cooldownTime = TimeSpan.FromSeconds(_config.DefaultCommandCooldown);
            return DateTime.UtcNow - lastUsed >= cooldownTime;
        }

        /// <summary>
        /// Set cooldown for a player and command
        /// </summary>
        private void SetCooldown(CCSPlayerController player, string commandName)
        {
            var key = $"{player.SteamID}_{commandName}";
            _playerCooldowns[key] = DateTime.UtcNow;
        }

        /// <summary>
        /// Get remaining cooldown time for a player and command
        /// </summary>
        private double GetRemainingCooldown(CCSPlayerController player, string commandName)
        {
            var key = $"{player.SteamID}_{commandName}";
            
            if (!_playerCooldowns.TryGetValue(key, out var lastUsed))
                return 0;

            var cooldownTime = TimeSpan.FromSeconds(_config.DefaultCommandCooldown);
            var elapsed = DateTime.UtcNow - lastUsed;
            var remaining = cooldownTime - elapsed;
            
            return Math.Max(0, remaining.TotalSeconds);
        }

        /// <summary>
        /// Check if a command is registered in this plugin
        /// </summary>
        /// <param name="commandName">Command name</param>
        /// <returns>True if command is registered in this plugin</returns>
        public bool IsCommandRegistered(string commandName)
        {
            var normalizedName = commandName.ToLower();
            return _commands.ContainsKey(normalizedName);
        }

        /// <summary>
        /// Get information about a command
        /// </summary>
        /// <param name="commandName">Command name</param>
        /// <returns>CommandInfo or null if not found</returns>
        public CommandInfo? GetCommandInfo(string commandName)
        {
            var normalizedName = commandName.ToLower();
            return _commands.TryGetValue(normalizedName, out var info) ? info : null;
        }

        /// <summary>
        /// Get all registered commands
        /// </summary>
        /// <returns>Dictionary of command names to CommandInfo</returns>
        public Dictionary<string, CommandInfo> GetAllCommands()
        {
            return new Dictionary<string, CommandInfo>(_commands);
        }

        /// <summary>
        /// Enable a command
        /// </summary>
        /// <param name="commandName">Command name</param>
        public void EnableCommand(string commandName)
        {
            var normalizedName = commandName.ToLower();
            if (_commands.TryGetValue(normalizedName, out var commandInfo))
            {
                commandInfo.Enabled = true;
                // Update config
                if (_config.Commands.TryGetValue(normalizedName, out var configCommand))
                {
                    configCommand.Enabled = true;
                }
            }
        }

        /// <summary>
        /// Disable a command
        /// </summary>
        /// <param name="commandName">Command name</param>
        public void DisableCommand(string commandName)
        {
            var normalizedName = commandName.ToLower();
            if (_commands.TryGetValue(normalizedName, out var commandInfo))
            {
                commandInfo.Enabled = false;
                // Update config
                if (_config.Commands.TryGetValue(normalizedName, out var configCommand))
                {
                    configCommand.Enabled = false;
                }
            }
        }

        /// <summary>
        /// Check if a command exists
        /// </summary>
        /// <param name="commandName">Command name</param>
        /// <returns>True if command exists</returns>
        public bool CommandExists(string commandName)
        {
            var normalizedName = commandName.ToLower();
            return _commands.ContainsKey(normalizedName);
        }

        /// <summary>
        /// Get command usage statistics
        /// </summary>
        /// <returns>Dictionary of command names to usage counts</returns>
        public Dictionary<string, int> GetUsageStatistics()
        {
            return _commands.ToDictionary(kvp => kvp.Key, kvp => kvp.Value.UsageCount);
        }

        /// <summary>
        /// Clear all cooldowns (useful for testing or admin commands)
        /// </summary>
        public void ClearCooldowns()
        {
            _playerCooldowns.Clear();
        }
    }
}
