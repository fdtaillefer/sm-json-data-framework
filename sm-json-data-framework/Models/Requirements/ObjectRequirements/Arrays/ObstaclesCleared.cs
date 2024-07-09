using sm_json_data_framework.Models.Rooms;
using sm_json_data_framework.Options;
using sm_json_data_framework.Rules;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace sm_json_data_framework.Models.Requirements.ObjectRequirements.Arrays
{
    public class ObstaclesCleared : AbstractArrayLogicalElement<string, string, UnfinalizedObstaclesCleared, ObstaclesCleared>
    {
        public ObstaclesCleared(UnfinalizedObstaclesCleared sourceElement, Action<ObstaclesCleared> mappingsInsertionCallback, ModelFinalizationMappings mappings)
            : base(sourceElement, mappingsInsertionCallback, mappings)
        {
            Obstacles = sourceElement.Obstacles.Values.Select(unfinalizedObstacle => unfinalizedObstacle.Finalize(mappings)).ToDictionary(obstacle => obstacle.Id);
        }

        public IReadOnlyDictionary<string, RoomObstacle> Obstacles { get; }

        protected override string ConvertItem(string sourceItem, ModelFinalizationMappings mappings)
        {
            return sourceItem;
        }

        protected override ExecutionResult ExecutePossible(SuperMetroidModel model, InGameStates.ReadOnlyInGameState inGameState, int times = 1, int previousRoomCount = 0)
        {
            bool fulfilled = true;
            // Iterate over all obstacles to ensure they are destroyed
            foreach (RoomObstacle roomObstacle in Obstacles.Values)
            {
                // If the obstacle isn't destroyed, execution fails
                if (!inGameState.InRoomState.DestroyedObstacleIds.Contains(roomObstacle.Id))
                {
                    fulfilled = false;
                }
                // If the obstacle isn't the expected one, then presumably we are in the wrong room so execution fails
                if (inGameState.CurrentRoom.Obstacles[roomObstacle.Id] != roomObstacle)
                {
                    fulfilled = false;
                }
            }

            if (fulfilled)
            {
                // Clone the InGameState to fulfill method contract
                return new ExecutionResult(inGameState.Clone());
            }
            else
            {
                // If this is not fulfilled, then execution fails
                return null;
            }
        }

        protected override void PropagateLogicalOptions(ReadOnlyLogicalOptions logicalOptions, SuperMetroidRules rules)
        {
            foreach (RoomObstacle obstacle in Obstacles.Values)
            {
                obstacle.ApplyLogicalOptions(logicalOptions, rules);
            }
        }

        protected override bool CalculateLogicallyNever(SuperMetroidRules rules)
        {
            // If any of the obstacles is impossible to clear, this is impossible to fulfill
            return Obstacles.Values.Any(roomObstacle => roomObstacle.LogicallyIndestructible);
        }

        protected override bool CalculateLogicallyAlways(SuperMetroidRules rules)
        {
            // We can't really know where/how the obstacle can be destroyed, besides the base common requirements
            return false;
        }

        protected override bool CalculateLogicallyFree(SuperMetroidRules rules)
        {
            // We can't really know where/how the obstacle can be destroyed, besides the base common requirements
            return false;
        }
    }

    public class UnfinalizedObstaclesCleared : AbstractUnfinalizedArrayLogicalElement<string, string, UnfinalizedObstaclesCleared, ObstaclesCleared>
    {
        public UnfinalizedObstaclesCleared(IList<string> items) : base(items)
        {

        }

        public IDictionary<string, UnfinalizedRoomObstacle> Obstacles { get; set; } = new Dictionary<string, UnfinalizedRoomObstacle>();

        protected override ObstaclesCleared CreateFinalizedElement(
            UnfinalizedObstaclesCleared sourceElement, Action<ObstaclesCleared> mappingsInsertionCallback, ModelFinalizationMappings mappings)
        {
            return new ObstaclesCleared(sourceElement, mappingsInsertionCallback, mappings);
        }

        public override IEnumerable<string> InitializeReferencedLogicalElementProperties(UnfinalizedSuperMetroidModel model, UnfinalizedRoom room)
        {
            if (room == null)
            {
                throw new ArgumentException("An ObstaclesCleared logical element should always be in a room");
            }

            List<string> unhandledValues = new();
            foreach(string obstacleId in Value)
            {
                if (room.Obstacles.TryGetValue(obstacleId, out UnfinalizedRoomObstacle obstacle))
                {
                    Obstacles.Add(obstacleId, obstacle);
                }
                else
                {
                    unhandledValues.Add($"ObstacleId {obstacleId} (referenced within an ObstaclesCleared logical element) not found int room {room.Name}");
                }
            }

            return unhandledValues;
        }
    }
}
