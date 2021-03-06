﻿using sm_json_data_framework.Models.Items;
using sm_json_data_framework.Models.Requirements;
using System;
using System.Collections.Generic;
using System.Text;

namespace sm_json_data_framework.Options
{
    /// <summary>
    /// Logical options to configure when farming from enemy spawners should be possible according to logic or not.
    /// </summary>
    public class SpawnerFarmingOptions
    {
        public SpawnerFarmingOptions()
        {
            // Set the minimum viable farming rates per second to default values
            MinimumRatesPerSecond = new Dictionary<ConsumableResourceEnum, decimal>
            {
                // The rate for energy off 5 Gamets, after heat loss, is roughly 13.7 assuming missiles and supers (but not PBs) are full.
                // With a 10% safety reduction in drops, that goes down to just about 10.8.
                // We surely want a minimum rate that allows for energy refills at Gamet spawners in heated rooms, so we have a value of 10
                { ConsumableResourceEnum.ENERGY, 10},
                { ConsumableResourceEnum.MISSILE, 0.175M},
                // The rate for supers off one Zebbo is roughly 0.196 (with a 2-second cycle)
                { ConsumableResourceEnum.SUPER, 0.175M},
                { ConsumableResourceEnum.POWER_BOMB, 0.175M}
            };
        }

        public SpawnerFarmingOptions(IDictionary<ConsumableResourceEnum, decimal> minimumRatesPerSecond)
        {
            MinimumRatesPerSecond = new Dictionary<ConsumableResourceEnum, decimal>(minimumRatesPerSecond);
        }

        public IDictionary<ConsumableResourceEnum, decimal> MinimumRatesPerSecond { get; private set; }

        /// <summary>
        /// <para>A percent value that will be used to adjust expected drops to account for bad luck
        /// when farming while trying to offset an ongoing loss of the farmed resource.</para>
        /// <para>When using this, the drop rate will be decreased by this percent.</para>
        /// </summary>
        public decimal SafetyMarginPercent { get; set; } = 10;
    }
}
