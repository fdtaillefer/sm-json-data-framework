﻿using sm_json_data_framework.Models.Rooms;
using sm_json_data_framework.Models.Rooms.Nodes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.AccessControl;
using System.Text;
using System.Text.Json.Serialization;

namespace sm_json_data_framework.Models.Requirements.ObjectRequirements.Integers
{
    /// <summary>
    /// An abstract object logical element with a int value that is interepreted as a node ID.
    /// </summary>
    /// <typeparam name="SourceType">The unfinalized type that finalizes into this type</typeparam>
    /// <typeparam name="ConcreteType">The self-type of the concrete sub-type</typeparam>
    public abstract class AbstractObjectLogicalElementWithNodeId<SourceType, ConcreteType> : AbstractObjectLogicalElementWithInteger<SourceType, ConcreteType>
        where SourceType : AbstractUnfinalizedObjectLogicalElementWithNodeId<SourceType, ConcreteType>
        where ConcreteType : AbstractObjectLogicalElementWithNodeId<SourceType, ConcreteType>
    {
        public AbstractObjectLogicalElementWithNodeId(SourceType sourceElement, Action<ConcreteType> mappingsInsertionCallback, ModelFinalizationMappings mappings)
            : base(sourceElement, mappingsInsertionCallback)
        {
            Node = sourceElement.Node.Finalize(mappings);
        }

        public RoomNode Node { get; }
    }

    public abstract class AbstractUnfinalizedObjectLogicalElementWithNodeId<ConcreteType, TargetType> : AbstractUnfinalizedObjectLogicalElementWithInteger<ConcreteType, TargetType>
        where ConcreteType : AbstractUnfinalizedObjectLogicalElementWithNodeId<ConcreteType, TargetType>
        where TargetType : AbstractObjectLogicalElementWithNodeId<ConcreteType, TargetType>
    {
        public AbstractUnfinalizedObjectLogicalElementWithNodeId()
        {

        }

        public AbstractUnfinalizedObjectLogicalElementWithNodeId(int id) : base(id)
        {

        }

        /// <summary>
        /// <para>Only available after a call to <see cref="InitializeReferencedLogicalElementProperties(UnfinalizedSuperMetroidModel, UnfinalizedRoom)"/>.</para>
        /// <para>The node that this element's value references. </para>
        /// </summary>
        public UnfinalizedRoomNode Node {get;set;}

        public override IEnumerable<string> InitializeReferencedLogicalElementProperties(UnfinalizedSuperMetroidModel model, UnfinalizedRoom room)
        {
            if (room.Nodes.TryGetValue(Value, out UnfinalizedRoomNode node))
            {
                Node = node;
                return Enumerable.Empty<string>();
            }
            else
            {
                return new[] { $"Node {Value} in room {room.Name}" };
            }
        }
    }
}
