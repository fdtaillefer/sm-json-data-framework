﻿using sm_json_data_framework.InGameStates;
using sm_json_data_framework.Models.Requirements;
using System;
using System.Collections.Generic;
using System.Text;

namespace sm_json_data_framework.Models.Navigation
{
    /// <summary>
    /// An action that represents farming enemies off a spawner, to refill resources.
    /// </summary>
    public class FarmSpawnerAction : AbstractNavigationAction
    {
        protected FarmSpawnerAction(string intent) : base(intent)
        {

        }

        public FarmSpawnerAction(string intent, SuperMetroidModel model, ReadOnlyInGameState initialInGameState, ExecutionResult executionResult) :
            base(intent, model, initialInGameState, executionResult)
        {
            
        }

        public override AbstractNavigationAction Reverse(SuperMetroidModel model)
        {
            FarmSpawnerAction reverseAction = new FarmSpawnerAction($"Undo action '{this.IntentDescription}'");
            TransferDataToReverseAbstractAction(reverseAction);

            return reverseAction;
        }
    }
}
