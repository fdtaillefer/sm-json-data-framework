using System;
using System.Collections.Generic;
using System.Text;

namespace sm_json_data_framework.Models.Requirements.ObjectRequirements.SubObjects
{
    public enum AmmoEnum
    {
        Missile,
        Super,
        PowerBomb
    }

    public static class AmmoEnumExtensions
    {
        /// <summary>
        /// Returns the ConsumableResourceEnum that is consumed when using this ammo type.
        /// </summary>
        /// <param name="ammoEnum"></param>
        /// <returns></returns>
        public static ConsumableResourceEnum GetConsumableResourceEnum(this AmmoEnum ammoEnum)
        {
            return ammoEnum switch
            {
                AmmoEnum.Missile => ConsumableResourceEnum.MISSILE,
                AmmoEnum.Super => ConsumableResourceEnum.SUPER,
                AmmoEnum.PowerBomb => ConsumableResourceEnum.POWER_BOMB
            };
        }
    }
}
