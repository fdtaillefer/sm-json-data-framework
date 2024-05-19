using sm_json_data_framework.Models.InGameStates;
using sm_json_data_framework.Options;
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

        public NeverLogicalElement(UnfinalizedNeverLogicalElement innerElement, Action<NeverLogicalElement> mappingsInsertionCallback)
            : base(innerElement, mappingsInsertionCallback)
        {

        }

        public override bool IsNever()
        {
            return true;
        }

        protected override ExecutionResult ExecuteUseful(SuperMetroidModel model, ReadOnlyInGameState inGameState, int times = 1, int previousRoomCount = 0)
        {
            return null;
        }

        protected override bool PropagateLogicalOptions(ReadOnlyLogicalOptions logicalOptions)
        {
            // Nothing in logical options can alter this
            return false;
        }

        protected override bool CalculateLogicallyNever()
        {
            // Never is always never
            return true;
        }
    }

    public class UnfinalizedNeverLogicalElement : AbstractUnfinalizedStringLogicalElement<UnfinalizedNeverLogicalElement, NeverLogicalElement>
    {
        protected override NeverLogicalElement CreateFinalizedElement(UnfinalizedNeverLogicalElement sourceElement, Action<NeverLogicalElement> mappingsInsertionCallback, ModelFinalizationMappings mapping)
        {
            return new NeverLogicalElement(sourceElement, mappingsInsertionCallback);
        }

        public override bool IsNever()
        {
            return true;
        }
    }
}
