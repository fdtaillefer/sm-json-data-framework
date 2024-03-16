using sm_json_data_framework.Models.Rooms;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace sm_json_data_framework.Models.Navigation
{
    // A filter that can be applied when moving nodes, to limit allowedstrats to a ubset of the existing ones.
    public class StratFilter
    {
        public Predicate<Strat> Predicate { get; }

        public string Description { get; }

        public StratFilter(Predicate<Strat> predicate, string description)
        {
            Predicate = predicate;
            Description = description;
        }

        /// <summary>
        /// Creates and returns a new StratFilter that will match Strats whose name starts with the provided prefix (case-insensitive)
        /// </summary>
        /// <param name="prefix">The value that matching strats' name must start with</param>
        /// <returns>The StratFilter</returns>
        public static StratFilter NameStartsWith(string prefix)
        {
            Predicate<Strat> predicate = strat => strat.Name?.StartsWith(prefix, ignoreCase: true, CultureInfo.InvariantCulture) is true;
            string description = $"Starts with '{prefix}'";

            return new StratFilter(predicate, description);
        }

        /// <summary>
        /// Creates and returns a new StratFilter that will match Strats whose name is the provided name (case-insensitive)
        /// </summary>
        /// <param name="name">The name that matching strats must have</param>
        /// <returns>The StratFilter</returns>
        public static StratFilter NameIs(string name)
        {
            Predicate<Strat> predicate = strat => strat.Name?.Equals(name, StringComparison.InvariantCultureIgnoreCase) is true;
            string description = $"Name is '{name}'";

            return new StratFilter(predicate, description);
        }

        /// <summary>
        /// Creates and returns a new StratFilter that will match Strats which destroy an obstacle with the provided ID (case-insensitive)
        /// </summary>
        /// <param name="obstacleId">The ID of the obstacle that must be broken by matching strats</param>
        /// <returns>The StratFilter</returns>
        public static StratFilter BreaksObstacle(string obstacleId)
        {
            Predicate<Strat> predicate = strat => strat.Obstacles.Any(
                obstacle => (obstacle?.ObstacleId?.Equals(obstacleId, StringComparison.InvariantCultureIgnoreCase) is true)
                || (obstacle?.AdditionalObstacleIds.Any(additionalId => additionalId?.Equals(obstacleId, StringComparison.InvariantCultureIgnoreCase) is true) is true)
                );
            string description = $"Breaks obstacle '{obstacleId}'";

            return new StratFilter(predicate, description);
        }
    }
}
