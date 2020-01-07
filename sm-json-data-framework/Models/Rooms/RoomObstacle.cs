using sm_json_data_parser.Models.Requirements;
using System;
using System.Collections.Generic;
using System.Text;

namespace sm_json_data_parser.Models.Rooms
{
    public class RoomObstacle
    {
        public string Id { get; set; }

        public string Name { get; set; }

        public ObstacleTypeEnum ObstacleType { get; set; }

        public LogicalRequirements Requires { get; set; } = new LogicalRequirements();

        // STITCHME Note?
    }
}
