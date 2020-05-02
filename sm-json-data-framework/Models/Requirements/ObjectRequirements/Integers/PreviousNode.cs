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
        /// <param name="usePreviousRoom">If true, uses the last known room state at the previous room instead of the current room to answer
        /// <returns></returns>
        public bool IsFulfilled(InGameState inGameState, bool usePreviousRoom)
        {
            // Look at second-to-last visited node (last node is the current node)
            IEnumerable<int> visitedNodeIds = inGameState.GetVisitedNodeIds(usePreviousRoom);
            return visitedNodeIds.ElementAtOrDefault(visitedNodeIds.Count() -2) == Value;
        }

        public override ExecutionResult Execute(SuperMetroidModel model, InGameState inGameState, int times = 1, bool usePreviousRoom = false)
        {
            if (IsFulfilled(inGameState, usePreviousRoom))
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
