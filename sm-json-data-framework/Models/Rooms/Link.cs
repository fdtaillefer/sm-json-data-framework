using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;

namespace sm_json_data_framework.Models.Rooms
{
    public class Link : InitializablePostDeserializeInRoom
    {
        [JsonPropertyName("from")]
        public int FromNodeId { get; set; }

        public IEnumerable<LinkTo> To { get; set; } = Enumerable.Empty<LinkTo>();

        public void Initialize(SuperMetroidModel model, Room room)
        {
            foreach (LinkTo linkTo in To)
            {
                linkTo.Initialize(model, room);
            }
        }

        public IEnumerable<string> InitializeReferencedLogicalElementProperties(SuperMetroidModel model, Room room)
        {
            List<string> unhandled = new List<string>();

            foreach(LinkTo linkTo in To)
            {
                unhandled.AddRange(linkTo.InitializeReferencedLogicalElementProperties(model, room));
            }

            return unhandled.Distinct();
        }
    }
}
