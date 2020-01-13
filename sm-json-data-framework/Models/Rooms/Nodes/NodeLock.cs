using sm_json_data_framework.Models.Requirements;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace sm_json_data_framework.Models.Rooms.Nodes
{
    public class NodeLock
    {
        public LockTypeEnum LockType { get; set; }

        public LogicalRequirements Lock { get; set; } = new LogicalRequirements();

        public IEnumerable<Strat> UnlockStrats { get; set; } = Enumerable.Empty<Strat>();

        public IEnumerable<Strat> BypassStrats { get; set; } = Enumerable.Empty<Strat>();

        /// <summary>
        /// Initializes additional properties in this Strat, which wouldn't be initialized by simply parsing a rooms json file.
        /// All such properties are identified in their own documentation and should not be read if this method isn't called.
        /// </summary>
        /// <param name="model">The model to use to initialize the additional properties</param>
        /// <param name="room">The room in which this StratFailure is</param>
        public void Initialize(SuperMetroidModel model, Room room)
        {
            foreach (Strat strat in UnlockStrats)
            {
                strat.Initialize(model, room);
            }

            foreach (Strat strat in BypassStrats)
            {
                strat.Initialize(model, room);
            }
        }

        /// <summary>
        /// Goes through all logical elements within this Lock (and all LogicalRequirements within any of them),
        /// attempting to initialize any property that is an object referenced by another property(which is its identifier).
        /// </summary>
        /// <param name="model">A SuperMetroidModel that contains global data</param>
        /// <param name="room">The room in which this Lock is</param>
        /// <returns>A sequence of strings describing references that could not be initialized properly.</returns>
        public IEnumerable<string> InitializeReferencedLogicalElementProperties(SuperMetroidModel model, Room room)
        {
            List<string> unhandled = new List<string>();

            unhandled.AddRange(Lock.InitializeReferencedLogicalElementProperties(model, room));

            foreach(Strat strat in UnlockStrats)
            {
                unhandled.AddRange(strat.InitializeReferencedLogicalElementProperties(model, room));
            }

            foreach (Strat strat in BypassStrats)
            {
                unhandled.AddRange(strat.InitializeReferencedLogicalElementProperties(model, room));
            }

            return unhandled.Distinct();
        }
    }
}
