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
    /// Represents how a parent <see cref="Link"/> can lead to a given node, and how to get there.
    /// </summary>
    public class LinkTo : AbstractModelElement<UnfinalizedLinkTo, LinkTo>
    {
        private UnfinalizedLinkTo InnerElement { get; set; }

        public LinkTo(UnfinalizedLinkTo innerElement, Action<LinkTo> mappingsInsertionCallback, ModelFinalizationMappings mappings)
            : base(innerElement, mappingsInsertionCallback)
        {
            InnerElement = innerElement;
            TargetNode = InnerElement.TargetNode.Finalize(mappings);
            Strats = InnerElement.Strats.Values.Select(strat => strat.Finalize(mappings)).ToDictionary(strat => strat.Name).AsReadOnly();
        }

        /// <summary>
        /// The node that this LinkTo leads to
        /// </summary>
        public RoomNode TargetNode { get; }

        /// <summary>
        /// The strats that can be executed to follow this LinkTo, mapped by name.
        /// </summary>
        public IReadOnlyDictionary<string, Strat> Strats { get; }
    }

    public class UnfinalizedLinkTo : AbstractUnfinalizedModelElement<UnfinalizedLinkTo, LinkTo>, InitializablePostDeserializeInRoom
    {
        [JsonPropertyName("id")]
        public int TargetNodeId { get; set; }

        /// <summary>
        /// <para>Not available before <see cref="Initialize(SuperMetroidModel, UnfinalizedRoom)"/> has been called.</para>
        /// <para>The node that this link leads to</para>
        /// </summary>
        [JsonIgnore]
        public UnfinalizedRoomNode TargetNode { get; set; }

        /// <summary>
        /// The strats that can be executed to follow this LinkTo, mapped by name.
        /// </summary>
        public IDictionary<string, UnfinalizedStrat> Strats { get; set; } = new Dictionary<string, UnfinalizedStrat>();

        public UnfinalizedLinkTo()
        {

        }

        public UnfinalizedLinkTo(RawLinkTo rawLinkTo, LogicalElementCreationKnowledgeBase knowledgeBase)
        {
            TargetNodeId = rawLinkTo.Id;
            Strats = rawLinkTo.Strats.Select(rawStrat => new UnfinalizedStrat(rawStrat, knowledgeBase)).ToDictionary(strat => strat.Name);
        }

        protected override LinkTo CreateFinalizedElement(UnfinalizedLinkTo sourceElement, Action<LinkTo> mappingsInsertionCallback, ModelFinalizationMappings mappings)
        {
            return new LinkTo(sourceElement, mappingsInsertionCallback, mappings);
        }

        protected override bool ApplyLogicalOptionsEffects(ReadOnlyLogicalOptions logicalOptions)
        {
            bool noPossibleStrat = true;
            foreach (UnfinalizedStrat strat in Strats.Values)
            {
                strat.ApplyLogicalOptions(logicalOptions);
                if(!strat.UselessByLogicalOptions)
                {
                    noPossibleStrat = false;
                }
            }

            // This LinkTo is unusable if all strats are unusable
            return noPossibleStrat;
        }

        public void InitializeProperties(SuperMetroidModel model, UnfinalizedRoom room)
        {
            // Initialize TargetNode
            TargetNode = room.Nodes[TargetNodeId];

            // Initialize strats
            foreach (UnfinalizedStrat strat in Strats.Values)
            {
                strat.InitializeProperties(model, room);
            }
        }

        public IEnumerable<string> InitializeReferencedLogicalElementProperties(SuperMetroidModel model, UnfinalizedRoom room)
        {
            List<string> unhandled = new List<string>();

            foreach(UnfinalizedStrat strat in Strats.Values)
            {
                unhandled.AddRange(strat.InitializeReferencedLogicalElementProperties(model, room));
            }

            return unhandled.Distinct();
        }
    }
}
