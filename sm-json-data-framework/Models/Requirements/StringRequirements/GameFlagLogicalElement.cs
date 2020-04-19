using sm_json_data_framework.Models.GameFlags;
using sm_json_data_framework.Models.InGameStates;
using System;
using System.Collections.Generic;
using System.Text;

namespace sm_json_data_framework.Models.Requirements.StringRequirements
{
    /// <summary>
    /// A logical element that is fulfilled by previously activating an in-game flag.
    /// </summary>
    public class GameFlagLogicalElement : AbstractStringLogicalElement
    {
        private GameFlag GameFlag { get; set; }

        public GameFlagLogicalElement(GameFlag gameFlag)
        {
            GameFlag = gameFlag;
        }

        public override bool IsFulfilled(SuperMetroidModel model, InGameState inGameState, int times = 1, bool usePreviousRoom = false)
        {
            return inGameState.HasGameFlag(GameFlag);
        }

        public override InGameState AttemptFulfill(SuperMetroidModel model, InGameState inGameState, int times = 1, bool usePreviousRoom = false)
        {
            if(inGameState.HasGameFlag(GameFlag))
            {
                // Clone the In-game state to fulfill method contract
                return inGameState.Clone();
            }
            else
            {
                return null;
            }
        }
    }
}
