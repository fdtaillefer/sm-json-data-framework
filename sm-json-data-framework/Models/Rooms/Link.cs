using sm_json_data_framework.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;

namespace sm_json_data_framework.Models.Rooms
{
    /// <summary>
    /// Represents all ways to navigate directly from a specific node to any other node in the same room.
    /// </summary>
    public class Link : InitializablePostDeserializeInRoom
    {
        [JsonPropertyName("from")]
        public int FromNodeId { get; set; }

        /// <summary>
        /// The details of how this Link links to different nodes, mapped by target node ID.
        /// </summary>
        public IDictionary<int, LinkTo> To {get;set;} = new Dictionary<int, LinkTo>();

        public void InitializeProperties(SuperMetroidModel model, Room room)
        {
            foreach (LinkTo linkTo in To.Values)
            {
                linkTo.InitializeProperties(model, room);
            }
        }

        public bool CleanUpUselessValues(SuperMetroidModel model, Room room)
        {
            To = To.Where(kvp => kvp.Value.CleanUpUselessValues(model, room)).ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
            // A link with no destinations is useless
            return To.Any();
        }

        public IEnumerable<string> InitializeReferencedLogicalElementProperties(SuperMetroidModel model, Room room)
        {
            List<string> unhandled = new List<string>();

            foreach(LinkTo linkTo in To.Values)
            {
                unhandled.AddRange(linkTo.InitializeReferencedLogicalElementProperties(model, room));
            }

            return unhandled.Distinct();
        }
    }
}
