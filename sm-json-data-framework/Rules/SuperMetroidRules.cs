﻿using sm_json_data_framework.Models;
using sm_json_data_framework.Models.Enemies;
using sm_json_data_framework.Models.InGameStates;
using sm_json_data_framework.Models.Items;
using sm_json_data_framework.Models.Requirements;
using sm_json_data_framework.Models.Rooms.Nodes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace sm_json_data_framework.Rules
{
    /// <summary>
    /// A repository of game rules. This offers some base values, as well as calculations based on game rules.
    /// By default, this class offers rules that align with the vanilla game, but a lot of it can be overridden via a subclass if desired.
    /// </summary>
    public class SuperMetroidRules
    {
        private const int DROP_RATE_DIVIDER = 102;

        public virtual int ThornDamage { get => 16; }

        public virtual int SpikeDamage { get => 60; }

        public virtual int HibashiDamage { get => 30; }

        /// <summary>
        /// The energy at which a shinespark is interrupted
        /// </summary>
        public virtual int ShinesparkEnergyLimit { get => 29; }

        /// <summary>
        /// The number of tiles that are lost when combining two runways via a room transition
        /// </summary>
        public virtual decimal RoomTransitionTilesLost { get => 1M; }

        // Apparently the gain varies between 8.5 and 9 pixels, which is just over half a tile.
        /// <summary>
        /// The number of tiles gained from having one open end in a runway.
        /// </summary>
        public virtual decimal TilesGainedPerOpenEnd { get => 0.5M; }

        // According to zqxk, charging on gentle up tiles multiplies required distance by 27/32. So each tile is worth 32/27 of a tile
        /// <summary>
        /// A multiplier that can be applied to a number of gentle up tiles to obtain the equivalent run length in flat tiles.
        /// </summary>
        public virtual decimal GentleUpTileMultiplier { get => 32M / 27M; }

        // According to zqxk, charging on steep up tiles multiplies required distance by 3/4. So each tile is worth 4/3 of a tile
        /// <summary>
        /// A multiplier that can be applied to a number of steep up tiles to obtain the equivalent run length in flat tiles.
        /// </summary>
        public virtual decimal SteepUpTileMultiplier { get => 4M / 3M; }

        // STITCHME Don't know about downward slopes yet
        /// <summary>
        /// A multiplier that can be applied to a number of gentle down tiles to obtain the equivalent run length in flat tiles.
        /// </summary>
        public virtual decimal GentleDownTileMultiplier { get => 1M; }

        // STITCHME Don't know about downward slopes yet
        /// <summary>
        /// A multiplier that can be applied to a number of steep down tiles to obtain the equivalent run length in flat tiles.
        /// </summary>
        public virtual decimal SteepDownTileMultiplier { get => 1; }

        /// <summary>
        /// Calculates the real in-game drop rate (out of DROP_RATE_DIVIDER) for the provided initial drop rates, after adjusting for the elimination of some drops.
        /// </summary>
        /// <param name="enemyDrops">The enemy drops (out of DROP_RATE_DIVIDER), as they would be if all resources can drop.</param>
        /// <param name="unneededDrops">The enumeration of drops that cannot drop (due to their resource being full).</param>
        /// <returns>The adjusted drop rates (out of DROP_RATE_DIVIDER).</returns>
        public virtual EnemyDrops CalculateEffectiveDropRates(EnemyDrops enemyDrops, IEnumerable<EnemyDropEnum> unneededDrops)
        {
            // Calculate the base drop rates for the formula, replacing unneeded drops by a rate of 0.
            // Our drop rates are out of DROP_RATE_DIVIDER. The formula needs them to be out of 255.
            EnemyDrops baseHexDropRates = new EnemyDrops();
            // Tier 1 drops
            baseHexDropRates.NoDrop = enemyDrops.NoDrop * 255 / DROP_RATE_DIVIDER;
            baseHexDropRates.SmallEnergy = unneededDrops.Contains(EnemyDropEnum.SMALL_ENERGY) ? 0M : enemyDrops.SmallEnergy * 255 / DROP_RATE_DIVIDER;
            baseHexDropRates.BigEnergy = unneededDrops.Contains(EnemyDropEnum.BIG_ENERGY) ? 0M : enemyDrops.BigEnergy * 255 / DROP_RATE_DIVIDER;
            baseHexDropRates.Missile = unneededDrops.Contains(EnemyDropEnum.MISSILE) ? 0M : enemyDrops.Missile * 255 / DROP_RATE_DIVIDER;

            // Tier 2 drops
            baseHexDropRates.Super = unneededDrops.Contains(EnemyDropEnum.SUPER) ? 0M : enemyDrops.Super * 255 / DROP_RATE_DIVIDER;
            baseHexDropRates.PowerBomb = unneededDrops.Contains(EnemyDropEnum.POWER_BOMB) ? 0M : enemyDrops.PowerBomb * 255 / DROP_RATE_DIVIDER;

            // Create functions for calculating effective drop rates. One for tier 1 drops and one for tier 2 drops.

            // Formula for tier one drops is (255 - super - pb) / (small + big + missile + nothing) * (current item), truncated
            Func<EnemyDrops, decimal, decimal> calculateTierOneRate = (baseHexDropRates, individualHexDropRate) =>
            {
                decimal tierTwoValue = 255 - baseHexDropRates.Super - baseHexDropRates.PowerBomb;
                decimal tierOneValue = baseHexDropRates.SmallEnergy + baseHexDropRates.BigEnergy
                    + baseHexDropRates.Missile + baseHexDropRates.NoDrop;
                decimal result = decimal.Floor(tierTwoValue / tierOneValue * individualHexDropRate);
                return result * DROP_RATE_DIVIDER / 255;
            };

            // Formula for tier two is simply the drop rate itself
            Func<EnemyDrops, decimal, decimal> calculateTierTwoRate = (baseHexDropRates, baseHexDropRate) =>
            {
                return baseHexDropRate * DROP_RATE_DIVIDER / 255;
            };

            // Calculate new drop rates using the appropriate calculation, except for no drop
            EnemyDrops returnValue = new EnemyDrops
            {
                SmallEnergy = calculateTierOneRate(baseHexDropRates, baseHexDropRates.SmallEnergy),
                BigEnergy = calculateTierOneRate(baseHexDropRates, baseHexDropRates.BigEnergy),
                Missile = calculateTierOneRate(baseHexDropRates, baseHexDropRates.Missile),
                Super = calculateTierTwoRate(baseHexDropRates, baseHexDropRates.Super),
                PowerBomb = calculateTierTwoRate(baseHexDropRates, baseHexDropRates.PowerBomb)
            };

            // No drop is just whatever's not another type of drop. It grabs the leftover from truncating on top of its own increase.
            returnValue.NoDrop = DROP_RATE_DIVIDER - returnValue.SmallEnergy - returnValue.BigEnergy - returnValue.Missile - returnValue.Super - returnValue.PowerBomb;

            return returnValue;
        }

        /// <summary>
        /// Converts the provided drop rate into a drop % chance.
        /// </summary>
        /// <param name="dropRate">The drop rate, out of DROP_RATE_DIVIDER</param>
        /// <returns></returns>
        public virtual decimal ConvertDropRateToPercent(decimal dropRate)
        {
            return dropRate / DROP_RATE_DIVIDER;
        }

        /// <summary>
        /// Given an enumeration of full rechargeable resources, returns the enemy drops that aren't needed because the associated resources are full.
        /// </summary>
        /// <param name="fullResources">Enumeration of full resources</param>
        /// <returns></returns>
        public virtual IEnumerable<EnemyDropEnum> GetUnneededDrops(IEnumerable<RechargeableResourceEnum> fullResources)
        {
            return Enum.GetValues(typeof(EnemyDropEnum))
                .Cast<EnemyDropEnum>()
                .Where(drop => {
                    IEnumerable<RechargeableResourceEnum> dropResources = drop.GetRechargeableResources();
                    // Return all drops that actually refill anything (so never return "no drop")
                    // and for which all refilled resources are already full
                    return dropResources.Any()
                        && dropResources.Intersect(fullResources).Count() == dropResources.Count();
                });
        }

        /// <summary>
        /// Given an enumeration of full consumable resources, returns the enemy drops that aren't needed because the associated resources are full.
        /// </summary>
        /// <param name="fullResources">Enumeration of full resources</param>
        /// <returns></returns>
        public virtual IEnumerable<EnemyDropEnum> GetUnneededDrops(IEnumerable<ConsumableResourceEnum> fullResources)
        {
            return Enum.GetValues(typeof(EnemyDropEnum))
                .Cast<EnemyDropEnum>()
                .Where(drop => {
                    ConsumableResourceEnum? dropResource = drop.GetConsumableResource();
                    // Return all drops whose consumable resource is already full
                    return dropResource != null
                        && fullResources.Contains((ConsumableResourceEnum)dropResource);
                });
        }

        /// <summary>
        /// Returns what quantity of its associated resource the provided enemy drop restores.
        /// </summary>
        /// <param name="enemyDrop">The enemy drop to evaluate</param>
        /// <returns></returns>
        public virtual int GetDropResourceCount(EnemyDropEnum enemyDrop)
        {
            return enemyDrop switch
            {
                EnemyDropEnum.NO_DROP => 0,
                EnemyDropEnum.SMALL_ENERGY => 5,
                EnemyDropEnum.BIG_ENERGY => 20,
                EnemyDropEnum.MISSILE => 2,
                EnemyDropEnum.SUPER => 1,
                EnemyDropEnum.POWER_BOMB => 1,
                _ => throw new Exception($"Unrecognized enemy drop {enemyDrop}")
            };
        }

        /// <summary>
        /// <para>Returns the enumeration of items found in the provided inGameState which would be responsible
        /// for a reduction in the damage of some damage source where Gravity and Varia both offer reduction, but Gravity supersedes Varia.</para>
        /// <para>Does not return Varia when Gravity is available.</para>
        /// </summary>
        /// <param name="model">A model that can be used to obtain data about the current game configuration.</param>
        /// <param name="inGameState">An in-game state describing the current player situation, notably knowing what items the player has.</param>
        /// <returns></returns>
        private IEnumerable<Item> GetDamageReducingItemsWhenGravitySupersedesVaria(SuperMetroidModel model, InGameState inGameState)
        {
            if (inGameState.HasGravitySuit())
            {
                return new [] { model.Items[SuperMetroidModel.GRAVITY_SUIT_NAME] };
            }
            else if (inGameState.HasVariaSuit())
            {
                return new [] { model.Items[SuperMetroidModel.VARIA_SUIT_NAME] };
            }
            else
            {
                return new Item[] { };
            }
        }

        /// <summary>
        /// Returns the enumeration of items found in the provided inGameState which would be responsible
        /// for a reduction in the damage of some damage source where Gravity and Varia both offer reduction, but Gravity is manually turned off even if available. 
        /// </summary>
        /// <param name="model">A model that can be used to obtain data about the current game configuration.</param>
        /// <param name="inGameState">An in-game state describing the current player situation, notably knowing what items the player has.</param>
        /// <returns></returns>
        private IEnumerable<Item> GetDamageReducingItemsWhenGravityTurnedOff(SuperMetroidModel model, InGameState inGameState)
        {
            if (inGameState.HasVariaSuit())
            {
                return new[] { model.Items[SuperMetroidModel.VARIA_SUIT_NAME] };
            }
            else
            {
                return new Item[] { };
            }
        }

        /// <summary>
        /// <para>Returns the enumeration of items found in the provided inGameState which would be responsible
        /// for a reduction in the damage of some damage source where Gravity and Varia both offer reduction, but Varia supersedes Gravity.</para>
        /// <para>Does not return Gravity when Varia is available.</para>
        /// </summary>
        /// <param name="model">A model that can be used to obtain data about the current game configuration.</param>
        /// <param name="inGameState">An in-game state describing the current player situation, notably knowing what items the player has.</param>
        /// <returns></returns>
        private IEnumerable<Item> GetDamageReducingItemsWhenVariaSupersedesGravity(SuperMetroidModel model, InGameState inGameState)
        {
            if (inGameState.HasVariaSuit())
            {
                return new [] { model.Items[SuperMetroidModel.VARIA_SUIT_NAME] };
            }
            else if (inGameState.HasGravitySuit())
            {
                return new [] { model.Items[SuperMetroidModel.GRAVITY_SUIT_NAME] };
            }
            else
            {
                return new Item[] { };
            }
        }

        /// <summary>
        /// Calculates and returns the environmental damage Samus would take for the provided in-game state and base environmental damage. This method is intended
        /// for environment-based punctual hits, not damage over time.
        /// </summary>
        /// <param name="inGameState">An in-game state describing the current player situation, notably knowing what items the player has.</param>
        /// <param name="baseDamage">The base damage</param>
        /// <returns>The calculated damage</returns>
        public virtual int CalculateEnvironmentalDamage(InGameState inGameState, int baseDamage)
        {
            bool hasVaria = inGameState.HasVariaSuit();
            bool hasGravity = inGameState.HasGravitySuit();
            if (hasGravity)
            {
                return baseDamage / 4;
            }
            else if (hasVaria)
            {
                return baseDamage / 2;
            }
            else
            {
                return baseDamage;
            }
        }

        /// <summary>
        /// <para>Returns the enumeration of items found in the provided inGameState which would be responsible
        /// for a reduction in the damage returned by <see cref="CalculateEnvironmentalDamage(InGameState, int)"/>.<para>
        /// <para>Does not return items that would reduce the damage, but are made irrelevant by another item's reduction</para>
        /// </summary>
        /// <param name="model">A model that can be used to obtain data about the current game configuration.</param>
        /// <param name="inGameState">An in-game state describing the current player situation, notably knowing what items the player has.</param>
        /// <returns></returns>
        public virtual IEnumerable<Item> GetEnvironmentalDamageReducingItems(SuperMetroidModel model, InGameState inGameState)
        {
            // Gravity supercedes Varia
            return GetDamageReducingItemsWhenGravitySupersedesVaria(model, inGameState);
        }

        /// <summary>
        /// Calculates and returns the damage Samus would take for spending the provided duration in a heated room, given the provided in-game state.
        /// </summary>
        /// <param name="inGameState">An in-game state describing the current player situation, notably knowing what items the player has.</param>
        /// <param name="heatFrames">The duration (in frames) of the heat exposure whose damage to calculate.</param>
        /// <returns>The calculated damage</returns>
        public virtual int CalculateHeatDamage(InGameState inGameState, int heatFrames)
        {
            bool hasVaria = inGameState.HasVariaSuit();
            bool hasGravity = inGameState.HasGravitySuit();
            if (hasGravity || hasVaria)
            {
                return 0;
            }
            else
            {
                return heatFrames / 4;
            }
        }

        /// <summary>
        /// <para>Returns the enumeration of items found in the provided inGameState which would be responsible
        /// for a reduction in the damage returned by <see cref="CalculateHeatDamage(InGameState, int)"/>.<para>
        /// <para>Does not return items that would reduce the damage, but are made irrelevant by another item's reduction</para>
        /// </summary>
        /// <param name="model">A model that can be used to obtain data about the current game configuration.</param>
        /// <param name="inGameState">An in-game state describing the current player situation, notably knowing what items the player has.</param>
        /// <returns></returns>
        public virtual IEnumerable<Item> GetHeatDamageReducingItems(SuperMetroidModel model, InGameState inGameState)
        {
            // Gravity and Varia are equivalent. Varia's more iconic for heat reduction, so let's prioritize it.
            return GetDamageReducingItemsWhenVariaSupersedesGravity(model, inGameState);
        }

        /// <summary>
        /// Calculates and returns the damage Samus would take for spending the provided duration in lava, given the provided in-game state.
        /// </summary>
        /// <param name="inGameState">An in-game state describing the current player situation, notably knowing what items the player has.</param>
        /// <param name="lavaFrames">The duration (in frames) of the lava exposure whose damage to calculate.</param>
        /// <returns>The calculated damage</returns>
        public virtual int CalculateLavaDamage(InGameState inGameState, int lavaFrames)
        {
            bool hasVaria = inGameState.HasVariaSuit();
            bool hasGravity = inGameState.HasGravitySuit();
            if (hasGravity)
            {
                return 0;
            }
            else if (hasVaria)
            {
                return lavaFrames / 4;
            }
            else
            {
                return lavaFrames / 2;
            }
        }

        /// <summary>
        /// Calculates and returns the damage Samus would take for spending the provided duration in lava while undergoing lava physics 
        /// (i.e. with Gravity Suit turned off if available), given the provided in-game state.
        /// </summary>
        /// <param name="inGameState">An in-game state describing the current player situation, notably knowing what items the player has.</param>
        /// <param name="lavaPhysicsFrames">The duration (in frames) of the lava exposure whose damage to calculate.</param>
        /// <returns>The calculated damage</returns>
        public virtual int CalculateLavaPhysicsDamage(InGameState inGameState, int lavaPhysicsFrames)
        {
            bool hasVaria = inGameState.HasVariaSuit();
            if (hasVaria)
            {
                return lavaPhysicsFrames / 4;
            }
            else
            {
                return lavaPhysicsFrames / 2;
            }
        }

        /// <summary>
        /// <para>Returns the enumeration of items found in the provided inGameState which would be responsible
        /// for a reduction in the damage returned by <see cref="CalculateLavaDamage(InGameState, int)"/>.<para>
        /// <para>Does not return items that would reduce the damage, but are made irrelevant by another item's reduction</para>
        /// </summary>
        /// <param name="model">A model that can be used to obtain data about the current game configuration.</param>
        /// <param name="inGameState">An in-game state describing the current player situation, notably knowing what items the player has.</param>
        /// <returns></returns>
        public virtual IEnumerable<Item> GetLavaDamageReducingItems(SuperMetroidModel model, InGameState inGameState)
        {
            // Gravity supercedes Varia
            return GetDamageReducingItemsWhenGravitySupersedesVaria(model, inGameState);
        }

        /// <summary>
        /// <para>Returns the enumeration of items found in the provided inGameState which would be responsible
        /// for a reduction in the damage returned by <see cref="CalculateLavaPhysicsDamage(InGameState, int)"/>.<para>
        /// <para>Does not return items that would reduce the damage, but are made irrelevant by another item's reduction</para>
        /// </summary>
        /// <param name="model">A model that can be used to obtain data about the current game configuration.</param>
        /// <param name="inGameState">An in-game state describing the current player situation, notably knowing what items the player has.</param>
        /// <returns></returns>
        public virtual IEnumerable<Item> GetLavaPhysicsDamageReducingItems(SuperMetroidModel model, InGameState inGameState)
        {
            // Gravity turned off
            return GetDamageReducingItemsWhenGravityTurnedOff(model, inGameState);
        }

        /// <summary>
        /// Calculates and returns the damage Samus would take for spending the provided duration in acid, given the provided in-game state.
        /// </summary>
        /// <param name="inGameState">An in-game state describing the current player situation, notably knowing what items the player has.</param>
        /// <param name="acidFrames">The duration (in frames) of the acid exposure whose damage to calculate.</param>
        /// <returns>The calculated damage</returns>
        public virtual int CalculateAcidDamage(InGameState inGameState, int acidFrames)
        {
            bool hasVaria = inGameState.HasVariaSuit();
            bool hasGravity = inGameState.HasGravitySuit();
            if (hasGravity)
            {
                return acidFrames * 3 / 8;
            }
            else if (hasVaria)
            {
                return acidFrames * 3 / 4;
            }
            else
            {
                return acidFrames * 6 / 4;
            }
        }

        /// <summary>
        /// <para>Returns the enumeration of items found in the provided inGameState which would be responsible
        /// for a reduction in the damage returned by <see cref="CalculateAcidDamage(InGameState, int)"/>.<para>
        /// <para>Does not return items that would reduce the damage, but are made irrelevant by another item's reduction</para>
        /// </summary>
        /// <param name="model">A model that can be used to obtain data about the current game configuration.</param>
        /// <param name="inGameState">An in-game state describing the current player situation, notably knowing what items the player has.</param>
        /// <returns></returns>
        public virtual IEnumerable<Item> GetAcidDamageReducingItems(SuperMetroidModel model, InGameState inGameState)
        {
            // Gravity supercedes Varia
            return GetDamageReducingItemsWhenGravitySupersedesVaria(model, inGameState);
        }

        /// <summary>
        /// Calculates and returns the damage Samus would take for spending the provided duration grappled to a broken Draygon turret, given the provided in-game state.
        /// </summary>
        /// <param name="inGameState">An in-game state describing the current player situation, notably knowing what items the player has.</param>
        /// <param name="electricityFrames">The duration (in frames) of the electricity exposure whose damage to calculate.</param>
        /// <returns>The calculated damage</returns>
        public virtual int CalculateElectricityGrappleDamage(InGameState inGameState, int electricityFrames)
        {
            bool hasVaria = inGameState.HasVariaSuit();
            bool hasGravity = inGameState.HasGravitySuit();
            if (hasGravity)
            {
                return electricityFrames / 4;
            }
            else if (hasVaria)
            {
                return electricityFrames / 2;
            }
            else
            {
                return electricityFrames;
            }
        }

        /// <summary>
        /// <para>Returns the enumeration of items found in the provided inGameState which would be responsible
        /// for a reduction in the damage returned by <see cref="CalculateElectricityGrappleDamage(InGameState, int)"/>.<para>
        /// <para>Does not return items that would reduce the damage, but are made irrelevant by another item's reduction</para>
        /// </summary>
        /// <param name="model">A model that can be used to obtain data about the current game configuration.</param>
        /// <param name="inGameState">An in-game state describing the current player situation, notably knowing what items the player has.</param>
        /// <returns></returns>
        public virtual IEnumerable<Item> GetElectricityGrappleDamageReducingItems(SuperMetroidModel model, InGameState inGameState)
        {
            // Gravity supercedes Varia
            return GetDamageReducingItemsWhenGravitySupersedesVaria(model, inGameState);
        }

        /// <summary>
        /// Calculates and returns the damage Samus would take for executing a shinespark of the provided duration n times, given the provided in-game state.
        /// </summary>
        /// <param name="inGameState">An in-game state describing the current player situation, notably knowing what items the player has.</param>
        /// <param name="shinesparkFrames">The duration (in frames) of the shinespark whose damage to calculate.</param>
        /// <param name="times">The number of times the shinespark will be performed.</param>
        /// <returns>The calculated damage</returns>
        public virtual int CalculateShinesparkDamage(InGameState inGameState, int shinesparkFrames, int times = 1)
        {
            return shinesparkFrames * times;
        }

        /// <summary>
        /// <para>Returns the minimum energy Samus must have before initiating a shinespark n times, for that shinespark to complete to the end all n times.
        /// By default, that is the energy cost of the shinespark * times + the energy at which a shinespark is interrupted.</para>
        /// <para>This does return 0 if shinesparkFrames is 0, interpreting that as meaning there is no shinespark.</para>
        /// </summary>
        /// <param name="shinesparkFrames">The duration (in frames) of the shinespark. 0 means no shinespark is being executed.</param>
        /// <param name="times">The number of times the shinespark will be performed.</param>
        /// <returns></returns>
        public virtual int CalculateEnergyNeededForShinespark(int shinesparkFrames, int times = 1)
        {
            return shinesparkFrames == 0 ? 0 : shinesparkFrames*times + ShinesparkEnergyLimit;
        }

        /// <summary>
        /// Calculates and returns how much damage the provided enemy attack would do to Samus, given the provided in-game state.
        /// </summary>
        /// <param name="inGameState">An in-game state describing the current player situation, notably knowing what items the player has.</param>
        /// <param name="attack">The enemy attack whose damage to calculate</param>
        /// <returns>The calculated damage</returns>
        public virtual int CalculateEnemyDamage(InGameState inGameState, EnemyAttack attack)
        {
            bool hasVaria = inGameState.HasVariaSuit();
            bool hasGravity = inGameState.HasGravitySuit();

            if (hasGravity && attack.AffectedByGravity)
            {
                return attack.BaseDamage / 4;
            }
            else if (hasVaria && attack.AffectedByVaria)
            {
                return attack.BaseDamage / 2;
            }
            else
            {
                return attack.BaseDamage;
            }
        }

        /// <summary>
        /// <para>Returns the enumeration of items found in the provided inGameState which would be responsible
        /// for a reduction in the damage returned by <see cref="CalculateEnemyDamage(InGameState, EnemyAttack)"/>.<para>
        /// <para>Does not return items that would reduce the damage, but are made irrelevant by another item's reduction</para>
        /// </summary>
        /// <param name="model">A model that can be used to obtain data about the current game configuration.</param>
        /// <param name="inGameState">An in-game state describing the current player situation, notably knowing what items the player has.</param>
        /// <returns></returns>
        public virtual IEnumerable<Item> GetEnemyDamageReducingItems(SuperMetroidModel model, InGameState inGameState, EnemyAttack enemyAttack)
        {
            // What we return depends not only on the suits available, but also on the attack
            if(enemyAttack.AffectedByGravity && enemyAttack.AffectedByVaria)
            {
                return GetDamageReducingItemsWhenGravitySupersedesVaria(model, inGameState);
            }

            if (enemyAttack.AffectedByGravity && inGameState.HasGravitySuit())
            {
                return new Item[] { model.Items[SuperMetroidModel.GRAVITY_SUIT_NAME] };
            }

            if (enemyAttack.AffectedByVaria && inGameState.HasVariaSuit())
            {
                return new Item[] { model.Items[SuperMetroidModel.VARIA_SUIT_NAME] };
            }

            return new Item[] { };
        }

        /// <summary>
        /// Calculates the effective length of the provided runway, in flat tiles.
        /// </summary>
        /// <param name="runway">The runway to calculate</param>
        /// <param name="tilesSavedWithStutter"><para>The number of tiles the player is expected to be saving using a stutter-step.
        /// If a portion of the runway to use begins downhill, it will be deemed unusable for a stutter-step and the effective runway length
        /// will be reduced accordingly.</para>
        /// <para>For example: If the configuration says the player is expected to use stutter-steps to save 1 tile in a runway,
        /// and to charge successfully in a 15-tile runway, and the provided runway has 15 tiles but starts on a downward slope, then
        /// the player is unable to save that one tile using thw stutter-step. So the effective length will be 14 tiles instead
        /// to reflect that the runway doesn't allow a shine charge with the current configuration.</para></param>
        /// <returns></returns>
        public virtual decimal CalculateEffectiveRunwayLength(IRunway runway, decimal tilesSavedWithStutter)
        {
            decimal runwayLength = runway.Length
                + runway.OpenEnds * TilesGainedPerOpenEnd
                + (GentleUpTileMultiplier - 1) * runway.GentleUpTiles
                + (SteepUpTileMultiplier - 1) * runway.SteepUpTiles
                + (GentleDownTileMultiplier - 1) * runway.GentleDownTiles
                + (SteepDownTileMultiplier - 1) * runway.SteepDownTiles;

            // We are going to consider downward slopes as unusable for stutter, and adjust the effective length accordingly.
            // It's apparently possible to stutter for reduced effectiveness on downard slopes, but we'll ignore it for now.
            runwayLength -= Math.Min(tilesSavedWithStutter, runway.StartingDownTiles);
            
            return runwayLength;
        }

        /// <summary>
        /// Calculates the effective length of the provided runway, used in the opposite direction, in flat tiles.
        /// </summary>
        /// <param name="runway">The runway to calculate in reverse</param>
        /// <param name="tilesSavedWithStutter">The number of tiles the player is expected to be saving using a stutter-step.
        /// If a portion of the runway to use begins downhill, it will be deemed unusable for a stutter-step and the effective runway length
        /// will be reduced accordingly.</param>
        /// <returns></returns>
        public decimal CalculateEffectiveReversedRunwayLength(IRunway runway, decimal tilesSavedWithStutter)
        {
            return CalculateEffectiveRunwayLength(ReverseRunway(runway), tilesSavedWithStutter);
        }

        /// <summary>
        /// Calculates the effective length of the provided runway, in flat tiles. Tries the runway in both directions and returns the longest result.
        /// </summary>
        /// <param name="runway">The runway to calculate, checking both directions</param>
        /// <param name="tilesSavedWithStutter">The number of tiles the player is expected to be saving using a stutter-step.
        /// If a portion of the runway to use begins downhill, it will be deemed unusable for a stutter-step and the effective runway length
        /// will be reduced accordingly.</param>
        /// <returns></returns>
        public decimal CalculateEffectiveReversibleRunwayLength(IRunway runway, decimal tilesSavedWithStutter)
        {
            return Math.Max(CalculateEffectiveRunwayLength(runway, tilesSavedWithStutter), CalculateEffectiveReversedRunwayLength(runway, tilesSavedWithStutter));
        }

        /// <summary>
        /// Creates and returns the equivalent of the provided runway, but used in the opposite direction.
        /// </summary>
        /// <param name="runway">The runway to reverse</param>
        /// <returns></returns>
        public virtual IRunway ReverseRunway(IRunway runway)
        {
            return new Runway {
                Length = runway.Length,
                EndingUpTiles = runway.StartingDownTiles,
                GentleDownTiles = runway.GentleUpTiles,
                GentleUpTiles = runway.GentleDownTiles,
                OpenEnds = runway.OpenEnds,
                StartingDownTiles = runway.EndingUpTiles,
                SteepDownTiles = runway.SteepUpTiles,
                SteepUpTiles = runway.SteepDownTiles
            };
        }

        /// <summary>
        /// Returns how the game adjusts a resource when picking up an associated expansion item.
        /// </summary>
        /// <param name="resource">The resource to get the behavior for.</param>
        /// <returns></returns>
        public virtual ExpansionPickupRestoreBehaviorEnum GetExpansionPickupRestoreBehavior(RechargeableResourceEnum resource)
        {
            return resource switch
            {
                RechargeableResourceEnum.Missile => ExpansionPickupRestoreBehaviorEnum.ADD_PICKED_UP,
                RechargeableResourceEnum.Super => ExpansionPickupRestoreBehaviorEnum.ADD_PICKED_UP,
                RechargeableResourceEnum.PowerBomb => ExpansionPickupRestoreBehaviorEnum.ADD_PICKED_UP,
                RechargeableResourceEnum.RegularEnergy => ExpansionPickupRestoreBehaviorEnum.REFILL,
                RechargeableResourceEnum.ReserveEnergy => ExpansionPickupRestoreBehaviorEnum.NOTHING,
                _ => throw new Exception($"Unrecognized rechargeable resource {resource}")
            };
        }
    }
}
