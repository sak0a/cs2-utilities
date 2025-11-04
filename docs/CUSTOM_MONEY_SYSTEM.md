# Custom Money System Documentation

## Overview

The CS2Utilities plugin features an intelligent **Custom Money System** that allows players to have money values exceeding the server's `mp_maxmoney` limit. The system automatically tracks and restores custom money values after purchases, ensuring players maintain their intended money amounts.

## How It Works

### Smart Tracking Logic

The system only tracks money when it **exceeds the server's `mp_maxmoney` limit**, making it highly efficient:

- **Normal Money (≤ server limit)**: No tracking overhead
- **Custom Money (> server limit)**: Automatic tracking and restoration

### Server Limit Detection

The system automatically detects the server's `mp_maxmoney` convar value:
- **Competitive**: Usually 16,000
- **Wingman**: Usually 8,000  
- **Casual**: Usually 12,000
- **Custom**: Any value set by server admin

## Commands

### Money Commands

#### Chat Commands
```
!money <value>                    # Set your money
!money <player> <value>           # Set specific player's money
!money <team> <value>             # Set team money (ct/t/all)
!money <target> +<value>          # Add money (modifier)
!money <target> -<value>          # Subtract money (modifier)
```

#### Console Commands
```
css_money <value>                 # Set your money
css_money <player> <value>        # Set specific player's money
css_money <team> <value>          # Set team money (ct/t/all)
css_money <target> +<value>       # Add money (modifier)
css_money <target> -<value>       # Subtract money (modifier)
```

### Management Commands

#### Clear Custom Tracking
```
!clearmoney <player>              # Stop tracking specific player
!clearmoney <team>                # Stop tracking team
!clearmoney all                   # Stop tracking all players

css_clearmoney <player>           # Console version
css_clearmoney <team>             # Console version
css_clearmoney all                # Console version
```

## User Experience

### Informative Messages

When custom money tracking is enabled/disabled, players receive clear feedback:

#### Tracking Enabled
```
💰 Money set to $25000 (exceeds server limit $16000). 
   Custom tracking enabled to maintain value after purchases.
```

#### Tracking Disabled
```
💰 Money set to $12000 (within server limit $16000). 
   Custom tracking disabled.
```

### Visual Indicators

- **Blue Info Messages**: System status updates
- **Green Success Messages**: Command execution confirmations
- **Red Error Messages**: Invalid commands or failures

## Technical Implementation

### Architecture Components

1. **Money Tracking Dictionary**: `Dictionary<ulong, int> _customMoneyValues`
2. **Server Limit Detection**: `GetServerMaxMoney()` method
3. **Restoration Timer**: 0.5-second monitoring interval
4. **Event Handlers**: Player disconnect and round start cleanup

### Performance Optimizations

#### Conditional Monitoring
- Timer only runs when players have custom money
- `HasAnyCustomMoney()` prevents unnecessary loops
- Zero overhead when all money is within server limits

#### Smart Cleanup
- Automatic removal when money drops to normal levels
- Player disconnect cleanup prevents memory leaks
- Round start restoration ensures consistency

### Memory Management

```csharp
// Efficient tracking check
public bool HasAnyCustomMoney() => _customMoneyValues.Count > 0;

// Automatic cleanup on disconnect
[GameEventHandler]
public HookResult OnPlayerDisconnect(EventPlayerDisconnect @event, GameEventInfo info)
{
    _playerCommands.RemoveCustomMoney(player.SteamID);
    return HookResult.Continue;
}
```

## Usage Examples

### Scenario 1: Competitive Server (mp_maxmoney 16000)

```bash
# Normal money - no tracking
!money 12000
> Money set to $12000 (within server limit $16000). Custom tracking disabled.

# Custom money - tracking enabled  
!money 25000
> Money set to $25000 (exceeds server limit $16000). Custom tracking enabled to maintain value after purchases.

# After buying AWP (~$4750)
# System automatically restores money from ~$11250 back to $25000

# Return to normal money
!money 15000  
> Money set to $15000 (within server limit $16000). Custom tracking disabled.
```

### Scenario 2: Wingman Server (mp_maxmoney 8000)

```bash
# Normal money - no tracking
!money 5000
> Money set to $5000 (within server limit $8000). Custom tracking disabled.

# Custom money - tracking enabled
!money 15000
> Money set to $15000 (exceeds server limit $8000). Custom tracking enabled to maintain value after purchases.

# After purchases, money is automatically restored to $15000
```

## Benefits

### For Players
- **Persistent Money**: Values maintained through purchases
- **Clear Feedback**: Always know when tracking is active
- **No Manual Management**: Automatic restoration
- **Flexible Limits**: Works with any server configuration

### For Server Performance
- **Zero Overhead**: No tracking when unnecessary
- **Efficient Monitoring**: Only checks players who need it
- **Smart Cleanup**: Automatic memory management
- **Minimal CPU Usage**: Optimized timer intervals

### For Server Admins
- **No Configuration**: Works with existing server settings
- **Debug Logging**: Optional detailed logging
- **Permission Control**: Requires `@css/root` permission
- **Console Support**: Full console command integration

## Troubleshooting

### Common Issues

#### Money Resets After Purchases
- **Cause**: Server `mp_maxmoney` limit enforcement
- **Solution**: System automatically detects and handles this

#### Tracking Not Working
- **Check**: Ensure money value exceeds server's `mp_maxmoney`
- **Verify**: Player has required permissions (`@css/root`)
- **Debug**: Enable debug logging in config

#### Performance Concerns
- **Monitor**: Use `!clearmoney all` to stop all tracking
- **Optimize**: System only tracks when necessary
- **Check**: Debug logs show tracking start/stop events

### Debug Information

Enable debug logging in `CS2UtilitiesConfig`:
```json
{
  "EnableDebugLogging": true
}
```

Debug output includes:
- Server `mp_maxmoney` detection
- Tracking start/stop events
- Money restoration activities
- Error handling information

## Configuration

### Server ConVars

The system reads the server's `mp_maxmoney` convar automatically:
```
mp_maxmoney 16000    # Competitive standard
mp_maxmoney 8000     # Wingman standard  
mp_maxmoney 12000    # Casual standard
```

### Plugin Config

No additional configuration required. The system adapts to any server setup automatically.

## API Reference

### Public Methods

```csharp
// Check if player has custom money tracking
public bool HasCustomMoney(ulong steamId)

// Check if any players are being tracked
public bool HasAnyCustomMoney()

// Remove custom money tracking
public void RemoveCustomMoney(ulong steamId)

// Restore custom money if needed
public void CheckAndRestoreCustomMoney(CCSPlayerController player)
```

### Events

```csharp
// Cleanup on player disconnect
[GameEventHandler]
public HookResult OnPlayerDisconnect(EventPlayerDisconnect @event, GameEventInfo info)

// Restore money on round start
[GameEventHandler] 
public HookResult OnRoundStart(EventRoundStart @event, GameEventInfo info)
```

## Version History

- **v1.0**: Initial custom money tracking implementation
- **v1.1**: Added smart tracking (only when exceeding server limits)
- **v1.2**: Added informative user messages and comprehensive documentation

---

*This system ensures players can enjoy custom money values while maintaining optimal server performance and providing clear user feedback.*
