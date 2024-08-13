using sm_json_data_framework.InGameStates;
using sm_json_data_framework.Options;
using sm_json_data_framework.Rules;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace sm_json_data_framework.Models.Requirements.ObjectRequirements.Integers
{
    /// <summary>
    /// A logical element which Requires Samus to have arrived at the current node strictly from a specific node.
    /// </summary>
    public class PreviousNode : AbstractObjectLogicalElementWithNodeId<UnfinalizedPreviousNode, PreviousNode>
    {
        public PreviousNode(UnfinalizedPreviousNode sourceElement, Action<PreviousNode> mappingsInsertionCallback, ModelFinalizationMappings mappings)
            : base(sourceElement, mappingsInsertionCallback, mappings)
        {

        }

        /// <summary>
        /// The in-room ID of the node from which Samus must have arrived.
        /// </summary>
        public int NodeId => Value;

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
            return visitedNodeIds.ElementAtOrDefault(visitedNodeIds.Count - 2) == NodeId;
        }

        protected override ExecutionResult ExecutePossible(SuperMetroidModel model, ReadOnlyInGameState inGameState, int times = 1, int previousRoomCount = 0)
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

        protected override void PropagateLogicalOptions(ReadOnlyLogicalOptions logicalOptions, SuperMetroidModel model)
        {
            // Nothing to do here
        }

        protected override bool CalculateLogicallyNever(SuperMetroidModel model)
        {
            // This could become impossible, but that depends on layout and not logic, and is beyond the scope of this method.
            return false;
        }

        protected override bool CalculateLogicallyAlways(SuperMetroidModel model)
        {
            return false;
        }

        protected override bool CalculateLogicallyFree(SuperMetroidModel model)
        {
            return false;
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
    }
}
