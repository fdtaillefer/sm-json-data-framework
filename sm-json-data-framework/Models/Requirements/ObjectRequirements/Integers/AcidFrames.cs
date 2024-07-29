using sm_json_data_framework.InGameStates.EnergyManagement;
using sm_json_data_framework.Options;
using sm_json_data_framework.Rules;
using System;
using System.Collections.Generic;
using System.Text;

namespace sm_json_data_framework.Models.Requirements.ObjectRequirements.Integers
{
    /// <summary>
    /// A logical element which requires Samus to spend some frames in acid.
    /// </summary>
    public class AcidFrames : AbstractDamageOverTimeLogicalElement<UnfinalizedAcidFrames, AcidFrames>
    {
        /// <summary>
        /// A multiplier to apply to acid frame requirements as a leniency, as per applied logical options.
        /// </summary>
        public decimal AcidLeniencyMultiplier => AppliedLogicalOptions.AcidLeniencyMultiplier;

        protected override DamageOverTimeEnum DotEnum => DamageOverTimeEnum.Acid;

        protected override decimal LeniencyMultiplier => AcidLeniencyMultiplier;

        public AcidFrames(int numberOfFrames): base(numberOfFrames)
        {

        }

        public AcidFrames(UnfinalizedAcidFrames sourceElement, Action<AcidFrames> mappingsInsertionCallback) : base(sourceElement, mappingsInsertionCallback)
        {

        }

        protected override void PropagateLogicalOptions(ReadOnlyLogicalOptions logicalOptions, SuperMetroidRules rules)
        {
            // Nothing to do here
        }
    }

    public class UnfinalizedAcidFrames : AbstractUnfinalizedDamageOverTimeLogicalElement<UnfinalizedAcidFrames, AcidFrames>
    {
        public UnfinalizedAcidFrames()
        {

        }

        public UnfinalizedAcidFrames(int frames) : base(frames)
        {
            
        }

        protected override AcidFrames CreateFinalizedElement(UnfinalizedAcidFrames sourceElement, Action<AcidFrames> mappingsInsertionCallback, ModelFinalizationMappings mappings)
        {
            return new AcidFrames(sourceElement, mappingsInsertionCallback);
        }
    }
}
