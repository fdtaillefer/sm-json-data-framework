using sm_json_data_framework.Models.InGameStates;
using sm_json_data_framework.Models.Items;
using sm_json_data_framework.Models.Rooms;
using sm_json_data_framework.Models.Techs;
using sm_json_data_framework.Options;
using sm_json_data_framework.Rules;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Json.Serialization;

namespace sm_json_data_framework.Models.Requirements.ObjectRequirements.SubObjects
{
    /// <summary>
    /// A logical element which requires Samus to charge a blue suit and possibly use it to shinespark (if it has any shinespark frames).
    /// </summary>
    public class CanShineCharge : AbstractObjectLogicalElement<UnfinalizedCanShineCharge, CanShineCharge>, IRunway
    {

        /// <summary>
        /// Number of tiles the player is expected to be able save if stutter is possible on a runway, as per applied logical options.
        /// </summary>
        public decimal TilesSavedWithStutter => AppliedLogicalOptions.TilesSavedWithStutter;

        /// <summary>
        /// Smallest number of tiles the player is expected to be able to obtain a shine charge with (before applying stutter), as per applied logical options.
        /// </summary>
        public decimal TilesToShineCharge => AppliedLogicalOptions.TilesToShineCharge;

        /// <summary>
        /// Whether the player is logically expected to know how to shinespark.
        /// </summary>
        public bool CanShinespark => AppliedLogicalOptions.CanShinespark;

        public CanShineCharge(UnfinalizedCanShineCharge sourceElement, Action<CanShineCharge> mappingsInsertionCallback) : base(sourceElement, mappingsInsertionCallback)
        {
            Length = sourceElement.Length;
            EndingUpTiles = sourceElement.EndingUpTiles;
            GentleUpTiles = sourceElement.GentleUpTiles;
            GentleDownTiles = sourceElement.GentleDownTiles;
            SteepUpTiles = sourceElement.SteepUpTiles;
            SteepDownTiles = sourceElement.SteepDownTiles;
            StartingDownTiles = sourceElement.StartingDownTiles;
            OpenEnds = sourceElement.OpenEnds;
            ShinesparkFrames = sourceElement.ShinesparkFrames;
            ExcessShinesparkFrames = sourceElement.ExcessShinesparkFrames;
        }

        public int Length { get; }

        public int EndingUpTiles { get; }

        public int GentleUpTiles { get; }

        public int GentleDownTiles { get; }

        public int SteepUpTiles { get; }

        public int SteepDownTiles { get; }

        public int StartingDownTiles { get; }

        public int OpenEnds { get; }

        /// <summary>
        /// The duration (in frames) of the shinespark that goes alongside this CanShineCharge, if any. Can be 0 if no shinespark is involved.
        /// </summary>
        public int ShinesparkFrames { get; }

        /// <summary>
        /// The amount of shinespark frames that happen after the primary objective of a shinespark has been met.
        /// Those excess frames will consume energy if the energy is there, but the shinespark should be considered possible (neregy-wise) as long as
        /// there is enough energy for the non-excess frames.
        /// Can be 0 if no shinespark is involved.
        /// </summary>
        public int ExcessShinesparkFrames { get; }

        /// <summary>
        /// Indicates whether this CanShineCharge involves executing a shinespark.
        /// </summary>
        public bool MustShinespark => ShinesparkFrames > 0;

        protected override ExecutionResult ExecutePossible(SuperMetroidModel model, ReadOnlyInGameState inGameState, int times = 1, int previousRoomCount = 0)
        {
            // Always need the SpeedBooster to charge a bluesuit
            if (!inGameState.Inventory.HasSpeedBooster())
            {
                return null;
            }

            // The runway must be long enough to charge
            if (LogicalEffectiveRunwayLength < TilesToShineCharge)
            {
                return null;
            }

            // If we have enough energy for the shinespark to go through, consume the energy cost and return the result
            int energyNeeded = model.Rules.CalculateMinimumEnergyNeededForShinespark(ShinesparkFrames, ExcessShinesparkFrames, times: times);

            // Not calling IsResourceAvailable() because Samus only needs to have that much energy, not necessarily spend all of it
            if (inGameState.Resources.GetAmount(ConsumableResourceEnum.Energy) >= energyNeeded)
            {
                int energyCost = model.Rules.CalculateShinesparkDamage(inGameState, ShinesparkFrames, times: times);
                InGameState resultingState = inGameState.Clone();
                resultingState.ApplyConsumeResource(ConsumableResourceEnum.Energy, energyCost);
                ExecutionResult result = new ExecutionResult(resultingState);
                result.AddItemsInvolved(new Item[] { model.Items[SuperMetroidModel.SPEED_BOOSTER_NAME] });
                return result;
            }
            // If we don't have enough for the shinespark, we cannot do this
            else
            {
                return null;
            }
        }

        protected override void PropagateLogicalOptions(ReadOnlyLogicalOptions logicalOptions, SuperMetroidRules rules)
        {
            // Nothing to do here
        }

        protected override void UpdateLogicalProperties(SuperMetroidRules rules)
        {
            LogicalEffectiveRunwayLength = rules.CalculateEffectiveRunwayLength(this, TilesSavedWithStutter);
            base.UpdateLogicalProperties(rules);
        }

        /// <summary>
        /// The effective runway length of this CanShineCharge, given the current logical options.
        /// </summary>
        public decimal LogicalEffectiveRunwayLength { get; private set; }

        protected override bool CalculateLogicallyNever(SuperMetroidRules rules)
        {
            bool impossible = false;

            if (!AppliedLogicalOptions.IsSpeedBoosterInGame())
            {
                impossible = true;
            }

            if (MustShinespark)
            {
                if(!CanShinespark)
                {
                    impossible = true;
                }
                else
                {
                    // If the shinespark requires having more than the possible max energy, this is impossible
                    int? maxEnergy = AppliedLogicalOptions.MaxPossibleAmount(ConsumableResourceEnum.Energy);
                    if(maxEnergy != null && rules.CalculateMinimumEnergyNeededForShinespark(ShinesparkFrames, ExcessShinesparkFrames) > maxEnergy.Value)
                    {
                        impossible = true;
                    }
                }
            }

            if (LogicalEffectiveRunwayLength < TilesToShineCharge)
            {
                impossible = true;
            }

            return impossible;
        }

        protected override bool CalculateLogicallyAlways(SuperMetroidRules rules)
        {
            return CalculateLogicallyFree(rules);
        }

        protected override bool CalculateLogicallyFree(SuperMetroidRules rules)
        {
            if (CalculateLogicallyNever(rules))
            {
                return false;
            }

            // If SpeedBooster isn't in-game and always available then this is not free
            if (!AppliedLogicalOptions.IsSpeedBoosterInGame() || !AppliedLogicalOptions.StartConditions.StartingInventory.HasSpeedBooster())
            {
                return false;
            }

            // Shinespark frames need energy, which is not free
            if (MustShinespark)
            {
                return false;
            }

            // If the runway is too short to shine charge, this is actually never possible
            if (LogicalEffectiveRunwayLength < TilesToShineCharge)
            {
                return false;
            }

            // If we can use the runway, always have SpeedBooster, and don't need to shinespark, then using this is free
            return true;
        }
    }

    public class UnfinalizedCanShineCharge : AbstractUnfinalizedObjectLogicalElement<UnfinalizedCanShineCharge, CanShineCharge>, IRunway
    {
        public int Length { get => UsedTiles; }

        public int EndingUpTiles { get => 0; }

        public int UsedTiles { get; set; }

        public int GentleUpTiles { get; set; } = 0;

        public int GentleDownTiles { get; set; } = 0;

        public int SteepUpTiles { get; set; } = 0;

        public int SteepDownTiles { get; set; } = 0;

        public int StartingDownTiles { get; set; } = 0;

        public int OpenEnds { get; set; } = 0;

        public int ShinesparkFrames { get; set; }

        public int ExcessShinesparkFrames { get; set; }

        protected override CanShineCharge CreateFinalizedElement(UnfinalizedCanShineCharge sourceElement, Action<CanShineCharge> mappingsInsertionCallback, ModelFinalizationMappings mappings)
        {
            return new CanShineCharge(sourceElement, mappingsInsertionCallback);
        }

        public override IEnumerable<string> InitializeReferencedLogicalElementProperties(UnfinalizedSuperMetroidModel model, UnfinalizedRoom room)
        {
            // No properties need to be handled here
            return Enumerable.Empty<string>();
        }
    }
}
