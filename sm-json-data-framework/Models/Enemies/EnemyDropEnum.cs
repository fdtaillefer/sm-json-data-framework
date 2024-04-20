

using sm_json_data_framework.Models.Items;
using sm_json_data_framework.Models.Requirements;
using System;
using System.Collections.Generic;
using System.Linq;

namespace sm_json_data_framework.Models.Enemies
{
    public enum EnemyDropEnum
    {
        NoDrop,
        SmallEnergy,
        BigEnergy,
        Missile,
        Super,
        PowerBomb
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
                EnemyDropEnum.NoDrop => null,
                EnemyDropEnum.SmallEnergy => ConsumableResourceEnum.Energy,
                EnemyDropEnum.BigEnergy => ConsumableResourceEnum.Energy,
                EnemyDropEnum.Missile => ConsumableResourceEnum.Missile,
                EnemyDropEnum.Super => ConsumableResourceEnum.Super,
                EnemyDropEnum.PowerBomb => ConsumableResourceEnum.PowerBomb,
                _ => throw new Exception($"Unrecognized enemy drop {enemyDrop}")
            };
        }
    }
}
