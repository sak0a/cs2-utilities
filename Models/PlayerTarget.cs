using CounterStrikeSharp.API.Core;

namespace CS2Utilities.Models
{
    /// <summary>
    /// Represents different types of player targeting for commands
    /// </summary>
    public enum TargetType
    {
        /// <summary>
        /// Target a specific player by name or ID
        /// </summary>
        Individual,
        
        /// <summary>
        /// Target all players on a specific team (CT, T, Spec)
        /// </summary>
        Team,
        
        /// <summary>
        /// Target all players on the server
        /// </summary>
        All,
        
        /// <summary>
        /// Target the command caller (self)
        /// </summary>
        Self
    }

    /// <summary>
    /// Represents a resolved player target with metadata
    /// </summary>
    public class PlayerTarget
    {
        /// <summary>
        /// The type of targeting used
        /// </summary>
        public TargetType Type { get; set; }

        /// <summary>
        /// The original target string provided by the user
        /// </summary>
        public string OriginalTarget { get; set; } = string.Empty;

        /// <summary>
        /// List of resolved player controllers
        /// </summary>
        public List<CCSPlayerController> Players { get; set; } = new();

        /// <summary>
        /// The team targeted (if applicable)
        /// </summary>
        public CsTeam? Team { get; set; }

        /// <summary>
        /// Whether the target resolution was successful
        /// </summary>
        public bool IsValid => Players.Count > 0;

        /// <summary>
        /// Number of players targeted
        /// </summary>
        public int Count => Players.Count;

        /// <summary>
        /// Error message if target resolution failed
        /// </summary>
        public string? ErrorMessage { get; set; }
    }
}
