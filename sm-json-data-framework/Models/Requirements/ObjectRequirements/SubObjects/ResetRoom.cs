using sm_json_data_framework.Models.InGameStates;
using sm_json_data_framework.Models.Requirements.ObjectRequirements.SubRequirements;
using sm_json_data_framework.Models.Rooms;
using sm_json_data_framework.Models.Rooms.Nodes;
using sm_json_data_framework.Options;
using sm_json_data_framework.Utils;
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
    public class ResetRoom : AbstractObjectLogicalElement<UnfinalizedResetRoom, ResetRoom>
    {
        public ResetRoom(UnfinalizedResetRoom sourceElement, Action<ResetRoom> mappingsInsertionCallback, ModelFinalizationMappings mappings)
            : base(sourceElement, mappingsInsertionCallback)
        {
            MustStayPut = sourceElement.MustStayPut;
            Nodes = sourceElement.Nodes.Select(node => node.Finalize(mappings)).ToDictionary(node => node.Id).AsReadOnly();
            NodesToAvoid = sourceElement.NodesToAvoid.Select(node => node.Finalize(mappings)).ToDictionary(node => node.Id).AsReadOnly();
            ObstaclesToAvoid = sourceElement.ObstaclesToAvoid.Select(obstacle => obstacle.Finalize(mappings)).ToDictionary(obstacle => obstacle.Id).AsReadOnly();
        }

        /// <summary>
        /// The nodes from which room entry allows fulfilling this ResetRoom, mapped by their in-room ID.
        /// If samus entered through a different node, this Resetroom cannot be fulfilled during the current room visit.
        /// </summary>
        public IReadOnlyDictionary<int, RoomNode> Nodes { get; }

        /// <summary>
        /// Nodes that must not have been previously visited in the current room visit in order to fulfill this ResetRoom, mapped by their in-room ID.
        /// If they have been visited, this Resetroom can no longer be fulfilled during the current room visit.
        /// </summary>
        public IReadOnlyDictionary<int, RoomNode> NodesToAvoid { get; }

        /// <summary>
        /// Osbtacles that must not have been previously destroyed in the current room visit in order to fulfill this ResetRoom, mapped by their in-room ID.
        /// If they have been destroyed, this Resetroom can no longer be fulfilled during the current room visit.
        /// </summary>
        public IReadOnlyDictionary<string, RoomObstacle> ObstaclesToAvoid { get; }

        /// <summary>
        /// If true, this is equivalent to <see cref="NodesToAvoid"/> containing all nodes in the room.
        /// </summary>
        public bool MustStayPut { get; }

        protected override ExecutionResult ExecuteUseful(SuperMetroidModel model, ReadOnlyInGameState inGameState, int times = 1, int previousRoomCount = 0)
        {
            IReadOnlyList<int> visitedNodeIds = inGameState.GetVisitedNodeIds(previousRoomCount);

            // If the node at which we entered is not allowed, this is not fulfilled.
            if (!Nodes.ContainsKey(visitedNodeIds[0]))
            {
                return null;
            }

            // If we have visited a node to avoid, this is not fulfilled.
            if (NodesToAvoid.Keys.Intersect(visitedNodeIds).Any())
            {
                return null;
            }

            // If we were supposed to stay put but have visited more than the starting node, this is not fulfilled
            if (MustStayPut && visitedNodeIds.Count > 1)
            {
                return null;
            }

            // If we have destroyed an obstacle that needed to be preserved, this is not fulfilled
            if (ObstaclesToAvoid.Keys.Intersect(inGameState.GetDestroyedObstacleIds(previousRoomCount)).Any())
            {
                return null;
            }

            // We've avoided all pitfalls. This ResetRoom is fulfilled. Clone the InGameState to fulfill method contract
            return new ExecutionResult(inGameState.Clone());
        }

        protected override void PropagateLogicalOptions(ReadOnlyLogicalOptions logicalOptions)
        {
            // Nothing to do here
        }

        protected override bool CalculateLogicallyNever()
        {
            // There might be some complex checks that can find cases where this is impossible,
            // but they are layout-based and not logic-based, so out of scope for this method.
            // (also unlikely to have enough value to be worth the effort)
            return false;
        }

        protected override bool CalculateLogicallyAlways()
        {
            return false;
        }

        protected override bool CalculateLogicallyFree()
        {
            return false;
        }
    }

    public class UnfinalizedResetRoom : AbstractUnfinalizedObjectLogicalElement<UnfinalizedResetRoom, ResetRoom>
    {
        public IList<int> NodeIds { get; set; } = new List<int>();

        /// <summary>
        /// <para>Only available after a call to <see cref="InitializeReferencedLogicalElementProperties(UnfinalizedSuperMetroidModel, UnfinalizedRoom)"/>.</para>
        /// <para>The nodes that this element's NodeIds reference. </para>
        /// </summary>
        public IList<UnfinalizedRoomNode> Nodes { get; set; }

        public ISet<int> NodeIdsToAvoid { get; set; } = new HashSet<int>();

        /// <summary>
        /// <para>Only available after a call to <see cref="InitializeReferencedLogicalElementProperties(UnfinalizedSuperMetroidModel, UnfinalizedRoom)"/>.</para>
        /// <para>The nodes that this element's NodeIdToAvoids reference. </para>
        /// </summary>
        public IList<UnfinalizedRoomNode> NodesToAvoid { get; set; }

        public ISet<string> ObstaclesIdsToAvoid { get; set; } = new HashSet<string>();

        /// <summary>
        /// <para>Only available after a call to <see cref="InitializeReferencedLogicalElementProperties(UnfinalizedSuperMetroidModel, UnfinalizedRoom)"/>.</para>
        /// <para>The obstacles that this element's ObstaclesIdsToAvoid reference. </para>
        /// </summary>
        public IList<UnfinalizedRoomObstacle> ObstaclesToAvoid { get; set; }

        public bool MustStayPut { get; set; } = false;

        protected override ResetRoom CreateFinalizedElement(UnfinalizedResetRoom sourceElement, Action<ResetRoom> mappingsInsertionCallback, ModelFinalizationMappings mappings)
        {
            return new ResetRoom(sourceElement, mappingsInsertionCallback, mappings);
        }

        public override IEnumerable<string> InitializeReferencedLogicalElementProperties(UnfinalizedSuperMetroidModel model, UnfinalizedRoom room)
        {
            List<string> unhandled = new List<string>();

            List<UnfinalizedRoomNode> nodes = new List<UnfinalizedRoomNode>();
            foreach (int nodeId in NodeIds)
            {
                if (room.Nodes.TryGetValue(nodeId, out UnfinalizedRoomNode node))
                {
                    nodes.Add(node);
                }
                else
                {
                    unhandled.Add($"Node {nodeId} in room {room.Name}");
                }
            }
            Nodes = nodes;

            List<UnfinalizedRoomNode> nodesToAvoid = new List<UnfinalizedRoomNode>();
            foreach (int nodeIdToAvoid in NodeIdsToAvoid)
            {
                if (room.Nodes.TryGetValue(nodeIdToAvoid, out UnfinalizedRoomNode nodeToAvoid))
                {
                    nodesToAvoid.Add(nodeToAvoid);
                }
                else
                {
                    unhandled.Add($"Node {nodeIdToAvoid} in room {room.Name}");
                }
            }
            NodesToAvoid = nodesToAvoid;

            List<UnfinalizedRoomObstacle> obstaclesToAvoid = new List<UnfinalizedRoomObstacle>();
            foreach (string obstacleIdToAvoid in ObstaclesIdsToAvoid)
            {
                if (room.Obstacles.TryGetValue(obstacleIdToAvoid, out UnfinalizedRoomObstacle obstacleToAvoid))
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
    }
}
