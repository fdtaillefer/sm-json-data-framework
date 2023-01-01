using sm_json_data_framework.Models.InGameStates;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace sm_json_data_framework.Models.Requirements.ObjectRequirements.Integers
{
    /// <summary>
    /// A logical element which Requires Samus to have arrived at the current node strictly from a subset of acceptable nodes.
    /// </summary>
    public class PreviousNode : AbstractObjectLogicalElementWithNodeId
    {
        /// <summary>
        /// Returns whether the provided InGameState fulfills this PreviousNode element.
        /// </summary>
        /// <param name="inGameState">The in-game state to evaluate</param>
        /// <param name="previousRoomCount">The number of playable rooms to go back by. 
        /// 0 means current room, 3 means go back 3 rooms (using last known state), negative values are invalid. Non-playable rooms are skipped.</param>
        /// <returns></returns>
        public bool IsFulfilled(InGameState inGameState, int previousRoomCount = 0)
        {
            // Look at second-to-last visited node (last node is the current node)
            IEnumerable<int> visitedNodeIds = inGameState.GetVisitedNodeIds(previousRoomCount);
            return visitedNodeIds.ElementAtOrDefault(visitedNodeIds.Count() -2) == Value;
        }

        public override ExecutionResult Execute(SuperMetroidModel model, InGameState inGameState, int times = 1, int previousRoomCount = 0)
        {
            if (IsFulfilled(inGameState, previousRoomCount))
            {
                // Clone the InGameState to fulfill method contract
                return new ExecutionResult(inGameState.Clone());
            }
            else
            {
                return null;
            }
        }
    }
}
