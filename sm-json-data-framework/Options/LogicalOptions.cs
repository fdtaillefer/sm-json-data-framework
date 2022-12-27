using sm_json_data_framework.Models.GameFlags;
using sm_json_data_framework.Models.Helpers;
using sm_json_data_framework.Models.InGameStates;
using sm_json_data_framework.Models.Requirements;
using sm_json_data_framework.Models.Rooms;
using sm_json_data_framework.Models.Techs;
using sm_json_data_framework.Options.ResourceValues;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace sm_json_data_framework.Options
{
    /// <summary>
    /// Options that describe what the player is expected to be able or unable to do.
    /// </summary>
    public class LogicalOptions
    {
        public LogicalOptions()
        {
            // Default resource comparer
            InGameResourceEvaluator = new ResourceEvaluatorByFixedValues(
                new Dictionary<ConsumableResourceEnum, int> {
                    {ConsumableResourceEnum.ENERGY,  1},
                    // Missile drops are super plentiful, AND each drop gives twice as much as Supers.
                    {ConsumableResourceEnum.MISSILE,  3},
                    {ConsumableResourceEnum.SUPER,  30},
                    {ConsumableResourceEnum.POWER_BOMB,  60}
                }
            );

            SpawnerFarmingOptions = new SpawnerFarmingOptions();
        }

        /// <summary>
        /// <para>If true, all techs are enabled unless their name is found in <see cref="DisabledTechs"/>.</para>
        /// <para>If false, all techs are disabled unless their name is found in <see cref="EnabledTechs"/>.</para>
        /// </summary>
        public bool TechsEnabledByDefault { get; set; } = true;

        /// <summary>
        /// A sequence of tech names that are disabled. Irrelevant if <see cref="TechsEnabledByDefault"/> is false.
        /// </summary>
        public IEnumerable<string> DisabledTechs { get; set; } = Enumerable.Empty<string>();

        /// <summary>
        /// A sequence of tech names that are enabled. Irrelevant if <see cref="TechsEnabledByDefault"/> is true.
        /// </summary>
        public IEnumerable<string> EnabledTechs { get; set; } = Enumerable.Empty<string>();

        /// <summary>
        /// A sequence of strat names that are disabled, regardless of their requirements. Only notable strats can be disabled.
        /// </summary>
        public IEnumerable<string> DisabledStrats { get; set; } = Enumerable.Empty<string>();

        public IEnumerable<string> RemovedGameFlags { get; set; } = Enumerable.Empty<string>();

        /// <summary>
        /// The number of tiles needed for the charging of a shinespark to be expected.
        /// </summary>
        public decimal TilesToShineCharge { get; set; } = 32.5M;

        /// <summary>
        /// The number of tiles that are saved by doing a stutter-step to reach the value of <see cref="TilesToShineCharge"/>
        /// </summary>
        public decimal TilesSavedWithStutter { get; set; } = 0M;

        private IInGameResourceEvaluator _inGameResourceEvaluator;
        /// <summary>
        /// A comparer that can comparer snapshots of in-game resources to decide which is more valuable.
        /// </summary>
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
        public void RegisterTechTries(string techName, int numberOfTries)
        {
            TriesByTech.Add(techName, numberOfTries);
        }

        /// <summary>
        /// Returns the number of tries that are logically expected to be attempted before a success for the provided tech.
        /// </summary>
        /// <param name="tech"></param>
        /// <returns></returns>
        public int NumberOfTries(Tech tech)
        {
            if (TriesByTech.TryGetValue(tech.Name, out int tries))
            {
                return tries;
            }
            else
            {
                return 1;
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
        public void RegisterHelperTries(string helperName, int numberOfTries)
        {
            TriesByHelper.Add(helperName, numberOfTries);
        }

        /// <summary>
        /// Returns the number of tries that are logically expected to be attempted before a success for the provided helper.
        /// </summary>
        /// <param name="helper"></param>
        /// <returns></returns>
        public int NumberOfTries(Helper helper)
        {
            if (TriesByHelper.TryGetValue(helper.Name, out int tries))
            {
                return tries;
            }
            else
            {
                return 1;
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
        public void RegisterStratTries(string stratName, int numberOfTries)
        {
            TriesByStrat.Add(stratName, numberOfTries);
        }

        /// <summary>
        /// Returns the number of tries that are logically expected to be attempted before a success for the provided strat.
        /// </summary>
        /// <param name="strat"></param>
        /// <returns></returns>
        public int NumberOfTries(Strat strat)
        {
            if (strat.Notable && TriesByStrat.TryGetValue(strat.Name, out int tries))
            {
                return tries;
            }
            else
            {
                return 1;
            }
        }

        /// <summary>
        /// If true, current counts for resources are maintained and used by logical elements.
        /// If false, each resource check only looks at the player's max value, so for example two consecutive losses of 200 energy would still only require 2 E-Tanks.
        /// </summary>
        public bool ResourceTrackingEnabled { get; set; } = true;

        /// <summary>
        /// An instance of <see cref="InGameStateComparer"/>, initialized with the current relative resource values.
        /// </summary>
        public InGameStateComparer InGameStateComparer { get; private set; }

        /// <summary>
        /// Indicates whether the value in <see cref="TilesToShineCharge"/> assumes that a stutter-step is being performed.
        /// This is relevant when trying to shine charge on a runway where you can't stutter.
        /// </summary>
        public bool ShineChargesWithStutter { get; set; } = false;

        /// <summary>
        /// A multiplier applied to all logical heat frame requirements.
        /// Larger values make strats logically require more energy, making them more lenient.
        /// Values below 1 are not recommended.
        /// </summary>
        public decimal HeatLeniencyMultiplier { get; set; } = 1;

        /// <summary>
        /// A multiplier applied to all logical lava frame requirements.
        /// Larger values make strats logically require more energy, making them more lenient.
        /// Values below 1 are not recommended.
        /// </summary>
        public decimal LavaLeniencyMultiplier { get; set; } = 1;

        /// <summary>
        /// A multiplier applied to all logical acid frame requirements.
        /// Larger values make strats logically require more energy, making them more lenient.
        /// Values below 1 are not recommended.
        /// </summary>
        public decimal AcidLeniencyMultiplier { get; set; } = 1;

        public bool IsTechEnabled(Tech tech)
        {
            if (TechsEnabledByDefault)
            {
                return !DisabledTechs.Contains(tech.Name);
            }
            else
            {
                return EnabledTechs.Contains(tech.Name);
            }
        }

        public bool IsStratEnabled (Strat strat)
        {
            // Non-notable strats are always enabled. Beyond that, strats are enabled by default unless disabled
            return (!strat.Notable || !DisabledStrats.Contains(strat.Name));
        }

        public bool IsGameFlagEnabled(GameFlag gameFlag)
        {
            return !RemovedGameFlags.Contains(gameFlag.Name);
        }

        public SpawnerFarmingOptions SpawnerFarmingOptions { get; set; }
    }
}
