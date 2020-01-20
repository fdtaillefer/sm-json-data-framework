using sm_json_data_framework.Models.InGameStates;
using sm_json_data_framework.Models.Rooms;
using sm_json_data_framework.Models.Rooms.Nodes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;

namespace sm_json_data_framework.Models.Requirements.ObjectRequirements.SubObjects
{
    public class ResetRoom : AbstractObjectLogicalElement
    {
        [JsonPropertyName("nodes")]
        public IEnumerable<int> NodeIds { get; set; } = Enumerable.Empty<int>();

        /// <summary>
        /// <para>Only available after a call to <see cref="InitializeReferencedLogicalElementProperties(SuperMetroidModel, Room)"/>.</para>
        /// <para>The nodes that this element's NodeIds reference. </para>
        /// </summary>
        [JsonIgnore]
        public IEnumerable<RoomNode> Nodes { get; set; }

        [JsonPropertyName("nodesToAvoid")]
        public IEnumerable<int> NodeIdsToAvoid { get; set; } = Enumerable.Empty<int>();

        /// <summary>
        /// <para>Only available after a call to <see cref="InitializeReferencedLogicalElementProperties(SuperMetroidModel, Room)"/>.</para>
        /// <para>The nodes that this element's NodeIdToAvoids reference. </para>
        /// </summary>
        [JsonIgnore]
        public IEnumerable<RoomNode> NodesToAvoid { get; set; }

        [JsonPropertyName("obstaclesToAvoid")]
        public IEnumerable<string> ObstaclesIdsToAvoid { get; set; } = Enumerable.Empty<string>();

        /// <summary>
        /// <para>Only available after a call to <see cref="InitializeReferencedLogicalElementProperties(SuperMetroidModel, Room)"/>.</para>
        /// <para>The obstacles that this element's ObstaclesIdsToAvoid reference. </para>
        /// </summary>
        [JsonIgnore]
        public IEnumerable<RoomObstacle> ObstaclesToAvoid { get; set; }

        public bool MustStayPut { get; set; } = false;

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

        public override bool IsFulfilled(InGameState inGameState, bool usePreviousRoom = false)
        {
            // STITCHME Do something
            throw new NotImplementedException();
        }
    }
}
