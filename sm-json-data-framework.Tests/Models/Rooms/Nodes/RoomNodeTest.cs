using sm_json_data_framework.Models.Rooms.Nodes;
using sm_json_data_framework.Models;
using sm_json_data_framework.Options;
using sm_json_data_framework.Rules;
using sm_json_data_framework.Tests.TestTools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using sm_json_data_framework.Models.InGameStates;

namespace sm_json_data_framework.Tests.Models.Rooms.Nodes
{
    public class RoomNodeTest
    {
        private static SuperMetroidModel Model = StaticTestObjects.UnmodifiableModel;
        private static SuperMetroidModel ModelWithOptions = StaticTestObjects.UnfinalizedModel.Finalize();

        #region Tests for construction from unfinalized model

        [Fact]
        public void CtorFromUnfinalized_SetsPropertiesCorrectly()
        {
            // Given/when standard model creation

            // Expect
            RoomNode node = Model.Rooms["Landing Site"].Nodes[1];
            Assert.Equal(1, node.Id);
            Assert.Equal("Landing Site_1", node.IdentifyingString);
            Assert.Equal("Landing Site Top Left Door (to Gauntlet)", node.Name);
            Assert.Equal(NodeTypeEnum.Door, node.NodeType);
            Assert.Equal(NodeSubTypeEnum.Blue, node.NodeSubType);
            Assert.Null(node.NodeItem);
            Assert.Equal("0x001892e", node.NodeAddress);
            Assert.Equal(1, node.DoorEnvironments.Count);
            Assert.Equal(PhysicsEnum.Normal, node.DoorEnvironments.First().Physics);
            Assert.Empty(node.InteractionRequires.LogicalElements);
            Assert.Equal(1, node.Runways.Count);
            Assert.Contains("Base Runway - Landing Site Top Left Door (to Gauntlet)", node.Runways.Keys);
            Assert.Equal(3, node.CanLeaveCharged.Count);
            Assert.Null(node.OverrideSpawnAtNode);
            Assert.Same(node, node.SpawnAtNode);
            Assert.False(node.SpawnsAtDifferentNode);
            Assert.Equal(1, node.Locks.Count);
            Assert.Contains("Landing Site Top Left Escape Lock (to Gauntlet)", node.Locks.Keys);
            Assert.Empty(node.Utility);
            Assert.Empty(node.ViewableNodes);
            Assert.Empty(node.Yields);
            Assert.Same(Model.Rooms["Landing Site"], node.Room);
            Assert.Same(Model.Connections[node.IdentifyingString], node.OutConnection);
            Assert.Same(Model.Rooms["Gauntlet Entrance"].Nodes[2], node.OutNode);
            Assert.Equal(2, node.LinksTo.Count);
            Assert.Same(Model.Rooms["Landing Site"].Links[1].To[4], node.LinksTo[4]);
            Assert.Empty(node.TwinDoorAddresses);

            RoomNode itemNode = Model.Rooms["Morph Ball Room"].Nodes[4];
            Assert.Same(Model.Items["Morph"], itemNode.NodeItem);
            Assert.Empty(itemNode.DoorEnvironments);
            Assert.Empty(itemNode.Runways);
            Assert.Empty(itemNode.CanLeaveCharged);
            Assert.Null(itemNode.OverrideSpawnAtNode);
            Assert.Same(itemNode, itemNode.SpawnAtNode);
            Assert.False(itemNode.SpawnsAtDifferentNode);
            Assert.Null(itemNode.OutConnection);
            Assert.Null(itemNode.OutNode);

            RoomNode utilityNode = Model.Rooms["Tourian Recharge Room"].Nodes[2];
            Assert.Equal(1, utilityNode.Utility.Count);
            Assert.Contains(UtilityEnum.Missile, utilityNode.Utility);

            RoomNode nodeWithTwin = Model.Rooms["East Pants Room"].Nodes[2];
            Assert.Equal(1, nodeWithTwin.TwinDoorAddresses.Count);
            Assert.Empty(nodeWithTwin.Locks);

            RoomNode nodeWithYields = Model.Rooms["Spore Spawn Room"].Nodes[3];
            Assert.Equal(1, nodeWithYields.Yields.Count);
            Assert.Same(Model.GameFlags["f_DefeatedSporeSpawn"], nodeWithYields.Yields["f_DefeatedSporeSpawn"]);

            RoomNode nodeWithNoLinks = Model.Rooms["Bug Sand Hole"].Nodes[2];
            Assert.Empty(nodeWithNoLinks.LinksTo);

            RoomNode nodeWithViewableNodes = Model.Rooms["Blue Brinstar Energy Tank Room"].Nodes[1];
            Assert.Equal(1, nodeWithViewableNodes.ViewableNodes.Count);
            Assert.Contains(3, nodeWithViewableNodes.ViewableNodes.Keys);

            RoomNode nodeWithSpawnAt = Model.Rooms["Oasis"].Nodes[3];
            Assert.Same(Model.Rooms["Oasis"].Nodes[4], nodeWithSpawnAt.SpawnAtNode);
            Assert.Same(Model.Rooms["Oasis"].Nodes[4], nodeWithSpawnAt.OverrideSpawnAtNode);
            Assert.True(nodeWithSpawnAt.SpawnsAtDifferentNode);
        }

        #endregion

        #region Tests for GetActiveLocks()

        [Fact]
        public void GetActiveLocks_ReturnsOnlyActiveLocks()
        {
            // Given
            InGameState inGameState = Model.CreateInitialGameState();

            // When
            RoomNode node = Model.Rooms["Landing Site"].Nodes[4];
            IDictionary<string, NodeLock> result = node.GetActiveLocks(Model, inGameState);

            // Expect
            Assert.Equal(1, result.Count);
            Assert.Same(Model.Locks["Landing Site Bottom Right Green Lock (to Crateria Tube)"], result["Landing Site Bottom Right Green Lock (to Crateria Tube)"]);
        }

        #endregion

        // Can't test InteractExecution.Execute(), because it's not used in the model...

        #region Tests for ApplyLogicalOptions() that check applied logical properties

        [Fact]
        public void ApplyLogicalOptions_SetsLogicalProperties()
        {
            // Given
            LogicalOptions logicalOptions = new LogicalOptions();

            // When
            ModelWithOptions.ApplyLogicalOptions(logicalOptions);

            // Expect
            RoomNode node = ModelWithOptions.GetNodeInRoom("Landing Site", 5);
            Assert.True(node.LogicallyRelevant);
            Assert.False(node.LogicallyNeverInteract);
            // Model doesn't contain a InteractRequires value so no way to test for its never...
        }

        #endregion
    }
}
