using sm_json_data_framework.Models.Rooms.Nodes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace sm_json_data_framework.Models.Rooms
{
    /// <summary>
    /// An exception expressing that a link from one node to another could not be found
    /// </summary>
    public class LinkNotFoundException : Exception
    {
        public LinkNotFoundException(Room room, RoomNode nodeFrom, int targetNodeId)
            : base($"Could not find a link from node {nodeFrom.Id} to node {targetNodeId} in room '{room.Name}'")
        {

        }
    }
}
