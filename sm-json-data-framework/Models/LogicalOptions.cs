using sm_json_data_framework.Models.GameFlags;
using sm_json_data_framework.Models.Rooms;
using sm_json_data_framework.Models.Techs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace sm_json_data_framework.Models
{
    /// <summary>
    /// Options that describe what the player is expected to be able or unable to do.
    /// </summary>
    public class LogicalOptions
    {
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

        /// <summary>
        /// Indicates whether the value in <see cref="TilesToShineCharge"/> assumes that a stutter-step is being performed.
        /// This is relevant when trying to shine charge on a runway where you can't stutter.
        /// </summary>
        public bool ShineChargesWithStutter { get; set; } = false;

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
   
    }
}
