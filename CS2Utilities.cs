using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Modules.Events;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Extensions;
using System.Text.Json;
using CS2Utilities.Commands;
using CS2Utilities.Config;
using CS2Utilities.Core;
using CS2Utilities.Extensions;
using CS2Utilities.Utils;

namespace CS2Utilities;

[MinimumApiVersion(160)]
public class CS2Utilities : BasePlugin, IPluginConfig<CS2UtilitiesConfig>
{
    public override string ModuleName => "CS2-Utilities";
    public override string ModuleDescription => "Comprehensive CS2 server administration plugin with 40+ commands";
    public override string ModuleAuthor => "sak0a";
    public override string ModuleVersion => "1.0.0";

    // Configuration
    public CS2UtilitiesConfig Config { get; set; } = new();

    // Core components
    private PermissionManager? _permissionManager;
    private PlayerTargetResolver? _targetResolver;
    private CommandManager? _commandManager;
    private StateManager? _stateManager;
    private PresetManager? _presetManager;
    private ServerCommands? _serverCommands;
    private GameStateCommands? _gameStateCommands;
    private PlayerCommands? _playerCommands;
    private AdvancedPlayerCommands? _advancedPlayerCommands;
    private BotCommands? _botCommands;
    private PresetCommands? _presetCommands;

    // Properties for accessing components
    public PermissionManager PermissionManager => _permissionManager!;
    public PlayerTargetResolver TargetResolver => _targetResolver!;
    public new Core.CommandManager CommandManager => _commandManager!;
    public StateManager StateManager => _stateManager!;
    public PresetManager PresetManager => _presetManager!;
    public ServerCommands ServerCommands => _serverCommands!;
    public GameStateCommands GameStateCommands => _gameStateCommands!;
    public PlayerCommands PlayerCommands => _playerCommands!;
    public AdvancedPlayerCommands AdvancedPlayerCommands => _advancedPlayerCommands!;
    public BotCommands BotCommands => _botCommands!;

    public void OnConfigParsed(CS2UtilitiesConfig config)
    {
        Config = config;

        if (Config.EnableDebugLogging)
        {
            Console.WriteLine($"[{ModuleName}] Configuration loaded with {Config.Commands.Count} command configurations");
        }
    }

    public override void Load(bool hotReload)
    {
        try
        {
            // Initialize default command configurations
            InitializeDefaultConfigurations();

            // Initialize core components
            InitializeComponents();

            // Initialize chat utilities
            ChatUtils.Initialize(Config);

            // Register base commands (help, reload, etc.)
            RegisterBaseCommands();

            // Restore saved state if enabled
            if (Config.RestoreStateOnLoad)
            {
                RestoreSavedState();
            }

            // Start money monitoring system
            StartMoneyMonitoring();

            Console.WriteLine($"[{ModuleName}] v{ModuleVersion} loaded successfully!");

            if (Config.EnableDebugLogging)
            {
                Console.WriteLine($"[{ModuleName}] Debug logging enabled");
                Console.WriteLine($"[{ModuleName}] Registered {_commandManager.GetAllCommands().Count} commands");
                Console.WriteLine($"[{ModuleName}] Money monitoring system started");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[{ModuleName}] Error during plugin load: {ex.Message}");
            throw;
        }
    }

    public override void Unload(bool hotReload)
    {
        try
        {
            // Cleanup money monitoring timer
            _moneyTimer?.Kill();
            _moneyTimer = null;

            // Cleanup game state commands (timers, etc.)
            _gameStateCommands?.Cleanup();

            // Cleanup player commands (frozen players, etc.)
            _playerCommands?.Cleanup();

            // Cleanup advanced player commands (immunity timers, etc.)
            _advancedPlayerCommands?.Cleanup();

            // Cleanup bot commands
            _botCommands?.Cleanup();

            // Save current state before unloading
            if (Config.AutoSaveState && _stateManager != null)
            {
                _stateManager.PersistToDisk().Wait(5000);
            }

            // Dispose state manager
            _stateManager?.Dispose();

            Console.WriteLine($"[{ModuleName}] Plugin unloaded successfully!");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[{ModuleName}] Error during plugin unload: {ex.Message}");
        }
    }

    /// <summary>
    /// Initialize default command configurations and save config
    /// </summary>
    private void InitializeDefaultConfigurations()
    {
        try
        {
            // Initialize all default commands
            Config.InitializeDefaultCommands();
            Config.InitializeAdvancedCommands();
            Config.InitializeGameStateCommands();
            Config.InitializeBotAndPresetCommands();
            Config.InitializeMapCategoryDefaults();

            // Save the updated configuration to disk
            // This ensures all default commands are written to the config file
            var configPath = Path.Combine(ModuleDirectory, "../../configs/plugins/CS2Utilities", "CS2Utilities.json");
            var configDir = Path.GetDirectoryName(configPath);

            if (!Directory.Exists(configDir))
            {
                Directory.CreateDirectory(configDir!);
            }

            // Force save the config with all commands
            var json = System.Text.Json.JsonSerializer.Serialize(Config, new System.Text.Json.JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase,
                Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
            });

            File.WriteAllText(configPath, json);

            Console.WriteLine($"[{ModuleName}] Initialized {Config.Commands.Count} default commands and saved to config");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[{ModuleName}] Error initializing default configurations: {ex.Message}");
        }
    }

    /// <summary>
    /// Initialize all core components
    /// </summary>
    private void InitializeComponents()
    {
        // Validate module directory
        if (string.IsNullOrEmpty(ModuleDirectory))
        {
            throw new InvalidOperationException("ModuleDirectory is null or empty. Plugin cannot initialize properly.");
        }

        Console.WriteLine($"[{ModuleName}] ModuleDirectory: {ModuleDirectory}");

        // Initialize permission manager
        _permissionManager = new PermissionManager(Config);

        // Initialize target resolver
        _targetResolver = new PlayerTargetResolver();

        // Initialize command manager
        _commandManager = new Core.CommandManager(Config, _permissionManager, _targetResolver);

        // Initialize state manager
        _stateManager = new StateManager(Config);

        // Initialize preset manager
        _presetManager = new PresetManager(_stateManager, Config);

        // Initialize server commands
        _serverCommands = new ServerCommands(_commandManager, _stateManager);

        // Initialize game state commands
        _gameStateCommands = new GameStateCommands(_commandManager, _stateManager);

        // Initialize player commands
        _playerCommands = new PlayerCommands(_commandManager, _stateManager, _targetResolver, Config);
        
        // Set PlayerCommands reference for infinite money support in presets (after PlayerCommands is created)
        _presetManager.SetPlayerCommands(_playerCommands);

        // Initialize advanced player commands
        _advancedPlayerCommands = new AdvancedPlayerCommands(_commandManager, _stateManager, _targetResolver);

        // Initialize bot commands
        _botCommands = new BotCommands(_commandManager, _stateManager, _targetResolver);

        // Initialize preset commands
        _presetCommands = new PresetCommands(_commandManager, _presetManager);

        if (Config.EnableDebugLogging)
        {
            Console.WriteLine($"[{ModuleName}] Core components initialized");
        }
    }

    /// <summary>
    /// Register base plugin commands
    /// </summary>
    private void RegisterBaseCommands()
    {
        // Save state command
        _commandManager.RegisterCommand("saveutilstate", HandleSaveStateCommand,
            "Save current server state", "", "@css/root", 0, 0);

        // Plugin overview command with help and reload functionality
        _commandManager.RegisterCommand("cs2utils", HandleCS2UtilsCommand,
            "Show CS2Utilities plugin overview, help, and admin functions", "[help|reload]", "", 0, 1);

        // Register server commands (Phase 2)
        _serverCommands.RegisterCommands();

        // Register game state commands (Phase 3)
        _gameStateCommands.RegisterCommands();

        // Register player commands (Phase 4)
        _playerCommands.RegisterCommands();

        // Register advanced player commands (Phase 5)
        _advancedPlayerCommands.RegisterCommands();

        // Register bot commands (Phase 5)
        _botCommands.RegisterCommands();

        // Register preset commands
        _presetCommands!.RegisterCommands();

        if (Config.EnableDebugLogging)
        {
            Console.WriteLine($"[{ModuleName}] Base commands registered");
            Console.WriteLine($"[{ModuleName}] Server commands registered");
            Console.WriteLine($"[{ModuleName}] Game state commands registered");
            Console.WriteLine($"[{ModuleName}] Player commands registered");
            Console.WriteLine($"[{ModuleName}] Advanced player commands registered");
            Console.WriteLine($"[{ModuleName}] Bot commands registered");
        }
    }

    /// <summary>
    /// Restore saved server state
    /// Now only restores presets - SavedState is only used for tracking current state, not restoration
    /// </summary>
    private void RestoreSavedState()
    {
        try
        {
            // Check if there's a current preset to load
            var currentPresetName = _presetManager?.GetCurrentPresetName();
            if (!string.IsNullOrEmpty(currentPresetName) && _presetManager?.PresetExists(currentPresetName) == true)
            {
                Console.WriteLine($"[{ModuleName}] Restoring preset '{currentPresetName}' on server start");
                
                // Use a timer to delay preset loading slightly to ensure server is ready
                // We delay by 1.5 seconds to give the server time to fully initialize
                AddTimer(1.5f, () =>
                {
                    try
                    {
                        if (_presetManager != null && _presetManager.PresetExists(currentPresetName))
                        {
                            Console.WriteLine($"[{ModuleName}] Loading preset '{currentPresetName}'...");
                            _presetManager.LoadPreset(currentPresetName, null);
                            Console.WriteLine($"[{ModuleName}] Preset '{currentPresetName}' loaded successfully");
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"[{ModuleName}] Error loading preset '{currentPresetName}' on server start: {ex.Message}");
                        Console.WriteLine($"[{ModuleName}] Stack trace: {ex.StackTrace}");
                    }
                });
                
                return;
            }

            // No preset to restore - server starts with default values
            // SavedState is only used for tracking current state between commands, not for restoration
            Console.WriteLine($"[{ModuleName}] No preset to restore on startup - server starting with default values");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[{ModuleName}] Error restoring saved state: {ex.Message}");
        }
    }

    #region Command Handlers

    /// <summary>
    /// Handle save state command
    /// </summary>
    private bool HandleSaveStateCommand(CCSPlayerController? player, string[] args)
    {
        try
        {
            StateManager.SaveState("current", "Current server state");

            var message = "Current server state saved successfully!";
            if (player != null)
            {
                ChatUtils.PrintToPlayer(player, message, MessageType.Success);
            }
            else
            {
                Console.WriteLine($"[{ModuleName}] {message}");
            }

            return true;
        }
        catch (Exception ex)
        {
            var errorMessage = $"Error saving state: {ex.Message}";

            if (player != null)
            {
                ChatUtils.PrintToPlayer(player, errorMessage, MessageType.Error);
            }
            else
            {
                Console.WriteLine($"[{ModuleName}] {errorMessage}");
            }

            return false;
        }
    }

    /// <summary>
    /// Handle !cs2utils command - Show plugin overview, help, and admin functions
    /// </summary>
    private bool HandleCS2UtilsCommand(CCSPlayerController? player, string[] args)
    {
        try
        {
            // Handle subcommands
            if (args.Length > 0)
            {
                var subCommand = args[0].ToLower();

                switch (subCommand)
                {
                    case "help":
                        return HandleHelpSubCommand(player, args.Skip(1).ToArray());

                    case "reload":
                        return HandleReloadSubCommand(player);

                    case "status":
                        return HandleStatusSubCommand(player);

                    default:
                        if (player != null)
                        {
                            ChatUtils.PrintToPlayer(player, $"Unknown subcommand: {args[0]}", MessageType.Error);
                            ChatUtils.PrintToPlayer(player, "Available: help, reload, status", MessageType.Info);
                        }
                        return false;
                }
            }

            // Default: Show plugin overview
            if (player != null)
            {
                ChatUtils.PrintToPlayer(player, "=== CS2Utilities Plugin Overview ===", MessageType.Info);
                ChatUtils.PrintToPlayer(player, $"Version: {ModuleVersion} by {ModuleAuthor}");
                ChatUtils.PrintToPlayer(player, $"Commands Available: {Config.Commands.Count}");
                ChatUtils.PrintToPlayer(player, $"Infinite Money Players: {_playerCommands.GetInfiniteMoneyCount()}");
                ChatUtils.PrintToPlayer(player, "");
                ChatUtils.PrintToPlayer(player, "Usage:", MessageType.Info);
                ChatUtils.PrintToPlayer(player, "!cs2utils help - Show command help");
                ChatUtils.PrintToPlayer(player, "!cs2utils reload - Reload config (admin)");
                ChatUtils.PrintToPlayer(player, "Type !help for command categories", MessageType.Success);
            }
            else
            {
                Console.WriteLine($"[{ModuleName}] CS2Utilities Plugin Overview");
                Console.WriteLine($"[{ModuleName}] Version: {ModuleVersion} by {ModuleAuthor}");
                Console.WriteLine($"[{ModuleName}] Commands Available: {Config.Commands.Count}");
                Console.WriteLine($"[{ModuleName}] Infinite Money Players: {_playerCommands.GetInfiniteMoneyCount()}");
            }

            return true;
        }
        catch (Exception ex)
        {
            var errorMessage = $"Error in cs2utils command: {ex.Message}";

            if (player != null)
            {
                ChatUtils.PrintToPlayer(player, errorMessage, MessageType.Error);
            }
            else
            {
                Console.WriteLine($"[{ModuleName}] {errorMessage}");
            }

            return false;
        }
    }

    /// <summary>
    /// Handle help subcommand
    /// </summary>
    private bool HandleHelpSubCommand(CCSPlayerController? player, string[] args)
    {
        try
        {
            if (args.Length == 0)
            {
                // Show general help
                if (player != null)
                {
                    ChatUtils.PrintToPlayer(player, "=== CS2Utilities Commands ===", MessageType.Info);
                    ChatUtils.PrintToPlayer(player, "👤 Player: health, money, tp, kill, kick, freeze");
                    ChatUtils.PrintToPlayer(player, "⚡ Advanced: god, respawn, immunity, weapons, bots");
                    ChatUtils.PrintToPlayer(player, "💰 Economy: maxmoney, startmoney, buyanywhere");
                    ChatUtils.PrintToPlayer(player, "⏰ Time: warmup, round, freeze, buy times");
                    ChatUtils.PrintToPlayer(player, "🤖 Bots: quota, difficulty, balance, limits");
                    ChatUtils.PrintToPlayer(player, "🔧 System: reload, save, changemap");
                    ChatUtils.PrintToPlayer(player, "");
                    ChatUtils.PrintToPlayer(player, "Use !cs2utils help <command> for details", MessageType.Success);
                }
                else
                {
                    Console.WriteLine($"[{ModuleName}] CS2Utilities Commands Available");
                    Console.WriteLine($"[{ModuleName}] Use 'css_cs2utils help <command>' for specific command help");
                }
            }
            else
            {
                // Show help for specific command
                var commandName = args[0].ToLower();
                var commandConfig = Config.Commands.ContainsKey(commandName) ? Config.Commands[commandName] : null;

                if (commandConfig == null)
                {
                    if (player != null)
                    {
                        ChatUtils.PrintToPlayer(player, $"Command '{commandName}' not found", MessageType.Error);
                        ChatUtils.PrintToPlayer(player, "Use !cs2utils help for available commands", MessageType.Info);
                    }
                    return false;
                }

                if (player != null)
                {
                    ChatUtils.PrintToPlayer(player, $"Command: !{commandName}", MessageType.Info);
                    ChatUtils.PrintToPlayer(player, $"Description: {commandConfig.Description}");
                    if (!string.IsNullOrEmpty(commandConfig.Usage))
                    {
                        ChatUtils.PrintToPlayer(player, $"Usage: !{commandName} {commandConfig.Usage}");
                    }
                    ChatUtils.PrintToPlayer(player, $"Permission: {commandConfig.Permission}");
                    if (commandConfig.Aliases.Any())
                    {
                        ChatUtils.PrintToPlayer(player, $"Aliases: {string.Join(", ", commandConfig.Aliases.Select(a => $"!{a}"))}");
                    }
                }
                else
                {
                    Console.WriteLine($"[{ModuleName}] Command: {commandName}");
                    Console.WriteLine($"[{ModuleName}] Description: {commandConfig.Description}");
                    Console.WriteLine($"[{ModuleName}] Usage: {commandConfig.Usage}");
                    Console.WriteLine($"[{ModuleName}] Permission: {commandConfig.Permission}");
                }
            }

            return true;
        }
        catch (Exception ex)
        {
            var errorMessage = $"Error in help command: {ex.Message}";

            if (player != null)
            {
                ChatUtils.PrintToPlayer(player, errorMessage, MessageType.Error);
            }
            else
            {
                Console.WriteLine($"[{ModuleName}] {errorMessage}");
            }

            return false;
        }
    }

    /// <summary>
    /// Handle reload subcommand
    /// </summary>
    private bool HandleReloadSubCommand(CCSPlayerController? player)
    {
        try
        {
            // Check permissions for reload
            if (!_permissionManager.HasPermission(player, "@css/root"))
            {
                if (player != null)
                {
                    ChatUtils.PrintPermissionDenied(player, "reload", "@css/root");
                }
                return false;
            }

            // Reload configuration by re-initializing default commands
            InitializeDefaultConfigurations();

            var message = "Plugin configuration reloaded successfully!";

            if (player != null)
            {
                ChatUtils.PrintToPlayer(player, message, MessageType.Success);
            }
            else
            {
                Console.WriteLine($"[{ModuleName}] {message}");
            }

            return true;
        }
        catch (Exception ex)
        {
            var errorMessage = $"Error reloading configuration: {ex.Message}";

            if (player != null)
            {
                ChatUtils.PrintToPlayer(player, errorMessage, MessageType.Error);
            }
            else
            {
                Console.WriteLine($"[{ModuleName}] {errorMessage}");
            }

            return false;
        }
    }

    /// <summary>
    /// Handle status subcommand
    /// </summary>
    private bool HandleStatusSubCommand(CCSPlayerController? player)
    {
        try
        {
            if (player != null)
            {
                ChatUtils.PrintToPlayer(player, "=== CS2Utilities Status ===", MessageType.Info);
                ChatUtils.PrintToPlayer(player, $"🔧 Plugin Version: {ModuleVersion}");
                ChatUtils.PrintToPlayer(player, $"📋 Commands Available: {Config.Commands.Count}");
                ChatUtils.PrintToPlayer(player, $"💰 Infinite Money Players: {_playerCommands.GetInfiniteMoneyCount()}");
                ChatUtils.PrintToPlayer(player, $"🎮 Presets Available: {Config.Presets.Count}");
                ChatUtils.PrintToPlayer(player, $"💾 Saved States: {Config.SavedState.Count}");

                // Game state status
                var damageStatus = _gameStateCommands.IsDamageDisabled() ? "Disabled" : "Enabled";
                var pauseStatus = _gameStateCommands.IsGamePaused() ? "Paused" : "Running";
                ChatUtils.PrintToPlayer(player, $"⚔️ Damage: {damageStatus}");
                ChatUtils.PrintToPlayer(player, $"⏸️ Game State: {pauseStatus}");

                // Current preset
                var currentPreset = _presetManager.GetCurrentPresetName();
                if (!string.IsNullOrEmpty(currentPreset))
                {
                    ChatUtils.PrintToPlayer(player, $"🎯 Current Preset: {currentPreset}");
                }
                else
                {
                    ChatUtils.PrintToPlayer(player, "🎯 Current Preset: None");
                }
            }
            else
            {
                Console.WriteLine($"[{ModuleName}] === CS2Utilities Status ===");
                Console.WriteLine($"[{ModuleName}] Plugin Version: {ModuleVersion}");
                Console.WriteLine($"[{ModuleName}] Commands Available: {Config.Commands.Count}");
                Console.WriteLine($"[{ModuleName}] Infinite Money Players: {_playerCommands.GetInfiniteMoneyCount()}");
                Console.WriteLine($"[{ModuleName}] Presets Available: {Config.Presets.Count}");
                Console.WriteLine($"[{ModuleName}] Saved States: {Config.SavedState.Count}");

                var damageStatus = _gameStateCommands.IsDamageDisabled() ? "Disabled" : "Enabled";
                var pauseStatus = _gameStateCommands.IsGamePaused() ? "Paused" : "Running";
                Console.WriteLine($"[{ModuleName}] Damage: {damageStatus}");
                Console.WriteLine($"[{ModuleName}] Game State: {pauseStatus}");

                var currentPreset = _presetManager.GetCurrentPresetName();
                if (!string.IsNullOrEmpty(currentPreset))
                {
                    Console.WriteLine($"[{ModuleName}] Current Preset: {currentPreset}");
                }
                else
                {
                    Console.WriteLine($"[{ModuleName}] Current Preset: None");
                }
            }

            return true;
        }
        catch (Exception ex)
        {
            var errorMessage = $"Error in status command: {ex.Message}";

            if (player != null)
            {
                ChatUtils.PrintToPlayer(player, errorMessage, MessageType.Error);
            }
            else
            {
                Console.WriteLine($"[{ModuleName}] {errorMessage}");
            }

            return false;
        }
    }

    #endregion

    #region Console Command Handlers

    /// <summary>
    /// Console command for setting player health
    /// Usage: css_health <player|team|all> <value> OR css_health <value>
    /// </summary>
    [ConsoleCommand("css_health", "Set player health")]
    public void OnHealthConsoleCommand(CCSPlayerController? player, CounterStrikeSharp.API.Modules.Commands.CommandInfo command)
    {
        try
        {
            var args = command.ArgString.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            _playerCommands.ExecuteHealthCommand(player, args);
        }
        catch (Exception ex)
        {
            command.ReplyToCommand($"Failed to execute health command: {ex.Message}");
        }
    }

    /// <summary>
    /// Console command for setting player money
    /// Usage: css_money <player|team|all> <value> OR css_money <value>
    /// </summary>
    [ConsoleCommand("css_money", "Set player money")]
    public void OnMoneyConsoleCommand(CCSPlayerController? player, CounterStrikeSharp.API.Modules.Commands.CommandInfo command)
    {
        try
        {
            var args = command.ArgString.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            _playerCommands.ExecuteMoneyCommand(player, args);
        }
        catch (Exception ex)
        {
            command.ReplyToCommand($"Failed to execute money command: {ex.Message}");
        }
    }

    #endregion

    #region Money Monitoring System

    private CounterStrikeSharp.API.Modules.Timers.Timer? _moneyTimer;

    /// <summary>
    /// Start monitoring infinite money players
    /// </summary>
    private void StartMoneyMonitoring()
    {
        // Use a more frequent timer (0.2 seconds) as a backup to the event-based system
        // This ensures money is restored even if the purchase event doesn't fire
        _moneyTimer = AddTimer(0.2f, () =>
        {
            try
            {
                // Monitor infinite money for all players
                foreach (var player in Utilities.GetPlayers())
                {
                    if (player != null && player.IsValid && _playerCommands.HasInfiniteMoney(player.SteamID))
                    {
                        _playerCommands.CheckAndRestoreInfiniteMoney(player);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[{ModuleName}] Error in infinite money monitoring: {ex.Message}");
            }
        });
    }

    #endregion

    #region Chat Command Handlers

    /// <summary>
    /// Handle chat commands (this will be the main entry point for all ! commands)
    /// </summary>
    [GameEventHandler]
    public HookResult OnPlayerChat(EventPlayerChat @event, GameEventInfo info)
    {
        var userId = @event.Userid;
        var message = @event.Text;

        if (string.IsNullOrWhiteSpace(message) || !message.StartsWith("!"))
            return HookResult.Continue;

        // Get player from user ID
        var player = Utilities.GetPlayerFromUserid(userId);
        if (player == null || !player.IsValid)
            return HookResult.Continue;

        // Parse command and arguments
        var parts = message[1..].Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length == 0)
            return HookResult.Continue;

        var command = parts[0].ToLower();
        var args = parts.Skip(1).ToArray();

        // Execute command directly on the main thread
        try
        {
            _commandManager.ExecuteCommand(player, command, args);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[{ModuleName}] Error executing command '{command}': {ex.Message}");
        }
        return HookResult.Continue;
    }

    #endregion

    #region Game Event Handlers

    /// <summary>
    /// Handle player hurt events for damage blocking and immunity
    /// </summary>
    [GameEventHandler]
    public HookResult OnPlayerHurt(EventPlayerHurt @event, GameEventInfo info)
    {
        // Check game state damage blocking first
        var gameStateResult = _gameStateCommands.OnPlayerHurt(@event, info);
        if (gameStateResult == HookResult.Changed)
            return gameStateResult;

        // Check advanced player immunity
        return _advancedPlayerCommands.OnPlayerHurt(@event, info);
    }

    /// <summary>
    /// Handle player spawn events for default weapons
    /// </summary>
    [GameEventHandler]
    public HookResult OnPlayerSpawn(EventPlayerSpawn @event, GameEventInfo info)
    {
        return _advancedPlayerCommands.OnPlayerSpawn(@event, info);
    }

    /// <summary>
    /// Handle player disconnect for cleanup
    /// </summary>
    [GameEventHandler]
    public HookResult OnPlayerDisconnect(EventPlayerDisconnect @event, GameEventInfo info)
    {
        var player = @event.Userid;
        if (player != null && player.IsValid)
        {
            // Clean up infinite money tracking
            _playerCommands.RemoveInfiniteMoney(player.SteamID);

            // Clean up advanced player features
            _advancedPlayerCommands.CleanupPlayer(player.SteamID);
        }

        return HookResult.Continue;
    }

    /// <summary>
    /// Handle item purchase events to immediately restore money for infinite money players
    /// </summary>
    [GameEventHandler]
    public HookResult OnItemPurchase(EventItemPurchase @event, GameEventInfo info)
    {
        try
        {
            var player = @event.Userid;
            if (player != null && player.IsValid && _playerCommands.HasInfiniteMoney(player.SteamID))
            {
                // Immediately restore money after purchase
                Server.NextFrame(() =>
                {
                    _playerCommands.CheckAndRestoreInfiniteMoney(player);
                });
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[{ModuleName}] Error in item purchase handler: {ex.Message}");
        }

        return HookResult.Continue;
    }

    #endregion
}
