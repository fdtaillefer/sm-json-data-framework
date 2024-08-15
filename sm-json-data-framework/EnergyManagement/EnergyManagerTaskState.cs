using sm_json_data_framework.InGameStates;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace sm_json_data_framework.EnergyManagement
{
    /// <summary>
    /// A mutable class that contains various properties about the state of an ongoing EnergyManager task, and offers some low-level operations to execute on that state.
    /// </summary>
    public class EnergyManagerTaskState
    {
        private ResourceCount EnergyVariation { get; }
        /// <summary>
        /// The resulting energy variation from executing the task (assuming the task has been completed).
        /// </summary>
        public ReadOnlyResourceCount ResultingEnergyVariation => EnergyVariation;
        private EnergyManager EnergyManager { get; }
        private ReadOnlyInGameState InitialInGameState { get; }
        private ResourceCount CurrentResources { get; }
        public int RegularEnergy => CurrentResources.GetAmount(Models.Items.RechargeableResourceEnum.RegularEnergy);
        public int ReserveEnergy => CurrentResources.GetAmount(Models.Items.RechargeableResourceEnum.ReserveEnergy);
        public int MaxRegularEnergy => InitialInGameState.ResourceMaximums.GetAmount(Models.Items.RechargeableResourceEnum.RegularEnergy);

        /// <summary>
        /// The excess energy cost associated with the task. It doesn't need to be spent to succeed, but it will be spent if regular energy is available.
        /// </summary>
        public int ExcessEnergyCost { get; }
        /// <summary>
        /// The minimum amount below which regular energy cannot go during the task. 
        /// The task ends if the player would have to go below this value to continue. This means a success if the minimum cost has been spent, and a failure otherwise.
        /// </summary>
        public int MinimumEnergyThreshold { get; }
        /// <summary>
        /// If true, task interrupts as soon as it hits threshold. If false, task can continue after hitting threshold but fails if it reaches below it.
        /// </summary>
        public bool InterruptsAtThreshold { get; }
        /// <summary>
        /// The minimum amount (without the excess cost) of regular energy that remains to be spent to conclude the task.
        /// </summary>
        public int RemainingMinimumCost { get; private set; }
        /// <summary>
        /// The maximum amount of energy that can be spent towards the task, including excess cost.
        /// </summary>
        public int RemainingMaximumCost => RemainingMinimumCost + ExcessEnergyCost;
        /// <summary>
        /// The minimum amount of energy that the player must have in order to be able to spend the minimum needed to complete the task.
        /// </summary>
        public int RemainingMinimumEnergyNeeded => RemainingMinimumCost + MinimumEnergyThreshold;
        /// <summary>
        /// The amount of energy that the player must have in order to be able to spend both the minimum and excess costs of the task.
        /// </summary>
        public int RemainingMaximumEnergyNeeded => RemainingMaximumCost + MinimumEnergyThreshold;
        /// <summary>
        /// The minimum amount of reserve energy that would need to be added to regular energy in order to complete the task.
        /// </summary>
        public int MinimumReserveUseNeeded => RemainingMinimumEnergyNeeded - RegularEnergy;

        /// <summary>
        /// True if there is enough regular energy available to at least spend the non-excess portion of the task's energy cost.
        /// </summary>
        public bool HasEnoughRegularEnergyToComplete => RegularEnergy >= RemainingMinimumEnergyNeeded;
        /// <summary>
        /// If true, the task can be considered a success without spending any more energy.
        /// </summary>
        public bool TaskCompletedSuccessfully => RegularEnergy > 0 && RemainingMinimumCost <= 0 && (RemainingMaximumCost <= 0 || RegularEnergy == MinimumEnergyThreshold);

        /// <summary>
        /// The number of iframes that would be left after an auto refill of reserve tanks if it triggered now.
        /// </summary>
        public int RemainingIframesIfAutoReservesTrigger => Math.Max(0, EnergyManager.Rules.NumberOfIframes - ((int)(ReserveEnergy / EnergyManager.Rules.AutoReserveRefillPerFrame)) );

        public EnergyManagerTaskState(EnergyManager energyManager, ReadOnlyInGameState initialInGameState, int minimumEnergyCost, int excessEnergyCost, int minimumEnergyThreshold)
        {
            EnergyManager = energyManager;
            InitialInGameState = initialInGameState;
            EnergyVariation = new ResourceCount();
            CurrentResources = initialInGameState.Resources.Clone();
            RemainingMinimumCost = minimumEnergyCost;
            ExcessEnergyCost = excessEnergyCost;
            MinimumEnergyThreshold = minimumEnergyThreshold;
        }

        /// <summary>
        /// Consumes regular energy for the task, adjusting the tasks' current regular energy, its resulting energy variation, and its remaining energy cost.
        /// </summary>
        /// <param name="amount">Amount of regular energy to consume</param>
        /// <returns>This, for chaining</returns>
        public EnergyManagerTaskState ConsumeRegularEnergy(int amount)
        {
            InternalReduceRegularEnergy(amount);
            RemainingMinimumCost -= amount;
            return this;
        }

        /// <summary>
        /// <para>
        /// Takes one punctual hit worth the provided amount of damage, progressing towards the task by that amount.
        /// This will trigger auto reserves if brought to 0 energy, and may take a double hit if too many iframes run out (according to logical options).
        /// </para>
        /// <para>
        /// If the hit kills the player, this method will simply leave the player at 0 energy.
        /// </para>
        /// </summary>
        /// <param name="hitDamage">Damage that the hit does</param>
        /// <returns></returns>
        public EnergyManagerTaskState TakePunctualHit(int hitDamage)
        {
            // Energy can't go below 0
            int actualDamageTaken = Math.Min(hitDamage, RegularEnergy);

            InternalReduceRegularEnergy(actualDamageTaken);
            // Player still took the full hit when it comes to progressing the task
            RemainingMinimumCost -= hitDamage;

            // Trigger auto-reserves if necessary
            if (RegularEnergy <= 0)
            {
                int iFramesLeft = RemainingIframesIfAutoReservesTrigger;

                UseAllReserves();

                // If there's not enough iframes left to avoid a double hit, take another hit that does not progress the task
                if (iFramesLeft < EnergyManager.IframesToAvoidDoubleHit)
                {
                    int secondHitActualDamage = Math.Min(hitDamage, RegularEnergy);
                    InternalReduceRegularEnergy(secondHitActualDamage);
                }
            }

            return this;
        }

        /// <summary>
        /// Consumes energy (as a DoT effect) for the task, all the way to 0, then auto-refills with reserves (taking excess damage over time during the refill).
        /// This method will stop consuming energy before 0 if it consumes all of the task's energy costs including excess costs.
        /// </summary>
        /// <param name="framesToDamageMultiplier">The damage per frame of the DoT effect</param>
        /// <returns>This, for chaining</returns>
        public EnergyManagerTaskState ConsumeDotAndAutoReserve(decimal framesToDamageMultiplier)
        {
            if (RegularEnergy > RemainingMaximumEnergyNeeded)
            {
                return ConsumeRegularEnergy(RemainingMaximumCost);
            }
            int autoRefillDotDamage = CalculateAutoRefillDotDamage(framesToDamageMultiplier);
            return ConsumeRegularEnergy(RegularEnergy)
                .UseAllReserves()
                .InternalReduceRegularEnergy(autoRefillDotDamage);
        }

        /// <summary>
        /// Calculates how much DoT damage would be taken during an auto-refill triggered at 0 regular energy, 
        /// based on the current reserve energy and given an active DoT effect with the provided damage per frame.
        /// </summary>
        /// <param name="framesToDamageMultiplier">The damage per frame of the DoT effect</param>
        /// <returns></returns>
        public int CalculateAutoRefillDotDamage(decimal framesToDamageMultiplier)
        {
            int autoRefillAmount = Math.Min(MaxRegularEnergy, ReserveEnergy);
            int framesToAutoRefill =  (int)Math.Ceiling(autoRefillAmount / EnergyManager.Rules.AutoReserveRefillPerFrame);
            return (int)(framesToAutoRefill * framesToDamageMultiplier);
        }

        /// <summary>
        /// <para>
        /// Uses reserve energy, attempting to reach the provided target regular energy amount.
        /// This method will respect the internal EnergyManager's ReserveRefillLeeway, which means it may overshoot a little,
        /// or undershoot a little to avoid hitting max energy and wasting a lot of reserves.
        /// </para>
        /// <para>
        /// If the caller prefers to empty reserves instead of undershooting as this method does,
        /// they can check the result and subsequently call <see cref="UseAllReserves"/> where appropriate.
        /// </para>
        /// <para>
        /// Take note that this method only converts reserves to regular energy, but does not spend any of that regular energy towards task progress.
        /// </para>
        /// </summary>
        /// <param name="targetRegularEnergyAmount">The regular energy amount that is the target for this operation</param>
        /// <returns>This, for chaining</returns>
        public EnergyManagerTaskState UseReservesWithTargetRegularEnergyAmount(int targetRegularEnergyAmount)
        {
            // If we don't have enough reserves to reach the target, pretend the target is the limit of the reserves
            if (ReserveEnergy < targetRegularEnergyAmount - RegularEnergy)
            {
                targetRegularEnergyAmount = RegularEnergy + ReserveEnergy;
            }

            // If the target is beyond the max regular energy, it can't be reached
            if (targetRegularEnergyAmount > MaxRegularEnergy)
            {
                // Have the player aim for max regular energy, and undershoot that by the ReserveRefillLeeway
                int resultingRegularEnergy = MaxRegularEnergy - EnergyManager.ReserveRefillLeewayEnergy;
                // Obviously, we'll do something only if the resulting amount is more than we originally had
                if (resultingRegularEnergy > RegularEnergy)
                {
                    InternalConvertReserveEnergy(resultingRegularEnergy - RegularEnergy);
                }

                return this;
            }
            // If target is reachable, decide how to apply the leeway.
            else
            {
                // If emptying the entire reserves overshoots by less than the reserve refill leeway, go ahead and do that
                int emptyReservesResult = RegularEnergy + ReserveEnergy;
                if (emptyReservesResult <= targetRegularEnergyAmount + EnergyManager.ReserveRefillLeewayEnergy)
                {
                    return UseAllReserves();
                }

                // At this point we know we won't empty reserves.
                // We'll end up at either targetEnergy + leeway, or maxEnergy - leeway
                // If targetEnergy+leeway hits the max, it's too dangerous to go for so we'll aim at max energy instead and end up at maxEnergy-leeway
                int resultingEnergy = targetRegularEnergyAmount + EnergyManager.ReserveRefillLeewayEnergy;
                if (resultingEnergy >= MaxRegularEnergy)
                {
                    resultingEnergy = MaxRegularEnergy - EnergyManager.ReserveRefillLeewayEnergy;
                }
                int reservesToUse = resultingEnergy - RegularEnergy;
                InternalConvertReserveEnergy(reservesToUse);
                return this;
            }
        }

        /// <summary>
        /// Uses the provided amount of reserves (or as much as available if that's not possible) directly towards task progress
        /// (rather than converting it to regular energy))
        /// </summary>
        /// <param name="amount">The amount of reserve energy to convert</param>
        /// <returns>This, for chaining</returns>
        public EnergyManagerTaskState UseReservesTowardsProgress(int amount)
        {
            amount = Math.Min(amount, ReserveEnergy);
            InternalReduceReserveEnergy(amount);
            RemainingMinimumCost -= amount;
            return this;
        }

        /// <summary>
        /// Empties reserves, regardless of whether that wastes energy by overflowing the max regular energy.
        /// </summary>
        /// <returns>This, for chaining</returns>
        public EnergyManagerTaskState UseAllReserves()
        {
            // Use reserves till correct regular energy
            int missingEnergy = MaxRegularEnergy - RegularEnergy;
            int energyGain = Math.Min(missingEnergy, ReserveEnergy);
            InternalConvertReserveEnergy(energyGain);

            // Empty any excess reserves
            int wastedReserve = ReserveEnergy;
            InternalReduceReserveEnergy(wastedReserve);

            return this;
        }

        /// <summary>
        /// In a non-interruptible DoT context (where auto-reserves can trigger without interrupting the task) where the game is currently paused,
        /// this method decides what wastes less reserve energy between emptying all resreves immediately and triggering auto-reserves with the DoT active.
        /// It then does one of those two options and returns once the reserves are empty.
        /// </summary>
        /// <param name="framesToDamageMultiplier">The damage per frame of the DoT effect</param>
        /// <returns>This, for chaining</returns>
        public EnergyManagerTaskState PreUseAllReservesOrAutoReserveDot(decimal framesToDamageMultiplier)
        {
            // We have two options: Fully drain reserves ahead of time, or fully drain reserves via auto reserves
            // Figure out which one wastes less energy...
            int autoRefillWastedEnergy = CalculateAutoRefillDotDamage(framesToDamageMultiplier)
                + Math.Max(0, MaxRegularEnergy - ReserveEnergy);
            int preRefillWastedEnergy = ReserveEnergy + RegularEnergy - ReserveEnergy;

            if (autoRefillWastedEnergy > preRefillWastedEnergy)
            {
                // Pre-refilling wastes less energy, do that
                UseAllReserves();
            }
            else
            {
                // Auto-refill wastes less energy, do that
                ConsumeDotAndAutoReserve(framesToDamageMultiplier);
            }
            return this;
        }

        /// <summary>
        /// <para>
        /// Consumes as much regular energy as possible towards task completion, stopping either when the minimum energy threshold is reached
        /// or when the maximum cost has been spent.
        /// </para>
        /// <para>
        /// Because this may hit the threshold and interrupt the task, only call this to conclude the task.
        /// </para>
        /// </summary>
        /// <returns>The resulting energy variation, if the resulting state corresponds to a completed task; null otherwise</returns>
        public ReadOnlyResourceCount CompleteWithRegularEnergy()
        {
            int availableEnergyToConsume = RegularEnergy - MinimumEnergyThreshold;
            int energyToConsume = Math.Min(availableEnergyToConsume, RemainingMaximumCost);
            ConsumeRegularEnergy(energyToConsume);

            if (TaskCompletedSuccessfully)
            {
                return ResultingEnergyVariation;
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// <para>
        /// Attempts to complete the task with regular energy.
        /// If not possible, will then attempt to complete with reserve energy, over-using reserves by up to the provided excess (if available).
        /// This means this method could leave the inner state with more regular energy than it started with.
        /// </para>
        /// <para>
        /// This method does not manage pause and refill cycles, and assumes that it's possible to convert the reserve energy 
        /// without wasting any by overflowing regular energy and without failing the task.
        /// </para>
        /// <para>
        /// Because this may hit the threshold and interrupt the task, only call this to conclude the task.
        /// </para>
        /// </summary>
        /// <param name="excessReserveToSpend">The amount by which to overshoot the use of reserves</param>
        /// <returns></returns>
        public ReadOnlyResourceCount CompleteWithReserveOverUse(int excessReserveToSpend)
        {
            ReadOnlyResourceCount regularResult = CompleteWithRegularEnergy();
            if (regularResult != null)
            {
                return regularResult;
            }

            // Use enough reserves to complete the task (if available).
            // In theory this may interrupt the attempt but in practice we can pretend the reserves were used before the interruption.
            UseReservesTowardsProgress(MinimumReserveUseNeeded);

            // Over-use reserves if some is available
            InternalConvertReserveEnergy(Math.Min(excessReserveToSpend, ReserveEnergy));
            // Some or all of that over-use may be spent as excess cost, this will take care of that
            return CompleteWithRegularEnergy();

        }

        /// <summary>
        /// Reduces regular energy in the internal ResourceCounts.
        /// Does not check if the resources are available.
        /// Does not apply the energy towards task progress.
        /// </summary>
        /// <param name="amount">Amount to reduce regular energy by</param>
        /// <returns>This, for chaining</returns>
        private EnergyManagerTaskState InternalReduceRegularEnergy(int amount)
        {
            CurrentResources.ApplyAmountReduction(Models.Items.RechargeableResourceEnum.RegularEnergy, amount);
            EnergyVariation.ApplyAmountReduction(Models.Items.RechargeableResourceEnum.RegularEnergy, amount);
            return this;
        }

        /// <summary>
        /// Reduces reserve energy in the internal ResourceCounts. Does not check if the resources are available.
        /// </summary>
        /// <param name="amount">Amount to reduce reserve energy by</param>
        /// <returns>This, for chaining</returns>
        private EnergyManagerTaskState InternalReduceReserveEnergy(int amount)
        {
            CurrentResources.ApplyAmountReduction(Models.Items.RechargeableResourceEnum.ReserveEnergy, amount);
            EnergyVariation.ApplyAmountReduction(Models.Items.RechargeableResourceEnum.ReserveEnergy, amount);
            return this;
        }

        /// <summary>
        /// Converts reserve energy to regular energy in the internal ResourceCounts. Does not check if the resources are available.
        /// </summary>
        /// <param name="amount">Amount to convert</param>
        /// <returns>This, for chaining</returns>
        private EnergyManagerTaskState InternalConvertReserveEnergy(int amount)
        {
            CurrentResources.ApplyConvertReservesToRegularEnergy(amount);
            EnergyVariation.ApplyConvertReservesToRegularEnergy(amount);
            return this;
        }
    }
}
