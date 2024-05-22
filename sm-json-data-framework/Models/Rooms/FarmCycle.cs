using sm_json_data_framework.Models.Enemies;
using sm_json_data_framework.Models.InGameStates;
using sm_json_data_framework.Models.Items;
using sm_json_data_framework.Models.Raw.Rooms;
using sm_json_data_framework.Models.Requirements;
using sm_json_data_framework.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;

namespace sm_json_data_framework.Models.Rooms
{
    /// <summary>
    /// Represents a method to farm one cycle of a respawning group of enemies, with the approximate duration.
    /// </summary>
    public class FarmCycle : AbstractModelElement<UnfinalizedFarmCycle, FarmCycle>
    {
        public ReadOnlySpawnerFarmingOptions AppliedFarmingLogicalOptions => AppliedLogicalOptions.SpawnerFarmingOptions;

        public FarmCycle(UnfinalizedFarmCycle sourceElement, Action<FarmCycle> mappingsInsertionCallback, ModelFinalizationMappings mappings)
            : base(sourceElement, mappingsInsertionCallback)
        {
            Name = sourceElement.Name;
            CycleFrames = sourceElement.CycleFrames;
            Requires = sourceElement.Requires.Finalize(mappings);
            RoomEnemy = sourceElement.RoomEnemy.Finalize(mappings);
        }

        /// <summary>
        /// A name to identify this FarmCycle. This is unique ONLY within a <see cref="RoomEnemy"/>.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// The number of frames it takes to wait for the enemies to spawn, kill them, and grab their drops
        /// </summary>
        public int CycleFrames { get; }

        /// <summary>
        /// The LogicalRequirements that must be fulfilled in order to execute a cycle of farming on the enemies.
        /// </summary>
        public LogicalRequirements Requires { get; }

        /// <summary>
        /// The RoomEnemy to which this FarmCycle applies.
        /// </summary>
        public RoomEnemy RoomEnemy { get; }

        /// <summary>
        /// Returns whether this farm cycle can be farmed "for free", without spending any resources during execution (regardless of drops).
        /// </summary>
        /// <param name="model">A model that can be used to obtain data about the current game configuration.</param>
        /// <param name="inGameState">The in-game state to use for execution. This will NOT be altered by this method.</param>
        /// <param name="times">The number of consecutive times that this should be executed.
        /// Only really impacts resource cost, since most items are non-consumable.</param>
        /// <param name="usePreviousRoom">If true, uses the last known room state at the previous room instead of the current room to answer
        /// (whenever in-room state is relevant).</param>
        /// <returns></returns>
        public bool IsFree(SuperMetroidModel model, ReadOnlyInGameState inGameState, int times = 1, int previousRoomCount = 0)
        {
            // Execute the requirements of this cycle once
            ExecutionResult executionResult = RequirementExecution.Execute(model, inGameState, times: times, previousRoomCount: previousRoomCount);

            // If we failed, this can't be farmed for free
            if (executionResult == null)
            {
                return false;
            }
            // If any resource was reduced by the execution, this can't be farmed for free
            else
            {
                return !executionResult.ResultingState.GetResourceVariationWith(inGameState).Any(variation => variation < 0);
            }
        }

        // Requirements execution isn't really needed. It's just an indirection to Requires.Execute().
        // It'll stay here for now just to be more consistent with the presence of Farm execution
        IExecutable _requirementsExecution = null;
        /// <summary>
        /// An IExecutable that corresponds to executing the requirements for this farm cycle once, without grabbing any drops.
        /// </summary>
        public IExecutable RequirementExecution
        {
            get
            {
                if (_requirementsExecution == null)
                {
                    _requirementsExecution = new RequirementExecution(this);
                }
                return _requirementsExecution;
            }
        }

        IExecutable _farmExecution = null;
        /// <summary>
        /// <para>An IExecutable that corresponds to farming this group of enemies, by camping its spawner(s), killing it repeatedly, and grabbing the drops.
        /// This is repeated until all qualifying resources are filled.</para>
        /// <para>Qualifying resources are determined based on logical options.</para>
        /// <para>For simplicity, a farm execution will be considered a failure if it results in any kind of resource tradeoff.</para>
        /// </summary>
        public IExecutable FarmExecution
        {
            get
            {
                if (_farmExecution == null)
                {
                    _farmExecution = new FarmExecution(this);
                }
                return _farmExecution;
            }
        }

        protected override void PropagateLogicalOptions(ReadOnlyLogicalOptions logicalOptions)
        {
            Requires.ApplyLogicalOptions(logicalOptions);
        }

        protected override void UpdateLogicalProperties()
        {
            base.UpdateLogicalProperties();
            LogicallyNever = CalculateLogicallyNever();
        }

        public override bool CalculateLogicallyRelevant()
        {
            // If a farm cycle cannot be executed, it may as well not exist
            return !CalculateLogicallyNever();
        }

        /// <summary>
        /// If true, then this farmCycle is impossible to execute given the current logical options, regardless of in-game state.
        /// </summary>
        public bool LogicallyNever { get; private set; }

        /// <summary>
        /// Calculates what the value of <see cref="LogicallyNever"/> should currently be.
        /// </summary>
        /// <returns></returns>
        protected bool CalculateLogicallyNever()
        {
            // A farm cycle is impossible if its requirements are impossible
            return Requires.LogicallyNever;
        }
    }


    /// <summary>
    /// A class that encloses the execution of a farm cycle's requirements (for one cycle)  in an IExecutable interface.
    /// </summary>
    internal class RequirementExecution : IExecutable
    {
        private FarmCycle FarmCycle { get; set; }

        public RequirementExecution(FarmCycle farmCycle)
        {
            FarmCycle = farmCycle;
        }
        public ExecutionResult Execute(SuperMetroidModel model, ReadOnlyInGameState inGameState, int times = 1, int previousRoomCount = 0)
        {
            return FarmCycle.Requires.Execute(model, inGameState, times: times, previousRoomCount: previousRoomCount);
        }
    }

    /// <summary>
    /// A class that encloses executing a refill of some resources using a FarmCycle in an IExecutable interface.
    /// </summary>
    internal class FarmExecution : IExecutable
    {
        private FarmCycle FarmCycle { get; set; }

        public FarmExecution(FarmCycle farmCycle)
        {
            FarmCycle = farmCycle;
        }

        public ExecutionResult Execute(SuperMetroidModel model, ReadOnlyInGameState inGameState, int times = 1, int previousRoomCount = 0)
        {
            if(FarmCycle.LogicallyNever)
            {
                return null;
            }
            var requirementsResult = FarmCycle.RequirementExecution.Execute(model, inGameState, times: times, previousRoomCount: previousRoomCount);
            // Can't even execute one cycle, so return a failure
            if (requirementsResult == null)
            {
                return null;
            }

            // Build a dictionary of resources that are spent while doing the execution of a cycle
            ResourceCount resourceVariation = requirementsResult.ResultingState.GetResourceVariationWith(inGameState);
            // Start with all consumable resources
            IDictionary<ConsumableResourceEnum, int> costingResources = Enum.GetValues(typeof(ConsumableResourceEnum))
                .Cast<ConsumableResourceEnum>()
                // Invert the resource variation to convert it to a resource cost
                .Select(resource => (resource: resource, cost: resourceVariation.GetAmount(resource) * -1))
                // Keep only pairs where some of the resource has been spent
                .Where(resourceCost => resourceCost.cost > 0)
                // Finally, build a dictionary from the pairs
                .ToDictionary(resourceCost => resourceCost.resource, resourceCost => resourceCost.cost);

            // Identify all resources that can be refilled by this farm cycle
            IEnumerable<ConsumableResourceEnum> farmableResources = ComputeFarmableResources(model, costingResources);

            // Calculate the initial effective drop rates, taking into account currently full resources.
            // However, resources that are full but not farmable here should not be taken into account.
            IEnumerable<ConsumableResourceEnum> fullResources = inGameState.GetFullConsumableResources().Intersect(farmableResources).ToArray();
            EnemyDrops initialEffectiveDropRates = FarmCycle.RoomEnemy.Enemy.GetEffectiveDropRates(model, fullResources);

            // Build a dictionary containing the variation per cycle for each consmable resource
            IDictionary<ConsumableResourceEnum, decimal> resourceVariationPerCycle = Enum.GetValues(typeof(ConsumableResourceEnum))
                .Cast<ConsumableResourceEnum>()
                .ToDictionary(resource => resource, resource => CalculateResourceVariationPerCycle(model, resource, initialEffectiveDropRates, costingResources));

            // Identify resources that are creeping down as we farm
            IEnumerable<ConsumableResourceEnum> initiallyUnstableResources = costingResources
                    .Where(pair => resourceVariationPerCycle[pair.Key] < 0)
                    .Select(pair => pair.Key)
                    .ToArray();

            // If there's no resources we can farm, just return now
            if (!farmableResources.Any())
            {
                // If any of the resources initially lose out per farm cycle, we're not even able to farm. Return a failure.
                if (initiallyUnstableResources.Any())
                {
                    return null;
                }

                // Otherwise, we're able to farm but it doesn't do anything according to logical options
                return new ExecutionResult(inGameState.Clone());
            }

            // If there's no resource that initially loses out, we're not concerned about losing any resources.
            // We can refill all farmable resources and report a success
            if (!initiallyUnstableResources.Any())
            {
                return ExecuteRefill(model, inGameState, farmableResources);
            }

            // If we have resources that initially lose out, they must eventually turn farmable.
            // Otherwise, we consider this a failure.
            if (initiallyUnstableResources.Except(farmableResources).Any())
            {
                return null;
            }

            // Now we know we have at least one resource that currently loses out per cycle, but can eventually recharge.
            // Execute some farming to see if we can stabilize those resources before we run out.

            IEnumerable<ConsumableResourceEnum> notFullFarmableResources = farmableResources.Except(fullResources).ToArray();
            IDictionary<ConsumableResourceEnum, decimal> resourceCounts = Enum.GetValues(typeof(ConsumableResourceEnum))
                .Cast<ConsumableResourceEnum>()
                .ToDictionary(resource => resource, resource => (decimal)inGameState.Resources.GetAmount(resource));
            EnemyDrops effectiveDropRates = initialEffectiveDropRates;

            // Execute farm cycles until a resource runs out or all costing resources have stabilized
            while (costingResources
                    .Where(pair => resourceVariationPerCycle[pair.Key] < 0)
                    .Any())
            {
                // Figure out how many cycles we need to execute in order to refill something farmable and stable
                int cyclesToRefillSomething = notFullFarmableResources.Select(resource =>
                    decimal.ToInt32(decimal.Ceiling((inGameState.ResourceMaximums.GetAmount(resource) - resourceCounts[resource]) / resourceVariationPerCycle[resource])))
                    .Where(cycleCount => cycleCount > 0)
                    .Min();

                // Apply to each farmable resource the resource variation from executing that many cycles. 
                // We don't care if it goes over maximum since we won't apply these to the in-game state
                foreach (ConsumableResourceEnum resource in notFullFarmableResources)
                {
                    resourceCounts[resource] += resourceVariationPerCycle[resource] * cyclesToRefillSomething;
                }

                // If an unstable resource has dipped below the cost per cycle, we can't go on. Return a failure.
                if (costingResources.Where(costingResource => resourceCounts[costingResource.Key] < costingResource.Value).Any())
                {
                    return null;
                }

                // If we haven't run out of anything, prepare the next loop

                // Update full resources
                fullResources = resourceCounts
                    .Where(pair => pair.Value >= inGameState.ResourceMaximums.GetAmount(pair.Key))
                    .Select(pair => pair.Key)
                    .Intersect(farmableResources)
                    .ToArray();

                // Update farmable resources by excluding newly-full resources
                notFullFarmableResources = notFullFarmableResources.Except(fullResources).ToArray();

                // Calculate a new effective drop rate using the new list of full resources
                // If that new effective drop rate stabilizes all unstable resources, we'll make it out of the loop
                effectiveDropRates = model.Rules.CalculateEffectiveDropRates(FarmCycle.RoomEnemy.Enemy.Drops, model.Rules.GetUnneededDrops(fullResources));

                // Use the new effective drop rate to calculate the new resourceVariationPerCycle for resources we still care about
                resourceVariationPerCycle = notFullFarmableResources
                    .ToDictionary(resource => resource, resource => CalculateResourceVariationPerCycle(model, resource, effectiveDropRates, costingResources));
            }

            // All resources are now stable. We already checked beforehand that all costing resources eventually become farmable,
            // so we can just apply a refill for all farmable resources and return a success.
            return ExecuteRefill(model, inGameState, farmableResources);
        }

        /// <summary>
        /// Creates and returns an ExecutionResult based on the provided in-game state, with the provided resources refilled.
        /// </summary>
        /// <param name="model">A model that can be used to obtain data about the current game configuration.</param>
        /// <param name="inGameState">The in-game state to use for execution. This will NOT be altered by this method.</param>
        /// <param name="resourcesToRefill">The resources that should be refilled.</param>
        /// <returns></returns>
        private ExecutionResult ExecuteRefill(SuperMetroidModel model, ReadOnlyInGameState inGameState, IEnumerable<ConsumableResourceEnum> resourcesToRefill)
        {
            InGameState resultingState = inGameState.Clone();
            foreach (ConsumableResourceEnum resource in resourcesToRefill)
            {
                resultingState.ApplyRefillResource(resource);
            }
            return new ExecutionResult(resultingState);
        }

        /// <summary>
        /// <para>Calculates and returns which resources can meet meet the logical criteria
        /// for refilling by farming this farm cycle.</para>
        /// <para>This includes resources that only meet those criteria after redistributing
        /// the drop rate from other resources that were farmed to full.</para>
        /// </summary>
        /// <param name="model">A model that can be used to obtain data about the current game configuration.</param>
        /// <param name="costingResources">A dictionary of resources that have a cost associated with executing a cycle.
        /// The resource is the key and the cost per cycle is the value.</param>
        /// <returns></returns>
        private IEnumerable<ConsumableResourceEnum> ComputeFarmableResources(SuperMetroidModel model,
            IDictionary<ConsumableResourceEnum, int> costingResources)
        {
            ISet<ConsumableResourceEnum> farmableResources = new HashSet<ConsumableResourceEnum>();
            IEnumerable<ConsumableResourceEnum> newFarmableResources;

            // Figure out which resources meet the threshold for farmability
            // We'll keep looping as long as we find farmable resources,
            // to discover resources that go above the threshold when redistributing drop rates
            do
            {
                // Determine effective drop rates for this iteration
                EnemyDrops effectiveDropRates = FarmCycle.RoomEnemy.Enemy.GetEffectiveDropRates(model, farmableResources);

                // Look for resources that meet the logical farming threshold, but didn't in the previous loops
                newFarmableResources = Enum.GetValues(typeof(ConsumableResourceEnum))
                    .Cast<ConsumableResourceEnum>()
                    .Except(farmableResources)
                    .Where(resource =>
                    {
                        return CalculateResourceVariationPerSecond(model, resource, effectiveDropRates, costingResources)
                            >= FarmCycle.AppliedFarmingLogicalOptions.MinimumRatesPerSecond[resource];
                    })
                    // Actualize this to calculate it only once
                    .ToArray();

                farmableResources.UnionWith(newFarmableResources);
            } // Stop iterating if the last loop revealed no new farmable resource
            while (newFarmableResources.Any());

            return farmableResources;
        }

        /// <summary>
        /// <para>Calculates how much of the provided resource is gained or lost (on average) over 60 frames of farming,
        /// given the provided effective drop rates and resource costs per cycle</para>
        /// <para>If the resource is also spent while executing a cycle, the calculation includes 
        /// reducing the drop rate by the safety margin in the logical options.</para>
        /// </summary>
        /// <param name="model">A model that can be used to obtain data about the current game configuration.</param>
        /// <param name="resource">The resource to check</param>
        /// <param name="effectiveDropRates">The effective drop rates, after accounting for unneeded drops.</param>
        /// <param name="costingResources">A dictionary  of resources to their cost per cycle.</param>
        /// <returns></returns>
        private decimal CalculateResourceVariationPerSecond(SuperMetroidModel model, ConsumableResourceEnum resource,
            EnemyDrops effectiveDropRates, IDictionary<ConsumableResourceEnum, int> costingResources)
        {
            decimal variationPerCycle = CalculateResourceVariationPerCycle(model, resource, effectiveDropRates, costingResources);
            return variationPerCycle / FarmCycle.CycleFrames * 60;
        }

        /// <summary>
        /// <para>Calculates how much of the provided resource is gained or lost (on average) by executing this farm cycle once,
        /// given the provided effective drop rates and resource costs per cycle</para>
        /// <para>If the resource is also spent while executing a cycle, the calculation includes 
        /// reducing the drop rate by the safety margin in the logical options.</para>
        /// </summary>
        /// <param name="model">A model that can be used to obtain data about the current game configuration.</param>
        /// <param name="resource">The resource to check</param>
        /// <param name="effectiveDropRates">The effective drop rates, after accounting for unneeded drops.</param>
        /// <param name="costingResources">A dictionary  of resources to their cost per cycle.</param>
        /// <returns></returns>
        private decimal CalculateResourceVariationPerCycle(SuperMetroidModel model, ConsumableResourceEnum resource,
        EnemyDrops effectiveDropRates, IDictionary<ConsumableResourceEnum, int> costingResources)
        {
            // If this farm cycle has to spend some of this resource, do some adaptations:
            // - Reduce the expected drop rate by the logical safety margin
            // - Include the cost when looking at the amount obtained from drops
            decimal dropRateMultiplier;
            int costPerCycle;
            if (costingResources.TryGetValue(resource, out costPerCycle))
            {
                dropRateMultiplier = (100 - FarmCycle.AppliedFarmingLogicalOptions.SafetyMarginPercent) / 100;
            }
            else
            {
                costPerCycle = 0;
                dropRateMultiplier = 1M;
            }

            decimal netGainPerCycle = resource.GetRelatedDrops()
                .Select(drop => dropRateMultiplier * model.Rules.ConvertDropRateToPercent(effectiveDropRates.GetDropRate(drop))
                    * FarmCycle.RoomEnemy.Quantity * model.Rules.GetDropResourceCount(drop))
                .Sum() - costPerCycle;
            return netGainPerCycle;
        }
    }

    public class UnfinalizedFarmCycle : AbstractUnfinalizedModelElement<UnfinalizedFarmCycle, FarmCycle>, InitializablePostDeserializableInRoomEnemy
    {
        public string Name { get; set; }

        public int CycleFrames { get; set; }

        public ReadOnlySpawnerFarmingOptions AppliedFarmingLogicalOptions { get; private set; } = new SpawnerFarmingOptions().AsReadOnly();

        public UnfinalizedLogicalRequirements Requires { get; set; }

        /// <summary>
        /// <para>Not available before <see cref="Initialize(UnfinalizedSuperMetroidModel, UnfinalizedRoom, UnfinalizedRoomEnemy)"/> has been called.</para>
        /// <para>The RoomEnemy to which this FarmCycle applies</para>
        /// </summary>
        public UnfinalizedRoomEnemy RoomEnemy { get; set; }

        public UnfinalizedFarmCycle()
        {

        }

        public UnfinalizedFarmCycle(RawFarmCycle rawCycle, LogicalElementCreationKnowledgeBase knowledgeBase)
        {
            Name = rawCycle.Name;
            CycleFrames = rawCycle.CycleFrames;
            if(rawCycle.Requires != null)
            {
                Requires = rawCycle.Requires.ToLogicalRequirements(knowledgeBase);
            }
        }

        protected override FarmCycle CreateFinalizedElement(UnfinalizedFarmCycle sourceElement, Action<FarmCycle> mappingsInsertionCallback, ModelFinalizationMappings mappings)
        {
            return new FarmCycle(sourceElement, mappingsInsertionCallback, mappings);
        }

        public void InitializeProperties(UnfinalizedSuperMetroidModel model, UnfinalizedRoom room, UnfinalizedRoomEnemy roomEnemy)
        {
            RoomEnemy = roomEnemy;
        }

        public IEnumerable<string> InitializeReferencedLogicalElementProperties(UnfinalizedSuperMetroidModel model, UnfinalizedRoom room, UnfinalizedRoomEnemy roomEnemy)
        {
            return Requires.InitializeReferencedLogicalElementProperties(model, room);
        }
    }
}
