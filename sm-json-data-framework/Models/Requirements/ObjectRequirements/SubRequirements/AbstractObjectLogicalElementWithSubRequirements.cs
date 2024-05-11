using sm_json_data_framework.Models.Rooms;
using System;
using System.Collections.Generic;
using System.Security.AccessControl;
using System.Text;

namespace sm_json_data_framework.Models.Requirements.ObjectRequirements.SubRequirements
{
    /// <summary>
    /// An abstract logical element that is composed of inner logical elements.
    /// </summary>
    /// <typeparam name="SourceType">The unfinalized type that finalizes into this type</typeparam>
    /// <typeparam name="ConcreteType">The self-type of the concrete sub-type</typeparam>
    public abstract class AbstractObjectLogicalElementWithSubRequirements<SourceType, ConcreteType> : AbstractObjectLogicalElement<SourceType, ConcreteType>
        where SourceType : AbstractUnfinalizedObjectLogicalElementWithSubRequirements<SourceType, ConcreteType>
        where ConcreteType : AbstractObjectLogicalElementWithSubRequirements<SourceType, ConcreteType>
    {
        protected AbstractObjectLogicalElementWithSubRequirements(SourceType innerElement, Action<ConcreteType> mappingsInsertionCallback, ModelFinalizationMappings mappings)
            : base(innerElement, mappingsInsertionCallback)
        {
            LogicalRequirements = innerElement.LogicalRequirements.Finalize(mappings);
        }

        /// <summary>
        /// The LogicalRequirements within this logical element.
        /// Fulfilling this logical element will involve fulfilling a number of the elements within these LogicalRequirements, as determined by the subclass.
        /// </summary>
        public LogicalRequirements LogicalRequirements {get; }
    }

    public abstract class AbstractUnfinalizedObjectLogicalElementWithSubRequirements<ConcreteType, TargetType> : AbstractUnfinalizedObjectLogicalElement<ConcreteType, TargetType>
        where ConcreteType : AbstractUnfinalizedObjectLogicalElementWithSubRequirements<ConcreteType, TargetType>
        where TargetType : AbstractObjectLogicalElementWithSubRequirements<ConcreteType, TargetType>
    {
        public UnfinalizedLogicalRequirements LogicalRequirements { get; set; }

        public AbstractUnfinalizedObjectLogicalElementWithSubRequirements()
        {

        }

        public AbstractUnfinalizedObjectLogicalElementWithSubRequirements(UnfinalizedLogicalRequirements logicalRequirements)
        {
            LogicalRequirements = logicalRequirements;
        }

        public override IEnumerable<string> InitializeReferencedLogicalElementProperties(SuperMetroidModel model, UnfinalizedRoom room)
        {
            return LogicalRequirements.InitializeReferencedLogicalElementProperties(model, room);
        }
    }
}
