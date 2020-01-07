using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;

namespace sm_json_data_parser.Models.Rooms.Node.NodeSparking
{
    public class CanLeaveCharged
    {
        public int UsedTiles { get; set; }

        public int FramesRemaining { get; set; }

        public int ShinesparkFrames { get; set; }

        [JsonPropertyName("initiateAt")]
        public int? InitiateAtNodeId { get; set; }

        public IEnumerable<Strat> Strats { get; set; } = Enumerable.Empty<Strat>();

        public int OpenEnd { get; set; } = 0;

        public int GentleUpTiles { get; set; } = 0;

        public int GentleDownTiles { get; set; } = 0;

        public int SteepUpTiles { get; set; } = 0;

        public int SteepDownTiles { get; set; } = 0;

        public int StartingDownTiles { get; set; } = 0;

        // STITCHME Note?
    }
}
