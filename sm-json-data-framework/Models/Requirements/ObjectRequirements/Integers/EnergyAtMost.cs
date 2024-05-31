using sm_json_data_framework.Models.InGameStates;
using sm_json_data_framework.Models.Items;
using sm_json_data_framework.Options;
using sm_json_data_framework.Rules;
using System;
using System.Collections.Generic;
using System.Text;

namespace sm_json_data_framework.Models.Requirements.ObjectRequirements.Integers
{
    /// <summary>
    /// A logical element which requires Samus to take damage down to a fixed amount. It can always be fulfilled, but the cost can vary.
    /// This ignores reserves.
    /// </summary>
    public class EnergyAtMost : AbstractObjectLogicalElementWithNumericalIntegerValue<UnfinalizedEnergyAtMost, EnergyAtMost>
    {
        public EnergyAtMost(UnfinalizedEnergyAtMost sourceElement, Action<EnergyAtMost> mappingsInsertionCallback) 
            : base(sourceElement, mappingsInsertionCallback)
        {

        }

        protected override ExecutionResult ExecuteUseful(SuperMetroidModel model, ReadOnlyInGameState inGameState, int times = 1, int previousRoomCount = 0)
        {
            int currentRegularEnergy = inGameState.Resources.GetAmount(Items.RechargeableResourceEnum.RegularEnergy);
            // Don't take damage if we've already reached the threshold
            int damage = Math.Max(0, currentRegularEnergy - Value);


            var resultingState = inGameState.Clone();
            resultingState.ApplyConsumeResource(ConsumableResourceEnum.Energy, damage);
            ExecutionResult result = new ExecutionResult(resultingState);
            return result;
        }

        protected override void PropagateLogicalOptions(ReadOnlyLogicalOptions logicalOptions, SuperMetroidRules rules)
        {
            // Nothing to do here
        }

        protected override bool CalculateLogicallyNever(SuperMetroidRules rules)
        {
            // This is always possible, by definition
            return false;
        }

        protected override bool CalculateLogicallyAlways(SuperMetroidRules rules)
        {
            // This is always possible, by definition - though it's not always free
            return true;
        }

        protected override bool CalculateLogicallyFree(SuperMetroidRules rules)
        {
            // This could be free if the highest non-reserve energy we can get is still no more than the threshold
            int? maxEnergy = AppliedLogicalOptions.MaxPossibleAmount(RechargeableResourceEnum.RegularEnergy);
            // We can't check that if the max possible energy isn't provided
            if (maxEnergy == null)
            {
                return false;
            }
            return maxEnergy <= Value;
        }
    }

    public class UnfinalizedEnergyAtMost : AbstractUnfinalizedObjectLogicalElementWithNumericalIntegerValue<UnfinalizedEnergyAtMost, EnergyAtMost>
    {
        public UnfinalizedEnergyAtMost()
        {

        }

        public UnfinalizedEnergyAtMost(int energy) : base(energy)
        {

        }

        protected override EnergyAtMost CreateFinalizedElement(UnfinalizedEnergyAtMost sourceElement, Action<EnergyAtMost> mappingsInsertionCallback, ModelFinalizationMappings mappings)
        {
            return new EnergyAtMost(sourceElement, mappingsInsertionCallback);
        }
    }
}
