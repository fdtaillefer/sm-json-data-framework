using sm_json_data_framework.Models.Rooms.Nodes;
using sm_json_data_framework.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;

namespace sm_json_data_framework.Models.Rooms
{
    public class LinkTo : InitializablePostDeserializeInRoom
    {
        [JsonPropertyName("id")]
        public int TargetNodeId { get; set; }

        /// <summary>
        /// <para>Not available before <see cref="Initialize(SuperMetroidModel, Room)"/> has been called.</para>
        /// <para>The node that this link leads to</para>
        /// </summary>
        [JsonIgnore]
        public RoomNode TargetNode { get; set; }

        public IDictionary<string, Strat> Strats { get; set; } = new Dictionary<string, Strat>();

        public IEnumerable<Action> Initialize(SuperMetroidModel model, Room room)
        {
            // Initialize TargetNode
            TargetNode = room.Nodes[TargetNodeId];

            // Eliminate disabled strats
            Strats = Strats.WhereEnabled(model);

            // Initialize Strats
            List<Action> postRoomInitializeCallbacks = new List<Action>();
            foreach (Strat strat in Strats.Values)
            {
                postRoomInitializeCallbacks.AddRange(strat.Initialize(model, room));
            }

            return postRoomInitializeCallbacks;
        }

        public IEnumerable<string> InitializeReferencedLogicalElementProperties(SuperMetroidModel model, Room room)
        {
            List<string> unhandled = new List<string>();

            foreach(Strat strat in Strats.Values)
            {
                unhandled.AddRange(strat.InitializeReferencedLogicalElementProperties(model, room));
            }

            return unhandled.Distinct();
        }
    }
}
