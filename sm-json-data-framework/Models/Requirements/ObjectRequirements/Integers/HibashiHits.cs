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
    /// A logical element which requires Samus to take a number of hits from the Norfair flame jets (known as Hibashi).
    /// </summary>
    public class HibashiHits : AbstractPunctualEnvironmentDamageLogicalElement<UnfinalizedHibashiHits, HibashiHits>
    {
        protected override PunctualEnvironmentDamageEnum EnvironmentDamageEnum => PunctualEnvironmentDamageEnum.HibashiHit;

        public HibashiHits(UnfinalizedHibashiHits sourceElement, Action<HibashiHits> mappingsInsertionCallback)
            : base(sourceElement, mappingsInsertionCallback)
        {

        }

        protected override void PropagateLogicalOptions(ReadOnlyLogicalOptions logicalOptions, SuperMetroidModel model)
        {
            // Nothing to do here
        }
    }

    public class UnfinalizedHibashiHits : AbstractUnfinalizedPunctualEnvironmentDamageLogicalElement<UnfinalizedHibashiHits, HibashiHits>
    {
        public UnfinalizedHibashiHits()
        {

        }

        public UnfinalizedHibashiHits(int hits) : base(hits)
        {

        }

        protected override HibashiHits CreateFinalizedElement(UnfinalizedHibashiHits sourceElement, Action<HibashiHits> mappingsInsertionCallback, ModelFinalizationMappings mappings)
        {
            return new HibashiHits(sourceElement, mappingsInsertionCallback);
        }
    }
}
