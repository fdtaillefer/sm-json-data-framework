using sm_json_data_framework.Models.Enemies;
using sm_json_data_framework.Models.Items;
using System;
using System.Collections.Generic;
using System.Text;

namespace sm_json_data_framework.Models.Requirements
{
    /// <summary>
    /// An enum that lists resources that can be spent. It differs from resources that can be recharged because of the distinction 
    /// between regular energy and reserve energy, which doesn't really exist when spending.
    /// </summary>
    public enum ConsumableResourceEnum
    {
        ENERGY,
        MISSILE,
        SUPER,
        POWER_BOMB
    }
    public static class ConsumableResourceUtils
    {
        /// <summary>
        /// Returns the enemy drops that this consumable resource can consume.
        /// </summary>
        /// <param name="resource">This resource</param>
        /// <returns>The enemy drops that this can consume</returns>
        public static IEnumerable<EnemyDropEnum> GetRelatedDrops(this ConsumableResourceEnum resource)
        {
            return resource switch
            {
                ConsumableResourceEnum.ENERGY => new[] { EnemyDropEnum.SMALL_ENERGY, EnemyDropEnum.BIG_ENERGY },
                ConsumableResourceEnum.MISSILE => new[] { EnemyDropEnum.MISSILE },
                ConsumableResourceEnum.SUPER => new[] { EnemyDropEnum.SUPER },
                ConsumableResourceEnum.POWER_BOMB => new[] { EnemyDropEnum.POWER_BOMB },
                _ => throw new Exception($"Unrecognized consumable resource {resource}")
            };
        }

        /// <summary>
        /// Returns the rechargeable resources that this consumable resource drains.
        /// </summary>
        /// <param name="resource">This resource</param>
        /// <returns></returns>
        public static IEnumerable<RechargeableResourceEnum> ToRechargeableResources(this ConsumableResourceEnum resource)
        {
            return resource switch
            {
                ConsumableResourceEnum.ENERGY => new[] { RechargeableResourceEnum.RegularEnergy, RechargeableResourceEnum.ReserveEnergy },
                ConsumableResourceEnum.MISSILE => new[] { RechargeableResourceEnum.Missile },
                ConsumableResourceEnum.SUPER => new[] { RechargeableResourceEnum.Super },
                ConsumableResourceEnum.POWER_BOMB => new[] { RechargeableResourceEnum.PowerBomb },
                _ => throw new Exception($"Unrecognized consumable resource {resource}")
            };
        }
    }
}
