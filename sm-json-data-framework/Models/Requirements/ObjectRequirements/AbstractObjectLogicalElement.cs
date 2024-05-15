using System;
using System.Collections.Generic;
using System.Text;

namespace sm_json_data_framework.Models.Requirements.ObjectRequirements
{
    /// <summary>
    /// An abstract logical element whose json format took the form of an object.
    /// </summary>
    /// <typeparam name="SourceType">The unfinalized type that finalizes into this type</typeparam>
    /// <typeparam name="ConcreteType">The self-type of the concrete sub-type</typeparam>
    public abstract class AbstractObjectLogicalElement<SourceType, ConcreteType> : AbstractLogicalElement<SourceType, ConcreteType>
        where SourceType : AbstractUnfinalizedLogicalElement<SourceType, ConcreteType>
        where ConcreteType : AbstractLogicalElement<SourceType, ConcreteType>
    {
        public AbstractObjectLogicalElement()
        {

        }

        public AbstractObjectLogicalElement(SourceType innerElement, Action<ConcreteType> mappingsInsertionCallback)
            : base(innerElement, mappingsInsertionCallback)
        {

        }
    }

    public abstract class AbstractUnfinalizedObjectLogicalElement<ConcreteType, TargetType> : AbstractUnfinalizedLogicalElement<ConcreteType, TargetType>
        where ConcreteType : AbstractUnfinalizedLogicalElement<ConcreteType, TargetType>
        where TargetType : AbstractLogicalElement<ConcreteType, TargetType>
    {

    }
}
