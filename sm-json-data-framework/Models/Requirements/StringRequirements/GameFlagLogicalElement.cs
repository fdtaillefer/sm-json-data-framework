using sm_json_data_framework.Models.GameFlags;
using sm_json_data_framework.Models.InGameStates;
using System;
using System.Collections.Generic;
using System.Text;

namespace sm_json_data_framework.Models.Requirements.StringRequirements
{
    public class GameFlagLogicalElement : AbstractStringLogicalElement
    {
        private GameFlag GameFlag { get; set; }

        public GameFlagLogicalElement(GameFlag gameFlag)
        {
            GameFlag = gameFlag;
        }

        public override bool IsFulfilled(SuperMetroidModel model, InGameState inGameState, bool usePreviousRoom = false)
        {
            return inGameState.HasGameFlag(GameFlag);
        }
    }
}
