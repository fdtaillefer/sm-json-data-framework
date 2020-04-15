﻿using sm_json_data_framework.Models.InGameStates;
using System;
using System.Collections.Generic;
using System.Text;

namespace sm_json_data_framework.Models.Requirements.StringRequirements
{
    public class RawStringLogicalElement: AbstractStringLogicalElement
    {
        public RawStringLogicalElement(string stringValue)
        {
            StringValue = stringValue;
        }

        public string StringValue { get; set; }

        public override bool IsFulfilled(SuperMetroidModel model, InGameState inGameState, int times = 1, bool usePreviousRoom = false)
        {
            throw new NotImplementedException("Raw string logical elements should be replaced before being evaluated");
        }
    }
}
