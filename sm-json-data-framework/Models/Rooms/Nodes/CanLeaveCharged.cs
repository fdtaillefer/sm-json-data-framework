using sm_json_data_framework.Models.InGameStates;
using sm_json_data_framework.Models.Requirements;
using sm_json_data_framework.Models.Rooms.Nodes;
using sm_json_data_framework.Rules;
using sm_json_data_framework.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;

namespace sm_json_data_framework.Models.Rooms.Node
{
    public class CanLeaveCharged : InitializablePostDeserializeInNode, IRunway
    {
        [JsonIgnore]
        public int Length { get => UsedTiles; }

        [JsonIgnore]
        public int EndingUpTiles { get => 0; }

        public int UsedTiles { get; set; }

        public int FramesRemaining { get; set; }

        public int ShinesparkFrames { get; set; }

        [JsonPropertyName("initiateAt")]
        public int? OverrideInitiateAtNodeId { get; set; }

        /// <summary>
        /// <para>Not available before <see cref="Initialize(SuperMetroidModel, RoomNode)"/> has been called.</para>
        /// <para>The node referenced by the <see cref="OverrideInitiateAtNodeId"/> property, if any.</para>
        /// </summary>
        [JsonIgnore]
        public RoomNode OverrideInitiateAtNode { get; set; }

        public bool MustOpenDoorFirst { get; set; } = false;

        /// <summary>
        /// <para>Not reliable before <see cref="Initialize(SuperMetroidModel)"/> has been called.</para>
        /// <para>The node at which Samus actually spawns upon entering the room via this node. In most cases it will be this node, but not always.</para>
        /// </summary>
        [JsonIgnore]
        public RoomNode InitiateAtNode { get { return OverrideInitiateAtNode ?? Node; } }

        public IEnumerable<Strat> Strats { get; set; } = Enumerable.Empty<Strat>();

        [JsonPropertyName("openEnd")]
        public int OpenEnds { get; set; } = 0;

        public int GentleUpTiles { get; set; } = 0;

        public int GentleDownTiles { get; set; } = 0;

        public int SteepUpTiles { get; set; } = 0;

        public int SteepDownTiles { get; set; } = 0;

        public int StartingDownTiles { get; set; } = 0;

        /// <summary>
        /// <para>Not available before <see cref="Initialize(SuperMetroidModel, Room)"/> has been called.</para>
        /// <para>The effective runway length for the runway of this CanLeaveCharged, based on initial rules and logical options.
        /// Because this is not considered reversible, there is only one value.</para>
        /// </summary>
        [JsonIgnore]
        public decimal EffectiveRunwayLength { get; protected set; } = 0;

        /// <summary>
        /// <para>Not available before <see cref="Initialize(SuperMetroidModel, RoomNode)"/> has been called.</para>
        /// <para>The node in which this CanLeaveCharged is.</para>
        /// </summary>
        [JsonIgnore]
        public RoomNode Node { get; set; }

        public void Initialize(SuperMetroidModel model, Room room, RoomNode node)
        {
            Node = node;

            // Initialize OverrideInitiateAtNode
            if (OverrideInitiateAtNodeId != null)
            {
                OverrideInitiateAtNode = node.Room.Nodes[(int)OverrideInitiateAtNodeId];
            }

            // Eliminate disabled strats
            Strats = Strats.WhereEnabled(model);

            foreach (Strat strat in Strats)
            {
                strat.Initialize(model, node.Room);
            }

            EffectiveRunwayLength = model.Rules.CalculateEffectiveRunwayLength(this, model.LogicalOptions.TilesSavedWithStutter);
        }

        public IEnumerable<string> InitializeReferencedLogicalElementProperties(SuperMetroidModel model, Room room, RoomNode node)
        {
            List<string> unhandled = new List<string>();

            foreach(Strat strat in Strats)
            {
                unhandled.AddRange(strat.InitializeReferencedLogicalElementProperties(model, room));
            }

            return unhandled.Distinct();
        }

        // STITCHME This probably ought to return a new InGameState after using the runway (or null if not possible)
        /// <summary>
        /// Returns whether this canLeaveCharged is usable according to the provided parameters
        /// </summary>
        /// <param name="model">A model that can be used to obtain data about the current game configuration.</param>
        /// <param name="inGameState">The in-game state to evaluate</param>
        /// <param name="times">The number of consecutive times that this should be checked for usability. Only really impacts resource cost, since most items are non-consumable.</param>
        /// <param name="usePreviousRoom">If true, uses the last known room state at the previous room instead of the current room to answer
        /// (whenever in-room state is relevant).</param>
        /// <returns></returns>
        public bool IsUsable(SuperMetroidModel model, InGameState inGameState, int times = 1, bool usePreviousRoom = false)
        {
            // There are many things to check...

            // If we don't have SpeedBooster, this is not usable
            if(!inGameState.HasSpeedBooster())
            {
                return false;
            }

            // If we need a shinespark and the tech is turned off, this is not usable
            bool mustShinespark = ShinesparkFrames > 0;
            if(mustShinespark && !model.CanShinespark())
            {
                return false;
            }

            // If the player is unable to charge a shinespark with the available runway, this is not usable
            if (EffectiveRunwayLength < model.LogicalOptions.TilesToShineCharge)
            {
                return false;
            }

            // If there are no strats we are able to execute, this is not usable
            if (!Strats.Any(s => s.IsFulfilled(model, inGameState, times: times, usePreviousRoom: usePreviousRoom)))
            {
                return false;
            }

            // If we don't have enough energy to actually execute the shinespark after all that, this is not usable
            if(!inGameState.IsResourceAvailable(ConsumableResourceEnum.ENERGY, model.Rules.CalculateEnergyNeededForShinespark(ShinesparkFrames) * times))
            {
                return false;
            }

            // Made it!
            return true;
        }
    }
}
