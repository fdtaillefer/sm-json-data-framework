using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;

namespace sm_json_data_parser.Models.Rooms
{
    public class LinkTo
    {
        [JsonPropertyName("id")]
        public int TargetNodeId { get; set; }

        public IEnumerable<Strat> Strats { get; set; } = Enumerable.Empty<Strat>();

        // STITCHME Note?
    }
}
