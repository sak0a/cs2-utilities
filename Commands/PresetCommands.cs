using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CS2Utilities.Core;
using CS2Utilities.Utils;

namespace CS2Utilities.Commands
{
    /// <summary>
    /// Implements server preset management commands
    /// </summary>
    public class PresetCommands
    {
        private readonly CommandManager _commandManager;
        private readonly PresetManager _presetManager;

        public PresetCommands(CommandManager commandManager, PresetManager presetManager)
        {
            _commandManager = commandManager;
            _presetManager = presetManager;
        }

        /// <summary>
        /// Register all preset commands with the command manager
        /// </summary>
        public void RegisterCommands()
        {
            // Unified preset command with subcommands
            _commandManager.RegisterCommand("preset", HandlePresetCommand,
                "Preset management", "<load|info|list> [presetname]", "@css/root", 1, 2);
        }

        #region Preset Command Handler

        /// <summary>
        /// Handle !preset command with subcommands
        /// </summary>
        private bool HandlePresetCommand(CCSPlayerController? player, string[] args)
        {
            if (args.Length == 0)
            {
                if (player != null)
                    ChatUtils.PrintToPlayer(player, "Usage: !preset <load|info|list> [presetname]", MessageType.Error);
                else
                    Console.WriteLine("[CS2Utils] Usage: !preset <load|info|list> [presetname]");
                return false;
            }

            var subcommand = args[0].ToLower();

            switch (subcommand)
            {
                case "load":
                    if (args.Length < 2)
                    {
                        if (player != null)
                            ChatUtils.PrintToPlayer(player, "Usage: !preset load <presetname>", MessageType.Error);
                        else
                            Console.WriteLine("[CS2Utils] Usage: !preset load <presetname>");
                        return false;
                    }
                    return HandleLoadPreset(player, args[1]);

                case "info":
                    if (args.Length < 2)
                    {
                        if (player != null)
                            ChatUtils.PrintToPlayer(player, "Usage: !preset info <presetname>", MessageType.Error);
                        else
                            Console.WriteLine("[CS2Utils] Usage: !preset info <presetname>");
                        return false;
                    }
                    return HandlePresetInfo(player, args[1]);

                case "list":
                    return HandleListPresets(player);

                default:
                    if (player != null)
                        ChatUtils.PrintToPlayer(player, $"Unknown subcommand '{subcommand}'. Use: load, info, or list", MessageType.Error);
                    else
                        Console.WriteLine($"[CS2Utils] Unknown subcommand '{subcommand}'. Use: load, info, or list");
                    return false;
            }
        }

        #endregion

        #region Load Preset

        /// <summary>
        /// Handle preset load subcommand
        /// </summary>
        private bool HandleLoadPreset(CCSPlayerController? player, string presetName)
        {
            try
            {
                // Check if preset exists
                if (!_presetManager.PresetExists(presetName))
                {
                    if (player != null)
                        ChatUtils.PrintToPlayer(player, $"Preset '{presetName}' not found. Use !preset list to see available presets.", MessageType.Error);
                    return false;
                }

                // Get current preset for unload commands
                var currentPreset = _presetManager.GetCurrentPresetName();

                // Load the preset
                if (_presetManager.LoadPreset(presetName, currentPreset))
                {
                    var message = $"Loaded preset '{presetName}'";
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
                            ChatUtils.PrintToPlayer(p, $"Server preset changed to: {presetName}", MessageType.Info);
                        }
                    }

                    return true;
                }
                else
                {
                    if (player != null)
                        ChatUtils.PrintToPlayer(player, "Failed to load preset.", MessageType.Error);
                    return false;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[CS2Utils] Error loading preset: {ex.Message}");
                if (player != null)
                    ChatUtils.PrintToPlayer(player, "Failed to load preset.", MessageType.Error);
                return false;
            }
        }

        #endregion

        #region List Presets

        /// <summary>
        /// Handle preset list subcommand
        /// </summary>
        private bool HandleListPresets(CCSPlayerController? player)
        {
            try
            {
                var presets = _presetManager.GetPresets();
                var currentPreset = _presetManager.GetCurrentPresetName();

                if (presets.Count == 0)
                {
                    if (player != null)
                        ChatUtils.PrintToPlayer(player, "No presets available.", MessageType.Info);
                    else
                        Console.WriteLine("[CS2Utils] No presets available");
                    return true;
                }

                var message = $"Available presets ({presets.Count}):";
                if (player != null)
                    ChatUtils.PrintToPlayer(player, message, MessageType.Info);
                else
                    Console.WriteLine($"[CS2Utils] {message}");

                foreach (var preset in presets.Values.OrderBy(p => p.Name))
                {
                    var isCurrentText = preset.Name.Equals(currentPreset, StringComparison.OrdinalIgnoreCase) ? " [CURRENT]" : "";
                    var presetInfo = $"  {preset.GetSummary()}{isCurrentText}";
                    
                    if (player != null)
                        ChatUtils.PrintToPlayer(player, presetInfo, MessageType.Info);
                    else
                        Console.WriteLine($"[CS2Utils] {presetInfo}");
                }

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[CS2Utils] Error listing presets: {ex.Message}");
                if (player != null)
                    ChatUtils.PrintToPlayer(player, "Failed to list presets.", MessageType.Error);
                return false;
            }
        }

        #endregion

        #region Preset Info

        /// <summary>
        /// Handle preset info subcommand
        /// </summary>
        private bool HandlePresetInfo(CCSPlayerController? player, string presetName)
        {
            try
            {
                // Check if preset exists
                if (!_presetManager.PresetExists(presetName))
                {
                    if (player != null)
                        ChatUtils.PrintToPlayer(player, $"Preset '{presetName}' not found.", MessageType.Error);
                    return false;
                }

                var presets = _presetManager.GetPresets();
                var preset = presets[presetName.ToLower()];
                var detailedInfo = preset.GetDetailedInfo();

                foreach (var line in detailedInfo)
                {
                    if (player != null)
                        ChatUtils.PrintToPlayer(player, line, MessageType.Info);
                    else
                        Console.WriteLine($"[CS2Utils] {line}");
                }

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[CS2Utils] Error showing preset info: {ex.Message}");
                if (player != null)
                    ChatUtils.PrintToPlayer(player, "Failed to show preset info.", MessageType.Error);
                return false;
            }
        }

        #endregion
    }
}
