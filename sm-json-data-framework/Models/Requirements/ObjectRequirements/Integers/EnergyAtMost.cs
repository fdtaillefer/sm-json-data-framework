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
    /// </summary>
    public class EnergyAtMost : AbstractDamageNumericalValueLogicalElement<UnfinalizedEnergyAtMost, EnergyAtMost>
    {
        public EnergyAtMost(UnfinalizedEnergyAtMost sourceElement, Action<EnergyAtMost> mappingsInsertionCallback) 
            : base(sourceElement, mappingsInsertionCallback)
        {

        }

        public override int CalculateDamage(SuperMetroidModel model, ReadOnlyInGameState inGameState, int times = 1, int previousRoomCount = 0)
        {
            int currentRegularEnergy = inGameState.Resources.GetAmount(Items.RechargeableResourceEnum.RegularEnergy);
            // Don't take damage if we've already reached the threshold
            return Math.Max(0, currentRegularEnergy - Value);
        }

        public override IEnumerable<Item> GetDamageReducingItems(SuperMetroidModel model, ReadOnlyInGameState inGameState)
        {
            // This brings energy down to a specific level, and has no cares for damage reduction items
            return new Item[] { };
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
            // While always possible, this can cost energy
            return false;
        }
    }

    public class UnfinalizedEnergyAtMost : AbstractUnfinalizedDamageNumericalValueLogicalElement<UnfinalizedEnergyAtMost, EnergyAtMost>
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
