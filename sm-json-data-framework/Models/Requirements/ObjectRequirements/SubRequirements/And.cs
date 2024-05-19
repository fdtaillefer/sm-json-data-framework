using sm_json_data_framework.Models.InGameStates;
using sm_json_data_framework.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace sm_json_data_framework.Models.Requirements.ObjectRequirements.SubRequirements
{
    /// <summary>
    /// A logical element that is fulfilled by fulfilling all of its inner logical elements.
    /// </summary>
    public class And : AbstractObjectLogicalElementWithSubRequirements<UnfinalizedAnd, And>
    {
        public And(UnfinalizedAnd innerElement, Action<And> mappingsInsertionCallback, ModelFinalizationMappings mappings)
            : base(innerElement, mappingsInsertionCallback, mappings)
        {

        }

        public override bool IsNever()
        {
            return LogicalRequirements.IsNever();
        }

        protected override ExecutionResult ExecuteUseful(SuperMetroidModel model, ReadOnlyInGameState inGameState, int times = 1, int previousRoomCount = 0)
        {
            return LogicalRequirements.Execute(model, inGameState, times: times, previousRoomCount: previousRoomCount);
        }

        protected override bool PropagateLogicalOptions(ReadOnlyLogicalOptions logicalOptions)
        {
            LogicalRequirements.ApplyLogicalOptions(logicalOptions);

            // Since this is an And, the default behavior of the internal LogicalRequirements matches our purposes
            return LogicalRequirements.UselessByLogicalOptions;
        }

        protected override bool CalculateLogicallyNever()
        {
            // Delegate to requirements
            return LogicalRequirements.LogicallyNever;
        }

        protected override bool CalculateLogicallyAlways()
        {
            // Delegate to requirements
            return LogicalRequirements.LogicallyAlways;
        }
    }

    public class UnfinalizedAnd : AbstractUnfinalizedObjectLogicalElementWithSubRequirements<UnfinalizedAnd, And>
    {
        public UnfinalizedAnd()
        {

        }

        public UnfinalizedAnd(UnfinalizedLogicalRequirements logicalRequirements): base(logicalRequirements)
        {
            
        }

        protected override And CreateFinalizedElement(UnfinalizedAnd sourceElement, Action<And> mappingsInsertionCallback, ModelFinalizationMappings mappings)
        {
            return new And(sourceElement, mappingsInsertionCallback, mappings);
        }

        public override bool IsNever()
        {
            return LogicalRequirements.IsNever();
        }
    }
}
