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
    /// A logical element which requires Samus to take a number of hits from spikes.
    /// </summary>
    public class SpikeHits : AbstractDamageNumericalValueLogicalElement<UnfinalizedSpikeHits, SpikeHits>
    {
        public SpikeHits(UnfinalizedSpikeHits sourceElement, Action<SpikeHits> mappingsInsertionCallback)
            : base(sourceElement, mappingsInsertionCallback)
        {

        }

        /// <summary>
        /// The number of spike hits that Samus must take.
        /// </summary>
        public int Hits => Value;

        public override int CalculateDamage(SuperMetroidModel model, ReadOnlyInGameState inGameState, int times = 1, int previousRoomCount = 0)
        {
            return model.Rules.CalculateEnvironmentalDamage(inGameState, model.Rules.SpikeDamage) * Hits * times;
        }

        public override int CalculateBestCastDamage(SuperMetroidRules rules)
        {
            return rules.CalculateBestCaseEnvironmentalDamage(rules.SpikeDamage * Hits, AppliedLogicalOptions.RemovedItems);
        }

        public override int CalculateWorstCastDamage(SuperMetroidRules rules)
        {
            return rules.CalculateWorstCaseEnvironmentalDamage(rules.SpikeDamage * Hits, AppliedLogicalOptions.StartConditions.StartingInventory);
        }

        public override IEnumerable<Item> GetDamageReducingItems(SuperMetroidModel model, ReadOnlyInGameState inGameState)
        {
            return model.Rules.GetEnvironmentalDamageReducingItems(model, inGameState);
        }

        protected override void PropagateLogicalOptions(ReadOnlyLogicalOptions logicalOptions, SuperMetroidRules rules)
        {
            // Nothing to do here
        }
    }

    public class UnfinalizedSpikeHits : AbstractUnfinalizedDamageNumericalValueLogicalElement<UnfinalizedSpikeHits, SpikeHits>
    {
        public UnfinalizedSpikeHits()
        {

        }

        public UnfinalizedSpikeHits(int hits) : base(hits)
        {

        }

        protected override SpikeHits CreateFinalizedElement(UnfinalizedSpikeHits sourceElement, Action<SpikeHits> mappingsInsertionCallback, ModelFinalizationMappings mappings)
        {
            return new SpikeHits(sourceElement, mappingsInsertionCallback);
        }
    }
}
