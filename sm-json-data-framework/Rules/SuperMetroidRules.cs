using sm_json_data_framework.EnergyManagement;
using sm_json_data_framework.InGameStates;
using sm_json_data_framework.Models;
using sm_json_data_framework.Models.Enemies;
using sm_json_data_framework.Models.Items;
using sm_json_data_framework.Models.Requirements;
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

        /// <summary>
        /// Multiplier to apply to shinespark frames to obtain shinespark damage.
        /// </summary>
        public virtual decimal ShinesparkFramesToDamageMultiplier => 1;

        /// <summary>
        /// The number of tiles that are lost when combining two runways via a room transition
        /// </summary>
        public virtual decimal RoomTransitionTilesLost => 1.25M;

        // Apparently the gain varies between 8.5 and 9 pixels, which is just over half a tile.
        /// <summary>
        /// The number of tiles gained from having one open end in a runway.
        /// </summary>
        public virtual decimal TilesGainedPerOpenEnd => 0.5M;

        // According to zqxk, charging on gentle up tiles multiplies required distance by 27/32. So each tile is worth 32/27 of a tile
        /// <summary>
        /// A multiplier that can be applied to a number of gentle up tiles to obtain the equivalent run length in flat tiles.
        /// </summary>
        public virtual decimal GentleUpTileMultiplier => 32M / 27M;

        // According to zqxk, charging on steep up tiles multiplies required distance by 3/4. So each tile is worth 4/3 of a tile
        /// <summary>
        /// A multiplier that can be applied to a number of steep up tiles to obtain the equivalent run length in flat tiles.
        /// </summary>
        public virtual decimal SteepUpTileMultiplier => 4M / 3M;

        // STITCHME Don't know about downward slopes yet
        /// <summary>
        /// A multiplier that can be applied to a number of gentle down tiles to obtain the equivalent run length in flat tiles.
        /// </summary>
        public virtual decimal GentleDownTileMultiplier => 1M;

        // STITCHME Don't know about downward slopes yet
        /// <summary>
        /// A multiplier that can be applied to a number of steep down tiles to obtain the equivalent run length in flat tiles.
        /// </summary>
        public virtual decimal SteepDownTileMultiplier => 1;

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
                _ => throw new NotImplementedException($"Unrecognized enemy drop {enemyDrop}")
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
        /// Returns the base damage for one hit of the provided environment damage source.
        /// </summary>
        /// <param name="environmentDamageEnum">The type of environment damage</param>
        /// <returns></returns>
        public virtual int GetPunctualEnvironmentBaseDamage(PunctualEnvironmentDamageEnum environmentDamageEnum)
        {
            return environmentDamageEnum switch
            {
                PunctualEnvironmentDamageEnum.ThornHit => 16,
                PunctualEnvironmentDamageEnum.SpikeHit => 60,
                PunctualEnvironmentDamageEnum.HibashiHit => 30,
                _ => throw new NotImplementedException($"PunctualEnvironmentDamageEnum enum {environmentDamageEnum} not supported here")
            };
        }

        /// <summary>
        /// Returns a multiplier to apply on top of the base damage of a punctual hit of environment damage based on the provided in-game state.
        /// </summary>
        /// <param name="inGameState">The in-game state on which to base the damage reduction</param>
        /// <param name="environmentDamageEnum">The environment damage to get damage reduction for</param>
        /// <returns></returns>
        public virtual decimal GetPunctualEnvironmentDamageReductionMultiplier(ReadOnlyInGameState inGameState, PunctualEnvironmentDamageEnum environmentDamageEnum)
        {
            return GetPunctualEnvironmentDamageReductionMultiplier(environmentDamageEnum, inGameState.Inventory.HasVariaSuit(), inGameState.Inventory.HasGravitySuit());
        }

        private decimal GetPunctualEnvironmentDamageReductionMultiplier(PunctualEnvironmentDamageEnum environmentDamageEnum, bool hasVaria, bool hasGravity)
        {
            if (hasGravity)
            {
                return 0.25M;
            }
            else if (hasVaria)
            {
                return 0.5M;
            }
            else
            {
                return 1;
            }
        }

        /// <summary>
        /// Calculates and returns the environment damage Samus would take for the provided in-game state and environment damage type.
        /// </summary>
        /// <param name="inGameState">An in-game state describing the current player situation, notably knowing what items the player has.</param>
        /// <param name="environmentDamageEnum">The environment damage source</param>
        /// <returns>The calculated damage</returns>
        public virtual int CalculatePunctualEnvironmentDamage(ReadOnlyInGameState inGameState, PunctualEnvironmentDamageEnum environmentDamageEnum)
        {
            return CalculatePunctualEnvironmentDamage(inGameState.Inventory.HasVariaSuit(), inGameState.Inventory.HasGravitySuit(), environmentDamageEnum);
        }

        private int CalculatePunctualEnvironmentDamage(bool hasVaria, bool hasGravity, PunctualEnvironmentDamageEnum environmentDamageEnum)
        {
            int baseDamage = GetPunctualEnvironmentBaseDamage(environmentDamageEnum);
            decimal multiplier = GetPunctualEnvironmentDamageReductionMultiplier(environmentDamageEnum, hasVaria, hasGravity);
            return (int)(baseDamage * multiplier);
        }

        /// <summary>
        /// Calculates and returns the environmental damage Samus would take for the provided environmental damage source, in the best case scenario
        /// (presumably, both suits).
        /// </summary>
        /// <param name="environmentDamageEnum">The environment damage source</param>
        /// <param name="removedItems">A set of item names that cannot logically be found in game</param>
        /// <returns>The calculated damage</returns>
        public virtual int CalculateBestCasePunctualEnvironmentDamage(PunctualEnvironmentDamageEnum environmentDamageEnum, IReadOnlySet<string> removedItems)
        {
            return CalculatePunctualEnvironmentDamage(!removedItems.ContainsVariaSuit(), !removedItems.ContainsGravitySuit(), environmentDamageEnum);
        }

        /// <summary>
        /// Calculates and returns the environmental damage Samus would take for the provided environmental damage source, in the worst case scenario
        /// (presumably, suitless).
        /// </summary>
        /// <param name="environmentDamageEnum">The environment damage source</param>
        /// <param name="startingInventory">The inventory of items that Samus is logically guaranteed to have</param>
        /// <returns>The calculated damage</returns>
        public virtual int CalculateWorstCasePunctualEnvironmentDamage(PunctualEnvironmentDamageEnum environmentDamageEnum, ReadOnlyItemInventory startingInventory)
        {
            return CalculatePunctualEnvironmentDamage(startingInventory.HasVariaSuit(), startingInventory.HasGravitySuit(), environmentDamageEnum);
        }

        /// <summary>
        /// <para>Returns the enumeration of items found in the provided inGameState which would be responsible
        /// for a reduction in the damage returned by <see cref="CalculateEnvironmentalDamage(ReadOnlyUnfinalizedInGameState, int)"/>.<para>
        /// <para>Does not return items that would reduce the damage, but are made irrelevant by another item's reduction</para>
        /// </summary>
        /// <param name="model">A model that can be used to obtain data about the current game configuration.</param>
        /// <param name="inGameState">An in-game state describing the current player situation, notably knowing what items the player has.</param>
        /// <returns></returns>
        public virtual IEnumerable<Item> GetPunctualEnvironmentDamageReducingItems(SuperMetroidModel model, ReadOnlyInGameState inGameState, 
            PunctualEnvironmentDamageEnum environmentDamageEnum)
        {
            decimal gravityOnlyMultiplier = GetPunctualEnvironmentDamageReductionMultiplier(environmentDamageEnum, hasVaria: false, hasGravity: true);
            decimal variaOnlyMultiplier = GetPunctualEnvironmentDamageReductionMultiplier(environmentDamageEnum, hasVaria: true, hasGravity: false);
            decimal bothMultiplier = GetPunctualEnvironmentDamageReductionMultiplier(environmentDamageEnum, hasVaria: true, hasGravity: true);

            return GetDamageReducingItems(model, inGameState, gravityOnlyMultiplier, variaOnlyMultiplier, bothMultiplier);
        }

        private IEnumerable<Item> GetDamageReducingItems(SuperMetroidModel model, ReadOnlyInGameState inGameState, 
            decimal gravityOnlyMultiplier, decimal variaOnlyMultiplier, decimal bothMultiplier)
        {
            bool gravityReduces = gravityOnlyMultiplier < 1;
            bool variaReduces = variaOnlyMultiplier < 1;
            bool variaMattersWithGravity = bothMultiplier < gravityOnlyMultiplier;

            HashSet<Item> items = new();
            if (inGameState.Inventory.HasGravitySuit() && gravityReduces)
            {
                items.Add(model.Items[SuperMetroidModel.GRAVITY_SUIT_NAME]);
            }

            if (inGameState.Inventory.HasVariaSuit() && variaReduces && (variaMattersWithGravity || !inGameState.Inventory.HasGravitySuit()))
            {
                items.Add(model.Items[SuperMetroidModel.VARIA_SUIT_NAME]);
            }

            return items;
        }

        /// <summary>
        /// Returns the minimum energy that Samus can go to during the provided DoT effect.
        /// Going below that energy amount should be seen as a failure even if it doesn't kill Samus.
        /// </summary>
        /// <param name="dotEnum">The DoT effect to get the minimum energy threshold for</param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public virtual int GetDamageOverTimeMinimumEnergyThreshold(DamageOverTimeEnum dotEnum)
        {
            return dotEnum switch
            {
                DamageOverTimeEnum.Acid => 1,
                DamageOverTimeEnum.GrappleElectricity => 1,
                DamageOverTimeEnum.Heat => 1,
                DamageOverTimeEnum.Lava => 1,
                DamageOverTimeEnum.LavaPhysics => 1,
                DamageOverTimeEnum.Shinespark => 29,
                DamageOverTimeEnum.SamusEater => 1,
                _ => throw new NotImplementedException($"DamageOverTime enum {dotEnum} not supported here")
            };
        }

        /// <summary>
        /// Returns the base damage per frame for the provided DoT effect.
        /// </summary>
        /// <param name="dotEnum">The DoT effect to get the base damage for</param>
        /// <returns></returns>
        public virtual decimal GetBaseDamagePerFrame(DamageOverTimeEnum dotEnum)
        {
            return dotEnum switch
            {
                DamageOverTimeEnum.Acid => 1.5M,
                DamageOverTimeEnum.GrappleElectricity => 1,
                DamageOverTimeEnum.Heat => 0.25M,
                DamageOverTimeEnum.Lava => 0.5M,
                DamageOverTimeEnum.LavaPhysics => 0.5M,
                DamageOverTimeEnum.Shinespark => 1,
                DamageOverTimeEnum.SamusEater => 0.1M,
                _ => throw new NotImplementedException($"DamageOverTime enum {dotEnum} not supported here")
            };
        }

        /// <summary>
        /// Returns a multiplier to apply on top of the base damage of a DoT effect based on the provided in-game state.
        /// </summary>
        /// <param name="inGameState">The in-game state on which to base the damage reduction</param>
        /// <param name="dotEnum">The DoT effect to get damage reduction for</param>
        /// <returns></returns>
        public virtual decimal GetDamageOverTimeReductionMultiplier(ReadOnlyInGameState inGameState, DamageOverTimeEnum dotEnum)
        {
            return GetDamageOverTimeReductionMultiplier(dotEnum, inGameState.Inventory.HasVariaSuit(), inGameState.Inventory.HasGravitySuit());
        }

        private decimal GetDamageOverTimeReductionMultiplier(DamageOverTimeEnum dotEnum, bool hasVaria, bool hasGravity)
        {
            if (hasGravity)
            {
                // Shinespark and lavaPhysics damage is unaffected by having Gravity
                switch (dotEnum)
                {
                    case DamageOverTimeEnum.Acid:
                    case DamageOverTimeEnum.GrappleElectricity:
                    case DamageOverTimeEnum.SamusEater:
                        return 0.25M;
                    case DamageOverTimeEnum.Lava:
                    case DamageOverTimeEnum.Heat:
                        return 0;
                }
            }

            if (hasVaria)
            {
                // Shinespark damage is unaffected by having Varia
                switch (dotEnum)
                {
                    case DamageOverTimeEnum.Acid:
                    case DamageOverTimeEnum.GrappleElectricity:
                    case DamageOverTimeEnum.SamusEater:
                    case DamageOverTimeEnum.Lava:
                    case DamageOverTimeEnum.LavaPhysics:
                        return 0.5M;
                    case DamageOverTimeEnum.Heat:
                        return 0;
                }
            }

            switch (dotEnum)
            {
                case DamageOverTimeEnum.Acid:
                case DamageOverTimeEnum.GrappleElectricity:
                case DamageOverTimeEnum.SamusEater:
                case DamageOverTimeEnum.Lava:
                case DamageOverTimeEnum.LavaPhysics:
                case DamageOverTimeEnum.Heat:
                case DamageOverTimeEnum.Shinespark:
                    return 1;
                default:
                    throw new NotImplementedException($"DamageOverTime enum {dotEnum} not supported here");
            }
        }

        /// <summary>
        /// Returns the damage done by the provided DoT over the provided number of frames, given the inGameState but assuming an ability to take the damage indefinitely.
        /// </summary>
        /// <param name="inGameState">An in-game state describing the current player situation, notably knowing what items the player has.</param>
        /// <param name="frames">The number of frames the DoT effect will be active for</param>
        /// <param name="dotEnum">The DoT effect to get damage for</param>
        /// <returns></returns>
        public virtual int CalculateDamageOverTime(ReadOnlyInGameState inGameState, int frames, DamageOverTimeEnum dotEnum)
        {
            return CalculateDamageOverTime(frames, inGameState.Inventory.HasVariaSuit(), inGameState.Inventory.HasGravitySuit(), dotEnum);
        }

        /// <summary>
        /// Calculates and returns the damage Samus would take for being subjected to a given DoT effect for the provided duration, in the best case scenario
        /// (presumably both suits).
        /// </summary>
        /// <param name="frames">The duration (in frames) of the DoT exposure whose damage to calculate</param>
        /// <param name="dotEnum">The DoT effect</param>
        /// <param name="removedItems">A set of item names that cannot logically be found in game</param>
        /// <returns>The calculated damage</returns>
        public virtual int CalculateBestCaseDamageOverTime(int frames, DamageOverTimeEnum dotEnum, IReadOnlySet<string> removedItems)
        {
            return CalculateDamageOverTime(frames, !removedItems.ContainsVariaSuit(), !removedItems.ContainsGravitySuit(), dotEnum);
        }

        /// <summary>
        /// Calculates and returns the damage Samus would take for being subjected to a given DoT effect for the provided duration, in the worst case scenario
        /// (presumably suitless).
        /// </summary>
        /// <param name="frames">The duration (in frames) of the DoT exposure whose damage to calculate</param>
        /// <param name="dotEnum">The DoT effect</param>
        /// <param name="startingInventory">The inventory of items that Samus is logically guaranteed to have</param>
        /// <returns>The calculated damage</returns>
        public virtual int CalculateWorstCaseDamageOverTime(int frames, DamageOverTimeEnum dotEnum, ReadOnlyItemInventory startingInventory)
        {
            return CalculateDamageOverTime(frames, startingInventory.HasVariaSuit(), startingInventory.HasGravitySuit(), dotEnum);
        }

        private int CalculateDamageOverTime(int frames, bool hasVaria, bool hasGravity, DamageOverTimeEnum dotEnum)
        {
            return (int)(frames * GetDamagePerFrame(hasVaria, hasGravity, dotEnum));
        }

        /// <summary>
        /// Returns the damage Samus will takes per frame of the provided DoT effect, given the provided in-game state.
        /// </summary>
        /// <param name="inGameState">An in-game state describing the current player situation, notably knowing what items the player has.</param>
        /// <param name="dotEnum">The DoT effect to get damage for</param>
        /// <returns></returns>
        public virtual decimal GetDamagePerFrame(ReadOnlyInGameState inGameState, DamageOverTimeEnum dotEnum)
        {
            return GetDamagePerFrame(inGameState.Inventory.HasVariaSuit(), inGameState.Inventory.HasGravitySuit(), dotEnum);
        }

        private decimal GetDamagePerFrame(bool hasVaria, bool hasGravity, DamageOverTimeEnum dotEnum)
        {
            return GetBaseDamagePerFrame(dotEnum) * GetDamageOverTimeReductionMultiplier(dotEnum, hasVaria, hasGravity);
        }

        /// <summary>
        /// <para>Returns the enumeration of items found in the provided inGameState which would be responsible
        /// for a reduction in the damage returned by <see cref="CalculateDamageOverTime(ReadOnlyInGameState, int, DamageOverTimeEnum)"/>.<para>
        /// <para>Does not return items that would reduce the damage, but are made irrelevant by another item's reduction</para>
        /// </summary>
        /// <param name="model">A model that can be used to obtain data about the current game configuration.</param>
        /// <param name="inGameState">An in-game state describing the current player situation, notably knowing what items the player has.</param>
        /// <returns></returns>
        public virtual IEnumerable<Item> GetDamageOverTimeReducingItems(SuperMetroidModel model, ReadOnlyInGameState inGameState, DamageOverTimeEnum dotEnum)
        {
            decimal gravityOnlyMultiplier = GetDamageOverTimeReductionMultiplier(dotEnum, hasVaria: false, hasGravity: true);
            decimal variaOnlyMultiplier = GetDamageOverTimeReductionMultiplier(dotEnum, hasVaria: true, hasGravity: false);
            decimal bothMultiplier = GetDamageOverTimeReductionMultiplier(dotEnum, hasVaria: true, hasGravity: true);

            return GetDamageReducingItems(model, inGameState, gravityOnlyMultiplier, variaOnlyMultiplier, bothMultiplier);
        }

        /// <summary>
        /// <para>
        /// Calculates and returns the damage Samus would take for attempting to execute a shinespark of the provided duration n times, given the provided regular energy value.
        /// If Samus has not enough energy to complete the shinesparks, this will return how much she will lose before shinesparks stop moving her.
        /// </para>
        /// <para>
        /// This method makes no comment on whether Samus has enough energy to accomplish anything with the shinespark.
        /// This can instead be checked by calling <see cref="CalculateMinimumEnergyNeededForShinespark(int, int, int)"/>.
        /// </para>
        /// </summary>
        /// <param name="currentRegularEnergy">The amount of regular energy available.</param>
        /// <param name="shinesparkFrames">The duration (in frames) of the shinespark whose damage to calculate (including any excess frames).</param>
        /// <param name="times">The number of times the shinespark will be performed.</param>
        /// <returns>The calculated damage</returns>
        public int CalculateInterruptibleShinesparkDamage(ReadOnlyInGameState inGameState, int shinesparkFrames, int times = 1)
        {
            int currentRegularEnergy = inGameState.Resources.GetAmount(RechargeableResourceEnum.RegularEnergy);
            int maxDamage = CalculateDamageOverTime(inGameState, shinesparkFrames, DamageOverTimeEnum.Shinespark) * times;
            int energyAvailableForSpark = currentRegularEnergy - GetDamageOverTimeMinimumEnergyThreshold(DamageOverTimeEnum.Shinespark);
            return Math.Min(maxDamage, energyAvailableForSpark);
        }

        /// <summary>
        /// <para>Returns the minimum energy that must be consumed to execute a shinespark n times.
        /// This is interpreted as meaning all shinesparks but the last one must execute fully, since otherwise there would be no energy to start the next one.
        /// By default, that is the energy cost of the full shinespark * (times -1) + the energy cost of just the non-excess frames once.</para>
        /// <para>This does return 0 if shinesparkFrames is 0, interpreting that as meaning there is no shinespark.</para>
        /// </summary>
        public int CalculateMinimumShinesparkDamage(int shinesparkFrames, int excessShinesparkFrames, int times = 1)
        {
            if (shinesparkFrames <= 0 || times <= 0)
            {
                return 0;
            }
            return ((int)(shinesparkFrames * ShinesparkFramesToDamageMultiplier)) * (times - 1) + ((int)((shinesparkFrames - excessShinesparkFrames) * ShinesparkFramesToDamageMultiplier));
        }

        /// <summary>
        /// <para>Returns the minimum energy Samus must have before initiating a shinespark n times, for that shinespark to complete at least the non-excess frames n consecutive times.
        /// By default, that is the energy spent (<see cref="CalculateMinimumShinesparkDamage(int, int, int)"/>) + the energy at which a shinespark is interrupted.</para>
        /// <para>This does return 0 if shinesparkFrames is 0, interpreting that as meaning there is no shinespark.</para>
        /// </summary>
        /// <param name="shinesparkFrames">The duration (in frames) of the shinespark whose damage to calculate (including any excess frames).</param>
        /// <param name="excessShinesparkFrames">The numberof frames in the shinespark that happen after the primary objective has been met,
        /// and so are not mandatory to be spent for the shinespark to succeed. They will be spent if available though.</param>
        /// <param name="times">The number of times the shinespark will be performed.</param>
        /// <returns>The calculated damage</returns>
        public virtual int CalculateMinimumEnergyNeededForShinespark(int shinesparkFrames, int excessShinesparkFrames, int times = 1)
        {
            if (shinesparkFrames <= 0 || times <= 0)
            {
                return 0;
            }
            return CalculateMinimumShinesparkDamage(shinesparkFrames, excessShinesparkFrames, times) + GetDamageOverTimeMinimumEnergyThreshold(DamageOverTimeEnum.Shinespark);
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
                _ => throw new NotImplementedException($"Unrecognized rechargeable resource {resource}")
            };
        }
    }
}
