using sm_json_data_framework.Models.Raw.Requirements;
using sm_json_data_framework.Models.Requirements;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace sm_json_data_framework.Models.Raw.Helpers
{
    public class RawHelper
    {
        public string Name { get; set; }

        public RawLogicalRequirements Requires { get; set; } = new RawLogicalRequirements();
    }
}
