using sm_json_data_framework.Models.Requirements;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace sm_json_data_framework.Models.Rooms.Nodes
{
    public class NodeLock : InitializablePostDeserializeInNode
    {
        public LockTypeEnum LockType { get; set; }

        public LogicalRequirements Lock { get; set; } = new LogicalRequirements();

        public IEnumerable<Strat> UnlockStrats { get; set; } = Enumerable.Empty<Strat>();

        public IEnumerable<Strat> BypassStrats { get; set; } = Enumerable.Empty<Strat>();

        public void Initialize(SuperMetroidModel model, Room room, RoomNode node)
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

        public IEnumerable<string> InitializeReferencedLogicalElementProperties(SuperMetroidModel model, Room room, RoomNode node)
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
