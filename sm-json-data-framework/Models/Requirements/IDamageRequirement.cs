using System;
using System.Collections.Generic;
using System.Text;

namespace sm_json_data_framework.Models.Requirements
{
    public interface IDamageRequirement
    {
        /// <summary>
        /// Calculates the damage requirement. Implementing classes should consider making this virtual.
        /// </summary>
        /// <param name="hasVaria">Whether Varia Suit is available</param>
        /// <param name="hasGravity">Whether Gravity Suit is available</param>
        /// <returns></returns>
        public int CalculateDamage(bool hasVaria, bool hasGravity);
    }
}
