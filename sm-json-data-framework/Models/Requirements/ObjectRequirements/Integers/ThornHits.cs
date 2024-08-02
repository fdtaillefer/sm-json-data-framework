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
    /// A logical element which requires Samus to take a number of hits from the weaker spikes (mostly found in Brinstar).
    /// </summary>
    public class ThornHits : AbstractPunctualEnvironmentDamageLogicalElement<UnfinalizedThornHits, ThornHits>
    {
        protected override PunctualEnvironmentDamageEnum EnvironmentDamageEnum => PunctualEnvironmentDamageEnum.ThornHit;

        public ThornHits(UnfinalizedThornHits sourceElement, Action<ThornHits> mappingsInsertionCallback)
            : base(sourceElement, mappingsInsertionCallback)
        {

        }

        protected override void PropagateLogicalOptions(ReadOnlyLogicalOptions logicalOptions, SuperMetroidRules rules)
        {
            // Nothing to do here
        }
    }

    public class UnfinalizedThornHits : AbstractUnfinalizedPunctualEnvironmentDamageLogicalElement<UnfinalizedThornHits, ThornHits>
    {
        public UnfinalizedThornHits()
        {

        }

        public UnfinalizedThornHits(int hits) : base(hits)
        {

        }

        protected override ThornHits CreateFinalizedElement(UnfinalizedThornHits sourceElement, Action<ThornHits> mappingsInsertionCallback, ModelFinalizationMappings mappings)
        {
            return new ThornHits(sourceElement, mappingsInsertionCallback);
        }
    }
}
