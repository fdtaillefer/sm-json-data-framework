using sm_json_data_framework.Models.Raw.Rooms.Nodes;
using sm_json_data_framework.Options;
using sm_json_data_framework.Rules;
using sm_json_data_framework.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;

namespace sm_json_data_framework.Models.Rooms.Nodes
{
    /// <summary>
    /// Describes the environment at a specific door. 
    /// This excludes any property that is always true for an entire room (see <see cref="RoomEnvironment"/> for that).
    /// </summary>
    public class DoorEnvironment : AbstractModelElement<UnfinalizedDoorEnvironment, DoorEnvironment>
    {
        private UnfinalizedDoorEnvironment InnerElement { get; set; }

        public DoorEnvironment(UnfinalizedDoorEnvironment innerElement, Action<DoorEnvironment> mappingsInsertionCallback, ModelFinalizationMappings mappings)
            : base(innerElement, mappingsInsertionCallback)
        {
            InnerElement = innerElement;
            EntranceNodes = InnerElement.EntranceNodes?.Select(node => node.Finalize(mappings)).ToDictionary(node => node.Id).AsReadOnly();
            Node = InnerElement.Node.Finalize(mappings);
        }

        /// <summary>
        /// The physics that are in effect in this DoorEnvironment
        /// </summary>
        public PhysicsEnum Physics => InnerElement.Physics;

        public IReadOnlySet<int> EntranceNodeIds { get; }

        /// <summary>
        /// The nodes that enable this environment if Samus has entered the room from one of them, mapped by in-room ID. Or, if null, the environment is always applicable.
        /// </summary>
        public IReadOnlyDictionary<int, RoomNode> EntranceNodes { get; }

        /// <summary>
        /// The RoomNode on which this environment is.
        /// </summary>
        public RoomNode Node { get; }

        protected override void PropagateLogicalOptions(ReadOnlyLogicalOptions logicalOptions)
        {
            // Nothing to do here
        }

        public override bool CalculateLogicallyRelevant()
        {
            // There's nothing that can make a door environment irrelevant
            return true;
        }
    }

    public class UnfinalizedDoorEnvironment : AbstractUnfinalizedModelElement<UnfinalizedDoorEnvironment, DoorEnvironment>, InitializablePostDeserializeInNode
    {
        public PhysicsEnum Physics { get; set; }

        public ISet<int> EntranceNodeIds { get; set; }

        /// <summary>
        /// <para>Not available before <see cref="Initialize(UnfinalizedSuperMetroidModel, UnfinalizedRoom, UnfinalizedRoomNode)"/> has been called.</para>
        /// <para>The nodes that Samus must have entered from for this environment to be applicable. Or, if null, the environment is always applicable.</para>
        /// </summary>
        public IList<UnfinalizedRoomNode> EntranceNodes { get; set; }

        /// <summary>
        /// <para>Not available before <see cref="Initialize(UnfinalizedSuperMetroidModel, UnfinalizedRoom, UnfinalizedRoomNode)"/> has been called.</para>
        /// <para>The RoomNode on which this environment is.</para>
        /// </summary>
        public UnfinalizedRoomNode Node { get; set; }

        public UnfinalizedDoorEnvironment()
        {
            
        }

        public UnfinalizedDoorEnvironment(RawDoorEnvironment rawEnvironment)
        {
            Physics = rawEnvironment.Physics;
            if(rawEnvironment.EntranceNodes != null)
            {
                EntranceNodeIds = new HashSet<int>(rawEnvironment.EntranceNodes);
            }
        }

        protected override DoorEnvironment CreateFinalizedElement(UnfinalizedDoorEnvironment sourceElement, Action<DoorEnvironment> mappingsInsertionCallback, ModelFinalizationMappings mappings)
        {
            return new DoorEnvironment(sourceElement, mappingsInsertionCallback, mappings);
        }

        public void InitializeProperties(UnfinalizedSuperMetroidModel model, UnfinalizedRoom room, UnfinalizedRoomNode node)
        {
            Node = node;

            // Initialize list of EntranceNodes. This also serves as a sanity check and will throw if an ID is invalid.
            if (EntranceNodeIds != null)
            {
                List<UnfinalizedRoomNode> entranceNodes = new List<UnfinalizedRoomNode>();
                foreach (int entranceNodeId in EntranceNodeIds)
                {
                    room.Nodes.TryGetValue(entranceNodeId, out UnfinalizedRoomNode entranceNode);
                    if (entranceNode == null)
                    {
                        throw new Exception($"A DoorEnvironment's entranceNode ID {entranceNodeId} not found in room '{room.Name}' (the environment was on node {node.Id}).");
                    }
                    entranceNodes.Add(entranceNode);
                }
                EntranceNodes = entranceNodes;
            }
        }

        public IEnumerable<string> InitializeReferencedLogicalElementProperties(UnfinalizedSuperMetroidModel model, UnfinalizedRoom room, UnfinalizedRoomNode node)
        {
            // No logical element in a door environment
            return Enumerable.Empty<string>();
        }
    }
}
