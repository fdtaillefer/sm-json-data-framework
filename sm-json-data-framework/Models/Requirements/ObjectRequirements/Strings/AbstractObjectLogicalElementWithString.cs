using sm_json_data_framework.Models.Requirements.ObjectRequirements.SubRequirements;
using System;
using System.Collections.Generic;
using System.Text;

namespace sm_json_data_framework.Models.Requirements.ObjectRequirements.Strings
{
    /// <summary>
    /// An abstract object logical element whose json form was an object with just string property with a meaningful property name.
    /// </summary>
    /// <typeparam name="SourceType">The unfinalized type that finalizes into this type</typeparam>
    /// <typeparam name="ConcreteType">The self-type of the concrete sub-type</typeparam>
    public abstract class AbstractObjectLogicalElementWithStrings<SourceType, ConcreteType> : AbstractObjectLogicalElement<SourceType, ConcreteType>
        where SourceType : AbstractUnfinalizedObjectLogicalElementWithString<SourceType, ConcreteType>
        where ConcreteType : AbstractObjectLogicalElementWithStrings<SourceType, ConcreteType>
    {
        private AbstractUnfinalizedObjectLogicalElementWithString<SourceType, ConcreteType> InnerElement { get; set; }

        protected AbstractObjectLogicalElementWithStrings(SourceType innerElement, Action<ConcreteType> mappingsInsertionCallback)
            : base(innerElement, mappingsInsertionCallback)
        {
            InnerElement = innerElement;
        }

        /// <summary>
        /// The string value of this logical element.
        /// </summary>
        public string Value { get { return InnerElement.Value; } }
    }

    public abstract class AbstractUnfinalizedObjectLogicalElementWithString<ConcreteType, TargetType> : AbstractUnfinalizedObjectLogicalElement<ConcreteType, TargetType>
        where ConcreteType : AbstractUnfinalizedObjectLogicalElementWithString<ConcreteType, TargetType>
        where TargetType : AbstractObjectLogicalElementWithStrings<ConcreteType, TargetType>
    {
        public AbstractUnfinalizedObjectLogicalElementWithString()
        {

        }

        public AbstractUnfinalizedObjectLogicalElementWithString(string value)
        {
            Value = value;
        }

        public string Value { get; set; }
    }
}
