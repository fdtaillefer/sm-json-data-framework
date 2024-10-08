﻿using sm_json_data_framework.InGameStates;
using sm_json_data_framework.Models;
using sm_json_data_framework.Models.GameFlags;
using sm_json_data_framework.Models.Helpers;
using sm_json_data_framework.Models.Items;
using sm_json_data_framework.Models.Requirements;
using sm_json_data_framework.Models.Rooms;
using sm_json_data_framework.Models.Techs;
using sm_json_data_framework.Options.ResourceValues;
using sm_json_data_framework.Rules.InitialState;
using sm_json_data_framework.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace sm_json_data_framework.Options
{
    /// <summary>
    /// Options that describe what the player is expected to be able or unable to do.
    /// </summary>
    public class LogicalOptions : ReadOnlyLogicalOptions
    {
        public static readonly int DefaultNumberOfTries = 1;
        public static readonly decimal DefaultTilesSavedWithStutter = 0;
        // Obscenely short distance so that all shine charges are possible. 12 is an in-room Red Tower shine charge.
        public static readonly decimal DefaultTilesToShineCharge = 12;
        public static readonly decimal DefaultFrameLeniencyMultiplier = 1;
        public static readonly int DefaultPauseSpamLeewayFrames = 10;
        public static readonly int DefaultReserveRefillLeeway = 20;
        public static readonly int DefaultReserveUsageTimingLeeway = 20;
        public static readonly bool DefaultCanUsePartialReserves = true;
        public static readonly int DefaultIframesToAvoidDoubleHit = 60;

        public static readonly ReadOnlySpawnerFarmingOptions DefaultSpawnerFarmingOptions = new SpawnerFarmingOptions().AsReadOnly();

        private static IInGameResourceEvaluator DefaultInGameResourceEvaluator { get; } = new ResourceEvaluatorByFixedValues(
                new Dictionary<ConsumableResourceEnum, int> {
                    {ConsumableResourceEnum.Energy, 1},
                    // Missile drops are super plentiful, AND each drop gives twice as much as Supers.
                    {ConsumableResourceEnum.Missile, 3},
                    {ConsumableResourceEnum.Super, 30},
                    {ConsumableResourceEnum.PowerBomb, 60}
                }
            );

        public static InGameStateComparer DefaultInGameStateComparer = new InGameStateComparer(DefaultInGameResourceEvaluator);

        /// <summary>
        /// A static instance of LogicalOptions will all default values. 
        /// It allows all techs and strats, with super short charge and no leniency.
        /// </summary>
        public static ReadOnlyLogicalOptions DefaultLogicalOptions = new LogicalOptions().AsReadOnly();

        public LogicalOptions()
        {
            // Default resource comparer
            InGameResourceEvaluator = DefaultInGameResourceEvaluator;
        }

        public LogicalOptions(LogicalOptions other)
        {
            InGameResourceEvaluator = other.InGameResourceEvaluator;
            TechsEnabledByDefault = other.TechsEnabledByDefault;
            InternalDisabledTechs = new HashSet<string>(other.InternalDisabledTechs);
            InternalEnabledTechs = new HashSet<string>(other.InternalEnabledTechs);
            InternalDisabledStrats = new HashSet<string>(other.InternalDisabledStrats);
            InternalRemovedItems = new HashSet<string>(other.InternalRemovedItems);
            InternalRemovedGameFlags = new HashSet<string>(other.InternalRemovedGameFlags);
            TilesToShineCharge = other.TilesToShineCharge;
            TilesSavedWithStutter = other.TilesSavedWithStutter;
            PauseSpamLeewayFrames = other.PauseSpamLeewayFrames;
            ReserveRefillLeewayEnergy = other.ReserveRefillLeewayEnergy;
            ReserveUsageTimingLeewayFrames = other.ReserveUsageTimingLeewayFrames;
            CanUsePartialReserves = other.CanUsePartialReserves;
            IframesToAvoidDoubleHit = other.IframesToAvoidDoubleHit;
            InGameResourceEvaluator = other.InGameResourceEvaluator; // This also assigns InGameStateComparer
            TriesByTech = new Dictionary<string, int>(other.TriesByTech);
            TriesByHelper = new Dictionary<string, int>(other.TriesByHelper);
            TriesByStrat = new Dictionary<string, int>(other.TriesByStrat);
            ShineChargesWithStutter = other.ShineChargesWithStutter;
            HeatLeniencyMultiplier = other.HeatLeniencyMultiplier;
            LavaLeniencyMultiplier = other.LavaLeniencyMultiplier;
            AcidLeniencyMultiplier = other.AcidLeniencyMultiplier;
            InternalSpawnerFarmingOptions = other.InternalSpawnerFarmingOptions.Clone();
            InternalStartConditions = other.InternalStartConditions?.Clone();
            InternalUnfinalizedStartConditions = other.InternalUnfinalizedStartConditions?.Clone();
            InternalAvailableResourceInventory = other.InternalAvailableResourceInventory?.Clone();
            InternalUnfinalizedAvailableResourceInventory = other.InternalUnfinalizedAvailableResourceInventory?.Clone();
        }

        public LogicalOptions Clone()
        {
            return new LogicalOptions(this);
        }

        public ReadOnlyLogicalOptions AsReadOnly()
        {
            return this;
        }

        public bool TechsEnabledByDefault { get; set; } = true;

        /// <summary>
        /// A set of tech names that are disabled. Irrelevant if <see cref="TechsEnabledByDefault"/> is false.
        /// </summary>
        private ISet<string> InternalDisabledTechs { get; set; } = new HashSet<string>();
        public IReadOnlySet<string> DisabledTechs => InternalDisabledTechs.AsReadOnly();

        /// <summary>
        /// A set of tech names that are enabled. Irrelevant if <see cref="TechsEnabledByDefault"/> is true.
        /// </summary>
        private ISet<string> InternalEnabledTechs { get; set; } = new HashSet<string>();
        public IReadOnlySet<string> EnabledTechs => InternalEnabledTechs.AsReadOnly();

        /// <summary>
        /// A set of strat names that are disabled, regardless of their requirements. Only notable strats can be disabled.
        /// </summary>
        private ISet<string> InternalDisabledStrats { get; set; } = new HashSet<string>();
        public IReadOnlySet<string> DisabledStrats => InternalDisabledStrats.AsReadOnly();

        /// <summary>
        /// A set of item names that are logically removed, causing logic to expect it to never be obtained.
        /// </summary>
        private ISet<string> InternalRemovedItems { get; set; } = new HashSet<string>();
        public IReadOnlySet<string> RemovedItems => InternalRemovedItems.AsReadOnly();

        /// <summary>
        /// A set of game flag names that are logically removed, causing logic to expect it to never be enabled.
        /// </summary>
        private ISet<string> InternalRemovedGameFlags { get; set; } = new HashSet<string>();
        public IReadOnlySet<string> RemovedGameFlags => InternalRemovedGameFlags.AsReadOnly();

        public decimal TilesToShineCharge { get; set; } = DefaultTilesToShineCharge;

        public decimal TilesSavedWithStutter { get; set; } = DefaultTilesSavedWithStutter;

        private IInGameResourceEvaluator _inGameResourceEvaluator;
        public IInGameResourceEvaluator InGameResourceEvaluator
        {
            get
            {
                return _inGameResourceEvaluator;
            }
            set
            {
                _inGameResourceEvaluator = value;
                // Update the inner InGameStateComparer to use the new resource evaluator
                InGameStateComparer = new InGameStateComparer(_inGameResourceEvaluator);
            }
        }

        /// <summary>
        /// An internal dictionary that contains all techs (identified by their name) for which a number of tries should be taken into account by the logic.
        /// This multiplies resources costs.
        /// </summary>
        private IDictionary<string, int> TriesByTech { get; set; } = new Dictionary<string, int>();

        /// <summary>
        /// <para>Register in this logical options that a tech with the provided name is logically expected
        /// to take the provided number of tries before succeeding, impacting its resource cost when applicable.</para>
        /// <para>A higher value makes the tech logically require more resources, making it more lenient.</para>
        /// 
        /// </summary>
        /// <param name="techName">The name of the tech</param>
        /// <param name="numberOfTries">The number of times the tech must be tried before an expected success</param>
        /// <returns>This, for chaining</returns>
        public LogicalOptions RegisterTechTries(string techName, int numberOfTries)
        {
            TriesByTech.Add(techName, numberOfTries);
            return this;
        }

        public int NumberOfTries(Tech tech)
        {
            if (TriesByTech.TryGetValue(tech.Name, out int tries))
            {
                return tries;
            }
            else
            {
                return DefaultNumberOfTries;
            }
        }

        /// <summary>
        /// An internal dictionary that contains all helpers (identified by their name) for which a number of tries should be taken into account by the logic.
        /// This multiplies resources costs.
        /// </summary>
        private IDictionary<string, int> TriesByHelper { get; set; } = new Dictionary<string, int>();

        /// <summary>
        /// <para>Register in this logical options that a helper with the provided name is logically expected
        /// to take the provided number of tries before succeeding, impacting its resource cost when applicable.</para>
        /// <para>A higher value makes the helper logically require more resources, making it more lenient.</para>
        /// 
        /// </summary>
        /// <param name="helperName">The name of the helper</param>
        /// <param name="numberOfTries">The number of times the helper must be tried before an expected success</param>
        /// <returns>This, for chaining</returns>
        public LogicalOptions RegisterHelperTries(string helperName, int numberOfTries)
        {
            TriesByHelper.Add(helperName, numberOfTries);
            return this;
        }

        public int NumberOfTries(Helper helper)
        {
            if (TriesByHelper.TryGetValue(helper.Name, out int tries))
            {
                return tries;
            }
            else
            {
                return DefaultNumberOfTries;
            }
        }

        /// <summary>
        /// An internal dictionary that contains all strats (identified by their name) for which a number of tries should be taken into account by the logic.
        /// This multiplies resources costs.
        /// </summary>
        private IDictionary<string, int> TriesByStrat { get; set; } = new Dictionary<string, int>();

        /// <summary>
        /// <para>Register in this logical options that a notable strat with the provided name is logically expected
        /// to take the provided number of tries before succeeding, impacting its resource cost when applicable.</para>
        /// <para>A higher value makes the strat logically require more resources, making it more lenient.</para>
        /// </summary>
        /// <param name="stratName">The name of the strat</param>
        /// <param name="numberOfTries">The number of times the strat must be tried before an expected success</param>
        /// <returns>This, for chaining</returns>
        public LogicalOptions RegisterStratTries(string stratName, int numberOfTries)
        {
            TriesByStrat.Add(stratName, numberOfTries);
            return this;
        }

        public int NumberOfTries(Strat strat)
        {
            if (strat.Notable && TriesByStrat.TryGetValue(strat.Name, out int tries))
            {
                return tries;
            }
            else
            {
                return DefaultNumberOfTries;
            }
        }

        public int PauseSpamLeewayFrames { get; set; } = DefaultPauseSpamLeewayFrames;

        public int ReserveRefillLeewayEnergy { get; set; } = DefaultReserveRefillLeeway;

        public int ReserveUsageTimingLeewayFrames { get; set; } = DefaultReserveUsageTimingLeeway;

        public InGameStateComparer InGameStateComparer { get; private set; }

        public bool ShineChargesWithStutter { get; set; } = false;

        public decimal HeatLeniencyMultiplier { get; set; } = DefaultFrameLeniencyMultiplier;

        public decimal LavaLeniencyMultiplier { get; set; } = DefaultFrameLeniencyMultiplier;

        public decimal AcidLeniencyMultiplier { get; set; } = DefaultFrameLeniencyMultiplier;

        /// <summary>
        /// Registers the provided tech name as a disabled tech.
        /// </summary>
        /// <param name="techName">Name of the tech to disable</param>
        /// <returns>This, for chaining</returns>
        public LogicalOptions RegisterDisabledTech(string techName)
        {
            InternalDisabledTechs.Add(techName);
            return this;
        }

        /// <summary>
        /// Unregisters the provided tech name as a disabled tech.
        /// </summary>
        /// <param name="techName">Name of the tech to un-disable</param>
        /// <returns>This, for chaining</returns>
        public LogicalOptions UnregisterDisabledTech(string techName)
        {
            InternalDisabledTechs.Remove(techName);
            return this;
        }

        /// <summary>
        /// Registers the provided tech name as an enabled tech (used when techs <see cref="TechsEnabledByDefault"/> is false).
        /// </summary>
        /// <param name="techName">Name of the tech to enable</param>
        /// <returns>This, for chaining</returns>
        public LogicalOptions RegisterEnabledTech(string techName)
        {
            InternalEnabledTechs.Add(techName);
            return this;
        }

        /// <summary>
        /// Unregisters the provided tech name as an enabled tech (used when techs <see cref="TechsEnabledByDefault"/> is false).
        /// </summary>
        /// <param name="techName">Name of the tech to un-enable</param>
        public void UnregisterEnabledTech(string techName)
        {
            InternalEnabledTechs.Remove(techName);
        }

        public bool IsTechEnabled(Tech tech)
        {
            return IsTechEnabled(tech.Name);
        }

        public bool IsTechEnabled(string techName)
        {
            if (TechsEnabledByDefault)
            {
                return !InternalDisabledTechs.Contains(techName);
            }
            else
            {
                return InternalEnabledTechs.Contains(techName);
            }
        }

        public bool CanShinespark => IsTechEnabled(SuperMetroidModel.SHINESPARK_TECH_NAME);

        public bool CanUseReservesToShinespark => IsTechEnabled(SuperMetroidModel.USE_RESERVES_FOR_SHINESPARK_TECH_NAME);

        public bool CanUsePartialReserves { get; set; } = DefaultCanUsePartialReserves;

        public int IframesToAvoidDoubleHit { get; set; } = DefaultIframesToAvoidDoubleHit;

        /// <summary>
        /// Registers the provided strat name as a disabled strat.
        /// </summary>
        /// <param name="stratName">Name of the strat to disable</param>
        /// <returns>This, for chaining</returns>
        public LogicalOptions RegisterDisabledStrat(string stratName)
        {
            InternalDisabledStrats.Add(stratName);
            return this;
        }

        /// <summary>
        /// Unregisters the provided strat name as a disabled strat.
        /// </summary>
        /// <param name="stratName">Name of the strat to un-disable</param>
        public void UnregisterDisabledStrat(string stratName)
        {
            InternalDisabledStrats.Remove(stratName);
        }

        public bool IsStratEnabled(Strat strat)
        {
            // Non-notable strats are always enabled. Beyond that, strats are enabled by default unless disabled
            return (!strat.Notable || !InternalDisabledStrats.Contains(strat.Name));
        }

        /// <summary>
        /// Registers the provided item name as a removed item.
        /// </summary>
        /// <param name="itemName">Name of the item to register</param>
        /// <returns>This, for chaining</returns>
        public LogicalOptions RegisterRemovedItem(string itemName)
        {
            InternalRemovedItems.Add(itemName);
            return this;
        }

        /// <summary>
        /// Unregisters the provided item name as a removed item.
        /// </summary>
        /// <param name="itemName">Name of the item to unregister</param>
        /// <returns>This, for chaining</returns>
        public LogicalOptions UnregisterRemovedItem(string itemName)
        {
            InternalRemovedItems.Remove(itemName);
            return this;
        }

        public bool IsItemInGame(Item item)
        {
            // Expansion items are not in-game if no pickups of them are
            if(item is ExpansionItem expansionItem && AvailableResourceInventory != null)
            {
                return !AvailableResourceInventory.HasItem(expansionItem);
            }
            // Items of all types are not in-game if explicitly disabled
            return !InternalRemovedItems.Contains(item.Name);
        }

        public bool IsSpeedBoosterInGame()
        {
            return !InternalRemovedItems.Contains(SuperMetroidModel.SPEED_BOOSTER_NAME);
        }

        /// <summary>
        /// Registers the provided game flag name as a disabled flag.
        /// </summary>
        /// <param name="flagName">Name of the flag to disable</param>
        /// <returns>This, for chaining</returns>
        public LogicalOptions RegisterDisabledGameFlag(string flagName)
        {
            InternalRemovedGameFlags.Add(flagName);
            return this;
        }

        /// <summary>
        /// Unregisters the provided game flag name as a disabled flag.
        /// </summary>
        /// <param name="flagName">Name of the flag to un-disable</param>
        public void UnregisterDisabledGameFlag(string flagName)
        {
            InternalRemovedGameFlags.Remove(flagName);
        }

        public bool IsGameFlagEnabled(GameFlag gameFlag)
        {
            return !InternalRemovedGameFlags.Contains(gameFlag.Name);
        }

        public SpawnerFarmingOptions InternalSpawnerFarmingOptions { get; set; } = DefaultSpawnerFarmingOptions.Clone();
        public ReadOnlySpawnerFarmingOptions SpawnerFarmingOptions => InternalSpawnerFarmingOptions.AsReadOnly();

        /// <summary>
        /// <para>
        /// Start conditions that should be logically considered. If no start conditions are supplied, default start conditions will be used.
        /// </para>
        /// <para>
        /// Note that when supplying start conditions for a model before it has been finalized, you should use the
        /// <see cref="InternalUnfinalizedStartConditions"/> property instead.
        /// </para>
        /// </summary>
        public StartConditions InternalStartConditions { get; set; }
        public StartConditions StartConditions => InternalStartConditions;

        /// <summary>
        /// This property is used to communicate start conditions before a finalized model exists, with the intent to apply the start conditions during finalization.
        /// If the finalized model already exists, this property is entirely ignored and only <see cref="InternalStartConditions"/> matters.
        /// </summary>
        public UnfinalizedStartConditions InternalUnfinalizedStartConditions { get; set; }

        /// <summary>
        /// An optional ResourceItemInventory which, if provided, expresses the max available number of each expansion item.
        /// Note that the base resource maximums in this will be overridden when applying, to the start conditions say.
        /// </summary>
        public ResourceItemInventory InternalAvailableResourceInventory { get; set; }
        public ReadOnlyResourceItemInventory AvailableResourceInventory => InternalAvailableResourceInventory?.AsReadOnly();

        /// <summary>
        /// This property is used to communicate available expansion item counts before a finalized model exists, with the intent to apply the start conditions during finalization.
        /// If the finalized model already exists, this property is entirely ignored and only <see cref="InternalAvailableResourceInventory"/> matters.
        /// </summary>
        public UnfinalizedResourceItemInventory InternalUnfinalizedAvailableResourceInventory { get; set; }

        public int? MaxPossibleAmount(ConsumableResourceEnum resource)
        {
            return AvailableResourceInventory?.ResourceMaximums.GetAmount(resource);
        }

        public int? MaxPossibleAmount(RechargeableResourceEnum resource)
        {
            return AvailableResourceInventory?.ResourceMaximums.GetAmount(resource);
        }
    }

    /// <summary>
    /// Exposes the read-only portion of a <see cref="LogicalOptions"/>.
    /// </summary>
    public interface ReadOnlyLogicalOptions
    {
        /// <summary>
        /// Creates and returns a full-fledged copy of this LohicalOptions.
        /// </summary>
        /// <returns></returns>
        public LogicalOptions Clone();

        /// <summary>
        /// <para>If true, all techs are enabled unless their name is found in <see cref="DisabledTechs"/>.</para>
        /// <para>If false, all techs are disabled unless their name is found in <see cref="EnabledTechs"/>.</para>
        /// </summary>
        public bool TechsEnabledByDefault { get; }

        /// <summary>
        /// A set of tech names that are disabled. Irrelevant if <see cref="TechsEnabledByDefault"/> is false.
        /// </summary>
        public IReadOnlySet<string> DisabledTechs { get; }

        /// <summary>
        /// A set of tech names that are enabled. Irrelevant if <see cref="TechsEnabledByDefault"/> is true.
        /// </summary>
        public IReadOnlySet<string> EnabledTechs { get; }

        /// <summary>
        /// A set of strat names that are disabled, regardless of their requirements. Only notable strats can be disabled.
        /// </summary>
        public IReadOnlySet<string> DisabledStrats { get; }

        /// <summary>
        /// A set of strat names that are considered to be unattainable.
        /// </summary>
        public IReadOnlySet<string> RemovedGameFlags { get; }

        /// <summary>
        /// A set of item names that are considered to be removed from the game and impossible to obtain.
        /// </summary>
        public IReadOnlySet<string> RemovedItems { get; }

        /// <summary>
        /// The number of tiles needed for the charging of a shinespark to be expected.
        /// </summary>
        public decimal TilesToShineCharge { get; }

        /// <summary>
        /// The number of tiles that are saved by doing a stutter-step to reach the value of <see cref="TilesToShineCharge"/>
        /// </summary>
        public decimal TilesSavedWithStutter { get; }

        /// <summary>
        /// A comparer that can comparer snapshots of in-game resources to decide which is more valuable.
        /// </summary>
        public IInGameResourceEvaluator InGameResourceEvaluator { get; }

        /// <summary>
        /// Returns the number of tries that are logically expected to be attempted before a success for the provided tech.
        /// Note that these values are not automatically applied to extension techs, because those are likely to be more difficult
        /// and have a higher number of tries - but we don't want those values to multiply each other.
        /// </summary>
        /// <param name="tech"></param>
        /// <returns></returns>
        public int NumberOfTries(Tech tech);

        /// <summary>
        /// Returns the number of tries that are logically expected to be attempted before a success for the provided helper.
        /// </summary>
        /// <param name="helper"></param>
        /// <returns></returns>
        public int NumberOfTries(Helper helper);

        /// <summary>
        /// Returns the number of tries that are logically expected to be attempted before a success for the provided strat.
        /// </summary>
        /// <param name="strat"></param>
        /// <returns></returns>
        public int NumberOfTries(Strat strat);

        /// <summary>
        /// The number of frames it's expected for the player to take to press pause after the unpause animation ends when pause spamming.
        /// </summary>
        public int PauseSpamLeewayFrames { get; }

        /// <summary>
        /// When using reserves to manually refill energy, this defines the amount of energy below max energy to which the player is espected to refill.
        /// This is necessary because over-refilling will empty the reserves
        /// </summary>
        public int ReserveRefillLeewayEnergy { get; }

        /// <summary>
        /// Determines how far away from optimal timing (in frames) the player is expected to be able to position a pause with the intent to use reserves.
        /// </summary>
        public int ReserveUsageTimingLeewayFrames { get; }

        /// <summary>
        /// An instance of <see cref="InGameStateComparer"/>, initialized with the current relative resource values.
        /// </summary>
        public InGameStateComparer InGameStateComparer { get; }

        /// <summary>
        /// Indicates whether the value in <see cref="TilesToShineCharge"/> assumes that a stutter-step is being performed.
        /// This is relevant when trying to shine charge on a runway where you can't stutter.
        /// </summary>
        public bool ShineChargesWithStutter { get; }

        /// <summary>
        /// A multiplier applied to all logical heat frame requirements.
        /// Larger values make strats logically require more energy, making them more lenient.
        /// Values below 1 are not recommended.
        /// </summary>
        public decimal HeatLeniencyMultiplier { get; }

        /// <summary>
        /// A multiplier applied to all logical lava frame requirements.
        /// Larger values make strats logically require more energy, making them more lenient.
        /// Values below 1 are not recommended.
        /// </summary>
        public decimal LavaLeniencyMultiplier { get; }

        /// <summary>
        /// A multiplier applied to all logical acid frame requirements.
        /// Larger values make strats logically require more energy, making them more lenient.
        /// Values below 1 are not recommended.
        /// </summary>
        public decimal AcidLeniencyMultiplier { get; }

        /// <summary>
        /// Indicates whether the player is expected to be able to execute the provided Tech according to this LogicalOptions.
        /// </summary>
        /// <param name="tech">Tech to check for</param>
        /// <returns></returns>
        public bool IsTechEnabled(Tech tech);

        /// <summary>
        /// Indicates whether logical options expect the player to shinespark.
        /// </summary>
        /// <returns></returns>
        public bool CanShinespark { get; }

        /// <summary>
        /// Whether the ability to use reserves during a shinespark is enabled.
        /// </summary>
        public bool CanUseReservesToShinespark { get; }

        /// <summary>
        /// Whether the ability to manually use a portion of remaining reserves (within the <see cref="ReserveRefillLeewayEnergy"/>) is logically expected.
        /// </summary>
        public bool CanUsePartialReserves { get; }

        /// <summary>
        /// The number of iframes that must remain (after auto reserves have triggered) for the player to be expected to avoid a double hit.
        /// </summary>
        public int IframesToAvoidDoubleHit { get; }

        /// <summary>
        /// Indicates whether the player is expected to be able to execute the provided Strat according to this LogicalOptions.
        /// </summary>
        /// <param name="strat">Strat to check for</param>
        /// <returns></returns>
        public bool IsStratEnabled(Strat strat);

        /// <summary>
        /// Indicates whether the provided item is considered to be possible to find in the game according to this LogicalOptions.
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public bool IsItemInGame(Item item);

        /// <summary>
        /// Indicates whether Speed Booster is considered to be possible to find in the game according to this LogicalOptions.
        /// </summary>
        /// <returns></returns>
        public bool IsSpeedBoosterInGame();

        /// <summary>
        /// Indicates whether the player is expected to be able to enable the provided GameFlag according to this LogicalOptions.
        /// </summary>
        /// <param name="gameFlag">GameFlag to check for</param>
        /// <returns></returns>
        public bool IsGameFlagEnabled(GameFlag gameFlag);

        /// <summary>
        /// A sub-model containing the logical options with regards to using enemy spawners for farming resources.
        /// </summary>
        public ReadOnlySpawnerFarmingOptions SpawnerFarmingOptions { get; }

        /// <summary>
        /// Some start conditions that can override a model's usual start conditions. Can be null to not override.
        /// </summary>
        public StartConditions StartConditions { get; }

        /// <summary>
        /// An optional ResourceItemInventory which, if provided, expresses the max available number of each expansion item.
        /// It also should have the correct base resource maximums, allowing it to express the max available capacity for all resources.
        /// </summary>
        public ReadOnlyResourceItemInventory AvailableResourceInventory { get; }

        /// <summary>
        /// Returns the max possible amount of the provided resource according to these logical options, if configured.
        /// Returns null otherwise.
        /// </summary>
        /// <param name="resource">Resource to get the max amount of</param>
        /// <returns></returns>
        public int? MaxPossibleAmount(ConsumableResourceEnum resource);

        /// <summary>
        /// Returns the max possible amount of the provided resource according to these logical options, if configured.
        /// Returns null otherwise.
        /// </summary>
        /// <param name="resource">Resource to get the max amount of</param>
        /// <returns></returns>
        public int? MaxPossibleAmount(RechargeableResourceEnum resource);
    }
}
