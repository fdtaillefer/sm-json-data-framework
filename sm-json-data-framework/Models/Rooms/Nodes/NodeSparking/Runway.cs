using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace sm_json_data_parser.Models.Rooms.Node.NodeSparking
{
    public class Runway
    {
        public int Length { get; set; }

        public int GentleUpTiles { get; set; } = 0;

        public int GentleDownTiles { get; set; } = 0;

        public int SteepUpTiles { get; set; } = 0;

        public int SteepDownTiles { get; set; } = 0;

        public int StartingDownTiles { get; set; } = 0;

        public int EndingUpTiles { get; set; } = 0;

        public IEnumerable<Strat> Strats { get; set; } = Enumerable.Empty<Strat>();

        public bool UsableComingIn = true;

        public int OpenEnd { get; set; }

        // STITCHME Note?
    }
}
