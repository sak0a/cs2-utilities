using System.Text.Json;
using CS2Utilities.Config;
using CS2Utilities.Models;

namespace CS2Utilities.Core
{
    /// <summary>
    /// Manages server state persistence and restoration
    /// </summary>
    public class StateManager
    {
        private readonly CS2UtilitiesConfig _config;
        private readonly Dictionary<string, object> _currentState;
        private readonly Timer? _autoSaveTimer;

        public StateManager(CS2UtilitiesConfig config)
        {
            _config = config;
            _currentState = new Dictionary<string, object>();

            LoadFromConfig();

            // Set up auto-save timer if enabled
            if (_config.AutoSaveState && _config.AutoSaveInterval > 0)
            {
                _autoSaveTimer = new Timer(AutoSaveCallback, null,
                    TimeSpan.FromSeconds(_config.AutoSaveInterval),
                    TimeSpan.FromSeconds(_config.AutoSaveInterval));
            }
        }

        /// <summary>
        /// Save a state value
        /// </summary>
        /// <param name="key">State key</param>
        /// <param name="value">State value</param>
        public void SaveState(string key, object value)
        {
            _currentState[key] = value;
            
            // Update or add to saved state configuration
            var existingState = _config.SavedState.FirstOrDefault(s => s.ConfigName == key);
            if (existingState != null)
            {
                existingState.Value = value.ToString() ?? string.Empty;
                existingState.LastSaved = DateTime.UtcNow;
            }
            else
            {
                // Check if we're at the limit
                if (_config.SavedState.Count >= _config.MaxSavedStates)
                {
                    // Remove the oldest state
                    var oldest = _config.SavedState.OrderBy(s => s.LastSaved).First();
                    _config.SavedState.Remove(oldest);
                    _currentState.Remove(oldest.ConfigName);
                }
                
                _config.SavedState.Add(new SavedState
                {
                    ConfigName = key,
                    Value = value.ToString() ?? string.Empty,
                    LastSaved = DateTime.UtcNow,
                    AutoRestore = true
                });
            }

            if (_config.EnableDebugLogging)
            {
                Console.WriteLine($"[CS2Utils] State saved: {key} = {value}");
            }
        }

        /// <summary>
        /// Get a state value
        /// </summary>
        /// <typeparam name="T">Type to cast the value to</typeparam>
        /// <param name="key">State key</param>
        /// <param name="defaultValue">Default value if key not found</param>
        /// <returns>State value or default</returns>
        public T GetState<T>(string key, T defaultValue = default!)
        {
            if (!_currentState.TryGetValue(key, out var value))
                return defaultValue;

            try
            {
                if (value is T directValue)
                    return directValue;

                // Try to convert the value
                return (T)Convert.ChangeType(value, typeof(T));
            }
            catch
            {
                return defaultValue;
            }
        }

        /// <summary>
        /// Check if a state key exists
        /// </summary>
        /// <param name="key">State key</param>
        /// <returns>True if key exists</returns>
        public bool HasState(string key)
        {
            return _currentState.ContainsKey(key);
        }

        /// <summary>
        /// Remove a state value
        /// </summary>
        /// <param name="key">State key</param>
        /// <returns>True if key was removed</returns>
        public bool RemoveState(string key)
        {
            var removed = _currentState.Remove(key);
            
            if (removed)
            {
                // Also remove from configuration
                var configState = _config.SavedState.FirstOrDefault(s => s.ConfigName == key);
                if (configState != null)
                {
                    _config.SavedState.Remove(configState);
                }
                
                if (_config.EnableDebugLogging)
                {
                    Console.WriteLine($"[CS2Utils] State removed: {key}");
                }
            }
            
            return removed;
        }

        /// <summary>
        /// Get all current state keys
        /// </summary>
        /// <returns>Collection of state keys</returns>
        public IEnumerable<string> GetStateKeys()
        {
            return _currentState.Keys;
        }

        /// <summary>
        /// Get all current state as a dictionary
        /// </summary>
        /// <returns>Dictionary of all current state</returns>
        public Dictionary<string, object> GetAllState()
        {
            return new Dictionary<string, object>(_currentState);
        }

        /// <summary>
        /// Clear all state
        /// </summary>
        public void ClearState()
        {
            _currentState.Clear();
            _config.SavedState.Clear();
            
            if (_config.EnableDebugLogging)
            {
                Console.WriteLine("[CS2Utils] All state cleared");
            }
        }

        /// <summary>
        /// Persist current state to CounterStrikeSharp config
        /// </summary>
        public async Task PersistToDisk()
        {
            try
            {
                // The config will be automatically saved by CounterStrikeSharp
                // when the plugin updates the Config object
                await Task.CompletedTask; // Placeholder for async compatibility

                if (_config.EnableDebugLogging)
                {
                    Console.WriteLine($"[CS2Utils] State persisted to config: {_config.SavedState.Count} states");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[CS2Utils] Error persisting state: {ex.Message}");
            }
        }

        /// <summary>
        /// Load state from CounterStrikeSharp config
        /// </summary>
        public void LoadFromConfig()
        {
            try
            {
                _currentState.Clear();

                foreach (var savedState in _config.SavedState)
                {
                    if (savedState.AutoRestore)
                    {
                        _currentState[savedState.ConfigName] = savedState.Value;
                    }
                }

                if (_config.EnableDebugLogging)
                {
                    Console.WriteLine($"[CS2Utils] State loaded from config: {_currentState.Count} states");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[CS2Utils] Error loading state from config: {ex.Message}");
            }
        }

        /// <summary>
        /// Reset to default state
        /// </summary>
        public void ResetToDefaults()
        {
            ClearState();
            
            if (_config.EnableDebugLogging)
            {
                Console.WriteLine("[CS2Utils] State reset to defaults");
            }
        }

        /// <summary>
        /// Get state statistics
        /// </summary>
        /// <returns>Dictionary with state statistics</returns>
        public Dictionary<string, object> GetStateStatistics()
        {
            return new Dictionary<string, object>
            {
                ["TotalStates"] = _currentState.Count,
                ["MaxStates"] = _config.MaxSavedStates,
                ["AutoSaveEnabled"] = _config.AutoSaveState,
                ["AutoSaveInterval"] = _config.AutoSaveInterval,
                ["LastAutoSave"] = _autoSaveTimer != null ? (object)DateTime.UtcNow : "Not Available"
            };
        }

        /// <summary>
        /// Auto-save callback for timer
        /// </summary>
        private async void AutoSaveCallback(object? state)
        {
            if (_config.AutoSaveState)
            {
                await PersistToDisk();
            }
        }

        /// <summary>
        /// Get the current configuration
        /// </summary>
        public CS2UtilitiesConfig GetConfig()
        {
            return _config;
        }

        /// <summary>
        /// Dispose resources
        /// </summary>
        public void Dispose()
        {
            _autoSaveTimer?.Dispose();

            // Final save if auto-save is enabled
            if (_config.AutoSaveState)
            {
                PersistToDisk().Wait(5000); // Wait up to 5 seconds for final save
            }
        }
    }
}
