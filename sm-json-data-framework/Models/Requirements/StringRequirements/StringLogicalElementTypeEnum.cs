using sm_json_data_framework.Models.GameFlags;
using sm_json_data_framework.Models.Helpers;
using sm_json_data_framework.Models.Items;
using sm_json_data_framework.Models.Techs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace sm_json_data_framework.Models.Requirements.StringRequirements
{

    /// <summary>
    /// An enum identifying the possible types of string logical elements.
    /// </summary>
    public enum StringLogicalElementTypeEnum
    {
        Never,
        Helper,
        Tech,
        Item,
        Gameflag
    }
}
