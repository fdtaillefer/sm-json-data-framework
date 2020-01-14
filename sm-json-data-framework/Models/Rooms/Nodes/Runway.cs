using sm_json_data_framework.Models.Rooms.Nodes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace sm_json_data_framework.Models.Rooms.Node
{
    public class Runway : InitializablePostDeserializeInNode
    {
        public int Length { get; set; }

        public int GentleUpTiles { get; set; } = 0;

        public int GentleDownTiles { get; set; } = 0;

        public int SteepUpTiles { get; set; } = 0;

        public int SteepDownTiles { get; set; } = 0;

        public int StartingDownTiles { get; set; } = 0;

        public int EndingUpTiles { get; set; } = 0;

        public IEnumerable<Strat> Strats { get; set; } = Enumerable.Empty<Strat>();

        public bool UsableComingIn = true;

        public int OpenEnd { get; set; }

        public void Initialize(SuperMetroidModel model, Room room, RoomNode node)
        {
            foreach (Strat strat in Strats)
            {
                strat.Initialize(model, room);
            }
        }

        public IEnumerable<string> InitializeReferencedLogicalElementProperties(SuperMetroidModel model, Room room, RoomNode node)
        {
            List<string> unhandled = new List<string>();

            foreach(Strat strat in Strats)
            {
                unhandled.AddRange(strat.InitializeReferencedLogicalElementProperties(model, room));
            }

            return unhandled.Distinct();
        }
    }
}
