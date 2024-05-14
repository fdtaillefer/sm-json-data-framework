using sm_json_data_framework.Models.InGameStates;
using sm_json_data_framework.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace sm_json_data_framework.Models.Requirements.ObjectRequirements.Integers
{
    /// <summary>
    /// A logical element which Requires Samus to have arrived at the current node strictly from a subset of acceptable nodes.
    /// </summary>
    public class PreviousNode : AbstractObjectLogicalElementWithNodeId<UnfinalizedPreviousNode, PreviousNode>
    {
        public PreviousNode(UnfinalizedPreviousNode innerElement, Action<PreviousNode> mappingsInsertionCallback, ModelFinalizationMappings mappings)
            : base(innerElement, mappingsInsertionCallback, mappings)
        {

        }
    }

    public class UnfinalizedPreviousNode : AbstractUnfinalizedObjectLogicalElementWithNodeId<UnfinalizedPreviousNode, PreviousNode>
    {
        public UnfinalizedPreviousNode()
        {

        }

        public UnfinalizedPreviousNode(int nodeId) : base(nodeId)
        {

        }

        protected override PreviousNode CreateFinalizedElement(UnfinalizedPreviousNode sourceElement, Action<PreviousNode> mappingsInsertionCallback, ModelFinalizationMappings mappings)
        {
            return new PreviousNode(sourceElement, mappingsInsertionCallback, mappings);
        }

        protected override bool ApplyLogicalOptionsEffects(ReadOnlyLogicalOptions logicalOptions)
        {
            // Nothing in logical options can alter this
            return false;
        }

        public override bool IsNever()
        {
            return false;
        }

        /// <summary>
        /// Returns whether the provided InGameState fulfills this PreviousNode element.
        /// </summary>
        /// <param name="inGameState">The in-game state to evaluate</param>
        /// <param name="previousRoomCount">The number of playable rooms to go back by. 
        /// 0 means current room, 3 means go back 3 rooms (using last known state), negative values are invalid. Non-playable rooms are skipped.</param>
        /// <returns></returns>
        public bool IsFulfilled(ReadOnlyInGameState inGameState, int previousRoomCount = 0)
        {
            // Look at second-to-last visited node (last node is the current node)
            IReadOnlyList<int> visitedNodeIds = inGameState.GetVisitedNodeIds(previousRoomCount);
            return visitedNodeIds.ElementAtOrDefault(visitedNodeIds.Count -2) == Value;
        }

        protected override ExecutionResult ExecuteUseful(UnfinalizedSuperMetroidModel model, ReadOnlyInGameState inGameState, int times = 1, int previousRoomCount = 0)
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
