using sm_json_data_framework.Models;
using sm_json_data_framework.Models.Connections;
using sm_json_data_framework.Models.Enemies;
using sm_json_data_framework.Models.Items;
using sm_json_data_framework.Models.Raw;
using sm_json_data_framework.Models.Requirements.ObjectRequirements;
using sm_json_data_framework.Options;
using sm_json_data_framework.Rules;
using sm_json_data_framework.Tests.TestTools;
using sm_json_data_framework.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace sm_json_data_framework.Reading
{
    public class ModelReaderTest
    {
        #region Tests for ReadUnfinalizedModel()
        [Fact]
        public void ReadUnfinalizedModel_ReadsAllData()
        {
            // When
            UnfinalizedSuperMetroidModel model = ModelReader.ReadUnfinalizedModel();

            // Expect
            // Room counts
            Assert.Equal(54, model.Rooms.Values.Where(room => room.Area == "Brinstar").Count());
            Assert.Equal(6, model.Rooms.Values.Where(room => room.Area == "Ceres Station").Count());
            Assert.Equal(34, model.Rooms.Values.Where(room => room.Area == "Crateria").Count());
            Assert.Equal(23, model.Rooms.Values.Where(room => room.Area == "Lower Norfair").Count());
            Assert.Equal(56, model.Rooms.Values.Where(room => room.Area == "Maridia").Count());
            Assert.Equal(53, model.Rooms.Values.Where(room => room.Area == "Norfair").Count());
            Assert.Equal(19, model.Rooms.Values.Where(room => room.Area == "Tourian").Count());
            Assert.Equal(16, model.Rooms.Values.Where(room => room.Area == "Wrecked Ship").Count());

            // Connection counts. Connection objects in SuperMetroidModel are one-way
            Assert.Equal(119, model.Connections.Values.Where(connection => connection.FromNode.Area == "Brinstar").Count());
            Assert.Equal(11, model.Connections.Values.Where(connection => connection.FromNode.Area == "Ceres Station").Count());
            Assert.Equal(80, model.Connections.Values.Where(connection => connection.FromNode.Area == "Crateria").Count());
            // This should be 52, not 53. There is an error in the model - a connection between the LN elevator and the adjacent save room has
            // Lower Norfair as its area instead of Norfair. Fix this test when the model gets fixed.
            Assert.Equal(53, model.Connections.Values.Where(connection => connection.FromNode.Area == "Lower Norfair").Count());
            Assert.Equal(125, model.Connections.Values.Where(connection => connection.FromNode.Area == "Maridia").Count());
            // This should be 123, not 122. related to the error mentioned for Lower Norfair. Fix this test when the model gets fixed.
            Assert.Equal(122, model.Connections.Values.Where(connection => connection.FromNode.Area == "Norfair").Count());
            Assert.Equal(37, model.Connections.Values.Where(connection => connection.FromNode.Area == "Tourian").Count());
            Assert.Equal(34, model.Connections.Values.Where(connection => connection.FromNode.Area == "Wrecked Ship").Count());

            Assert.Equal(23, model.Items.Count);

            Assert.Equal(27, model.GameFlags.Count);

            Assert.Equal(39, model.Weapons.Count);

            // 81 normal enemies and 14 bosses
            Assert.Equal(95, model.Enemies.Count);

            Assert.Equal(25, model.Helpers.Count);

            Assert.Equal(9, model.TechCategories.Count);
            Assert.Equal(105, model.Techs.Count);

            // Not getting an exact nodes count, but make sure there are many in the top-level dictionary
            Assert.True(model.Nodes.Count > 20);

            // Not getting an exact lock count, but make sure there are many in the top-level dictionary
            Assert.True(model.Locks.Count > 20);

            // Not getting an exact runway count, but make sure there are many in the top-level dictionary
            Assert.True(model.Runways.Count > 20);

            // Not getting an exact room enemy count, but make sure there are many in the top-level dictionary
            Assert.True(model.RoomEnemies.Count > 20);

            // Starting max resources
            Assert.Equal(99, model.StartConditions.BaseResourceMaximums.GetAmount(RechargeableResourceEnum.RegularEnergy));
            Assert.Equal(0, model.StartConditions.BaseResourceMaximums.GetAmount(RechargeableResourceEnum.ReserveEnergy));
            Assert.Equal(0, model.StartConditions.BaseResourceMaximums.GetAmount(RechargeableResourceEnum.Missile));
            Assert.Equal(0, model.StartConditions.BaseResourceMaximums.GetAmount(RechargeableResourceEnum.Super));
            Assert.Equal(0, model.StartConditions.BaseResourceMaximums.GetAmount(RechargeableResourceEnum.PowerBomb));

            // Starting resources
            Assert.Equal(99, model.StartConditions.StartingResources.GetAmount(RechargeableResourceEnum.RegularEnergy));
            Assert.Equal(0, model.StartConditions.StartingResources.GetAmount(RechargeableResourceEnum.ReserveEnergy));
            Assert.Equal(0, model.StartConditions.StartingResources.GetAmount(RechargeableResourceEnum.Missile));
            Assert.Equal(0, model.StartConditions.StartingResources.GetAmount(RechargeableResourceEnum.Super));
            Assert.Equal(0, model.StartConditions.StartingResources.GetAmount(RechargeableResourceEnum.PowerBomb));

            // Starting items
            Assert.Equal(2, model.StartConditions.StartingInventory.NonConsumableItems.Count);
            Assert.True(model.StartConditions.StartingInventory.NonConsumableItems.ContainsItem("PowerBeam"));
            Assert.True(model.StartConditions.StartingInventory.NonConsumableItems.ContainsItem("PowerSuit"));
            Assert.Empty(model.StartConditions.StartingInventory.ExpansionItems);

            // Starting game state
            Assert.Empty(model.StartConditions.StartingGameFlags);
            Assert.Empty(model.StartConditions.StartingOpenLocks);
            Assert.Empty(model.StartConditions.StartingTakenItemLocations);

            // Starting location
            Assert.Equal("Ceres Elevator Room", model.StartConditions.StartingNode.Room.Name);
            Assert.Equal(1, model.StartConditions.StartingNode.Id);
        }

        [Fact]
        public void ReadUnfinalizedModel_UsesOptionalParameters()
        {
            // When
            UnfinalizedSuperMetroidModel model = ModelReader.ReadUnfinalizedModel(rules: new RandoSuperMetroidRules(),
                overrideObjectTypes: new List<(ObjectLogicalElementTypeEnum typeEnum, Type type)> { (ObjectLogicalElementTypeEnum.AcidFrames, typeof(ExtendedAcidFrames)) });

            // Expect
            Assert.True(model.Rules is RandoSuperMetroidRules);
            Assert.NotEmpty(model.Rooms["Crocomire's Room"].Nodes[3].LinksTo[6].Strats["Gravity Acid"].Requires.LogicalElements.Where(element => element.GetType() == typeof(ExtendedAcidFrames)));
        }
        #endregion

        #region Tests for ReadRawModel()

        [Fact]
        public void ReadRawModel_ReadsAllData()
        {
            // When
            RawSuperMetroidModel model = ModelReader.ReadRawModel();

            // Expect
            // Room counts
            Assert.Equal(54, model.RoomContainer.Rooms.Where(room => room.Area == "Brinstar").Count());
            Assert.Equal(6, model.RoomContainer.Rooms.Where(room => room.Area == "Ceres Station").Count());
            Assert.Equal(34, model.RoomContainer.Rooms.Where(room => room.Area == "Crateria").Count());
            Assert.Equal(23, model.RoomContainer.Rooms.Where(room => room.Area == "Lower Norfair").Count());
            Assert.Equal(56, model.RoomContainer.Rooms.Where(room => room.Area == "Maridia").Count());
            Assert.Equal(53, model.RoomContainer.Rooms.Where(room => room.Area == "Norfair").Count());
            Assert.Equal(19, model.RoomContainer.Rooms.Where(room => room.Area == "Tourian").Count());
            Assert.Equal(16, model.RoomContainer.Rooms.Where(room => room.Area == "Wrecked Ship").Count());

            // Connection counts
            Assert.Equal(298, model.ConnectionContainer.Connections.Count());

            // Item counts
            Assert.Equal(2, model.ItemContainer.ImplicitItems.Count());
            Assert.Equal(16, model.ItemContainer.UpgradeItems.Count());
            Assert.Equal(5, model.ItemContainer.ExpansionItems.Count());

            Assert.Equal(27, model.ItemContainer.GameFlags.Count());

            Assert.Equal(5, model.ItemContainer.StartingResources.Count());

            Assert.Equal(39, model.WeaponContainer.Weapons.Count());

            // Normal enemies and 14 bosses
            Assert.Equal(81, model.EnemyContainer.Enemies.Count());
            Assert.Equal(14, model.BossContainer.Enemies.Count());

            // Helpers
            Assert.Equal(25, model.HelperContainer.Helpers.Count());

            // Techs
            Assert.Equal(105, model.TechContainer.TechCategories.SelectMany(category => category.Techs).SelectMany(tech => tech.SelectWithExtensions()).ToList().Count());

            // Starting items
            Assert.Equal(2, model.ItemContainer.StartingItems.Count());
            Assert.Contains("PowerBeam", model.ItemContainer.StartingItems);
            Assert.Contains("PowerSuit", model.ItemContainer.StartingItems);

            // Starting game state
            Assert.Empty(model.ItemContainer.StartingFlags);
            Assert.Empty(model.ItemContainer.StartingLocks);

            // Starting location
            Assert.Equal("Ceres Elevator Room", model.ItemContainer.StartingRoom);
            Assert.Equal(1, model.ItemContainer.StartingNode);
        }

        #endregion
    }
}
