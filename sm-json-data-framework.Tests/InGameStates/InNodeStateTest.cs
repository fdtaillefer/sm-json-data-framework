﻿using sm_json_data_framework.Models;
using sm_json_data_framework.Models.InGameStates;
using sm_json_data_framework.Models.Rooms.Nodes;
using sm_json_data_framework.Reading;
using sm_json_data_framework.Tests.TestTools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace sm_json_data_framework.Tests.InGameStates
{
    public class InNodeStateTest
    {
        // Use a static model to build it only once.
        private static UnfinalizedSuperMetroidModel Model { get; set; } = new UnfinalizedSuperMetroidModel(StaticTestObjects.RawModel);

        #region Tests for ApplyOpenLock()
        [Fact]
        public void ApplyOpenLock_ById_RemembersLock()
        {
            // Given
            UnfinalizedRoomNode node = Model.GetNodeInRoom("Bomb Torizo Room", 1);
            UnfinalizedNodeLock openedLock = Model.Locks["Bomb Torizo Room Grey Lock (to Flyway)"];
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
            UnfinalizedRoomNode node = Model.GetNodeInRoom("Landing Site", 5);
            InNodeState state = new InNodeState(node);

            // When and expect
            Assert.Throws<ArgumentException>(() => state.ApplyOpenLock("FakeLock"));
        }

        [Fact]
        public void ApplyOpenLock_RemembersLock()
        {
            // Given
            UnfinalizedRoomNode node = Model.GetNodeInRoom("Bomb Torizo Room", 1);
            UnfinalizedNodeLock openedLock = Model.Locks["Bomb Torizo Room Grey Lock (to Flyway)"];
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
            UnfinalizedRoomNode node = Model.GetNodeInRoom("Landing Site", 5);
            UnfinalizedNodeLock openedLock = Model.Locks["Bomb Torizo Room Grey Lock (to Flyway)"];
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
            UnfinalizedRoomNode node = Model.GetNodeInRoom("Bomb Torizo Room", 1);
            UnfinalizedNodeLock bypassedLock = Model.Locks["Animal Escape Grey Lock (to Flyway)"];
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
            UnfinalizedRoomNode node = Model.GetNodeInRoom("Landing Site", 5);
            InNodeState state = new InNodeState(node);

            // When and expect
            Assert.Throws<ArgumentException>(() => state.ApplyBypassLock("FakeLock"));
        }

        [Fact]
        public void ApplyBypassLock_RemembersLock()
        {
            // Given
            UnfinalizedRoomNode node = Model.GetNodeInRoom("Bomb Torizo Room", 1);
            UnfinalizedNodeLock bypassedLock = Model.Locks["Animal Escape Grey Lock (to Flyway)"];
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
            UnfinalizedRoomNode node = Model.GetNodeInRoom("Landing Site", 5);
            UnfinalizedNodeLock bypassedLock = Model.Locks["Animal Escape Grey Lock (to Flyway)"];
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
            UnfinalizedRoomNode node = Model.GetNodeInRoom("Bomb Torizo Room", 1);
            UnfinalizedNodeLock openedLock = Model.Locks["Bomb Torizo Room Grey Lock (to Flyway)"];
            UnfinalizedNodeLock bypassedLock = Model.Locks["Animal Escape Grey Lock (to Flyway)"];
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
            UnfinalizedRoomNode node = Model.GetNodeInRoom("Bomb Torizo Room", 1);
            InNodeState original = new InNodeState(node);

            // When
            InNodeState clone = original.Clone();

            // Subsequently given
            UnfinalizedNodeLock openedLock = Model.Locks["Bomb Torizo Room Grey Lock (to Flyway)"];
            UnfinalizedNodeLock bypassedLock = Model.Locks["Animal Escape Grey Lock (to Flyway)"];
            clone.ApplyOpenLock(openedLock);
            clone.ApplyBypassLock(bypassedLock);

            // Expect
            Assert.Empty(original.OpenedLocks);
            Assert.Empty(original.BypassedLocks);
        }
        #endregion
    }
}
