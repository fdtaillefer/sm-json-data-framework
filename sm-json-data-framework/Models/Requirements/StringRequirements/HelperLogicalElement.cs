﻿using sm_json_data_framework.Models.Helpers;
using sm_json_data_framework.Models.InGameStates;
using System;
using System.Collections.Generic;
using System.Text;

namespace sm_json_data_framework.Models.Requirements.StringRequirements
{
    /// <summary>
    /// A logical element that is fulfilled by fulfilling the requirements of an inner Helper.
    /// </summary>
    public class HelperLogicalElement : AbstractStringLogicalElement
    {
        private Helper Helper { get; set; }

        public HelperLogicalElement(Helper helper)
        {
            Helper = helper;
        }

        public override ExecutionResult Execute(SuperMetroidModel model, InGameState inGameState, int times = 1, bool usePreviousRoom = false)
        {
            return Helper.Requires.Execute(model, inGameState, times: times, usePreviousRoom: usePreviousRoom);
        }
    }
}
