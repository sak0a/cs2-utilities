using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Admin;
using CS2Utilities.Config;

namespace CS2Utilities.Core
{
    /// <summary>
    /// Manages permission validation and role-based access control for commands
    /// </summary>
    public class PermissionManager
    {
        private readonly CS2UtilitiesConfig _config;
        public PermissionManager(CS2UtilitiesConfig config)
        {
            _config = config;
        }

        /// <summary>
        /// Check if a player has permission to execute a specific command
        /// </summary>
        /// <param name="player">The player to check permissions for</param>
        /// <param name="permission">The permission string to check</param>
        /// <returns>True if the player has permission, false otherwise</returns>
        public bool HasPermission(CCSPlayerController? player, string permission)
        {
            // Console/server always has permission
            if (player == null)
                return true;

            // Empty permission means everyone can use it
            if (string.IsNullOrEmpty(permission))
                return true;

            // Check if player is valid
            if (!player.IsValid || player.IsBot)
                return false;

            // Check if the player has the required permission
            return AdminManager.PlayerHasPermissions(player, permission);
        }

        /// <summary>
        /// Reload permissions from configuration
        /// </summary>
        public static void ReloadPermissions()
        {
            // This method can be called to trigger a permission reload
            // The actual reloading happens when the configuration is reloaded
        }
    }
}
