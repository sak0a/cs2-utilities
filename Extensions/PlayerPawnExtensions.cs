using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;

namespace CS2Utilities.Extensions
{
    /// <summary>
    /// Provides extension methods for <see cref="CCSPlayerPawn"/> to perform pawn-specific actions.
    /// </summary>
    public static class PlayerPawnExtensions
    {
        #region Movement Methods

        /// <summary>
        /// Freezes the player pawn, preventing movement.
        /// </summary>
        /// <param name="pawn">The player pawn to freeze.</param>
        public static void Freeze(this CCSPlayerPawn? pawn)
        {
            if (pawn == null || !pawn.IsValid)
                return;

            pawn.MoveType = MoveType_t.MOVETYPE_NONE;
            Utilities.SetStateChanged(pawn, "CBaseEntity", "m_MoveType");
        }

        /// <summary>
        /// Unfreezes the player pawn, allowing movement.
        /// </summary>
        /// <param name="pawn">The player pawn to unfreeze.</param>
        public static void Unfreeze(this CCSPlayerPawn? pawn)
        {
            if (pawn == null || !pawn.IsValid)
                return;

            pawn.MoveType = MoveType_t.MOVETYPE_WALK;
            Utilities.SetStateChanged(pawn, "CBaseEntity", "m_MoveType");
        }

        /// <summary>
        /// Checks if the player pawn is frozen.
        /// </summary>
        /// <param name="pawn">The player pawn to check.</param>
        /// <returns><c>true</c> if the pawn is frozen; otherwise, <c>false</c>.</returns>
        public static bool IsFrozen(this CCSPlayerPawn? pawn)
        {
            return pawn != null && pawn.IsValid && pawn.MoveType == MoveType_t.MOVETYPE_NONE;
        }

        #endregion

        #region God Mode and Immunity Methods

        /// <summary>
        /// Enables god mode for the player pawn (complete invulnerability).
        /// </summary>
        /// <param name="pawn">The player pawn to enable god mode for.</param>
        public static void EnableGodMode(this CCSPlayerPawn? pawn)
        {
            if (pawn == null || !pawn.IsValid)
                return;

            pawn.TakesDamage = false;
            Utilities.SetStateChanged(pawn, "CBaseEntity", "m_takedamage");
        }

        /// <summary>
        /// Disables god mode for the player pawn.
        /// </summary>
        /// <param name="pawn">The player pawn to disable god mode for.</param>
        public static void DisableGodMode(this CCSPlayerPawn? pawn)
        {
            if (pawn == null || !pawn.IsValid)
                return;

            pawn.TakesDamage = true;
            Utilities.SetStateChanged(pawn, "CBaseEntity", "m_takedamage");
        }

        /// <summary>
        /// Checks if the player pawn has god mode enabled.
        /// </summary>
        /// <param name="pawn">The player pawn to check.</param>
        /// <returns><c>true</c> if god mode is enabled; otherwise, <c>false</c>.</returns>
        public static bool HasGodMode(this CCSPlayerPawn? pawn)
        {
            return pawn != null && pawn.IsValid && !pawn.TakesDamage;
        }

        #endregion

        #region Health and Damage Methods

        /// <summary>
        /// Sets the health of the player pawn with bounds checking.
        /// </summary>
        /// <param name="pawn">The player pawn to set health for.</param>
        /// <param name="health">The health value to set (0-1000).</param>
        /// <param name="allowOverflow">Whether to allow health to exceed max health.</param>
        public static void SetHealthSafe(this CCSPlayerPawn? pawn, int health, bool allowOverflow = true)
        {
            if (pawn == null || !pawn.IsValid)
                return;

            pawn.Health = Math.Max(0, Math.Min(1000, health));

            if (allowOverflow && health > pawn.MaxHealth)
                pawn.MaxHealth = health;

            Utilities.SetStateChanged(pawn, "CBaseEntity", "m_iHealth");
        }

        /// <summary>
        /// Heals the player pawn by the specified amount.
        /// </summary>
        /// <param name="pawn">The player pawn to heal.</param>
        /// <param name="amount">The amount to heal.</param>
        /// <param name="allowOverheal">Whether to allow healing beyond max health.</param>
        public static void Heal(this CCSPlayerPawn? pawn, int amount, bool allowOverheal = false)
        {
            if (pawn == null || !pawn.IsValid || amount <= 0)
                return;

            var newHealth = pawn.Health + amount;
            if (!allowOverheal)
                newHealth = Math.Min(newHealth, pawn.MaxHealth);

            pawn.SetHealthSafe(newHealth, allowOverheal);
        }

        /// <summary>
        /// Damages the player pawn by the specified amount.
        /// </summary>
        /// <param name="pawn">The player pawn to damage.</param>
        /// <param name="amount">The amount of damage to deal.</param>
        public static void Damage(this CCSPlayerPawn? pawn, int amount)
        {
            if (pawn == null || !pawn.IsValid || amount <= 0)
                return;

            var newHealth = Math.Max(0, pawn.Health - amount);
            pawn.SetHealthSafe(newHealth);

            if (newHealth <= 0)
                pawn.CommitSuicide(false, true);
        }

        #endregion

        #region Weapon and Equipment Methods

        /// <summary>
        /// Strips all weapons from the player pawn.
        /// </summary>
        /// <param name="pawn">The player pawn to strip weapons from.</param>
        public static void StripWeapons(this CCSPlayerPawn? pawn)
        {
            if (pawn == null || !pawn.IsValid)
                return;

            pawn.RemoveWeapons();
        }

        /// <summary>
        /// Gives a weapon to the player pawn.
        /// </summary>
        /// <param name="pawn">The player pawn to give the weapon to.</param>
        /// <param name="weaponName">The weapon name (e.g., "weapon_ak47").</param>
        public static void GiveWeapon(this CCSPlayerPawn? pawn, string weaponName)
        {
            if (pawn == null || !pawn.IsValid || string.IsNullOrEmpty(weaponName))
                return;

            pawn.GiveNamedItem(weaponName);
        }

        #endregion

        #region Respawn Methods

        /// <summary>
        /// Respawns the player pawn instantly.
        /// </summary>
        /// <param name="pawn">The player pawn to respawn.</param>
        public static void Respawn(this CCSPlayerPawn? pawn)
        {
            if (pawn?.Controller.Value?.As<CCSPlayerController>() == null)
                return;

            var controller = pawn.Controller.Value.As<CCSPlayerController>()!;
            
            // Queue for next frame to avoid threading issues
            Server.NextFrame(() => 
            {
                controller.Respawn();
            });
        }

        #endregion

        #region Utility Methods

        /// <summary>
        /// Checks if the player pawn is valid and alive.
        /// </summary>
        /// <param name="pawn">The player pawn to check.</param>
        /// <returns><c>true</c> if the pawn is valid and alive; otherwise, <c>false</c>.</returns>
        public static bool IsValidAndAlive(this CCSPlayerPawn? pawn)
        {
            return pawn != null && pawn.IsValid && pawn.LifeState == (byte)LifeState_t.LIFE_ALIVE;
        }

        /// <summary>
        /// Gets the controller associated with this pawn.
        /// </summary>
        /// <param name="pawn">The player pawn to get the controller for.</param>
        /// <returns>The associated <see cref="CCSPlayerController"/>, or null if not found.</returns>
        public static CCSPlayerController? GetController(this CCSPlayerPawn? pawn)
        {
            return pawn?.Controller.Value?.As<CCSPlayerController>();
        }

        /// <summary>
        /// Gets the position of the player pawn.
        /// </summary>
        /// <param name="pawn">The player pawn to get the position for.</param>
        /// <returns>The position as a <see cref="Vector"/>, or <see cref="Vector.Zero"/> if invalid.</returns>
        public static Vector GetPosition(this CCSPlayerPawn? pawn)
        {
            return pawn?.AbsOrigin ?? Vector.Zero;
        }

        /// <summary>
        /// Gets the eye position of the player pawn.
        /// </summary>
        /// <param name="pawn">The player pawn to get the eye position for.</param>
        /// <returns>The eye position as a <see cref="Vector"/>.</returns>
        public static Vector GetEyePosition(this CCSPlayerPawn? pawn)
        {
            if (pawn?.AbsOrigin == null)
                return Vector.Zero;

            var absOrigin = pawn.AbsOrigin;
            var camera = pawn.CameraServices;
            var viewOffset = camera?.OldPlayerViewOffsetZ ?? 64.0f;

            return new Vector(absOrigin.X, absOrigin.Y, absOrigin.Z + viewOffset);
        }

        /// <summary>
        /// Sets the velocity of the player pawn.
        /// </summary>
        /// <param name="pawn">The player pawn to set velocity for.</param>
        /// <param name="velocity">The velocity vector to set.</param>
        public static void SetVelocity(this CCSPlayerPawn? pawn, Vector velocity)
        {
            if (pawn == null || !pawn.IsValid)
                return;

            pawn.AbsVelocity = velocity;
            Utilities.SetStateChanged(pawn, "CBaseEntity", "m_vecAbsVelocity");
        }

        /// <summary>
        /// Adds velocity to the player pawn (for knockback effects, etc.).
        /// </summary>
        /// <param name="pawn">The player pawn to add velocity to.</param>
        /// <param name="velocity">The velocity vector to add.</param>
        public static void AddVelocity(this CCSPlayerPawn? pawn, Vector velocity)
        {
            if (pawn?.AbsVelocity == null)
                return;

            var currentVelocity = pawn.AbsVelocity;
            var newVelocity = new Vector(
                currentVelocity.X + velocity.X,
                currentVelocity.Y + velocity.Y,
                currentVelocity.Z + velocity.Z
            );

            pawn.SetVelocity(newVelocity);
        }

        #endregion
    }
}
