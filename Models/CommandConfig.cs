using System.Text.Json.Serialization;

namespace CS2Utilities.Models
{
    /// <summary>
    /// Configuration for individual commands
    /// </summary>
    public class CommandConfig
    {
        /// <summary>
        /// Whether the command is enabled
        /// </summary>
        [JsonPropertyName("enabled")]
        public bool Enabled { get; set; } = true;

        /// <summary>
        /// Permission required to use this command
        /// </summary>
        [JsonPropertyName("permission")]
        public string Permission { get; set; } = "@css/root";

        /// <summary>
        /// Description of what the command does
        /// </summary>
        [JsonPropertyName("description")]
        public string Description { get; set; } = "";

        /// <summary>
        /// Usage example for the command
        /// </summary>
        [JsonPropertyName("usage")]
        public string Usage { get; set; } = "";

        /// <summary>
        /// Alternative names for the command
        /// </summary>
        [JsonPropertyName("aliases")]
        public List<string> Aliases { get; set; } = new();

        /// <summary>
        /// Cooldown in seconds between command uses (0 = no cooldown)
        /// </summary>
        [JsonPropertyName("cooldown")]
        public int Cooldown { get; set; } = 0;

        /// <summary>
        /// Minimum arguments required for the command
        /// </summary>
        [JsonPropertyName("minArgs")]
        public int MinArgs { get; set; } = 0;

        /// <summary>
        /// Maximum arguments allowed for the command
        /// </summary>
        [JsonPropertyName("maxArgs")]
        public int MaxArgs { get; set; } = 10;

        /// <summary>
        /// Whether the command can be used from console
        /// </summary>
        [JsonPropertyName("consoleEnabled")]
        public bool ConsoleEnabled { get; set; } = true;

        /// <summary>
        /// Whether the command can be used from chat
        /// </summary>
        [JsonPropertyName("chatEnabled")]
        public bool ChatEnabled { get; set; } = true;
    }
}
