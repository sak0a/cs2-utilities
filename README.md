# CS2 Utilities Plugin

A comprehensive Counter-Strike 2 server administration plugin built with CounterStrikeSharp, featuring **50+ commands** for complete server management, advanced player manipulation, and custom server presets.

## 🚀 Features Overview

- **🎮 Complete Server Administration** - 50+ chat commands for every aspect of server management
- **👥 Advanced Player Manipulation** - Health, money, teleportation, weapons, and physics control
- **🎯 Game State Management** - Pause/unpause, warmup control, damage settings, and visual debugging
- **🐰 Movement Enhancement** - Bunny hopping with multiple presets (matchmaking/supernatural)
- **🔫 Weapon & Ammo Control** - Infinite ammo modes, default weapons, grenade limits
- **🤖 Bot Management** - Intelligent bot placement, difficulty, and quota control
- **💾 Server Presets** - Save/load custom server configurations with command execution
- **🔐 Permission System** - Role-based access control with admin management
- **💬 Rich Chat Integration** - Color-coded messages and comprehensive feedback
- **⚡ State Persistence** - All settings survive server restarts

## 📋 Command Categories

### 🎯 Server Preset System (8 Commands)
Save and load custom server configurations with automatic command execution:

| Command | Description | Usage |
|---------|-------------|-------|
| `!savepreset` | Save current server state as preset | `!savepreset <name> [description]` |
| `!loadpreset` | Load a saved preset | `!loadpreset <name>` |
| `!listpresets` | List all saved presets | `!listpresets` |
| `!deletepreset` | Delete a saved preset | `!deletepreset <name>` |
| `!presetinfo` | Show detailed preset information | `!presetinfo <name>` |
| `!currentpreset` | Show currently loaded preset | `!currentpreset` |
| `!presetaddcmd` | Add command to preset | `!presetaddcmd <preset> <load\|unload> <command>` |
| `!presetremovecmd` | Remove command from preset | `!presetremovecmd <preset> <load\|unload> <command>` |

### 💰 Money & Economy Commands (4 Commands)
Control server economy settings with **infinite money system**:

| Command | Description | Usage |
|---------|-------------|-------|
| `!money` | Set player money with infinite money | `!money <player\|team\|all> <value\|+/-amount>` |
| `!clearmoney` | Disable infinite money | `!clearmoney <player\|team\|all>` |
| `!maxmoney` | Set maximum money players can have | `!maxmoney <amount>` |
| `!startmoney` | Set starting money for players | `!startmoney <amount>` |
| `!afterroundmoney` | Set money awarded after round end | `!afterroundmoney <amount>` |

#### 🎯 Infinite Money System
The `!money` command features **infinite money** that activates when money exceeds the server's `mp_maxmoney` limit:
- **Normal Money (≤ server limit)**: Standard behavior, no infinite money
- **Infinite Money (> server limit)**: Unlimited money, automatically restored after purchases
- **User Feedback**: Clear messages when infinite money is enabled/disabled
- **Performance Optimized**: Simple boolean tracking with efficient monitoring

**Example**: On a server with `mp_maxmoney 16000`:
```bash
!money 12000   # Normal money, no infinite money
!money 25000   # Infinite money enabled, always restored to server max after purchases
```

📖 **[Full Documentation](docs/INFINITE_MONEY_SYSTEM.md)** - Complete guide to the infinite money system

### ⏰ Time Management Commands (5 Commands)
Control game timing and phases:

| Command | Description | Usage |
|---------|-------------|-------|
| `!warmuptime` | Set warmup duration | `!warmuptime <seconds\|default>` |
| `!roundtime` | Set round time | `!roundtime <minutes>` |
| `!roundfreezetime` | Set freeze time | `!roundfreezetime <seconds>` |
| `!roundrestartdelay` | Set restart delay | `!roundrestartdelay <seconds>` |
| `!buytime` | Set buy time | `!buytime <seconds>` |

### 👥 Team & Bot Commands (4 Commands)
Manage teams and bots:

| Command | Description | Usage |
|---------|-------------|-------|
| `!autoteambalance` | Toggle auto team balance | `!autoteambalance` |
| `!limitteams` | Set team limits | `!limitteams <value>` |
| `!botdifficulty` | Set bot difficulty | `!botdifficulty <0-3>` |
| `!botquota` | Set bot quota | `!botquota <count>` |

### 🛒 Utility Commands (2 Commands)
Server utility functions:

| Command | Description | Usage |
|---------|-------------|-------|
| `!buyanywhere` | Toggle buy anywhere | `!buyanywhere` |
| `!changemap` | Enhanced map changing with workshop support | `!changemap <mapname\|workshop/ID\|workshopID\|steamURL> [category]` |

### 🎮 Game State Commands (9 Commands)
Control game rules and state:

| Command | Description | Usage |
|---------|-------------|-------|
| `!pause` / `!unpause` | Pause/unpause the game | `!pause` / `!unpause` |
| `!endwarmup` / `!warmupend` | End warmup period | `!endwarmup [seconds]` |
| `!startwarmup` / `!warmupstart` | Start warmup period | `!startwarmup [seconds]` |
| `!warmup` | Unified warmup control | `!warmup <start\|end\|0\|1> [seconds]` |
| `!allowknifedrop` | Toggle knife dropping | `!allowknifedrop` |
| `!friendlyfire` | Toggle friendly fire | `!friendlyfire [team]` |
| `!disabledamage` | Toggle damage on/off | `!disabledamage` |
| `!showimpacts` | Show bullet impacts | `!showimpacts [seconds]` |
| `!grenadeview` | Show grenade trajectories | `!grenadeview [seconds]` |
| `!maxgrenades` | Set maximum grenades per player | `!maxgrenades <value>` |
| `!infiniteammo` | Set infinite ammo mode | `!infiniteammo [off\|clip\|reserve\|both]` |

### 👤 Player Manipulation Commands (5 Commands)
Direct player control and manipulation:

| Command | Description | Usage |
|---------|-------------|-------|
| `!health` | Set player health | `!health <player\|team\|all> <value>` OR `!health <value>` |
| `!kill` | Kill players | `!kill <player\|team\|all>` OR `!kill` |
| `!tp` / `!teleport` | Teleport players | `!tp <from> [to]` OR `!tp <to>` |
| `!kick` | Kick players | `!kick <player\|team\|all>` |
| `!freeze` / `!unfreeze` | Freeze/unfreeze players | `!freeze <player\|team\|all>` |

### ⚡ Advanced Player Commands (7 Commands)
Advanced player manipulation with complex state tracking:

| Command | Description | Usage |
|---------|-------------|-------|
| `!instantrespawn` | Instantly respawn players | `!instantrespawn <player\|team\|all>` |
| `!respawnimmunity` | Grant temporary damage immunity | `!respawnimmunity <player\|team\|all> [seconds]` |
| `!god` | Toggle god mode (complete invulnerability) | `!god <player\|team\|all>` |
| `!bhop` | Set bunny hopping mode | `!bhop [off\|matchmaking\|supernatural]` |
| `!defaultprimary` | Set default primary weapon | `!defaultprimary <weapon>` |
| `!defaultsecondary` | Set default secondary weapon | `!defaultsecondary <weapon>` |
| `!placebot` | Place bot with intelligent positioning | `!placebot [team] [name]` |

### 🔧 System Commands (3 Commands)
Plugin management and system utilities:

| Command | Description | Usage |
|---------|-------------|-------|
| `!cs2utils` | Show plugin overview and information | `!cs2utils` |
| `!help` | Show available commands | `!help [command]` |
| `!reload` | Reload plugin configuration | `!reload` |
| `!saveutilstate` | Save current server state | `!saveutilstate` |

## 🎯 Advanced Features

### 🐰 Bunny Hopping System
- **Three Modes**: Off, Matchmaking (competitive), Supernatural (uncapped)
- **No sv_cheats Required**: Automatic cheat flag removal from convars
- **Smart Toggle**: `!bhop` without arguments toggles between off and last used mode
- **Server-Wide**: Affects all players simultaneously
- **State Persistence**: Mode survives server restarts

### 🔫 Infinite Ammo System
- **Four Modes**: Off, Clip (no reload), Reserve (reload required), Both
- **Toggle Support**: `!infiniteammo` toggles between off and last used mode
- **Cheat-Free**: Removes cheat flags from `sv_infinite_ammo`
- **Instant Application**: Immediate effect on all players

### 💾 Server Preset System
- **Complete State Capture**: Saves all server settings automatically
- **Custom Commands**: Add load/unload commands to presets
- **Smart Switching**: Executes unload commands when switching presets
- **Persistent Storage**: JSON-based storage survives server restarts
- **Detailed Management**: List, info, add/remove commands from presets

### 🗺️ Enhanced Workshop Map Support
- **Multiple Input Formats**: Workshop IDs, Steam URLs, workshop/ format
- **Automatic Game Mode Detection**: Detects aim, surf, 1v1, bhop, retake maps
- **Category-Specific Settings**: Auto-applies optimal settings per map type
- **Map Favorites System**: Save frequently used workshop maps with aliases
- **Smart Configuration**: Automatic respawn, physics, and game rule adjustments

### 🎮 Player Targeting System
- **Individual Players**: Target specific players by name
- **Team Targeting**: Target entire teams (CT/T)
- **All Players**: Target all players simultaneously
- **Smart Resolution**: Automatic player name matching and validation

### 🔐 Permission System
- **Role-Based Access**: Different permission levels for different commands
- **Admin Integration**: Integrates with CounterStrikeSharp admin system
- **Flexible Permissions**: Granular control over command access

## 📦 Installation

1. **Prerequisites**:
   - Counter-Strike 2 server
   - CounterStrikeSharp installed
   - .NET 8.0 runtime

2. **Installation Steps**:
   ```bash
   # Download the latest release
   # Extract to your CounterStrikeSharp plugins directory
   # Restart your server
   ```

3. **File Structure**:
   ```
   addons/counterstrikesharp/plugins/CS2Utilities/
   ├── CS2Utilities.dll
   ├── CS2UtilitiesConfig.json
   ├── presets.json (created automatically)
   └── states.json (created automatically)
   ```

## ⚙️ Configuration

The plugin creates a `CS2UtilitiesConfig.json` file with the following options:

```json
{
  "EnableDebugLogging": false,
  "CommandPrefix": "!",
  "DefaultPermissionLevel": "@css/root",
  "SaveStateInterval": 300,
  "MaxPresets": 50
}
```

## 🎮 Usage Examples

### Getting Started
```bash
# Show plugin overview and features
!cs2utils

# View all available commands
!help
```

### Quick Server Mode Switching
```bash
# Save current competitive settings
!savepreset competitive "5v5 matchmaking settings"

# Configure fun server
!bhop supernatural
!infiniteammo clip
!maxmoney 65000
!savepreset funserver "Bhop + infinite ammo"

# Quick switching
!loadpreset competitive  # Switch to competitive mode
!loadpreset funserver     # Switch to fun mode
```

### Advanced Player Management
```bash
# Give all players full health and money
!health all 100
!money all 16000

# Teleport entire CT team to a player
!tp ct player1

# Grant temporary immunity to terrorists
!respawnimmunity t 10
```

### Game State Control
```bash
# Setup practice mode
!warmup start 300
!infiniteammo clip
!bhop matchmaking
!showimpacts 60

# Quick match setup
!warmup end
!bhop off
!infiniteammo off
!mp_restartgame 1
```

## 🔧 Technical Details

- **Framework**: CounterStrikeSharp (.NET 8.0)
- **Architecture**: Modular command system with dependency injection
- **State Management**: JSON-based persistence with automatic backup
- **Error Handling**: Comprehensive exception handling with user feedback
- **Performance**: Optimized for minimal server impact
- **Memory Management**: Proper resource cleanup and disposal

## 📝 Documentation

### Command Reference
All commands support the following features:
- **Permission Validation**: Commands check user permissions before execution
- **Parameter Validation**: Input validation with helpful error messages
- **State Persistence**: Settings are automatically saved and restored
- **Rich Feedback**: Color-coded chat messages for success/error/info
- **Help Integration**: All commands documented in the help system

### Workshop Maps Documentation
- **[Workshop Maps - Complete Guide](docs/WORKSHOP_MAPS.md)** - Everything you need to know about workshop maps

## 🤝 Contributing

Contributions are welcome! Please feel free to submit issues, feature requests, or pull requests.

## 📄 License

This project is licensed under the MIT License - see the LICENSE file for details.

## 🔗 Links

- [CounterStrikeSharp Documentation](https://docs.cssharp.dev/)
- [CounterStrikeSharp Discord](https://discord.gg/tfPyCqyCPv)
- [CS2 Server Administration Guide](https://developer.valvesoftware.com/wiki/Counter-Strike_2/Dedicated_Servers)
