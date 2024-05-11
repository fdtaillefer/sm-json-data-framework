using sm_json_data_framework.Models.Rooms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace sm_json_data_framework.Models.Requirements.StringRequirements
{
    /// <summary>
    /// And abstract superclass for logical elements that have an inner string value.
    /// </summary>
    public abstract class AbstractStringLogicalElement<SourceType, ConcreteType> : AbstractLogicalElement<SourceType, ConcreteType> 
        where SourceType: AbstractUnfinalizedLogicalElement<SourceType, ConcreteType>
        where ConcreteType: AbstractLogicalElement<SourceType, ConcreteType>
    {
        public AbstractStringLogicalElement(SourceType innerElement, Action<ConcreteType> mappingsInsertionCallback) 
            :base(innerElement, mappingsInsertionCallback)
        {

        }
    }

    /// <summary>
    /// And abstract superclass for unfinalized logical elements that have an inner string value.
    /// </summary>
    public abstract class AbstractUnfinalizedStringLogicalElement<ConcreteType, TargetType> : AbstractUnfinalizedLogicalElement<ConcreteType, TargetType>
        where ConcreteType : AbstractUnfinalizedLogicalElement<ConcreteType, TargetType>
        where TargetType : AbstractLogicalElement<ConcreteType, TargetType>
    {
        public override IEnumerable<string> InitializeReferencedLogicalElementProperties(SuperMetroidModel model, UnfinalizedRoom room)
        {
            // String logical elements don't have properties
            return Enumerable.Empty<string>();
        }
    }
}
