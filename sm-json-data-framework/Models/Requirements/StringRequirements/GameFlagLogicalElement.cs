using sm_json_data_framework.Models.GameFlags;
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
    }
}
