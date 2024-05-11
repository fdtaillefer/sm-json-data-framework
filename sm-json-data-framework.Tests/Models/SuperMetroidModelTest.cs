using sm_json_data_framework.Models;
using sm_json_data_framework.Models.Connections;
using sm_json_data_framework.Models.Enemies;
using sm_json_data_framework.Models.Helpers;
using sm_json_data_framework.Models.Items;
using sm_json_data_framework.Models.Raw;
using sm_json_data_framework.Models.Requirements;
using sm_json_data_framework.Models.Requirements.ObjectRequirements;
using sm_json_data_framework.Models.Requirements.ObjectRequirements.SubRequirements;
using sm_json_data_framework.Models.Requirements.StringRequirements;
using sm_json_data_framework.Models.Rooms;
using sm_json_data_framework.Models.Rooms.Nodes;
using sm_json_data_framework.Models.Techs;
using sm_json_data_framework.Models.Weapons;
using sm_json_data_framework.Options;
using sm_json_data_framework.Reading;
using sm_json_data_framework.Tests.TestTools;
using sm_json_data_framework.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace sm_json_data_framework.Tests.Models
{
    public class SuperMetroidModelTest
    {
        #region Tests for Ctor(RawSuperMetroidModel, [...])
        [Fact]
        public void ConstructorFromRawModel_ReadsAllData()
        {
            // When
            SuperMetroidModel model = new SuperMetroidModel(StaticTestObjects.RawModel);

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

            Assert.Equal(91, model.Techs.Count);

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
        public void ConstructorFromRawModel_UsesOptionalParameters()
        {
            // Given
            RawSuperMetroidModel rawModel = ModelReader.ReadRawModel();

            // When
            SuperMetroidModel model = new SuperMetroidModel(rawModel, rules: new RandoSuperMetroidRules(),
                basicStartConditionsCustomizer: new RandoBasicStartConditionsCustomizer(),
                overrideObjectTypes: new List<(ObjectLogicalElementTypeEnum typeEnum, Type type)> { (ObjectLogicalElementTypeEnum.AcidFrames, typeof(ExtendedAcidFrames)) },
                overrideStringTypes: new List<(StringLogicalElementTypeEnum typeEnum, Type type)> { (StringLogicalElementTypeEnum.Item, typeof(ExtendedItemLogicalElement)) });


            // Expect
            Assert.True(model.Rules is RandoSuperMetroidRules);
            Assert.Contains("f_ZebesAwake", model.StartConditions.StartingGameFlags.Select(flag => flag.Name));
            Assert.NotEmpty(model.Rooms["Crocomire's Room"].Nodes[3].Links[6].Strats["Gravity Acid"].Requires.LogicalElements.Where(element => element.GetType() == typeof(ExtendedAcidFrames)));
            Assert.NotEmpty(model.Rooms["Parlor and Alcatraz"].Nodes[5].Links[8].Strats["Alcatraz Escape"].Requires.LogicalElements.Where(element => element.GetType() == typeof(ExtendedItemLogicalElement)));
        }
        #endregion

        #region Tests for ApplyLogicalOptiopns()

        [Fact]
        public void ApplyLogicalOptions_AppliesCloneToSuperMetroidModelProperties()
        {
            // Given
            LogicalOptions logicalOptions = new LogicalOptions();
            logicalOptions.TilesToShineCharge = 20;
            SuperMetroidModel model = new SuperMetroidModel(StaticTestObjects.RawModel);

            // When
            model.ApplyLogicalOptions(logicalOptions);

            // Expect

            ReadOnlyLogicalOptions appliedOptions = model.Rooms["Landing Site"].AppliedLogicalOptions;
            Assert.NotSame(logicalOptions, appliedOptions);
            Assert.Equal(20, appliedOptions.TilesToShineCharge);

            Assert.Same(appliedOptions, model.Weapons["Wave"].AppliedLogicalOptions);
            Assert.Same(appliedOptions, model.Enemies["Geemer (blue)"].AppliedLogicalOptions);
            Assert.Same(appliedOptions, model.Items["SpeedBooster"].AppliedLogicalOptions);
            Assert.Same(appliedOptions, model.GameFlags["f_ZebesSetAblaze"].AppliedLogicalOptions);
            Assert.Same(appliedOptions, model.Techs["canWalljump"].AppliedLogicalOptions);
            Assert.Same(appliedOptions, model.Helpers["h_canOpenZebetites"].AppliedLogicalOptions);
            Assert.Same(appliedOptions, model.Connections[model.GetNodeInRoom("Landing Site", 1).IdentifyingString].AppliedLogicalOptions);
        }

        [Fact]
        public void ApplyLogicalOptions_AppliesCloneToTechProperties()
        {
            // Given
            LogicalOptions logicalOptions = new LogicalOptions();
            logicalOptions.TilesToShineCharge = 20;
            SuperMetroidModel model = new SuperMetroidModel(StaticTestObjects.RawModel);

            // When
            model.ApplyLogicalOptions(logicalOptions);

            // Expect
            UnfinalizedTech arbitraryTech = model.Techs["canWalljump"];
            ReadOnlyLogicalOptions appliedOptions = arbitraryTech.AppliedLogicalOptions;
            Assert.NotSame(logicalOptions, appliedOptions);
            Assert.Equal(20, appliedOptions.TilesToShineCharge);

            Assert.Same(appliedOptions, arbitraryTech.Requires.AppliedLogicalOptions);
        }

        [Fact]
        public void ApplyLogicalOptions_AppliesCloneToHelperProperties()
        {
            // Given
            LogicalOptions logicalOptions = new LogicalOptions();
            logicalOptions.TilesToShineCharge = 20;
            SuperMetroidModel model = new SuperMetroidModel(StaticTestObjects.RawModel);

            // When
            model.ApplyLogicalOptions(logicalOptions);

            // Expect
            UnfinalizedHelper arbitraryHelper = model.Helpers["h_canOpenZebetites"];
            ReadOnlyLogicalOptions appliedOptions = arbitraryHelper.AppliedLogicalOptions;
            Assert.NotSame(logicalOptions, appliedOptions);
            Assert.Equal(20, appliedOptions.TilesToShineCharge);

            Assert.Same(appliedOptions, arbitraryHelper.Requires.AppliedLogicalOptions);
        }

        [Fact]
        public void ApplyLogicalOptions_AppliesCloneToWeaponProperties()
        {
            // Given
            LogicalOptions logicalOptions = new LogicalOptions();
            logicalOptions.TilesToShineCharge = 20;
            SuperMetroidModel model = new SuperMetroidModel(StaticTestObjects.RawModel);

            // When
            model.ApplyLogicalOptions(logicalOptions);

            // Expect
            UnfinalizedWeapon arbitraryWeapon = model.Weapons["Wave"];
            ReadOnlyLogicalOptions appliedOptions = arbitraryWeapon.AppliedLogicalOptions;
            Assert.NotSame(logicalOptions, appliedOptions);
            Assert.Equal(20, appliedOptions.TilesToShineCharge);

            Assert.Same(appliedOptions, arbitraryWeapon.UseRequires.AppliedLogicalOptions);
            Assert.Same(appliedOptions, arbitraryWeapon.ShotRequires.AppliedLogicalOptions);
        }

        [Fact]
        public void ApplyLogicalOptions_AppliesCloneToEnemyProperties()
        {
            // Given
            LogicalOptions logicalOptions = new LogicalOptions();
            logicalOptions.TilesToShineCharge = 20;
            SuperMetroidModel model = new SuperMetroidModel(StaticTestObjects.RawModel);

            // When
            model.ApplyLogicalOptions(logicalOptions);

            // Expect
            UnfinalizedEnemy arbitraryEnemy = model.Enemies["Geemer (blue)"];
            ReadOnlyLogicalOptions appliedOptions = arbitraryEnemy.AppliedLogicalOptions;
            Assert.NotSame(logicalOptions, appliedOptions);
            Assert.Equal(20, appliedOptions.TilesToShineCharge);

            Assert.Same(appliedOptions, arbitraryEnemy.Attacks["contact"].AppliedLogicalOptions);
        }

        [Fact]
        public void ApplyLogicalOptions_AppliesCloneToConnectionProperties()
        {
            // Given
            LogicalOptions logicalOptions = new LogicalOptions();
            logicalOptions.TilesToShineCharge = 20;
            SuperMetroidModel model = new SuperMetroidModel(StaticTestObjects.RawModel);

            // When
            model.ApplyLogicalOptions(logicalOptions);

            // Expect
            UnfinalizedConnection arbitraryConnection= model.Connections[model.GetNodeInRoom("Landing Site", 1).IdentifyingString];
            ReadOnlyLogicalOptions appliedOptions = arbitraryConnection.AppliedLogicalOptions;
            Assert.NotSame(logicalOptions, appliedOptions);
            Assert.Equal(20, appliedOptions.TilesToShineCharge);

            Assert.Same(appliedOptions, arbitraryConnection.FromNode.AppliedLogicalOptions);
            Assert.Same(appliedOptions, arbitraryConnection.ToNode.AppliedLogicalOptions);
        }

        [Fact]
        public void ApplyLogicalOptions_AppliesCloneToRoomProperties()
        {
            // Given
            LogicalOptions logicalOptions = new LogicalOptions();
            logicalOptions.TilesToShineCharge = 20;
            SuperMetroidModel model = new SuperMetroidModel(StaticTestObjects.RawModel);

            // When
            model.ApplyLogicalOptions(logicalOptions);

            // Expect
            UnfinalizedRoom arbitraryRoom = model.Rooms["Climb"];
            ReadOnlyLogicalOptions appliedOptions = arbitraryRoom.AppliedLogicalOptions;
            Assert.NotSame(logicalOptions, appliedOptions);
            Assert.Equal(20, appliedOptions.TilesToShineCharge);

            Assert.Same(appliedOptions, arbitraryRoom.Nodes[1].AppliedLogicalOptions);
            Assert.Same(appliedOptions, arbitraryRoom.Links[1].AppliedLogicalOptions);
            Assert.Same(appliedOptions, arbitraryRoom.RoomEnvironments[0].AppliedLogicalOptions);
            Assert.Same(appliedOptions, arbitraryRoom.Obstacles["A"].AppliedLogicalOptions);
            Assert.Same(appliedOptions, arbitraryRoom.Enemies["e1"].AppliedLogicalOptions);
        }

        [Fact]
        public void ApplyLogicalOptions_AppliesCloneToLinkProperties()
        {
            // Given
            LogicalOptions logicalOptions = new LogicalOptions();
            logicalOptions.TilesToShineCharge = 20;
            SuperMetroidModel model = new SuperMetroidModel(StaticTestObjects.RawModel);

            // When
            model.ApplyLogicalOptions(logicalOptions);

            // Expect
            UnfinalizedLink arbitraryLink = model.Rooms["Landing Site"].Links[1];
            ReadOnlyLogicalOptions appliedOptions = arbitraryLink.AppliedLogicalOptions;
            Assert.NotSame(logicalOptions, appliedOptions);
            Assert.Equal(20, appliedOptions.TilesToShineCharge);

            Assert.Same(appliedOptions, arbitraryLink.To[4].AppliedLogicalOptions);
        }

        [Fact]
        public void ApplyLogicalOptions_AppliesCloneToLinkToProperties()
        {
            // Given
            LogicalOptions logicalOptions = new LogicalOptions();
            logicalOptions.TilesToShineCharge = 20;
            SuperMetroidModel model = new SuperMetroidModel(StaticTestObjects.RawModel);

            // When
            model.ApplyLogicalOptions(logicalOptions);

            // Expect
            UnfinalizedLinkTo arbitraryLinkTo = model.Rooms["Landing Site"].Links[1].To[4];
            ReadOnlyLogicalOptions appliedOptions = arbitraryLinkTo.AppliedLogicalOptions;
            Assert.NotSame(logicalOptions, appliedOptions);
            Assert.Equal(20, appliedOptions.TilesToShineCharge);

            Assert.Same(appliedOptions, arbitraryLinkTo.Strats["Shinespark"].AppliedLogicalOptions);
        }

        [Fact]
        public void ApplyLogicalOptions_AppliesCloneToStratProperties()
        {
            // Given
            LogicalOptions logicalOptions = new LogicalOptions();
            logicalOptions.TilesToShineCharge = 20;
            SuperMetroidModel model = new SuperMetroidModel(StaticTestObjects.RawModel);

            // When
            model.ApplyLogicalOptions(logicalOptions);

            // Expect
            UnfinalizedStrat arbitraryStrat = model.Rooms["Pink Brinstar Power Bomb Room"].Links[3].To[4].Strats["Mission Impossible"];
            ReadOnlyLogicalOptions appliedOptions = arbitraryStrat.AppliedLogicalOptions;
            Assert.NotSame(logicalOptions, appliedOptions);
            Assert.Equal(20, appliedOptions.TilesToShineCharge);

            Assert.Same(appliedOptions, arbitraryStrat.Failures["Crumble Failure"].AppliedLogicalOptions);
            Assert.Same(appliedOptions, arbitraryStrat.Obstacles["A"].AppliedLogicalOptions);
            Assert.Same(appliedOptions, arbitraryStrat.Requires.AppliedLogicalOptions);
        }

        [Fact]
        public void ApplyLogicalOptions_AppliesCloneToRoomObstacleProperties()
        {
            // Given
            LogicalOptions logicalOptions = new LogicalOptions();
            logicalOptions.TilesToShineCharge = 20;
            SuperMetroidModel model = new SuperMetroidModel(StaticTestObjects.RawModel);

            // When
            model.ApplyLogicalOptions(logicalOptions);

            // Expect
            UnfinalizedRoomObstacle arbitraryRoomObstacle = model.Rooms["Climb"].Obstacles["A"];
            ReadOnlyLogicalOptions appliedOptions = arbitraryRoomObstacle.AppliedLogicalOptions;
            Assert.NotSame(logicalOptions, appliedOptions);
            Assert.Equal(20, appliedOptions.TilesToShineCharge);

            Assert.Same(appliedOptions, arbitraryRoomObstacle.Requires.AppliedLogicalOptions);
        }

        [Fact]
        public void ApplyLogicalOptions_AppliesCloneToRoomEnemyProperties()
        {
            // Given
            LogicalOptions logicalOptions = new LogicalOptions();
            logicalOptions.TilesToShineCharge = 20;
            SuperMetroidModel model = new SuperMetroidModel(StaticTestObjects.RawModel);

            // When
            model.ApplyLogicalOptions(logicalOptions);

            // Expect
            UnfinalizedRoomEnemy arbitraryRoomEnemy = model.Rooms["Early Supers Room"].Enemies["e1"];
            ReadOnlyLogicalOptions appliedOptions = arbitraryRoomEnemy.AppliedLogicalOptions;
            Assert.NotSame(logicalOptions, appliedOptions);
            Assert.Equal(20, appliedOptions.TilesToShineCharge);

            Assert.Same(appliedOptions, arbitraryRoomEnemy.FarmCycles["Crouch over spawn point"].AppliedLogicalOptions);
            Assert.Same(appliedOptions, arbitraryRoomEnemy.Spawn.AppliedLogicalOptions);
            Assert.Same(appliedOptions, arbitraryRoomEnemy.StopSpawn.AppliedLogicalOptions);
            Assert.Same(appliedOptions, arbitraryRoomEnemy.DropRequires.AppliedLogicalOptions);
        }

        [Fact]
        public void ApplyLogicalOptions_AppliesCloneToFarmCycleProperties()
        {
            // Given
            LogicalOptions logicalOptions = new LogicalOptions();
            logicalOptions.TilesToShineCharge = 20;
            SuperMetroidModel model = new SuperMetroidModel(StaticTestObjects.RawModel);

            // When
            model.ApplyLogicalOptions(logicalOptions);

            // Expect
            UnfinalizedFarmCycle arbitraryFarmCycle = model.Rooms["Early Supers Room"].Enemies["e1"].FarmCycles["Crouch over spawn point"];
            ReadOnlyLogicalOptions appliedOptions = arbitraryFarmCycle.AppliedLogicalOptions;
            Assert.NotSame(logicalOptions, appliedOptions);
            Assert.Equal(20, appliedOptions.TilesToShineCharge);

            Assert.Same(appliedOptions, arbitraryFarmCycle.Requires.AppliedLogicalOptions);
        }

        [Fact]
        public void ApplyLogicalOptions_AppliesCloneToNodeProperties()
        {
            // Given
            LogicalOptions logicalOptions = new LogicalOptions();
            logicalOptions.TilesToShineCharge = 20;
            SuperMetroidModel model = new SuperMetroidModel(StaticTestObjects.RawModel);

            // When
            model.ApplyLogicalOptions(logicalOptions);

            // Expect
            UnfinalizedRoomNode arbitraryNode = model.GetNodeInRoom("Landing Site", 1);
            ReadOnlyLogicalOptions appliedOptions = arbitraryNode.AppliedLogicalOptions;
            Assert.NotSame(logicalOptions, appliedOptions);
            Assert.Equal(20, appliedOptions.TilesToShineCharge);
            
            Assert.Same(appliedOptions, arbitraryNode.Locks["Landing Site Top Left Escape Lock (to Gauntlet)"].AppliedLogicalOptions);
            Assert.Same(appliedOptions, arbitraryNode.DoorEnvironments[0].AppliedLogicalOptions);
            Assert.Same(appliedOptions, arbitraryNode.CanLeaveCharged[0].AppliedLogicalOptions);
            Assert.Same(appliedOptions, arbitraryNode.InteractionRequires.AppliedLogicalOptions);
            Assert.Same(appliedOptions, arbitraryNode.Runways["Base Runway - Landing Site Top Left Door (to Gauntlet)"].AppliedLogicalOptions);
            Assert.Same(appliedOptions, model.GetNodeInRoom("Blue Brinstar Energy Tank Room", 1).ViewableNodes[0].AppliedLogicalOptions);
        }

        [Fact]
        public void ApplyLogicalOptions_AppliesCloneToRunwayProperties()
        {
            // Given
            LogicalOptions logicalOptions = new LogicalOptions();
            logicalOptions.TilesToShineCharge = 20;
            SuperMetroidModel model = new SuperMetroidModel(StaticTestObjects.RawModel);

            // When
            model.ApplyLogicalOptions(logicalOptions);

            // Expect
            UnfinalizedRunway arbitraryRunway = model.GetNodeInRoom("Climb", 5).Runways["Base Runway - Climb Bottom Right Door (to Pit Room)"];
            ReadOnlyLogicalOptions appliedOptions = arbitraryRunway.AppliedLogicalOptions;
            Assert.NotSame(logicalOptions, appliedOptions);
            Assert.Equal(20, appliedOptions.TilesToShineCharge);

            Assert.Same(appliedOptions, arbitraryRunway.Strats["Base"].AppliedLogicalOptions);
        }

        [Fact]
        public void ApplyLogicalOptions_AppliesCloneToCanLeaveChargedProperties()
        {
            // Given
            LogicalOptions logicalOptions = new LogicalOptions();
            logicalOptions.TilesToShineCharge = 20;
            SuperMetroidModel model = new SuperMetroidModel(StaticTestObjects.RawModel);

            // When
            model.ApplyLogicalOptions(logicalOptions);

            // Expect
            UnfinalizedCanLeaveCharged arbitraryCanLeaveCharged = model.GetNodeInRoom("Landing Site", 1).CanLeaveCharged.First();
            ReadOnlyLogicalOptions appliedOptions = arbitraryCanLeaveCharged.AppliedLogicalOptions;
            Assert.NotSame(logicalOptions, appliedOptions);
            Assert.Equal(20, appliedOptions.TilesToShineCharge);

            Assert.Same(appliedOptions, arbitraryCanLeaveCharged.Strats["Base"].AppliedLogicalOptions);
            Assert.Same(appliedOptions, arbitraryCanLeaveCharged.InitiateRemotely.AppliedLogicalOptions);
        }

        [Fact]
        public void ApplyLogicalOptions_AppliesCloneToViewableNodeProperties()
        {
            // Given
            LogicalOptions logicalOptions = new LogicalOptions();
            logicalOptions.TilesToShineCharge = 20;
            SuperMetroidModel model = new SuperMetroidModel(StaticTestObjects.RawModel);

            // When
            model.ApplyLogicalOptions(logicalOptions);

            // Expect
            UnfinalizedViewableNode arbitraryViewableNode = model.GetNodeInRoom("Blue Brinstar Energy Tank Room", 1).ViewableNodes[0];
            ReadOnlyLogicalOptions appliedOptions = arbitraryViewableNode.AppliedLogicalOptions;
            Assert.NotSame(logicalOptions, appliedOptions);
            Assert.Equal(20, appliedOptions.TilesToShineCharge);

            Assert.Same(appliedOptions, arbitraryViewableNode.Strats["Base"].AppliedLogicalOptions);
        }

        [Fact]
        public void ApplyLogicalOptions_AppliesCloneToNodeLockProperties()
        {
            // Given
            LogicalOptions logicalOptions = new LogicalOptions();
            logicalOptions.TilesToShineCharge = 20;
            SuperMetroidModel model = new SuperMetroidModel(StaticTestObjects.RawModel);

            // When
            model.ApplyLogicalOptions(logicalOptions);

            // Expect
            UnfinalizedNodeLock arbitraryLock = model.GetNodeInRoom("West Ocean", 4).Locks["West Ocean Ship Exit Grey Lock (to Gravity Suit Room)"];
            ReadOnlyLogicalOptions appliedOptions = arbitraryLock.AppliedLogicalOptions;
            Assert.NotSame(logicalOptions, appliedOptions);
            Assert.Equal(20, appliedOptions.TilesToShineCharge);

            Assert.Same(appliedOptions, arbitraryLock.UnlockStrats["Base"].AppliedLogicalOptions);
            Assert.Same(appliedOptions, arbitraryLock.BypassStrats["Bowling Skip"].AppliedLogicalOptions);
            Assert.Same(appliedOptions, arbitraryLock.Lock.AppliedLogicalOptions);
        }

        [Fact]
        public void ApplyLogicalOptions_AppliesCloneToLogicalRequirementsProperties()
        {
            // Given
            LogicalOptions logicalOptions = new LogicalOptions();
            logicalOptions.TilesToShineCharge = 20;
            SuperMetroidModel model = new SuperMetroidModel(StaticTestObjects.RawModel);

            // When
            model.ApplyLogicalOptions(logicalOptions);

            // Expect
            UnfinalizedLogicalRequirements arbitraryLogicalRequirements = model.Rooms["Climb"].Links[6].To[3].Strats["Behemoth Spark Top"].Requires;
            ReadOnlyLogicalOptions appliedOptions = arbitraryLogicalRequirements.AppliedLogicalOptions;
            Assert.NotSame(logicalOptions, appliedOptions);
            Assert.Equal(20, appliedOptions.TilesToShineCharge);

            foreach (IUnfinalizedLogicalElement logicalElement in arbitraryLogicalRequirements.LogicalElements)
            {
                Assert.Same(appliedOptions, logicalElement.AppliedLogicalOptions);
            }
        }

        [Fact]
        public void ApplyLogicalOptions_AppliesCloneToAndOrProperties()
        {
            // Given
            LogicalOptions logicalOptions = new LogicalOptions();
            logicalOptions.TilesToShineCharge = 20;
            SuperMetroidModel model = new SuperMetroidModel(StaticTestObjects.RawModel);

            // When
            model.ApplyLogicalOptions(logicalOptions);

            // Expect
            UnfinalizedOr arbitraryOr = (UnfinalizedOr)model.Helpers["h_canPassBombPassages"].Requires.LogicalElements.Where(element => typeof(UnfinalizedOr).IsAssignableFrom(element.GetType())).First();
            ReadOnlyLogicalOptions appliedOptions = arbitraryOr.AppliedLogicalOptions;
            Assert.NotSame(logicalOptions, appliedOptions);
            Assert.Equal(20, appliedOptions.TilesToShineCharge);

            Assert.Same(appliedOptions, arbitraryOr.AppliedLogicalOptions);
        }

        [Fact]
        public void ApplyLogicalOptions_AppliesCloneToAndProperties()
        {
            // Given
            LogicalOptions logicalOptions = new LogicalOptions();
            logicalOptions.TilesToShineCharge = 20;
            SuperMetroidModel model = new SuperMetroidModel(StaticTestObjects.RawModel);

            // When
            model.ApplyLogicalOptions(logicalOptions);

            // Expect
            UnfinalizedAnd arbitraryAnd =(UnfinalizedAnd) ((UnfinalizedOr)model.Helpers["h_canPassBombPassages"].Requires.LogicalElements.Where(element => typeof(UnfinalizedOr).IsAssignableFrom(element.GetType())).First())
                .LogicalRequirements.LogicalElements.Where(element => typeof(UnfinalizedAnd).IsAssignableFrom(element.GetType())).First();
            ReadOnlyLogicalOptions appliedOptions = arbitraryAnd.AppliedLogicalOptions;
            Assert.NotSame(logicalOptions, appliedOptions);
            Assert.Equal(20, appliedOptions.TilesToShineCharge);

            Assert.Same(appliedOptions, arbitraryAnd.AppliedLogicalOptions);
        }

        #endregion
    }
}
