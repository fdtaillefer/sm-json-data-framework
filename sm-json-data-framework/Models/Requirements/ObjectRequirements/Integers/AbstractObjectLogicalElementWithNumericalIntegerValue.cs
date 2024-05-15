using sm_json_data_framework.Models.Rooms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace sm_json_data_framework.Models.Requirements.ObjectRequirements.Integers
{
    public abstract class AbstractObjectLogicalElementWithNumericalIntegerValue<SourceType, ConcreteType> : AbstractObjectLogicalElementWithInteger<SourceType, ConcreteType>
        where SourceType : AbstractUnfinalizedObjectLogicalElementWithNumericalIntegerValue<SourceType, ConcreteType>
        where ConcreteType : AbstractObjectLogicalElementWithNumericalIntegerValue<SourceType, ConcreteType>
    {
        protected AbstractObjectLogicalElementWithNumericalIntegerValue(int value): base(value)
        {

        }

        protected AbstractObjectLogicalElementWithNumericalIntegerValue(SourceType innerElement, Action<ConcreteType> mappingsInsertionCallback)
            : base(innerElement, mappingsInsertionCallback)
        {

        }
    }

    /// <summary>
    /// An abstract object logical element with a int value that is interepreted as a numerical value.
    /// </summary>
    /// <typeparam name="SourceType">The unfinalized type that finalizes into this type</typeparam>
    /// <typeparam name="ConcreteType">The self-type of the concrete sub-type</typeparam>
    public abstract class AbstractUnfinalizedObjectLogicalElementWithNumericalIntegerValue<ConcreteType, TargetType>
        : AbstractUnfinalizedObjectLogicalElementWithInteger<ConcreteType, TargetType>
        where ConcreteType : AbstractUnfinalizedObjectLogicalElementWithNumericalIntegerValue<ConcreteType, TargetType>
        where TargetType : AbstractObjectLogicalElementWithNumericalIntegerValue<ConcreteType, TargetType>
    {
        public AbstractUnfinalizedObjectLogicalElementWithNumericalIntegerValue()
        {

        }

        public AbstractUnfinalizedObjectLogicalElementWithNumericalIntegerValue(int value): base(value)
        {
            
        }

        public override IEnumerable<string> InitializeReferencedLogicalElementProperties(UnfinalizedSuperMetroidModel model, UnfinalizedRoom room)
        {
            // If the value is just a numerical value, it doesn't need to match up to anything
            return Enumerable.Empty<string>();
        }
    }
}
