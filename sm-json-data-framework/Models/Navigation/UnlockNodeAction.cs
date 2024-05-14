using sm_json_data_framework.Models.InGameStates;
using sm_json_data_framework.Models.Requirements;
using System;
using System.Collections.Generic;
using System.Text;

namespace sm_json_data_framework.Models.Navigation
{
    /// <summary>
    /// An action that represents unlocking the node the player is at, without interacting with it.
    /// </summary>
    public class UnlockNodeAction : AbstractNavigationAction
    {
        protected UnlockNodeAction(string intent) : base(intent)
        {

        }

        public UnlockNodeAction(string intent, UnfinalizedSuperMetroidModel model, ReadOnlyInGameState initialInGameState, ExecutionResult executionResult) :
            base(intent, model, initialInGameState, executionResult)
        {

        }

        public override AbstractNavigationAction Reverse(UnfinalizedSuperMetroidModel model)
        {
            UnlockNodeAction reverseAction = new UnlockNodeAction($"Undo action '{this.IntentDescription}'");
            TransferDataToReverseAbstractAction(reverseAction);

            return reverseAction;
        }
    }
}
