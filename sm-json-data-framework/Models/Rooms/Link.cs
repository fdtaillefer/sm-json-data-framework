using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;

namespace sm_json_data_framework.Models.Rooms
{
    public class Link
    {
        [JsonPropertyName("from")]
        public int FromNodeId { get; set; }

        public IEnumerable<LinkTo> To { get; set; } = Enumerable.Empty<LinkTo>();
    }
}
