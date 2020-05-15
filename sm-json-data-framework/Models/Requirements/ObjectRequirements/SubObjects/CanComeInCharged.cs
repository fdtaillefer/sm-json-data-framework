using sm_json_data_framework.Models.InGameStates;
using sm_json_data_framework.Models.Items;
using sm_json_data_framework.Models.Rooms;
using sm_json_data_framework.Models.Rooms.Nodes;
using sm_json_data_framework.Rules;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;

namespace sm_json_data_framework.Models.Requirements.ObjectRequirements.SubObjects {
    /// <summary>
    /// A logical element which requires Samus to have been able to come into the room either with
    /// a shinespark charged or, possibly, while doing a shinespark (depending on the required
    /// number of remaining frames in the shinespark charge)
    /// </summary>
    public class CanComeInCharged : AbstractObjectLogicalElement
    {
        [JsonPropertyName("fromNode")]
        public int FromNodeId { get; set; }

        /// <summary>
        /// <para>Only available after a call to <see cref="InitializeReferencedLogicalElementProperties(SuperMetroidModel, Room)"/>.</para>
        /// <para>The node that this element's FromNodeId references.</para>
        /// </summary>
        [JsonIgnore]
        public RoomNode FromNode { get; set; }

        public IEnumerable<int> InRoomPath { get; set; } = Enumerable.Empty<int>();

        public int FramesRemaining { get; set; }

        public int ShinesparkFrames { get; set; }

        public override IEnumerable<string> InitializeReferencedLogicalElementProperties(SuperMetroidModel model, Room room)
        {
            if (room.Nodes.TryGetValue(FromNodeId, out RoomNode node))
            {
                FromNode = node;
                return Enumerable.Empty<string>();
            }
            else
            {
                return new[] { $"Node {FromNodeId} in room {room.Name}" };
            }
        }

        public override ExecutionResult Execute(SuperMetroidModel model, InGameState inGameState, int times = 1, bool usePreviousRoom = false)
        {
            var mustShinespark = ShinesparkFrames > 0;
            var energyNeededForShinespark = model.Rules.CalculateEnergyNeededForShinespark(ShinesparkFrames) * times;
            Predicate<InGameState> hasEnergyForShinespark = state => state.IsResourceAvailable(ConsumableResourceEnum.ENERGY, energyNeededForShinespark);
            Action<ExecutionResult> consumeShinesparkEnergy = result => result.ResultingState.ApplyConsumeResource(ConsumableResourceEnum.ENERGY, energyNeededForShinespark);

            // Check simple preconditions before looking at anything
            if (!inGameState.HasSpeedBooster() || (mustShinespark && !model.CanShinespark()))
            {
                return null;
            }

            ExecutionResult bestOverallResult = null;

            // So we have to see if we can use an in-room runway (not coming in).
            // And we have to see if we can use an adjacent runway by itself.
            // And we have to see if we can use a canLeavecharged.
            // And we have to see if we can combine an adjacent runway with a an in-room runway (coming in).
            // And after that we have to still be able to execute the shinespark.

            // We'll start with looking at in-room runways (not coming in).
            // For all in-room runways we are able to use while still doing the shinespark after,
            // figure out the resulting state, effective length, and the overall best resulting state
            var (usableInRoomRunwayEvaluations, bestInRoomResult) =
                EvaluateRunways(model, inGameState, FromNode.Runways, times, usePreviousRoom, hasEnergyForShinespark, runwaysReversible: true);

            // If using this in-room runway cost nothing, spend the shinespark and return
            if (model.CompareInGameStates(inGameState, bestInRoomResult?.ResultingState) == 0)
            {
                consumeShinesparkEnergy(bestInRoomResult);
                bestInRoomResult.AddItemsInvolved(new Item[] { model.Items[SuperMetroidModel.SPEED_BOOSTER_NAME] });
                return bestInRoomResult;
            }

            // If the best in-room runway we found is an improvement over the previous best solution, replace it
            if (model.CompareInGameStates(bestInRoomResult?.ResultingState, bestOverallResult?.ResultingState) > 0)
            {
                bestOverallResult = bestInRoomResult;
            }

            // Next Step: all adjacent runways with their resulting state.

            // If no in-room path is specified, then player is expected to have entered at fromNode and not moved
            var requiredInRoomPath = (InRoomPath == null || !InRoomPath.Any()) ? new[] { FromNodeId } : InRoomPath;
            // For all adjacent runways that can be used retroactively while still doing the shinespark after,
            // figure out the resulting state, effective length, and the overall best resulting state
            var (usableAdjacentRunwayEvaluations, bestAdjacentRunwayResult) =
                EvaluateRunways(model, inGameState, inGameState.GetRetroactiveRunways(requiredInRoomPath, usePreviousRoom: true), times, usePreviousRoom,
                    hasEnergyForShinespark, runwaysReversible: false);

            // If using this adjacent runway cost nothing, spend the shinespark and return
            if (model.CompareInGameStates(inGameState, bestAdjacentRunwayResult?.ResultingState) == 0)
            {
                consumeShinesparkEnergy(bestAdjacentRunwayResult);
                bestAdjacentRunwayResult.AddItemsInvolved(new Item[] { model.Items[SuperMetroidModel.SPEED_BOOSTER_NAME] });
                return bestAdjacentRunwayResult;
            }

            // If the best adjacent runway we found is an improvement over the previous best solution, replace it
            if (model.CompareInGameStates(bestAdjacentRunwayResult?.ResultingState, bestOverallResult?.ResultingState) > 0)
            {
                bestOverallResult = bestAdjacentRunwayResult;
            }

            // Next step: Find the best retroactive CanLeaveCharged that has enough frames
            // remaining and leaves Samus with enough energy for the shinespark
            var usableCanLeaveChargeds = inGameState.GetRetroactiveCanLeaveChargeds(model, requiredInRoomPath, usePreviousRoom: usePreviousRoom)
                .Where(clc => clc.FramesRemaining >= FramesRemaining);
            (_, ExecutionResult bestLeaveChargedResult) = model.ExecuteBest(usableCanLeaveChargeds, inGameState, times: times,
                usePreviousRoom: usePreviousRoom, hasEnergyForShinespark);

            // If using this CanLeaveCharged cost nothing, spend the shinespark and return
            if (model.CompareInGameStates(inGameState, bestLeaveChargedResult?.ResultingState) == 0)
            {
                consumeShinesparkEnergy(bestLeaveChargedResult);
                bestLeaveChargedResult.AddItemsInvolved(new Item[] { model.Items[SuperMetroidModel.SPEED_BOOSTER_NAME] });
                return bestLeaveChargedResult;
            }

            // If the best CanLeaveCharged we found is an improvement over the previous best solution, replace it
            if (model.CompareInGameStates(bestLeaveChargedResult?.ResultingState, bestOverallResult?.ResultingState) > 0)
            {
                bestOverallResult = bestLeaveChargedResult;
            }

            // Next step: Find the best combination of adjacent and in-room runway
            // We can re-use the results from evaluating the adjacent runways that we calculated, but not
            // the in-room ones (because executions, not results, must be applied on top of each other).
            // Iterate over usable adjacent runways that actually offer a gain over the number of
            // tiles lost by the room transation, and match each of those against each in-room
            // runway that is usable coming in and combines for a long enough runway
            foreach (var (_, currentAdjacentRunwayResult, currentLength) in usableAdjacentRunwayEvaluations.Where(runway => runway.length > model.Rules.RoomTransitionTilesLost))
            {
                var requiredInRoomLength = model.LogicalOptions.TilesToShineCharge + model.Rules.RoomTransitionTilesLost - currentLength;

                // Determine which runways we may attempt to use. Limit to the ones we evaluated
                // earlier because there's no point re-evaluating those we couldn't execute then,
                // but we'll re-attempt to use the runways using the resulting state of the current
                // adjacent runway.
                // We'll also ignore in-room runways that are not long enough to combine with our
                // current adjacent runway.
                var adequateRunways =
                    from r in usableInRoomRunwayEvaluations
                    where r.length >= requiredInRoomLength
                    select r.runway;
                var (_, bestCombinationResult) = model.ExecuteBest(adequateRunways.Select(runway => runway.AsExecutable(comingIn: true)),
                    currentAdjacentRunwayResult.ResultingState, times: times, usePreviousRoom: usePreviousRoom, hasEnergyForShinespark);

                // If the best combination we found is free, spend energy for the shinespark and return it.
                // Make sure to apply the in-room runway result on top of the adjacent runway result.
                if (model.CompareInGameStates(inGameState, bestCombinationResult?.ResultingState) == 0)
                {
                    consumeShinesparkEnergy(bestCombinationResult);
                    bestCombinationResult.AddItemsInvolved(new Item[] { model.Items[SuperMetroidModel.SPEED_BOOSTER_NAME] });
                    return currentAdjacentRunwayResult.Clone().ApplySubsequentResult(bestCombinationResult);
                }

                // If the best combination we found is free, spend energy for the shinespark and return it.
                // Make sure to apply the in-room runway result on top of the adjacent runway result.
                if (model.CompareInGameStates(bestCombinationResult?.ResultingState, bestOverallResult?.ResultingState) > 0)
                {
                    consumeShinesparkEnergy(bestCombinationResult);
                    bestOverallResult = currentAdjacentRunwayResult.Clone().ApplySubsequentResult(bestCombinationResult);
                }
            }

            // If we have found no solution at all that we can execute and that leaves us with
            // enough energy for the shinespark, we cannot do this
            if (bestOverallResult == null)
            {
                return null;
            }
            // Apply shinespark on the best solution we've found and return it
            else
            {
                consumeShinesparkEnergy(bestOverallResult);
                bestOverallResult.AddItemsInvolved(new Item[] { model.Items[SuperMetroidModel.SPEED_BOOSTER_NAME] });
                return bestOverallResult;
            }
        }

        /// <summary>
        /// <para>Evaluates all the provided runways, returning for each usable one an evaluation result (composed of the runway, its ExecutionResult, and its effective length).
        /// Groups that enumeration of evaluations with the best (least costly) found Execution result.</para>
        /// <para>All runway executions are done in-room, not coming in. Runways are considered usable if they can be executed,
        /// with enough energy remaining for a </para>
        /// </summary>
        /// <param name="model">A model that can be used to obtain data about the current game configuration.</param>
        /// <param name="inGameState">The in-game state to use for execution. This will NOT be altered by this method.</param>
        /// <param name="runways">The runways to evaluate.</param>
        /// <param name="times">The number of consecutive times that this should be executed.
        /// Only really impacts resource cost, since most items are non-consumable.</param>
        /// <param name="usePreviousRoom">If true, uses the last known room state at the previous room instead of the current room to answer
        /// (whenever in-room state is relevant).</param>
        /// <param name="hasEnergyForShinespark">A predicate that checks whether a resulting InGameState has enough energy for the subsequent shinespark</param>
        /// <param name="runwaysReversible">If true, runways can be used in either direction. If false, they can only be used in their normal direction.</param>
        /// <returns></returns>
        (IEnumerable<(Runway runway, ExecutionResult executionResult, decimal length)> runwayEvaluations, ExecutionResult bestResults) EvaluateRunways(
            SuperMetroidModel model, InGameState inGameState,
            IEnumerable<Runway> runways, int times, bool usePreviousRoom,
            Predicate<InGameState> hasEnergyForShinespark, bool runwaysReversible
        ) {
            Func<IRunway, decimal, decimal> calculateRunwayLength = runwaysReversible? model.Rules.CalculateEffectiveReversibleRunwayLength :
                (Func<IRunway, decimal, decimal>) model.Rules.CalculateEffectiveRunwayLength;

            // Obtain info about all runways that can be used without dropping below the energy needed to shinespark
            var usableRunways =
                from runway in runways
                let executionResult = runway.Execute(model, inGameState, comingIn: false, times, usePreviousRoom)
                where executionResult != null && hasEnergyForShinespark(executionResult.ResultingState)
                select (runway, executionResult, length: calculateRunwayLength(runway, model.LogicalOptions.TilesSavedWithStutter));

            // Find the best resulting state among all provided runways whose length is enough to fulfill this CanComeInCharged
            var bestResult = (
                    from runway in usableRunways
                    where runway.length >= model.LogicalOptions.TilesToShineCharge
                    select runway.executionResult
                ).OrderByDescending(result => result.ResultingState, model.GetInGameStateComparer())
                .FirstOrDefault();

            return (usableRunways, bestResult);
        }
    }
}
