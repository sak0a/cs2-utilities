# CS2 Utilities - Complete Command Reference

## 📊 Plugin Statistics
- **Total Commands**: 50+
- **Command Categories**: 8
- **Advanced Features**: Server Presets, Bunny Hopping, Infinite Ammo, Player Targeting
- **Framework**: CounterStrikeSharp (.NET 8.0)
- **State Persistence**: Full server state management with JSON storage

## 🎯 Command Categories Overview

### 💾 Server Preset System (8 Commands)
**Save, load, and manage custom server configurations**
- `!savepreset <name> [description]` - Save current server state as preset
- `!loadpreset <name>` - Load a saved preset
- `!listpresets` - List all saved presets  
- `!deletepreset <name>` - Delete a saved preset
- `!presetinfo <name>` - Show detailed preset information
- `!currentpreset` - Show currently loaded preset
- `!presetaddcmd <preset> <load|unload> <command>` - Add command to preset
- `!presetremovecmd <preset> <load|unload> <command>` - Remove command from preset

### 🎮 Game State Commands (11 Commands)
**Control game rules, physics, and visual debugging**
- `!pause` / `!unpause` - Pause/unpause the game
- `!warmup <start|end|0|1> [seconds]` - Unified warmup control
- `!endwarmup` / `!warmupend [seconds]` - End warmup period
- `!startwarmup` / `!warmupstart [seconds]` - Start warmup period
- `!bhop [off|matchmaking|supernatural]` - Bunny hopping mode with toggle
- `!infiniteammo [off|clip|reserve|both]` - Infinite ammo mode with toggle
- `!allowknifedrop` - Toggle knife dropping
- `!friendlyfire [team]` - Toggle friendly fire
- `!disabledamage` - Toggle damage on/off
- `!showimpacts [seconds]` - Show bullet impacts temporarily (no sv_cheats required)
- `!grenadeview [seconds|toggle|off]` - Show grenade trajectories (no sv_cheats required)
- `!maxgrenades <value>` - Set maximum grenades per player

### 👤 Player Manipulation Commands (7 Commands)
**Direct player control and manipulation**
- `!health <target> <value>` OR `!health <value>` - Set player health
- `!money <target> <value|+/-amount>` OR `!money <value>` - Set player money
- `!tp <from> [to]` OR `!tp <to>` - Teleport players
- `!kill <target>` OR `!kill` - Kill players
- `!kick <target>` - Kick players from server
- `!freeze <target>` / `!unfreeze <target>` - Freeze/unfreeze players
- `!noclip` - Toggle noclip for yourself (like console noclip command)

### ⚡ Advanced Player Commands (6 Commands)
**Complex player manipulation with state tracking**
- `!instantrespawn <target>` - Instantly respawn players with team logic
- `!respawnimmunity <target> [seconds]` - Grant temporary damage immunity
- `!god` - Toggle god mode for yourself (like console god command, no sv_cheats required)
- `!defaultprimary <weapon>` - Set default primary weapon for spawning
- `!defaultsecondary <weapon>` - Set default secondary weapon for spawning
- `!placebot [team] [name]` - Place bot with intelligent positioning

### 💰 Economy Commands (3 Commands)
**Server economy and money management**
- `!maxmoney <amount>` - Set maximum money players can have
- `!startmoney <amount>` - Set starting money for players
- `!afterroundmoney <amount>` - Set money awarded after round end

### ⏰ Time Management Commands (5 Commands)
**Game timing and phase control**
- `!warmuptime <seconds|default>` - Set warmup duration
- `!roundtime <minutes>` - Set round time
- `!roundfreezetime <seconds>` - Set freeze time duration
- `!roundrestartdelay <seconds>` - Set round restart delay
- `!buytime <seconds>` - Set buy time duration

### 🤖 Team & Bot Commands (4 Commands)
**Team balance and bot management**
- `!autoteambalance` - Toggle automatic team balance
- `!limitteams <value>` - Set team size limits
- `!botdifficulty <0-3>` - Set bot difficulty level
- `!botquota <count>` - Set number of bots

### 🛒 Utility Commands (3 Commands)
**Server utility functions**
- `!buyanywhere` - Toggle buy anywhere on map
- `!changemap <mapname|workshop/ID|workshopID|steamURL>` - Simple map changing with workshop support
- `!listmaps` - List available workshop maps and configured favorites

### 🔧 System Commands (3 Commands)
**Plugin management and system utilities**
- `!help [command|category]` - Show help information with categories
- `!reload` - Reload plugin configuration
- `!saveutilstate` - Save current server state manually

**Basic bot management**
- `!addbot <t|ct> [amount]` - Add bot(s) to specified team (1-10 bots)
- `!kickbot [t|ct|all] [amount]` - Kick bot(s) from team or all teams
- `!kickbots` - Kick all bots from the server

## 🎯 Advanced Features

### 🗺️ Simple Workshop Map Support
- **Multiple Input Formats**: Workshop IDs, Steam URLs, workshop/ format, map names
- **Automatic Command Selection**: Uses `ds_workshop_changelevel` for workshop maps
- **CS2 Native Config Support**: Leverages CS2's built-in per-map configuration system
- **Clean & Simple**: No complex categories, just reliable map loading

**Per-Map Configuration (CS2 Native):**
Create `csgo/cfg/{mapname}.cfg` files for automatic per-map settings:
```cfg
# csgo/cfg/aim_botz.cfg
mp_respawn_on_death_ct 1
mp_respawn_on_death_t 1
sv_infinite_ammo 2

# csgo/cfg/1v1v1v1.cfg
mp_maxplayers 2
mp_roundtime 3
```

**Usage Examples:**
```bash
!changemap 3070244931              # Workshop ID
!changemap workshop/3070244931     # Full workshop format
!changemap steamURL                # Steam Workshop URL
!changemap 1v1v1v1                 # Workshop map name
!changemap aim_botz                # Workshop map name
!changemap de_dust2                # Regular map
```

### 🗺️ Workshop Maps Management
- **List Available Maps**: `!listmaps` shows all workshop maps from your collection
- **Console Integration**: Uses `ds_workshop_listmaps` for comprehensive server output
- **Simple Display**: Shows available maps with ready-to-use commands

**Usage Examples:**
```bash
!listmaps                          # List all available workshop maps
# Output shows your workshop collection maps
```

### 🎯 Enhanced Grenade Trajectory System
- **Multiple Modes**: Timed, toggle, and permanent visualization
- **Smart Toggle**: `!grenadeview` without arguments toggles on/off
- **Large Number Support**: Values >24 hours treated as permanent
- **State Persistence**: Grenade view state survives server restarts
- **Flexible Control**: Support for seconds, toggle, or off modes

**Usage Examples:**
```bash
!grenadeview              # Toggle on/off
!grenadeview 30           # Enable for 30 seconds
!grenadeview 999          # Enable permanently (large number)
!grenadeview toggle       # Explicit toggle
!grenadeview off          # Disable
```

### 🚀 Simple Player Movement & God Mode System
- **Noclip Toggle**: `!noclip` executes the actual CS2 noclip command for you
- **God Mode Toggle**: `!god` executes the actual CS2 god command for you
- **No sv_cheats Required**: Removes cheat flags automatically from both commands
- **Console-like Behavior**: Uses CS2's native commands via ExecuteClientCommand
- **Player-only**: Only works for players, not console (like CS2's native commands)

**Usage Examples:**
```bash
!noclip                   # Executes "noclip" command for you
!god                      # Executes "god" command for you
# Both work exactly like typing the commands in console, but without needing sv_cheats 1
```

### 🐰 Enhanced Bunny Hopping System
- **Three Modes**: Off, Matchmaking (competitive), Supernatural (uncapped)
- **Smart Toggle**: `!bhop` without arguments toggles between off and last used mode
- **No sv_cheats Required**: Automatic cheat flag removal from convars
- **Server-Wide Application**: Affects all players simultaneously
- **State Persistence**: Mode and last mode survive server restarts

**Technical Implementation:**
- Removes `FCVAR_CHEAT` flag from bhop-related convars
- Applies specific cvar configurations for each mode
- Tracks current and last used mode for intelligent toggling

### 🔫 Comprehensive Infinite Ammo System
- **Four Modes**: Off, Clip (no reload), Reserve (reload required), Both
- **Toggle Support**: `!infiniteammo` toggles between off and last used mode
- **Cheat-Free Operation**: Removes cheat flags from `sv_infinite_ammo`
- **Instant Application**: Immediate effect on all players

**Technical Implementation:**
- Uses `sv_infinite_ammo 0/1/2` for different modes
- Automatic cheat flag removal for seamless operation
- State persistence with mode memory

### 🛡️ God Mode System
- **Complete Invulnerability**: Players take zero damage from all sources
- **Toggle Functionality**: Toggle god mode on/off for individual players or teams
- **State Persistence**: God mode status survives server restarts
- **Player Feedback**: Clear notifications when god mode is enabled/disabled
- **Event Integration**: Hooks into damage events to block all damage

**Technical Implementation:**
- Integrates with PlayerHurt event to block damage
- Maintains god mode state in dictionary with SteamID tracking
- Automatic state restoration on server restart
- Works alongside temporary immunity system

### 💾 Advanced Server Preset System
- **Complete State Capture**: Automatically saves all server settings
- **Custom Command Execution**: Add load/unload commands to presets
- **Smart Preset Switching**: Executes unload commands when switching
- **Persistent Storage**: JSON-based storage survives server restarts
- **Detailed Management**: Full CRUD operations on presets

**What Gets Saved:**
- Game state settings (bhop, infinite ammo, damage, etc.)
- Economy settings (money limits, buy settings)
- Time settings (warmup, round times, delays)
- Team and bot settings
- Grenade limits and weapon defaults

### 🎮 Intelligent Player Targeting System
- **Individual Players**: Target specific players by name with fuzzy matching
- **Team Targeting**: Target entire teams using 'ct', 't', 'terrorist', 'counter-terrorist'
- **All Players**: Target all players simultaneously with 'all'
- **Smart Resolution**: Automatic player name matching and validation
- **Error Handling**: Clear feedback for invalid targets

### 🔐 Comprehensive Permission System
- **Role-Based Access**: Different permission levels for different commands
- **Admin Integration**: Seamless integration with CounterStrikeSharp admin system
- **Granular Control**: Individual command permission requirements
- **Permission Feedback**: Clear messages when permissions are denied

## 🎮 Enhanced Help System

### Categorized Help
- `!help` - Shows overview of all command categories with emojis
- `!help <category>` - Shows all commands in a specific category
- `!help <command>` - Shows detailed information about a specific command

### Available Categories
- `presets` - Server preset management commands
- `game` - Game state and physics commands  
- `players` - Player manipulation commands
- `advanced` - Advanced player features
- `economy` - Money and economy commands
- `time` - Time and phase management
- `bots` - Bot and team management
- `system` - Plugin system commands

## 🔧 Technical Architecture

### Modular Design
- **Separate Command Classes**: Each category has its own command class
- **Dependency Injection**: Clean separation of concerns with proper DI
- **Event Integration**: PlayerHurt, PlayerSpawn event hooks for advanced features
- **State Management**: Centralized state management with JSON persistence

### Performance Optimizations
- **Resource Management**: Proper timer cleanup and disposal
- **Memory Efficiency**: Optimized data structures and cleanup procedures
- **Error Handling**: Comprehensive exception handling throughout
- **Async Operations**: Non-blocking operations where appropriate

### Integration Features
- **State Restoration**: All settings automatically restored on server restart
- **Command Cooldowns**: Built-in cooldown system to prevent spam
- **Permission Validation**: Every command validates permissions before execution
- **Rich Feedback**: Color-coded chat messages for different message types

This plugin provides a complete server administration solution with advanced features, intelligent systems, and comprehensive state management for Counter-Strike 2 servers.
