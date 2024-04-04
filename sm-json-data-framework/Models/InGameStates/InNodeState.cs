using sm_json_data_framework.Models.Rooms.Nodes;
using sm_json_data_framework.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace sm_json_data_framework.Models.InGameStates
{
    /// <summary>
    /// Represents the logically-relevant parts of the state during a given visit of Samus at the current node.
    /// </summary>
    public class InNodeState : ReadOnlyInNodeState
    {
        public RoomNode Node { get; }

        public IEnumerable<NodeLock> OpenedLocks { get; protected set; } = Enumerable.Empty<NodeLock>();

        public IEnumerable<NodeLock> BypassedLocks { get; protected set; } = Enumerable.Empty<NodeLock>();


        public InNodeState(RoomNode node)
        {
            Node = node;
        }

        public InNodeState(InNodeState other)
        {
            Node = other.Node;
            OpenedLocks = new List<NodeLock>(other.OpenedLocks);
            BypassedLocks = new List<NodeLock>(other.BypassedLocks);
        }

        public InNodeState Clone()
        {
            return new InNodeState(this);
        }

        public InNodeState AsReadOnly()
        {
            return this;
        }

        /// <summary>
        /// Registers the provided NodeLock as being opened during the current node visit.
        /// </summary>
        /// <param name="nodeLock">Lock being opened</param>
        public void ApplyOpenLock(NodeLock nodeLock)
        {
            if(!Node.Locks.Contains(nodeLock, ObjectReferenceEqualityComparer<NodeLock>.Default))
            {
                throw new ArgumentException("Can't open a lock that's not on the node being visited");
            }
            OpenedLocks = OpenedLocks.Append(nodeLock);
        }

        /// <summary>
        /// Registers the provided NodeLock as being bypassed during the current node visit.
        /// </summary>
        /// <param name="nodeLock">Lock being bypassed</param>
        public void ApplyBypassLock(NodeLock nodeLock)
        {
            if (!Node.Locks.Contains(nodeLock, ObjectReferenceEqualityComparer<NodeLock>.Default))
            {
                throw new ArgumentException("Can't bypass a lock that's not on the node being visited");
            }
            BypassedLocks = BypassedLocks.Append(nodeLock);
        }
    }

    /// <summary>
    /// Exposes the read-only portion of an <see cref="InNodeState"/>.
    /// </summary>
    public interface ReadOnlyInNodeState
    {
        /// <summary>
        /// The node at which this InNodeState describes the situation.
        /// </summary>
        public RoomNode Node { get; }

        /// <summary>
        /// An enumeration of locks that were opened by Samus during this visit.
        /// </summary>
        public IEnumerable<NodeLock> OpenedLocks { get; }

        /// <summary>
        /// An enumeration of locks that were bypassed by Samus during this visit.
        /// </summary>
        public IEnumerable<NodeLock> BypassedLocks { get; }

        /// <summary>
        /// Creates and returns a copy of this InNodeState, as a full-fledged modifiable one.
        /// </summary>
        /// <returns>The clone</returns>
        public InNodeState Clone();
    }
}
