using sm_json_data_framework.EnergyManagement;
using sm_json_data_framework.InGameStates;
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
    public class SpikeHits : AbstractPunctualEnvironmentDamageLogicalElement<UnfinalizedSpikeHits, SpikeHits>
    {
        protected override PunctualEnvironmentDamageEnum EnvironmentDamageEnum => PunctualEnvironmentDamageEnum.SpikeHit;

        public SpikeHits(UnfinalizedSpikeHits sourceElement, Action<SpikeHits> mappingsInsertionCallback)
            : base(sourceElement, mappingsInsertionCallback)
        {

        }

        protected override void PropagateLogicalOptions(ReadOnlyLogicalOptions logicalOptions, SuperMetroidRules rules)
        {
            // Nothing to do here
        }
    }

    public class UnfinalizedSpikeHits : AbstractUnfinalizedPunctualEnvironmentDamageLogicalElement<UnfinalizedSpikeHits, SpikeHits>
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
