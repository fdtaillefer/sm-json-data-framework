using sm_json_data_framework.Models.GameFlags;
using sm_json_data_framework.Models.Helpers;
using sm_json_data_framework.Models.Items;
using sm_json_data_framework.Models.Requirements.ObjectRequirements;
using sm_json_data_framework.Models.Requirements.StringRequirements;
using sm_json_data_framework.Models.Techs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace sm_json_data_framework.Models.Requirements
{
    /// <summary>
    /// A model that contains the data and config needed to create logical elements.
    /// </summary>
    public class LogicalElementCreationKnowledgeBase
    {
        /// <summary>
        /// A dictionary containing the logical element types to create, per ObjectLogicalElementTypeEnum.
        /// </summary>
        public IDictionary<ObjectLogicalElementTypeEnum, Type> ObjectLogicalElementTypes { get; set; }

        /// <summary>
        /// A dictionary containing the logical element types to create, per StringLogicalElementTypeEnum.
        /// </summary>
        public IDictionary<StringLogicalElementTypeEnum, Type> StringLogicalElementTypes { get; set; }

        /// <summary>
        /// The techs in this model, mapped by name.
        /// </summary>
        public IDictionary<string, UnfinalizedTech> Techs { get; set;  }

        /// <summary>
        /// The helpers in this model, mapped by name.
        /// </summary>
        public IDictionary<string, UnfinalizedHelper> Helpers { get; set; }

        /// <summary>
        /// The items in this model, mapped by name.
        /// </summary>
        public IDictionary<string, UnfinalizedItem> Items { get; set; } = new Dictionary<string, UnfinalizedItem>();

        /// <summary>
        /// The game flags in this model, mapped by name.
        /// </summary>
        public IDictionary<string, UnfinalizedGameFlag> GameFlags { get; set; } = new Dictionary<string, UnfinalizedGameFlag>();
    }
}
