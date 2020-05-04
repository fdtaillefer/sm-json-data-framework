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
        public Strat StratUsed { get; set; }

        protected MoveToNodeAction()
        {

        }

        public MoveToNodeAction(SuperMetroidModel model, InGameState initialInGameState, Strat stratUsed, ExecutionResult executionResult) :
            base(model, initialInGameState, executionResult)
        {
            StratUsed = stratUsed;
        }

        public override AbstractNavigationAction Reverse(SuperMetroidModel model)
        {
            MoveToNodeAction reverseAction = new MoveToNodeAction();
            TransferDataToReverseAbstractAction(reverseAction);
            reverseAction.StratUsed = StratUsed;

            return reverseAction;
        }
    }
}
