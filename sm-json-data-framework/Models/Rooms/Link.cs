using sm_json_data_framework.Models.Raw.Rooms;
using sm_json_data_framework.Models.Requirements;
using sm_json_data_framework.Models.Rooms.Nodes;
using sm_json_data_framework.Options;
using sm_json_data_framework.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;

namespace sm_json_data_framework.Models.Rooms
{
    /// <summary>
    /// Represents all ways to navigate directly from a specific node to any other node in the same room.
    /// </summary>
    public class Link : AbstractModelElement<UnfinalizedLink, Link>
    {
        private UnfinalizedLink InnerElement { get; set; }

        public Link(UnfinalizedLink innerElement, Action<Link> mappingsInsertionCallback, ModelFinalizationMappings mappings)
            : base(innerElement, mappingsInsertionCallback)
        {
            InnerElement = innerElement;
            FromNode = InnerElement.FromNode.Finalize(mappings);
            To = InnerElement.To.Values.Select(linkTo => linkTo.Finalize(mappings)).ToDictionary(linkTo => linkTo.TargetNode.Id).AsReadOnly();
        }

        public RoomNode FromNode { get; }

        /// <summary>
        /// The details of how this Link links to different nodes, mapped by target node ID.
        /// </summary>
        public IReadOnlyDictionary<int, LinkTo> To { get; }

        protected override void PropagateLogicalOptions(ReadOnlyLogicalOptions logicalOptions)
        {
            foreach (LinkTo linkTo in To.Values)
            {
                linkTo.ApplyLogicalOptions(logicalOptions);
            }
        }

        public override bool CalculateLogicallyRelevant()
        {
            // A link has no logical relevance if it has no destination that can logically be reached
            return To.Values.WhereLogicallyRelevant().Any();
        }
    }

    public class UnfinalizedLink : AbstractUnfinalizedModelElement<UnfinalizedLink, Link>, InitializablePostDeserializeInRoom
    {
        public int FromNodeId { get; set; }

        /// <summary>
        /// <para>Not available before <see cref="InitializeProperties(UnfinalizedSuperMetroidModel, UnfinalizedRoom)"/> has been called.</para>
        /// <para>The node that this link initiates from</para>
        /// </summary>
        public UnfinalizedRoomNode FromNode { get; set; }

        /// <summary>
        /// The details of how this Link links to different nodes, mapped by target node ID.
        /// </summary>
        public IDictionary<int, UnfinalizedLinkTo> To {get;set;} = new Dictionary<int, UnfinalizedLinkTo>();

        public UnfinalizedLink()
        {

        }

        public UnfinalizedLink(RawLink rawLink, LogicalElementCreationKnowledgeBase knowledgeBase)
        {
            FromNodeId = rawLink.From;
            To = rawLink.To.Select(linkTo => new UnfinalizedLinkTo(linkTo, knowledgeBase)).ToDictionary(linkTo => linkTo.TargetNodeId);
        }

        protected override Link CreateFinalizedElement(UnfinalizedLink sourceElement, Action<Link> mappingsInsertionCallback, ModelFinalizationMappings mappings)
        {
            return new Link(sourceElement, mappingsInsertionCallback, mappings);
        }

        public void InitializeProperties(UnfinalizedSuperMetroidModel model, UnfinalizedRoom room)
        {
            foreach (UnfinalizedLinkTo linkTo in To.Values)
            {
                linkTo.InitializeProperties(model, room);
            }

            FromNode = room.Nodes[FromNodeId];
        }

        public IEnumerable<string> InitializeReferencedLogicalElementProperties(UnfinalizedSuperMetroidModel model, UnfinalizedRoom room)
        {
            List<string> unhandled = new List<string>();

            foreach(UnfinalizedLinkTo linkTo in To.Values)
            {
                unhandled.AddRange(linkTo.InitializeReferencedLogicalElementProperties(model, room));
            }

            return unhandled.Distinct();
        }
    }
}
