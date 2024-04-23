using sm_json_data_framework.Rules;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;

namespace sm_json_data_framework.Models.Rooms.Nodes
{
    public class DoorEnvironment : InitializablePostDeserializeInNode
    {
        public PhysicsEnum Physics { get; set; }

        [JsonPropertyName("entranceNodes")]
        public IEnumerable<int> EntranceNodeIds { get; set; }

        /// <summary>
        /// <para>Not available before <see cref="Initialize(SuperMetroidModel, Room, RoomNode)"/> has been called.</para>
        /// <para>The nodes that Samus must have entered from for this environment to be applicable. Or, if null, the environment is always applicable.</para>
        /// </summary>
        [JsonIgnore]
        public IEnumerable<RoomNode> EntranceNodes { get; set; }

        /// <summary>
        /// <para>Not available before <see cref="Initialize(SuperMetroidModel, Room, RoomNode)"/> has been called.</para>
        /// <para>The RoomNode on which this environment is.</para>
        /// </summary>
        [JsonIgnore]
        public RoomNode Node { get; set; }

        public void InitializeForeignProperties(SuperMetroidModel model, Room room, RoomNode node)
        {
            Node = node;

            // Initialize list of EntranceNodes. This also serves as a sanity check and will throw if an ID is invalid.
            if (EntranceNodeIds != null)
            {
                List<RoomNode> entranceNodes = new List<RoomNode>();
                foreach (int entranceNodeId in EntranceNodeIds)
                {
                    room.Nodes.TryGetValue(entranceNodeId, out RoomNode entranceNode);
                    if (entranceNode == null)
                    {
                        throw new Exception($"A DoorEnvironment's entranceNode ID {entranceNodeId} not found in room '{room.Name}' (the environment was on node {node.Id}).");
                    }
                    entranceNodes.Add(entranceNode);
                }
                EntranceNodes = entranceNodes;
            }
        }

        public void InitializeOtherProperties(SuperMetroidModel model, Room room, RoomNode node)
        {
            // Nothing relevant to initialize
        }

        public bool CleanUpUselessValues(SuperMetroidModel model, Room room, RoomNode node)
        {
            // Nothing relevant to cleanup

            return true;
        }

        public IEnumerable<string> InitializeReferencedLogicalElementProperties(SuperMetroidModel model, Room room, RoomNode node)
        {
            // No logical element in a door environment
            return Enumerable.Empty<string>();
        }
    }
}
