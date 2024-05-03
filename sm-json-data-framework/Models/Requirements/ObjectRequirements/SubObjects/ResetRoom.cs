using sm_json_data_framework.Models.InGameStates;
using sm_json_data_framework.Models.Rooms;
using sm_json_data_framework.Models.Rooms.Nodes;
using sm_json_data_framework.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;

namespace sm_json_data_framework.Models.Requirements.ObjectRequirements.SubObjects
{
    /// <summary>
    /// A logical element which requires Samus to have followed restricted pathing in the room since entering. Includes entering from a subset of the room's nodes.
    /// </summary>
    public class ResetRoom : AbstractObjectLogicalElement
    {
        [JsonPropertyName("nodes")]
        public IList<int> NodeIds { get; set; } = new List<int>();

        /// <summary>
        /// <para>Only available after a call to <see cref="InitializeReferencedLogicalElementProperties(SuperMetroidModel, Room)"/>.</para>
        /// <para>The nodes that this element's NodeIds reference. </para>
        /// </summary>
        [JsonIgnore]
        public IList<RoomNode> Nodes { get; set; }

        [JsonPropertyName("nodesToAvoid")]
        public ISet<int> NodeIdsToAvoid { get; set; } = new HashSet<int>();

        /// <summary>
        /// <para>Only available after a call to <see cref="InitializeReferencedLogicalElementProperties(SuperMetroidModel, Room)"/>.</para>
        /// <para>The nodes that this element's NodeIdToAvoids reference. </para>
        /// </summary>
        [JsonIgnore]
        public IList<RoomNode> NodesToAvoid { get; set; }

        [JsonPropertyName("obstaclesToAvoid")]
        public ISet<string> ObstaclesIdsToAvoid { get; set; } = new HashSet<string>();

        /// <summary>
        /// <para>Only available after a call to <see cref="InitializeReferencedLogicalElementProperties(SuperMetroidModel, Room)"/>.</para>
        /// <para>The obstacles that this element's ObstaclesIdsToAvoid reference. </para>
        /// </summary>
        [JsonIgnore]
        public IList<RoomObstacle> ObstaclesToAvoid { get; set; }

        public bool MustStayPut { get; set; } = false;

        protected override bool ApplyLogicalOptionsEffects(ReadOnlyLogicalOptions logicalOptions)
        {
            // Nothing in logical options can alter this
            return false;
        }

        public override bool IsNever()
        {
            return false;
        }

        public override IEnumerable<string> InitializeReferencedLogicalElementProperties(SuperMetroidModel model, Room room)
        {
            List<string> unhandled = new List<string>();

            List<RoomNode> nodes = new List<RoomNode>();
            foreach (int nodeId in NodeIds)
            {
                if (room.Nodes.TryGetValue(nodeId, out RoomNode node))
                {
                    nodes.Add(node);
                }
                else
                {
                    unhandled.Add($"Node {nodeId} in room {room.Name}");
                }
            }
            Nodes = nodes;

            List<RoomNode> nodesToAvoid = new List<RoomNode>();
            foreach (int nodeIdToAvoid in NodeIdsToAvoid)
            {
                if (room.Nodes.TryGetValue(nodeIdToAvoid, out RoomNode nodeToAvoid))
                {
                    nodesToAvoid.Add(nodeToAvoid);
                }
                else
                {
                    unhandled.Add($"Node {nodeIdToAvoid} in room {room.Name}");
                }
            }
            NodesToAvoid = nodesToAvoid;

            List<RoomObstacle> obstaclesToAvoid = new List<RoomObstacle>();
            foreach (string obstacleIdToAvoid in ObstaclesIdsToAvoid)
            {
                if (room.Obstacles.TryGetValue(obstacleIdToAvoid, out RoomObstacle obstacleToAvoid))
                {
                    obstaclesToAvoid.Add(obstacleToAvoid);
                }
                else
                {
                    unhandled.Add($"Obstacle {obstacleIdToAvoid} in room {room.Name}");
                }
            }
            ObstaclesToAvoid = obstaclesToAvoid;

            return unhandled.Distinct();
        }

        protected override ExecutionResult ExecuteUseful(SuperMetroidModel model, ReadOnlyInGameState inGameState, int times = 1, int previousRoomCount = 0)
        {
            IReadOnlyList<int> visitedNodeIds = inGameState.GetVisitedNodeIds(previousRoomCount);

            // If the node at which we entered is not allowed, this is not fulfilled.
            if (!NodeIds.Contains(visitedNodeIds[0]))
            {
                return null;
            }

            // If we have visited a node to avoid, this is not fulfilled.
            if (NodeIdsToAvoid.Intersect(visitedNodeIds).Any())
            {
                return null;
            }

            // If we were supposed to stay put but have visited more than the starting node, this is not fulfilled
            if (MustStayPut && visitedNodeIds.Count > 1)
            {
                return null;
            }

            // If we have destroyed an obstacle that needed to be preserved, this is not fulfilled
            if (ObstaclesIdsToAvoid.Intersect(inGameState.GetDestroyedObstacleIds(previousRoomCount)).Any())
            {
                return null;
            }

            // We've avoided all pitfalls. This ResetRoom is fulfilled. Clone the InGameState to fulfill method contract
            return new ExecutionResult(inGameState.Clone());
        }
    }
}
