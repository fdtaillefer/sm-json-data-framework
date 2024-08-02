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
    /// A logical element which requires Samus to spend some frames grappled to one of Draygon's turrets.
    /// </summary>
    public class DraygonElectricityFrames : AbstractDamageOverTimeLogicalElement<UnfinalizedDraygonElectricityFrames, DraygonElectricityFrames>
    {
        protected override DamageOverTimeEnum DotEnum => DamageOverTimeEnum.GrappleElectricity;

        // No leniency for electricity
        protected override decimal LeniencyMultiplier => 1;

        public DraygonElectricityFrames(UnfinalizedDraygonElectricityFrames sourceElement, Action<DraygonElectricityFrames> mappingsInsertionCallback)
            : base(sourceElement, mappingsInsertionCallback)
        {

        }

        protected override void PropagateLogicalOptions(ReadOnlyLogicalOptions logicalOptions, SuperMetroidRules rules)
        {
            // Nothing to do here
        }
    }

    public class UnfinalizedDraygonElectricityFrames : AbstractUnfinalizedDamageOverTimeLogicalElement<UnfinalizedDraygonElectricityFrames, DraygonElectricityFrames>
    {
        public UnfinalizedDraygonElectricityFrames()
        {

        }

        public UnfinalizedDraygonElectricityFrames(int frames) : base(frames)
        {

        }

        protected override DraygonElectricityFrames CreateFinalizedElement(UnfinalizedDraygonElectricityFrames sourceElement, Action<DraygonElectricityFrames> mappingsInsertionCallback, ModelFinalizationMappings mappings)
        {
            return new DraygonElectricityFrames(sourceElement, mappingsInsertionCallback);
        }
    }
}
