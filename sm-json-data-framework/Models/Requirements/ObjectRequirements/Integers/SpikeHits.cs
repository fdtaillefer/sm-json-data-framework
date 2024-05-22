using sm_json_data_framework.Models.InGameStates;
using sm_json_data_framework.Models.Items;
using sm_json_data_framework.Options;
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
        public SpikeHits(UnfinalizedSpikeHits innerElement, Action<SpikeHits> mappingsInsertionCallback)
            : base(innerElement, mappingsInsertionCallback)
        {

        }

        public override int CalculateDamage(SuperMetroidModel model, ReadOnlyInGameState inGameState, int times = 1, int previousRoomCount = 0)
        {
            return model.Rules.CalculateEnvironmentalDamage(inGameState, model.Rules.SpikeDamage) * Value * times;
        }

        public override IEnumerable<Item> GetDamageReducingItems(SuperMetroidModel model, ReadOnlyInGameState inGameState)
        {
            return model.Rules.GetEnvironmentalDamageReducingItems(model, inGameState);
        }

        protected override void PropagateLogicalOptions(ReadOnlyLogicalOptions logicalOptions)
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
