using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace sm_json_data_framework.Models
{
    /// <summary>
    /// An interface for model elements that have logical properties that describe how they can be used/executed to some extent.
    /// </summary>
    public interface ILogicalExecutionPreProcessable : IModelElement
    {
        /// <summary>
        /// If true, then this element is impossible to use or execute given the current logical options, regardless of in-game state.
        /// </summary>
        public bool LogicallyNever { get; }

        /// <summary>
        /// <para>
        /// If true, then this element is always possible to use or execute (though not necessarily for free) given the current logical options, 
        /// regardless of in-game state.
        /// </para>
        /// <para>
        /// Note that this property never makes a statement on whether retroactive use/execution is possible. That must always be checked separately.
        /// </para>
        /// </summary>
        public bool LogicallyAlways { get; }

        /// <summary>
        /// <para>
        /// If true, not only can this element always be used or executed given the current logical options, regardless of in-game state,
        /// but that fulfillment is also guaranteed to cost no resources.
        /// </para>
        /// <para>
        /// Note that this property never makes a statement on whether retroactive use/execution is possible. That must always be checked separately.
        /// </para>
        /// </summary>
        public bool LogicallyFree { get; }
    }
}
