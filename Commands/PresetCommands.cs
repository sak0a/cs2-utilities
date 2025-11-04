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
            // Save preset command
            _commandManager.RegisterCommand("savepreset", HandleSavePresetCommand,
                "Save current server state as preset", "<name> [description]", "@css/root", 1, 2);

            // Load preset command
            _commandManager.RegisterCommand("loadpreset", HandleLoadPresetCommand,
                "Load a saved preset", "<name>", "@css/root", 1, 1);

            // List presets command
            _commandManager.RegisterCommand("listpresets", HandleListPresetsCommand,
                "List all saved presets", "", "@css/root", 0, 0);

            // Delete preset command
            _commandManager.RegisterCommand("deletepreset", HandleDeletePresetCommand,
                "Delete a saved preset", "<name>", "@css/root", 1, 1);

            // Show preset details command
            _commandManager.RegisterCommand("presetinfo", HandlePresetInfoCommand,
                "Show detailed preset information", "<name>", "@css/root", 1, 1);

            // Add commands to preset
            _commandManager.RegisterCommand("presetaddcmd", HandlePresetAddCommandCommand,
                "Add command to preset", "<preset> <load|unload> <command>", "@css/root", 3, 3);

            // Remove commands from preset
            _commandManager.RegisterCommand("presetremovecmd", HandlePresetRemoveCommandCommand,
                "Remove command from preset", "<preset> <load|unload> <command>", "@css/root", 3, 3);

            // Show current preset
            _commandManager.RegisterCommand("currentpreset", HandleCurrentPresetCommand,
                "Show currently loaded preset", "", "@css/root", 0, 0);
        }

        #region Save Preset Command

        /// <summary>
        /// Handle !savepreset command
        /// </summary>
        private bool HandleSavePresetCommand(CCSPlayerController? player, string[] args)
        {
            try
            {
                var presetName = args[0];
                var description = args.Length > 1 ? string.Join(" ", args.Skip(1)) : "";

                // Validate preset name
                if (string.IsNullOrWhiteSpace(presetName) || presetName.Length > 50)
                {
                    if (player != null)
                        ChatUtils.PrintToPlayer(player, "Invalid preset name. Must be 1-50 characters.", MessageType.Error);
                    return false;
                }

                // Check if preset already exists
                if (_presetManager.PresetExists(presetName))
                {
                    if (player != null)
                        ChatUtils.PrintToPlayer(player, $"Preset '{presetName}' already exists. Use a different name or delete the existing one first.", MessageType.Error);
                    return false;
                }

                // Save the preset
                if (_presetManager.SavePreset(presetName, description))
                {
                    var message = $"Saved preset '{presetName}' with current server settings";
                    if (player != null)
                        ChatUtils.PrintToPlayer(player, message, MessageType.Success);
                    else
                        Console.WriteLine($"[CS2Utils] {message}");
                    return true;
                }
                else
                {
                    if (player != null)
                        ChatUtils.PrintToPlayer(player, "Failed to save preset.", MessageType.Error);
                    return false;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[CS2Utils] Error saving preset: {ex.Message}");
                if (player != null)
                    ChatUtils.PrintToPlayer(player, "Failed to save preset.", MessageType.Error);
                return false;
            }
        }

        #endregion

        #region Load Preset Command

        /// <summary>
        /// Handle !loadpreset command
        /// </summary>
        private bool HandleLoadPresetCommand(CCSPlayerController? player, string[] args)
        {
            try
            {
                var presetName = args[0];

                // Check if preset exists
                if (!_presetManager.PresetExists(presetName))
                {
                    if (player != null)
                        ChatUtils.PrintToPlayer(player, $"Preset '{presetName}' not found. Use !listpresets to see available presets.", MessageType.Error);
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

        #region List Presets Command

        /// <summary>
        /// Handle !listpresets command
        /// </summary>
        private bool HandleListPresetsCommand(CCSPlayerController? player, string[] args)
        {
            try
            {
                var presets = _presetManager.GetPresets();
                var currentPreset = _presetManager.GetCurrentPresetName();

                if (presets.Count == 0)
                {
                    if (player != null)
                        ChatUtils.PrintToPlayer(player, "No presets saved. Use !savepreset to create one.", MessageType.Info);
                    else
                        Console.WriteLine("[CS2Utils] No presets saved");
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

        #region Delete Preset Command

        /// <summary>
        /// Handle !deletepreset command
        /// </summary>
        private bool HandleDeletePresetCommand(CCSPlayerController? player, string[] args)
        {
            try
            {
                var presetName = args[0];

                // Check if preset exists
                if (!_presetManager.PresetExists(presetName))
                {
                    if (player != null)
                        ChatUtils.PrintToPlayer(player, $"Preset '{presetName}' not found.", MessageType.Error);
                    return false;
                }

                // Delete the preset
                if (_presetManager.DeletePreset(presetName))
                {
                    var message = $"Deleted preset '{presetName}'";
                    if (player != null)
                        ChatUtils.PrintToPlayer(player, message, MessageType.Success);
                    else
                        Console.WriteLine($"[CS2Utils] {message}");
                    return true;
                }
                else
                {
                    if (player != null)
                        ChatUtils.PrintToPlayer(player, "Failed to delete preset.", MessageType.Error);
                    return false;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[CS2Utils] Error deleting preset: {ex.Message}");
                if (player != null)
                    ChatUtils.PrintToPlayer(player, "Failed to delete preset.", MessageType.Error);
                return false;
            }
        }

        #endregion

        #region Preset Info Command

        /// <summary>
        /// Handle !presetinfo command
        /// </summary>
        private bool HandlePresetInfoCommand(CCSPlayerController? player, string[] args)
        {
            try
            {
                var presetName = args[0];

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

        #region Current Preset Command

        /// <summary>
        /// Handle !currentpreset command
        /// </summary>
        private bool HandleCurrentPresetCommand(CCSPlayerController? player, string[] args)
        {
            try
            {
                var currentPreset = _presetManager.GetCurrentPresetName();
                
                if (string.IsNullOrEmpty(currentPreset))
                {
                    var message = "No preset currently loaded";
                    if (player != null)
                        ChatUtils.PrintToPlayer(player, message, MessageType.Info);
                    else
                        Console.WriteLine($"[CS2Utils] {message}");
                }
                else
                {
                    var message = $"Current preset: {currentPreset}";
                    if (player != null)
                        ChatUtils.PrintToPlayer(player, message, MessageType.Info);
                    else
                        Console.WriteLine($"[CS2Utils] {message}");
                }

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[CS2Utils] Error showing current preset: {ex.Message}");
                if (player != null)
                    ChatUtils.PrintToPlayer(player, "Failed to show current preset.", MessageType.Error);
                return false;
            }
        }

        #endregion
    }
}
