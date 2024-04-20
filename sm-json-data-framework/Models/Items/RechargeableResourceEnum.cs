using sm_json_data_framework.Models.Enemies;
using sm_json_data_framework.Models.Requirements;
using System;
using System.Collections.Generic;
using System.Text;

namespace sm_json_data_framework.Models.Items
{
    /// <summary>
    /// An enum that lists the different resources that can be recharged in the game. 
    /// This is different from consumable resources because regular energy and reserve energy are recharged differently 
    /// but both satisfy the same consumable resource: energy.
    /// </summary>
    public enum RechargeableResourceEnum
    {
        RegularEnergy,
        ReserveEnergy,
        Missile,
        Super,
        PowerBomb
    }

    public static class RechargeableResourceUtils
    {
        /// <summary>
        /// Returns the enemy drops that can recharge this resource.
        /// </summary>
        /// <param name="resource">This resource</param>
        /// <returns>The enemy drops that can refill this</returns>
        public static IEnumerable<EnemyDropEnum> GetRelatedDrops(this RechargeableResourceEnum resource)
        {
            return resource.ToConsumableResource().GetRelatedDrops();
        }

        /// <summary>
        /// Returns the consumable resource that drains this rechargeable resource.
        /// </summary>
        /// <param name="resource">This rechargeable resource</param>
        /// <returns></returns>
        public static ConsumableResourceEnum ToConsumableResource(this RechargeableResourceEnum resource)
        {
            return resource switch
            {
                RechargeableResourceEnum.RegularEnergy => ConsumableResourceEnum.Energy,
                RechargeableResourceEnum.ReserveEnergy => ConsumableResourceEnum.Energy,
                RechargeableResourceEnum.Missile => ConsumableResourceEnum.Missile,
                RechargeableResourceEnum.Super => ConsumableResourceEnum.Super,
                RechargeableResourceEnum.PowerBomb => ConsumableResourceEnum.PowerBomb,
                _ => throw new Exception($"Unrecognized rechargeable resource {resource}")
            };
        }
    }
}
