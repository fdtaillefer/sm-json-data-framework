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
}
