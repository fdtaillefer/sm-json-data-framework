using sm_json_data_framework.InGameStates;
using sm_json_data_framework.Options;
using sm_json_data_framework.Rules;
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
        public And(UnfinalizedAnd sourceElement, Action<And> mappingsInsertionCallback, ModelFinalizationMappings mappings)
            : base(sourceElement, mappingsInsertionCallback, mappings)
        {

        }

        protected override ExecutionResult ExecutePossible(SuperMetroidModel model, ReadOnlyInGameState inGameState, int times = 1, int previousRoomCount = 0)
        {
            return LogicalRequirements.Execute(model, inGameState, times: times, previousRoomCount: previousRoomCount);
        }

        protected override void PropagateLogicalOptions(ReadOnlyLogicalOptions logicalOptions, SuperMetroidModel model)
        {
            LogicalRequirements.ApplyLogicalOptions(logicalOptions, model);
        }

        protected override bool CalculateLogicallyNever(SuperMetroidModel model)
        {
            // Delegate to requirements
            return LogicalRequirements.LogicallyNever;
        }

        protected override bool CalculateLogicallyAlways(SuperMetroidModel model)
        {
            // Delegate to requirements
            return LogicalRequirements.LogicallyAlways;
        }

        protected override bool CalculateLogicallyFree(SuperMetroidModel model)
        {
            // Delegate to requirements
            return LogicalRequirements.LogicallyFree;
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
    }
}
