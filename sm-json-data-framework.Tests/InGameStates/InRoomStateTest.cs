using sm_json_data_framework.Models;
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
        public void ApplyVisitNode_LinkDoesntExist_throwsException()
        {
            RoomNode initialNode = Model.GetNodeInRoom("Parlor and Alcatraz", 4);
            InRoomState state = new InRoomState(initialNode);
            Assert.Throws<ArgumentException>(() => state.ApplyVisitNode(Model.GetNodeInRoom("Parlor and Alcatraz", 1), state.CurrentNode.Links[8].Strats["Base"]));
        }

        [Fact]
        public void ApplyVisitNode_StratNotOnOriginLink_throwsException()
        {
            RoomNode initialNode = Model.GetNodeInRoom("Parlor and Alcatraz", 4);
            InRoomState state = new InRoomState(initialNode);
            Strat wrongStrat = Model.GetNodeInRoom("Parlor and Alcatraz", 8).Links[1].Strats["Base"];
            Assert.Throws<ArgumentException>(() => state.ApplyVisitNode(Model.GetNodeInRoom("Parlor and Alcatraz", 8), wrongStrat));
        }

    }
}
