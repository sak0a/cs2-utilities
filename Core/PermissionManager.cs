using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Admin;
using CS2Utilities.Config;
using CS2Utilities.Models;

namespace CS2Utilities.Core
{
    /// <summary>
    /// Manages permission validation and role-based access control for commands
    /// </summary>
    public class PermissionManager
    {
        private readonly CS2UtilitiesConfig _config;
        private readonly Dictionary<string, string> _commandPermissions;

        public PermissionManager(CS2UtilitiesConfig config)
        {
            _config = config;
            _commandPermissions = new Dictionary<string, string>();
            LoadPermissions();
        }

        /// <summary>
        /// Load permissions from configuration into memory for fast lookup
        /// </summary>
        private void LoadPermissions()
        {
            _commandPermissions.Clear();
            
            foreach (var permission in _config.CommandPermissions)
            {
                if (!string.IsNullOrEmpty(permission.CommandName) && permission.Enabled)
                {
                    _commandPermissions[permission.CommandName.ToLower()] = permission.Permission;
                }
            }
        }

        /// <summary>
        /// Check if a player has permission to execute a specific command
        /// </summary>
        /// <param name="player">The player to check permissions for</param>
        /// <param name="commandName">The command name (without ! prefix)</param>
        /// <returns>True if the player has permission, false otherwise</returns>
        public bool HasPermission(CCSPlayerController? player, string commandName)
        {
            // Console/server always has permission
            if (player == null)
                return true;

            // Check if player is valid
            if (!player.IsValid || player.IsBot)
                return false;

            var normalizedCommand = commandName.ToLower();
            
            // Get the required permission for this command
            var requiredPermission = GetRequiredPermission(normalizedCommand);
            
            // Check if the player has the required permission
            return AdminManager.PlayerHasPermissions(player, requiredPermission);
        }

        /// <summary>
        /// Get the required permission for a command
        /// </summary>
        /// <param name="commandName">The command name (normalized to lowercase)</param>
        /// <returns>The required permission string</returns>
        public string GetRequiredPermission(string commandName)
        {
            if (_commandPermissions.TryGetValue(commandName, out var permission))
            {
                return permission;
            }
            
            return _config.DefaultPermission;
        }

        /// <summary>
        /// Add or update a command permission
        /// </summary>
        /// <param name="commandName">The command name</param>
        /// <param name="permission">The required permission</param>
        public void SetCommandPermission(string commandName, string permission)
        {
            var normalizedCommand = commandName.ToLower();
            _commandPermissions[normalizedCommand] = permission;
            
            // Update the configuration
            var existingPermission = _config.CommandPermissions
                .FirstOrDefault(p => p.CommandName.ToLower() == normalizedCommand);
            
            if (existingPermission != null)
            {
                existingPermission.Permission = permission;
            }
            else
            {
                _config.CommandPermissions.Add(new CommandPermission
                {
                    CommandName = commandName,
                    Permission = permission,
                    Enabled = true
                });
            }
        }

        /// <summary>
        /// Remove a command permission (will fall back to default)
        /// </summary>
        /// <param name="commandName">The command name</param>
        public void RemoveCommandPermission(string commandName)
        {
            var normalizedCommand = commandName.ToLower();
            _commandPermissions.Remove(normalizedCommand);
            
            // Remove from configuration
            var existingPermission = _config.CommandPermissions
                .FirstOrDefault(p => p.CommandName.ToLower() == normalizedCommand);
            
            if (existingPermission != null)
            {
                _config.CommandPermissions.Remove(existingPermission);
            }
        }

        /// <summary>
        /// Check if a command is disabled
        /// </summary>
        /// <param name="commandName">The command name</param>
        /// <returns>True if the command is disabled</returns>
        public bool IsCommandDisabled(string commandName)
        {
            return _config.DisabledCommands.Contains(commandName, StringComparer.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Disable a command
        /// </summary>
        /// <param name="commandName">The command name</param>
        public void DisableCommand(string commandName)
        {
            if (!_config.DisabledCommands.Contains(commandName, StringComparer.OrdinalIgnoreCase))
            {
                _config.DisabledCommands.Add(commandName);
            }
        }

        /// <summary>
        /// Enable a command
        /// </summary>
        /// <param name="commandName">The command name</param>
        public void EnableCommand(string commandName)
        {
            _config.DisabledCommands.RemoveAll(cmd => 
                string.Equals(cmd, commandName, StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>
        /// Get all configured permissions
        /// </summary>
        /// <returns>Dictionary of command names to permissions</returns>
        public Dictionary<string, string> GetAllPermissions()
        {
            return new Dictionary<string, string>(_commandPermissions);
        }

        /// <summary>
        /// Reload permissions from configuration
        /// </summary>
        public void ReloadPermissions()
        {
            LoadPermissions();
        }

        /// <summary>
        /// Get a formatted permission denial message
        /// </summary>
        /// <param name="commandName">The command name</param>
        /// <returns>Formatted error message</returns>
        public string GetPermissionDeniedMessage(string commandName)
        {
            var requiredPermission = GetRequiredPermission(commandName);
            return $"You don't have permission to use this command. Required: {requiredPermission}";
        }
    }
}
