using sm_json_data_framework.Models;
using sm_json_data_framework.Models.InGameStates;
using sm_json_data_framework.Models.Rooms.Nodes;
using sm_json_data_framework.Reading;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace sm_json_data_framework.Tests.InGameStates
{
    public class InNodeStateTest
    {
        // Use a static model to read it only once.
        private static SuperMetroidModel Model { get; set; } = ModelReader.ReadModel();

        [Fact]
        public void ApplyOpenLock_RemembersLock()
        {
            RoomNode node = Model.GetNodeInRoom("Bomb Torizo Room", 1);
            NodeLock openedLock = Model.Locks["Bomb Torizo Room Grey Lock (to Flyway)"];
            InNodeState state = new InNodeState(node);
            state.ApplyOpenLock(openedLock);
            Assert.Contains(openedLock, state.OpenedLocks);
        }

        [Fact]
        public void ApplyOpenLock_LockNotOnNode_ThrowsException()
        {
            RoomNode node = Model.GetNodeInRoom("Landing Site", 5);
            NodeLock openedLock = Model.Locks["Bomb Torizo Room Grey Lock (to Flyway)"];
            InNodeState state = new InNodeState(node);
            Assert.Throws<ArgumentException>(() => state.ApplyOpenLock(openedLock));
        }

        [Fact]
        public void ApplyBypassLock_RemembersLock()
        {
            RoomNode node = Model.GetNodeInRoom("Bomb Torizo Room", 1);
            NodeLock bypassedLock = Model.Locks["Animal Escape Grey Lock (to Flyway)"];
            InNodeState state = new InNodeState(node);
            state.ApplyBypassLock(bypassedLock);
            Assert.Contains(bypassedLock, state.BypassedLocks);
        }

        [Fact]
        public void ApplyBypassLock_LockNotOnNode_ThrowsException()
        {
            RoomNode node = Model.GetNodeInRoom("Landing Site", 5);
            NodeLock bypassedLock = Model.Locks["Animal Escape Grey Lock (to Flyway)"];
            InNodeState state = new InNodeState(node);
            Assert.Throws<ArgumentException>(() => state.ApplyBypassLock(bypassedLock));
        }

            [Fact]
        public void Clone_CopiesCorrectly()
        {
            RoomNode node = Model.GetNodeInRoom("Bomb Torizo Room", 1);
            NodeLock openedLock = Model.Locks["Bomb Torizo Room Grey Lock (to Flyway)"];
            NodeLock bypassedLock = Model.Locks["Animal Escape Grey Lock (to Flyway)"];
            InNodeState original = new InNodeState(node);
            original.ApplyOpenLock(openedLock);
            original.ApplyBypassLock(bypassedLock);

            InNodeState clone = original.Clone();
            Assert.Equal(node.Name, clone.Node.Name);
            Assert.Contains(openedLock, clone.OpenedLocks);
            Assert.Contains(bypassedLock, clone.BypassedLocks);
        }

        [Fact]
        public void Clone_SeparatesState()
        {
            RoomNode node = Model.GetNodeInRoom("Bomb Torizo Room", 1);
            InNodeState original = new InNodeState(node);

            InNodeState clone = original.Clone();
            NodeLock openedLock = Model.Locks["Bomb Torizo Room Grey Lock (to Flyway)"];
            NodeLock bypassedLock = Model.Locks["Animal Escape Grey Lock (to Flyway)"];
            clone.ApplyOpenLock(openedLock);
            clone.ApplyBypassLock(bypassedLock);

            Assert.Empty(original.OpenedLocks);
            Assert.Empty(original.BypassedLocks);
        }
    }
}
