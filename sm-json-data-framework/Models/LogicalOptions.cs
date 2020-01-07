using sm_json_data_parser.Models.GameFlags;
using sm_json_data_parser.Models.Rooms;
using sm_json_data_parser.Models.Techs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace sm_json_data_parser.Models
{
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
        public int TilesToShineCharge { get; set; } = 31;

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
