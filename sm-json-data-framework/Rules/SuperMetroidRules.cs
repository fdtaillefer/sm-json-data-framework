using sm_json_data_framework.Models;
using sm_json_data_framework.Models.Enemies;
using sm_json_data_framework.Models.InGameStates;
using sm_json_data_framework.Models.Items;
using sm_json_data_framework.Models.Requirements;
using sm_json_data_framework.Models.Requirements.ObjectRequirements.Integers;
using sm_json_data_framework.Models.Rooms.Nodes;
using sm_json_data_framework.Utils;
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
            EnemyDrops baseHexDropRates = new EnemyDrops(
                // Tier 1 drops
                noDrop: enemyDrops.NoDrop * 255 / DROP_RATE_DIVIDER, 
                smallEnergy: unneededDrops.Contains(EnemyDropEnum.SmallEnergy) ? 0M : enemyDrops.SmallEnergy * 255 / DROP_RATE_DIVIDER, 
                bigEnergy: unneededDrops.Contains(EnemyDropEnum.BigEnergy) ? 0M : enemyDrops.BigEnergy * 255 / DROP_RATE_DIVIDER, 
                missile: unneededDrops.Contains(EnemyDropEnum.Missile) ? 0M : enemyDrops.Missile * 255 / DROP_RATE_DIVIDER,
                // Tier 2 drops
                super: unneededDrops.Contains(EnemyDropEnum.Super) ? 0M : enemyDrops.Super * 255 / DROP_RATE_DIVIDER, 
                powerBomb: unneededDrops.Contains(EnemyDropEnum.PowerBomb) ? 0M : enemyDrops.PowerBomb * 255 / DROP_RATE_DIVIDER
            );

            // Create functions for calculating effective drop rates. One for tier 1 drops and one for tier 2 drops.

            // Formula for tier one drops is (255 - super - pb) / (small + big + missile + nothing) * (current item), truncated
            Func<EnemyDrops, decimal, decimal> calculateTierOneRate = (baseHexDropRates, individualHexDropRate) =>
            {
                // A value of 0 stays at 0 regardless of any other drops.
                // Returning immediately also makes it impossible to get a divide by 0 if all tier 1 drops are at 0.
                if (individualHexDropRate == 0)
                {
                    return 0;
                }
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
            decimal smallEnergy = calculateTierOneRate(baseHexDropRates, baseHexDropRates.SmallEnergy);
            decimal bigEnergy = calculateTierOneRate(baseHexDropRates, baseHexDropRates.BigEnergy);
            decimal missile = calculateTierOneRate(baseHexDropRates, baseHexDropRates.Missile);
            decimal super = calculateTierTwoRate(baseHexDropRates, baseHexDropRates.Super);
            decimal powerBomb = calculateTierTwoRate(baseHexDropRates, baseHexDropRates.PowerBomb);
            EnemyDrops returnValue = new EnemyDrops(
                smallEnergy: smallEnergy,
                bigEnergy: bigEnergy,
                missile:missile,
                super:super,
                powerBomb:powerBomb,
                // No drop is just whatever's not another type of drop. It grabs the leftover from truncating on top of its own increase.
                noDrop: DROP_RATE_DIVIDER - smallEnergy - bigEnergy - missile - super - powerBomb
            );

            return returnValue;
        }

        /// <summary>
        /// Converts the provided drop rate into a drop chance (out of 1, rather than a percent).
        /// </summary>
        /// <param name="dropRate">The drop rate, out of DROP_RATE_DIVIDER</param>
        /// <returns></returns>
        public virtual decimal ConvertDropRateToProportion(decimal dropRate)
        {
            return dropRate / DROP_RATE_DIVIDER;
        }

        /// <summary>
        /// Given an enumeration of full rechargeable resources, returns the enemy drops that aren't needed because the associated resources are full.
        /// </summary>
        /// <param name="fullResources">Enumeration of full resources</param>
        /// <returns></returns>
        public virtual ISet<EnemyDropEnum> GetUnneededDrops(IEnumerable<RechargeableResourceEnum> fullResources)
        {
            return Enum.GetValues<EnemyDropEnum>()
                .Where(drop => {
                    IEnumerable<RechargeableResourceEnum> dropResources = drop.GetRechargeableResources();
                    // Return all drops that actually refill anything (so never return "no drop")
                    // and for which all refilled resources are already full
                    return dropResources.Any()
                        && dropResources.Intersect(fullResources).Count() == dropResources.Count();
                })
                .ToHashSet();
        }

        /// <summary>
        /// Given an enumeration of full consumable resources, returns the enemy drops that aren't needed because the associated resources are full.
        /// </summary>
        /// <param name="fullResources">Enumeration of full resources</param>
        /// <returns></returns>
        public virtual ISet<EnemyDropEnum> GetUnneededDrops(IEnumerable<ConsumableResourceEnum> fullResources)
        {
            return Enum.GetValues<EnemyDropEnum>()
                .Cast<EnemyDropEnum>()
                .Where(drop => {
                    ConsumableResourceEnum? dropResource = drop.GetConsumableResource();
                    // Return all drops whose consumable resource is already full
                    return dropResource != null
                        && fullResources.Contains((ConsumableResourceEnum)dropResource);
                })
                .ToHashSet();
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
                EnemyDropEnum.NoDrop => 0,
                EnemyDropEnum.SmallEnergy => 5,
                EnemyDropEnum.BigEnergy => 20,
                EnemyDropEnum.Missile => 2,
                EnemyDropEnum.Super => 1,
                EnemyDropEnum.PowerBomb => 1,
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
        private IEnumerable<Item> GetDamageReducingItemsWhenGravitySupersedesVaria(SuperMetroidModel model, ReadOnlyInGameState inGameState)
        {
            if (inGameState.Inventory.HasGravitySuit())
            {
                return new[] { model.Items[SuperMetroidModel.GRAVITY_SUIT_NAME] };
            }
            else if (inGameState.Inventory.HasVariaSuit())
            {
                return new[] { model.Items[SuperMetroidModel.VARIA_SUIT_NAME] };
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
        private IEnumerable<Item> GetDamageReducingItemsWhenGravityTurnedOff(SuperMetroidModel model, ReadOnlyInGameState inGameState)
        {
            if (inGameState.Inventory.HasVariaSuit())
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
        private IEnumerable<Item> GetDamageReducingItemsWhenVariaSupersedesGravity(SuperMetroidModel model, ReadOnlyInGameState inGameState)
        {
            if (inGameState.Inventory.HasVariaSuit())
            {
                return new[] { model.Items[SuperMetroidModel.VARIA_SUIT_NAME] };
            }
            else if (inGameState.Inventory.HasGravitySuit())
            {
                return new[] { model.Items[SuperMetroidModel.GRAVITY_SUIT_NAME] };
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
        public virtual int CalculateEnvironmentalDamage(ReadOnlyInGameState inGameState, int baseDamage)
        {
            return CalculateEnvironmentalDamage(baseDamage, inGameState.Inventory.HasVariaSuit(), inGameState.Inventory.HasGravitySuit());
        }

        /// <summary>
        /// Calculates and returns the environmental damage Samus would take for the provided base environmental damage, in the best case scenario
        /// (presumably, both suits).
        /// This method is intended for environment-based punctual hits, not damage over time.
        /// </summary>
        /// <param name="baseDamage">The base damage</param>
        /// <param name="removedItems">A set of item names that cannot logically be found in game</param>
        /// <returns>The calculated damage</returns>
        public virtual int CalculateBestCaseEnvironmentalDamage(int baseDamage, IReadOnlySet<string> removedItems)
        {
            return CalculateEnvironmentalDamage(baseDamage, !removedItems.ContainsVariaSuit(), !removedItems.ContainsGravitySuit());
        }

        /// <summary>
        /// Calculates and returns the environmental damage Samus would take for the provided base environmental damage, in the worst case scenario
        /// (presumably, suitless).
        /// This method is intended for environment-based punctual hits, not damage over time.
        /// </summary>
        /// <param name="baseDamage">The base damage</param>
        /// <param name="startingInventory">The inventory of items that Samus is logically guaranteed to have</param>
        /// <returns>The calculated damage</returns>
        public virtual int CalculateWorstCaseEnvironmentalDamage(int baseDamage, ReadOnlyItemInventory startingInventory)
        {
            return CalculateEnvironmentalDamage(baseDamage, startingInventory.HasVariaSuit(), startingInventory.HasGravitySuit());
        }

        private int CalculateEnvironmentalDamage(int baseDamage, bool hasVaria, bool hasGravity)
        {
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
        /// for a reduction in the damage returned by <see cref="CalculateEnvironmentalDamage(ReadOnlyUnfinalizedInGameState, int)"/>.<para>
        /// <para>Does not return items that would reduce the damage, but are made irrelevant by another item's reduction</para>
        /// </summary>
        /// <param name="model">A model that can be used to obtain data about the current game configuration.</param>
        /// <param name="inGameState">An in-game state describing the current player situation, notably knowing what items the player has.</param>
        /// <returns></returns>
        public virtual IEnumerable<Item> GetEnvironmentalDamageReducingItems(SuperMetroidModel model, ReadOnlyInGameState inGameState)
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
        public virtual int CalculateHeatDamage(ReadOnlyInGameState inGameState, int heatFrames)
        {
            return CalculateHeatDamage(heatFrames, inGameState.Inventory.HasVariaSuit(), inGameState.Inventory.HasGravitySuit());
        }

        /// <summary>
        /// Calculates and returns the damage Samus would take for spending the provided duration in a heated room, in the best case scenario
        /// (presumably both suits).
        /// </summary>
        /// <param name="heatFrames">The duration (in frames) of the heat exposure whose damage to calculate.</param>
        /// <param name="removedItems">A set of item names that cannot logically be found in game</param>
        /// <returns>The calculated damage</returns>
        public virtual int CalculateBestCaseHeatDamage(int heatFrames, IReadOnlySet<string> removedItems)
        {
            return CalculateHeatDamage(heatFrames, !removedItems.ContainsVariaSuit(), !removedItems.ContainsGravitySuit());
        }

        /// <summary>
        /// Calculates and returns the damage Samus would take for spending the provided duration in a heated room, in the worst case scenario
        /// (presumably suitless).
        /// </summary>
        /// <param name="heatFrames">The duration (in frames) of the heat exposure whose damage to calculate.</param>
        /// <param name="startingInventory">The inventory of items that Samus is logically guaranteed to have</param>
        /// <returns>The calculated damage</returns>
        public virtual int CalculateWorstCaseHeatDamage(int heatFrames, ReadOnlyItemInventory startingInventory)
        {
            return CalculateHeatDamage(heatFrames, startingInventory.HasVariaSuit(), startingInventory.HasGravitySuit());
        }

        private int CalculateHeatDamage(int heatFrames, bool hasVaria, bool hasGravity)
        {
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
        /// for a reduction in the damage returned by <see cref="CalculateHeatDamage(ReadOnlyInGameState, int)"/>.<para>
        /// <para>Does not return items that would reduce the damage, but are made irrelevant by another item's reduction</para>
        /// </summary>
        /// <param name="model">A model that can be used to obtain data about the current game configuration.</param>
        /// <param name="inGameState">An in-game state describing the current player situation, notably knowing what items the player has.</param>
        /// <returns></returns>
        public virtual IEnumerable<Item> GetHeatDamageReducingItems(SuperMetroidModel model, ReadOnlyInGameState inGameState)
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
        public virtual int CalculateLavaDamage(ReadOnlyInGameState inGameState, int lavaFrames)
        {
            return CalculateLavaDamage(lavaFrames, inGameState.Inventory.HasVariaSuit(), inGameState.Inventory.HasGravitySuit());
        }

        /// <summary>
        /// Calculates and returns the damage Samus would take for spending the provided duration in lava, in the best case scenario
        /// (presumably both suits).
        /// </summary>
        /// <param name="lavaFrames">The duration (in frames) of the lava exposure whose damage to calculate.</param>
        /// <param name="removedItems">A set of item names that cannot logically be found in game</param>
        /// <returns>The calculated damage</returns>
        public virtual int CalculateBestCaseLavaDamage(int lavaFrames, IReadOnlySet<string> removedItems)
        {
            return CalculateLavaDamage(lavaFrames, !removedItems.ContainsVariaSuit(), !removedItems.ContainsGravitySuit());
        }

        /// <summary>
        /// Calculates and returns the damage Samus would take for spending the provided duration in lava, in the worst case scenario
        /// (presumably suitless).
        /// </summary>
        /// <param name="lavaFrames">The duration (in frames) of the lava exposure whose damage to calculate.</param>
        /// <param name="startingInventory">The inventory of items that Samus is logically guaranteed to have</param>
        /// <returns>The calculated damage</returns>
        public virtual int CalculateWorstCaseLavaDamage(int lavaFrames, ReadOnlyItemInventory startingInventory)
        {
            return CalculateLavaDamage(lavaFrames, startingInventory.HasVariaSuit(), startingInventory.HasGravitySuit());
        }

        private int CalculateLavaDamage(int lavaFrames, bool hasVaria, bool hasGravity)
        {
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
        public virtual int CalculateLavaPhysicsDamage(ReadOnlyInGameState inGameState, int lavaPhysicsFrames)
        {
            return CalculateLavaPhysicsDamage(lavaPhysicsFrames, inGameState.Inventory.HasVariaSuit());
        }

        /// <summary>
        /// Calculates and returns the damage Samus would take for spending the provided duration in lava while undergoing lava physics 
        /// (i.e. with Gravity Suit turned off if available), in the best case scenario (presumably with Varia).
        /// </summary>
        /// <param name="lavaPhysicsFrames">The duration (in frames) of the lava exposure whose damage to calculate.</param>
        /// <param name="removedItems">A set of item names that cannot logically be found in game</param>
        /// <returns>The calculated damage</returns>
        public virtual int CalculateBestCaseLavaPhysicsDamage(int lavaPhysicsFrames, IReadOnlySet<string> removedItems)
        {
            return CalculateLavaPhysicsDamage(lavaPhysicsFrames, !removedItems.ContainsVariaSuit());
        }

        /// <summary>
        /// Calculates and returns the damage Samus would take for spending the provided duration in lava while undergoing lava physics 
        /// (i.e. with Gravity Suit turned off if available), in the worst case scenario (presumably suitless).
        /// </summary>
        /// <param name="lavaPhysicsFrames">The duration (in frames) of the lava exposure whose damage to calculate.</param>
        /// <param name="startingInventory">The inventory of items that Samus is logically guaranteed to have</param>
        /// <returns>The calculated damage</returns>
        public virtual int CalculateWorstCaseLavaPhysicsDamage(int lavaPhysicsFrames, ReadOnlyItemInventory startingInventory)
        {
            return CalculateLavaPhysicsDamage(lavaPhysicsFrames, startingInventory.HasVariaSuit());
        }

        private int CalculateLavaPhysicsDamage(int lavaPhysicsFrames, bool hasVaria)
        {
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
        /// for a reduction in the damage returned by <see cref="CalculateLavaDamage(ReadOnlyInGameState, int)"/>.<para>
        /// <para>Does not return items that would reduce the damage, but are made irrelevant by another item's reduction</para>
        /// </summary>
        /// <param name="model">A model that can be used to obtain data about the current game configuration.</param>
        /// <param name="inGameState">An in-game state describing the current player situation, notably knowing what items the player has.</param>
        /// <returns></returns>
        public virtual IEnumerable<Item> GetLavaDamageReducingItems(SuperMetroidModel model, ReadOnlyInGameState inGameState)
        {
            // Gravity supercedes Varia
            return GetDamageReducingItemsWhenGravitySupersedesVaria(model, inGameState);
        }

        /// <summary>
        /// <para>Returns the enumeration of items found in the provided inGameState which would be responsible
        /// for a reduction in the damage returned by <see cref="CalculateLavaPhysicsDamage(ReadOnlyInGameState, int)"/>.<para>
        /// <para>Does not return items that would reduce the damage, but are made irrelevant by another item's reduction</para>
        /// </summary>
        /// <param name="model">A model that can be used to obtain data about the current game configuration.</param>
        /// <param name="inGameState">An in-game state describing the current player situation, notably knowing what items the player has.</param>
        /// <returns></returns>
        public virtual IEnumerable<Item> GetLavaPhysicsDamageReducingItems(SuperMetroidModel model, ReadOnlyInGameState inGameState)
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
        public virtual int CalculateAcidDamage(ReadOnlyInGameState inGameState, int acidFrames)
        {
            return CalculateAcidDamage(acidFrames, inGameState.Inventory.HasVariaSuit(), inGameState.Inventory.HasGravitySuit());
        }

        /// <summary>
        /// Calculates and returns the damage Samus would take for spending the provided duration in acid, in the best case scenario
        /// (presumably both suits).
        /// </summary>
        /// <param name="acidFrames">The duration (in frames) of the acid exposure whose damage to calculate.</param>
        /// <param name="removedItems">A set of item names that cannot logically be found in game</param>
        /// <returns>The calculated damage</returns>
        public virtual int CalculateBestCaseAcidDamage(int acidFrames, IReadOnlySet<string> removedItems)
        {
            return CalculateAcidDamage(acidFrames, !removedItems.ContainsVariaSuit(), !removedItems.ContainsGravitySuit());
        }

        /// <summary>
        /// Calculates and returns the damage Samus would take for spending the provided duration in acid, in the worst case scenario
        /// (presumably suitless).
        /// </summary>
        /// <param name="acidFrames">The duration (in frames) of the acid exposure whose damage to calculate.</param>
        /// <param name="startingInventory">The inventory of items that Samus is logically guaranteed to have</param>
        /// <returns>The calculated damage</returns>
        public virtual int CalculateWorstCaseAcidDamage(int acidFrames, ReadOnlyItemInventory startingInventory)
        {
            return CalculateAcidDamage(acidFrames, startingInventory.HasVariaSuit(), startingInventory.HasGravitySuit());
        }

        private int CalculateAcidDamage(int acidFrames, bool hasVaria, bool hasGravity)
        {
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
        /// for a reduction in the damage returned by <see cref="CalculateAcidDamage(ReadOnlyInGameState, int)"/>.<para>
        /// <para>Does not return items that would reduce the damage, but are made irrelevant by another item's reduction</para>
        /// </summary>
        /// <param name="model">A model that can be used to obtain data about the current game configuration.</param>
        /// <param name="inGameState">An in-game state describing the current player situation, notably knowing what items the player has.</param>
        /// <returns></returns>
        public virtual IEnumerable<Item> GetAcidDamageReducingItems(SuperMetroidModel model, ReadOnlyInGameState inGameState)
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
        public virtual int CalculateElectricityGrappleDamage(ReadOnlyInGameState inGameState, int electricityFrames)
        {
            return CalculateElectricityGrappleDamage(electricityFrames, inGameState.Inventory.HasVariaSuit(), inGameState.Inventory.HasGravitySuit());
        }

        /// <summary>
        /// Calculates and returns the damage Samus would take for spending the provided duration grappled to a broken Draygon turret, in the best case scenario
        /// (presumably both suits).
        /// </summary>
        /// <param name="electricityFrames">The duration (in frames) of the electricity exposure whose damage to calculate.</param>
        /// <param name="removedItems">A set of item names that cannot logically be found in game</param>
        /// <returns>The calculated damage</returns>
        public virtual int CalculateBestCaseElectricityGrappleDamage(int electricityFrames, IReadOnlySet<string> removedItems)
        {
            return CalculateElectricityGrappleDamage(electricityFrames, !removedItems.ContainsVariaSuit(), !removedItems.ContainsGravitySuit());
        }

        /// <summary>
        /// Calculates and returns the damage Samus would take for spending the provided duration grappled to a broken Draygon turret, in the worst case scenario
        /// (presumably suitless).
        /// </summary>
        /// <param name="electricityFrames">The duration (in frames) of the electricity exposure whose damage to calculate.</param>
        /// <param name="startingInventory">The inventory of items that Samus is logically guaranteed to have</param>
        /// <returns>The calculated damage</returns>
        public virtual int CalculateWorstCaseElectricityGrappleDamage(int electricityFrames, ReadOnlyItemInventory startingInventory)
        {
            return CalculateElectricityGrappleDamage(electricityFrames, startingInventory.HasVariaSuit(), startingInventory.HasGravitySuit());
        }

        private int CalculateElectricityGrappleDamage(int electricityFrames, bool hasVaria, bool hasGravity)
        {
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
        /// for a reduction in the damage returned by <see cref="CalculateElectricityGrappleDamage(ReadOnlyInGameState, int)"/>.<para>
        /// <para>Does not return items that would reduce the damage, but are made irrelevant by another item's reduction</para>
        /// </summary>
        /// <param name="model">A model that can be used to obtain data about the current game configuration.</param>
        /// <param name="inGameState">An in-game state describing the current player situation, notably knowing what items the player has.</param>
        /// <returns></returns>
        public virtual IEnumerable<Item> GetElectricityGrappleDamageReducingItems(SuperMetroidModel model, ReadOnlyInGameState inGameState)
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
        public virtual int CalculateShinesparkDamage(ReadOnlyInGameState inGameState, int shinesparkFrames, int times = 1)
        {
            return CalculateShinesparkDamage(shinesparkFrames) * times;
        }

        /// <summary>
        /// Calculates and returns the damage Samus would take for executing a shinespark of the provided duration n times, in the best case scenario
        /// (though vanilla has only one case).
        /// </summary>
        /// <param name="shinesparkFrames">The duration (in frames) of the shinespark whose damage to calculate.</param>
        /// <param name="removedItems">A set of item names that cannot logically be found in game</param>
        /// <returns>The calculated damage</returns>
        public virtual int CalculateBestCaseShinesparkDamage(int shinesparkFrames, IReadOnlySet<string> removedItems)
        {
            return CalculateShinesparkDamage(shinesparkFrames);
        }

        /// <summary>
        /// Calculates and returns the damage Samus would take for executing a shinespark of the provided duration n times, in the worst case scenario
        /// (though vanilla has only one case).
        /// </summary>
        /// <param name="shinesparkFrames">The duration (in frames) of the shinespark whose damage to calculate.</param>
        /// <param name="startingInventory">The inventory of items that Samus is logically guaranteed to have</param>
        /// <returns>The calculated damage</returns>
        public virtual int CalculateWorstCaseShinesparkDamage(int shinesparkFrames, ReadOnlyItemInventory startingInventory)
        {
            return CalculateShinesparkDamage(shinesparkFrames);
        }

        private int CalculateShinesparkDamage(int shinesparkFrames)
        {
            return shinesparkFrames;
        }

        /// <summary>
        /// <para>Returns the minimum energy Samus must have before initiating a shinespark n times, for that shinespark to complete to the end all n times.
        /// By default, that is the energy cost of the shinespark * times + the energy at which a shinespark is interrupted.</para>
        /// <para>This does return 0 if shinesparkFrames is 0, interpreting that as meaning there is no shinespark.</para>
        /// </summary>
        /// <param name="inGameState">An in-game state describing the current player situation, notably knowing what items the player has.</param>
        /// <param name="shinesparkFrames">The duration (in frames) of the shinespark. 0 means no shinespark is being executed.</param>
        /// <param name="times">The number of times the shinespark will be performed.</param>
        /// <returns></returns>
        public virtual int CalculateEnergyNeededForShinespark(ReadOnlyInGameState inGameState, int shinesparkFrames, int times = 1)
        {
            return CalculateEnergyNeededForShinespark(shinesparkFrames, times);
        }

        /// <summary>
        /// <para>Returns the minimum energy Samus must have before initiating a shinespark n times, for that shinespark to complete to the end all n times,
        /// in the best case scenario.
        /// By default, that is the energy cost of the shinespark * times + the energy at which a shinespark is interrupted.</para>
        /// <para>This does return 0 if shinesparkFrames is 0, interpreting that as meaning there is no shinespark.</para>
        /// </summary>
        /// <param name="shinesparkFrames">The duration (in frames) of the shinespark whose damage to calculate.</param>
        /// <param name="removedItems">A set of item names that cannot logically be found in game</param>
        /// <param name="times">The number of times the shinespark will be performed.</param>
        /// <returns>The calculated damage</returns>
        public virtual int CalculateBestCaseEnergyNeededForShinespark(int shinesparkFrames, IReadOnlySet<string> removedItems, int times = 1)
        {
            return CalculateEnergyNeededForShinespark(shinesparkFrames, times);
        }

        /// <summary>
        /// <para>Returns the minimum energy Samus must have before initiating a shinespark n times, for that shinespark to complete to the end all n times,
        /// in the worst case scenario.
        /// By default, that is the energy cost of the shinespark * times + the energy at which a shinespark is interrupted.</para>
        /// <para>This does return 0 if shinesparkFrames is 0, interpreting that as meaning there is no shinespark.</para>
        /// </summary>
        /// <param name="shinesparkFrames">The duration (in frames) of the shinespark whose damage to calculate.</param>
        /// <param name="startingInventory">The inventory of items that Samus is logically guaranteed to have</param>
        /// <param name="times">The number of times the shinespark will be performed.</param>
        /// <returns>The calculated damage</returns>
        public virtual int CalculateWorstCaseEnergyNeededForShinespark(int shinesparkFrames, ReadOnlyItemInventory startingInventory, int times = 1)
        {
            return CalculateEnergyNeededForShinespark(shinesparkFrames);
        }

        private int CalculateEnergyNeededForShinespark(int shinesparkFrames, int times = 1)
        {
            return shinesparkFrames == 0 ? 0 : CalculateShinesparkDamage(shinesparkFrames)*times + ShinesparkEnergyLimit;
        }

        /// <summary>
        /// Calculates and returns how much damage the provided enemy attack would do to Samus, given the provided in-game state.
        /// </summary>
        /// <param name="inGameState">An in-game state describing the current player situation, notably knowing what items the player has.</param>
        /// <param name="attack">The enemy attack whose damage to calculate</param>
        /// <returns>The calculated damage</returns>
        public virtual int CalculateEnemyDamage(ReadOnlyInGameState inGameState, EnemyAttack attack)
        {
            return CalculateEnemyDamage(attack, inGameState.Inventory.HasVariaSuit(), inGameState.Inventory.HasGravitySuit());
        }

        /// <summary>
        /// Calculates and returns how much damage the provided enemy attack would do to Samus, in the best case scenario (presumably with both suits).
        /// </summary>
        /// <param name="attack">The enemy attack whose damage to calculate</param>
        /// <param name="removedItems">A set of item names that cannot logically be found in game</param>
        /// <returns>The calculated damage</returns>
        public virtual int CalculateBestCaseEnemyDamage(EnemyAttack attack, IReadOnlySet<string> removedItems)
        {
            return CalculateEnemyDamage(attack, !removedItems.ContainsVariaSuit(), !removedItems.ContainsGravitySuit());
        }

        /// <summary>
        /// Calculates and returns how much damage the provided enemy attack would do to Samus, in the worst case scenario (presumably suitless).
        /// </summary>
        /// <param name="attack">The enemy attack whose damage to calculate</param>
        /// <param name="startingInventory">The inventory of items that Samus is logically guaranteed to have</param>
        /// <returns>The calculated damage</returns>
        public virtual int CalculateWorstCaseEnemyDamage(EnemyAttack attack, ReadOnlyItemInventory startingInventory)
        {
            return CalculateEnemyDamage(attack, startingInventory.HasVariaSuit(), startingInventory.HasGravitySuit());
        }

        private int CalculateEnemyDamage(EnemyAttack attack, bool hasVaria, bool hasGravity)
        {
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
        /// for a reduction in the damage returned by <see cref="CalculateEnemyDamage(ReadOnlyInGameState, EnemyAttack)"/>.<para>
        /// <para>Does not return items that would reduce the damage, but are made irrelevant by another item's reduction</para>
        /// </summary>
        /// <param name="model">A model that can be used to obtain data about the current game configuration.</param>
        /// <param name="inGameState">An in-game state describing the current player situation, notably knowing what items the player has.</param>
        /// <returns></returns>
        public virtual IEnumerable<Item> GetEnemyDamageReducingItems(SuperMetroidModel model, ReadOnlyInGameState inGameState, EnemyAttack enemyAttack)
        {
            // What we return depends not only on the suits available, but also on the attack
            if (enemyAttack.AffectedByGravity && enemyAttack.AffectedByVaria)
            {
                return GetDamageReducingItemsWhenGravitySupersedesVaria(model, inGameState);
            }

            if (enemyAttack.AffectedByGravity && inGameState.Inventory.HasGravitySuit())
            {
                return new Item[] { model.Items[SuperMetroidModel.GRAVITY_SUIT_NAME] };
            }

            if (enemyAttack.AffectedByVaria && inGameState.Inventory.HasVariaSuit())
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
        /// Creates and returns the equivalent of the provided IRunway, but used in the opposite direction.
        /// </summary>
        /// <param name="runway">The runway to reverse</param>
        /// <returns></returns>
        public virtual IRunway ReverseRunway(IRunway runway)
        {
            return new BasicRunway (
                length: runway.Length,
                endingUpTiles: runway.StartingDownTiles,
                gentleDownTiles: runway.GentleUpTiles,
                gentleUpTiles: runway.GentleDownTiles,
                openEnds: runway.OpenEnds,
                startingDownTiles: runway.EndingUpTiles,
                steepDownTiles: runway.SteepUpTiles,
                steepUpTiles: runway.SteepDownTiles
            );
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
