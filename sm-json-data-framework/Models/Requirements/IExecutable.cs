using sm_json_data_framework.Models.InGameStates;
using System;
using System.Collections.Generic;
using System.Text;

namespace sm_json_data_framework.Models.Requirements
{
    public interface IExecutable
    {
        /// <summary>
        /// Attempts to execute this executable based on the provided in-game state (which will not be altered), 
        /// by fulfilling its execution requirements.
        /// </summary>
        /// <param name="model">A model that can be used to obtain data about the current game configuration.</param>
        /// <param name="inGameState">The in-game state to use for execution. This will NOT be altered by this method.</param>
        /// <param name="times">The number of consecutive times that this should be executed.
        /// Only really impacts resource cost, since most items are non-consumable.</param>
        /// <param name="previousRoomCount">The number of playable rooms to go back by (whenever in-room state is relevant). 
        /// 0 means current room, 3 means go back 3 rooms (using last known state), negative values are invalid. Non-playable rooms are skipped.</param>
        /// <returns>An ExecutionResult describing the execution if successful, or null otherwise.
        /// The in-game state in that ExecutionResult will never be the same instance as the provided one.</returns>
        public ExecutionResult Execute(UnfinalizedSuperMetroidModel model, ReadOnlyInGameState inGameState, int times = 1, int previousRoomCount = 0);
    }
}
