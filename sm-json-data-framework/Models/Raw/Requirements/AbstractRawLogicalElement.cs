using sm_json_data_framework.Models.Requirements;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace sm_json_data_framework.Models.Raw.Requirements
{
    /// <summary>
    /// Abstract superclass for all raw logical elements.
    /// Raw logical elements are exact matches for the contents of a logical element in the json file.
    /// </summary>
    public abstract class AbstractRawLogicalElement
    {
        /// <summary>
        /// Creates and returns a logical element that corresponds to this raw logical element.
        /// </summary>
        /// <param name="knowledgeBase">A model containing all data that could be needed to create any logical element.</param>
        /// <returns></returns>
        public abstract AbstractLogicalElement ToLogicalElement(LogicalElementCreationKnowledgeBase knowledgeBase);
    }
}
