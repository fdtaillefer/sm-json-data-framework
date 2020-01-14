using sm_json_data_framework.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;

namespace sm_json_data_framework.Models.Rooms.Nodes
{
    public class ViewableNode: InitializablePostDeserializeInNode
    {
        [JsonPropertyName("id")]
        public int NodeId { get; set; }

        /// <summary>
        /// <para>Not available before <see cref="Initialize(SuperMetroidModel, Room)"/> has been called.</para>
        /// <para>The node that is viewable</para>
        /// </summary>
        [JsonIgnore]
        public RoomNode Node { get; set; }

        public IEnumerable<Strat> Strats { get; set; } = Enumerable.Empty<Strat>();

        public void Initialize(SuperMetroidModel model, Room room, RoomNode node)
        {
            // Initialize Node
            Node = room.Nodes[NodeId];

            // Eliminate disabled strats
            Strats = Strats.WhereEnabled(model);

            // Initialize Strats
            foreach (Strat strat in Strats)
            {
                strat.Initialize(model, room);
            }
        }

        public IEnumerable<string> InitializeReferencedLogicalElementProperties(SuperMetroidModel model, Room room, RoomNode node)
        {
            List<string> unhandled = new List<string>();

            foreach(Strat strat in Strats)
            {
                unhandled.AddRange(strat.InitializeReferencedLogicalElementProperties(model, room));
            }

            return unhandled.Distinct();
        }
    }
}
