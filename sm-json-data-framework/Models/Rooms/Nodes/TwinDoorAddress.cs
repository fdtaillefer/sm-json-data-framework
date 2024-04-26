using sm_json_data_framework.Models.Raw.Rooms.Nodes;
using System;
using System.Collections.Generic;
using System.Text;

namespace sm_json_data_framework.Models.Rooms.Nodes
{
    public class TwinDoorAddress: RawTwinDoorAddress
    {
        public TwinDoorAddress()
        {

        }

        public TwinDoorAddress(RawTwinDoorAddress twinAddress)
        {
            DoorAddress = twinAddress.DoorAddress;
            RoomAddress = twinAddress.RoomAddress;
        }
    }
}
