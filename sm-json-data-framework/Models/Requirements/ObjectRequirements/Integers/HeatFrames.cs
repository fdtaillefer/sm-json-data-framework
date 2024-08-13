using sm_json_data_framework.EnergyManagement;
using sm_json_data_framework.Options;
using sm_json_data_framework.Rules;
using System;
using System.Collections.Generic;
using System.Text;

namespace sm_json_data_framework.Models.Requirements.ObjectRequirements.Integers
{
    /// <summary>
    /// A logical element which requires Samus to spend some frames in a heated room.
    /// </summary>
    public class HeatFrames : AbstractDamageOverTimeLogicalElement<UnfinalizedHeatFrames, HeatFrames>
    {
        /// <summary>
        /// A multiplier to apply to heat frame requirements as a leniency, as per applied logical options.
        /// </summary>
        public decimal HeatLeniencyMultiplier => AppliedLogicalOptions.HeatLeniencyMultiplier;

        protected override DamageOverTimeEnum DotEnum => DamageOverTimeEnum.Heat;

        protected override decimal LeniencyMultiplier => HeatLeniencyMultiplier;

        public HeatFrames(int numberOfFrames) : base(numberOfFrames)
        {

        }

        public HeatFrames(UnfinalizedHeatFrames sourceElement, Action<HeatFrames> mappingsInsertionCallback)
            : base(sourceElement, mappingsInsertionCallback)
        {

        }

        protected override void PropagateLogicalOptions(ReadOnlyLogicalOptions logicalOptions, SuperMetroidModel model)
        {
            // Nothing to do here
        }
    }

    public class UnfinalizedHeatFrames : AbstractUnfinalizedDamageOverTimeLogicalElement<UnfinalizedHeatFrames, HeatFrames>
    {
        public UnfinalizedHeatFrames()
        {

        }

        public UnfinalizedHeatFrames(int frames): base(frames)
        {
            
        }

        protected override HeatFrames CreateFinalizedElement(UnfinalizedHeatFrames sourceElement, Action<HeatFrames> mappingsInsertionCallback, ModelFinalizationMappings mappings)
        {
            return new HeatFrames(sourceElement, mappingsInsertionCallback);
        }
    }
}
