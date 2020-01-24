using System;
using System.Collections.Generic;
using System.Text;

namespace sm_json_data_framework.Rules
{
    public interface IRunway
    {
        int Length { get; }

        int GentleUpTiles { get; }

        int GentleDownTiles { get; }

        int SteepUpTiles { get; } 

        int SteepDownTiles { get; }

        int StartingDownTiles { get; }

        int EndingUpTiles { get; }

        int OpenEnds { get; }
    }
}
