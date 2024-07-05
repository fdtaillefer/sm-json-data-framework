using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Markup;

namespace sm_json_data_framework.Models.Requirements.ObjectRequirements.Arrays
{
    public abstract class AbstractArrayLogicalElement<SourceItemType, ConcreteItemType, SourceType, ConcreteType> : AbstractObjectLogicalElement<SourceType, ConcreteType>
        where SourceType : AbstractUnfinalizedArrayLogicalElement<SourceItemType, ConcreteItemType, SourceType, ConcreteType>
        where ConcreteType : AbstractArrayLogicalElement<SourceItemType, ConcreteItemType, SourceType, ConcreteType>
    {
        public AbstractArrayLogicalElement(SourceType sourceElement, Action<ConcreteType> mappingsInsertionCallback, ModelFinalizationMappings mappings)
            : base(sourceElement, mappingsInsertionCallback)
        {
            Value = sourceElement.Value.Select(sourceItem => ConvertItem(sourceItem, mappings)).ToList();
        }

        /// <summary>
        /// Converts the provided item (from an unfinalized array logical element) into an item for this array logical element.
        /// </summary>
        /// <param name="sourceItem">Item to convert</param>
        /// <param name="mappings">A model containing mappings between unfinalized instances and corresponding finalized instances</param>
        /// <returns></returns>
        protected abstract ConcreteItemType ConvertItem(SourceItemType sourceItem, ModelFinalizationMappings mappings);

        protected List<ConcreteItemType> Value { get; } = new();
    }

    public abstract class AbstractUnfinalizedArrayLogicalElement<ConcreteItemType, TargetItemType, ConcreteType, TargetType> : AbstractUnfinalizedObjectLogicalElement<ConcreteType, TargetType>
    where ConcreteType : AbstractUnfinalizedArrayLogicalElement<ConcreteItemType, TargetItemType, ConcreteType, TargetType>
    where TargetType : AbstractArrayLogicalElement<ConcreteItemType, TargetItemType, ConcreteType, TargetType>
    {
        public AbstractUnfinalizedArrayLogicalElement(IList<ConcreteItemType> items)
        {
            Value = new List<ConcreteItemType>(items);
        }

        public List<ConcreteItemType> Value { get; }
    }
}
