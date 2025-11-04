using CounterStrikeSharp.API.Core;
using System.Text.Json.Serialization;
using CS2Utilities.Models;

namespace CS2Utilities.Config
{
    /// <summary>
    /// Comprehensive configuration class for CS2 Utilities plugin
    /// </summary>
    public class CS2UtilitiesConfig : BasePluginConfig
    {
        /// <summary>
        /// Configuration version for migration purposes
        /// </summary>
        public override int Version { get; set; } = 1;

        /// <summary>
        /// List of commands that are disabled and should not be registered
        /// </summary>
        [JsonPropertyName("DisabledCommands")]
        public List<string> DisabledCommands { get; set; } = new();

        /// <summary>
        /// Command-specific permission mappings
        /// </summary>
        [JsonPropertyName("CommandPermissions")]
        public List<CommandPermission> CommandPermissions { get; set; } = new();

        /// <summary>
        /// Saved server states that can be restored
        /// </summary>
        [JsonPropertyName("SavedState")]
        public List<SavedState> SavedState { get; set; } = new();

        /// <summary>
        /// Chat prefix displayed before all plugin messages
        /// </summary>
        [JsonPropertyName("ChatPrefix")]
        public string ChatPrefix { get; set; } = "[CS2Utils]";

        /// <summary>
        /// Default permission required for commands that don't have specific permissions set
        /// </summary>
        [JsonPropertyName("DefaultPermission")]
        public string DefaultPermission { get; set; } = "@css/root";

        /// <summary>
        /// Whether to enable debug logging
        /// </summary>
        [JsonPropertyName("EnableDebugLogging")]
        public bool EnableDebugLogging { get; set; } = false;

        /// <summary>
        /// Whether to automatically save state changes to disk
        /// </summary>
        [JsonPropertyName("AutoSaveState")]
        public bool AutoSaveState { get; set; } = true;

        /// <summary>
        /// Interval in seconds for automatic state saving (if enabled)
        /// </summary>
        [JsonPropertyName("AutoSaveInterval")]
        public int AutoSaveInterval { get; set; } = 300; // 5 minutes

        /// <summary>
        /// Whether to restore saved states on plugin load
        /// </summary>
        [JsonPropertyName("RestoreStateOnLoad")]
        public bool RestoreStateOnLoad { get; set; } = true;

        /// <summary>
        /// Maximum number of saved states to keep in memory
        /// </summary>
        [JsonPropertyName("MaxSavedStates")]
        public int MaxSavedStates { get; set; } = 100;

        /// <summary>
        /// Whether to log command executions for audit purposes
        /// </summary>
        [JsonPropertyName("LogCommandExecutions")]
        public bool LogCommandExecutions { get; set; } = true;

        /// <summary>
        /// Rate limiting: Maximum commands per player per minute
        /// </summary>
        [JsonPropertyName("CommandRateLimit")]
        public int CommandRateLimit { get; set; } = 30;

        /// <summary>
        /// Whether to enable command cooldowns
        /// </summary>
        [JsonPropertyName("EnableCommandCooldowns")]
        public bool EnableCommandCooldowns { get; set; } = false;

        /// <summary>
        /// Default cooldown in seconds for commands (if cooldowns are enabled)
        /// </summary>
        [JsonPropertyName("DefaultCommandCooldown")]
        public int DefaultCommandCooldown { get; set; } = 1;

        /// <summary>
        /// Chat color theme settings
        /// </summary>
        [JsonPropertyName("ChatColors")]
        public ChatColorSettings ChatColors { get; set; } = new();
    }

    /// <summary>
    /// Chat color theme configuration
    /// </summary>
    public class ChatColorSettings
    {
        /// <summary>
        /// Color for the chat prefix
        /// </summary>
        [JsonPropertyName("PrefixColor")]
        public string PrefixColor { get; set; } = "DarkBlue";

        /// <summary>
        /// Default text color
        /// </summary>
        [JsonPropertyName("DefaultColor")]
        public string DefaultColor { get; set; } = "Grey";

        /// <summary>
        /// Highlight color for important information
        /// </summary>
        [JsonPropertyName("HighlightColor")]
        public string HighlightColor { get; set; } = "Yellow";

        /// <summary>
        /// Color for enabled/success states
        /// </summary>
        [JsonPropertyName("EnabledColor")]
        public string EnabledColor { get; set; } = "Green";

        /// <summary>
        /// Color for disabled/error states
        /// </summary>
        [JsonPropertyName("DisabledColor")]
        public string DisabledColor { get; set; } = "Red";

        /// <summary>
        /// Color for warning messages
        /// </summary>
        [JsonPropertyName("WarningColor")]
        public string WarningColor { get; set; } = "Orange";

        /// <summary>
        /// Color for informational messages
        /// </summary>
        [JsonPropertyName("InfoColor")]
        public string InfoColor { get; set; } = "LightBlue";
    }
}
