﻿using sm_json_data_framework.Models.Requirements.ObjectRequirements;
using System;
using System.Collections.Generic;
using System.Text;

namespace sm_json_data_framework.Models.Requirements.ObjectRequirements.Integers
{
    public abstract class AbstractObjectLogicalElementWithInteger : AbstractObjectLogicalElement
    {
        public int Value { get; set; }
    }
}
