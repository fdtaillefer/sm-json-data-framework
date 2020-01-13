using sm_json_data_framework.Models.Requirements;
using sm_json_data_framework.Models.Rooms.Nodes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;

namespace sm_json_data_framework.Models.Rooms
{
    public class StratFailure
    {
        public string Name { get; set; }

        [JsonPropertyName("leadsToNode")]
        public int? LeadsToNodeId { get; set; }

        /// <summary>
        /// <para>Not available before <see cref="Initialize(SuperMetroidModel, Room)"/> has been called.</para>
        /// <para>The node that this strat failure leads to, if it leads to a node</para>
        /// </summary>
        [JsonIgnore]
        public RoomNode LeadsToNode { get; set; }

        public LogicalRequirements Cost { get; set; } = new LogicalRequirements();

        public bool Softlock { get; set; } = false;

        public bool ClearsPreviousNode { get; set; } = false;

        /// <summary>
        /// Initializes additional properties in this StratFailure, which wouldn't be initialized by simply parsing a rooms json file.
        /// All such properties are identified in their own documentation and should not be read if this method isn't called.
        /// </summary>
        /// <param name="model">The model to use to initialize the additional properties</param>
        /// <param name="room">The room in which this StratFailure is</param>
        public void Initialize(SuperMetroidModel model, Room room)
        {
            // Initialize LeadsToNode
            if(LeadsToNodeId != null)
            {
                LeadsToNode = room.Nodes[(int)LeadsToNodeId];
            }
        }

        /// <summary>
        /// Goes through all logical elements within this StratFailure (and all LogicalRequirements within any of them),
        /// attempting to initialize any property that is an object referenced by another property(which is its identifier).
        /// </summary>
        /// <param name="model">A SuperMetroidModel that contains global data</param>
        /// <param name="room">The room in which this StratFailure is</param>
        /// <returns>A sequence of strings describing references that could not be initialized properly.</returns>
        public IEnumerable<string> InitializeReferencedLogicalElementProperties(SuperMetroidModel model, Room room)
        {
            List<string> unhandled = new List<string>();

            unhandled.AddRange(Cost.InitializeReferencedLogicalElementProperties(model, room));

            return unhandled.Distinct();
        }
    }
}
