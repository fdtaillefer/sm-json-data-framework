using System;
using System.Collections.Generic;
using System.Text;

namespace sm_json_data_framework.Rules
{
    /// <summary>
    /// Represents a contiguous stretch of tiles that Samus can run on, in a specific direction.
    /// To get the same stretch in the opposite direction, a runway could be reversed.
    /// </summary>
    public interface IRunway
    {
        /// <summary>
        /// Number of tiles in this runway.
        /// </summary>
        int Length { get; }

        /// <summary>
        /// Number of tiles in this runway that slope up gently.
        /// </summary>
        int GentleUpTiles { get; }

        /// <summary>
        /// Number of tiles in this runway that slope down gently.
        /// </summary>
        int GentleDownTiles { get; }

        /// <summary>
        /// Number of tiles in this runway that slope up steeply.
        /// </summary>
        int SteepUpTiles { get; }

        /// <summary>
        /// Number of tiles in this runway that slope down steeply.
        /// </summary>
        int SteepDownTiles { get; }

        /// <summary>
        /// Number of tiles at the start of the runway that slope down.
        /// </summary>
        int StartingDownTiles { get; }

        /// <summary>
        /// Number of tiles at the end of the runway that slope up.
        /// </summary>
        int EndingUpTiles { get; }

        /// <summary>
        /// Number of open ends at the edges of this runway.
        /// And open end is a tile that Samus can walk all the way to the very edge of, without being blocked by a wall sooner.
        /// </summary>
        int OpenEnds { get; }
    }
}
