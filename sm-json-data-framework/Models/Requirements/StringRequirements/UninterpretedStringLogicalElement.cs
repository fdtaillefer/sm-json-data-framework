using sm_json_data_framework.Models.InGameStates;
using sm_json_data_framework.Options;
using System;
using System.Collections.Generic;
using System.Text;

namespace sm_json_data_framework.Models.Requirements.StringRequirements
{
    /// <summary>
    /// A logical element that is just a raw, uninterpreted string, directly from a json file.
    /// </summary>
    public class UninterpretedStringLogicalElement : AbstractStringLogicalElement<UnfinalizedUninterpretedStringLogicalElement, UninterpretedStringLogicalElement>
    {
        public UninterpretedStringLogicalElement(UnfinalizedUninterpretedStringLogicalElement innerElement, Action<UninterpretedStringLogicalElement> mappingsInsertionCallback) : base(innerElement, mappingsInsertionCallback)
        {

        }
    }

    public class UnfinalizedUninterpretedStringLogicalElement: AbstractUnfinalizedStringLogicalElement<UnfinalizedUninterpretedStringLogicalElement, UninterpretedStringLogicalElement>
    {
        public UnfinalizedUninterpretedStringLogicalElement(string stringValue)
        {
            StringValue = stringValue;
        }

        public string StringValue { get; set; }

        protected override UninterpretedStringLogicalElement CreateFinalizedElement(UnfinalizedUninterpretedStringLogicalElement sourceElement, Action<UninterpretedStringLogicalElement> mappingsInsertionCallback, ModelFinalizationMappings mapping)
        {
            return new UninterpretedStringLogicalElement(sourceElement, mappingsInsertionCallback);
        }

        protected override bool ApplyLogicalOptionsEffects(ReadOnlyLogicalOptions logicalOptions)
        {
            // Nothing in logical options can alter this
            return false;
        }

        public override bool IsNever()
        {
            return false;
        }

        protected override ExecutionResult ExecuteUseful(SuperMetroidModel model, ReadOnlyInGameState inGameState, int times = 1, int previousRoomCount = 0)
        {
            throw new NotImplementedException("Raw string logical elements should be replaced before being evaluated");
        }
    }
}
