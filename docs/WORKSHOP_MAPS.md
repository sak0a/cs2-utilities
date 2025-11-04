# Workshop Maps - Complete Guide

This comprehensive guide covers everything you need to know about using workshop maps with the CS2Utilities plugin.

## Table of Contents

1. [Overview](#overview)
2. [Quick Start](#quick-start)
3. [Enhanced !changemap Command](#enhanced-changemap-command)
4. [Automatic Game Mode Detection](#automatic-game-mode-detection)
5. [Map Categories](#map-categories)
6. [Server Setup](#server-setup)
7. [Configuration](#configuration)
8. [Troubleshooting](#troubleshooting)
9. [Advanced Features](#advanced-features)

## Overview

The CS2Utilities plugin provides enhanced workshop map support with automatic game mode detection and configuration. When you change to a workshop map, the plugin automatically:

- Detects the map type (aim, surf, 1v1, etc.)
- Applies appropriate game mode and settings
- Configures server variables for optimal gameplay
- Manages player limits and respawn settings

### Key Features

- **Multiple Input Formats**: Workshop IDs, Steam URLs, workshop/ format
- **Automatic Game Mode Detection**: Detects aim, surf, 1v1, bhop, retake maps
- **Category-Specific Settings**: Auto-applies optimal settings per map type
- **Map Favorites System**: Save frequently used workshop maps with aliases
- **Smart Configuration**: Automatic respawn, physics, and game rule adjustments

## Quick Start

### 5-Minute Setup

1. **Configure Workshop Collection** (server.cfg):
   ```cfg
   host_workshop_collection YOUR_COLLECTION_ID
   sv_allowdownload 1
   ```

2. **Test Basic Commands**:
   ```bash
   !changemap 3070244931    # Aim Botz (auto-detects as 'aim')
   !changemap workshop/3070244931
   !changemap 3070244931 surf  # Force category
   ```

3. **Verify Settings Applied**: Check for instant respawn on aim maps, modified physics on surf maps

### Popular Workshop Map Examples

```bash
# Aim training maps
!changemap 3070244931    # Aim Botz
!changemap 243702660     # aim_redline

# Surf maps (replace with actual IDs)
!changemap 1234567890 surf

# 1v1 maps (replace with actual IDs)
!changemap 0987654321 1v1
```

## Enhanced !changemap Command

### Supported Input Formats

| Format | Example | Description |
|--------|---------|-------------|
| **Regular Map** | `de_dust2` | Standard CS2 maps |
| **Workshop ID** | `3070244931` | Direct workshop ID |
| **Workshop Format** | `workshop/3070244931` | Full workshop format |
| **Steam URL** | `https://steamcommunity.com/...` | Steam Workshop page URL |
| **With Category** | `3070244931 surf` | Force specific category |

### Command Syntax

```
!changemap <mapname|workshop/ID|workshopID|steamURL> [category|nomode]
```

**Parameters:**
- `mapname`: Regular CS2 map name
- `workshop/ID`: Workshop map in full format
- `workshopID`: Just the workshop ID number
- `steamURL`: Full Steam Workshop URL
- `category` (optional): Force specific category (aim, surf, 1v1, etc.)
- `nomode` (optional): Skip automatic game mode changes (keeps current settings)

### Usage Examples

```bash
# Regular map (no special settings)
!changemap de_dust2

# Workshop map with auto-detection (numeric ID)
!changemap 3070244931
# → Detects "aim" category, applies aim training settings

# Workshop map with auto-detection (map name)
!changemap 1v1v1v1
# → Auto-detects as workshop map, applies appropriate settings

!changemap aim_botz
# → Auto-detects as workshop map with "aim" category

# Workshop map with manual category
!changemap workshop/123456789 surf
# → Forces surf settings regardless of map name

# Steam Workshop URL
!changemap https://steamcommunity.com/sharedfiles/filedetails/?id=3070244931
# → Auto-detects and applies appropriate settings

# Using favorites (if configured)
!changemap aim_botz
# → Uses predefined settings from WorkshopMapFavorites

# Skip automatic game mode changes
!changemap 3070244931 nomode
# → Changes to workshop map but keeps current game mode/settings

# Alternative keywords for skipping mode changes
!changemap aim_botz keep
!changemap workshop/123456789 none
```

## Automatic Game Mode Detection

The plugin automatically detects map categories using:

1. **Map Name Patterns**: Recognizes common prefixes and keywords
2. **Workshop Favorites**: Uses configured favorites with predefined categories
3. **Manual Override**: Allows specifying category as second parameter
4. **Skip Mode Changes**: Use special keywords to prevent automatic mode changes

### Preventing Automatic Mode Changes

Sometimes you want to change to a workshop map but keep your current game mode and settings. Use these special keywords as the second parameter:

- `nomode` - Skip all automatic game mode changes
- `keep` - Keep current settings (same as nomode)
- `none` - Don't apply any category settings (same as nomode)

**Examples:**
```bash
# Change to aim map but keep current competitive settings
!changemap 3070244931 nomode

# Change to surf map but keep current casual settings
!changemap surf_ski2 keep

# Change using workshop URL but don't change mode
!changemap https://steamcommunity.com/sharedfiles/filedetails/?id=123456789 none
```

### Detection Patterns

| Pattern | Category | Examples |
|---------|----------|----------|
| `aim_*` | aim | aim_botz, aim_redline |
| `surf_*` | surf | surf_ski2, surf_mesa |
| `1v1*` or `arena*` | 1v1 | 1v1_arena, arena_dust2 |
| `bhop_*` or `kz_*` | bhop | bhop_monster, kz_longjumps2 |
| `awp_*` | awp | awp_lego_2, awp_india |
| `hs_*` | hs | hs_world, hs_aim |
| `*retake*` | retake | mirage_retake |

## Map Categories

### Supported Categories

| Category | Description | Key Settings |
|----------|-------------|--------------|
| **aim** | Aim training maps | Instant respawn, infinite ammo, no round timer |
| **surf** | Surf maps | Air acceleration 1000, gravity 800, respawn |
| **bhop** | Bunny hop maps | Auto bunnyhopping, air acceleration |
| **1v1** | 1 vs 1 duel maps | 2 players max, round-based gameplay |
| **retake** | Retake scenario maps | Buy anywhere, high money, short rounds |
| **awp** | AWP-focused maps | Instant respawn, high money |
| **hs** | Headshot only maps | Headshot damage only |

### What Happens When You Change to Different Map Types

| Map Type | Game Mode | Key Settings Applied |
|----------|-----------|---------------------|
| **Regular Maps** | Keeps current | No changes |
| **Aim Maps** | Casual | Instant respawn, infinite ammo, no round timer |
| **Surf Maps** | Casual | Air acceleration 1000, modified gravity |
| **1v1 Maps** | Casual | Max 2 players, 3-minute rounds |
| **Bhop Maps** | Casual | Auto bunnyhopping, air acceleration |
| **Retake Maps** | Casual | Buy anywhere, high money, short rounds |
| **AWP Maps** | Casual | Instant respawn, high money |

## Server Setup

### Prerequisites

1. **Steam Workshop Subscription**: Subscribe to workshop maps on Steam
2. **Server Configuration**: Proper server setup for workshop content
3. **Plugin Installation**: CS2Utilities plugin properly installed

### Server Configuration Methods

#### Method 1: Workshop Collection (Recommended)

1. **Create Steam Workshop Collection**:
   - Go to Steam Workshop for CS2
   - Create a new collection
   - Add your favorite workshop maps
   - Note the collection ID from the URL

2. **Configure Server** (server.cfg):
   ```cfg
   host_workshop_collection YOUR_COLLECTION_ID
   sv_allowdownload 1
   ```

#### Method 2: Individual Maps

```cfg
# Add to server.cfg or startup parameters
workshop_download_item 730 3070244931  # Aim Botz
workshop_download_item 730 243702660   # aim_redline
```

#### Method 3: Startup Parameters

```bash
# Linux/Windows server startup
./srcds_run -game csgo +map workshop/3070244931 +maxplayers 10

# With collection
./srcds_run -game csgo +host_workshop_collection 123456789
```

#### Method 4: Map Groups (Advanced)

Create `mapgroup.txt`:
```
"mapgroup"
{
    "mg_workshop_aim"
    {
        "name" "Workshop Aim Maps"
        "maps"
        {
            "workshop/3070244931" ""
            "workshop/243702660" ""
        }
    }
}
```

## Configuration

### Automatic Configuration

The plugin includes default configurations for all supported categories. No manual setup is required for basic functionality.

### Custom Configuration (Optional)

#### Workshop Map Favorites

```json
{
  "WorkshopMapFavorites": {
    "aim_botz": {
      "WorkshopId": "3070244931",
      "Name": "Aim Botz",
      "Description": "Popular aim training map",
      "Category": "aim",
      "WorkshopUrl": "https://steamcommunity.com/sharedfiles/filedetails/?id=3070244931",
      "OnLoadCommands": [
        "mp_respawn_on_death_ct 1",
        "mp_respawn_on_death_t 1",
        "sv_infinite_ammo 2",
        "mp_roundtime 0"
      ],
      "MaxPlayers": 10,
      "RequiresCustomSettings": true
    }
  }
}
```

#### Map Category Defaults

```json
{
  "MapCategoryDefaults": {
    "aim": {
      "GameMode": "casual",
      "GameType": "classic",
      "Commands": [
        "mp_respawn_on_death_ct 1",
        "mp_respawn_on_death_t 1",
        "mp_respawn_immunitytime 0",
        "mp_roundtime 0",
        "mp_freezetime 0",
        "mp_buytime 0",
        "sv_infinite_ammo 2"
      ],
      "MaxPlayers": 10,
      "Description": "Aim training and practice maps"
    },
    "surf": {
      "GameMode": "casual",
      "GameType": "classic",
      "Commands": [
        "sv_airaccelerate 1000",
        "sv_gravity 800",
        "sv_maxspeed 3500",
        "mp_respawn_on_death_ct 1",
        "mp_respawn_on_death_t 1",
        "mp_roundtime 0",
        "mp_freezetime 0"
      ],
      "MaxPlayers": 20,
      "Description": "Surf maps with modified physics"
    }
  }
}
```

### Configuration Properties

#### WorkshopMapFavorites Properties

| Property | Type | Required | Description |
|----------|------|----------|-------------|
| `WorkshopId` | string | Yes | Steam Workshop ID |
| `Name` | string | No | Display name for the map |
| `Description` | string | No | Map description |
| `Category` | string | No | Map category (aim, surf, 1v1, etc.) |
| `OnLoadCommands` | array | No | Commands to execute when loading map |
| `MaxPlayers` | number | No | Maximum players for this map |

#### MapCategoryDefaults Properties

| Property | Type | Required | Description |
|----------|------|----------|-------------|
| `GameMode` | string | No | Game mode to apply |
| `GameType` | string | No | Game type to apply |
| `Commands` | array | No | Console commands to execute |
| `MaxPlayers` | number | No | Maximum players for this category |
| `Description` | string | No | Category description |

## Troubleshooting

### Common Issues and Solutions

#### 1. Workshop Map Won't Load

**Symptoms:**
- `!changemap 123456789` shows "Failed to change map"
- Server stays on current map

**Solutions:**
```bash
# Check workshop subscription
# Visit: https://steamcommunity.com/sharedfiles/filedetails/?id=YOUR_ID

# Verify server setup (server.cfg)
host_workshop_collection YOUR_COLLECTION_ID

# Test correct formats
!changemap 3070244931           # Correct
!changemap workshop/3070244931  # Correct
```

#### 2. Wrong Game Mode Applied

**Symptoms:**
- Map loads but wrong settings applied
- Surf map without proper physics

**Solutions:**
```bash
# Force specific category
!changemap 3070244931 surf

# Enable debug logging (config)
"EnableDebugLogging": true

# Configure map favorites with correct category
```

#### 3. Settings Not Applied

**Symptoms:**
- Map loads but server settings unchanged

**Solutions:**
```bash
# Check plugin permissions
# Verify console commands work manually:
mp_respawn_on_death_ct 1
sv_airaccelerate 1000

# Enable debug logging to see command execution
```

### Error Messages and Solutions

| Error Message | Cause | Solution |
|---------------|-------|----------|
| "Invalid map format" | Wrong workshop ID format | Use: `3070244931` or `workshop/3070244931` |
| "Failed to change map" | Map not accessible | Check workshop subscription |
| "No configuration found for category" | Missing category config | Add to MapCategoryDefaults |

### Debug Information

Enable debug logging:
```json
{
  "EnableDebugLogging": true
}
```

Look for console messages:
```
[CS2Utils] Applied aim category settings (7 commands)
[CS2Utils] Changing to workshop map 3070244931...
```

### Validation Checklist

- [ ] Workshop map is public and accessible
- [ ] Server has subscribed to workshop content
- [ ] Plugin has proper permissions
- [ ] Configuration syntax is valid
- [ ] Manual commands work in console

## Advanced Features

### Custom Categories

Add custom categories:
```json
{
  "MapCategoryDefaults": {
    "custom_category": {
      "GameMode": "casual",
      "Commands": [
        "your_custom_command 1",
        "another_setting 0"
      ],
      "MaxPlayers": 16,
      "Description": "Your custom category"
    }
  }
}
```

### Integration with Presets

Workshop maps work with server presets:
```bash
# Save current state including workshop map
!savepreset "aim_training_setup" "Aim training configuration"

# Load preset (includes map change)
!loadpreset "aim_training_setup"
```

### Performance Optimization

#### Server-Side
```cfg
# Optimize workshop content loading
host_workshop_collection YOUR_COLLECTION_ID
workshop_start_map YOUR_DEFAULT_MAP

# Reduce network overhead
sv_maxrate 0
sv_minrate 128000
```

#### Plugin Configuration
```json
{
  "EnableDebugLogging": false,  // Disable in production
  "AutoSaveState": true,        // Keep enabled
  "AutoSaveInterval": 300       // Adjust as needed
}
```

---

This unified guide provides everything you need to set up and use workshop maps with CS2Utilities. The system automatically handles game mode detection and applies optimal settings for different map types, creating a seamless experience for both players and administrators.
