using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;
using CS2Utilities.Config;

namespace CS2Utilities.Utils
{
    /// <summary>
    /// Utility class for consistent chat messaging with color themes
    /// </summary>
    public static class ChatUtils
    {
        private static CS2UtilitiesConfig? _config;
        
        /// <summary>
        /// Initialize ChatUtils with configuration
        /// </summary>
        /// <param name="config">Plugin configuration</param>
        public static void Initialize(CS2UtilitiesConfig config)
        {
            _config = config;
        }

        /// <summary>
        /// Get the configured chat prefix with color
        /// </summary>
        /// <returns>Formatted chat prefix</returns>
        public static string GetPrefix()
        {
            var prefix = _config?.ChatPrefix ?? "[CS2Utils]";
            var prefixColor = GetChatColor(_config?.ChatColors.PrefixColor ?? "DarkBlue");
            return $"{prefixColor}{prefix}";
        }

        /// <summary>
        /// Send a message to a specific player with the plugin prefix
        /// </summary>
        /// <param name="player">Target player</param>
        /// <param name="message">Message to send</param>
        /// <param name="messageType">Type of message for color coding</param>
        public static void PrintToPlayer(CCSPlayerController player, string message, MessageType messageType = MessageType.Default)
        {
            if (!player.IsValid || player.IsBot)
                return;

            var formattedMessage = FormatMessage(message, messageType);
            player.PrintToChat(formattedMessage);
        }

        /// <summary>
        /// Send a message to all players with the plugin prefix
        /// </summary>
        /// <param name="message">Message to send</param>
        /// <param name="messageType">Type of message for color coding</param>
        public static void PrintToAll(string message, MessageType messageType = MessageType.Default)
        {
            var formattedMessage = FormatMessage(message, messageType);
            Server.PrintToChatAll(formattedMessage);
        }

        /// <summary>
        /// Send a message to all players on a specific team
        /// </summary>
        /// <param name="team">Target team</param>
        /// <param name="message">Message to send</param>
        /// <param name="messageType">Type of message for color coding</param>
        public static void PrintToTeam(CsTeam team, string message, MessageType messageType = MessageType.Default)
        {
            var formattedMessage = FormatMessage(message, messageType);
            
            var players = Utilities.GetPlayers()
                .Where(p => p.IsValid && !p.IsBot && p.Team == team && 
                           p.Connected == PlayerConnectedState.PlayerConnected);
            
            foreach (var player in players)
            {
                player.PrintToChat(formattedMessage);
            }
        }

        /// <summary>
        /// Format a message with the appropriate colors and prefix
        /// </summary>
        /// <param name="message">Raw message</param>
        /// <param name="messageType">Type of message for color coding</param>
        /// <returns>Formatted message with colors</returns>
        public static string FormatMessage(string message, MessageType messageType = MessageType.Default)
        {
            var prefix = GetPrefix();
            
            return $" {ChatColors.DarkBlue}{prefix} | {ChatColors.Grey}{message}";
        }

        /// <summary>
        /// Format a success message
        /// </summary>
        /// <param name="message">Message content</param>
        /// <returns>Formatted success message</returns>
        public static string FormatSuccess(string message)
        {
            return FormatMessage(message, MessageType.Success);
        }

        /// <summary>
        /// Format an error message
        /// </summary>
        /// <param name="message">Message content</param>
        /// <returns>Formatted error message</returns>
        public static string FormatError(string message)
        {
            return FormatMessage(message, MessageType.Error);
        }

        /// <summary>
        /// Format a warning message
        /// </summary>
        /// <param name="message">Message content</param>
        /// <returns>Formatted warning message</returns>
        public static string FormatWarning(string message)
        {
            return FormatMessage(message, MessageType.Warning);
        }

        /// <summary>
        /// Format an info message
        /// </summary>
        /// <param name="message">Message content</param>
        /// <returns>Formatted info message</returns>
        public static string FormatInfo(string message)
        {
            return FormatMessage(message, MessageType.Info);
        }

        /// <summary>
        /// Format a highlight message
        /// </summary>
        /// <param name="message">Message content</param>
        /// <returns>Formatted highlight message</returns>
        public static string FormatHighlight(string message)
        {
            return FormatMessage(message, MessageType.Highlight);
        }

        /// <summary>
        /// Get the appropriate message color based on message type
        /// </summary>
        /// <param name="messageType">Type of message</param>
        /// <returns>Chat color for the message</returns>
        private static char GetMessageColor(MessageType messageType)
        {
            return messageType switch
            {
                MessageType.Success => GetChatColor(_config?.ChatColors.EnabledColor ?? "Green"),
                MessageType.Error => GetChatColor(_config?.ChatColors.DisabledColor ?? "Red"),
                MessageType.Warning => GetChatColor(_config?.ChatColors.WarningColor ?? "Orange"),
                MessageType.Info => GetChatColor(_config?.ChatColors.InfoColor ?? "LightBlue"),
                MessageType.Highlight => GetChatColor(_config?.ChatColors.HighlightColor ?? "Yellow"),
                MessageType.Default => GetChatColor(_config?.ChatColors.DefaultColor ?? "Grey"),
                _ => ChatColors.Default
            };
        }

        /// <summary>
        /// Convert color name to ChatColors character
        /// </summary>
        /// <param name="colorName">Name of the color</param>
        /// <returns>ChatColors character</returns>
        private static char GetChatColor(string colorName)
        {
            return colorName.ToLower() switch
            {
                "darkblue" => ChatColors.DarkBlue,
                "grey" or "gray" => ChatColors.Grey,
                "yellow" => ChatColors.Yellow,
                "green" => ChatColors.Green,
                "red" => ChatColors.Red,
                "orange" => ChatColors.Orange,
                "lightblue" => ChatColors.LightBlue,
                "blue" => ChatColors.Blue,
                "purple" => ChatColors.Purple,
                "lime" => ChatColors.Lime,
                "lightred" => ChatColors.LightRed,
                "lightpurple" => ChatColors.LightPurple,
                "lightyellow" => ChatColors.LightYellow,
                "darkred" => ChatColors.DarkRed,
                "white" => ChatColors.White,
                _ => ChatColors.Default
            };
        }

        /// <summary>
        /// Send a formatted command usage message
        /// </summary>
        /// <param name="player">Target player</param>
        /// <param name="commandName">Name of the command</param>
        /// <param name="usage">Usage syntax</param>
        public static void PrintUsage(CCSPlayerController player, string commandName, string usage)
        {
            var message = $"Usage: {ChatColors.Yellow}!{commandName} {usage}";
            PrintToPlayer(player, message, MessageType.Info);
        }

        /// <summary>
        /// Send a permission denied message
        /// </summary>
        /// <param name="player">Target player</param>
        /// <param name="commandName">Name of the command</param>
        /// <param name="requiredPermission">Required permission</param>
        public static void PrintPermissionDenied(CCSPlayerController player, string commandName, string requiredPermission)
        {
            var message = $"Access denied for command {ChatColors.Yellow}!{commandName}{ChatColors.Default}. Required permission: {ChatColors.Yellow}{requiredPermission}";
            PrintToPlayer(player, message, MessageType.Error);
        }

        /// <summary>
        /// Send a command disabled message
        /// </summary>
        /// <param name="player">Target player</param>
        /// <param name="commandName">Name of the command</param>
        public static void PrintCommandDisabled(CCSPlayerController player, string commandName)
        {
            var message = $"Command {ChatColors.Yellow}!{commandName}{ChatColors.Default} is currently disabled.";
            PrintToPlayer(player, message, MessageType.Error);
        }
    }

    /// <summary>
    /// Types of messages for color coding
    /// </summary>
    public enum MessageType
    {
        Default,
        Success,
        Error,
        Warning,
        Info,
        Highlight
    }
}
