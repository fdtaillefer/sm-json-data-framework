using sm_json_data_framework.Models.InGameStates;
using sm_json_data_framework.Models.Requirements;
using System;
using System.Collections.Generic;
using System.Text;

namespace sm_json_data_framework.Models.Navigation
{
    /// <summary>
    /// An action that represents re-enabling an item in the player's inventory that is disabled.
    /// Fails if the item is not currently disabled.
    /// </summary>
    public class EnableItemAction : AbstractNavigationAction
    {
        protected EnableItemAction(string intent) : base(intent)
        {

        }

        public EnableItemAction(string intent, UnfinalizedSuperMetroidModel model, ReadOnlyInGameState initialInGameState, ExecutionResult executionResult) :
            base(intent, model, initialInGameState, executionResult)
        {

        }

        public override AbstractNavigationAction Reverse(UnfinalizedSuperMetroidModel model)
        {
            EnableItemAction reverseAction = new EnableItemAction($"Undo action '{this.IntentDescription}'");
            TransferDataToReverseAbstractAction(reverseAction);

            return reverseAction;
        }
    }
}
