using System.Text.Json.Serialization;

namespace CS2Utilities.Models
{
    /// <summary>
    /// Represents a saved server state configuration that can be persisted and restored
    /// </summary>
    public class SavedState
    {
        /// <summary>
        /// The configuration name/key (e.g., "afterRoundMoney", "warmupTime")
        /// </summary>
        [JsonPropertyName("ConfigName")]
        public string ConfigName { get; set; } = string.Empty;

        /// <summary>
        /// The configuration value as a string (will be parsed based on context)
        /// </summary>
        [JsonPropertyName("Value")]
        public string Value { get; set; } = string.Empty;

        /// <summary>
        /// Optional description of what this state represents
        /// </summary>
        [JsonPropertyName("Description")]
        public string? Description { get; set; }

        /// <summary>
        /// Timestamp when this state was last saved
        /// </summary>
        [JsonPropertyName("LastSaved")]
        public DateTime LastSaved { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Whether this state should be automatically restored on server startup
        /// </summary>
        [JsonPropertyName("AutoRestore")]
        public bool AutoRestore { get; set; } = true;
    }
}
