using sm_json_data_framework.Models.InGameStates;
using sm_json_data_framework.Models.Rooms;
using System;
using System.Collections.Generic;
using System.Text;

namespace sm_json_data_framework.Models.Requirements
{
    public abstract class AbstractLogicalElement
    {
        /// <summary>
        /// If this logical element contains any properties that are an object referenced by another property(which is its identifier), initializes them.
        /// Also delegates to any sub-logical elements.
        /// </summary>
        /// <param name="model">A SuperMetroidModel that contains global data</param>
        /// <param name="room">The room in which this logical element is, or null if it's not in a room</param>
        /// <returns>A sequence of strings describing references that could not be initialized properly.</returns>
        public abstract IEnumerable<string> InitializeReferencedLogicalElementProperties(SuperMetroidModel model, Room room);

        /// <summary>
        /// Evaluates whether the requirements of this logical element are met by the provided in-game state.
        /// </summary>
        /// <param name="inGameState">The in-game state to evaluate</param>
        /// <param name="usePreviousRoom">If true, uses the last known room state at the previous room instead of the current room to answer
        /// (whenever in-room state is relevant).</param>
        /// <returns></returns>
        public abstract bool IsFulfilled(InGameState inGameState, bool usePreviousRoom = false);

        // STITCHME could be nice to ask for always and never?
    }
}
