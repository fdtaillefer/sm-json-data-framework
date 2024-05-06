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
        private Predicate<Strat> Predicate { get; }

        /// <summary>
        /// Keys to obtain from a dictionary, as a potentially faster way to get values out of a dictionary.
        /// The predicate will be used if applying to something that isn't a dictionary.
        /// </summary>
        private ISet<String> Keys { get; set; }

        public string Description { get; }

        public StratFilter(Predicate<Strat> predicate, string description)
        {
            Predicate = predicate;
            Description = description;
        }

        public StratFilter(ISet<String> keys, string description)
        {
            Description = description;
            Keys = new HashSet<string>(keys);
            Predicate = strat => Keys.Contains(strat.Name);
        }

        /// <summary>
        /// Applies this StratFilter to an enmeration of Strats.
        /// </summary>
        /// <param name="strats"></param>
        /// <returns></returns>
        public IEnumerable<Strat> Apply(IEnumerable<Strat> strats) {
            return strats.Where(Predicate.Invoke);
        }

        /// <summary>
        /// Applies this StratFilter to an enmeration of stratName-strat KeyValuePairs.
        /// </summary>
        /// <param name="strats"></param>
        /// <returns></returns>
        public IEnumerable<KeyValuePair<string, Strat>> Apply(IEnumerable<KeyValuePair<string, Strat>> strats)
        {
            if(strats is IDictionary<string, Strat> dictionary && Keys != null)
            {
                return Keys.Select(key => dictionary[key]).ToDictionary(strat => strat.Name);
            }
            return strats.Where(kvp => Predicate.Invoke(kvp.Value));
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
            string description = $"Name is '{name}'";
            return new StratFilter(new HashSet<string> { name }, description);
        }

        /// <summary>
        /// Creates and returns a new StratFilter that will match Strats which destroy an obstacle with the provided ID (case-insensitive)
        /// </summary>
        /// <param name="obstacleId">The ID of the obstacle that must be broken by matching strats</param>
        /// <returns>The StratFilter</returns>
        public static StratFilter BreaksObstacle(string obstacleId)
        {
            Predicate<Strat> predicate = strat => strat.Obstacles.Values.Any(
                obstacle => (obstacle?.ObstacleId?.Equals(obstacleId, StringComparison.InvariantCultureIgnoreCase) is true)
                || (obstacle?.AdditionalObstacleIds.Any(additionalId => additionalId?.Equals(obstacleId, StringComparison.InvariantCultureIgnoreCase) is true) is true)
                );
            string description = $"Breaks obstacle '{obstacleId}'";

            return new StratFilter(predicate, description);
        }
    }
}
