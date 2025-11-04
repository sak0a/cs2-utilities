using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;
using CS2Utilities.Models;

namespace CS2Utilities.Core
{
    /// <summary>
    /// Resolves player targets from command arguments supporting individual, team, and all player targeting
    /// </summary>
    public class PlayerTargetResolver
    {
        /// <summary>
        /// Resolve targets from a target string
        /// </summary>
        /// <param name="target">Target string (player name, team, "all", etc.)</param>
        /// <param name="caller">The player who issued the command (for "self" targeting)</param>
        /// <returns>PlayerTarget with resolved players</returns>
        public PlayerTarget ResolveTargets(string target, CCSPlayerController? caller = null)
        {
            var result = new PlayerTarget
            {
                OriginalTarget = target
            };

            if (string.IsNullOrWhiteSpace(target))
            {
                // Default to self if no target specified and caller exists
                if (caller != null && caller.IsValid && !caller.IsBot)
                {
                    result.Type = TargetType.Self;
                    result.Players.Add(caller);
                    return result;
                }
                
                result.ErrorMessage = "No target specified and no valid caller";
                return result;
            }

            var normalizedTarget = target.ToLower().Trim();

            // Handle special targets
            switch (normalizedTarget)
            {
                case "@all":
                    return ResolveAllPlayers(result);
                
                case "@ct":
                case "@counterterrorist":
                case "@counterterrorists":
                    return ResolveTeamPlayers(result, CsTeam.CounterTerrorist);
                
                case "@t":
                case "@terrorist":
                case "@terrorists":
                    return ResolveTeamPlayers(result, CsTeam.Terrorist);
                
                case "@spec":
                case "@spectator":
                case "@spectators":
                    return ResolveTeamPlayers(result, CsTeam.Spectator);
                
                case "@self":
                case "@me":
                    return ResolveSelfTarget(result, caller);
                
                default:
                    return ResolveIndividualPlayer(result, target);
            }
        }

        /// <summary>
        /// Resolve all players on the server
        /// </summary>
        private PlayerTarget ResolveAllPlayers(PlayerTarget result)
        {
            result.Type = TargetType.All;
            
            var players = Utilities.GetPlayers()
                .Where(p => p.IsValid && !p.IsBot && p.Connected == PlayerConnectedState.PlayerConnected)
                .ToList();
            
            result.Players.AddRange(players);
            
            if (result.Players.Count == 0)
            {
                result.ErrorMessage = "No valid players found on the server";
            }
            
            return result;
        }

        /// <summary>
        /// Resolve players on a specific team
        /// </summary>
        private PlayerTarget ResolveTeamPlayers(PlayerTarget result, CsTeam team)
        {
            result.Type = TargetType.Team;
            result.Team = team;
            
            var players = Utilities.GetPlayers()
                .Where(p => p.IsValid && !p.IsBot && 
                           p.Connected == PlayerConnectedState.PlayerConnected && 
                           p.Team == team)
                .ToList();
            
            result.Players.AddRange(players);
            
            if (result.Players.Count == 0)
            {
                result.ErrorMessage = $"No valid players found on team {team}";
            }
            
            return result;
        }

        /// <summary>
        /// Resolve self target (the command caller)
        /// </summary>
        private PlayerTarget ResolveSelfTarget(PlayerTarget result, CCSPlayerController? caller)
        {
            result.Type = TargetType.Self;
            
            if (caller != null && caller.IsValid && !caller.IsBot && 
                caller.Connected == PlayerConnectedState.PlayerConnected)
            {
                result.Players.Add(caller);
            }
            else
            {
                result.ErrorMessage = "Invalid caller or caller is not a valid player";
            }
            
            return result;
        }

        /// <summary>
        /// Resolve an individual player by name or user ID
        /// </summary>
        private PlayerTarget ResolveIndividualPlayer(PlayerTarget result, string target)
        {
            result.Type = TargetType.Individual;
            
            var players = Utilities.GetPlayers()
                .Where(p => p.IsValid && !p.IsBot && p.Connected == PlayerConnectedState.PlayerConnected)
                .ToList();
            
            // Try to find by exact name match first
            var exactMatch = players.FirstOrDefault(p => 
                string.Equals(p.PlayerName, target, StringComparison.OrdinalIgnoreCase));
            
            if (exactMatch != null)
            {
                result.Players.Add(exactMatch);
                return result;
            }
            
            // Try to find by partial name match
            var partialMatches = players.Where(p => 
                p.PlayerName.Contains(target, StringComparison.OrdinalIgnoreCase)).ToList();
            
            if (partialMatches.Count == 1)
            {
                result.Players.Add(partialMatches[0]);
                return result;
            }
            
            if (partialMatches.Count > 1)
            {
                result.ErrorMessage = $"Multiple players found matching '{target}': {string.Join(", ", partialMatches.Select(p => p.PlayerName))}";
                return result;
            }
            
            // Try to parse as user ID
            if (int.TryParse(target, out var userId))
            {
                var playerByUserId = players.FirstOrDefault(p => p.UserId == userId);
                if (playerByUserId != null)
                {
                    result.Players.Add(playerByUserId);
                    return result;
                }
            }
            
            result.ErrorMessage = $"No player found matching '{target}'";
            return result;
        }

        /// <summary>
        /// Check if a target string is valid (doesn't resolve, just validates format)
        /// </summary>
        /// <param name="target">Target string to validate</param>
        /// <returns>True if the target format is valid</returns>
        public bool IsValidTarget(string target)
        {
            if (string.IsNullOrWhiteSpace(target))
                return false;
            
            var normalizedTarget = target.ToLower().Trim();
            
            // Check for known special targets
            var specialTargets = new[] { "@all", "@ct", "@counterterrorist", "@counterterrorists", 
                                       "@t", "@terrorist", "@terrorists", "spec", "spectator", 
                                       "@spectators", "@self", "@me" };
            
            if (specialTargets.Contains(normalizedTarget))
                return true;
            
            // Check if it could be a player name (basic validation)
            if (target.Length >= 1 && target.Length <= 32)
                return true;
            
            // Check if it could be a user ID
            if (int.TryParse(target, out var userId) && userId > 0)
                return true;
            
            return false;
        }

        /// <summary>
        /// Parse a team name string to CsTeam enum
        /// </summary>
        /// <param name="teamName">Team name string</param>
        /// <returns>CsTeam enum value or null if invalid</returns>
        public CsTeam? ParseTeam(string teamName)
        {
            if (string.IsNullOrWhiteSpace(teamName))
                return null;
            
            var normalizedTeam = teamName.ToLower().Trim();
            
            return normalizedTeam switch
            {
                "@ct" or "@counterterrorist" or "@counterterrorists" => CsTeam.CounterTerrorist,
                "@t" or "@terrorist" or "@terrorists" => CsTeam.Terrorist,
                "@spec" or "@spectator" or "@spectators" => CsTeam.Spectator,
                _ => null
            };
        }

        /// <summary>
        /// Get a human-readable description of the target
        /// </summary>
        /// <param name="target">PlayerTarget to describe</param>
        /// <returns>Human-readable description</returns>
        public string GetTargetDescription(PlayerTarget target)
        {
            return target.Type switch
            {
                TargetType.All => $"all players ({target.Count} players)",
                TargetType.Team => $"team {target.Team} ({target.Count} players)",
                TargetType.Self => "yourself",
                TargetType.Individual => target.Count > 0 ? target.Players[0].PlayerName : "unknown player",
                _ => "unknown target"
            };
        }
    }
}
