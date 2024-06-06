using sm_json_data_framework.Models;
using sm_json_data_framework.Models.InGameStates;
using sm_json_data_framework.Models.Rooms.Nodes;
using sm_json_data_framework.Reading;
using sm_json_data_framework.Tests.TestTools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace sm_json_data_framework.Tests.Models.InGameStates
{
    public class InNodeStateTest
    {
        // Use a static model to build it only once.
        private static SuperMetroidModel Model { get; set; } = StaticTestObjects.UnmodifiableModel;

        #region Tests for ApplyOpenLock()
        [Fact]
        public void ApplyOpenLock_ById_RemembersLock()
        {
            // Given
            RoomNode node = Model.GetNodeInRoom("Bomb Torizo Room", 1);
            NodeLock openedLock = Model.Locks["Bomb Torizo Room Grey Lock (to Flyway)"];
            InNodeState state = new InNodeState(node);

            // When
            state.ApplyOpenLock("Bomb Torizo Room Grey Lock (to Flyway)");

            // Expect
            Assert.Contains(openedLock, state.OpenedLocks);
        }

        [Fact]
        public void ApplyOpenLock_ById_LockNotOnNode_ThrowsException()
        {
            // Given
            RoomNode node = Model.GetNodeInRoom("Landing Site", 5);
            InNodeState state = new InNodeState(node);

            // When and expect
            Assert.Throws<ArgumentException>(() => state.ApplyOpenLock("FakeLock"));
        }

        [Fact]
        public void ApplyOpenLock_RemembersLock()
        {
            // Given
            RoomNode node = Model.GetNodeInRoom("Bomb Torizo Room", 1);
            NodeLock openedLock = Model.Locks["Bomb Torizo Room Grey Lock (to Flyway)"];
            InNodeState state = new InNodeState(node);

            // When
            state.ApplyOpenLock(openedLock);

            // Expect
            Assert.Contains(openedLock, state.OpenedLocks);
        }

        [Fact]
        public void ApplyOpenLock_LockNotOnNode_ThrowsException()
        {
            // Given
            RoomNode node = Model.GetNodeInRoom("Landing Site", 5);
            NodeLock openedLock = Model.Locks["Bomb Torizo Room Grey Lock (to Flyway)"];
            InNodeState state = new InNodeState(node);

            // When and expect
            Assert.Throws<ArgumentException>(() => state.ApplyOpenLock(openedLock));
        }
        #endregion

        #region Tests for ApplyBypassLock()
        [Fact]
        public void ApplyBypassLock_ById_RemembersLock()
        {
            // Given
            RoomNode node = Model.GetNodeInRoom("Bomb Torizo Room", 1);
            NodeLock bypassedLock = Model.Locks["Animal Escape Grey Lock (to Flyway)"];
            InNodeState state = new InNodeState(node);

            // When
            state.ApplyBypassLock("Animal Escape Grey Lock (to Flyway)");

            // Expect
            Assert.Contains(bypassedLock, state.BypassedLocks);
        }

        [Fact]
        public void ApplyBypassLock_ById_LockNotOnNode_ThrowsException()
        {
            // Given
            RoomNode node = Model.GetNodeInRoom("Landing Site", 5);
            InNodeState state = new InNodeState(node);

            // When and expect
            Assert.Throws<ArgumentException>(() => state.ApplyBypassLock("FakeLock"));
        }

        [Fact]
        public void ApplyBypassLock_RemembersLock()
        {
            // Given
            RoomNode node = Model.GetNodeInRoom("Bomb Torizo Room", 1);
            NodeLock bypassedLock = Model.Locks["Animal Escape Grey Lock (to Flyway)"];
            InNodeState state = new InNodeState(node);

            // When
            state.ApplyBypassLock(bypassedLock);

            // Expect
            Assert.Contains(bypassedLock, state.BypassedLocks);
        }

        [Fact]
        public void ApplyBypassLock_LockNotOnNode_ThrowsException()
        {
            // Given
            RoomNode node = Model.GetNodeInRoom("Landing Site", 5);
            NodeLock bypassedLock = Model.Locks["Animal Escape Grey Lock (to Flyway)"];
            InNodeState state = new InNodeState(node);

            // When and expect
            Assert.Throws<ArgumentException>(() => state.ApplyBypassLock(bypassedLock));
        }
        #endregion

        #region Tests for Clone()
        [Fact]
        public void Clone_CopiesCorrectly()
        {
            // Given
            RoomNode node = Model.GetNodeInRoom("Bomb Torizo Room", 1);
            NodeLock openedLock = Model.Locks["Bomb Torizo Room Grey Lock (to Flyway)"];
            NodeLock bypassedLock = Model.Locks["Animal Escape Grey Lock (to Flyway)"];
            InNodeState original = new InNodeState(node);
            original.ApplyOpenLock(openedLock);
            original.ApplyBypassLock(bypassedLock);

            // When
            InNodeState clone = original.Clone();

            // Expect
            Assert.Equal(node.Name, clone.Node.Name);
            Assert.Contains(openedLock, clone.OpenedLocks);
            Assert.Contains(bypassedLock, clone.BypassedLocks);
        }

        [Fact]
        public void Clone_SeparatesState()
        {
            // Given
            RoomNode node = Model.GetNodeInRoom("Bomb Torizo Room", 1);
            InNodeState original = new InNodeState(node);

            // When
            InNodeState clone = original.Clone();

            // Subsequently given
            NodeLock openedLock = Model.Locks["Bomb Torizo Room Grey Lock (to Flyway)"];
            NodeLock bypassedLock = Model.Locks["Animal Escape Grey Lock (to Flyway)"];
            clone.ApplyOpenLock(openedLock);
            clone.ApplyBypassLock(bypassedLock);

            // Expect
            Assert.Empty(original.OpenedLocks);
            Assert.Empty(original.BypassedLocks);
        }
        #endregion
    }
}
