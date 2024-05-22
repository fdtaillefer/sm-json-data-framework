using sm_json_data_framework.Models.Requirements.ObjectRequirements;
using sm_json_data_framework.Models.Requirements.ObjectRequirements.Strings;
using System;
using System.Collections.Generic;
using System.Text;

namespace sm_json_data_framework.Models.Requirements.ObjectRequirements.Integers
{
    /// <summary>
    /// An abstract object logical element whose json form was an object with just int property with a meaningful property name.
    /// </summary>
    /// <typeparam name="SourceType">The unfinalized type that finalizes into this type</typeparam>
    /// <typeparam name="ConcreteType">The self-type of the concrete sub-type</typeparam>
    public abstract class AbstractObjectLogicalElementWithInteger<SourceType, ConcreteType>
        : AbstractObjectLogicalElement<SourceType, ConcreteType>
        where SourceType : AbstractUnfinalizedObjectLogicalElementWithInteger<SourceType, ConcreteType>
        where ConcreteType : AbstractObjectLogicalElementWithInteger<SourceType, ConcreteType>
    {
        protected AbstractObjectLogicalElementWithInteger (int value)
        {
            Value = value;
        }

        protected AbstractObjectLogicalElementWithInteger(SourceType sourceElement, Action<ConcreteType> mappingsInsertionCallback)
            : base(sourceElement, mappingsInsertionCallback)
        {
            Value = sourceElement.Value;
        }

        /// <summary>
        /// The int value of this logical element.
        /// </summary>
        public int Value { get; }
    }

    public abstract class AbstractUnfinalizedObjectLogicalElementWithInteger<ConcreteType, TargetType> 
        : AbstractUnfinalizedObjectLogicalElement<ConcreteType, TargetType>
        where ConcreteType : AbstractUnfinalizedObjectLogicalElementWithInteger<ConcreteType, TargetType>
        where TargetType : AbstractObjectLogicalElementWithInteger<ConcreteType, TargetType>
    {
        public int Value { get; set; }

        public AbstractUnfinalizedObjectLogicalElementWithInteger()
        {

        }

        public AbstractUnfinalizedObjectLogicalElementWithInteger(int value)
        {
            Value = value;
        }
    }
}
