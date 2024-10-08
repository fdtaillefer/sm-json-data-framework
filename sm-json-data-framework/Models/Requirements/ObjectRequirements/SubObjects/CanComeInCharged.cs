﻿using sm_json_data_framework.InGameStates;
using sm_json_data_framework.Models.Items;
using sm_json_data_framework.Models.Rooms;
using sm_json_data_framework.Models.Rooms.Nodes;
using sm_json_data_framework.Options;
using sm_json_data_framework.Rules;
using sm_json_data_framework.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace sm_json_data_framework.Models.Requirements.ObjectRequirements.SubObjects
{
    /// <summary>
    /// A logical element which requires Samus to have been able to come into the room either with
    /// a shinespark charged or, possibly, while doing a shinespark (depending on the required
    /// number of remaining frames in the shinespark charge)
    /// </summary>
    public class CanComeInCharged : AbstractObjectLogicalElement<UnfinalizedCanComeInCharged, CanComeInCharged>
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

        public CanComeInCharged(UnfinalizedCanComeInCharged sourceElement, Action<CanComeInCharged> mappingsInsertionCallback, ModelFinalizationMappings mappings)
            : base(sourceElement, mappingsInsertionCallback)
        {
            FramesRemaining = sourceElement.FramesRemaining;
            ShinesparkFrames = sourceElement.ShinesparkFrames;
            ExcessShinesparkFrames = sourceElement.ExcessShinesparkFrames;
            FromNode = sourceElement.FromNode.Finalize(mappings);
            InRoomPath = sourceElement.InRoomPath.AsReadOnly();
        }

        /// <summary>
        /// The node that this element's FromNodeId references.
        /// </summary>
        public RoomNode FromNode { get; }

        /// <summary>
        /// The precise list of nodes that must be traveled by Samus to execute this CanComeInCharged, from FromNode to the node where it's executed.
        /// </summary>
        public IReadOnlyList<int> InRoomPath { get; }

        /// <summary>
        /// Minimum number of frames that must be remaining in the shine charge when coming in the room in order to satisgy this CanComeInCharged.
        /// </summary>
        public int FramesRemaining { get; }

        /// <summary>
        /// The duration (in frames) of the shinespark that goes alongside this CanComeInCharged, if any. Can be 0 if no shinespark is involved.
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
        /// Indicates whether this CanComeInCharged involves executing a shinespark.
        /// </summary>
        public bool MustShinespark => ShinesparkFrames > 0;

        protected override ExecutionResult ExecutePossible(SuperMetroidModel model, ReadOnlyInGameState inGameState, int times = 1, int previousRoomCount = 0)
        {
            int energyNeededForShinespark = model.Rules.CalculateMinimumEnergyNeededForShinespark(ShinesparkFrames, ExcessShinesparkFrames, times: times);
            int shinesparkEnergyCost = model.Rules.CalculateInterruptibleShinesparkDamage(inGameState, ShinesparkFrames, times: times);
            // Not calling IsResourceAvailable() because Samus only needs to have that much energy, not necessarily spend all of it
            Predicate<ReadOnlyInGameState> hasEnergyForShinespark = state => state.Resources.GetAmount(ConsumableResourceEnum.Energy) >= energyNeededForShinespark;
            Action<ExecutionResult> consumeShinesparkEnergy = result => result.ResultingState.ApplyConsumeResource(ConsumableResourceEnum.Energy, shinesparkEnergyCost);

            // Check simple preconditions before looking at anything
            if (!inGameState.Inventory.HasSpeedBooster())
            {
                return null;
            }

            // Another precondition to check is if we respect the in-room path
            // If no in-room path is specified, then implicit in-room path is to have entered at fromNode and not moved
            IList<int> requiredInRoomPath = (InRoomPath == null || !InRoomPath.Any()) ? new[] { FromNode.Id } : InRoomPath.ToList();

            // While the required path must be followed exactly for cross-room execution, we are more lenient for in-room execution.
            // In that case, we only need a shineCharge to be obtained at fromNode and carried along the path.
            // So the requirement in that case is that the current InRoomPath must end with the required InRoomPath.
            // If it doesn't, there is no way we can fulfill this in any way.

            IReadOnlyList<(ReadOnlyInNodeState nodeState, Strat strat)> currentPath = inGameState.GetVisitedPath();
            // If we haven't even visited as many nodes as necessary, there's no way we've carried a charge along the path
            if (currentPath.Count < requiredInRoomPath.Count)
            {
                return null;
            }
            // Extract the last n nodes of the current path and check if it matches required path
            List<int> currentInRoomPathEnd = currentPath.Skip(currentPath.Count - requiredInRoomPath.Count).Select(node => node.nodeState.Node.Id).ToList();
            if (!currentInRoomPathEnd.SequenceEqual(requiredInRoomPath))
            {
                return null;
            }
            // Stricter checks can be done later to further restrict for cross-room execution


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
                EvaluateRunways(model, inGameState, FromNode.Runways.Values.WhereLogicallyRelevant(), times, previousRoomCount, hasEnergyForShinespark, runwaysReversible: true);

            // If using this in-room runway cost nothing, spend the shinespark and return. We won't find a better option.
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

            // For all adjacent runways that can be used retroactively while still doing the shinespark after,
            // figure out the resulting state, effective length, and the overall best resulting state
            var (usableAdjacentRunwayEvaluations, bestAdjacentRunwayResult) =
                EvaluateRunways(model, inGameState, inGameState.GetRetroactiveRunways(requiredInRoomPath, acceptablePhysics: null, previousRoomCount), times, previousRoomCount,
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
            var usableCanLeaveChargeds = inGameState.GetRetroactiveCanLeaveChargeds(requiredInRoomPath, previousRoomCount: previousRoomCount)
                .Where(clc => clc.FramesRemaining >= FramesRemaining);
            (_, ExecutionResult bestLeaveChargedResult) = usableCanLeaveChargeds.ExecuteBest(model, inGameState, times: times,
                previousRoomCount: previousRoomCount, hasEnergyForShinespark);

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
                var requiredInRoomLength = TilesToShineCharge + model.Rules.RoomTransitionTilesLost - currentLength;

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
                var (_, bestCombinationResult) = adequateRunways.Select(runway => runway.AsExecutable(comingIn: true))
                    .ExecuteBest(model, currentAdjacentRunwayResult.ResultingState, times: times, previousRoomCount: previousRoomCount, hasEnergyForShinespark);

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
        /// <param name="previousRoomCount">The number of playable rooms to go back by (whenever in-room state is relevant). 
        /// 0 means current room, 3 means go back 3 rooms (using last known state), negative values are invalid. Non-playable rooms are skipped.</param>
        /// <param name="hasEnergyForShinespark">A predicate that checks whether a resulting InGameState has enough energy for the subsequent shinespark</param>
        /// <param name="runwaysReversible">If true, runways can be used in either direction. If false, they can only be used in their normal direction.</param>
        /// <returns></returns>
        private (IEnumerable<(Runway runway, ExecutionResult executionResult, decimal length)> runwayEvaluations, ExecutionResult bestResults) EvaluateRunways(
            SuperMetroidModel model, ReadOnlyInGameState inGameState,
            IEnumerable<Runway> runways, int times, int previousRoomCount,
            Predicate<ReadOnlyInGameState> hasEnergyForShinespark, bool runwaysReversible
        )
        {
            // Obtain info about all runways that can be used without dropping below the energy needed to shinespark
            var usableRunways =
                from runway in runways
                let executionResult = runway.Execute(model, inGameState, comingIn: false, times, previousRoomCount)
                where executionResult != null && hasEnergyForShinespark(executionResult.ResultingState)
                select (runway, executionResult, length: runwaysReversible?runway.LogicalEffectiveReversibleRunwayLength:runway.LogicalEffectiveRunwayLength);

            // Find the best resulting state among all provided runways whose length is enough to fulfill this CanComeInCharged
            var bestResult = (
                    from runway in usableRunways
                    where runway.length >= TilesToShineCharge
                    select runway.executionResult
                ).OrderByDescending(result => result.ResultingState, model.InGameStateComparer)
                .FirstOrDefault();

            return (usableRunways, bestResult);
        }

        protected override void PropagateLogicalOptions(ReadOnlyLogicalOptions logicalOptions, SuperMetroidModel model)
        {
            // Nothing to do here
        }

        protected override bool CalculateLogicallyNever(SuperMetroidModel model)
        {
            if(!AppliedLogicalOptions.IsSpeedBoosterInGame())
            {
                return true;
            }

            if (MustShinespark)
            {
                if (!CanShinespark)
                {
                    return true;
                }
                else
                {
                    // If the shinespark requires having more energy than the possible max energy, this is impossible
                    int? maxEnergy = AppliedLogicalOptions.MaxPossibleAmount(ConsumableResourceEnum.Energy);
                    if (maxEnergy != null && model.Rules.CalculateMinimumEnergyNeededForShinespark(ShinesparkFrames, ExcessShinesparkFrames) > maxEnergy.Value)
                    {
                        return true;
                    }
                }
            }

            // This could also become impossible based on layout and not logic, but that part is beyond the scope of this method.

            return false;
        }

        protected override bool CalculateLogicallyAlways(SuperMetroidModel model)
        {
            // This could be always possible based on layout and not logic, but that part is beyond the scope of this method.
            // It would also require SpeedBooster to always be available (and not removed) and to not require a shinespark.
            return false;
        }

        protected override bool CalculateLogicallyFree(SuperMetroidModel model)
        {
            // This could be always free based on layout and not logic, but that part is beyond the scope of this method.
            // It would also need SpeedBooster to always be available (and not removed) and it would need to not require a shinespark.
            return false;
        }
    }

    public class UnfinalizedCanComeInCharged : AbstractUnfinalizedObjectLogicalElement<UnfinalizedCanComeInCharged, CanComeInCharged>
    {
        public int FromNodeId { get; set; }

        /// <summary>
        /// <para>Only available after a call to <see cref="InitializeReferencedLogicalElementProperties(UnfinalizedSuperMetroidModel, UnfinalizedRoom)"/>.</para>
        /// <para>The node that this element's FromNodeId references.</para>
        /// </summary>
        public UnfinalizedRoomNode FromNode { get; set; }

        public IList<int> InRoomPath { get; set; } = new List<int>();

        public int FramesRemaining { get; set; }

        public int ShinesparkFrames { get; set; }

        public int ExcessShinesparkFrames { get; set; }

        protected override CanComeInCharged CreateFinalizedElement(UnfinalizedCanComeInCharged sourceElement, Action<CanComeInCharged> mappingsInsertionCallback, ModelFinalizationMappings mappings)
        {
            return new CanComeInCharged(sourceElement, mappingsInsertionCallback, mappings);
        }

        public override IEnumerable<string> InitializeReferencedLogicalElementProperties(UnfinalizedSuperMetroidModel model, UnfinalizedRoom room)
        {
            if (room.Nodes.TryGetValue(FromNodeId, out UnfinalizedRoomNode node))
            {
                FromNode = node;
                return Enumerable.Empty<string>();
            }
            else
            {
                return new[] { $"Node {FromNodeId} in room {room.Name}" };
            }
        }
    }
}
