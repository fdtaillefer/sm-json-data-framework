﻿using sm_json_data_framework.Models.Rooms;
using System;
using System.Collections.Generic;
using System.Text;

namespace sm_json_data_framework.Models.Requirements.ObjectRequirements.SubRequirements
{
    public abstract class AbstractObjectLogicalElementWithSubRequirements : AbstractObjectLogicalElement
    {
        public LogicalRequirements LogicalRequirements { get; set; }
        
        public override IEnumerable<string> InitializeReferencedLogicalElementProperties(SuperMetroidModel model, Room room)
        {
            return LogicalRequirements.InitializeReferencedLogicalElementProperties(model, room);
        }
    }
}
