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
    public class SamusEaterFrames : AbstractDamageNumericalValueLogicalElement<UnfinalizedSamusEaterFrames, SamusEaterFrames>
    {
        public SamusEaterFrames(UnfinalizedSamusEaterFrames sourceElement, Action<SamusEaterFrames> mappingsInsertionCallback) : base(sourceElement, mappingsInsertionCallback)
        {

        }

        /// <summary>
        /// The number of frames that Samus must spend in a Samus eater.
        /// </summary>
        public int Frames => Value;

        public override int CalculateDamage(SuperMetroidModel model, ReadOnlyInGameState inGameState, int times = 1, int previousRoomCount = 0)
        {
            return model.Rules.CalculateSamusEaterDamage(inGameState, Frames) * times;
        }

        public override int CalculateBestCastDamage(SuperMetroidRules rules)
        {
            return rules.CalculateBestCaseSamusEaterDamage(Frames, AppliedLogicalOptions.RemovedItems);
        }

        public override int CalculateWorstCastDamage(SuperMetroidRules rules)
        {
            return rules.CalculateWorstCaseSamusEaterDamage(Frames, AppliedLogicalOptions.StartConditions.StartingInventory);
        }

        public override IEnumerable<Item> GetDamageReducingItems(SuperMetroidModel model, ReadOnlyInGameState inGameState)
        {
            return model.Rules.GetSamusEaterDamageReducingItems(model, inGameState);
        }

        protected override void PropagateLogicalOptions(ReadOnlyLogicalOptions logicalOptions, SuperMetroidRules rules)
        {
            // Nothing to do here
        }
    }

    public class UnfinalizedSamusEaterFrames : AbstractUnfinalizedDamageNumericalValueLogicalElement<UnfinalizedSamusEaterFrames, SamusEaterFrames>
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
