using sm_json_data_parser.Models.Requirements;
using System;
using System.Collections.Generic;
using System.Text;

namespace sm_json_data_parser.Models.Helpers
{
    public class Helper
    {
        public string Name { get; set; }

        public LogicalRequirements Requires { get; set; } = new LogicalRequirements();

        // STITCHME Note?
    }
}
