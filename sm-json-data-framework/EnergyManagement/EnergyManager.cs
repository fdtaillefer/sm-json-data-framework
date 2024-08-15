using sm_json_data_framework.InGameStates;
using sm_json_data_framework.Models.Enemies;
using sm_json_data_framework.Models.Items;
using sm_json_data_framework.Options;
using sm_json_data_framework.Rules;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Security;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace sm_json_data_framework.EnergyManagement
{
    /// <summary>
    /// A class that is responsible for figuring out how energy should be spent according to an in-game state and some configurations.
    /// </summary>
    public class EnergyManager
    {
        public EnergyManager(ReadOnlyLogicalOptions logicalOptions, SuperMetroidRules rules)
        {
            // The CanUseReservesToShinespark logical option is based on a tech that's a bit more specific than we use it for
            // The techs in the model will be expanded upon eventually and we'll be able to break those up when we catch up to those changes
            UseReservesMidTask = logicalOptions.CanUseReservesToShinespark;
            UsePartialReserves = logicalOptions.CanUsePartialReserves;
            PauseSpamLeewayFrames = logicalOptions.PauseSpamLeewayFrames;
            ReserveRefillLeewayEnergy = logicalOptions.ReserveRefillLeewayEnergy;
            ReserveUsageTimingLeewayFrames = logicalOptions.ReserveUsageTimingLeewayFrames;
            IframesToAvoidDoubleHit = logicalOptions.IframesToAvoidDoubleHit;

            Rules = rules;
        }

        public SuperMetroidRules Rules { get; }
        public bool UseReservesMidTask { get; }
        public bool UsePartialReserves { get; }
        public int PauseSpamLeewayFrames { get; }
        public int ReserveRefillLeewayEnergy { get; }
        public int ReserveUsageTimingLeewayFrames { get; }
        public int IframesToAvoidDoubleHit { get; }

        public ReadOnlyResourceCount CalculatePunctualEnemyDamageEnergyVariation(ReadOnlyInGameState inGameState, EnemyAttack attack, int hits,
            bool canActBeforeFirstHit)
        {
            int damagePerHit = Rules.CalculateEnemyDamage(inGameState, attack);
            return CalculatePunctualDamageEnergyVariation(inGameState, damagePerHit, hits, canActBeforeFirstHit);
        }

        /// <summary>
        /// Attempts to calculate a way to successfully execute a task that involves taking a number of environment damage hits, using the internal rules and logical options
        /// (especially to decide how reserves can be managed).
        /// </summary>
        /// <param name="inGameState">An in-game state, used for current and maximum energy counts and to determine the environment damage</param>
        /// <param name="environmentDamageEnum">The source of the environment damage</param>
        /// <param name="hits">The number of hits to take</param>
        /// <returns>The resulting resource variation if successful, null otherwise</returns>
        public ReadOnlyResourceCount CalculatePunctualEnvironmentDamageEnergyVariation(ReadOnlyInGameState inGameState, PunctualEnvironmentDamageEnum environmentDamageEnum, int hits,
            bool canActBeforeFirstHit)
        {
            int damagePerHit = Rules.CalculatePunctualEnvironmentDamage(inGameState, environmentDamageEnum);
            return CalculatePunctualDamageEnergyVariation(inGameState, damagePerHit, hits, canActBeforeFirstHit);
        }

        /// <summary>
        /// Attempts to calculate a way to successfully execute a task that involves taking a number of hits worth a given amount of damage each, 
        /// using the internal rules and logical options (especially to decide how reserves can be managed).
        /// </summary>
        /// <param name="inGameState">An in-game state, used for current and maximum energy counts and to determine the environment damage</param>
        /// <param name="damagePerHit">The amount of damage taken for each hit</param>
        /// <param name="hits">The number of hits to take</param>
        /// <returns>The resulting resource variation if successful, null otherwise</returns>
        private ReadOnlyResourceCount CalculatePunctualDamageEnergyVariation(ReadOnlyInGameState inGameState, int damagePerHit, int hits,
            bool canActBeforeFirstHit)
        {
            int totalDamage = damagePerHit * hits;

            EnergyManagerTaskState state = new EnergyManagerTaskState(this, inGameState, totalDamage, excessEnergyCost: 0, minimumEnergyThreshold: 1);

            // If there's enough regular energy to take all the damage, simply do that
            if (state.RegularEnergy > totalDamage)
            {
                return state.CompleteWithRegularEnergy();
            }

            // At this point we will need to use some reserves, determine exactly how and whether it's doable
            bool canActBefore = canActBeforeFirstHit;
            for (int i = 0; i < hits; i++)
            {
                // If there's enough regular energy to take this hit, do that
                if (state.RegularEnergy > damagePerHit)
                {
                    state.ConsumeRegularEnergy(damagePerHit);
                }
                // We need to use reserves to survive this hit
                else
                {
                    // If we have no more reserves, nothing can save us. Fail.
                    if (state.ReserveEnergy <= 0)
                    {
                        return null;
                    }

                    int reservesNeeded = damagePerHit + 1 - state.RegularEnergy;
                    // Pre-use reserves if we can and if it even allows us to survive the next hit
                    if (canActBefore && state.ReserveEnergy >= reservesNeeded)
                    {
                        // Use reserves manually. To simplify, try to use as much reserves as needed for all the hits, if max energy allows
                        state.UseReservesWithTargetRegularEnergyAmount(state.RemainingMinimumEnergyNeeded);
                    }
                    
                    // Take the hit (this will trigger auto-reserves if needed and kill us if we can't survive)
                    state.TakePunctualHit(damagePerHit);

                    // If this last hit killed us, it's a fail
                    if (state.RegularEnergy <= 0)
                    {
                        return null;
                    }
                }

                // We're assuming i-frames are always enough to use reserves between two hits
                canActBefore = UseReservesMidTask;
            }

            return state.ResultingEnergyVariation;
        }

        /// <summary>
        /// Attempts to calculate a way to successfully execute a task that involves taking damage over time, using the internal rules and logical options
        /// (especially to decide how reserves can be managed).
        /// </summary>
        /// <param name="inGameState">An in-game state, used for current and maximum energy counts and to determine the damage over time</param>
        /// <param name="dotEnum">The type of damage-over-time that is happening</param>
        /// <param name="frames">The number of frames that the damage-over-time lasts for</param>
        /// <param name="canActBefore">If true, the play can do other things before the damage over time begins. If false, it's starting immediately.</param>
        /// <returns></returns>
        public ReadOnlyResourceCount CalculateDamageOverTimeEnergyVariation(ReadOnlyInGameState inGameState, DamageOverTimeEnum dotEnum, int frames, bool canActBefore)
        {
            decimal framesToDamageMultiplier = Rules.GetDamagePerFrame(inGameState, dotEnum);
            int minimumEnergyThreshold = Rules.GetDamageOverTimeMinimumEnergyThreshold(dotEnum);
            bool interruptible = Rules.IsInterruptibleDot(dotEnum);
            return CalculateFrameDamageTaskEnergyVariation(inGameState, frames, 0, framesToDamageMultiplier, minimumEnergyThreshold, interruptible, canActBefore);
        }

        /// <summary>
        /// Attempts to calculate a way to successfully execute at least the non-excess portion of a shinespark, using the internal rules and logical options
        /// (especially to decide how reserves can be managed).
        /// </summary>
        /// <param name="inGameState">An in-game state, used for current and maximum energy counts</param>
        /// <param name="frames">The number of frames that the shinespark can go for before it bonks</param>
        /// <param name="excessFrames">The number of frames the shinespark can go for *after accomplishing its task* before it bonks</param>
        /// <param name="canActBefore">If true, the play can do other things before the shinespark activates. If false, it's starting immediately.</param>
        /// <returns>The resulting resource variation if successful, null otherwise</returns>
        public ReadOnlyResourceCount CalculateShinesparkEnergyVariation(ReadOnlyInGameState inGameState, int frames, int excessFrames, bool canActBefore)
        {
            decimal framesToDamageMultiplier = Rules.GetBaseDamagePerFrame(DamageOverTimeEnum.Shinespark);
            int minimumEnergyThreshold = Rules.GetDamageOverTimeMinimumEnergyThreshold(DamageOverTimeEnum.Shinespark);
            bool interruptible = Rules.IsInterruptibleDot(DamageOverTimeEnum.Shinespark);
            return CalculateFrameDamageTaskEnergyVariation(inGameState, frames, excessFrames, framesToDamageMultiplier, minimumEnergyThreshold, interruptible, canActBefore);
        }

        /// <summary>
        /// Attempts to calculate a way to successfully execute a task that involves damage-over-time.
        /// This method will notice if the task is impossible based on its parameters and this EnergyManager's rules and logical options.
        /// </summary>
        /// <param name="inGameState">An in-game state, used for current and maximum energy counts</param>
        /// <param name="frames">The number of frames that the task can go for (including excess frames)</param>
        /// <param name="excessFrames">The number of excess frames of the task. Excess frames will be spent if the energy is available,
        /// but don't need to be spent for the task to succeed.</param>
        /// <param name="framesToDamageMultiplier">A multiplier to express how much damage happens over a number of frames</param>
        /// <param name="minimumEnergyThreshold">A minimum amount of energy below which Samus cannot go. Going below this amount means failing the task.</param>
        /// <param name="interruptibleDot">Indicates whether the DoT effect is one that cuts off at the energy threshold, rather than triggering reserves or death
        /// below it</param>
        /// <param name="canActBefore">If true, the player can do other things before the shinespark activates. If false, it's starting immediately.</param>
        /// <returns>The resulting resource variation if successful, null otherwise</returns>
        private ReadOnlyResourceCount CalculateFrameDamageTaskEnergyVariation(ReadOnlyInGameState inGameState, int frames, int excessFrames,
            decimal framesToDamageMultiplier, int minimumEnergyThreshold, bool interruptibleDot, bool canActBefore)
        {
            int minimumEnergyCost = (int)((frames - excessFrames) * framesToDamageMultiplier);
            int excessEnergyCost = (int)(excessFrames * framesToDamageMultiplier);
            EnergyManagerTaskState state = new EnergyManagerTaskState(this, inGameState, minimumEnergyCost, excessEnergyCost, minimumEnergyThreshold);

            // If there's no damage to take, don't bother with these calculations
            if (framesToDamageMultiplier == 0)
            {
                return state.ResultingEnergyVariation;
            }

            decimal damageToFramesMultiplier = 1 / framesToDamageMultiplier;

            // If there's enough regular energy to do at least the non-excess portion of the task, simply do the task with regular energy
            if (state.HasEnoughRegularEnergyToComplete)
            {
                return state.CompleteWithRegularEnergy();
            }
            
            // If we don't have enough regular energy, we need reserves. If that's still not enough, then it's not possible to spend the energy for the task.
            if (state.RegularEnergy + state.ReserveEnergy < state.RemainingMinimumEnergyNeeded)
            {
                return null;
            }

            // We have enough energy to do the task in a vacuum, but circumstances might not allow us to use all that reserve energy
            // Let's see if we can use reserves before we begin

            int energyLeftAtThresholdPause = minimumEnergyThreshold + (interruptibleDot? 1: 0) + ((int)(framesToDamageMultiplier * ReserveUsageTimingLeewayFrames));

            if (canActBefore)
            {
                if (UsePartialReserves)
                {
                    state.UseReservesWithTargetRegularEnergyAmount(state.RemainingMinimumEnergyNeeded);
                }
                else
                {
                    // We can't use partial reserves - so whatever we do here, it will fully drain reserves and then we'll have to try to resolve the task

                    // The first question here is whether we can use our reserves mid-task
                    // The best moment to pause mid-task is just before the minimum energy threshold, though we have a tolerance to respect
                    if (UseReservesMidTask && state.RegularEnergy > energyLeftAtThresholdPause)
                    {
                        // The timing works, and we can initiate the pause before we start if needed, so progress task until that point then empty reserves
                        int energyToConsume = state.RegularEnergy - energyLeftAtThresholdPause;
                        state.ConsumeRegularEnergy(state.RegularEnergy - energyToConsume)
                            .UseAllReserves();
                    }
                    else
                    {
                        // We can't use partial reserves and we can't use reserves mid-task (either logically or because the timing doesn't work)
                        if (interruptibleDot)
                        {
                            // We can't even rely of auto reserves (the task interrupts instead) so best we can do is fully empty reserves ahead of time
                            state.UseAllReserves();
                        }
                        else
                        {
                            // We have two options: Fully drain reserves ahead of time, or fully drain reserves via auto reserves
                            state.PreUseAllReservesOrAutoReserveDot(framesToDamageMultiplier);
                        }
                    }
                }
            }

            // If we've spent enough to complete the task, conclude now (spending any available excess)
            // Likewise, if we've run out of reserves, there's nothing left to do and we can only try to conclude
            if (state.RemainingMinimumCost <= 0 || state.ReserveEnergy <= 0)
            {
                return state.CompleteWithRegularEnergy();
            }

            // At this point we've done all we could before beginning the task, don't have enough regular energy to complete, but we have reserves left
            if (!UseReservesMidTask)
            {
                // If we can't use reserves mid-task, our only option left is auto-reserves if possible, otherwise it's a fail
                if (interruptibleDot)
                {
                    return null;
                }
                else
                {
                    return state.ConsumeDotAndAutoReserve(framesToDamageMultiplier)
                        .CompleteWithRegularEnergy();
                }
            }

            // The maximum energy we can expect the player to safely refill to using reserves
            int maxRegularEnergyReserveRefillPoint = state.MaxRegularEnergy - ReserveRefillLeewayEnergy;

            // The number of in-game frames that will elapse during an unpause/pause cycle (including re-pause leeway)
            int effectivePauseCycleFrames = Rules.FramesToPauseAndUnpause + PauseSpamLeewayFrames;

            // The amount of damage that occurs during an unpause/pause cycle (including re-pause leeway)
            int pauseCycleDamage = (int)(effectivePauseCycleFrames * framesToDamageMultiplier);
            // The amount of damage that occurs during the pause portion of the pause cycle (including pause spamming leeway)
            int pauseDamage = (int)(framesToDamageMultiplier * (Rules.PauseFadeOutFrames + PauseSpamLeewayFrames));


            int maxRegenPerPauseCycle = maxRegularEnergyReserveRefillPoint - minimumEnergyThreshold - (interruptibleDot? 1: 0);

            // If we don't have time to pause before the task interrupts, we can not use any more reserves
            // This will probably be a fail
            if (!canActBefore && state.RegularEnergy <= pauseDamage + minimumEnergyThreshold + (interruptibleDot ? 1 : 0))
            {
                return state.CompleteWithRegularEnergy();
            }

            // At this point we know we have reserves left and we know we're able to pause at least once mid-task

            // If we can regen as much energy during a pause cycle as we spend, then we are not limited in the number of pauses we can do
            if (maxRegenPerPauseCycle >= pauseCycleDamage)
            {
                // Over-use reserves by our precision margin
                return state.CompleteWithReserveOverUse(ReserveRefillLeewayEnergy);
            }

            // We spend more energy in a pause cycle than the amount we are able to restore, so we can only pause once and not necessarily optimally.
            // The optimal moment to pause in this case is as late as possible before hitting the minimum threshold,
            // but we will calculate a pause that's earlier than that by a leeway
            int energyAtTargetedPause = minimumEnergyThreshold + (interruptibleDot ? 1 : 0) + (int)(ReserveUsageTimingLeewayFrames * framesToDamageMultiplier);
            int energyToSpendUntilTargetedPause = state.RegularEnergy - energyAtTargetedPause;
            // Not sure if this can happen, but this means we would need to pause earlier than we can even start the task, so the task will fail
            if (energyToSpendUntilTargetedPause <= 0)
            {
                return null;
            }

            // Execute a portion of the task until we pause.
            // How this happens depends on whether we can start the pause pre-emptively or not.
            if (canActBefore)
            {
                // Since we can pause pre-emptively, just go to the targeted pause
                state.ConsumeRegularEnergy(energyAtTargetedPause);
            }
            else
            {
                int framesBeforeTargetedPause = energyToSpendUntilTargetedPause * (int)damageToFramesMultiplier;
                int framesBeforeSpammedPause = Rules.PauseFadeOutFrames + PauseSpamLeewayFrames;

                // We can't pause pre-emptively. We've checked before that there's time to pause before we hit the threshold.
                // We'll do a targeted pause, unless spamming actually takes us further into the task
                int framesBeforeActualPause = Math.Max(framesBeforeTargetedPause, framesBeforeSpammedPause);
                int energyToSpendBeforePause = (int)(framesBeforeActualPause * framesToDamageMultiplier);

                state.ConsumeRegularEnergy(energyToSpendBeforePause);
            }

            // We have now paused mid-task and won't be able to un-pause and re-pause again
            // Try to use optimal amount of energy (within leeway)
            state.UseReservesWithTargetRegularEnergyAmount(state.RegularEnergy + state.RemainingMinimumCost);

            // If that was enough, complete the task
            if (state.MinimumReserveUseNeeded <= 0)
            {
                return state.CompleteWithRegularEnergy();
            }

            // If that wasn't enough, the final action depends on whether auto-reserves can trigger in this task
            if(interruptibleDot)
            {
                // Task will interrupt rather than trigger reserves
                // Use all remaining reserves with no concern for any wasted energy and try to complete the task
                return state.UseAllReserves()
                    .CompleteWithRegularEnergy();
            }
            else
            {
                // Task does not interrupt and can trigger auto-reserves
                // We have two options: Fully drain reserves ahead of time, or fully drain reserves via auto reserves
                return state.PreUseAllReservesOrAutoReserveDot(framesToDamageMultiplier)
                    .CompleteWithRegularEnergy();
            }
        }
    }
}
