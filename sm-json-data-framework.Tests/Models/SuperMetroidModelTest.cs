using sm_json_data_framework.Models;
using sm_json_data_framework.Models.Connections;
using sm_json_data_framework.Models.Enemies;
using sm_json_data_framework.Models.GameFlags;
using sm_json_data_framework.Models.Helpers;
using sm_json_data_framework.Models.InGameStates;
using sm_json_data_framework.Models.Items;
using sm_json_data_framework.Models.Requirements;
using sm_json_data_framework.Models.Requirements.ObjectRequirements.Integers;
using sm_json_data_framework.Models.Requirements.ObjectRequirements.SubObjects;
using sm_json_data_framework.Models.Requirements.ObjectRequirements.SubRequirements;
using sm_json_data_framework.Models.Requirements.StringRequirements;
using sm_json_data_framework.Models.Rooms;
using sm_json_data_framework.Models.Rooms.Nodes;
using sm_json_data_framework.Models.Techs;
using sm_json_data_framework.Models.Weapons;
using sm_json_data_framework.Options;
using sm_json_data_framework.Rules;
using sm_json_data_framework.Rules.InitialState;
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
        private static SuperMetroidModel ReusableModel() => StaticTestObjects.UnmodifiableModel;
        private static SuperMetroidModel NewModelForOptions() => StaticTestObjects.UnfinalizedModel.Finalize();

        #region Tests for Ctor(SuperMetroidModel)

        [Fact]
        public void Ctor_AssignsAllData()
        {
            // Given  When
            SuperMetroidModel model = ReusableModel();

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
            Assert.Equal(8, model.TechCategories["General"].Techs.Count);
            Assert.Equal(16, model.TechCategories["Movement"].Techs.Count);
            Assert.Equal(25, model.TechCategories["Jumps"].Techs.Count);
            Assert.Equal(13, model.TechCategories["Bomb Jumps"].Techs.Count);
            Assert.Equal(25, model.TechCategories["Enemy-Dependent"].Techs.Count);
            Assert.Equal(5, model.TechCategories["Shots"].Techs.Count);
            Assert.Equal(4, model.TechCategories["Speed Booster"].Techs.Count);
            Assert.Equal(12, model.TechCategories["Miscellaneous"].Techs.Count);
            Assert.Equal(1, model.TechCategories["Meta"].Techs.Count);
            Assert.Equal(109, model.Techs.Count);

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
        public void Ctor_AppliesLogicalOptions()
        {
            // Given
            UnfinalizedSuperMetroidModel unfinalizedModel = StaticTestObjects.UnfinalizedModel;
            UnfinalizedStartConditions startConditions = UnfinalizedStartConditions.CreateVanillaStartConditions(unfinalizedModel);
            startConditions.StartingGameFlags = new List<UnfinalizedGameFlag> { unfinalizedModel.GameFlags["f_ZebesAwake"] };
            LogicalOptions logicalOptions = new LogicalOptions();
            logicalOptions.InternalUnfinalizedStartConditions = startConditions;
            logicalOptions.TilesToShineCharge = 20;

            // When
            SuperMetroidModel model = new SuperMetroidModel(unfinalizedModel, logicalOptions);

            // Expect
            ReadOnlyLogicalOptions appliedOptions = model.Rooms["Landing Site"].AppliedLogicalOptions;
            Assert.NotSame(logicalOptions, appliedOptions);
            Assert.Equal(20, appliedOptions.TilesToShineCharge);
            Assert.Contains("f_ZebesAwake", model.StartConditions.StartingGameFlags.Values.Select(flag => flag.Name));
        }

        [Fact]
        public void Ctor_CreatesOneInstancePerUnfinalizedRoom()
        {
            // Given  When
            SuperMetroidModel model = ReusableModel();

            // Expect all properties that reference a room, to have the same instance as the one in the main model
            Room arbitraryRoom = model.Rooms["Climb"];
            RoomNode node = model.GetNodeInRoom("Climb", 1);
            Assert.Same(arbitraryRoom, node.Room);

            RoomEnvironment roomEnvironment = model.Rooms["Climb"].RoomEnvironments.First();
            Assert.Same(arbitraryRoom, roomEnvironment.Room);

            RoomEnemy roomEnemy = model.RoomEnemies["Climb Pirates"];
            Assert.Same(arbitraryRoom, roomEnemy.Room);

            RoomObstacle roomObstacle = model.Rooms["Climb"].Obstacles["A"];
            Assert.Same(arbitraryRoom, roomObstacle.Room);
        }

        [Fact]
        public void Ctor_CreatesOneInstancePerUnfinalizedNode()
        {
            // Given  When
            SuperMetroidModel model = ReusableModel();

            // Expect all properties that reference a node, to have the same instance as the one in the Room model
            // Nodes in models
            RoomNode arbitraryNode = model.GetNodeInRoom("Big Pink", 14);
            RoomNode node = model.GetNodeInRoom("Big Pink", 5);
            Assert.Same(arbitraryNode, node.SpawnAtNode);

            Link link = model.Rooms["Big Pink"].Links[14];
            Assert.Same(arbitraryNode, link.FromNode);

            LinkTo linkTo = model.Rooms["Big Pink"].Links[5].To[14];
            Assert.Same(arbitraryNode, linkTo.TargetNode);

            Assert.Same(arbitraryNode, model.Nodes["Big Pink X-Ray Climb Setup Junction"]);

            RoomNode arbitraryNode2 = model.GetNodeInRoom("Landing Site", 1);
            CanLeaveCharged canLeaveCharged = model.GetNodeInRoom("Landing Site", 1).CanLeaveCharged.First();
            Assert.Same(arbitraryNode2, canLeaveCharged.Node);

            InitiateRemotely initiateRemotely = model.GetNodeInRoom("Landing Site", 1).CanLeaveCharged.First().InitiateRemotely;
            Assert.Same(arbitraryNode2, initiateRemotely.ExitNode);

            DoorEnvironment doorEnvironment = model.GetNodeInRoom("Landing Site", 1).DoorEnvironments.First();
            Assert.Same(arbitraryNode2, doorEnvironment.Node);

            NodeLock nodeLock = model.Locks["Landing Site Top Left Escape Lock (to Gauntlet)"];
            Assert.Same(arbitraryNode2, nodeLock.Node);

            node = model.GetNodeInRoom("Gauntlet Entrance", 2);
            Assert.Same(arbitraryNode2, node.OutNode);

            Runway runway = model.GetNodeInRoom("Landing Site", 1).Runways["Base Runway - Landing Site Top Left Door (to Gauntlet)"];
            Assert.Same(arbitraryNode2, runway.Node);

            RoomNode arbitraryNode3 = model.GetNodeInRoom("Landing Site", 3);
            initiateRemotely = model.GetNodeInRoom("Landing Site", 1).CanLeaveCharged.First().InitiateRemotely;
            Assert.Same(arbitraryNode3, initiateRemotely.InitiateAtNode);

            RoomNode arbitraryNode4 = model.GetNodeInRoom("Blue Brinstar Energy Tank Room", 3);
            ViewableNode viewableNode = model.GetNodeInRoom("Blue Brinstar Energy Tank Room", 1).ViewableNodes[3];
            Assert.Same(arbitraryNode4, viewableNode.Node);

            RoomNode arbitraryNode5 = model.GetNodeInRoom("Early Supers Room", 3);
            StratFailure stratFailure = model.Rooms["Early Supers Room"].Links[1].To[2].Strats["Early Supers Mockball"].Failures["Crumble Fall"];
            Assert.Same(arbitraryNode5, stratFailure.LeadsToNode);

            RoomNode arbitraryNode6 = model.GetNodeInRoom("Ceres Elevator Room", 1);
            StartConditions startConditions = model.StartConditions;
            Assert.Same(arbitraryNode6, startConditions.StartingNode);

            RoomNode arbitraryNode7 = model.GetNodeInRoom("Volcano Room", 1);
            doorEnvironment = model.GetNodeInRoom("Volcano Room", 2).DoorEnvironments.Where(environment => environment.Physics == PhysicsEnum.Lava).First();
            Assert.Same(arbitraryNode7, doorEnvironment.EntranceNodes[1]);

            RoomEnvironment roomEnvironment = model.Rooms["Volcano Room"].RoomEnvironments.Where(environment => !environment.Heated).First();
            Assert.Same(arbitraryNode7, roomEnvironment.EntranceNodes[1]);

            RoomNode arbitraryNode8 = model.GetNodeInRoom("Construction Zone", 4);
            RoomEnemy roomEnemy = model.RoomEnemies["Construction Zone Geemers"];
            Assert.Same(arbitraryNode8, roomEnemy.HomeNodes[4]);

            RoomNode arbitraryNode9 = model.GetNodeInRoom("Waterway Energy Tank Room", 2);
            roomEnemy = model.RoomEnemies["Waterway Skulteras"];
            Assert.Same(arbitraryNode9, roomEnemy.BetweenNodes[2]);

            // Nodes in logical elements
            // PreviousNode
            RoomNode arbitraryNode10 = model.GetNodeInRoom("Pink Brinstar Power Bomb Room", 4);
            PreviousNode previousNodeLogicalElement = model.Rooms["Pink Brinstar Power Bomb Room"].Links[3].To[4].Strats["Mission Impossible"].Requires.LogicalElement<PreviousNode>(0);
            Assert.Same(arbitraryNode10, previousNodeLogicalElement.Node);

            // AdjacentRunway
            RoomNode arbitraryNode11 = model.GetNodeInRoom("Construction Zone", 3);
            AdjacentRunway adjacentRunway = model.Rooms["Construction Zone"].Links[3].To[4].Strats["Construction Room X-Ray Climb"].Requires.LogicalElement<AdjacentRunway>(0);
            Assert.Same(arbitraryNode11, adjacentRunway.FromNode);
            // ResetRoom
            ResetRoom resetRoom = model.Rooms["Construction Zone"].Links[3].To[4].Strats["Construction Room X-Ray Climb"].Requires.LogicalElement<ResetRoom>(0);
            Assert.Same(arbitraryNode11, resetRoom.Nodes[3]);

            RoomNode arbitraryNode12 = model.GetNodeInRoom("Big Pink", 13);
            resetRoom = model.Rooms["Big Pink"].Links[5].To[4].Strats["Big Pink Left-Side X-Ray Climb"].Requires.LogicalElement<ResetRoom>(0);
            Assert.Same(arbitraryNode12, resetRoom.NodesToAvoid[13]);

            // CanComeInCharged
            RoomNode arbitraryNode13 = model.GetNodeInRoom("Green Brinstar Main Shaft / Etecoon Room", 10);
            CanComeInCharged canComeInCharged = model.Rooms["Green Brinstar Main Shaft / Etecoon Room"].Links[10].To[9].Strats["Shinespark"].Requires.LogicalElement<CanComeInCharged>(0);
            Assert.Same(arbitraryNode13, canComeInCharged.FromNode);
        }

        [Fact]
        public void Ctor_CreatesOneInstancePerUnfinalizedStrat()
        {
            // Given  When
            SuperMetroidModel model = ReusableModel();

            // Expect all properties that reference a strat, to have the same instance as the one in the Room model
            Strat arbitraryStrat = model.Rooms["Landing Site"].Links[3].To[1].Strats["Shinespark"];
            var initiateRemotelyPathToDoorNode = model.GetNodeInRoom("Landing Site", 1).CanLeaveCharged.First().InitiateRemotely.PathToDoor[0];
            Assert.Same(arbitraryStrat, initiateRemotelyPathToDoorNode.strats["Shinespark"]);
        }

        [Fact]
        public void Ctor_CreatesOneInstancePerUnfinalizedEnemy()
        {
            // Given  When
            SuperMetroidModel model = ReusableModel();

            // Expect all properties that reference an enemy, to have the same instance as the one in the main model
            Enemy arbitraryEnemy = model.Enemies["Sidehopper"];
            RoomEnemy roomEnemy = model.RoomEnemies["Morph Ball Room Sidehoppers"];
            Assert.Same(arbitraryEnemy, roomEnemy.Enemy);

            EnemyDamage enemyDamage = model.Rooms["Morph Ball Room"].Links[1].To[6].Strats["Run Through"].Requires.LogicalElement<EnemyDamage>(0);
            Assert.Same(arbitraryEnemy, enemyDamage.Enemy);

            EnemyKill enemyKill = model.Rooms["Morph Ball Room"].Links[1].To[6].Strats["PB Sidehopper Kill with Bomb Blocks"].Obstacles["C"].Requires.LogicalElement<EnemyKill>(0);
            Assert.Same(arbitraryEnemy, enemyKill.GroupedEnemies.First().First());
        }

        [Fact]
        public void Ctor_CreatesOneInstancePerUnfinalizedWeapon()
        {
            // Given  When
            SuperMetroidModel model = ReusableModel();

            // Expect all properties that reference a weapon, to have the same instance as the one in the main model
            Weapon arbitraryWeapon = model.Weapons["Missile"];
            WeaponMultiplier weaponMultiplier = model.Enemies["Alcoon"].WeaponMultipliers["Missile"];
            Assert.Same(arbitraryWeapon, weaponMultiplier.Weapon);

            WeaponSusceptibility weaponSusceptibility = model.Enemies["Alcoon"].WeaponSusceptibilities["Missile"];
            Assert.Same(arbitraryWeapon, weaponSusceptibility.Weapon);

            Enemy enemy = model.Enemies["Boyon"];
            Assert.Same(arbitraryWeapon, enemy.InvulnerableWeapons["Missile"]);

            EnemyKill enemyKill = model.Rooms["Pink Brinstar Power Bomb Room"].Links[1].To[4].Strats["Good Weapon Sidehopper Kill"].Obstacles["A"].Requires.LogicalElement<EnemyKill>(0);
            Assert.Same(arbitraryWeapon, enemyKill.ExplicitWeapons["Missile"]);

            Assert.Same(arbitraryWeapon, enemyKill.ValidWeapons["Missile"]);

            enemyKill = model.Rooms["Metroid Room 1"].Links[1].To[3].Strats["Tank and PB Kill"].Requires.LogicalElement<EnemyKill>(0);
            Assert.Same(arbitraryWeapon, enemyKill.ExcludedWeapons["Missile"]);
        }

        [Fact]
        public void Ctor_CreatesOneInstancePerUnfinalizedWeaponMultiplier()
        {
            // Given  When
            SuperMetroidModel model = ReusableModel();

            // Expect all properties that reference a weaponMultiplier, to have the same instance as the one in the enemy
            WeaponMultiplier arbitraryWeaponMultiplier = model.Enemies["Alcoon"].WeaponMultipliers["Missile"];
            WeaponSusceptibility weaponSusceptibility = model.Enemies["Alcoon"].WeaponSusceptibilities["Missile"];
            Assert.Same(arbitraryWeaponMultiplier, weaponSusceptibility.WeaponMultiplier);
        }

        [Fact]
        public void Ctor_CreatesOneInstancePerUnfinalizedTech()
        {
            // Given  When
            SuperMetroidModel model = ReusableModel();

            // Expect all properties that reference a tech, to have the same instance as the one in the main model
            Tech arbitraryTech = model.Techs["canDelayedWalljump"];
            TechLogicalElement techLogicalElement = model.Rooms["Landing Site"].Links[5].To[7].Strats["Gauntlet Walljumps"].Requires.LogicalElement<TechLogicalElement>(0);
            Assert.Same(arbitraryTech, techLogicalElement.Tech);
            Tech tech = model.Techs["canPreciseWalljump"];
            Assert.Same(arbitraryTech, tech.ExtensionTechs["canDelayedWalljump"]);
        }

        [Fact]
        public void Ctor_CreatesOneInstancePerUnfinalizedHelper()
        {
            // Given  When
            SuperMetroidModel model = ReusableModel();

            // Expect all properties that reference a helper, to have the same instance as the one in the main model
            Helper arbitraryHelper = model.Helpers["h_canDestroyBombWalls"];
            HelperLogicalElement helperLogicalElement = model.Rooms["Landing Site"].Links[1].To[7].Strats["Base"].Obstacles["A"].Requires.LogicalElement<HelperLogicalElement>(0);
            Assert.Same(arbitraryHelper, helperLogicalElement.Helper);
        }

        [Fact]
        public void Ctor_CreatesOneInstancePerUnfinalizedItem()
        {
            // Given  When
            SuperMetroidModel model = ReusableModel();

            // Expect all properties that reference an item, to have the same instance as the one in the main model
            Item arbitraryItem = model.Items["Morph"];
            RoomNode node = model.GetNodeInRoom("Morph Ball Room", 4);
            Assert.Same(arbitraryItem, node.NodeItem);

            ItemLogicalElement itemLogicalElement = model.Rooms["Parlor and Alcatraz"].Links[2].To[8].Strats["Base"].Requires.LogicalElement<ItemLogicalElement>(0);
            Assert.Same(arbitraryItem, itemLogicalElement.Item);
        }

        [Fact]
        public void Ctor_CreatesOneInstancePerUnfinalizedGameFlag()
        {
            // Given  When
            SuperMetroidModel model = ReusableModel();

            // Expect all properties that reference a game flag, to have the same instance as the one in the main model
            GameFlag arbitraryGameFlag = model.GameFlags["f_ZebesAwake"];
            NodeLock nodeLock = model.Locks["Pit Room Left Grey Lock (to Climb)"];
            Assert.Same(arbitraryGameFlag, nodeLock.Yields["f_ZebesAwake"]);

            GameFlag arbitraryGameFlag2 = model.GameFlags["f_DefeatedSporeSpawn"];
            RoomNode node = model.GetNodeInRoom("Spore Spawn Room", 3);
            Assert.Same(arbitraryGameFlag2, node.Yields["f_DefeatedSporeSpawn"]);

            GameFlagLogicalElement gameFlagLogicalElement = model.RoomEnemies["Spore Spawn"].StopSpawn.LogicalElement<GameFlagLogicalElement>(0);
            Assert.Same(arbitraryGameFlag2, gameFlagLogicalElement.GameFlag);
        }

        [Fact]
        public void Ctor_CreatesOneInstancePerUnfinalizedRoomEnemy()
        {
            // Given  When
            SuperMetroidModel model = ReusableModel();

            // Expect all properties that reference a room enemy, to have the same instance as the one in the room
            RoomEnemy arbitraryRoomEnemy = model.Rooms["West Ocean"].Enemies["e1"];
            Assert.Same(arbitraryRoomEnemy, model.RoomEnemies["West Ocean Zeb"]);

            FarmCycle farmCycle = model.RoomEnemies["West Ocean Zeb"].FarmCycles["Crouch over spawn point"];
            Assert.Same(arbitraryRoomEnemy, farmCycle.RoomEnemy);
        }

        [Fact]
        public void Ctor_CreatesOneInstancePerUnfinalizedRoomObstacle()
        {
            // Given  When
            SuperMetroidModel model = ReusableModel();

            // Expect all properties that reference a room obstacle, to have the same instance as the one in the room
            RoomObstacle arbitraryObstacle = model.Rooms["Green Brinstar Main Shaft / Etecoon Room"].Obstacles["A"];
            ResetRoom resetRoom = model.Runways["Base Runway - Green Shaft Mid-Low Left Door (to Firefleas)"].Strats["Base"].Requires.LogicalElement<ResetRoom>(0);
            Assert.Same(arbitraryObstacle, resetRoom.ObstaclesToAvoid["A"]);

            RoomObstacle arbitraryObstacle2 = model.Rooms["Morph Ball Room"].Obstacles["A"];
            StratObstacle stratObstacle = model.Rooms["Morph Ball Room"].Links[1].To[6].Strats["PB Sidehopper Kill with Bomb Blocks"].Obstacles["C"];
            Assert.Same(arbitraryObstacle2, stratObstacle.AdditionalObstacles["A"]);

            RoomObstacle arbitraryObstacle3 = model.Rooms["Morph Ball Room"].Obstacles["C"];
            Assert.Same(arbitraryObstacle3, stratObstacle.Obstacle);
        }

        [Fact]
        public void Ctor_CreatesOneInstancePerUnfinalizedConnection()
        {
            // Given  When
            SuperMetroidModel model = ReusableModel();

            // Expect all properties that reference a connection, to have the same instance as the one in the main model
            Connection arbitraryConnection = model.Connections[model.GetNodeInRoom("Landing Site", 1).IdentifyingString];
            RoomNode node = model.GetNodeInRoom("Landing Site", 1);
            Assert.Same(arbitraryConnection, node.OutConnection);
        }

        [Fact]
        public void Ctor_CreatesOneInstancePerUnfinalizedLinkTo()
        {
            // Given  When
            SuperMetroidModel model = ReusableModel();

            // Expect all properties that reference a LinkTo, to have the same instance as the one in the parent Link
            LinkTo arbitraryLinkTo = model.Rooms["Landing Site"].Links[3].To[1];
            var initiateRemotelyPathToDoorNode = model.GetNodeInRoom("Landing Site", 1).CanLeaveCharged.First().InitiateRemotely.PathToDoor[0];
            Assert.Same(arbitraryLinkTo, initiateRemotelyPathToDoorNode.linkTo);

            RoomNode node = model.GetNodeInRoom("Landing Site", 3);
            Assert.Same(arbitraryLinkTo, node.LinksTo[1]);
        }

        // Then a test for optional stuff??

        #endregion

        #region Tests for ApplyLogicalOptions() that check propagation of the logical options
        [Fact]
        public void ApplyLogicalOptions_AppliesCloneToSuperMetroidModelProperties()
        {
            // Given
            SuperMetroidModel model = NewModelForOptions();
            LogicalOptions logicalOptions = new LogicalOptions();
            logicalOptions.TilesToShineCharge = 20;

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
            SuperMetroidModel model = NewModelForOptions();
            LogicalOptions logicalOptions = new LogicalOptions();
            logicalOptions.TilesToShineCharge = 20;

            // When
            model.ApplyLogicalOptions(logicalOptions);

            // Expect
            Tech arbitraryTech = model.Techs["canWalljump"];
            ReadOnlyLogicalOptions appliedOptions = arbitraryTech.AppliedLogicalOptions;
            Assert.NotSame(logicalOptions, appliedOptions);
            Assert.Equal(20, appliedOptions.TilesToShineCharge);

            Assert.Same(appliedOptions, arbitraryTech.Requires.AppliedLogicalOptions);
        }

        [Fact]
        public void ApplyLogicalOptions_AppliesCloneToHelperProperties()
        {
            // Given
            SuperMetroidModel model = NewModelForOptions();
            LogicalOptions logicalOptions = new LogicalOptions();
            logicalOptions.TilesToShineCharge = 20;

            // When
            model.ApplyLogicalOptions(logicalOptions);

            // Expect
            Helper arbitraryHelper = model.Helpers["h_canOpenZebetites"];
            ReadOnlyLogicalOptions appliedOptions = arbitraryHelper.AppliedLogicalOptions;
            Assert.NotSame(logicalOptions, appliedOptions);
            Assert.Equal(20, appliedOptions.TilesToShineCharge);

            Assert.Same(appliedOptions, arbitraryHelper.Requires.AppliedLogicalOptions);
        }

        [Fact]
        public void ApplyLogicalOptions_AppliesCloneToWeaponProperties()
        {
            // Given
            SuperMetroidModel model = NewModelForOptions();
            LogicalOptions logicalOptions = new LogicalOptions();
            logicalOptions.TilesToShineCharge = 20;

            // When
            model.ApplyLogicalOptions(logicalOptions);

            // Expect
            Weapon arbitraryWeapon = model.Weapons["Wave"];
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
            SuperMetroidModel model = NewModelForOptions();
            LogicalOptions logicalOptions = new LogicalOptions();
            logicalOptions.TilesToShineCharge = 20;

            // When
            model.ApplyLogicalOptions(logicalOptions);

            // Expect
            Enemy arbitraryEnemy = model.Enemies["Geemer (blue)"];
            ReadOnlyLogicalOptions appliedOptions = arbitraryEnemy.AppliedLogicalOptions;
            Assert.NotSame(logicalOptions, appliedOptions);
            Assert.Equal(20, appliedOptions.TilesToShineCharge);

            Assert.Same(appliedOptions, arbitraryEnemy.Attacks["contact"].AppliedLogicalOptions);
        }

        [Fact]
        public void ApplyLogicalOptions_AppliesCloneToConnectionProperties()
        {
            // Given
            SuperMetroidModel model = NewModelForOptions();
            LogicalOptions logicalOptions = new LogicalOptions();
            logicalOptions.TilesToShineCharge = 20;

            // When
            model.ApplyLogicalOptions(logicalOptions);

            // Expect
            Connection arbitraryConnection = model.Connections[model.GetNodeInRoom("Landing Site", 1).IdentifyingString];
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
            SuperMetroidModel model = NewModelForOptions();
            LogicalOptions logicalOptions = new LogicalOptions();
            logicalOptions.TilesToShineCharge = 20;

            // When
            model.ApplyLogicalOptions(logicalOptions);

            // Expect
            Room arbitraryRoom = model.Rooms["Climb"];
            ReadOnlyLogicalOptions appliedOptions = arbitraryRoom.AppliedLogicalOptions;
            Assert.NotSame(logicalOptions, appliedOptions);
            Assert.Equal(20, appliedOptions.TilesToShineCharge);

            Assert.Same(appliedOptions, arbitraryRoom.Nodes[1].AppliedLogicalOptions);
            Assert.Same(appliedOptions, arbitraryRoom.Links[1].AppliedLogicalOptions);
            Assert.Same(appliedOptions, arbitraryRoom.RoomEnvironments.First().AppliedLogicalOptions);
            Assert.Same(appliedOptions, arbitraryRoom.Obstacles["A"].AppliedLogicalOptions);
            Assert.Same(appliedOptions, arbitraryRoom.Enemies["e1"].AppliedLogicalOptions);
        }

        [Fact]
        public void ApplyLogicalOptions_AppliesCloneToLinkProperties()
        {
            // Given
            SuperMetroidModel model = NewModelForOptions();
            LogicalOptions logicalOptions = new LogicalOptions();
            logicalOptions.TilesToShineCharge = 20;

            // When
            model.ApplyLogicalOptions(logicalOptions);

            // Expect
            Link arbitraryLink = model.Rooms["Landing Site"].Links[1];
            ReadOnlyLogicalOptions appliedOptions = arbitraryLink.AppliedLogicalOptions;
            Assert.NotSame(logicalOptions, appliedOptions);
            Assert.Equal(20, appliedOptions.TilesToShineCharge);

            Assert.Same(appliedOptions, arbitraryLink.To[4].AppliedLogicalOptions);
        }

        [Fact]
        public void ApplyLogicalOptions_AppliesCloneToLinkToProperties()
        {
            // Given
            SuperMetroidModel model = NewModelForOptions();
            LogicalOptions logicalOptions = new LogicalOptions();
            logicalOptions.TilesToShineCharge = 20;

            // When
            model.ApplyLogicalOptions(logicalOptions);

            // Expect
            LinkTo arbitraryLinkTo = model.Rooms["Landing Site"].Links[1].To[4];
            ReadOnlyLogicalOptions appliedOptions = arbitraryLinkTo.AppliedLogicalOptions;
            Assert.NotSame(logicalOptions, appliedOptions);
            Assert.Equal(20, appliedOptions.TilesToShineCharge);

            Assert.Same(appliedOptions, arbitraryLinkTo.Strats["Shinespark"].AppliedLogicalOptions);
        }

        [Fact]
        public void ApplyLogicalOptions_AppliesCloneToStratProperties()
        {
            // Given
            SuperMetroidModel model = NewModelForOptions();
            LogicalOptions logicalOptions = new LogicalOptions();
            logicalOptions.TilesToShineCharge = 20;

            // When
            model.ApplyLogicalOptions(logicalOptions);

            // Expect
            Strat arbitraryStrat = model.Rooms["Pink Brinstar Power Bomb Room"].Links[3].To[4].Strats["Mission Impossible"];
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
            SuperMetroidModel model = NewModelForOptions();
            LogicalOptions logicalOptions = new LogicalOptions();
            logicalOptions.TilesToShineCharge = 20;

            // When
            model.ApplyLogicalOptions(logicalOptions);

            // Expect
            RoomObstacle arbitraryRoomObstacle = model.Rooms["Climb"].Obstacles["A"];
            ReadOnlyLogicalOptions appliedOptions = arbitraryRoomObstacle.AppliedLogicalOptions;
            Assert.NotSame(logicalOptions, appliedOptions);
            Assert.Equal(20, appliedOptions.TilesToShineCharge);

            Assert.Same(appliedOptions, arbitraryRoomObstacle.Requires.AppliedLogicalOptions);
        }

        [Fact]
        public void ApplyLogicalOptions_AppliesCloneToRoomEnemyProperties()
        {
            // Given
            SuperMetroidModel model = NewModelForOptions();
            LogicalOptions logicalOptions = new LogicalOptions();
            logicalOptions.TilesToShineCharge = 20;

            // When
            model.ApplyLogicalOptions(logicalOptions);

            // Expect
            RoomEnemy arbitraryRoomEnemy = model.Rooms["Early Supers Room"].Enemies["e1"];
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
            SuperMetroidModel model = NewModelForOptions();
            LogicalOptions logicalOptions = new LogicalOptions();
            logicalOptions.TilesToShineCharge = 20;

            // When
            model.ApplyLogicalOptions(logicalOptions);

            // Expect
            FarmCycle arbitraryFarmCycle = model.Rooms["Early Supers Room"].Enemies["e1"].FarmCycles["Crouch over spawn point"];
            ReadOnlyLogicalOptions appliedOptions = arbitraryFarmCycle.AppliedLogicalOptions;
            Assert.NotSame(logicalOptions, appliedOptions);
            Assert.Equal(20, appliedOptions.TilesToShineCharge);

            Assert.Same(appliedOptions, arbitraryFarmCycle.Requires.AppliedLogicalOptions);
        }

        [Fact]
        public void ApplyLogicalOptions_AppliesCloneToNodeProperties()
        {
            // Given
            SuperMetroidModel model = NewModelForOptions();
            LogicalOptions logicalOptions = new LogicalOptions();
            logicalOptions.TilesToShineCharge = 20;

            // When
            model.ApplyLogicalOptions(logicalOptions);

            // Expect
            RoomNode arbitraryNode = model.GetNodeInRoom("Landing Site", 1);
            ReadOnlyLogicalOptions appliedOptions = arbitraryNode.AppliedLogicalOptions;
            Assert.NotSame(logicalOptions, appliedOptions);
            Assert.Equal(20, appliedOptions.TilesToShineCharge);

            Assert.Same(appliedOptions, arbitraryNode.Locks["Landing Site Top Left Escape Lock (to Gauntlet)"].AppliedLogicalOptions);
            Assert.Same(appliedOptions, arbitraryNode.DoorEnvironments.First().AppliedLogicalOptions);
            Assert.Same(appliedOptions, arbitraryNode.CanLeaveCharged.First().AppliedLogicalOptions);
            Assert.Same(appliedOptions, arbitraryNode.InteractionRequires.AppliedLogicalOptions);
            Assert.Same(appliedOptions, arbitraryNode.Runways["Base Runway - Landing Site Top Left Door (to Gauntlet)"].AppliedLogicalOptions);
            Assert.Same(appliedOptions, model.GetNodeInRoom("Blue Brinstar Energy Tank Room", 1).ViewableNodes[3].AppliedLogicalOptions);
        }

        [Fact]
        public void ApplyLogicalOptions_AppliesCloneToRunwayProperties()
        {
            // Given
            SuperMetroidModel model = NewModelForOptions();
            LogicalOptions logicalOptions = new LogicalOptions();
            logicalOptions.TilesToShineCharge = 20;

            // When
            model.ApplyLogicalOptions(logicalOptions);

            // Expect
            Runway arbitraryRunway = model.GetNodeInRoom("Climb", 5).Runways["Base Runway - Climb Bottom Right Door (to Pit Room)"];
            ReadOnlyLogicalOptions appliedOptions = arbitraryRunway.AppliedLogicalOptions;
            Assert.NotSame(logicalOptions, appliedOptions);
            Assert.Equal(20, appliedOptions.TilesToShineCharge);

            Assert.Same(appliedOptions, arbitraryRunway.Strats["Base"].AppliedLogicalOptions);
        }

        [Fact]
        public void ApplyLogicalOptions_AppliesCloneToCanLeaveChargedProperties()
        {
            // Given
            SuperMetroidModel model = NewModelForOptions();
            LogicalOptions logicalOptions = new LogicalOptions();
            logicalOptions.TilesToShineCharge = 20;

            // When
            model.ApplyLogicalOptions(logicalOptions);

            // Expect
            CanLeaveCharged arbitraryCanLeaveCharged = model.GetNodeInRoom("Landing Site", 1).CanLeaveCharged.First();
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
            SuperMetroidModel model = NewModelForOptions();
            LogicalOptions logicalOptions = new LogicalOptions();
            logicalOptions.TilesToShineCharge = 20;

            // When
            model.ApplyLogicalOptions(logicalOptions);

            // Expect
            ViewableNode arbitraryViewableNode = model.GetNodeInRoom("Blue Brinstar Energy Tank Room", 1).ViewableNodes[3];
            ReadOnlyLogicalOptions appliedOptions = arbitraryViewableNode.AppliedLogicalOptions;
            Assert.NotSame(logicalOptions, appliedOptions);
            Assert.Equal(20, appliedOptions.TilesToShineCharge);

            Assert.Same(appliedOptions, arbitraryViewableNode.Strats["Base"].AppliedLogicalOptions);
        }

        [Fact]
        public void ApplyLogicalOptions_AppliesCloneToNodeLockProperties()
        {
            // Given
            SuperMetroidModel model = NewModelForOptions();
            LogicalOptions logicalOptions = new LogicalOptions();
            logicalOptions.TilesToShineCharge = 20;

            // When
            model.ApplyLogicalOptions(logicalOptions);

            // Expect
            NodeLock arbitraryLock = model.GetNodeInRoom("West Ocean", 4).Locks["West Ocean Ship Exit Grey Lock (to Gravity Suit Room)"];
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
            SuperMetroidModel model = NewModelForOptions();
            LogicalOptions logicalOptions = new LogicalOptions();
            logicalOptions.TilesToShineCharge = 20;

            // When
            model.ApplyLogicalOptions(logicalOptions);

            // Expect
            LogicalRequirements arbitraryLogicalRequirements = model.Rooms["Climb"].Links[6].To[3].Strats["Behemoth Spark Top"].Requires;
            ReadOnlyLogicalOptions appliedOptions = arbitraryLogicalRequirements.AppliedLogicalOptions;
            Assert.NotSame(logicalOptions, appliedOptions);
            Assert.Equal(20, appliedOptions.TilesToShineCharge);

            foreach (ILogicalElement logicalElement in arbitraryLogicalRequirements.LogicalElements)
            {
                Assert.Same(appliedOptions, logicalElement.AppliedLogicalOptions);
            }
        }

        [Fact]
        public void ApplyLogicalOptions_AppliesCloneToOrProperties()
        {
            // Given
            SuperMetroidModel model = NewModelForOptions();
            LogicalOptions logicalOptions = new LogicalOptions();
            logicalOptions.TilesToShineCharge = 20;

            // When
            model.ApplyLogicalOptions(logicalOptions);

            // Expect
            Or arbitraryOr = (Or)model.Helpers["h_canPassBombPassages"].Requires.LogicalElement<Or>(0);
            ReadOnlyLogicalOptions appliedOptions = arbitraryOr.AppliedLogicalOptions;
            Assert.NotSame(logicalOptions, appliedOptions);
            Assert.Equal(20, appliedOptions.TilesToShineCharge);

            Assert.Same(appliedOptions, arbitraryOr.AppliedLogicalOptions);
        }

        [Fact]
        public void ApplyLogicalOptions_AppliesCloneToAndProperties()
        {
            // Given
            SuperMetroidModel model = NewModelForOptions();
            LogicalOptions logicalOptions = new LogicalOptions();
            logicalOptions.TilesToShineCharge = 20;

            // When
            model.ApplyLogicalOptions(logicalOptions);

            // Expect
            And arbitraryAnd = model.Helpers["h_canPassBombPassages"].Requires.LogicalElement<Or>(0)
                .LogicalRequirements.LogicalElement<And>(0);
            ReadOnlyLogicalOptions appliedOptions = arbitraryAnd.AppliedLogicalOptions;
            Assert.NotSame(logicalOptions, appliedOptions);
            Assert.Equal(20, appliedOptions.TilesToShineCharge);

            Assert.Same(appliedOptions, arbitraryAnd.AppliedLogicalOptions);
        }

        #endregion
    }
}
