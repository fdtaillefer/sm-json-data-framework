using System;
using System.Collections.Generic;
using System.Text;

namespace sm_json_data_framework.Models.Navigation
{
    public class GameNavigatorOptions
    {
        /// <summary>
        /// Indicates whether picking up an expansion item should adjust current resources or not.
        /// Should be set to true when just moving around in the game.
        /// Consider setting to false when using the navigator to explore logical implications,
        /// to prevent some logic from relying on a non-repeatable item pickup.
        /// </summary>
        public bool AddResourcesOnExpansionPickup { get; set; } = true;
    }
}
