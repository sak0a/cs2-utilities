using System.Text.Json;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Cvars;
using CS2Utilities.Models;
using CS2Utilities.Config;

namespace CS2Utilities.Core
{
    /// <summary>
    /// Manages server state presets - hardcoded server configurations
    /// </summary>
    public class PresetManager
    {
        private readonly StateManager _stateManager;
        private readonly CS2UtilitiesConfig _config;
        private readonly Dictionary<string, ServerPreset> _hardcodedPresets;
        private Commands.PlayerCommands? _playerCommands;

        public PresetManager(StateManager stateManager, CS2UtilitiesConfig config)
        {
            _stateManager = stateManager;
            _config = config;
            _hardcodedPresets = new Dictionary<string, ServerPreset>();
            
            // Initialize hardcoded presets
            InitializeHardcodedPresets();
        }

        /// <summary>
        /// Set PlayerCommands instance for infinite money support
        /// </summary>
        public void SetPlayerCommands(Commands.PlayerCommands playerCommands)
        {
            _playerCommands = playerCommands;
        }

        /// <summary>
        /// Initialize hardcoded presets
        /// </summary>
        private void InitializeHardcodedPresets()
        {
            // Skin Inspecting Preset
            // Perfect for players to inspect weapon skins with plenty of time and money
            var skinInspectingPreset = new ServerPreset
            {
                Name = "skin_inspecting",
                Description = "Skin inspecting mode - long buy time, buy anywhere, lots of money",
                CreatedAt = DateTime.UtcNow,
                OnLoadCommands = new List<string>(),
                OnUnloadCommands = new List<string>(),
                States = new Dictionary<string, object>
                {
                    // Enable buy anywhere on the map
                    { "mp_buy_anywhere", 1 },
                    
                    // Very long buy time (999999 seconds = ~277 hours)
                    { "mp_buytime", 999999 },
                    
                    // Long round time for inspecting (60 minutes)
                    { "mp_roundtime_defuse", 60 },
                    { "mp_roundtime_hostage", 60 },
                    
                    // No freeze time - players can move immediately
                    { "mp_freezetime", 0 },
                    
                    // Lots of money to buy any weapon
                    { "mp_maxmoney", 999999 },
                    { "mp_startmoney", 999999 },
                    { "mp_afterroundmoney", 999999 },
                    
                    // Disable friendly fire for safety
                    { "mp_friendlyfire", 0 },
                    
                    // Allow knife dropping
                    { "mp_drop_knife_enable", 1 },
                    
                    // Enable infinite money for all players (money restored after purchases)
                    { "infinite_money_all", 1 },
                }
            };
            
            _hardcodedPresets["skin_inspecting"] = skinInspectingPreset;
            
            Console.WriteLine($"[CS2Utils] Initialized {_hardcodedPresets.Count} hardcoded preset(s)");
        }

        /// <summary>
        /// Save preset - DISABLED (hardcoded presets only)
        /// </summary>
        public bool SavePreset(string presetName, string description = "", List<string>? onLoadCommands = null, List<string>? onUnloadCommands = null)
        {
            Console.WriteLine($"[CS2Utils] SavePreset is disabled - only hardcoded presets are available");
            return false;
        }

        /// <summary>
        /// Load a preset and apply its settings
        /// </summary>
        public bool LoadPreset(string presetName, string? currentPresetName = null)
        {
            try
            {
                var key = presetName.ToLower();
                if (!_hardcodedPresets.ContainsKey(key))
                {
                    Console.WriteLine($"[CS2Utils] Preset '{presetName}' not found");
                    return false;
                }

                var preset = _hardcodedPresets[key];

                // Execute unload commands from current preset if specified
                if (!string.IsNullOrEmpty(currentPresetName) && _hardcodedPresets.ContainsKey(currentPresetName.ToLower()))
                {
                    var currentPreset = _hardcodedPresets[currentPresetName.ToLower()];
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
        /// Delete preset - DISABLED (hardcoded presets only)
        /// </summary>
        public bool DeletePreset(string presetName)
        {
            Console.WriteLine($"[CS2Utils] DeletePreset is disabled - only hardcoded presets are available");
            return false;
        }

        /// <summary>
        /// Get all available presets (hardcoded only)
        /// </summary>
        public Dictionary<string, ServerPreset> GetPresets()
        {
            return new Dictionary<string, ServerPreset>(_hardcodedPresets);
        }

        /// <summary>
        /// Get current preset name
        /// </summary>
        public string GetCurrentPresetName()
        {
            return _stateManager.GetState("current_preset", "");
        }

        /// <summary>
        /// Check if a preset exists (hardcoded presets only)
        /// </summary>
        public bool PresetExists(string presetName)
        {
            return _hardcodedPresets.ContainsKey(presetName.ToLower());
        }

        /// <summary>
        /// Add commands to preset - DISABLED (hardcoded presets only)
        /// </summary>
        public bool AddCommandsToPreset(string presetName, List<string> commands, bool isLoadCommands = true)
        {
            Console.WriteLine($"[CS2Utils] AddCommandsToPreset is disabled - only hardcoded presets are available");
            return false;
        }

        /// <summary>
        /// Remove commands from preset - DISABLED (hardcoded presets only)
        /// </summary>
        public bool RemoveCommandsFromPreset(string presetName, List<string> commands, bool isLoadCommands = true)
        {
            Console.WriteLine($"[CS2Utils] RemoveCommandsFromPreset is disabled - only hardcoded presets are available");
            return false;
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
            Console.WriteLine($"[CS2Utils] Applying {preset.States.Count} states from preset '{preset.Name}'");
            
            foreach (var kvp in preset.States)
            {
                try
                {
                    Console.WriteLine($"[CS2Utils] Applying state: {kvp.Key} = {kvp.Value} (type: {kvp.Value?.GetType().Name ?? "null"})");
                    
                    // Apply the state immediately - this will also save to StateManager
                    ApplyStateImmediately(kvp.Key, kvp.Value);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[CS2Utils] Error applying state '{kvp.Key}': {ex.Message}");
                    Console.WriteLine($"[CS2Utils] Stack trace: {ex.StackTrace}");
                }
            }
            
            Console.WriteLine($"[CS2Utils] Finished applying preset states");
        }

        /// <summary>
        /// Apply a state immediately to the server
        /// </summary>
        private void ApplyStateImmediately(string key, object value)
        {
            try
            {
                // Convert value to appropriate type
                var typedValue = ConvertValueToAppropriateType(value);
                
                // Console commands (mp_*, bot_*, ammo_*) should always use Server.ExecuteCommand
                // This matches how the command handlers execute them and is more reliable
                if (key.StartsWith("mp_") || key.StartsWith("bot_") || key.StartsWith("ammo_"))
                {
                    Server.ExecuteCommand($"{key} {typedValue}");
                    _stateManager.SaveState(key, typedValue);
                    Console.WriteLine($"[CS2Utils] Applied state '{key}' = {typedValue} via Server.ExecuteCommand");
                    return;
                }
                
                // Special cases that need specific command handler methods
                switch (key)
                {
                    case "infinite_money_all":
                        // Enable infinite money for all players
                        if (typedValue is int infMoneyAllValue && infMoneyAllValue == 1)
                        {
                            ApplyInfiniteMoneyToAllPlayers();
                            _stateManager.SaveState(key, typedValue);
                            Console.WriteLine($"[CS2Utils] Applied state '{key}' = {typedValue} (enabled infinite money for all players)");
                        }
                        else
                        {
                            // Disable infinite money for all players
                            RemoveInfiniteMoneyFromAllPlayers();
                            _stateManager.SaveState(key, typedValue);
                            Console.WriteLine($"[CS2Utils] Applied state '{key}' = {typedValue} (disabled infinite money for all players)");
                        }
                        break;
                        
                    case "infinite_money_ct":
                        // Enable infinite money for CT team
                        if (typedValue is int infMoneyCtValue && infMoneyCtValue == 1)
                        {
                            ApplyInfiniteMoneyToTeam("ct");
                            _stateManager.SaveState(key, typedValue);
                            Console.WriteLine($"[CS2Utils] Applied state '{key}' = {typedValue} (enabled infinite money for CT team)");
                        }
                        else
                        {
                            RemoveInfiniteMoneyFromTeam("ct");
                            _stateManager.SaveState(key, typedValue);
                            Console.WriteLine($"[CS2Utils] Applied state '{key}' = {typedValue} (disabled infinite money for CT team)");
                        }
                        break;
                        
                    case "infinite_money_t":
                        // Enable infinite money for T team
                        if (typedValue is int infMoneyTValue && infMoneyTValue == 1)
                        {
                            ApplyInfiniteMoneyToTeam("t");
                            _stateManager.SaveState(key, typedValue);
                            Console.WriteLine($"[CS2Utils] Applied state '{key}' = {typedValue} (enabled infinite money for T team)");
                        }
                        else
                        {
                            RemoveInfiniteMoneyFromTeam("t");
                            _stateManager.SaveState(key, typedValue);
                            Console.WriteLine($"[CS2Utils] Applied state '{key}' = {typedValue} (disabled infinite money for T team)");
                        }
                        break;
                        
                    case "money_all":
                        // Set money for all players
                        if (_playerCommands != null && typedValue is int moneyAllValue)
                        {
                            _playerCommands.ExecuteMoneyCommand(null, new[] { "@all", moneyAllValue.ToString() });
                            _stateManager.SaveState(key, typedValue);
                            Console.WriteLine($"[CS2Utils] Applied state '{key}' = {typedValue} (set money for all players)");
                        }
                        break;
                        
                    case "money_ct":
                        // Set money for CT team
                        if (_playerCommands != null && typedValue is int moneyCtValue)
                        {
                            _playerCommands.ExecuteMoneyCommand(null, new[] { "@ct", moneyCtValue.ToString() });
                            _stateManager.SaveState(key, typedValue);
                            Console.WriteLine($"[CS2Utils] Applied state '{key}' = {typedValue} (set money for CT team)");
                        }
                        break;
                        
                    case "money_t":
                        // Set money for T team
                        if (_playerCommands != null && typedValue is int moneyTValue)
                        {
                            _playerCommands.ExecuteMoneyCommand(null, new[] { "@t", moneyTValue.ToString() });
                            _stateManager.SaveState(key, typedValue);
                            Console.WriteLine($"[CS2Utils] Applied state '{key}' = {typedValue} (set money for T team)");
                        }
                        break;
                        
                    case "health_all":
                        // Set health for all players
                        if (_playerCommands != null && typedValue is int healthAllValue)
                        {
                            _playerCommands.ExecuteHealthCommand(null, new[] { "@all", healthAllValue.ToString() });
                            _stateManager.SaveState(key, typedValue);
                            Console.WriteLine($"[CS2Utils] Applied state '{key}' = {typedValue} (set health for all players)");
                        }
                        break;
                        
                    case "health_ct":
                        // Set health for CT team
                        if (_playerCommands != null && typedValue is int healthCtValue)
                        {
                            _playerCommands.ExecuteHealthCommand(null, new[] { "@ct", healthCtValue.ToString() });
                            _stateManager.SaveState(key, typedValue);
                            Console.WriteLine($"[CS2Utils] Applied state '{key}' = {typedValue} (set health for CT team)");
                        }
                        break;
                        
                    case "health_t":
                        // Set health for T team
                        if (_playerCommands != null && typedValue is int healthTValue)
                        {
                            _playerCommands.ExecuteHealthCommand(null, new[] { "@t", healthTValue.ToString() });
                            _stateManager.SaveState(key, typedValue);
                            Console.WriteLine($"[CS2Utils] Applied state '{key}' = {typedValue} (set health for T team)");
                        }
                        break;
                        
                    case "bhop_mode":
                        // This needs to be handled by AdvancedPlayerCommands.ApplyBhopMode()
                        // For now, just save the state - it should be applied when the command is used
                        _stateManager.SaveState(key, typedValue);
                        Console.WriteLine($"[CS2Utils] State '{key}' = {typedValue} saved (requires command handler to apply)");
                        break;
                        
                    case "infinite_ammo_mode":
                        // This needs to be handled by GameStateCommands.ApplyInfiniteAmmoMode()
                        // For now, just save the state - it should be applied when the command is used
                        _stateManager.SaveState(key, typedValue);
                        Console.WriteLine($"[CS2Utils] State '{key}' = {typedValue} saved (requires command handler to apply)");
                        break;
                        
                    default:
                        // Try ConVar API for other CVars
                        var convar = ConVar.Find(key);
                        if (convar != null)
                        {
                            // Use ConVar API to set value directly
                            if (typedValue is int intValue)
                            {
                                convar.SetValue(intValue);
                                _stateManager.SaveState(key, intValue);
                            }
                            else if (typedValue is float floatValue)
                            {
                                convar.SetValue(floatValue);
                                _stateManager.SaveState(key, floatValue);
                            }
                            else if (typedValue is string stringValue)
                            {
                                convar.SetValue(stringValue);
                                _stateManager.SaveState(key, stringValue);
                            }
                            else if (typedValue is bool boolValue)
                            {
                                convar.SetValue(boolValue);
                                _stateManager.SaveState(key, boolValue ? 1 : 0);
                            }
                            else
                            {
                                // Fallback to string conversion
                                convar.SetValue(typedValue?.ToString() ?? "");
                                _stateManager.SaveState(key, typedValue);
                            }
                            
                            Console.WriteLine($"[CS2Utils] Applied state '{key}' = {typedValue} via ConVar API");
                        }
                        else
                        {
                            // No ConVar found, just save the state
                            _stateManager.SaveState(key, typedValue);
                            Console.WriteLine($"[CS2Utils] State '{key}' = {typedValue} saved (ConVar not found)");
                        }
                        break;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[CS2Utils] Error applying state '{key}' immediately: {ex.Message}");
                Console.WriteLine($"[CS2Utils] Stack trace: {ex.StackTrace}");
            }
        }

        /// <summary>
        /// Convert a value to the appropriate type, handling JsonElement deserialization
        /// </summary>
        private object ConvertValueToAppropriateType(object value)
        {
            if (value == null)
                return 0;

            // Handle JsonElement from JSON deserialization
            if (value is JsonElement jsonElement)
            {
                switch (jsonElement.ValueKind)
                {
                    case System.Text.Json.JsonValueKind.Number:
                        if (jsonElement.TryGetInt32(out var intVal))
                            return intVal;
                        if (jsonElement.TryGetInt64(out var longVal))
                            return (int)longVal;
                        if (jsonElement.TryGetDouble(out var doubleVal))
                            return (float)doubleVal;
                        return 0;
                        
                    case System.Text.Json.JsonValueKind.String:
                        return jsonElement.GetString() ?? "";
                        
                    case System.Text.Json.JsonValueKind.True:
                        return 1;
                        
                    case System.Text.Json.JsonValueKind.False:
                        return 0;
                        
                    default:
                        return value.ToString() ?? "";
                }
            }

            // Handle string representation of numbers
            if (value is string strValue)
            {
                if (int.TryParse(strValue, out var intVal))
                    return intVal;
                if (float.TryParse(strValue, out var floatVal))
                    return floatVal;
                if (bool.TryParse(strValue, out var boolVal))
                    return boolVal ? 1 : 0;
                return strValue;
            }

            // Already the right type
            return value;
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
        /// Apply infinite money to all players
        /// </summary>
        private void ApplyInfiniteMoneyToAllPlayers()
        {
            try
            {
                if (_playerCommands == null)
                {
                    Console.WriteLine($"[CS2Utils] Warning: PlayerCommands not set, cannot enable infinite money for all players");
                    return;
                }

                // Use PlayerCommands to execute money command for all players
                _playerCommands.ExecuteMoneyCommand(null, new[] { "@all", "-1" });
                Console.WriteLine($"[CS2Utils] Enabled infinite money for all players via money command");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[CS2Utils] Error applying infinite money to all players: {ex.Message}");
            }
        }

        /// <summary>
        /// Remove infinite money from all players
        /// </summary>
        private void RemoveInfiniteMoneyFromAllPlayers()
        {
            try
            {
                if (_playerCommands == null)
                {
                    Console.WriteLine($"[CS2Utils] Warning: PlayerCommands not set, cannot disable infinite money for all players");
                    return;
                }

                // Use PlayerCommands to clear infinite money for all players
                _playerCommands.ExecuteMoneyCommand(null, new[] { "@all", "0" });
                Console.WriteLine($"[CS2Utils] Disabled infinite money for all players via money command");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[CS2Utils] Error removing infinite money from all players: {ex.Message}");
            }
        }

        /// <summary>
        /// Apply infinite money to a specific team
        /// </summary>
        private void ApplyInfiniteMoneyToTeam(string team)
        {
            try
            {
                if (_playerCommands == null)
                {
                    Console.WriteLine($"[CS2Utils] Warning: PlayerCommands not set, cannot enable infinite money for {team} team");
                    return;
                }

                // Use PlayerCommands to execute money command for team
                var teamTarget = team.ToLower() == "ct" ? "@ct" : "@t";
                _playerCommands.ExecuteMoneyCommand(null, new[] { teamTarget, "-1" });
                Console.WriteLine($"[CS2Utils] Enabled infinite money for {team} team via money command");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[CS2Utils] Error applying infinite money to {team} team: {ex.Message}");
            }
        }

        /// <summary>
        /// Remove infinite money from a specific team
        /// </summary>
        private void RemoveInfiniteMoneyFromTeam(string team)
        {
            try
            {
                if (_playerCommands == null)
                {
                    Console.WriteLine($"[CS2Utils] Warning: PlayerCommands not set, cannot disable infinite money for {team} team");
                    return;
                }

                // Use PlayerCommands to clear infinite money for team
                var teamTarget = team.ToLower() == "ct" ? "@ct" : "@t";
                _playerCommands.ExecuteMoneyCommand(null, new[] { teamTarget, "0" });
                Console.WriteLine($"[CS2Utils] Disabled infinite money for {team} team via money command");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[CS2Utils] Error removing infinite money from {team} team: {ex.Message}");
            }
        }

        /// <summary>
        /// Get the server's current mp_maxmoney setting
        /// </summary>
        private int GetServerMaxMoney()
        {
            try
            {
                var maxMoneyConvar = ConVar.Find("mp_maxmoney");
                if (maxMoneyConvar != null)
                {
                    return maxMoneyConvar.GetPrimitiveValue<int>();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[CS2Utils] Error getting mp_maxmoney convar: {ex.Message}");
            }

            // Default CS2 max money if we can't get the convar
            return 16000;
        }

        /// <summary>
        /// Save the config to disk to persist preset changes
        /// </summary>
        private void SaveConfigToDisk()
        {
            try
            {
                // Use the same approach as in CS2Utilities.cs for saving config
                var configPath = Path.Combine(CounterStrikeSharp.API.Server.GameDirectory, "csgo", "addons", "counterstrikesharp", "configs", "plugins", "CS2Utilities", "CS2Utilities.json");

                // Ensure directory exists
                var configDir = Path.GetDirectoryName(configPath);
                if (!string.IsNullOrEmpty(configDir) && !Directory.Exists(configDir))
                {
                    Directory.CreateDirectory(configDir);
                }

                // Serialize config with proper formatting
                var json = System.Text.Json.JsonSerializer.Serialize(_config, new System.Text.Json.JsonSerializerOptions
                {
                    WriteIndented = true,
                    PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase,
                    Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
                });

                File.WriteAllText(configPath, json);

                if (_config.EnableDebugLogging)
                {
                    Console.WriteLine($"[CS2Utils] Config saved to disk with {_config.Presets.Count} presets");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[CS2Utils] Error saving config to disk: {ex.Message}");
            }
        }

        // Presets are now stored in the main config file and automatically saved by CounterStrikeSharp
    }
}
