# CS2Utilities Extension Methods

This directory contains extension methods that extend the functionality of CounterStrikeSharp's base classes, making common player operations more convenient and readable.

## PlayerControllerExtensions.cs

Extends `CCSPlayerController` with convenient methods for player management.

### Validation Methods
- `IsPlayer()` - Checks if the controller represents a valid player
- `IsAlive()` - Checks if the player is alive and has a valid pawn
- `HasPermission(string permission)` - Checks if player has a specific permission

### Health and Armor Methods
- `SetHealth(int health, bool allowOverflow = true)` - Sets player health (0-1000)
- `GetHealth()` - Gets current player health
- `SetArmor(int armor, bool helmet = false, bool heavy = false)` - Sets armor and equipment

### Money Methods
- `SetMoney(int money)` - Sets player money (0-65535)
- `AddMoney(int amount)` - Adds/subtracts money from current amount
- `GetMoney()` - Gets current player money

### Movement and Position Methods
- `Freeze()` - Freezes the player (prevents movement)
- `Unfreeze()` - Unfreezes the player
- `IsFrozen()` - Checks if player is frozen
- `Teleport(Vector position, QAngle? angles = null)` - Teleports player to position
- `TeleportToPlayer(CCSPlayerController targetPlayer)` - Teleports to another player
- `GetEyePosition()` - Gets player's eye position

### Team and Server Actions
- `MoveToTeam(CsTeam team)` - Moves player to specified team
- `Kick(string reason = "Kicked by admin")` - Kicks player from server
- `Kill()` - Kills the player (forces suicide)

### Player Information Methods
- `SetName(string name)` - Sets player's name
- `SetClantag(string clantag = "")` - Sets player's clan tag
- `GetDisplayName()` - Gets formatted display name for logging/chat

## PlayerPawnExtensions.cs

Extends `CCSPlayerPawn` with pawn-specific operations.

### Movement Methods
- `Freeze()` - Freezes the pawn
- `Unfreeze()` - Unfreezes the pawn
- `IsFrozen()` - Checks if pawn is frozen

### God Mode and Immunity Methods
- `EnableGodMode()` - Enables complete invulnerability
- `DisableGodMode()` - Disables god mode
- `HasGodMode()` - Checks if god mode is enabled

### Health and Damage Methods
- `SetHealthSafe(int health, bool allowOverflow = true)` - Sets health with bounds checking
- `Heal(int amount, bool allowOverheal = false)` - Heals by specified amount
- `Damage(int amount)` - Damages by specified amount

### Weapon and Equipment Methods
- `StripWeapons()` - Removes all weapons from player
- `GiveWeapon(string weaponName)` - Gives a weapon to player

### Utility Methods
- `IsValidAndAlive()` - Checks if pawn is valid and alive
- `GetController()` - Gets associated controller
- `GetPosition()` - Gets pawn position
- `GetEyePosition()` - Gets eye position
- `SetVelocity(Vector velocity)` - Sets pawn velocity
- `AddVelocity(Vector velocity)` - Adds velocity (for knockback effects)
- `Respawn()` - Respawns the player

## Usage Examples

### Before (without extensions):
```csharp
// Setting player health
if (target.IsValid && !target.IsBot && target.PawnIsAlive)
{
    target.PlayerPawn.Value!.Health = health;
    Utilities.SetStateChanged(target.PlayerPawn.Value, "CBaseEntity", "m_iHealth");
}

// Setting player money
var moneyService = target.InGameMoneyServices;
if (moneyService != null)
{
    moneyService.Account = money;
    Utilities.SetStateChanged(target, "CCSPlayerController", "m_pInGameMoneyServices");
}
```

### After (with extensions):
```csharp
// Setting player health
if (target.IsAlive())
{
    target.SetHealth(health);
}

// Setting player money
if (target.IsPlayer())
{
    target.SetMoney(money);
}
```

## Benefits

1. **Cleaner Code**: Reduces boilerplate and makes intentions clear
2. **Consistent Validation**: Built-in null checks and validation
3. **Better Readability**: Method names clearly indicate what they do
4. **Reduced Errors**: Less chance of forgetting state updates or null checks
5. **Maintainability**: Centralized logic for common operations
6. **IntelliSense Support**: Better IDE support with method documentation

## Integration

These extensions are automatically available when you add:
```csharp
using CS2Utilities.Extensions;
```

The existing command implementations in `Commands/PlayerCommands.cs` have been updated to use these extension methods, demonstrating their practical usage.
