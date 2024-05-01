using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace sm_json_data_framework.Models.Techs
{
    public class TechCategory
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public IList<Tech> Techs { get; set; } = new List<Tech>();
    }
}
