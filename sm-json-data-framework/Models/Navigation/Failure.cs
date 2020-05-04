using sm_json_data_framework.Models.GameFlags;
using sm_json_data_framework.Models.Items;
using sm_json_data_framework.Models.Rooms;
using sm_json_data_framework.Models.Rooms.Nodes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace sm_json_data_framework.Models.Navigation
{

    /// <summary>
    /// A navigation action that ends in failure. What was attempted is not known.
    /// </summary>
    public class Failure : AbstractNavigationAction
    {
        public static readonly Failure Instance = new Failure();

        private Failure()
        {
            Succeeded = false;
        }

        public override AbstractNavigationAction Reverse(SuperMetroidModel model)
        {
            // Failures have no reverse, they just fail.
            return this;
        }
    }
}
