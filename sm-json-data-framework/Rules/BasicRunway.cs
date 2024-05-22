using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace sm_json_data_framework.Rules
{
    /// <summary>
    /// A basic implementation of IRunway, with no additional bells and whistles
    /// </summary>
    public class BasicRunway: IRunway
    {
        public int Length { get; }

        public int GentleUpTiles { get; }

        public int GentleDownTiles { get; }

        public int SteepUpTiles { get; }

        public int SteepDownTiles { get; }

        public int StartingDownTiles { get; }

        public int EndingUpTiles { get; }

        public int OpenEnds { get; }

        public BasicRunway(int length, int gentleUpTiles, int gentleDownTiles, int steepUpTiles, int steepDownTiles, int startingDownTiles, int endingUpTiles, int openEnds)
        {
            Length = length;
            GentleUpTiles = gentleUpTiles;
            GentleDownTiles = gentleDownTiles;
            SteepUpTiles = steepUpTiles;
            SteepDownTiles = steepDownTiles;
            StartingDownTiles = startingDownTiles;
            EndingUpTiles = endingUpTiles;
            OpenEnds = openEnds;
        }
    }
}
