using sm_json_data_framework.InGameStates;
using sm_json_data_framework.Models.Requirements;
using System;
using System.Collections.Generic;
using System.Text;

namespace sm_json_data_framework.Models.Navigation
{
    /// <summary>
    /// An action that represents disabling an item in the player's inventory.
    /// The item stays in the inventory but becomes unusable until re-enabled.
    /// Fails if the item is not current present and enabled.
    /// </summary>
    public class DisableItemAction : AbstractNavigationAction
    {
        protected DisableItemAction(string intent) : base(intent)
        {

        }

        public DisableItemAction(string intent, SuperMetroidModel model, ReadOnlyInGameState initialInGameState, ExecutionResult executionResult) :
            base(intent, model, initialInGameState, executionResult)
        {

        }

        public override AbstractNavigationAction Reverse(SuperMetroidModel model)
        {
            DisableItemAction reverseAction = new DisableItemAction($"Undo action '{IntentDescription}'");
            TransferDataToReverseAbstractAction(reverseAction);

            return reverseAction;
        }
    }
}
