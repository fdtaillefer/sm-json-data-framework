using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace sm_json_data_parser.Models.Helpers
{
    public class HelperContainer
    {
        public IEnumerable<Helper> Helpers { get; set; } = Enumerable.Empty<Helper>();
    }
}
