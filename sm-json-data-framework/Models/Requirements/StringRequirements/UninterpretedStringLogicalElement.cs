using sm_json_data_framework.Models.InGameStates;
using System;
using System.Collections.Generic;
using System.Text;

namespace sm_json_data_framework.Models.Requirements.StringRequirements
{
    /// <summary>
    /// A logical element that is just a raw, uninterpreted string, directly from a json file.
    /// </summary>
    public class UninterpretedStringLogicalElement: AbstractStringLogicalElement
    {
        public UninterpretedStringLogicalElement(string stringValue)
        {
            StringValue = stringValue;
        }

        public string StringValue { get; set; }

        public override bool IsNever()
        {
            return false;
        }

        public override ExecutionResult Execute(SuperMetroidModel model, ReadOnlyInGameState inGameState, int times = 1, int previousRoomCount = 0)
        {
            throw new NotImplementedException("Raw string logical elements should be replaced before being evaluated");
        }
    }
}
