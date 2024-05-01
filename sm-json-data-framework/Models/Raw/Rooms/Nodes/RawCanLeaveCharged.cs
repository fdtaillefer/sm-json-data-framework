using sm_json_data_framework.Models.Rooms.Nodes;
using sm_json_data_framework.Models.Rooms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace sm_json_data_framework.Models.Raw.Rooms.Nodes
{
    public class RawCanLeaveCharged
    {
        public int UsedTiles { get; set; }

        public int FramesRemaining { get; set; }

        public int ShinesparkFrames { get; set; }

        public RawInitiateRemotely InitiateRemotely { get; set; }

        public IList<RawStrat> Strats { get; set; } = new List<RawStrat>();

        public int OpenEnd { get; set; } = 0;

        public int GentleUpTiles { get; set; } = 0;

        public int GentleDownTiles { get; set; } = 0;

        public int SteepUpTiles { get; set; } = 0;

        public int SteepDownTiles { get; set; } = 0;

        public int StartingDownTiles { get; set; } = 0;
    }
}
