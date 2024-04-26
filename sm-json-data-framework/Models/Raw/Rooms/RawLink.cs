using sm_json_data_framework.Models.Rooms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace sm_json_data_framework.Models.Raw.Rooms
{
    public class RawLink
    {
        public int From { get; set; }

        public IEnumerable<RawLinkTo> To { get; set; } = Enumerable.Empty<RawLinkTo>();
    }
}
