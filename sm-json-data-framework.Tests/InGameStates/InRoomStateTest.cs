using sm_json_data_framework.Models;
using sm_json_data_framework.Models.InGameStates;
using sm_json_data_framework.Models.Rooms;
using sm_json_data_framework.Models.Rooms.Nodes;
using sm_json_data_framework.Reading;
using sm_json_data_framework.Tests.TestTools;
using sm_json_data_framework.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace sm_json_data_framework.Tests.InGameStates
{
    public class InRoomStateTest
    {
        // Use a static model to build it only once.
        private static SuperMetroidModel Model { get; set; } = new SuperMetroidModel(StaticTestObjects.RawModel);

        #region Tests for ctor(RoomNode)
        [Fact]
        public void ConstructorWithNode_NoSpawnAt_InitializesCorrectly()
        {
            // Given
            UnfinalizedRoomNode initialNode = Model.GetNodeInRoom("Parlor and Alcatraz", 4);

            // When
            InRoomState state = new InRoomState(initialNode);

            // Expect
            Assert.Equal("Parlor and Alcatraz", state.CurrentRoom.Name);
            Assert.Equal(4, state.CurrentNode.Id);
            Assert.Null(state.LastStrat);
            Assert.Single(state.VisitedRoomPath);
            Assert.Equal(4, state.VisitedRoomPath[0].nodeState.Node.Id);
            Assert.Null(state.VisitedRoomPath[0].strat);
        }

        [Fact]
        public void ConstructorWithNode_NodeHasSpawnAt_InitializesCorrectly()
        {
            // Given
            UnfinalizedRoomNode initialNode = Model.GetNodeInRoom("Crocomire's Room", 2);

            // When
            InRoomState state = new InRoomState(initialNode);

            // Expect
            Assert.Equal("Crocomire's Room", state.CurrentRoom.Name);
            Assert.Null(state.LastStrat);
            // Node 2 in Crocomire's Room has spawnAt 5
            Assert.Equal(5, state.CurrentNode.Id);
            Assert.Equal(2, state.VisitedRoomPath.Count);

            Assert.Equal(2, state.VisitedRoomPath[0].nodeState.Node.Id);
            Assert.Null(state.VisitedRoomPath[0].strat);

            Assert.Equal(5, state.VisitedRoomPath[1].nodeState.Node.Id);
            Assert.Null(state.VisitedRoomPath[1].strat);
        }
        #endregion

        #region Tests for GetLinkToNode()
        [Fact]
        public void GetLinkToNode_LinkExists_ReturnsLink()
        {
            // Given
            UnfinalizedRoomNode initialNode = Model.GetNodeInRoom("Parlor and Alcatraz", 4);
            UnfinalizedLinkTo expectedLink = initialNode.Links[8];
            InRoomState state = new InRoomState(initialNode);

            // When
            UnfinalizedLinkTo result = state.GetLinkToNode(8);

            // Expect
            Assert.Same(expectedLink, result);
        }

        [Fact]
        public void GetLinkToNode_LinkDoesntExist_ReturnsNull()
        {
            // Given
            UnfinalizedRoomNode initialNode = Model.GetNodeInRoom("Parlor and Alcatraz", 4);
            InRoomState state = new InRoomState(initialNode);

            // When
            UnfinalizedLinkTo result = state.GetLinkToNode(88);

            // Expect
            Assert.Null(result);
        }
        #endregion

        #region Tests for GetStratToNode()
        [Fact]
        public void GetStratToNode_LinkAndStratExist_ReturnsStrat()
        {
            // Given
            UnfinalizedRoomNode initialNode = Model.GetNodeInRoom("Parlor and Alcatraz", 4);
            UnfinalizedStrat expectedStrat = initialNode.Links[8].Strats["Base"];
            InRoomState state = new InRoomState(initialNode);

            // When
            UnfinalizedStrat result = state.GetStratToNode(8, "Base");

            // Expect
            Assert.Same(expectedStrat, result);
        }

        [Fact]
        public void GetStratToNode_LinkExistsButNotStrat_ReturnsNull()
        {
            // Given
            UnfinalizedRoomNode initialNode = Model.GetNodeInRoom("Parlor and Alcatraz", 4);
            InRoomState state = new InRoomState(initialNode);

            // When
            UnfinalizedStrat result = state.GetStratToNode(8, "BLECH");

            // Expect
            Assert.Null(result);
        }

        [Fact]
        public void GetStratToNode_LinkDoesntExist_ReturnsNull()
        {
            // Given
            UnfinalizedRoomNode initialNode = Model.GetNodeInRoom("Parlor and Alcatraz", 4);
            InRoomState state = new InRoomState(initialNode);

            // When
            UnfinalizedStrat result = state.GetStratToNode(88, "Base");

            // Expect
            Assert.Null(result);
        }
        #endregion

        #region Tests for ApplyVisitNode(int, string)
        [Fact]
        public void ApplyVisitNode_ById_AccumulatesVisitedNodesAndStrats()
        {
            // Given
            UnfinalizedRoomNode initialNode = Model.GetNodeInRoom("Parlor and Alcatraz", 4);
            InRoomState state = new InRoomState(initialNode);

            // When
            state.ApplyVisitNode(8, "Base")
                .ApplyVisitNode(1, "Parlor Quick Charge");

            // Expect
            Assert.Equal("Parlor and Alcatraz", state.CurrentRoom.Name);
            Assert.Equal(1, state.CurrentNode.Id);
            Assert.Equal("Parlor Quick Charge", state.LastStrat.Name);
            Assert.Equal(3, state.VisitedRoomPath.Count);

            Assert.Equal(4, state.VisitedRoomPath[0].nodeState.Node.Id);
            Assert.Null(state.VisitedRoomPath[0].strat);

            Assert.Equal(8, state.VisitedRoomPath[1].nodeState.Node.Id);
            Assert.Equal("Base", state.VisitedRoomPath[1].strat.Name);

            Assert.Equal(1, state.VisitedRoomPath[2].nodeState.Node.Id);
            Assert.Equal("Parlor Quick Charge", state.VisitedRoomPath[2].strat.Name);
        }

        [Fact]
        public void ApplyVisitNode_ById_NotSpawningInRoom_RejectsNullStrat()
        {
            // Given
            UnfinalizedRoomNode initialNode = Model.GetNodeInRoom("Parlor and Alcatraz", 4);
            InRoomState state = new InRoomState(initialNode);

            // When and expect
            Assert.Throws<ArgumentException>(() => state.ApplyVisitNode(8, null));
        }

        [Fact]
        public void ApplyVisitNode_ById_LinkDoesntExist_ThrowsException()
        {
            // Given
            UnfinalizedRoomNode initialNode = Model.GetNodeInRoom("Parlor and Alcatraz", 4);
            InRoomState state = new InRoomState(initialNode);

            // When and expect
            Assert.Throws<ArgumentException>(() => state.ApplyVisitNode(1, "Base"));
        }

        [Fact]
        public void ApplyVisitNode_ById_StratNotOnOriginLink_ThrowsException()
        {
            // Given
            UnfinalizedRoomNode initialNode = Model.GetNodeInRoom("Parlor and Alcatraz", 4);
            InRoomState state = new InRoomState(initialNode);

            // When and expect
            Assert.Throws<ArgumentException>(() => state.ApplyVisitNode(8, "wrongStrat"));
        }

        [Theory]
        [InlineData("stratName")]
        [InlineData(null)]
        public void ApplyVisitNode_ById_NoCurrentRoom_ThrowsException(string stratName)
        {
            // Given
            InRoomState state = new InRoomState(Model.GetNodeInRoom("Landing Site", 5));
            state.ClearRoomState();

            // When and expect
            Assert.Throws<InvalidOperationException>(() => state.ApplyVisitNode(8, stratName));
        }

        [Fact]
        public void ApplyVisitNode_ById_OngoingSpawnAt_GoesToCorrectNodeWithNoStrat_AccumulatesVisitedNodesAndStrats()
        {
            // Given
            UnfinalizedRoomNode firstNode = Model.GetNodeInRoom("Crocomire's Room", 2);
            UnfinalizedRoomNode secondNode = Model.GetNodeInRoom("Crocomire's Room", 5);
            InRoomState state = new InRoomState(firstNode);
            state.ClearRoomState();
            state.ApplyVisitNode(firstNode, null);

            // When
            state.ApplyVisitNode(5, null);

            // Expect
            Assert.Same(secondNode, state.CurrentNode);
            Assert.Same(secondNode, state.VisitedRoomPath[1].nodeState.Node);
            Assert.Null(state.LastStrat);
            Assert.Null(state.VisitedRoomPath[1].strat);
            Assert.Equal(2, state.VisitedRoomPath.Count);
        }

        [Fact]
        public void ApplyVisitNode_ById_OngoingSpawnAt_GoesToDifferentNode_ThrowsException()
        {
            // Given
            UnfinalizedRoomNode firstNode = Model.GetNodeInRoom("Crocomire's Room", 2);
            UnfinalizedRoomNode secondNode = Model.GetNodeInRoom("Crocomire's Room", 4);
            InRoomState state = new InRoomState(firstNode);
            state.ClearRoomState();
            state.ApplyVisitNode(firstNode, null);

            // When and expect
            Assert.Throws<ArgumentException>(() => state.ApplyVisitNode(4, null));
        }

        [Fact]
        public void ApplyVisitNode_ById_OngoingSpawnAt_IncludesStrat_ThrowsException()
        {
            // Given
            UnfinalizedRoomNode firstNode = Model.GetNodeInRoom("Crocomire's Room", 2);
            InRoomState state = new InRoomState(firstNode);
            state.ClearRoomState();
            state.ApplyVisitNode(firstNode, null);

            // When and expect
            Assert.Throws<ArgumentException>(() => state.ApplyVisitNode(5, "Base"));
        }
        #endregion

        #region Tests for ApplyVisitNode(RoomNode, Strat)
        [Fact]
        public void ApplyVisitNode_AccumulatesVisitedNodesAndStrats()
        {
            // Given
            UnfinalizedRoomNode initialNode = Model.GetNodeInRoom("Parlor and Alcatraz", 4);
            InRoomState state = new InRoomState(initialNode);

            // When
            state.ApplyVisitNode(Model.GetNodeInRoom("Parlor and Alcatraz", 8), state.CurrentNode.Links[8].Strats["Base"])
                .ApplyVisitNode(Model.GetNodeInRoom("Parlor and Alcatraz", 1), state.CurrentNode.Links[1].Strats["Parlor Quick Charge"]);

            // Expect
            Assert.Equal("Parlor and Alcatraz", state.CurrentRoom.Name);
            Assert.Equal(1, state.CurrentNode.Id);
            Assert.Equal("Parlor Quick Charge", state.LastStrat.Name);
            Assert.Equal(3, state.VisitedRoomPath.Count);

            Assert.Equal(4, state.VisitedRoomPath[0].nodeState.Node.Id);
            Assert.Null(state.VisitedRoomPath[0].strat);

            Assert.Equal(8, state.VisitedRoomPath[1].nodeState.Node.Id);
            Assert.Equal("Base", state.VisitedRoomPath[1].strat.Name);

            Assert.Equal(1, state.VisitedRoomPath[2].nodeState.Node.Id);
            Assert.Equal("Parlor Quick Charge", state.VisitedRoomPath[2].strat.Name);
        }

        [Fact]
        public void ApplyVisitNode_NotSpawningInRoom_RejectsNullStrat()
        {
            // Given
            UnfinalizedRoomNode initialNode = Model.GetNodeInRoom("Parlor and Alcatraz", 4);
            InRoomState state = new InRoomState(initialNode);

            // When and expect
            Assert.Throws<ArgumentException>(() => state.ApplyVisitNode(Model.GetNodeInRoom("Parlor and Alcatraz", 8), null));
        }

        [Fact]
        public void ApplyVisitNode_LinkDoesntExist_ThrowsException()
        {
            // Given
            UnfinalizedRoomNode initialNode = Model.GetNodeInRoom("Parlor and Alcatraz", 4);
            InRoomState state = new InRoomState(initialNode);

            // When and expect
            Assert.Throws<ArgumentException>(() => state.ApplyVisitNode(Model.GetNodeInRoom("Parlor and Alcatraz", 1), state.CurrentNode.Links[8].Strats["Base"]));
        }

        [Fact]
        public void ApplyVisitNode_StratNotOnOriginLink_ThrowsException()
        {
            // Given
            UnfinalizedRoomNode initialNode = Model.GetNodeInRoom("Parlor and Alcatraz", 4);
            InRoomState state = new InRoomState(initialNode);
            UnfinalizedStrat wrongStrat = Model.GetNodeInRoom("Parlor and Alcatraz", 8).Links[1].Strats["Base"];

            // When and expect
            Assert.Throws<ArgumentException>(() => state.ApplyVisitNode(Model.GetNodeInRoom("Parlor and Alcatraz", 8), wrongStrat));
        }

        [Fact]
        public void ApplyVisitNode_NoCurrentRoom_IncludesStrat_ThrowsException()
        {
            // Given
            UnfinalizedRoomNode node = Model.GetNodeInRoom("Landing Site", 5);
            UnfinalizedStrat strat = Model.GetNodeInRoom("Landing Site", 2).Links[5].Strats["Base"];
            InRoomState state = new InRoomState(node);
            state.ClearRoomState();

            // When and expect
            Assert.Throws<ArgumentException>(() => state.ApplyVisitNode(node, strat));
        }

        [Fact]
        public void ApplyVisitNode_NoCurrentRoom_IncludesNoStrat_AccumulatesVisitedNodesAndStrats()
        {
            // Given
            UnfinalizedRoomNode node = Model.GetNodeInRoom("Landing Site", 5);
            InRoomState state = new InRoomState(node);
            state.ClearRoomState();

            // When
            state.ApplyVisitNode(node, null);

            // Expect
            Assert.Same(node, state.CurrentNode);
            Assert.Same(node, state.VisitedRoomPath[0].nodeState.Node);
            Assert.Null(state.LastStrat);
            Assert.Null(state.VisitedRoomPath[0].strat);
            Assert.Single(state.VisitedRoomPath);
        }

        [Fact]
        public void ApplyVisitNode_OngoingSpawnAt_GoesToCorrectNodeWithNoStrat_AccumulatesVisitedNodesAndStrats()
        {
            // Given
            UnfinalizedRoomNode firstNode = Model.GetNodeInRoom("Crocomire's Room", 2);
            UnfinalizedRoomNode secondNode = Model.GetNodeInRoom("Crocomire's Room", 5);
            InRoomState state = new InRoomState(firstNode);
            state.ClearRoomState();
            state.ApplyVisitNode(firstNode, null);

            // When
            state.ApplyVisitNode(secondNode, null);

            // Expect
            Assert.Same(secondNode, state.CurrentNode);
            Assert.Same(secondNode, state.VisitedRoomPath[1].nodeState.Node);
            Assert.Null(state.LastStrat);
            Assert.Null(state.VisitedRoomPath[1].strat);
            Assert.Equal(2, state.VisitedRoomPath.Count);
        }

        [Fact]
        public void ApplyVisitNode_OngoingSpawnAt_GoesToDifferentNode_ThrowsException()
        {
            // Given
            UnfinalizedRoomNode firstNode = Model.GetNodeInRoom("Crocomire's Room", 2);
            UnfinalizedRoomNode secondNode = Model.GetNodeInRoom("Crocomire's Room", 4);
            InRoomState state = new InRoomState(firstNode);
            state.ClearRoomState();
            state.ApplyVisitNode(firstNode, null);

            // When and expect
            Assert.Throws<ArgumentException>(() => state.ApplyVisitNode(secondNode, null));
        }

        [Fact]
        public void ApplyVisitNode_OngoingSpawnAt_IncludesStrat_ThrowsException()
        {
            // Given
            UnfinalizedRoomNode firstNode = Model.GetNodeInRoom("Crocomire's Room", 2);
            UnfinalizedRoomNode secondNode = Model.GetNodeInRoom("Crocomire's Room", 5);
            UnfinalizedStrat strat = Model.GetNodeInRoom("Crocomire's Room", 2).Links[5].Strats["Base"];
            InRoomState state = new InRoomState(firstNode);
            state.ClearRoomState();
            state.ApplyVisitNode(firstNode, null);

            // When and expect
            Assert.Throws<ArgumentException>(() => state.ApplyVisitNode(secondNode, strat));
        }
        #endregion

        #region Tests for ApplyDestroyObstacle(string)
        [Fact]
        public void ApplyDestroyObstacle_ById_RegistersObstacle()
        {
            // Given
            UnfinalizedRoomNode initialNode = Model.GetNodeInRoom("Landing Site", 5);
            InRoomState state = new InRoomState(initialNode);

            // When
            state.ApplyDestroyObstacle("A");

            // Expect
            Assert.Single(state.DestroyedObstacleIds);
            Assert.Contains("A", state.DestroyedObstacleIds);
        }

        [Fact]
        public void ApplyDestroyObstacle_ById_ObstacleNotInRoom_ThrowsException()
        {
            // Given
            UnfinalizedRoomNode initialNode = Model.GetNodeInRoom("Parlor and Alcatraz", 4);
            InRoomState state = new InRoomState(initialNode);

            // When and expect
            Assert.Throws<ArgumentException>(() => state.ApplyDestroyObstacle("Z"));
        }

        [Fact]
        public void ApplyDestroyObstacle_ById_NoCurrentRoom_ThrowsException()
        {
            // Given
            UnfinalizedRoomNode initialNode = Model.GetNodeInRoom("Parlor and Alcatraz", 4);
            InRoomState state = new InRoomState(initialNode);
            state.ClearRoomState();

            // When and expect
            Assert.Throws<InvalidOperationException>(() => state.ApplyDestroyObstacle("Z"));
        }
        #endregion

        #region Tests for ApplyDestroyObstacle(RoomObstacle)
        [Fact]
        public void ApplyDestroyObstacle_RegistersObstacle()
        {
            // Given
            UnfinalizedRoomNode initialNode = Model.GetNodeInRoom("Landing Site", 5);
            InRoomState state = new InRoomState(initialNode);

            // When
            state.ApplyDestroyObstacle(state.CurrentRoom.Obstacles["A"]);

            // Expect
            Assert.Single(state.DestroyedObstacleIds);
            Assert.Contains("A", state.DestroyedObstacleIds);
        }

        [Fact]
        public void ApplyDestroyObstacle_ObstacleNotInRoom_ThrowsException()
        {
            // Given
            UnfinalizedRoomNode initialNode = Model.GetNodeInRoom("Parlor and Alcatraz", 4);
            InRoomState state = new InRoomState(initialNode);
            
            // When and expect
            Assert.Throws<ArgumentException>(() => state.ApplyDestroyObstacle(Model.Rooms["Landing Site"].Obstacles["A"]));
        }
        #endregion

        #region Tests for ApplyOpenLock(string)
        [Fact]
        public void ApplyOpenLock_ById_RegistersLock()
        {
            // Given
            UnfinalizedRoomNode node = Model.GetNodeInRoom("Bomb Torizo Room", 1);
            UnfinalizedNodeLock openedLock = Model.Locks["Bomb Torizo Room Grey Lock (to Flyway)"];
            InRoomState state = new InRoomState(node);

            // Given
            state.ApplyOpenLock("Bomb Torizo Room Grey Lock (to Flyway)");

            // Expect
            Assert.Contains(openedLock, state.OpenedExitLocks, ObjectReferenceEqualityComparer<UnfinalizedNodeLock>.Default);
        }

        [Fact]
        public void ApplyOpenLock_ById_AtNoNode_ThrowsException()
        {
            // Given
            UnfinalizedRoomNode node = Model.GetNodeInRoom("Landing Site", 5);
            InRoomState state = new InRoomState(node);
            state.ClearRoomState();

            // When and expect
            Assert.Throws<InvalidOperationException>(() => state.ApplyOpenLock("FakeLock"));
        }

        [Fact]
        public void ApplyOpenLock_ById_LockDoesntExist_ThrowsException()
        {
            // Given
            UnfinalizedRoomNode node = Model.GetNodeInRoom("Landing Site", 5);
            InRoomState state = new InRoomState(node);

            // When and expect
            Assert.Throws<ArgumentException>(() => state.ApplyOpenLock("FakeLock"));
        }
        #endregion

        #region Tests for ApplyOpenLock(NodeLock)
        [Fact]
        public void ApplyOpenLock_RegistersLock()
        {
            // Given
            UnfinalizedRoomNode node = Model.GetNodeInRoom("Bomb Torizo Room", 1);
            UnfinalizedNodeLock openedLock = Model.Locks["Bomb Torizo Room Grey Lock (to Flyway)"];
            InRoomState state = new InRoomState(node);

            // When
            state.ApplyOpenLock(openedLock);

            // Expect
            Assert.Contains(openedLock, state.OpenedExitLocks, ObjectReferenceEqualityComparer<UnfinalizedNodeLock>.Default);
        }

        [Fact]
        public void ApplyOpenLock_AtNoNode_ThrowsException()
        {
            // Given
            UnfinalizedRoomNode node = Model.GetNodeInRoom("Landing Site", 5);
            UnfinalizedNodeLock openedLock = Model.Locks["Bomb Torizo Room Grey Lock (to Flyway)"];
            InRoomState state = new InRoomState(node);
            state.ClearRoomState();

            // When and expect
            Assert.Throws<InvalidOperationException>(() => state.ApplyOpenLock(openedLock));
        }

        [Fact]
        public void ApplyOpenLock_LockNotOnNode_ThrowsException()
        {
            // Given
            UnfinalizedRoomNode node = Model.GetNodeInRoom("Landing Site", 5);
            UnfinalizedNodeLock openedLock = Model.Locks["Bomb Torizo Room Grey Lock (to Flyway)"];
            InRoomState state = new InRoomState(node);

            // When and expect
            Assert.Throws<ArgumentException>(() => state.ApplyOpenLock(openedLock));
        }
        #endregion

        #region Tests for ApplyBypassLock(string)
        [Fact]
        public void ApplyBypassLock_ById_RemembersLock()
        {
            // Given
            UnfinalizedRoomNode node = Model.GetNodeInRoom("Bomb Torizo Room", 1);
            UnfinalizedNodeLock bypassedLock = Model.Locks["Animal Escape Grey Lock (to Flyway)"];
            InRoomState state = new InRoomState(node);

            // When
            state.ApplyBypassLock("Animal Escape Grey Lock (to Flyway)");

            // Expect
            Assert.Contains(bypassedLock, state.BypassedExitLocks, ObjectReferenceEqualityComparer<UnfinalizedNodeLock>.Default);
        }

        [Fact]
        public void ApplyBypassLock_ById_AtNoNode_ThrowsException()
        {
            // Given
            UnfinalizedRoomNode node = Model.GetNodeInRoom("Landing Site", 5);
            InRoomState state = new InRoomState(node);
            state.ClearRoomState();

            // When and expect
            Assert.Throws<InvalidOperationException>(() => state.ApplyBypassLock("FakeLock"));
        }

        [Fact]
        public void ApplyBypassLock_ById_LockNotOnNode_ThrowsException()
        {
            // Given
            UnfinalizedRoomNode node = Model.GetNodeInRoom("Landing Site", 5);
            InRoomState state = new InRoomState(node);

            // When and expect
            Assert.Throws<ArgumentException>(() => state.ApplyBypassLock("FakeLock"));
        }
        #endregion

        #region Tests for ApplyBypassLock(NodeLock)
        [Fact]
        public void ApplyBypassLock_RemembersLock()
        {
            // Given
            UnfinalizedRoomNode node = Model.GetNodeInRoom("Bomb Torizo Room", 1);
            UnfinalizedNodeLock bypassedLock = Model.Locks["Animal Escape Grey Lock (to Flyway)"];
            InRoomState state = new InRoomState(node);

            // When
            state.ApplyBypassLock(bypassedLock);

            // Expect
            Assert.Contains(bypassedLock, state.BypassedExitLocks, ObjectReferenceEqualityComparer<UnfinalizedNodeLock>.Default);
        }

        [Fact]
        public void ApplyBypassLock_AtNoNode_ThrowsException()
        {
            // Given
            UnfinalizedRoomNode node = Model.GetNodeInRoom("Landing Site", 5);
            UnfinalizedNodeLock bypassedLock = Model.Locks["Animal Escape Grey Lock (to Flyway)"];
            InRoomState state = new InRoomState(node);
            state.ClearRoomState();

            // When and expect
            Assert.Throws<InvalidOperationException>(() => state.ApplyBypassLock(bypassedLock));
        }

        [Fact]
        public void ApplyBypassLock_LockNotOnNode_ThrowsException()
        {
            // Given
            UnfinalizedRoomNode node = Model.GetNodeInRoom("Landing Site", 5);
            UnfinalizedNodeLock bypassedLock = Model.Locks["Animal Escape Grey Lock (to Flyway)"];
            InRoomState state = new InRoomState(node);

            // When and expect
            Assert.Throws<ArgumentException>(() => state.ApplyBypassLock(bypassedLock));
        }
        #endregion

        #region Tests for ClearRoomState()
        [Fact]
        public void ClearRoomState_ClearsEverything()
        {
            // Given
            UnfinalizedRoomNode node = Model.GetNodeInRoom("Landing Site", 4);
            InRoomState state = new InRoomState(node);
            state.ApplyDestroyObstacle("A");
            state.ApplyVisitNode(3, "Base");
            state.ApplyOpenLock("Landing Site Top Right Yellow Lock (to Power Bombs)");
            state.ApplyBypassLock("Landing Site Top Right Escape Lock (to Power Bombs)");

            // When
            state.ClearRoomState();

            // Expect
            Assert.Null(state.CurrentNode);
            Assert.Null(state.CurrentRoom);
            Assert.Null(state.CurrentNodeState);
            Assert.Empty(state.DestroyedObstacleIds);
            Assert.Empty(state.OpenedExitLocks);
            Assert.Empty(state.BypassedExitLocks);
            Assert.Null(state.LastStrat);
            Assert.Empty(state.VisitedRoomPath);
        }
        #endregion

        #region Tests for ApplyEnterRoom()
        [Fact]
        public void ApplyEnterRoom_ClearsPreviousState()
        {
            // Given
            InRoomState state = new InRoomState(Model.GetNodeInRoom("Landing Site", 4));
            state.ApplyDestroyObstacle("A");
            state.ApplyVisitNode(3, "Base");
            state.ApplyOpenLock("Landing Site Top Right Yellow Lock (to Power Bombs)");
            state.ApplyBypassLock("Landing Site Top Right Escape Lock (to Power Bombs)");
            UnfinalizedRoomNode newNode = Model.GetNodeInRoom("Parlor and Alcatraz", 4);

            // When
            state.ApplyEnterRoom(newNode);

            // Expect
            Assert.Same(newNode, state.CurrentNode);
            Assert.Same(newNode, state.VisitedRoomPath[0].nodeState.Node);
            Assert.Same(newNode.Room, state.CurrentRoom);
            Assert.Empty(state.DestroyedObstacleIds);
            Assert.Empty(state.OpenedExitLocks);
            Assert.Empty(state.BypassedExitLocks);
            Assert.Null(state.LastStrat);
            Assert.Null(state.VisitedRoomPath[0].strat);
            Assert.Single(state.VisitedRoomPath);
        }

        [Fact]
        public void ApplyEnterRoom_NoSpawnAt_InitializesCorrectly()
        {
            // Given
            InRoomState state = new InRoomState(Model.GetNodeInRoom("Landing Site", 5));

            // When
            state.ApplyEnterRoom(Model.GetNodeInRoom("Parlor and Alcatraz", 4));

            // Expect
            Assert.Equal("Parlor and Alcatraz", state.CurrentRoom.Name);
            Assert.Equal(4, state.CurrentNode.Id);
            Assert.Null(state.LastStrat);
            Assert.Single(state.VisitedRoomPath);

            Assert.Equal(4, state.VisitedRoomPath[0].nodeState.Node.Id);
            Assert.Null(state.VisitedRoomPath[0].strat);
        }

        [Fact]
        public void ApplyEnterRoom_NodeHasSpawnAt_InitializesCorrectly()
        {
            // Given
            InRoomState state = new InRoomState(Model.GetNodeInRoom("Landing Site", 5));

            // When
            state.ApplyEnterRoom(Model.GetNodeInRoom("Crocomire's Room", 2));

            // Expect
            Assert.Equal("Crocomire's Room", state.CurrentRoom.Name);
            Assert.Null(state.LastStrat);
            // Node 2 in Crocomire's Room has spawnAt 5
            Assert.Equal(5, state.CurrentNode.Id);
            Assert.Equal(2, state.VisitedRoomPath.Count);

            Assert.Equal(2, state.VisitedRoomPath[0].nodeState.Node.Id);
            Assert.Null(state.VisitedRoomPath[0].strat);

            Assert.Equal(5, state.VisitedRoomPath[1].nodeState.Node.Id);
            Assert.Null(state.VisitedRoomPath[1].strat);
        }
        #endregion

        #region Tests for Clone()
        [Fact]
        public void Clone_CopiesCorrectly()
        {
            // Given
            UnfinalizedRoomNode node = Model.GetNodeInRoom("Landing Site", 4);
            UnfinalizedRoomNode secondNode = Model.GetNodeInRoom("Landing Site", 3);
            UnfinalizedStrat strat = node.Links[3].Strats["Base"];
            UnfinalizedNodeLock openedLock = Model.Locks["Landing Site Top Right Yellow Lock (to Power Bombs)"];
            UnfinalizedNodeLock bypassedLock  = Model.Locks["Landing Site Top Right Escape Lock (to Power Bombs)"];
            InRoomState state = new InRoomState(node);
            state.ApplyDestroyObstacle("A");
            state.ApplyVisitNode(secondNode, strat);
            state.ApplyOpenLock(openedLock);
            state.ApplyBypassLock(bypassedLock);

            // When
            InRoomState clone = state.Clone();
            
            // Expect
            Assert.Same(secondNode, clone.CurrentNode);
            Assert.Same(secondNode.Room, clone.CurrentRoom);
            Assert.Same(strat, clone.LastStrat);

            Assert.Equal(2, clone.VisitedRoomPath.Count);
            Assert.Same(node, clone.VisitedRoomPath[0].nodeState.Node);
            Assert.Null(clone.VisitedRoomPath[0].strat);
            Assert.Same(secondNode, clone.VisitedRoomPath[1].nodeState.Node);
            Assert.Same(strat, clone.VisitedRoomPath[1].strat);

            Assert.Single(clone.OpenedExitLocks);
            Assert.Contains(openedLock, clone.OpenedExitLocks, ObjectReferenceEqualityComparer<UnfinalizedNodeLock>.Default);
            Assert.Single(clone.BypassedExitLocks);
            Assert.Contains(bypassedLock, clone.BypassedExitLocks, ObjectReferenceEqualityComparer<UnfinalizedNodeLock>.Default);
            Assert.Single(clone.DestroyedObstacleIds);
            Assert.Contains("A", clone.DestroyedObstacleIds);
        }

        [Fact]
        public void Clone_SeparatesState()
        {
            // Given
            UnfinalizedRoomNode node = Model.GetNodeInRoom("Landing Site", 4);
            InRoomState state = new InRoomState(node);

            // When
            InRoomState clone = state.Clone();

            // Subsequently given
            clone.ApplyDestroyObstacle("A");
            clone.ApplyVisitNode(3, "Base");
            clone.ApplyOpenLock("Landing Site Top Right Yellow Lock (to Power Bombs)");
            clone.ApplyBypassLock("Landing Site Top Right Escape Lock (to Power Bombs)");

            // Expect
            Assert.Same(node, state.CurrentNode);
            Assert.Null(state.LastStrat);

            Assert.Single(state.VisitedRoomPath);
            Assert.Same(node, state.VisitedRoomPath[0].nodeState.Node);
            Assert.Null(state.VisitedRoomPath[0].strat);

            Assert.Empty(state.OpenedExitLocks);
            Assert.Empty(state.BypassedExitLocks);
            Assert.Empty(state.DestroyedObstacleIds);
        }
        #endregion
    }
}
