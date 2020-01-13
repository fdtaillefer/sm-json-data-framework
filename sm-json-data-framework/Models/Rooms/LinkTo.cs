using sm_json_data_framework.Models.Rooms.Nodes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;

namespace sm_json_data_framework.Models.Rooms
{
    public class LinkTo
    {
        [JsonPropertyName("id")]
        public int TargetNodeId { get; set; }

        /// <summary>
        /// <para>Not available before <see cref="Initialize(SuperMetroidModel, Room)"/> has been called.</para>
        /// <para>The node that this link leads to</para>
        /// </summary>
        [JsonIgnore]
        public RoomNode TargetNode { get; set; }

        public IEnumerable<Strat> Strats { get; set; } = Enumerable.Empty<Strat>();

        /// <summary>
        /// Initializes additional properties in this LinkTo, which wouldn't be initialized by simply parsing a rooms json file.
        /// All such properties are identified in their own documentation and should not be read if this method isn't called.
        /// </summary>
        /// <param name="model">The model to use to initialize the additional properties</param>
        /// <param name="room">The room in which this LinkTo is</param>
        public void Initialize(SuperMetroidModel model, Room room)
        {
            // Initialize TargetNode
            TargetNode = room.Nodes[TargetNodeId];

            // Initialize Strats
            foreach(Strat strat in Strats)
            {
                strat.Initialize(model, room);
            }
        }

        /// <summary>
        /// Goes through all logical elements within this LinkTo (and all LogicalRequirements within any of them),
        /// attempting to initialize any property that is an object referenced by another property(which is its identifier).
        /// </summary>
        /// <param name="model">A SuperMetroidModel that contains global data</param>
        /// <param name="room">The room in which this LinkTo is</param>
        /// <returns>A sequence of strings describing references that could not be initialized properly.</returns>
        public IEnumerable<string> InitializeReferencedLogicalElementProperties(SuperMetroidModel model, Room room)
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
