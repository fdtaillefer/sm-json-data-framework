﻿using sm_json_data_framework.Models.Raw.Rooms;
using sm_json_data_framework.Models.Rooms.Nodes;
using sm_json_data_framework.Options;
using sm_json_data_framework.Rules;
using sm_json_data_framework.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;

namespace sm_json_data_framework.Models.Rooms
{
    /// <summary>
    /// Describes the environment in a specific room. 
    /// This excludes any property that can vary within a given room (see <see cref="DoorEnvironment"/> for that).
    /// </summary>
    public class RoomEnvironment : AbstractModelElement<UnfinalizedRoomEnvironment, RoomEnvironment>
    {
        public RoomEnvironment(UnfinalizedRoomEnvironment sourceElement, Action<RoomEnvironment> mappingsInsertionCallback, ModelFinalizationMappings mappings)
            : base(sourceElement, mappingsInsertionCallback)
        {
            Heated = sourceElement.Heated;
            EntranceNodes = sourceElement.EntranceNodes?.Select(node => node.Finalize(mappings)).ToDictionary(node => node.Id).AsReadOnly();
            Room = sourceElement.Room.Finalize(mappings);
        }

        /// <summary>
        /// Whether the environment is geated, making Samus take damage unless she has something to mitigate the heat.
        /// </summary>
        public bool Heated { get; }

        /// <summary>
        /// The nodes that enable this environment if Samus has entered the room from one of them, mapped by in-room ID. Or, if null, the environment is always applicable.
        /// </summary>
        public IReadOnlyDictionary<int, RoomNode> EntranceNodes { get; }

        /// <summary>
        /// The room to which this environment applies.
        /// </summary>
        public Room Room { get; }

        protected override void PropagateLogicalOptions(ReadOnlyLogicalOptions logicalOptions, SuperMetroidModel model)
        {
            // Nothing to do here
        }

        public override bool CalculateLogicallyRelevant(SuperMetroidModel model)
        {
            // There's nothing that can make a room environment irrelevant
            return true;
        }
    }

    public class UnfinalizedRoomEnvironment : AbstractUnfinalizedModelElement<UnfinalizedRoomEnvironment, RoomEnvironment>, InitializablePostDeserializeInRoom
    {
        public bool Heated { get; set; }

        public ISet<int> EntranceNodeIds { get; set; }

        /// <summary>
        /// <para>Not available before <see cref="Initialize(UnfinalizedSuperMetroidModel, UnfinalizedRoom)"/> has been called.</para>
        /// <para>The nodes that Samus must have entered from for this environment to be applicable. Or, if null, the environment is always applicable.</para>
        /// </summary>
        public IList<UnfinalizedRoomNode> EntranceNodes { get; set; }

        /// <summary>
        /// <para>Not available before <see cref="Initialize(UnfinalizedSuperMetroidModel, UnfinalizedRoom)"/> has been called.</para>
        /// <para>The room to which this environment applies.</para>
        /// </summary>
        public UnfinalizedRoom Room { get; set; }

        public UnfinalizedRoomEnvironment()
        {

        }

        public UnfinalizedRoomEnvironment(RawRoomEnvironment rawEnvironment)
        {
            Heated = rawEnvironment.Heated;
            if (rawEnvironment.EntranceNodes != null)
            {
                EntranceNodeIds = new HashSet<int>(rawEnvironment.EntranceNodes);
            }
        }

        protected override RoomEnvironment CreateFinalizedElement(UnfinalizedRoomEnvironment sourceElement, Action<RoomEnvironment> mappingsInsertionCallback, ModelFinalizationMappings mappings)
        {
            return new RoomEnvironment(sourceElement, mappingsInsertionCallback, mappings);
        }

        public void InitializeProperties(UnfinalizedSuperMetroidModel model, UnfinalizedRoom room)
        {
            Room = room;

            // Initialize list of EntranceNodes. This also serves as a sanity check and will throw if an ID is invalid.
            if (EntranceNodeIds != null)
            {
                List<UnfinalizedRoomNode> entranceNodes = new List<UnfinalizedRoomNode>();
                foreach (int nodeId in EntranceNodeIds)
                {
                    room.Nodes.TryGetValue(nodeId, out UnfinalizedRoomNode node);
                    if (node == null)
                    {
                        throw new Exception($"A RoomEnvironment's entranceNode ID {nodeId} not found in room '{room.Name}'.");
                    }
                    entranceNodes.Add(node);
                }
                EntranceNodes = entranceNodes;
            }
        }

        public IEnumerable<string> InitializeReferencedLogicalElementProperties(UnfinalizedSuperMetroidModel model, UnfinalizedRoom room)
        {
            // No logical element in a room environment
            return Enumerable.Empty<string>();
        }
    }
}
