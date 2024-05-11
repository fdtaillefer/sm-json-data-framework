using sm_json_data_framework.Models.InGameStates;
using sm_json_data_framework.Models.Requirements;
using sm_json_data_framework.Models.Rooms;
using System;
using System.Collections.Generic;
using System.Text;

namespace sm_json_data_framework.Models.Navigation
{
    /// <summary>
    /// An action that represents moving from a node to another within a room.
    /// </summary>
    public class MoveToNodeAction : AbstractNavigationAction
    {
        /// <summary>
        ///  The Strat that was used to navigate a link between two nodes.
        ///  Naturally, if this action is a reversed action, this is the Strat that was "unused".
        /// </summary>
        public UnfinalizedStrat StratUsed { get; set; }

        protected MoveToNodeAction(string intent) : base(intent)
        {

        }

        public MoveToNodeAction(string intent, SuperMetroidModel model, ReadOnlyInGameState initialInGameState, UnfinalizedStrat stratUsed, ExecutionResult executionResult) :
            base(intent, model, initialInGameState, executionResult)
        {
            StratUsed = stratUsed;
        }

        public override AbstractNavigationAction Reverse(SuperMetroidModel model)
        {
            MoveToNodeAction reverseAction = new MoveToNodeAction($"Undo action '{this.IntentDescription}'");
            TransferDataToReverseAbstractAction(reverseAction);
            reverseAction.StratUsed = StratUsed;

            return reverseAction;
        }

        public override string GetSuccessOutputString()
        {
            return $"Action succeeded using strat '{StratUsed.Name}'";
        }
    }
}
