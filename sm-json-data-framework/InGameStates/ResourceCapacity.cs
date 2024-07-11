using sm_json_data_framework.Models.Raw.Items;
using System;
using System.Collections.Generic;
using System.Text;

namespace sm_json_data_framework.InGameStates
{
    /// <summary>
    /// Represents an object with a resource and an amount in the json model.
    /// </summary>
    public class ResourceCapacity : RawResourceCapacity
    {
        public ResourceCapacity()
        {

        }

        public ResourceCapacity(RawResourceCapacity resourceCapacity)
        {
            Resource = resourceCapacity.Resource;
            MaxAmount = resourceCapacity.MaxAmount;
        }
    }
}
