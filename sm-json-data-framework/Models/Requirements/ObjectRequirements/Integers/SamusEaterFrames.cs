using sm_json_data_framework.EnergyManagement;
using sm_json_data_framework.InGameStates;
using sm_json_data_framework.Models.Items;
using sm_json_data_framework.Options;
using sm_json_data_framework.Rules;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace sm_json_data_framework.Models.Requirements.ObjectRequirements.Integers
{
    /// <summary>
    /// A logical element which requires Samus to spend some frames in a Samus eater.
    /// </summary>
    public class SamusEaterFrames : AbstractDamageOverTimeLogicalElement<UnfinalizedSamusEaterFrames, SamusEaterFrames>
    {
        protected override DamageOverTimeEnum DotEnum => DamageOverTimeEnum.SamusEater;

        // No leniency for Samus eaters
        protected override decimal LeniencyMultiplier => 1;

        public SamusEaterFrames(UnfinalizedSamusEaterFrames sourceElement, Action<SamusEaterFrames> mappingsInsertionCallback) : base(sourceElement, mappingsInsertionCallback)
        {

        }

        protected override void PropagateLogicalOptions(ReadOnlyLogicalOptions logicalOptions, SuperMetroidModel model)
        {
            // Nothing to do here
        }
    }

    public class UnfinalizedSamusEaterFrames : AbstractUnfinalizedDamageOverTimeLogicalElement<UnfinalizedSamusEaterFrames, SamusEaterFrames>
    {
        public UnfinalizedSamusEaterFrames()
        {

        }

        public UnfinalizedSamusEaterFrames(int frames) : base(frames)
        {

        }

        protected override SamusEaterFrames CreateFinalizedElement(UnfinalizedSamusEaterFrames sourceElement, Action<SamusEaterFrames> mappingsInsertionCallback, ModelFinalizationMappings mappings)
        {
            return new SamusEaterFrames(sourceElement, mappingsInsertionCallback);
        }
    }
}
