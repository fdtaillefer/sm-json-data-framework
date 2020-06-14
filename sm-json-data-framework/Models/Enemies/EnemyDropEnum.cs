

using sm_json_data_framework.Models.Items;
using sm_json_data_framework.Models.Requirements;
using System;
using System.Collections.Generic;
using System.Linq;

namespace sm_json_data_framework.Models.Enemies
{
    public enum EnemyDropEnum
    {
        NO_DROP,
        SMALL_ENERGY,
        BIG_ENERGY,
        MISSILE,
        SUPER,
        POWER_BOMB
    }

    public static class EnemyDropsUtils
    {
        /// <summary>
        /// Returns the rechargeable resources that this enemy drop can recharge.
        /// If this drop recharges no resources, returns an empty enumeration.
        /// </summary>
        /// <param name="enemyDrop">This enemy drop</param>
        /// <returns>The resources that this enemy drop can refill</returns>
        public static IEnumerable<RechargeableResourceEnum> GetRechargeableResources(this EnemyDropEnum enemyDrop)
        {
            ConsumableResourceEnum? consumableResource = enemyDrop.GetConsumableResource();
            return consumableResource == null ? Enumerable.Empty<RechargeableResourceEnum>() : ((ConsumableResourceEnum)consumableResource).ToRechargeableResources();
        }

        /// <summary>
        /// Returns the consumable resource that consumes this enemy drop.
        /// /// If this drop recharges no resources, returns null.
        /// </summary>
        /// <param name="enemyDrop">This enemy drop</param>
        /// <returns>The resources that this enemy drop can refill</returns>
        public static ConsumableResourceEnum? GetConsumableResource(this EnemyDropEnum enemyDrop)
        {
            return enemyDrop switch
            {
                EnemyDropEnum.NO_DROP => null,
                EnemyDropEnum.SMALL_ENERGY => ConsumableResourceEnum.ENERGY,
                EnemyDropEnum.BIG_ENERGY => ConsumableResourceEnum.ENERGY,
                EnemyDropEnum.MISSILE => ConsumableResourceEnum.MISSILE,
                EnemyDropEnum.SUPER => ConsumableResourceEnum.SUPER,
                EnemyDropEnum.POWER_BOMB => ConsumableResourceEnum.POWER_BOMB,
                _ => throw new Exception($"Unrecognized enemy drop {enemyDrop}")
            };
        }
    }
}
