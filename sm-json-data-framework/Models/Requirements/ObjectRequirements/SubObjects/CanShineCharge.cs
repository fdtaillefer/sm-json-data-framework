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

        /// <summary>
        /// Number of tiles the player is expected to be able save if stutter is possible on a runway, as per applied logical options.
        /// </summary>
        public decimal TilesSavedWithStutter => AppliedLogicalOptions?.TilesSavedWithStutter ?? LogicalOptions.DefaultTilesSavedWithStutter;

        /// <summary>
        /// Smallest number of tiles the player is expected to be able to obtain a shine charge with (before applying stutter), as per applied logical options.
        /// </summary>
        public decimal TilesToShineCharge => AppliedLogicalOptions?.TilesToShineCharge ?? LogicalOptions.DefaultTilesToShineCharge;

        /// <summary>
        /// Whether the player is logically expected to know how to shinespark.
        /// </summary>
        public bool CanShinespark => AppliedLogicalOptions?.CanShinespark ?? LogicalOptions.DefaultTechsAllowed;

        private UnfinalizedCanShineCharge InnerElement {get;set;}
        public CanShineCharge(UnfinalizedCanShineCharge innerElement, Action<CanShineCharge> mappingsInsertionCallback) : base(innerElement, mappingsInsertionCallback)
        {
            InnerElement = innerElement;
        }

        public int Length => InnerElement.Length;

        public int EndingUpTiles => InnerElement.EndingUpTiles;

        public int GentleUpTiles => InnerElement.GentleUpTiles;

        public int GentleDownTiles => InnerElement.GentleDownTiles;

        public int SteepUpTiles => InnerElement.SteepUpTiles;

        public int SteepDownTiles => InnerElement.SteepDownTiles;

        public int StartingDownTiles => InnerElement.StartingDownTiles;

        public int OpenEnds => InnerElement.OpenEnds;

        /// <summary>
        /// The duration (in frames) of the shinespark that goes alongside this CanShineCharge, if any. Can be 0 if no shinespark is involved.
        /// </summary>
        public int ShinesparkFrames => InnerElement.ShinesparkFrames;

        /// <summary>
        /// Indicates whether this CanShineCharge involves executing a shinespark.
        /// </summary>
        public bool MustShinespark => InnerElement.MustShinespark;

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

        protected override bool PropagateLogicalOptions(ReadOnlyLogicalOptions logicalOptions)
        {
            bool useless = false;
            if (MustShinespark && !CanShinespark)
            {
                useless = true;
            }

            // Since this is an in-room shine charge, its required number of tiles is constant.
            // As such, we could check here whether the logical options make the shine too short to be possible.
            // However, this requires access to the game rules, which we don't have here.
            // Improve this if we decide to pass the rules here.

            // We could pre-calculate an effective runway length here if we had the rules...

            return useless;
        }

        protected override bool CalculateLogicallyNever()
        {
            bool impossible = false;
            if (MustShinespark && !CanShinespark)
            {
                impossible = true;
            }

            // Since this is an in-room shine charge, its required number of tiles is constant.
            // As such, we could check here whether the logical options make the shine too short to be possible.
            // However, this requires access to the game rules, which we don't have here.
            // Improve this if we decide to pass the rules here.
            return impossible;
        }

        protected override bool CalculateLogicallyAlways()
        {
            // Since this is an in-room shine charge, its required number of tiles is constant.
            // As such, we could check here whether the logical options make the shine long enough to always be possible.
            // However, this requires access to the game rules, which we don't have here.
            // It would also require SpeedBooster to always be available which we don't have a way to check here.
            return false;
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

        /// <summary>
        /// Indicates whether this CanShineCharge involves executing a shinespark.
        /// </summary>
        public bool MustShinespark => ShinesparkFrames > 0;

        protected override CanShineCharge CreateFinalizedElement(UnfinalizedCanShineCharge sourceElement, Action<CanShineCharge> mappingsInsertionCallback, ModelFinalizationMappings mappings)
        {
            return new CanShineCharge(sourceElement, mappingsInsertionCallback);
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
    }
}
