using System;
using System.Collections.Generic;
using System.Text;

namespace sm_json_data_parser.Models.Requirements.ObjectRequirements.SubObjects
{
    public class CanShineCharge : AbstractObjectLogicalElement
    {
        public int UsedTiles { get; set; }

        public int GentleUpTiles { get; set; } = 0;

        public int GentleDownTiles { get; set; } = 0;

        public int SteepUpTiles { get; set; } = 0;

        public int SteepDownTiles { get; set; } = 0;

        public int StartingDownTiles { get; set; } = 0;

        public int OpenEnd { get; set; }

        public int ShinesparkFrames { get; set; }
    }
}
