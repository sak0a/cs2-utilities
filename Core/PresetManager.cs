using System.Text.Json;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CS2Utilities.Models;

namespace CS2Utilities.Core
{
    /// <summary>
    /// Manages server state presets - save/load custom server configurations
    /// </summary>
    public class PresetManager
    {
        private readonly StateManager _stateManager;
        private readonly string _presetsFilePath;
        private Dictionary<string, ServerPreset> _presets;

        public PresetManager(StateManager stateManager)
        {
            _stateManager = stateManager;
            _presetsFilePath = Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location)!, "presets.json");
            _presets = new Dictionary<string, ServerPreset>();
            LoadPresets();
        }

        /// <summary>
        /// Save current server state as a preset
        /// </summary>
        public bool SavePreset(string presetName, string description = "", List<string>? onLoadCommands = null, List<string>? onUnloadCommands = null)
        {
            try
            {
                var preset = new ServerPreset
                {
                    Name = presetName,
                    Description = description,
                    CreatedAt = DateTime.UtcNow,
                    OnLoadCommands = onLoadCommands ?? new List<string>(),
                    OnUnloadCommands = onUnloadCommands ?? new List<string>(),
                    States = CaptureCurrentStates()
                };

                _presets[presetName.ToLower()] = preset;
                SavePresets();

                Console.WriteLine($"[CS2Utils] Saved preset '{presetName}' with {preset.States.Count} settings");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[CS2Utils] Error saving preset '{presetName}': {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Load a preset and apply its settings
        /// </summary>
        public bool LoadPreset(string presetName, string? currentPresetName = null)
        {
            try
            {
                var key = presetName.ToLower();
                if (!_presets.ContainsKey(key))
                {
                    Console.WriteLine($"[CS2Utils] Preset '{presetName}' not found");
                    return false;
                }

                var preset = _presets[key];

                // Execute unload commands from current preset if specified
                if (!string.IsNullOrEmpty(currentPresetName) && _presets.ContainsKey(currentPresetName.ToLower()))
                {
                    var currentPreset = _presets[currentPresetName.ToLower()];
                    ExecuteCommands(currentPreset.OnUnloadCommands, $"unloading preset '{currentPresetName}'");
                }

                // Apply preset states
                ApplyPresetStates(preset);

                // Execute load commands
                ExecuteCommands(preset.OnLoadCommands, $"loading preset '{presetName}'");

                // Save current preset name
                _stateManager.SaveState("current_preset", presetName);

                Console.WriteLine($"[CS2Utils] Loaded preset '{presetName}' with {preset.States.Count} settings");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[CS2Utils] Error loading preset '{presetName}': {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Delete a preset
        /// </summary>
        public bool DeletePreset(string presetName)
        {
            try
            {
                var key = presetName.ToLower();
                if (!_presets.ContainsKey(key))
                {
                    return false;
                }

                _presets.Remove(key);
                SavePresets();

                Console.WriteLine($"[CS2Utils] Deleted preset '{presetName}'");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[CS2Utils] Error deleting preset '{presetName}': {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Get all available presets
        /// </summary>
        public Dictionary<string, ServerPreset> GetPresets()
        {
            return new Dictionary<string, ServerPreset>(_presets);
        }

        /// <summary>
        /// Get current preset name
        /// </summary>
        public string GetCurrentPresetName()
        {
            return _stateManager.GetState("current_preset", "");
        }

        /// <summary>
        /// Check if a preset exists
        /// </summary>
        public bool PresetExists(string presetName)
        {
            return _presets.ContainsKey(presetName.ToLower());
        }

        /// <summary>
        /// Add commands to a preset
        /// </summary>
        public bool AddCommandsToPreset(string presetName, List<string> commands, bool isLoadCommands = true)
        {
            try
            {
                var key = presetName.ToLower();
                if (!_presets.ContainsKey(key))
                {
                    return false;
                }

                var preset = _presets[key];
                var targetList = isLoadCommands ? preset.OnLoadCommands : preset.OnUnloadCommands;
                
                foreach (var command in commands)
                {
                    if (!targetList.Contains(command))
                    {
                        targetList.Add(command);
                    }
                }

                SavePresets();
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[CS2Utils] Error adding commands to preset '{presetName}': {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Remove commands from a preset
        /// </summary>
        public bool RemoveCommandsFromPreset(string presetName, List<string> commands, bool isLoadCommands = true)
        {
            try
            {
                var key = presetName.ToLower();
                if (!_presets.ContainsKey(key))
                {
                    return false;
                }

                var preset = _presets[key];
                var targetList = isLoadCommands ? preset.OnLoadCommands : preset.OnUnloadCommands;
                
                foreach (var command in commands)
                {
                    targetList.Remove(command);
                }

                SavePresets();
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[CS2Utils] Error removing commands from preset '{presetName}': {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Capture current server states
        /// </summary>
        private Dictionary<string, object> CaptureCurrentStates()
        {
            var states = new Dictionary<string, object>();

            // Get all current states from StateManager
            var stateKeys = new[]
            {
                // Game state settings
                "bhop_mode", "last_bhop_mode", "infinite_ammo_mode", "last_infinite_ammo_mode",
                "game_paused", "damage_disabled", "mp_drop_knife_enable", "mp_friendlyfire",
                
                // Grenade limits
                "ammo_grenade_limit_flashbang", "ammo_grenade_limit_he", "ammo_grenade_limit_smoke",
                "ammo_grenade_limit_decoy", "ammo_grenade_limit_molotov",
                
                // Money settings
                "mp_maxmoney", "mp_startmoney", "mp_afterroundmoney",
                
                // Time settings
                "mp_warmuptime", "mp_roundtime_defuse", "mp_roundtime_hostage", 
                "mp_freezetime", "mp_round_restart_delay", "mp_buytime",
                
                // Other settings
                "mp_buy_anywhere", "mp_autoteambalance", "mp_limitteams", "bot_difficulty", "bot_quota",
                
                // Advanced player settings
                "default_primary", "default_secondary"
            };

            foreach (var key in stateKeys)
            {
                if (_stateManager.HasState(key))
                {
                    states[key] = _stateManager.GetState<object>(key);
                }
            }

            return states;
        }

        /// <summary>
        /// Apply preset states to current server
        /// </summary>
        private void ApplyPresetStates(ServerPreset preset)
        {
            foreach (var kvp in preset.States)
            {
                try
                {
                    _stateManager.SaveState(kvp.Key, kvp.Value);
                    
                    // Apply the state immediately based on the key
                    ApplyStateImmediately(kvp.Key, kvp.Value);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[CS2Utils] Error applying state '{kvp.Key}': {ex.Message}");
                }
            }
        }

        /// <summary>
        /// Apply a state immediately to the server
        /// </summary>
        private void ApplyStateImmediately(string key, object value)
        {
            try
            {
                switch (key)
                {
                    // Console commands that can be executed directly
                    case var k when k.StartsWith("mp_") || k.StartsWith("bot_") || k.StartsWith("ammo_"):
                        Server.ExecuteCommand($"{key} {value}");
                        break;
                        
                    // Special cases that need specific handling
                    case "bhop_mode":
                        // This would need to be handled by the AdvancedPlayerCommands class
                        break;
                    case "infinite_ammo_mode":
                        // This would need to be handled by the GameStateCommands class
                        break;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[CS2Utils] Error applying state '{key}' immediately: {ex.Message}");
            }
        }

        /// <summary>
        /// Execute a list of commands
        /// </summary>
        private void ExecuteCommands(List<string> commands, string context)
        {
            foreach (var command in commands)
            {
                try
                {
                    if (command.StartsWith("!"))
                    {
                        // This is a plugin command - would need integration with CommandManager
                        Console.WriteLine($"[CS2Utils] Plugin command in {context}: {command}");
                    }
                    else
                    {
                        // This is a server console command
                        Server.ExecuteCommand(command);
                        Console.WriteLine($"[CS2Utils] Executed command in {context}: {command}");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[CS2Utils] Error executing command '{command}' in {context}: {ex.Message}");
                }
            }
        }

        /// <summary>
        /// Load presets from file
        /// </summary>
        private void LoadPresets()
        {
            try
            {
                if (File.Exists(_presetsFilePath))
                {
                    var json = File.ReadAllText(_presetsFilePath);
                    var presets = JsonSerializer.Deserialize<Dictionary<string, ServerPreset>>(json);
                    _presets = presets ?? new Dictionary<string, ServerPreset>();
                    Console.WriteLine($"[CS2Utils] Loaded {_presets.Count} presets");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[CS2Utils] Error loading presets: {ex.Message}");
                _presets = new Dictionary<string, ServerPreset>();
            }
        }

        /// <summary>
        /// Save presets to file
        /// </summary>
        private void SavePresets()
        {
            try
            {
                var json = JsonSerializer.Serialize(_presets, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(_presetsFilePath, json);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[CS2Utils] Error saving presets: {ex.Message}");
            }
        }
    }
}
