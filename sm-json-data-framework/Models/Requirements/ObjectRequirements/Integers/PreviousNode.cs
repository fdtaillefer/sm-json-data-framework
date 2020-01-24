using sm_json_data_framework.Models.InGameStates;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace sm_json_data_framework.Models.Requirements.ObjectRequirements.Integers
{
    public class PreviousNode : AbstractObjectLogicalElementWithNodeId
    {
        public override bool IsFulfilled(SuperMetroidModel model, InGameState inGameState, bool usePreviousRoom = false)
        {
            // Look at second-to-last visited node. Last node is the current node.
            IEnumerable<int> visitedNodeIds = inGameState.GetVisitedNodeIds(usePreviousRoom);
            return visitedNodeIds.ElementAtOrDefault(visitedNodeIds.Count() -2) == Value;
        }
    }
}
