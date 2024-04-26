﻿using sm_json_data_framework.Models.Techs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace sm_json_data_framework.Models.Raw.Techs
{
    public class RawTechContainer
    {
        public IEnumerable<RawTechCategory> TechCategories { get; set; } = Enumerable.Empty<RawTechCategory>();
    }
}
