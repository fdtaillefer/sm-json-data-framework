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
        // STITCHME When removing IsFulfilled, keep it here. It will have value. But remove times and model, they're not used. And remove uPR default.
        public override bool IsFulfilled(SuperMetroidModel model, InGameState inGameState, int times = 1, bool usePreviousRoom = false)
        {
            // Look at second-to-last visited node (last node is the current node)
            IEnumerable<int> visitedNodeIds = inGameState.GetVisitedNodeIds(usePreviousRoom);
            return visitedNodeIds.ElementAtOrDefault(visitedNodeIds.Count() -2) == Value;
        }

        public override InGameState AttemptFulfill(SuperMetroidModel model, InGameState inGameState, int times = 1, bool usePreviousRoom = false)
        {
            if (IsFulfilled(model, inGameState, times: times, usePreviousRoom: usePreviousRoom))
            {
                // Clone the InGameState to fulfill method contract
                return inGameState.Clone();
            }
            else
            {
                return null;
            }
        }
    }
}
