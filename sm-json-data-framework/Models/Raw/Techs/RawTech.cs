using sm_json_data_framework.Models.Raw.Requirements;
using sm_json_data_framework.Models.Requirements;
using sm_json_data_framework.Models.Techs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace sm_json_data_framework.Models.Raw.Techs
{
    public class RawTech
    {
        public string Name { get; set; }

        public RawLogicalRequirements Requires { get; set; } = new RawLogicalRequirements();

        public IList<RawTech> ExtensionTechs { get; set; } = new List<RawTech>();
    }
}
