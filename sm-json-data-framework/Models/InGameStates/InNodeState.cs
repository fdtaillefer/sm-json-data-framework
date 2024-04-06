using sm_json_data_framework.Models.Rooms.Nodes;
using sm_json_data_framework.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace sm_json_data_framework.Models.InGameStates
{
    /// <summary>
    /// Represents the logically-relevant parts of the state during a given visit of Samus at a given node.
    /// An instance of this class is not intended to be moved from node to node, but should be replaced instead.
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
        /// Registers the node with the provided name on the current node as being opened during the current node visit.
        /// </summary>
        /// <param name="lockName">Name of the lock being opened</param>
        public void ApplyOpenLock(string lockName)
        {
            if (!Node.Locks.TryGetValue(lockName, out NodeLock foundLock))
            {
                throw new ArgumentException($"There is no lock named {lockName} on node {Node.Id} of room '{Node.Room.Name}'");
            }
            ApplyOpenLockSafe(foundLock);
        }

        /// <summary>
        /// Registers the provided NodeLock as being opened during the current node visit.
        /// </summary>
        /// <param name="nodeLock">Lock being opened</param>
        public void ApplyOpenLock(NodeLock nodeLock)
        {
            Node.Locks.TryGetValue(nodeLock.Name, out NodeLock foundLock);
            if(foundLock != nodeLock)
            {
                throw new ArgumentException("Can't open a lock that's not on the node being visited");
            }
            ApplyOpenLockSafe(nodeLock);
        }

        /// <summary>
        /// Does the actual registering of the provided NodeLock as being opened during the current node visit.
        /// This should only be called by one of the public ApplyOpenLock() methods, after validating the operation.
        /// </summary>
        /// <param name="nodeLock">Lock being opened</param>
        protected void ApplyOpenLockSafe(NodeLock nodeLock)
        {
            OpenedLocks = OpenedLocks.Append(nodeLock);
        }

        /// <summary>
        /// Registers the node with the provided name on the current node as being bypassed during the current node visit.
        /// </summary>
        /// <param name="lockName">Name of the lock being bypassed</param>
        public void ApplyBypassLock(string lockName)
        {
            if (!Node.Locks.TryGetValue(lockName, out NodeLock foundLock))
            {
                throw new ArgumentException($"There is no lock named {lockName} on node {Node.Id} of room '{Node.Room.Name}'");
            }
            ApplyBypassLockSafe(foundLock);
        }

        /// <summary>
        /// Registers the provided NodeLock as being bypassed during the current node visit.
        /// </summary>
        /// <param name="nodeLock">Lock being bypassed</param>
        public void ApplyBypassLock(NodeLock nodeLock)
        {
            Node.Locks.TryGetValue(nodeLock.Name, out NodeLock foundLock);
            if (foundLock != nodeLock)
            { 
                throw new ArgumentException("Can't bypass a lock that's not on the node being visited");
            }
            ApplyBypassLockSafe(nodeLock);
        }

        /// <summary>
        /// Does the actual registering of the provided NodeLock as being bypassed during the current node visit.
        /// This should only be called by one of the public ApplyBypassLock() methods, after validating the operation.
        /// </summary>
        /// <param name="nodeLock">Lock being bypassed</param>
        protected void ApplyBypassLockSafe(NodeLock nodeLock)
        {
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
