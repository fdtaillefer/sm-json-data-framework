using sm_json_data_framework.Models.Items;
using sm_json_data_framework.Models.Requirements;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;
using static sm_json_data_framework.Options.SpawnerFarmingOptions;

namespace sm_json_data_framework.Options
{
    /// <summary>
    /// Logical options to configure when farming from enemy spawners should be possible according to logic or not.
    /// </summary>
    public class SpawnerFarmingOptions : ReadOnlySpawnerFarmingOptions
    {
        public SpawnerFarmingOptions()
        {
            // Set the minimum viable farming rates per second to default values
            InternalMinimumRatesPerSecond = new Dictionary<ConsumableResourceEnum, decimal>
            {
                // The rate for energy off 5 Gamets, after heat loss, is roughly 13.7 assuming missiles and supers (but not PBs) are full.
                // With a 10% safety reduction in drops, that goes down to just about 10.8.
                // We surely want a minimum rate that allows for energy refills at Gamet spawners in heated rooms, so we have a value of 10
                { ConsumableResourceEnum.Energy, 10},
                { ConsumableResourceEnum.Missile, 0.175M},
                // The rate for supers off one Zebbo is roughly 0.196 (with a 2-second cycle)
                { ConsumableResourceEnum.Super, 0.175M},
                { ConsumableResourceEnum.PowerBomb, 0.175M}
            };
        }

        public SpawnerFarmingOptions(SpawnerFarmingOptions other)
        {
            InternalMinimumRatesPerSecond = new Dictionary<ConsumableResourceEnum, decimal>(other.InternalMinimumRatesPerSecond);
            SafetyMarginPercent = other.SafetyMarginPercent;
        }

        public SpawnerFarmingOptions Clone()
        {
            return new SpawnerFarmingOptions(this);
        }

        public ReadOnlySpawnerFarmingOptions AsReadOnly()
        {
            return this;
        }

        public SpawnerFarmingOptions(IDictionary<ConsumableResourceEnum, decimal> minimumRatesPerSecond)
        {
            InternalMinimumRatesPerSecond = new Dictionary<ConsumableResourceEnum, decimal>(minimumRatesPerSecond);
        }

        public IDictionary<ConsumableResourceEnum, decimal> InternalMinimumRatesPerSecond { get; set; }
        public IReadOnlyDictionary<ConsumableResourceEnum, decimal> MinimumRatesPerSecond { get { return InternalMinimumRatesPerSecond.AsReadOnly(); } }

        public decimal SafetyMarginPercent { get; set; } = 10;
    }

    /// <summary>
    /// Exposes the read-only portion of a <see cref="SpawnerFarmingOptions"/>.
    /// </summary>
    public interface ReadOnlySpawnerFarmingOptions
    {
        /// <summary>
        /// Creates and returns a full-fledges copy of this SpawnerFarmingOptions.
        /// </summary>
        /// <returns></returns>
        public SpawnerFarmingOptions Clone();

        /// <summary>
        /// A dictionary containing, per conumsable resource, the minimum amount per second that farming should yield 
        /// in order for farming to be logically required.
        /// </summary>
        public IReadOnlyDictionary<ConsumableResourceEnum, decimal> MinimumRatesPerSecond { get; }

        /// <summary>
        /// <para>A percent value that will be used to adjust expected drops to account for bad luck
        /// when farming while trying to offset an ongoing loss of the farmed resource.</para>
        /// <para>When using this, the drop rate will be decreased by this percent.</para>
        /// </summary>
        public decimal SafetyMarginPercent { get; }
    }
}
