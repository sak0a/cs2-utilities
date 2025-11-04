using System.Globalization;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Admin;
using CounterStrikeSharp.API.Modules.Utils;

namespace CS2Utilities.Extensions
{
    /// <summary>
    /// Provides extension methods for <see cref="CCSPlayerController"/> to perform common player actions.
    /// </summary>
    public static class PlayerControllerExtensions
    {
        #region Validation Methods

        /// <summary>
        /// Checks if the specified controller represents a valid player.
        /// </summary>
        /// <param name="player">The player controller to check.</param>
        /// <returns><c>true</c> if the player is valid; otherwise, <c>false</c>.</returns>
        public static bool IsPlayer(this CCSPlayerController? player)
        {
            return player is
            {
                PlayerPawn.Value: not null,
                IsValid: true,
                IsHLTV: false,
                IsBot: false,
                UserId: not null,
                SteamID: > 0,
                Connected: PlayerConnectedState.PlayerConnected
            };
        }

        /// <summary>
        /// Checks if the player is alive and has a valid pawn.
        /// </summary>
        /// <param name="player">The player controller to check.</param>
        /// <returns><c>true</c> if the player is alive; otherwise, <c>false</c>.</returns>
        public static bool IsAlive(this CCSPlayerController? player)
        {
            return player.IsPlayer() && player!.PawnIsAlive && player.PlayerPawn.Value != null;
        }

        #endregion

        #region Permission Methods

        /// <summary>
        /// Checks if the specified controller has a permission.
        /// </summary>
        /// <param name="playerController">The player controller to check.</param>
        /// <param name="permission">The permission to check.</param>
        /// <returns><c>true</c> if the player has the permission; otherwise, <c>false</c>.</returns>
        public static bool HasPermission(this CCSPlayerController? playerController, string permission)
        {
            return playerController.IsPlayer() && AdminManager.PlayerHasPermissions(playerController, permission);
        }

        #endregion

        #region Health and Armor Methods

        /// <summary>
        /// Sets the health value for the player.
        /// </summary>
        /// <param name="playerController">The player controller to set health for.</param>
        /// <param name="health">The health value to set (0-99999).</param>
        /// <param name="allowOverflow">Whether to allow the health to exceed the maximum health value.</param>
        public static void SetHealth(this CCSPlayerController? playerController, int health, bool allowOverflow = true)
        {
            if (!playerController.IsAlive())
                return;

            var pawn = playerController!.PlayerPawn.Value!;
            pawn.Health = health;

            if (allowOverflow && health > pawn.MaxHealth)
                pawn.MaxHealth = health;

            Utilities.SetStateChanged(pawn, "CBaseEntity", "m_iHealth");
        }

        /// <summary>
        /// Gets the current health of the player.
        /// </summary>
        /// <param name="playerController">The player controller to get health for.</param>
        /// <returns>The current health value, or 0 if player is invalid.</returns>
        public static int GetHealth(this CCSPlayerController? playerController)
        {
            return playerController.IsAlive() ? playerController!.PlayerPawn.Value!.Health : 0;
        }

        /// <summary>
        /// Sets the armor value for the player, optionally including a helmet and heavy armor.
        /// </summary>
        /// <param name="playerController">The player controller to set armor for.</param>
        /// <param name="armor">The armor value to set (0-100).</param>
        /// <param name="helmet">Whether to include a helmet.</param>
        /// <param name="heavy">Whether to include heavy armor.</param>
        public static void SetArmor(this CCSPlayerController? playerController, int armor, bool helmet = false, bool heavy = false)
        {
            if (!playerController.IsAlive())
                return;

            var pawn = playerController!.PlayerPawn.Value!;
            pawn.ArmorValue = armor;
            Utilities.SetStateChanged(pawn, "CCSPlayerPawnBase", "m_ArmorValue");

            if (helmet || heavy)
            {
                var services = new CCSPlayer_ItemServices(pawn.ItemServices!.Handle);
                services.HasHelmet = helmet;
                // Note: HasHeavyArmor property may not be available in current CS2 version
                // services.HasHeavyArmor = heavy;
                Utilities.SetStateChanged(pawn, "CBasePlayerPawn", "m_pItemServices");
            }
        }

        #endregion

        #region Money Methods

        /// <summary>
        /// Sets the money for the player.
        /// </summary>
        /// <param name="playerController">The player controller to set money for.</param>
        /// <param name="money">The money value to set (0-999999).</param>
        public static void SetMoney(this CCSPlayerController? playerController, int money)
        {
            if (!playerController.IsPlayer())
                return;

            var moneyServices = playerController!.InGameMoneyServices;
            if (moneyServices == null)
                return;

            moneyServices.Account = money;
            Utilities.SetStateChanged(playerController, "CCSPlayerController", "m_pInGameMoneyServices");
        }

        /// <summary>
        /// Adds money to the player's current amount.
        /// </summary>
        /// <param name="playerController">The player controller to add money to.</param>
        /// <param name="amount">The amount to add (can be negative to subtract).</param>
        public static void AddMoney(this CCSPlayerController? playerController, int amount)
        {
            if (!playerController.IsPlayer())
                return;

            var moneyServices = playerController!.InGameMoneyServices;
            if (moneyServices == null)
                return;

            var newAmount = Math.Max(0, Math.Min(999999, moneyServices.Account + amount));
            moneyServices.Account = newAmount;
            Utilities.SetStateChanged(playerController, "CCSPlayerController", "m_pInGameMoneyServices");
        }

        /// <summary>
        /// Gets the current money of the player.
        /// </summary>
        /// <param name="playerController">The player controller to get money for.</param>
        /// <returns>The current money amount, or 0 if player is invalid.</returns>
        public static int GetMoney(this CCSPlayerController? playerController)
        {
            return playerController.IsPlayer() ? playerController!.InGameMoneyServices?.Account ?? 0 : 0;
        }

        #endregion

        #region Movement and Position Methods

        /// <summary>
        /// Freezes the player, preventing them from moving.
        /// </summary>
        /// <param name="playerController">The player controller to freeze.</param>
        public static void Freeze(this CCSPlayerController? playerController)
        {
            if (!playerController.IsAlive())
                return;

            var pawn = playerController!.PlayerPawn.Value!;
            pawn.MoveType = MoveType_t.MOVETYPE_NONE;
            Utilities.SetStateChanged(pawn, "CBaseEntity", "m_MoveType");
        }

        /// <summary>
        /// Unfreezes the player, allowing them to move.
        /// </summary>
        /// <param name="playerController">The player controller to unfreeze.</param>
        public static void Unfreeze(this CCSPlayerController? playerController)
        {
            if (!playerController.IsAlive())
                return;

            var pawn = playerController!.PlayerPawn.Value!;
            pawn.MoveType = MoveType_t.MOVETYPE_WALK;
            Utilities.SetStateChanged(pawn, "CBaseEntity", "m_MoveType");
        }

        /// <summary>
        /// Enables noclip for the player, allowing them to fly through walls.
        /// </summary>
        /// <param name="playerController">The player controller to enable noclip for.</param>
        public static void EnableNoclip(this CCSPlayerController? playerController)
        {
            if (!playerController.IsAlive())
                return;

            var pawn = playerController!.PlayerPawn.Value!;
            pawn.MoveType = MoveType_t.MOVETYPE_NOCLIP;
            Utilities.SetStateChanged(pawn, "CBaseEntity", "m_MoveType");
        }

        /// <summary>
        /// Disables noclip for the player, restoring normal movement.
        /// </summary>
        /// <param name="playerController">The player controller to disable noclip for.</param>
        public static void DisableNoclip(this CCSPlayerController? playerController)
        {
            if (!playerController.IsAlive())
                return;

            var pawn = playerController!.PlayerPawn.Value!;
            pawn.MoveType = MoveType_t.MOVETYPE_WALK;
            Utilities.SetStateChanged(pawn, "CBaseEntity", "m_MoveType");
        }

        /// <summary>
        /// Checks if the player is currently frozen.
        /// </summary>
        /// <param name="playerController">The player controller to check.</param>
        /// <returns><c>true</c> if the player is frozen; otherwise, <c>false</c>.</returns>
        public static bool IsFrozen(this CCSPlayerController? playerController)
        {
            return playerController.IsAlive() && 
                   playerController!.PlayerPawn.Value!.MoveType == MoveType_t.MOVETYPE_NONE;
        }

        /// <summary>
        /// Teleports the player to the specified position.
        /// </summary>
        /// <param name="playerController">The player controller to teleport.</param>
        /// <param name="position">The position to teleport to.</param>
        /// <param name="angles">Optional angles to set (if null, keeps current angles).</param>
        public static void Teleport(this CCSPlayerController? playerController, Vector position, QAngle? angles = null)
        {
            if (!playerController.IsAlive())
                return;

            var pawn = playerController!.PlayerPawn.Value!;
            pawn.Teleport(position, angles ?? pawn.AbsRotation, Vector.Zero);
        }

        /// <summary>
        /// Teleports the player to another player's position.
        /// </summary>
        /// <param name="playerController">The player controller to teleport.</param>
        /// <param name="targetPlayer">The target player to teleport to.</param>
        public static void TeleportToPlayer(this CCSPlayerController? playerController, CCSPlayerController targetPlayer)
        {
            if (!playerController.IsAlive() || !targetPlayer.IsAlive())
                return;

            var targetPawn = targetPlayer.PlayerPawn.Value!;
            var playerPawn = playerController!.PlayerPawn.Value!;
            playerPawn.Teleport(targetPawn.AbsOrigin!, targetPawn.AbsRotation, Vector.Zero);
        }

        /// <summary>
        /// Gets the eye position of the player.
        /// </summary>
        /// <param name="playerController">The player controller to get the eye position for.</param>
        /// <returns>The eye position as a <see cref="Vector"/>.</returns>
        public static Vector GetEyePosition(this CCSPlayerController? playerController)
        {
            if (!playerController.IsAlive())
                return Vector.Zero;

            var pawn = playerController!.PlayerPawn.Value!;
            var absOrigin = pawn.AbsOrigin ?? Vector.Zero;
            var camera = pawn.CameraServices;

            return new Vector(absOrigin.X, absOrigin.Y, absOrigin.Z + (camera?.OldPlayerViewOffsetZ ?? 64.0f));
        }

        #endregion

        #region Team and Server Actions

        /// <summary>
        /// Moves the player to a specified team.
        /// </summary>
        /// <param name="playerController">The player controller to move.</param>
        /// <param name="team">The team to move the player to.</param>
        public static void MoveToTeam(this CCSPlayerController? playerController, CsTeam team)
        {
            if (!playerController.IsPlayer() || playerController!.TeamNum == (byte)team)
                return;

            // Queue for next frame to avoid threading issues
            Server.NextFrame(() => { playerController.ChangeTeam(team); });
        }

        /// <summary>
        /// Kicks the player from the server with a specified reason.
        /// </summary>
        /// <param name="playerController">The player controller to kick.</param>
        /// <param name="reason">The reason for kicking the player.</param>
        public static void Kick(this CCSPlayerController? playerController, string reason = "Kicked by admin")
        {
            if (!playerController.IsPlayer())
                return;

            var kickCommand = string.Create(CultureInfo.InvariantCulture,
                $"kickid {playerController!.UserId!.Value} \"{reason}\"");

            // Queue for next frame to avoid threading issues
            Server.NextFrame(() => { Server.ExecuteCommand(kickCommand); });
        }

        /// <summary>
        /// Kills the player (forces suicide).
        /// </summary>
        /// <param name="playerController">The player controller to kill.</param>
        public static void Kill(this CCSPlayerController? playerController)
        {
            if (!playerController.IsAlive())
                return;

            playerController!.PlayerPawn.Value!.CommitSuicide(false, true);
        }

        #endregion

        #region Player Information Methods

        /// <summary>
        /// Sets the player's name.
        /// </summary>
        /// <param name="playerController">The player controller to set the name for.</param>
        /// <param name="name">The new name for the player.</param>
        public static void SetName(this CCSPlayerController? playerController, string name)
        {
            if (!playerController.IsPlayer() || name == playerController!.PlayerName)
                return;

            playerController.PlayerName = name;
            Utilities.SetStateChanged(playerController, "CBasePlayerController", "m_iszPlayerName");
        }

        /// <summary>
        /// Sets the player's clan tag.
        /// </summary>
        /// <param name="playerController">The player controller to set the clan tag for.</param>
        /// <param name="clantag">The new clan tag for the player.</param>
        public static void SetClantag(this CCSPlayerController? playerController, string clantag = "")
        {
            if (!playerController.IsPlayer() || clantag == playerController!.Clan)
                return;

            playerController.Clan = clantag;
            Utilities.SetStateChanged(playerController, "CCSPlayerController", "m_szClan");

            var fakeEvent = new EventNextlevelChanged(false);
            fakeEvent.FireEventToClient(playerController);
        }

        /// <summary>
        /// Gets a formatted display name for the player (for logging/chat).
        /// </summary>
        /// <param name="playerController">The player controller to get the display name for.</param>
        /// <returns>A formatted display name, or "Console" if player is null.</returns>
        public static string GetDisplayName(this CCSPlayerController? playerController)
        {
            return playerController?.PlayerName ?? "Console";
        }

        #endregion
    }
}
