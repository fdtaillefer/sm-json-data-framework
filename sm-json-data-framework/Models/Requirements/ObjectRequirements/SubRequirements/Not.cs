using sm_json_data_framework.Models.InGameStates;
using sm_json_data_framework.Options;
using sm_json_data_framework.Rules;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace sm_json_data_framework.Models.Requirements.ObjectRequirements.SubRequirements
{
    /// <summary>
    /// A logical element that is fulfilled by being unable to fulfill its inner logical elements.
    /// </summary>
    public class Not : AbstractObjectLogicalElementWithSubRequirements<UnfinalizedNot, Not>
    {
        public Not(UnfinalizedNot sourceElement, Action<Not> mappingsInsertionCallback, ModelFinalizationMappings mappings)
            : base(sourceElement, mappingsInsertionCallback, mappings)
        {

        }

        protected override ExecutionResult ExecutePossible(SuperMetroidModel model, ReadOnlyInGameState inGameState, int times = 1, int previousRoomCount = 0)
        {
            if (LogicalRequirements.Execute(model, inGameState, times: times, previousRoomCount: previousRoomCount) == null)
            {
                // If we are unable to fulfill the inner logical elements, then the Not is successful. Clone the inGameState to respect the contract.
                return new ExecutionResult(inGameState.Clone());
            }
            else
            {
                return null;
            }
        }

        protected override void PropagateLogicalOptions(ReadOnlyLogicalOptions logicalOptions, SuperMetroidRules rules)
        {
            LogicalRequirements.ApplyLogicalOptions(logicalOptions, rules);
        }

        protected override bool CalculateLogicallyNever(SuperMetroidRules rules)
        {
            // This is never possible if the inner requirements are always possible
            return LogicalRequirements.LogicallyAlways;
        }

        protected override bool CalculateLogicallyAlways(SuperMetroidRules rules)
        {
            // This is always possible if the inner requirements are impossible
            return LogicalRequirements.LogicallyNever;
        }

        protected override bool CalculateLogicallyFree(SuperMetroidRules rules)
        {
            // This is always free if the inner requirements are impossible
            return LogicalRequirements.LogicallyNever;
        }
    }

    public class UnfinalizedNot : AbstractUnfinalizedObjectLogicalElementWithSubRequirements<UnfinalizedNot, Not>
    {
        public UnfinalizedNot()
        {

        }

        public UnfinalizedNot(UnfinalizedLogicalRequirements logicalRequirements) : base(logicalRequirements)
        {

        }

        protected override Not CreateFinalizedElement(UnfinalizedNot sourceElement, Action<Not> mappingsInsertionCallback, ModelFinalizationMappings mappings)
        {
            return new Not(sourceElement, mappingsInsertionCallback, mappings);
        }
    }
}
