using sm_json_data_framework.Models.InGameStates;
using sm_json_data_framework.Models.Requirements;
using sm_json_data_framework.Models.Rooms;
using System;
using System.Collections.Generic;
using System.Text;

namespace sm_json_data_framework.Models.Navigation
{
    /// <summary>
    /// An action that represents interacting with the node the player is at.
    /// </summary>
    public class InteractWithNodeAction: AbstractNavigationAction
    {
        protected InteractWithNodeAction(string intent) : base(intent)
        {

        }

        public InteractWithNodeAction(string intent, UnfinalizedSuperMetroidModel model, ReadOnlyUnfinalizedInGameState initialInGameState, UnfinalizedExecutionResult executionResult) :
            base(intent, model, initialInGameState, executionResult)
        {
            
        }

        public override AbstractNavigationAction Reverse(UnfinalizedSuperMetroidModel model)
        {
            InteractWithNodeAction reverseAction = new InteractWithNodeAction($"Undo action '{this.IntentDescription}'");
            TransferDataToReverseAbstractAction(reverseAction);

            return reverseAction;
        }
    }
}
