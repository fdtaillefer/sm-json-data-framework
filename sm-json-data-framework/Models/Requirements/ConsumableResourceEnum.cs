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
}
