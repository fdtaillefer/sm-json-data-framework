using sm_json_data_framework.Models.InGameStates;
using sm_json_data_framework.Models.Techs;
using sm_json_data_framework.Options;
using sm_json_data_framework.Rules;
using System;
using System.Collections.Generic;
using System.Text;

namespace sm_json_data_framework.Models.Requirements.StringRequirements
{
    /// <summary>
    /// A logical element that is never ever fulfilled.
    /// </summary>
    public class NeverLogicalElement : AbstractStringLogicalElement<UnfinalizedNeverLogicalElement, NeverLogicalElement>
    {
        public NeverLogicalElement()
        {

        }

        public NeverLogicalElement(UnfinalizedNeverLogicalElement sourceElement, Action<NeverLogicalElement> mappingsInsertionCallback)
            : base(sourceElement, mappingsInsertionCallback)
        {

        }

        protected override ExecutionResult ExecuteUseful(SuperMetroidModel model, ReadOnlyInGameState inGameState, int times = 1, int previousRoomCount = 0)
        {
            return null;
        }

        protected override void PropagateLogicalOptions(ReadOnlyLogicalOptions logicalOptions, SuperMetroidRules rules)
        {
            // Nothing to do here
        }

        protected override bool CalculateLogicallyNever(SuperMetroidRules rules)
        {
            // Never is always never
            return true;
        }

        protected override bool CalculateLogicallyAlways(SuperMetroidRules rules)
        {
            // Never is never always
            return false;
        }

        protected override bool CalculateLogicallyFree(SuperMetroidRules rules)
        {
            // Never is never possible, let alone free
            return false;
        }
    }

    public class UnfinalizedNeverLogicalElement : AbstractUnfinalizedStringLogicalElement<UnfinalizedNeverLogicalElement, NeverLogicalElement>
    {
        protected override NeverLogicalElement CreateFinalizedElement(UnfinalizedNeverLogicalElement sourceElement, Action<NeverLogicalElement> mappingsInsertionCallback, ModelFinalizationMappings mapping)
        {
            return new NeverLogicalElement(sourceElement, mappingsInsertionCallback);
        }
    }
}
