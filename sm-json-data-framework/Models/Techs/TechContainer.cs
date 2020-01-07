using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace sm_json_data_parser.Models.Techs
{
    public class TechContainer
    {
        public IEnumerable<Tech> Techs { get; set; } = Enumerable.Empty<Tech>();
    }
}
