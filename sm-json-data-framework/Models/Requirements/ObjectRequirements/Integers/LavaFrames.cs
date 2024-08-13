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
    /// A logical element which requires Samus to spend some frames in lava.
    /// </summary>
    public class LavaFrames : AbstractDamageOverTimeLogicalElement<UnfinalizedLavaFrames, LavaFrames>
    {
        /// <summary>
        /// A multiplier to apply to lava frame requirements as a leniency, as per applied logical options.
        /// </summary>
        public decimal LavaLeniencyMultiplier => AppliedLogicalOptions.LavaLeniencyMultiplier;

        protected override DamageOverTimeEnum DotEnum => DamageOverTimeEnum.Lava;

        protected override decimal LeniencyMultiplier => LavaLeniencyMultiplier;

        public LavaFrames(int numberOfFrames) : base(numberOfFrames)
        {

        }

        public LavaFrames(UnfinalizedLavaFrames sourceElement, Action<LavaFrames> mappingsInsertionCallback) 
            : base(sourceElement, mappingsInsertionCallback)
        {

        }

        protected override void PropagateLogicalOptions(ReadOnlyLogicalOptions logicalOptions, SuperMetroidModel model)
        {
            // Nothing to do here
        }
    }

    public class UnfinalizedLavaFrames : AbstractUnfinalizedDamageOverTimeLogicalElement<UnfinalizedLavaFrames, LavaFrames>
    {
        public UnfinalizedLavaFrames()
        {

        }

        public UnfinalizedLavaFrames(int frames): base (frames)
        {
            
        }

        protected override LavaFrames CreateFinalizedElement(UnfinalizedLavaFrames sourceElement, Action<LavaFrames> mappingsInsertionCallback, ModelFinalizationMappings mappings)
        {
            return new LavaFrames(sourceElement, mappingsInsertionCallback);
        }
    }
}
