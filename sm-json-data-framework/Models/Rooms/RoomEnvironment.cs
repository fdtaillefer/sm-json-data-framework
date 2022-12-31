using sm_json_data_framework.Models.Rooms.Nodes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;

namespace sm_json_data_framework.Models.Rooms
{
    public class RoomEnvironment : InitializablePostDeserializeInRoom
    {
        public Boolean Heated { get; set; }

        [JsonPropertyName("entranceNodes")]
        public IEnumerable<int> EntranceNodeIds { get; set; }

        /// <summary>
        /// <para>Not available before <see cref="Initialize(SuperMetroidModel, Room)"/> has been called.</para>
        /// <para>The room to which this environment applies.</para>
        /// </summary>
        [JsonIgnore]
        public Room Room { get; set; }

        /// <summary>
        /// <para>Not available before <see cref="Initialize(SuperMetroidModel, Room, RoomNode)"/> has been called.</para>
        /// <para>The nodes that Samus must have entered from for this envonrment to be applicable. Or, if null, the environment is always applicable.</para>
        /// </summary>
        [JsonIgnore]
        public IEnumerable<RoomNode> EntranceNodes { get; set; }

        public IEnumerable<Action> Initialize(SuperMetroidModel model, Room room)
        {
            List<Action> initializedRoomCallbacks = new List<Action>();

            Room = room;

            // Initialize list of EntranceNodes. This also servers as a sanity check and will throw if an ID is invalid.
            // Depending on initialization order, this could be done without being a callback
            if(EntranceNodeIds != null)
            {
                initializedRoomCallbacks.Add(() => {
                    List<RoomNode> entranceNodes = new List<RoomNode>();
                    foreach (int nodeId in EntranceNodeIds)
                    {
                        room.Nodes.TryGetValue(nodeId, out RoomNode node);
                        if(node == null)
                        {
                            throw new Exception($"A RoomEnvironment's entranceNode ID {nodeId} not found in room '{room.Name}'.");
                        }
                        entranceNodes.Add(node);
                    }
                    EntranceNodes = entranceNodes;
                });
            }

            return initializedRoomCallbacks;
        }

        public IEnumerable<string> InitializeReferencedLogicalElementProperties(SuperMetroidModel model, Room room)
        {
            // No logical element in a room environment
            return Enumerable.Empty<string>();
        }

    }
}
