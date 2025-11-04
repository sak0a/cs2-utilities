# 🗺️ Per-Map Configuration Guide

This guide shows how to use CS2's built-in per-map configuration system with the CS2Utilities plugin.

## 🎯 How It Works

When you change maps using `!changemap`, CS2 automatically loads configuration files from `csgo/cfg/{mapname}.cfg` if they exist.

**Benefits:**
- ✅ **Standard CS2 Feature**: Uses official CS2 functionality
- ✅ **Automatic Loading**: No plugin intervention needed
- ✅ **Full Control**: Any console command can be used
- ✅ **Persistent**: Works even if plugin is disabled

## 📁 File Locations

Create configuration files in your server's `csgo/cfg/` directory:

```
csgo/cfg/
├── server.cfg              # Main server config
├── aim_botz.cfg            # Config for aim_botz map
├── 1v1v1v1.cfg             # Config for 1v1v1v1 map
├── de_dust2.cfg            # Config for de_dust2 map
└── mini_mirage.cfg         # Config for mini_mirage map
```

## 🎮 Example Configurations

### Aim Training Maps
**File: `csgo/cfg/aim_botz.cfg`**
```cfg
// Aim training settings
echo "Loading aim_botz configuration..."

// Respawn settings
mp_respawn_on_death_ct 1
mp_respawn_on_death_t 1
mp_respawn_immunitytime 0

// Round settings
mp_roundtime 0
mp_freezetime 0
mp_round_restart_delay 1

// Money and equipment
mp_buytime 0
mp_buy_anywhere 1
sv_infinite_ammo 2
mp_maxmoney 65000
mp_startmoney 65000

// Player limits
mp_maxplayers 10
mp_autoteambalance 0
mp_limitteams 0

// Bot settings (if using bots)
bot_kick all
bot_stop 1
```

### 1v1 Maps
**File: `csgo/cfg/1v1v1v1.cfg`**
```cfg
// 1v1 duel settings
echo "Loading 1v1v1v1 configuration..."

// Player limits
mp_maxplayers 2
mp_autoteambalance 0
mp_limitteams 0

// Round settings
mp_roundtime 3
mp_freezetime 3
mp_round_restart_delay 2

// No respawn for competitive feel
mp_respawn_on_death_ct 0
mp_respawn_on_death_t 0

// Money settings
mp_maxmoney 16000
mp_startmoney 800
mp_buytime 15

// Competitive settings
mp_solid_teammates 1
mp_teammates_are_enemies 0
```

### Surf Maps
**File: `csgo/cfg/surf_ski_2.cfg`**
```cfg
// Surf settings
echo "Loading surf_ski_2 configuration..."

// Physics modifications
sv_airaccelerate 1000
sv_air_max_wishspeed 30
sv_gravity 800

// Respawn settings
mp_respawn_on_death_ct 1
mp_respawn_on_death_t 1
mp_respawn_immunitytime 0

// Round settings
mp_roundtime 0
mp_freezetime 0

// Player settings
mp_maxplayers 20
mp_autoteambalance 0
mp_limitteams 0

// Remove fall damage
mp_falldamage 0
```

### Retake Maps
**File: `csgo/cfg/de_cache_retake.cfg`**
```cfg
// Retake settings
echo "Loading de_cache_retake configuration..."

// Money settings
mp_maxmoney 65000
mp_startmoney 65000
mp_buy_anywhere 1
mp_buytime 60

// Round settings
mp_roundtime 2
mp_freezetime 5
mp_round_restart_delay 3

// Team settings
mp_maxplayers 10
mp_autoteambalance 1
mp_limitteams 1

// Tactical settings
mp_solid_teammates 0
mp_teammates_are_enemies 0
```

## 🔧 Common Settings Reference

### Player Management
```cfg
mp_maxplayers 10                    # Maximum players
mp_autoteambalance 0               # Disable auto team balance
mp_limitteams 0                    # No team size limits
```

### Respawn Settings
```cfg
mp_respawn_on_death_ct 1           # CT respawn on death
mp_respawn_on_death_t 1            # T respawn on death
mp_respawn_immunitytime 0          # No spawn immunity
```

### Round Settings
```cfg
mp_roundtime 3                     # Round time in minutes
mp_freezetime 5                    # Freeze time in seconds
mp_round_restart_delay 2           # Delay between rounds
```

### Money & Equipment
```cfg
mp_maxmoney 65000                  # Maximum money
mp_startmoney 16000                # Starting money
mp_buytime 60                      # Buy time in seconds
mp_buy_anywhere 1                  # Buy anywhere on map
sv_infinite_ammo 2                 # Infinite ammo (2 = no reload)
```

### Physics (for movement maps)
```cfg
sv_airaccelerate 1000              # Air acceleration
sv_air_max_wishspeed 30            # Air strafe speed
sv_gravity 800                     # Gravity (default: 800)
mp_falldamage 0                    # Disable fall damage
```

## 🎯 Usage with CS2Utilities

1. **Create your map configs** in `csgo/cfg/`
2. **Use the plugin to change maps**:
   ```bash
   !changemap aim_botz              # Loads csgo/cfg/aim_botz.cfg
   !changemap 1v1v1v1               # Loads csgo/cfg/1v1v1v1.cfg
   !changemap 3070244931            # Loads csgo/cfg/3070244931.cfg
   ```
3. **CS2 automatically applies** the configuration when the map loads

## 💡 Pro Tips

1. **Test your configs**: Use `exec mapname.cfg` in console to test before map changes
2. **Add echo statements**: Include `echo "Loading mapname config..."` for debugging
3. **Use comments**: Document your settings with `//` comments
4. **Backup configs**: Keep copies of working configurations
5. **Workshop ID configs**: You can also create configs using workshop IDs as filenames

## 🔍 Troubleshooting

**Config not loading?**
- Check file is in correct location: `csgo/cfg/mapname.cfg`
- Verify filename matches exactly (case-sensitive on Linux)
- Check file permissions (readable by server)
- Look for syntax errors in config file

**Settings not applying?**
- Some settings require map restart to take effect
- Check console for error messages
- Verify ConVar names are correct
- Some settings may be protected/locked by server

This system gives you complete control over per-map settings while keeping the plugin simple and focused on reliable map loading!
