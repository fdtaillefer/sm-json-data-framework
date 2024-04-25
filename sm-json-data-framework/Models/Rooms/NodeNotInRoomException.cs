using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace sm_json_data_framework.Models.Rooms
{
    public class NodeNotInRoomException: Exception
    {
        public NodeNotInRoomException(Room room, int nodeId)
            : base($"Could not find node {nodeId} in room '{room.Name}'")
        {

        }
    }
}
