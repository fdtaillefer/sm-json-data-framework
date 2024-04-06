﻿using sm_json_data_framework.Models;
using sm_json_data_framework.Models.InGameStates;
using sm_json_data_framework.Models.Rooms;
using sm_json_data_framework.Models.Rooms.Nodes;
using sm_json_data_framework.Reading;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace sm_json_data_framework.Tests.InGameStates
{
    public class InRoomStateTest
    {
        // Use a static model to read it only once.
        private static SuperMetroidModel Model { get; set; } = ModelReader.ReadModel();

        [Fact]
        public void ConstructorWithNode_NoSpawnAt_InitializesCorrectly()
        {
            RoomNode initialNode = Model.GetNodeInRoom("Parlor and Alcatraz", 4);
            InRoomState state = new InRoomState(initialNode);

            Assert.Equal("Parlor and Alcatraz", state.CurrentRoom.Name);
            Assert.Equal(4, state.CurrentNode.Id);
            Assert.Null(state.LastStrat);
            Assert.Single(state.VisitedRoomPath);

            Assert.Equal(4, state.VisitedRoomPath.First().nodeState.Node.Id);
            Assert.Null(state.VisitedRoomPath.First().strat);
        }

        [Fact]
        public void ConstructorWithNode_NodeHasSpawnAt_InitializesCorrectly()
        {
            RoomNode initialNode = Model.GetNodeInRoom("Crocomire's Room", 2);
            InRoomState state = new InRoomState(initialNode);

            Assert.Equal("Crocomire's Room", state.CurrentRoom.Name);
            Assert.Null(state.LastStrat);
            // Node 2 in Crocomire's Room has spawnAt 5
            Assert.Equal(5, state.CurrentNode.Id);
            Assert.Equal(2, state.VisitedRoomPath.Count());

            Assert.Equal(2, state.VisitedRoomPath.First().nodeState.Node.Id);
            Assert.Null(state.VisitedRoomPath.First().strat);

            Assert.Equal(5, state.VisitedRoomPath.Skip(1).First().nodeState.Node.Id);
            Assert.Null(state.VisitedRoomPath.Skip(1).First().strat);
        }

        [Fact]
        public void ApplyVisitNode_ById_AccumulatesVisitedNodesAndStrats()
        {
            RoomNode initialNode = Model.GetNodeInRoom("Parlor and Alcatraz", 4);
            InRoomState state = new InRoomState(initialNode);
            state.ApplyVisitNode(8, "Base");
            state.ApplyVisitNode(1, "Parlor Quick Charge");

            Assert.Equal("Parlor and Alcatraz", state.CurrentRoom.Name);
            Assert.Equal(1, state.CurrentNode.Id);
            Assert.Equal("Parlor Quick Charge", state.LastStrat.Name);
            Assert.Equal(3, state.VisitedRoomPath.Count());

            Assert.Equal(4, state.VisitedRoomPath.First().nodeState.Node.Id);
            Assert.Null(state.VisitedRoomPath.First().strat);

            Assert.Equal(8, state.VisitedRoomPath.Skip(1).First().nodeState.Node.Id);
            Assert.Equal("Base", state.VisitedRoomPath.Skip(1).First().strat.Name);

            Assert.Equal(1, state.VisitedRoomPath.Skip(2).First().nodeState.Node.Id);
            Assert.Equal("Parlor Quick Charge", state.VisitedRoomPath.Skip(2).First().strat.Name);
        }

        [Fact]
        public void ApplyVisitNode_ById_NotSpawningInRoom_RejectsNullStrat()
        {
            RoomNode initialNode = Model.GetNodeInRoom("Parlor and Alcatraz", 4);
            InRoomState state = new InRoomState(initialNode);
            Assert.Throws<ArgumentException>(() => state.ApplyVisitNode(8, null));
        }

        [Fact]
        public void ApplyVisitNode_ById_LinkDoesntExist_ThrowsException()
        {
            RoomNode initialNode = Model.GetNodeInRoom("Parlor and Alcatraz", 4);
            InRoomState state = new InRoomState(initialNode);
            Assert.Throws<ArgumentException>(() => state.ApplyVisitNode(1, "Base"));
        }

        [Fact]
        public void ApplyVisitNode_ById_StratNotOnOriginLink_ThrowsException()
        {
            RoomNode initialNode = Model.GetNodeInRoom("Parlor and Alcatraz", 4);
            InRoomState state = new InRoomState(initialNode);
            Assert.Throws<ArgumentException>(() => state.ApplyVisitNode(8, "wrongStrat"));
        }

        [Fact]
        public void ApplyVisitNode_AccumulatesVisitedNodesAndStrats()
        {
            RoomNode initialNode = Model.GetNodeInRoom("Parlor and Alcatraz", 4);
            InRoomState state = new InRoomState(initialNode);
            state.ApplyVisitNode(Model.GetNodeInRoom("Parlor and Alcatraz", 8), state.CurrentNode.Links[8].Strats["Base"]);
            state.ApplyVisitNode(Model.GetNodeInRoom("Parlor and Alcatraz", 1), state.CurrentNode.Links[1].Strats["Parlor Quick Charge"]);

            Assert.Equal("Parlor and Alcatraz", state.CurrentRoom.Name);
            Assert.Equal(1, state.CurrentNode.Id);
            Assert.Equal("Parlor Quick Charge", state.LastStrat.Name);
            Assert.Equal(3, state.VisitedRoomPath.Count());

            Assert.Equal(4, state.VisitedRoomPath.First().nodeState.Node.Id);
            Assert.Null(state.VisitedRoomPath.First().strat);

            Assert.Equal(8, state.VisitedRoomPath.Skip(1).First().nodeState.Node.Id);
            Assert.Equal("Base", state.VisitedRoomPath.Skip(1).First().strat.Name);

            Assert.Equal(1, state.VisitedRoomPath.Skip(2).First().nodeState.Node.Id);
            Assert.Equal("Parlor Quick Charge", state.VisitedRoomPath.Skip(2).First().strat.Name);
        }

        [Fact]
        public void ApplyVisitNode_NotSpawningInRoom_RejectsNullStrat()
        {
            RoomNode initialNode = Model.GetNodeInRoom("Parlor and Alcatraz", 4);
            InRoomState state = new InRoomState(initialNode);
            Assert.Throws<ArgumentException>(() => state.ApplyVisitNode(Model.GetNodeInRoom("Parlor and Alcatraz", 8), null));
        }

        [Fact]
        public void ApplyVisitNode_LinkDoesntExist_ThrowsException()
        {
            RoomNode initialNode = Model.GetNodeInRoom("Parlor and Alcatraz", 4);
            InRoomState state = new InRoomState(initialNode);
            Assert.Throws<ArgumentException>(() => state.ApplyVisitNode(Model.GetNodeInRoom("Parlor and Alcatraz", 1), state.CurrentNode.Links[8].Strats["Base"]));
        }

        [Fact]
        public void ApplyVisitNode_StratNotOnOriginLink_ThrowsException()
        {
            RoomNode initialNode = Model.GetNodeInRoom("Parlor and Alcatraz", 4);
            InRoomState state = new InRoomState(initialNode);
            Strat wrongStrat = Model.GetNodeInRoom("Parlor and Alcatraz", 8).Links[1].Strats["Base"];
            Assert.Throws<ArgumentException>(() => state.ApplyVisitNode(Model.GetNodeInRoom("Parlor and Alcatraz", 8), wrongStrat));
        }

        [Fact]
        public void ApplyDestroyObstacle_ById_RegistersObstacle()
        {
            RoomNode initialNode = Model.GetNodeInRoom("Landing Site", 5);
            InRoomState state = new InRoomState(initialNode);
            state.ApplyDestroyObstacle("A");

            Assert.Single(state.DestroyedObstacleIds);
            Assert.Contains("A", state.DestroyedObstacleIds);
        }

        [Fact]
        public void ApplyDestroyObstacle_ById_ObstacleNotInRoom_ThrowsException()
        {
            RoomNode initialNode = Model.GetNodeInRoom("Parlor and Alcatraz", 4);
            InRoomState state = new InRoomState(initialNode);

            Assert.Throws<ArgumentException>(() => state.ApplyDestroyObstacle("Z"));
        }

        [Fact]
        public void ApplyDestroyObstacle_RegistersObstacle()
        {
            RoomNode initialNode = Model.GetNodeInRoom("Landing Site", 5);
            InRoomState state = new InRoomState(initialNode);
            state.ApplyDestroyObstacle(state.CurrentRoom.Obstacles["A"]);

            Assert.Single(state.DestroyedObstacleIds);
            Assert.Contains("A", state.DestroyedObstacleIds);
        }

        [Fact]
        public void ApplyDestroyObstacle_ObstacleNotInRoom_ThrowsException()
        {
            RoomNode initialNode = Model.GetNodeInRoom("Parlor and Alcatraz", 4);
            InRoomState state = new InRoomState(initialNode);
            
            Assert.Throws<ArgumentException>(() => state.ApplyDestroyObstacle(Model.Rooms["Landing Site"].Obstacles["A"]));
        }

        [Fact]
        public void ApplyOpenLock_ById_RegistersLock()
        {
            RoomNode node = Model.GetNodeInRoom("Bomb Torizo Room", 1);
            NodeLock openedLock = Model.Locks["Bomb Torizo Room Grey Lock (to Flyway)"];
            InRoomState state = new InRoomState(node);
            state.ApplyOpenLock("Bomb Torizo Room Grey Lock (to Flyway)");
            Assert.Contains(openedLock, state.OpenedExitLocks);
        }

        [Fact]
        public void ApplyOpenLock_ById_LockDoesntExist_ThrowsException()
        {
            RoomNode node = Model.GetNodeInRoom("Landing Site", 5);
            InRoomState state = new InRoomState(node);
            Assert.Throws<ArgumentException>(() => state.ApplyOpenLock("FakeLock"));
        }

        [Fact]
        public void ApplyOpenLock_RegistersLock()
        {
            RoomNode node = Model.GetNodeInRoom("Bomb Torizo Room", 1);
            NodeLock openedLock = Model.Locks["Bomb Torizo Room Grey Lock (to Flyway)"];
            InRoomState state = new InRoomState(node);
            state.ApplyOpenLock(openedLock);
            Assert.Contains(openedLock, state.OpenedExitLocks);
        }

        [Fact]
        public void ApplyOpenLock_LockNotOnNode_ThrowsException()
        {
            RoomNode node = Model.GetNodeInRoom("Landing Site", 5);
            NodeLock openedLock = Model.Locks["Bomb Torizo Room Grey Lock (to Flyway)"];
            InRoomState state = new InRoomState(node);
            Assert.Throws<ArgumentException>(() => state.ApplyOpenLock(openedLock));
        }

        [Fact]
        public void ApplyBypassLock_ById_RemembersLock()
        {
            RoomNode node = Model.GetNodeInRoom("Bomb Torizo Room", 1);
            NodeLock bypassedLock = Model.Locks["Animal Escape Grey Lock (to Flyway)"];
            InRoomState state = new InRoomState(node);
            state.ApplyBypassLock("Animal Escape Grey Lock (to Flyway)");
            Assert.Contains(bypassedLock, state.BypassedExitLocks);
        }

        [Fact]
        public void ApplyBypassLock_ById_LockNotOnNode_ThrowsException()
        {
            RoomNode node = Model.GetNodeInRoom("Landing Site", 5);
            InRoomState state = new InRoomState(node);
            Assert.Throws<ArgumentException>(() => state.ApplyBypassLock("FakeLock"));
        }

        [Fact]
        public void ApplyBypassLock_RemembersLock()
        {
            RoomNode node = Model.GetNodeInRoom("Bomb Torizo Room", 1);
            NodeLock bypassedLock = Model.Locks["Animal Escape Grey Lock (to Flyway)"];
            InRoomState state = new InRoomState(node);
            state.ApplyBypassLock(bypassedLock);
            Assert.Contains(bypassedLock, state.BypassedExitLocks);
        }

        [Fact]
        public void ApplyBypassLock_LockNotOnNode_ThrowsException()
        {
            RoomNode node = Model.GetNodeInRoom("Landing Site", 5);
            NodeLock bypassedLock = Model.Locks["Animal Escape Grey Lock (to Flyway)"];
            InRoomState state = new InRoomState(node);
            Assert.Throws<ArgumentException>(() => state.ApplyBypassLock(bypassedLock));
        }
    }
}
