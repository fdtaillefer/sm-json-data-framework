using sm_json_data_framework.Models.InGameStates;
using sm_json_data_framework.Models.Items;
using sm_json_data_framework.Models.Rooms;
using sm_json_data_framework.Models.Techs;
using sm_json_data_framework.Options;
using sm_json_data_framework.Rules;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;

namespace sm_json_data_framework.Models.Requirements.ObjectRequirements.SubObjects
{
    /// <summary>
    /// A logical element which requires Samus to charge a blue suit and possibly use it to shinespark (if it has any shinespark frames).
    /// </summary>
    public class CanShineCharge : AbstractObjectLogicalElement<UnfinalizedCanShineCharge, CanShineCharge>, IRunway
    {
        private UnfinalizedCanShineCharge InnerElement {get;set;}
        public CanShineCharge(UnfinalizedCanShineCharge innerElement, Action<CanShineCharge> mappingsInsertionCallback) : base(innerElement, mappingsInsertionCallback)
        {
            InnerElement = innerElement;
        }

        public int Length { get { return InnerElement.Length; } }

        public int EndingUpTiles { get { return InnerElement.EndingUpTiles; } }

        public int GentleUpTiles { get { return InnerElement.GentleUpTiles; } }

        public int GentleDownTiles { get { return InnerElement.GentleDownTiles; } }

        public int SteepUpTiles { get { return InnerElement.SteepUpTiles; } }

        public int SteepDownTiles { get { return InnerElement.SteepDownTiles; } }

        public int StartingDownTiles { get { return InnerElement.StartingDownTiles; } }

        public int OpenEnds { get { return InnerElement.OpenEnds; } }

        /// <summary>
        /// The duration (in frames) of the shinespark that goes alongside this CanShineCharge, if any. Can be 0 if no shinespark is involved.
        /// </summary>
        public int ShinesparkFrames { get { return InnerElement.ShinesparkFrames; } }

        /// <summary>
        /// Indicates whether this CanShineCharge involves executing a shinespark.
        /// </summary>
        public bool MustShinespark { get { return InnerElement.MustShinespark; } }

        public override bool IsNever()
        {
            return false;
        }

        protected override ExecutionResult ExecuteUseful(SuperMetroidModel model, ReadOnlyInGameState inGameState, int times = 1, int previousRoomCount = 0)
        {
            // Always need the SpeedBooster to charge a bluesuit
            if (!inGameState.Inventory.HasSpeedBooster())
            {
                return null;
            }

            // The runway must be long enough to charge
            if (model.Rules.CalculateEffectiveRunwayLength(this, InnerElement.TilesSavedWithStutter) < InnerElement.TilesToShineCharge)
            {
                return null;
            }

            // If we have enough energy for the shinespark to go through, consume the energy cost and return the result
            int energyNeeded = model.Rules.CalculateEnergyNeededForShinespark(ShinesparkFrames, times: times);

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
    }

    public class UnfinalizedCanShineCharge : AbstractUnfinalizedObjectLogicalElement<UnfinalizedCanShineCharge, CanShineCharge>, IRunway
    {
        [JsonIgnore]
        public int Length { get => UsedTiles; }

        [JsonIgnore]
        public int EndingUpTiles { get => 0; }

        public int UsedTiles { get; set; }

        public int GentleUpTiles { get; set; } = 0;

        public int GentleDownTiles { get; set; } = 0;

        public int SteepUpTiles { get; set; } = 0;

        public int SteepDownTiles { get; set; } = 0;

        public int StartingDownTiles { get; set; } = 0;

        [JsonPropertyName("openEnd")]
        public int OpenEnds { get; set; } = 0;

        public int ShinesparkFrames { get; set; }

        /// <summary>
        /// Indicates whether this CanShineCharge involves executing a shinespark.
        /// </summary>
        public bool MustShinespark { get { return ShinesparkFrames > 0; } }

        /// <summary>
        /// Number of tiles the player is expected to be able save if stutter is possible on a runway, as per applied logical options.
        /// </summary>
        public decimal TilesSavedWithStutter { get; private set; } = LogicalOptions.DefaultTilesSavedWithStutter;

        /// <summary>
        /// Smallest number of tiles the player is expected to be able to obtain a shine charge with (before applying stutter), as per applied logical options.
        /// </summary>
        public decimal TilesToShineCharge { get; private set; } = LogicalOptions.DefaultTilesToShineCharge;

        protected override CanShineCharge CreateFinalizedElement(UnfinalizedCanShineCharge sourceElement, Action<CanShineCharge> mappingsInsertionCallback, ModelFinalizationMappings mappings)
        {
            return new CanShineCharge(sourceElement, mappingsInsertionCallback);
        }

        protected override bool ApplyLogicalOptionsEffects(ReadOnlyLogicalOptions logicalOptions)
        {
            TilesSavedWithStutter = logicalOptions?.TilesSavedWithStutter ?? LogicalOptions.DefaultTilesSavedWithStutter;
            TilesToShineCharge = logicalOptions?.TilesToShineCharge ?? LogicalOptions.DefaultTilesToShineCharge;

            bool useless = false;
            if(MustShinespark && !logicalOptions.CanShinespark)
            {
                useless = true;
            }

            // Since this is an in-room shine charge, its required nunmber of tiles is constant.
            // As such, we could check here whether the logical options make the shine too short to be possible.
            // However, this requires access to the game rules, which we don't have here.
            // Improve this if we decide to pass the rules here.

            // We could also pre-calculate an effective runway length if we had the rules

            return useless;
        }

        public override bool IsNever()
        {
            return false;
        }

        public override IEnumerable<string> InitializeReferencedLogicalElementProperties(UnfinalizedSuperMetroidModel model, UnfinalizedRoom room)
        {
            // No properties need to be handled here
            return Enumerable.Empty<string>();
        }

        protected override UnfinalizedExecutionResult ExecuteUseful(UnfinalizedSuperMetroidModel model, ReadOnlyUnfinalizedInGameState inGameState, int times = 1, int previousRoomCount = 0)
        {
            // Always need the SpeedBooster to charge a bluesuit
            if (!inGameState.Inventory.HasSpeedBooster())
            {
                return null;
            }

            // The runway must be long enough to charge
            if (model.Rules.CalculateEffectiveRunwayLength(this, TilesSavedWithStutter) < TilesToShineCharge)
            {
                return null;
            }

            // If we have enough energy for the shinespark to go through, consume the energy cost and return the result
            int energyNeeded = model.Rules.CalculateEnergyNeededForShinespark(ShinesparkFrames, times: times);

            // Not calling IsResourceAvailable() because Samus only needs to have that much energy, not necessarily spend all of it
            if (inGameState.Resources.GetAmount(ConsumableResourceEnum.Energy) >= energyNeeded)
            {
                int energyCost = model.Rules.CalculateShinesparkDamage(inGameState, ShinesparkFrames, times: times);
                UnfinalizedInGameState resultingState = inGameState.Clone();
                resultingState.ApplyConsumeResource(ConsumableResourceEnum.Energy, energyCost);
                UnfinalizedExecutionResult result = new UnfinalizedExecutionResult(resultingState);
                result.AddItemsInvolved(new UnfinalizedItem[] { model.Items[SuperMetroidModel.SPEED_BOOSTER_NAME] });
                return result;
            }
            // If we don't have enough for the shinespark, we cannot do this
            else
            {
                return null;
            }
        }
    }
}
