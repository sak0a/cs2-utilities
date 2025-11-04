namespace CS2Utilities.Models
{
    /// <summary>
    /// Represents a saved server configuration preset
    /// </summary>
    public class ServerPreset
    {
        /// <summary>
        /// Name of the preset
        /// </summary>
        public string Name { get; set; } = "";

        /// <summary>
        /// Description of the preset
        /// </summary>
        public string Description { get; set; } = "";

        /// <summary>
        /// When the preset was created
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// Commands to execute when loading this preset
        /// </summary>
        public List<string> OnLoadCommands { get; set; } = new List<string>();

        /// <summary>
        /// Commands to execute when unloading this preset
        /// </summary>
        public List<string> OnUnloadCommands { get; set; } = new List<string>();

        /// <summary>
        /// Dictionary of state keys and their values
        /// </summary>
        public Dictionary<string, object> States { get; set; } = new Dictionary<string, object>();

        /// <summary>
        /// Get a formatted summary of the preset
        /// </summary>
        public string GetSummary()
        {
            var summary = $"'{Name}'";
            if (!string.IsNullOrEmpty(Description))
            {
                summary += $" - {Description}";
            }
            summary += $" ({States.Count} settings, {OnLoadCommands.Count} load commands, {OnUnloadCommands.Count} unload commands)";
            summary += $" [Created: {CreatedAt:yyyy-MM-dd HH:mm}]";
            return summary;
        }

        /// <summary>
        /// Get detailed information about the preset
        /// </summary>
        public List<string> GetDetailedInfo()
        {
            var info = new List<string>
            {
                $"Preset: {Name}",
                $"Description: {(string.IsNullOrEmpty(Description) ? "None" : Description)}",
                $"Created: {CreatedAt:yyyy-MM-dd HH:mm:ss} UTC",
                $"Settings: {States.Count}",
                $"Load Commands: {OnLoadCommands.Count}",
                $"Unload Commands: {OnUnloadCommands.Count}"
            };

            if (States.Count > 0)
            {
                info.Add("");
                info.Add("Settings:");
                foreach (var kvp in States.OrderBy(x => x.Key))
                {
                    info.Add($"  {kvp.Key}: {kvp.Value}");
                }
            }

            if (OnLoadCommands.Count > 0)
            {
                info.Add("");
                info.Add("Load Commands:");
                foreach (var command in OnLoadCommands)
                {
                    info.Add($"  {command}");
                }
            }

            if (OnUnloadCommands.Count > 0)
            {
                info.Add("");
                info.Add("Unload Commands:");
                foreach (var command in OnUnloadCommands)
                {
                    info.Add($"  {command}");
                }
            }

            return info;
        }
    }
}
