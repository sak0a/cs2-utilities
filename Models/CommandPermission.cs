using System.Text.Json.Serialization;

namespace CS2Utilities.Models
{
    /// <summary>
    /// Represents a command permission mapping for role-based access control
    /// </summary>
    public class CommandPermission
    {
        /// <summary>
        /// The name of the command (without the ! prefix)
        /// </summary>
        [JsonPropertyName("CommandName")]
        public string CommandName { get; set; } = string.Empty;

        /// <summary>
        /// The permission required to execute this command (e.g., "@css/root", "@css/generic")
        /// </summary>
        [JsonPropertyName("Permission")]
        public string Permission { get; set; } = "@css/root";

        /// <summary>
        /// Optional description of what this command does
        /// </summary>
        [JsonPropertyName("Description")]
        public string? Description { get; set; }

        /// <summary>
        /// Whether this command is enabled
        /// </summary>
        [JsonPropertyName("Enabled")]
        public bool Enabled { get; set; } = true;
    }
}
