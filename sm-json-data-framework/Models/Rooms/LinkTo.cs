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

        public void InitializeProperties(SuperMetroidModel model, Room room)
        {
            // Initialize TargetNode
            TargetNode = room.Nodes[TargetNodeId];

            // Initialize strats
            foreach (Strat strat in Strats.Values)
            {
                strat.InitializeProperties(model, room);
            }
        }

        public bool CleanUpUselessValues(SuperMetroidModel model, Room room)
        {
            // Clean up strats
            Strats = Strats.Where(kvp => kvp.Value.CleanUpUselessValues(model, room)).ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

            // A LinkTo with no strats is useless
            return Strats.Any();
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
