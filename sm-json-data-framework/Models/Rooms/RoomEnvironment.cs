﻿using sm_json_data_framework.Models.Raw.Rooms;
using sm_json_data_framework.Models.Rooms.Nodes;
using sm_json_data_framework.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;

namespace sm_json_data_framework.Models.Rooms
{
    public class RoomEnvironment : AbstractModelElement, InitializablePostDeserializeInRoom
    {
        public bool Heated { get; set; }

        [JsonPropertyName("entranceNodes")]
        public ISet<int> EntranceNodeIds { get; set; }

        /// <summary>
        /// <para>Not available before <see cref="Initialize(SuperMetroidModel, Room)"/> has been called.</para>
        /// <para>The nodes that Samus must have entered from for this environment to be applicable. Or, if null, the environment is always applicable.</para>
        /// </summary>
        [JsonIgnore]
        public IList<RoomNode> EntranceNodes { get; set; }

        /// <summary>
        /// <para>Not available before <see cref="Initialize(SuperMetroidModel, Room)"/> has been called.</para>
        /// <para>The room to which this environment applies.</para>
        /// </summary>
        [JsonIgnore]
        public Room Room { get; set; }

        public RoomEnvironment()
        {

        }

        protected override bool ApplyLogicalOptionsEffects(ReadOnlyLogicalOptions logicalOptions)
        {
            // Logical options have no power here
            return false;
        }

        public RoomEnvironment(RawRoomEnvironment rawEnvironment)
        {
            Heated = rawEnvironment.Heated;
            if (rawEnvironment.EntranceNodes != null)
            {
                EntranceNodeIds = new HashSet<int>(rawEnvironment.EntranceNodes);
            }
        }

        public void InitializeProperties(SuperMetroidModel model, Room room)
        {
            Room = room;

            // Initialize list of EntranceNodes. This also serves as a sanity check and will throw if an ID is invalid.
            if (EntranceNodeIds != null)
            {
                List<RoomNode> entranceNodes = new List<RoomNode>();
                foreach (int nodeId in EntranceNodeIds)
                {
                    room.Nodes.TryGetValue(nodeId, out RoomNode node);
                    if (node == null)
                    {
                        throw new Exception($"A RoomEnvironment's entranceNode ID {nodeId} not found in room '{room.Name}'.");
                    }
                    entranceNodes.Add(node);
                }
                EntranceNodes = entranceNodes;
            }
        }

        public IEnumerable<string> InitializeReferencedLogicalElementProperties(SuperMetroidModel model, Room room)
        {
            // No logical element in a room environment
            return Enumerable.Empty<string>();
        }

    }
}
