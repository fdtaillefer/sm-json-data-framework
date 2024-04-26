using sm_json_data_framework.Models.Techs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace sm_json_data_framework.Models.Raw.Techs
{
    public class RawTechCategory
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public IEnumerable<RawTech> Techs { get; set; } = Enumerable.Empty<RawTech>();
    }
}
