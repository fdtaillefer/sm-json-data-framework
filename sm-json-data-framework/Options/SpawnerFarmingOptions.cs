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
        public static readonly decimal DefaultSafetyMarginPercent = 10;
        // The rate for energy off 5 Gamets, after heat loss, is roughly 13.7 assuming missiles and supers (but not PBs) are full.
        // With a 10% safety reduction in drops, that goes down to just about 10.8.
        // We surely want a minimum rate that allows that refill
        // What's more: the rate for energy off a Zebbo (120-frame cycle) if non-farmable Missiles and PBs are not full (but supers are) is ~9.135.
        // They drop big energy at a very high rate in that situation (96) so that seems a reasonable farm.
        public static readonly decimal DefaultEnergyMinimumRatePerSecond = 9;
        public static readonly decimal DefaultMissileMinimumRatePerSecond = 0.175M;
        // The rate for supers off one Zebbo is roughly 0.196 (with a 2-second cycle)
        public static readonly decimal DefaultSuperMinimumRatePerSecond = 0.175M;
        public static readonly decimal DefaultPowerBombMinimumRatePerSecond = 0.175M;

        public SpawnerFarmingOptions()
        {
            // Set the minimum viable farming rates per second to default values
            InternalMinimumRatesPerSecond = new Dictionary<ConsumableResourceEnum, decimal>
            {
                { ConsumableResourceEnum.Energy, DefaultEnergyMinimumRatePerSecond},
                { ConsumableResourceEnum.Missile, DefaultMissileMinimumRatePerSecond},
                { ConsumableResourceEnum.Super, DefaultSuperMinimumRatePerSecond},
                { ConsumableResourceEnum.PowerBomb, DefaultPowerBombMinimumRatePerSecond}
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
        public IReadOnlyDictionary<ConsumableResourceEnum, decimal> MinimumRatesPerSecond => InternalMinimumRatesPerSecond.AsReadOnly();

        public decimal SafetyMarginPercent { get; set; } = DefaultSafetyMarginPercent;
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
