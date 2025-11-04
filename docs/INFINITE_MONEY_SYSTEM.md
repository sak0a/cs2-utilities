# Infinite Money System Documentation

## Overview

The CS2Utilities plugin features a **Infinite Money System** that provides unlimited money to players when their money is set above the server's `mp_maxmoney` limit. This system automatically maintains maximum money levels for designated players, ensuring they always have sufficient funds without complex tracking.

## How It Works

### Simple Infinite Money Logic

The system operates on a straightforward principle:

1. **Detection**: When money is set above `mp_maxmoney` limit → Enable infinite money
2. **Monitoring**: Timer checks players with infinite money every 0.5 seconds
3. **Restoration**: If money drops below server max → Restore to server max
4. **Cleanup**: When money is set within normal limits → Disable infinite money

### Server Limit Detection

```csharp
// Automatically detects server's mp_maxmoney setting
var maxMoney = GetServerMaxMoney(); // e.g., 16000 in competitive, 8000 in wingman

// Decision logic
if (setMoney > maxMoney)
    EnableInfiniteMoney(player);
else
    DisableInfiniteMoney(player);
```

## Key Features

### ✅ **Automatic Operation**
- **Zero Configuration**: Works with any server `mp_maxmoney` setting
- **Smart Detection**: Automatically reads server ConVar values
- **Instant Activation**: Infinite money enabled immediately when needed

### ✅ **Performance Optimized**
- **Conditional Monitoring**: Timer only runs when players have infinite money
- **Efficient Checks**: Only monitors players who need infinite money
- **Memory Efficient**: Simple boolean tracking instead of complex values

### ✅ **User-Friendly Messages**
- **Clear Feedback**: Players know when infinite money is enabled/disabled
- **Status Updates**: Informative messages about money system state
- **Debug Logging**: Optional detailed logging for troubleshooting

## Usage Examples

### Basic Commands

```bash
# Set money within server limits (normal behavior)
!money 8000
> Money set to $8000 (within server limit $16000). Infinite money disabled.

# Set money above server limits (infinite money enabled)
!money 25000
> Money set to $25000 (exceeds server limit $16000). Infinite money enabled.

# Player buys AWP (costs $4750)
# System automatically restores money to $16000 (server max)

# Disable infinite money
!money 10000
> Money set to $10000 (within server limit $16000). Infinite money disabled.
```

### Admin Commands

```bash
# Clear infinite money for all players
!clearmoney
> Disabled infinite money for 3 player(s)

# Check plugin status
!cs2utils
> Infinite Money System: Active (2 players)
```

## Technical Implementation

### Core Components

#### **1. Player Tracking**
```csharp
private readonly Dictionary<ulong, bool> _infiniteMoneyPlayers;
```

#### **2. Money Restoration**
```csharp
public void CheckAndRestoreInfiniteMoney(CCSPlayerController player)
{
    if (currentMoney < maxMoney)
    {
        player.SetMoney(maxMoney); // Restore to server max
    }
}
```

#### **3. Monitoring Timer**
```csharp
// Runs every 0.5 seconds, only when needed
_moneyTimer = new Timer(CheckAllPlayersInfiniteMoney, null, 
    TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(0.5));
```

### Event Handling

#### **Player Disconnect Cleanup**
```csharp
[GameEventHandler]
public HookResult OnPlayerDisconnect(EventPlayerDisconnect @event)
{
    _playerCommands.RemoveInfiniteMoney(player.SteamID);
    return HookResult.Continue;
}
```

#### **Round Start Restoration**
```csharp
[GameEventHandler] 
public HookResult OnRoundStart(EventRoundStart @event)
{
    // Restore infinite money for all tracked players
    foreach (var player in Utilities.GetPlayers())
    {
        if (player.IsPlayer() && HasInfiniteMoney(player.SteamID))
        {
            CheckAndRestoreInfiniteMoney(player);
        }
    }
}
```

## Benefits Over Complex Tracking

### 🎯 **Simplicity**
- **No Value Tracking**: Just boolean on/off state
- **No Complex Logic**: Simple max money restoration
- **Easy Debugging**: Clear infinite money status

### 🚀 **Performance**
- **Minimal Memory**: Only stores boolean flags
- **Fast Execution**: Simple money restoration logic
- **Efficient Monitoring**: Only runs when necessary

### 🛠️ **Reliability**
- **No Edge Cases**: Always restores to server maximum
- **Consistent Behavior**: Predictable infinite money experience
- **Automatic Cleanup**: Self-managing system

## Configuration

The infinite money system uses the main plugin configuration:

```json
{
  "EnableDebugLogging": true,  // Show detailed infinite money logs
  "AutoSaveState": true,       // Save infinite money states
  "RestoreStateOnLoad": true   // Restore on plugin reload
}
```

## Troubleshooting

### Common Issues

#### **Money Not Restoring**
- Check if player has infinite money enabled: `HasInfiniteMoney(steamId)`
- Verify timer is running: Look for debug logs
- Ensure player is valid and connected

#### **Performance Issues**
- Monitor timer frequency (default: 0.5 seconds)
- Check number of players with infinite money
- Review debug logging overhead

#### **State Not Persisting**
- Verify `AutoSaveState` is enabled in config
- Check CounterStrikeSharp config file permissions
- Review plugin reload behavior

### Debug Commands

```bash
# Check infinite money status
!cs2utils
> Shows count of players with infinite money

# Enable debug logging
# Edit CS2Utilities.json: "EnableDebugLogging": true

# View debug output
[CS2Utils] Enabled infinite money for PlayerName ($25000 > server limit $16000)
[CS2Utils] Restored infinite money for PlayerName: $12000 -> $16000
[CS2Utils] Disabled infinite money for PlayerName ($8000 <= server limit $16000)
```

## API Reference

### Public Methods

```csharp
// Check if player has infinite money
bool HasInfiniteMoney(ulong steamId)

// Check if any players have infinite money  
bool HasAnyInfiniteMoney()

// Get count of players with infinite money
int GetInfiniteMoneyCount()

// Remove infinite money for player
void RemoveInfiniteMoney(ulong steamId)

// Clear infinite money for player
void ClearInfiniteMoney(CCSPlayerController player)
```

### Events

```csharp
// Player disconnect cleanup
OnPlayerDisconnect(EventPlayerDisconnect @event)

// Round start restoration  
OnRoundStart(EventRoundStart @event)
```

This infinite money system provides a much simpler and more reliable approach to handling money values above server limits, focusing on user experience and performance rather than complex value tracking.
