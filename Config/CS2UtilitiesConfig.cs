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
        /// Command configurations with permissions, descriptions, and settings
        /// </summary>
        [JsonPropertyName("Commands")]
        public Dictionary<string, CommandConfig> Commands { get; set; } = new();

        /// <summary>
        /// Initialize default command configurations for all available commands
        /// </summary>
        public void InitializeDefaultCommands()
        {
            // Always add missing commands, don't check if Commands.Count == 0
            // This ensures new commands are added when the plugin is updated

            // System Commands
            AddCommandIfMissing("cs2utils", new CommandConfig
            {
                Enabled = true,
                Permission = "",
                Description = "Show plugin overview, help, and admin functions",
                Usage = "!cs2utils [help|reload|status]",
                Aliases = new List<string>(),
                MinArgs = 0,
                MaxArgs = 1
            });

            // Money & Economy Commands
            AddCommandIfMissing("money", new CommandConfig
            {
                Enabled = true,
                Permission = "@css/root",
                Description = "Set player money with infinite money system",
                Usage = "!money <player|team|all> <value>",
                Aliases = new List<string> { "setmoney" },
                MinArgs = 1,
                MaxArgs = 2
            });

            AddCommandIfMissing("clearmoney", new CommandConfig
            {
                Enabled = true,
                Permission = "@css/root",
                Description = "Clear infinite money for players",
                Usage = "!clearmoney <player|team|all>",
                Aliases = new List<string> { "resetmoney" },
                MinArgs = 0,
                MaxArgs = 1
            });

            AddCommandIfMissing("maxmoney", new CommandConfig
            {
                Enabled = true,
                Permission = "@css/root",
                Description = "Set maximum money players can have",
                Usage = "!maxmoney <amount>",
                MinArgs = 1,
                MaxArgs = 1
            });

            AddCommandIfMissing("startmoney", new CommandConfig
            {
                Enabled = true,
                Permission = "@css/root",
                Description = "Set starting money for players",
                Usage = "!startmoney <amount>",
                MinArgs = 1,
                MaxArgs = 1
            });

            AddCommandIfMissing("afterroundmoney", new CommandConfig
            {
                Enabled = true,
                Permission = "@css/root",
                Description = "Set money awarded after round end",
                Usage = "!afterroundmoney <amount>",
                MinArgs = 1,
                MaxArgs = 1
            });

            // Player Management Commands
            AddCommandIfMissing("health", new CommandConfig
            {
                Enabled = true,
                Permission = "@css/root",
                Description = "Set player health",
                Usage = "!health <player|team|all> <value>",
                Aliases = new List<string> { "hp" },
                MinArgs = 1,
                MaxArgs = 2
            });

            AddCommandIfMissing("kill", new CommandConfig
            {
                Enabled = true,
                Permission = "@css/root",
                Description = "Kill players",
                Usage = "!kill <player|team|all>",
                MinArgs = 0,
                MaxArgs = 1
            });

            AddCommandIfMissing("tp", new CommandConfig
            {
                Enabled = true,
                Permission = "@css/root",
                Description = "Teleport players",
                Usage = "!tp <fromplayer|fromteam|all> [toplayer]",
                Aliases = new List<string> { "teleport" },
                MinArgs = 1,
                MaxArgs = 2
            });

            AddCommandIfMissing("kick", new CommandConfig
            {
                Enabled = true,
                Permission = "@css/root",
                Description = "Kick players",
                Usage = "!kick <player|team|all>",
                MinArgs = 1,
                MaxArgs = 1
            });

            AddCommandIfMissing("freeze", new CommandConfig
            {
                Enabled = true,
                Permission = "@css/root",
                Description = "Freeze players",
                Usage = "!freeze <player|team|all>",
                MinArgs = 1,
                MaxArgs = 1
            });

            AddCommandIfMissing("unfreeze", new CommandConfig
            {
                Enabled = true,
                Permission = "@css/root",
                Description = "Unfreeze players",
                Usage = "!unfreeze <player|team|all>",
                MinArgs = 1,
                MaxArgs = 1
            });

            AddCommandIfMissing("noclip", new CommandConfig
            {
                Enabled = true,
                Permission = "@css/root",
                Description = "Toggle noclip for yourself",
                Usage = "!noclip",
                MinArgs = 0,
                MaxArgs = 0
            });
        }

        /// <summary>
        /// Initialize remaining commands (Part 2) - Advanced Player Commands
        /// </summary>
        public void InitializeAdvancedCommands()
        {
            // Advanced Player Commands
            AddCommandIfMissing("instantrespawn", new CommandConfig
            {
                Enabled = true,
                Permission = "@css/root",
                Description = "Instantly respawn players",
                Usage = "!instantrespawn <player|team|all>",
                MinArgs = 1,
                MaxArgs = 1
            });

            AddCommandIfMissing("respawnimmunity", new CommandConfig
            {
                Enabled = true,
                Permission = "@css/root",
                Description = "Grant temporary damage immunity",
                Usage = "!respawnimmunity <player|team|all> [seconds]",
                MinArgs = 1,
                MaxArgs = 2
            });

            AddCommandIfMissing("bhop", new CommandConfig
            {
                Enabled = true,
                Permission = "@css/root",
                Description = "Set bunny hopping mode",
                Usage = "!bhop [off|matchmaking|supernatural]",
                MinArgs = 0,
                MaxArgs = 1
            });

            AddCommandIfMissing("defaultprimary", new CommandConfig
            {
                Enabled = true,
                Permission = "@css/root",
                Description = "Set default primary weapon",
                Usage = "!defaultprimary <weapon>",
                MinArgs = 1,
                MaxArgs = 1
            });

            AddCommandIfMissing("defaultsecondary", new CommandConfig
            {
                Enabled = true,
                Permission = "@css/root",
                Description = "Set default secondary weapon",
                Usage = "!defaultsecondary <weapon>",
                MinArgs = 1,
                MaxArgs = 1
            });

            AddCommandIfMissing("god", new CommandConfig
            {
                Enabled = true,
                Permission = "@css/root",
                Description = "Toggle god mode for players",
                Usage = "!god <player|team|all>",
                MinArgs = 1,
                MaxArgs = 1
            });
        }

        /// <summary>
        /// Initialize remaining commands (Part 3) - Game State & Server Commands
        /// </summary>
        public void InitializeGameStateCommands()
        {
            // Game State Commands
            AddCommandIfMissing("pause", new CommandConfig
            {
                Enabled = true,
                Permission = "@css/root",
                Description = "Pause the game",
                Usage = "!pause",
                MinArgs = 0,
                MaxArgs = 0
            });

            AddCommandIfMissing("unpause", new CommandConfig
            {
                Enabled = true,
                Permission = "@css/root",
                Description = "Unpause the game",
                Usage = "!unpause",
                MinArgs = 0,
                MaxArgs = 0
            });

            AddCommandIfMissing("endwarmup", new CommandConfig
            {
                Enabled = true,
                Permission = "@css/root",
                Description = "End warmup period",
                Usage = "!endwarmup [seconds]",
                MinArgs = 0,
                MaxArgs = 1
            });

            AddCommandIfMissing("startwarmup", new CommandConfig
            {
                Enabled = true,
                Permission = "@css/root",
                Description = "Start warmup period",
                Usage = "!startwarmup [seconds]",
                MinArgs = 0,
                MaxArgs = 1
            });

            AddCommandIfMissing("restartround", new CommandConfig
            {
                Enabled = true,
                Permission = "@css/root",
                Description = "Restart the current round",
                Usage = "!restartround [seconds]",
                MinArgs = 0,
                MaxArgs = 1
            });

            AddCommandIfMissing("infiniteammo", new CommandConfig
            {
                Enabled = true,
                Permission = "@css/root",
                Description = "Toggle infinite ammo",
                Usage = "!infiniteammo [clip|reserve|both|off]",
                MinArgs = 0,
                MaxArgs = 1
            });

            AddCommandIfMissing("friendlyfire", new CommandConfig
            {
                Enabled = true,
                Permission = "@css/root",
                Description = "Toggle friendly fire",
                Usage = "!friendlyfire [team]",
                MinArgs = 0,
                MaxArgs = 1
            });

            AddCommandIfMissing("disabledamage", new CommandConfig
            {
                Enabled = true,
                Permission = "@css/root",
                Description = "Toggle damage on/off",
                Usage = "!disabledamage",
                MinArgs = 0,
                MaxArgs = 0
            });

            AddCommandIfMissing("showimpacts", new CommandConfig
            {
                Enabled = true,
                Permission = "@css/root",
                Description = "Show bullet impacts",
                Usage = "!showimpacts [seconds]",
                MinArgs = 0,
                MaxArgs = 1
            });

            AddCommandIfMissing("grenadeview", new CommandConfig
            {
                Enabled = true,
                Permission = "@css/root",
                Description = "Show grenade trajectories",
                Usage = "!grenadeview [seconds]",
                MinArgs = 0,
                MaxArgs = 1
            });

            AddCommandIfMissing("maxgrenades", new CommandConfig
            {
                Enabled = true,
                Permission = "@css/root",
                Description = "Set maximum grenades per player",
                Usage = "!maxgrenades <value>",
                MinArgs = 1,
                MaxArgs = 1
            });

            // Server Time Commands
            AddCommandIfMissing("warmuptime", new CommandConfig
            {
                Enabled = true,
                Permission = "@css/root",
                Description = "Set warmup duration",
                Usage = "!warmuptime <seconds|default>",
                MinArgs = 1,
                MaxArgs = 1
            });

            AddCommandIfMissing("roundtime", new CommandConfig
            {
                Enabled = true,
                Permission = "@css/root",
                Description = "Set round time in minutes",
                Usage = "!roundtime <minutes>",
                MinArgs = 1,
                MaxArgs = 1
            });

            AddCommandIfMissing("roundfreezetime", new CommandConfig
            {
                Enabled = true,
                Permission = "@css/root",
                Description = "Set freeze time at round start",
                Usage = "!roundfreezetime <seconds>",
                MinArgs = 1,
                MaxArgs = 1
            });

            AddCommandIfMissing("roundrestartdelay", new CommandConfig
            {
                Enabled = true,
                Permission = "@css/root",
                Description = "Set delay before round restart",
                Usage = "!roundrestartdelay <seconds>",
                MinArgs = 1,
                MaxArgs = 1
            });

            AddCommandIfMissing("buytime", new CommandConfig
            {
                Enabled = true,
                Permission = "@css/root",
                Description = "Set buy time duration",
                Usage = "!buytime <seconds>",
                MinArgs = 1,
                MaxArgs = 1
            });

            AddCommandIfMissing("autoteambalance", new CommandConfig
            {
                Enabled = true,
                Permission = "@css/root",
                Description = "Toggle automatic team balancing",
                Usage = "!autoteambalance",
                MinArgs = 0,
                MaxArgs = 0
            });
        }

        /// <summary>
        /// Initialize remaining commands (Part 4) - Bot & Preset Commands
        /// </summary>
        public void InitializeBotAndPresetCommands()
        {
            // Basic Bot Commands
            AddCommandIfMissing("addbot", new CommandConfig
            {
                Enabled = true,
                Permission = "@css/root",
                Description = "Add bot(s) to specified team",
                Usage = "!addbot <t|ct> [amount]",
                MinArgs = 1,
                MaxArgs = 2
            });

            AddCommandIfMissing("kickbot", new CommandConfig
            {
                Enabled = true,
                Permission = "@css/root",
                Description = "Kick bot(s) from specified team or all",
                Usage = "!kickbot [t|ct|all] [amount]",
                MinArgs = 0,
                MaxArgs = 2
            });

            AddCommandIfMissing("kickbots", new CommandConfig
            {
                Enabled = true,
                Permission = "@css/root",
                Description = "Kick all bots from the server",
                Usage = "!kickbots",
                MinArgs = 0,
                MaxArgs = 0
            });

            // Advanced Bot Commands
            AddCommandIfMissing("placebot", new CommandConfig
            {
                Enabled = true,
                Permission = "@css/root",
                Description = "Spawn a bot with team assignment",
                Usage = "!placebot <team> [position]",
                MinArgs = 1,
                MaxArgs = 2
            });

            // Preset Commands
            AddCommandIfMissing("savepreset", new CommandConfig
            {
                Enabled = true,
                Permission = "@css/root",
                Description = "Save current server state as preset",
                Usage = "!savepreset <name> [description]",
                MinArgs = 1,
                MaxArgs = 2
            });

            AddCommandIfMissing("loadpreset", new CommandConfig
            {
                Enabled = true,
                Permission = "@css/root",
                Description = "Load a saved preset",
                Usage = "!loadpreset <name>",
                MinArgs = 1,
                MaxArgs = 1
            });

            AddCommandIfMissing("listpresets", new CommandConfig
            {
                Enabled = true,
                Permission = "@css/root",
                Description = "List all saved presets",
                Usage = "!listpresets",
                MinArgs = 0,
                MaxArgs = 0
            });

            AddCommandIfMissing("deletepreset", new CommandConfig
            {
                Enabled = true,
                Permission = "@css/root",
                Description = "Delete a saved preset",
                Usage = "!deletepreset <name>",
                MinArgs = 1,
                MaxArgs = 1
            });
        }

        /// <summary>
        /// Helper method to add a command only if it doesn't already exist
        /// </summary>
        private void AddCommandIfMissing(string commandName, CommandConfig config)
        {
            if (!Commands.ContainsKey(commandName))
            {
                Commands[commandName] = config;
            }
        }

        /// <summary>
        /// Initialize default map category configurations
        /// </summary>
        public void InitializeMapCategoryDefaults()
        {
            // 1v1 Maps
            AddMapCategoryIfMissing("1v1", new MapCategoryConfig
            {
                GameMode = "casual",
                GameType = "classic",
                MaxPlayers = 2,
                Description = "1 vs 1 duel maps",
                Commands = new List<string>
                {
                    "mp_maxplayers 2",
                    "mp_roundtime 3",
                    "mp_freezetime 3",
                    "mp_round_restart_delay 2",
                    "mp_respawn_on_death_ct 0",
                    "mp_respawn_on_death_t 0"
                }
            });

            // Aim Training Maps
            AddMapCategoryIfMissing("aim", new MapCategoryConfig
            {
                GameMode = "casual",
                GameType = "classic",
                MaxPlayers = 10,
                Description = "Aim training and practice maps",
                Commands = new List<string>
                {
                    "mp_respawn_on_death_ct 1",
                    "mp_respawn_on_death_t 1",
                    "mp_respawn_immunitytime 0",
                    "mp_roundtime 0",
                    "mp_freezetime 0",
                    "mp_buytime 0",
                    "sv_infinite_ammo 2"
                }
            });

            // Surf Maps
            AddMapCategoryIfMissing("surf", new MapCategoryConfig
            {
                GameMode = "casual",
                GameType = "classic",
                MaxPlayers = 20,
                Description = "Surf maps with modified physics",
                Commands = new List<string>
                {
                    "sv_airaccelerate 1000",
                    "sv_gravity 800",
                    "sv_maxspeed 3500",
                    "mp_respawn_on_death_ct 1",
                    "mp_respawn_on_death_t 1",
                    "mp_roundtime 0",
                    "mp_freezetime 0"
                }
            });

            // Bhop Maps
            AddMapCategoryIfMissing("bhop", new MapCategoryConfig
            {
                GameMode = "casual",
                GameType = "classic",
                MaxPlayers = 20,
                Description = "Bunny hop maps",
                Commands = new List<string>
                {
                    "sv_autobunnyhopping 1",
                    "sv_enablebunnyhopping 1",
                    "sv_airaccelerate 1000",
                    "mp_respawn_on_death_ct 1",
                    "mp_respawn_on_death_t 1",
                    "mp_roundtime 0",
                    "mp_freezetime 0"
                }
            });

            // Retake Maps
            AddMapCategoryIfMissing("retake", new MapCategoryConfig
            {
                GameMode = "casual",
                GameType = "classic",
                MaxPlayers = 10,
                Description = "Retake scenario maps",
                Commands = new List<string>
                {
                    "mp_roundtime 2",
                    "mp_freezetime 5",
                    "mp_round_restart_delay 3",
                    "mp_maxmoney 16000",
                    "mp_startmoney 16000"
                }
            });

            // AWP Maps
            AddMapCategoryIfMissing("awp", new MapCategoryConfig
            {
                GameMode = "casual",
                GameType = "classic",
                MaxPlayers = 20,
                Description = "AWP-focused maps",
                Commands = new List<string>
                {
                    "mp_respawn_on_death_ct 1",
                    "mp_respawn_on_death_t 1",
                    "mp_respawn_immunitytime 2",
                    "mp_roundtime 0",
                    "mp_maxmoney 16000",
                    "mp_startmoney 16000"
                }
            });

            // Headshot Only Maps
            AddMapCategoryIfMissing("hs", new MapCategoryConfig
            {
                GameMode = "casual",
                GameType = "classic",
                MaxPlayers = 16,
                Description = "Headshot only maps",
                Commands = new List<string>
                {
                    "mp_respawn_on_death_ct 1",
                    "mp_respawn_on_death_t 1",
                    "mp_roundtime 0",
                    "mp_damage_headshot_only 1"
                }
            });
        }

        /// <summary>
        /// Helper method to add a map category only if it doesn't already exist
        /// </summary>
        private void AddMapCategoryIfMissing(string categoryName, MapCategoryConfig config)
        {
            if (!MapCategoryDefaults.ContainsKey(categoryName))
            {
                MapCategoryDefaults[categoryName] = config;
            }
        }

        /// <summary>
        /// Saved server states that can be restored
        /// </summary>
        [JsonPropertyName("SavedState")]
        public List<SavedState> SavedState { get; set; } = new();

        /// <summary>
        /// Server presets that can be loaded/saved
        /// </summary>
        [JsonPropertyName("Presets")]
        public Dictionary<string, ServerPreset> Presets { get; set; } = new();

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

        /// <summary>
        /// Workshop map favorites for quick access
        /// </summary>
        [JsonPropertyName("WorkshopMapFavorites")]
        public Dictionary<string, WorkshopMapInfo> WorkshopMapFavorites { get; set; } = new();

        /// <summary>
        /// Default game mode configurations for different map categories
        /// </summary>
        [JsonPropertyName("MapCategoryDefaults")]
        public Dictionary<string, MapCategoryConfig> MapCategoryDefaults { get; set; } = new();
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

    /// <summary>
    /// Workshop map information for favorites
    /// </summary>
    public class WorkshopMapInfo
    {
        /// <summary>
        /// Workshop ID of the map
        /// </summary>
        [JsonPropertyName("WorkshopId")]
        public string WorkshopId { get; set; } = "";

        /// <summary>
        /// Display name for the map
        /// </summary>
        [JsonPropertyName("Name")]
        public string Name { get; set; } = "";

        /// <summary>
        /// Description of the map
        /// </summary>
        [JsonPropertyName("Description")]
        public string Description { get; set; } = "";

        /// <summary>
        /// Map category (e.g., "aim", "retake", "surf", "bhop", "1v1", "awp", "hs")
        /// </summary>
        [JsonPropertyName("Category")]
        public string Category { get; set; } = "";

        /// <summary>
        /// Steam Workshop URL
        /// </summary>
        [JsonPropertyName("WorkshopUrl")]
        public string WorkshopUrl { get; set; } = "";

        /// <summary>
        /// Required game mode for this map (casual, competitive, deathmatch, etc.)
        /// </summary>
        [JsonPropertyName("RequiredGameMode")]
        public string RequiredGameMode { get; set; } = "";

        /// <summary>
        /// Required game type for this map (classic, gungame, etc.)
        /// </summary>
        [JsonPropertyName("RequiredGameType")]
        public string RequiredGameType { get; set; } = "";

        /// <summary>
        /// Console commands to execute when loading this map
        /// </summary>
        [JsonPropertyName("OnLoadCommands")]
        public List<string> OnLoadCommands { get; set; } = new();

        /// <summary>
        /// Console commands to execute when leaving this map
        /// </summary>
        [JsonPropertyName("OnUnloadCommands")]
        public List<string> OnUnloadCommands { get; set; } = new();

        /// <summary>
        /// Maximum players recommended for this map
        /// </summary>
        [JsonPropertyName("MaxPlayers")]
        public int MaxPlayers { get; set; } = 10;

        /// <summary>
        /// Whether this map requires specific plugins or settings
        /// </summary>
        [JsonPropertyName("RequiresCustomSettings")]
        public bool RequiresCustomSettings { get; set; } = false;
    }

    /// <summary>
    /// Default configuration for map categories
    /// </summary>
    public class MapCategoryConfig
    {
        /// <summary>
        /// Game mode to use for this category
        /// </summary>
        [JsonPropertyName("GameMode")]
        public string GameMode { get; set; } = "casual";

        /// <summary>
        /// Game type to use for this category
        /// </summary>
        [JsonPropertyName("GameType")]
        public string GameType { get; set; } = "classic";

        /// <summary>
        /// Console commands to execute for this category
        /// </summary>
        [JsonPropertyName("Commands")]
        public List<string> Commands { get; set; } = new();

        /// <summary>
        /// Maximum players for this category
        /// </summary>
        [JsonPropertyName("MaxPlayers")]
        public int MaxPlayers { get; set; } = 10;

        /// <summary>
        /// Description of this category
        /// </summary>
        [JsonPropertyName("Description")]
        public string Description { get; set; } = "";
    }
}
