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
        private static SuperMetroidModel ModelWithOptions = StaticTestObjects.UnfinalizedModel.Finalize();

        #region Tests for Ctor(SuperMetroidModel)

        [Fact]
        public void Ctor_AssignsAllData()
        {
            // Given  When
            SuperMetroidModel model = StaticTestObjects.UnmodifiableModel;

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

            Assert.Equal(8, model.TechCategories.Count);
            Assert.Equal(7, model.TechCategories["General"].Techs.Count);
            Assert.Equal(13, model.TechCategories["Movement"].Techs.Count);
            Assert.Equal(23, model.TechCategories["Jumps"].Techs.Count);
            Assert.Equal(8, model.TechCategories["Bomb Jumps"].Techs.Count);
            Assert.Equal(22, model.TechCategories["Enemy-Dependent"].Techs.Count);
            Assert.Equal(5, model.TechCategories["Shots"].Techs.Count);
            Assert.Equal(4, model.TechCategories["Speed Booster"].Techs.Count);
            Assert.Equal(9, model.TechCategories["Miscellaneous"].Techs.Count);
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
            SuperMetroidModel model = StaticTestObjects.UnmodifiableModel;

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
            SuperMetroidModel model = StaticTestObjects.UnmodifiableModel;

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
            ViewableNode viewableNode = model.GetNodeInRoom("Blue Brinstar Energy Tank Room", 1).ViewableNodes[0];
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
            SuperMetroidModel model = StaticTestObjects.UnmodifiableModel;

            // Expect all properties that reference a strat, to have the same instance as the one in the Room model
            Strat arbitraryStrat = model.Rooms["Landing Site"].Links[3].To[1].Strats["Shinespark"];
            var initiateRemotelyPathToDoorNode = model.GetNodeInRoom("Landing Site", 1).CanLeaveCharged.First().InitiateRemotely.PathToDoor[0];
            Assert.Same(arbitraryStrat, initiateRemotelyPathToDoorNode.strats["Shinespark"]);
        }

        [Fact]
        public void Ctor_CreatesOneInstancePerUnfinalizedEnemy()
        {
            // Given  When
            SuperMetroidModel model = StaticTestObjects.UnmodifiableModel;

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
            SuperMetroidModel model = StaticTestObjects.UnmodifiableModel;

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
            SuperMetroidModel model = StaticTestObjects.UnmodifiableModel;

            // Expect all properties that reference a weaponMultiplier, to have the same instance as the one in the enemy
            WeaponMultiplier arbitraryWeaponMultiplier = model.Enemies["Alcoon"].WeaponMultipliers["Missile"];
            WeaponSusceptibility weaponSusceptibility = model.Enemies["Alcoon"].WeaponSusceptibilities["Missile"];
            Assert.Same(arbitraryWeaponMultiplier, weaponSusceptibility.WeaponMultiplier);
        }

        [Fact]
        public void Ctor_CreatesOneInstancePerUnfinalizedTech()
        {
            // Given  When
            SuperMetroidModel model = StaticTestObjects.UnmodifiableModel;

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
            SuperMetroidModel model = StaticTestObjects.UnmodifiableModel;

            // Expect all properties that reference a helper, to have the same instance as the one in the main model
            Helper arbitraryHelper = model.Helpers["h_canDestroyBombWalls"];
            HelperLogicalElement helperLogicalElement = model.Rooms["Landing Site"].Links[1].To[7].Strats["Base"].Obstacles["A"].Requires.LogicalElement<HelperLogicalElement>(0);
            Assert.Same(arbitraryHelper, helperLogicalElement.Helper);
        }

        [Fact]
        public void Ctor_CreatesOneInstancePerUnfinalizedItem()
        {
            // Given  When
            SuperMetroidModel model = StaticTestObjects.UnmodifiableModel;

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
            SuperMetroidModel model = StaticTestObjects.UnmodifiableModel;

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
            SuperMetroidModel model = StaticTestObjects.UnmodifiableModel;

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
            SuperMetroidModel model = StaticTestObjects.UnmodifiableModel;

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
            SuperMetroidModel model = StaticTestObjects.UnmodifiableModel;

            // Expect all properties that reference a connection, to have the same instance as the one in the main model
            Connection arbitraryConnection = model.Connections[model.GetNodeInRoom("Landing Site", 1).IdentifyingString];
            RoomNode node = model.GetNodeInRoom("Landing Site", 1);
            Assert.Same(arbitraryConnection, node.OutConnection);
        }

        [Fact]
        public void Ctor_CreatesOneInstancePerUnfinalizedLinkTo()
        {
            // Given  When
            SuperMetroidModel model = StaticTestObjects.UnmodifiableModel;

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
            LogicalOptions logicalOptions = new LogicalOptions();
            logicalOptions.TilesToShineCharge = 20;

            // When
            ModelWithOptions.ApplyLogicalOptions(logicalOptions);

            // Expect
            ReadOnlyLogicalOptions appliedOptions = ModelWithOptions.Rooms["Landing Site"].AppliedLogicalOptions;
            Assert.NotSame(logicalOptions, appliedOptions);
            Assert.Equal(20, appliedOptions.TilesToShineCharge);

            Assert.Same(appliedOptions, ModelWithOptions.Weapons["Wave"].AppliedLogicalOptions);
            Assert.Same(appliedOptions, ModelWithOptions.Enemies["Geemer (blue)"].AppliedLogicalOptions);
            Assert.Same(appliedOptions, ModelWithOptions.Items["SpeedBooster"].AppliedLogicalOptions);
            Assert.Same(appliedOptions, ModelWithOptions.GameFlags["f_ZebesSetAblaze"].AppliedLogicalOptions);
            Assert.Same(appliedOptions, ModelWithOptions.Techs["canWalljump"].AppliedLogicalOptions);
            Assert.Same(appliedOptions, ModelWithOptions.Helpers["h_canOpenZebetites"].AppliedLogicalOptions);
            Assert.Same(appliedOptions, ModelWithOptions.Connections[ModelWithOptions.GetNodeInRoom("Landing Site", 1).IdentifyingString].AppliedLogicalOptions);
        }

        [Fact]
        public void ApplyLogicalOptions_AppliesCloneToTechProperties()
        {
            // Given
            LogicalOptions logicalOptions = new LogicalOptions();
            logicalOptions.TilesToShineCharge = 20;

            // When
            ModelWithOptions.ApplyLogicalOptions(logicalOptions);

            // Expect
            Tech arbitraryTech = ModelWithOptions.Techs["canWalljump"];
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

            // When
            ModelWithOptions.ApplyLogicalOptions(logicalOptions);

            // Expect
            Helper arbitraryHelper = ModelWithOptions.Helpers["h_canOpenZebetites"];
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

            // When
            ModelWithOptions.ApplyLogicalOptions(logicalOptions);

            // Expect
            Weapon arbitraryWeapon = ModelWithOptions.Weapons["Wave"];
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

            // When
            ModelWithOptions.ApplyLogicalOptions(logicalOptions);

            // Expect
            Enemy arbitraryEnemy = ModelWithOptions.Enemies["Geemer (blue)"];
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

            // When
            ModelWithOptions.ApplyLogicalOptions(logicalOptions);

            // Expect
            Connection arbitraryConnection = ModelWithOptions.Connections[ModelWithOptions.GetNodeInRoom("Landing Site", 1).IdentifyingString];
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

            // When
            ModelWithOptions.ApplyLogicalOptions(logicalOptions);

            // Expect
            Room arbitraryRoom = ModelWithOptions.Rooms["Climb"];
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
            LogicalOptions logicalOptions = new LogicalOptions();
            logicalOptions.TilesToShineCharge = 20;

            // When
            ModelWithOptions.ApplyLogicalOptions(logicalOptions);

            // Expect
            Link arbitraryLink = ModelWithOptions.Rooms["Landing Site"].Links[1];
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

            // When
            ModelWithOptions.ApplyLogicalOptions(logicalOptions);

            // Expect
            LinkTo arbitraryLinkTo = ModelWithOptions.Rooms["Landing Site"].Links[1].To[4];
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

            // When
            ModelWithOptions.ApplyLogicalOptions(logicalOptions);

            // Expect
            Strat arbitraryStrat = ModelWithOptions.Rooms["Pink Brinstar Power Bomb Room"].Links[3].To[4].Strats["Mission Impossible"];
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

            // When
            ModelWithOptions.ApplyLogicalOptions(logicalOptions);

            // Expect
            RoomObstacle arbitraryRoomObstacle = ModelWithOptions.Rooms["Climb"].Obstacles["A"];
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

            // When
            ModelWithOptions.ApplyLogicalOptions(logicalOptions);

            // Expect
            RoomEnemy arbitraryRoomEnemy = ModelWithOptions.Rooms["Early Supers Room"].Enemies["e1"];
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

            // When
            ModelWithOptions.ApplyLogicalOptions(logicalOptions);

            // Expect
            FarmCycle arbitraryFarmCycle = ModelWithOptions.Rooms["Early Supers Room"].Enemies["e1"].FarmCycles["Crouch over spawn point"];
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

            // When
            ModelWithOptions.ApplyLogicalOptions(logicalOptions);

            // Expect
            RoomNode arbitraryNode = ModelWithOptions.GetNodeInRoom("Landing Site", 1);
            ReadOnlyLogicalOptions appliedOptions = arbitraryNode.AppliedLogicalOptions;
            Assert.NotSame(logicalOptions, appliedOptions);
            Assert.Equal(20, appliedOptions.TilesToShineCharge);

            Assert.Same(appliedOptions, arbitraryNode.Locks["Landing Site Top Left Escape Lock (to Gauntlet)"].AppliedLogicalOptions);
            Assert.Same(appliedOptions, arbitraryNode.DoorEnvironments.First().AppliedLogicalOptions);
            Assert.Same(appliedOptions, arbitraryNode.CanLeaveCharged.First().AppliedLogicalOptions);
            Assert.Same(appliedOptions, arbitraryNode.InteractionRequires.AppliedLogicalOptions);
            Assert.Same(appliedOptions, arbitraryNode.Runways["Base Runway - Landing Site Top Left Door (to Gauntlet)"].AppliedLogicalOptions);
            Assert.Same(appliedOptions, ModelWithOptions.GetNodeInRoom("Blue Brinstar Energy Tank Room", 1).ViewableNodes[0].AppliedLogicalOptions);
        }

        [Fact]
        public void ApplyLogicalOptions_AppliesCloneToRunwayProperties()
        {
            // Given
            LogicalOptions logicalOptions = new LogicalOptions();
            logicalOptions.TilesToShineCharge = 20;

            // When
            ModelWithOptions.ApplyLogicalOptions(logicalOptions);

            // Expect
            Runway arbitraryRunway = ModelWithOptions.GetNodeInRoom("Climb", 5).Runways["Base Runway - Climb Bottom Right Door (to Pit Room)"];
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

            // When
            ModelWithOptions.ApplyLogicalOptions(logicalOptions);

            // Expect
            CanLeaveCharged arbitraryCanLeaveCharged = ModelWithOptions.GetNodeInRoom("Landing Site", 1).CanLeaveCharged.First();
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

            // When
            ModelWithOptions.ApplyLogicalOptions(logicalOptions);

            // Expect
            ViewableNode arbitraryViewableNode = ModelWithOptions.GetNodeInRoom("Blue Brinstar Energy Tank Room", 1).ViewableNodes[0];
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

            // When
            ModelWithOptions.ApplyLogicalOptions(logicalOptions);

            // Expect
            NodeLock arbitraryLock = ModelWithOptions.GetNodeInRoom("West Ocean", 4).Locks["West Ocean Ship Exit Grey Lock (to Gravity Suit Room)"];
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

            // When
            ModelWithOptions.ApplyLogicalOptions(logicalOptions);

            // Expect
            LogicalRequirements arbitraryLogicalRequirements = ModelWithOptions.Rooms["Climb"].Links[6].To[3].Strats["Behemoth Spark Top"].Requires;
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
            LogicalOptions logicalOptions = new LogicalOptions();
            logicalOptions.TilesToShineCharge = 20;

            // When
            ModelWithOptions.ApplyLogicalOptions(logicalOptions);

            // Expect
            Or arbitraryOr = (Or)ModelWithOptions.Helpers["h_canPassBombPassages"].Requires.LogicalElements.Where(element => typeof(Or).IsAssignableFrom(element.GetType())).First();
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

            // When
            ModelWithOptions.ApplyLogicalOptions(logicalOptions);

            // Expect
            And arbitraryAnd = ModelWithOptions.Helpers["h_canPassBombPassages"].Requires.LogicalElement<Or>(0)
                .LogicalRequirements.LogicalElement<And>(0);
            ReadOnlyLogicalOptions appliedOptions = arbitraryAnd.AppliedLogicalOptions;
            Assert.NotSame(logicalOptions, appliedOptions);
            Assert.Equal(20, appliedOptions.TilesToShineCharge);

            Assert.Same(appliedOptions, arbitraryAnd.AppliedLogicalOptions);
        }

        #endregion

        #region Tests for ApplyLogicalOptions() that check applied logical properties

        // Tests in this section belong more in individual classes' test, we can move them when those classes get some focus on their tests

        [Fact]
        public void ApplyLogicalOptions_SetsLogicalPropertiesOnRooms()
        {
            // Given
            LogicalOptions logicalOptions = new LogicalOptions();
            logicalOptions.InternalAvailableResourceInventory = new ResourceItemInventory(ResourceCount.CreateVanillaBaseResourceMaximums())
                .ApplyAddExpansionItem((ExpansionItem)ModelWithOptions.Items["Missile"], 46)
                .ApplyAddExpansionItem((ExpansionItem)ModelWithOptions.Items["Super"], 10)
                .ApplyAddExpansionItem((ExpansionItem)ModelWithOptions.Items["ETank"], 14)
                .ApplyAddExpansionItem((ExpansionItem)ModelWithOptions.Items["ReserveTank"], 4);

            // When
            ModelWithOptions.ApplyLogicalOptions(logicalOptions);

            // Expect
            Room unreachableRoom = ModelWithOptions.Rooms["Norfair Map Room"];
            // Room accessibility is not considered in-scope for logical relevance
            Assert.True(unreachableRoom.LogicallyRelevant);

            Room reachableRoom = ModelWithOptions.Rooms["Landing Site"];
            Assert.True(reachableRoom.LogicallyRelevant);
        }

        [Fact]
        public void ApplyLogicalOptions_ImpossibleObstacleCommonRequirements_SetsLogicalPropertiesOnRoomObstacles()
        {
            // Given
            LogicalOptions logicalOptions = new LogicalOptions()
                .RegisterRemovedItem(SuperMetroidModel.SPEED_BOOSTER_NAME)
                .RegisterRemovedItem("ScrewAttack")
                .RegisterRemovedItem("Morph")
                .RegisterDisabledGameFlag("f_ZebesSetAblaze");
            logicalOptions.InternalAvailableResourceInventory = new ResourceItemInventory(ResourceCount.CreateVanillaBaseResourceMaximums())
                .ApplyAddExpansionItem((ExpansionItem)ModelWithOptions.Items["Missile"], 46)
                .ApplyAddExpansionItem((ExpansionItem)ModelWithOptions.Items["Super"], 10)
                .ApplyAddExpansionItem((ExpansionItem)ModelWithOptions.Items["ETank"], 14)
                .ApplyAddExpansionItem((ExpansionItem)ModelWithOptions.Items["ReserveTank"], 4);

            // When
            ModelWithOptions.ApplyLogicalOptions(logicalOptions);

            // Expect
            RoomObstacle obstacle = ModelWithOptions.Rooms["Climb"].Obstacles["A"];
            Assert.True(obstacle.LogicallyRelevant);
            Assert.True(obstacle.LogicallyIndestructible);
            Assert.False(obstacle.LogicallyAlwaysDestructible);
            Assert.False(obstacle.LogicallyDestructibleForFree);
        }

        [Fact]
        public void ApplyLogicalOptions_FreeObstacleCommonRequirements_SetsLogicalPropertiesOnRoomObstacles()
        {
            // Given
            LogicalOptions logicalOptions = new LogicalOptions();
            logicalOptions.InternalStartConditions = StartConditions.CreateVanillaStartConditionsBuilder(ModelWithOptions).StartingInventory(
                ItemInventory.CreateVanillaStartingInventory(ModelWithOptions)
                    .ApplyAddItem(ModelWithOptions.Items["ScrewAttack"])
            )
            .Build();

            // When
            ModelWithOptions.ApplyLogicalOptions(logicalOptions);

            // Expect
            RoomObstacle obstacle = ModelWithOptions.Rooms["Climb"].Obstacles["A"];
            Assert.True(obstacle.LogicallyRelevant);
            Assert.False(obstacle.LogicallyIndestructible);
            Assert.True(obstacle.LogicallyAlwaysDestructible);
            Assert.True(obstacle.LogicallyDestructibleForFree);
        }

        [Fact]
        public void ApplyLogicalOptions_SetsLogicalPropertiesOnStrats()
        {
            // Given
            LogicalOptions logicalOptions = new LogicalOptions()
                .RegisterRemovedItem("Grapple")
                .RegisterDisabledStrat("Ceiling E-Tank Dboost");

            // When
            ModelWithOptions.ApplyLogicalOptions(logicalOptions);

            // Expect
            Strat impossibleRequirementsStrat = ModelWithOptions.Rooms["Pants Room"].Links[4].To[5].Strats["Base"];
            Assert.False(impossibleRequirementsStrat.LogicallyRelevant);
            Assert.False(impossibleRequirementsStrat.LogicallyAlways);
            Assert.False(impossibleRequirementsStrat.LogicallyFree);
            Assert.True(impossibleRequirementsStrat.LogicallyNever);

            Strat disabledStrat = ModelWithOptions.Rooms["Blue Brinstar Energy Tank Room"].Links[1].To[3].Strats["Ceiling E-Tank Dboost"];
            Assert.False(disabledStrat.LogicallyRelevant);
            Assert.False(disabledStrat.LogicallyAlways);
            Assert.False(disabledStrat.LogicallyFree);
            Assert.True(disabledStrat.LogicallyNever);

            Strat nonFreeStrat = ModelWithOptions.Rooms["Blue Brinstar Energy Tank Room"].Links[1].To[3].Strats["Ceiling E-Tank Speed Jump"];
            Assert.True(nonFreeStrat.LogicallyRelevant);
            Assert.False(nonFreeStrat.LogicallyAlways);
            Assert.False(nonFreeStrat.LogicallyFree);
            Assert.False(nonFreeStrat.LogicallyNever);

            Strat freeStrat = ModelWithOptions.Rooms["Landing Site"].Links[5].To[4].Strats["Base"];
            Assert.True(freeStrat.LogicallyRelevant);
            Assert.True(freeStrat.LogicallyAlways);
            Assert.True(freeStrat.LogicallyFree);
            Assert.False(freeStrat.LogicallyNever);
        }

        [Fact]
        public void ApplyLogicalOptions_SetsLogicalPropertiesOnStratFailures()
        {

            // Given
            LogicalOptions logicalOptions = new LogicalOptions();

            // When
            ModelWithOptions.ApplyLogicalOptions(logicalOptions);

            // Expect
            StratFailure stratFailure = ModelWithOptions.Rooms["Early Supers Room"].Links[3].To[1].Strats["Early Supers Quick Crumble Escape (Space)"].Failures["Crumble Failure"];
            Assert.True(stratFailure.LogicallyRelevant);
        }

        [Fact]
        public void ApplyLogicalOptions_SetsLogicalPropertiesOnStratObstacles()
        {
            // Given
            LogicalOptions logicalOptions = new LogicalOptions()
                .RegisterRemovedItem(SuperMetroidModel.SPEED_BOOSTER_NAME)
                .RegisterRemovedItem("ScrewAttack")
                .RegisterRemovedItem("Bombs")
                .RegisterDisabledGameFlag("f_ZebesSetAblaze");
            logicalOptions.InternalAvailableResourceInventory = new ResourceItemInventory(ResourceCount.CreateVanillaBaseResourceMaximums())
                .ApplyAddExpansionItem((ExpansionItem)ModelWithOptions.Items["Missile"], 46)
                .ApplyAddExpansionItem((ExpansionItem)ModelWithOptions.Items["Super"], 10)
                .ApplyAddExpansionItem((ExpansionItem)ModelWithOptions.Items["ETank"], 14)
                .ApplyAddExpansionItem((ExpansionItem)ModelWithOptions.Items["ReserveTank"], 4);
            logicalOptions.InternalStartConditions = StartConditions.CreateVanillaStartConditionsBuilder(ModelWithOptions).StartingInventory(
                ItemInventory.CreateVanillaStartingInventory(ModelWithOptions)
                    .ApplyAddItem(ModelWithOptions.Items["Morph"])
                )
                .Build();

            // When
            ModelWithOptions.ApplyLogicalOptions(logicalOptions);

            // Expect
            StratObstacle fullyImpossibleStratObstacle = ModelWithOptions.Rooms["Climb"].Links[2].To[6].Strats["Base"].Obstacles["A"];
            Assert.True(fullyImpossibleStratObstacle.LogicallyRelevant);
            Assert.True(fullyImpossibleStratObstacle.LogicallyNever);
            Assert.True(fullyImpossibleStratObstacle.LogicallyNeverFromHere);
            Assert.False(fullyImpossibleStratObstacle.LogicallyAlways);
            Assert.False(fullyImpossibleStratObstacle.LogicallyFree);

            StratObstacle locallyIndestructibleFreeToBypassStratObstacle = ModelWithOptions.Rooms["Post Crocomire Jump Room"].Links[5].To[1].Strats["PCJR Frozen Mella Door"].Obstacles["B"];
            Assert.True(locallyIndestructibleFreeToBypassStratObstacle.LogicallyRelevant);
            Assert.False(locallyIndestructibleFreeToBypassStratObstacle.LogicallyNever);
            Assert.False(locallyIndestructibleFreeToBypassStratObstacle.LogicallyNeverFromHere);
            Assert.True(locallyIndestructibleFreeToBypassStratObstacle.LogicallyAlways);
            Assert.True(locallyIndestructibleFreeToBypassStratObstacle.LogicallyFree);

            StratObstacle locallyImpossibleStratObstacle = ModelWithOptions.Rooms["Post Crocomire Jump Room"].Nodes[2].CanLeaveCharged.First().Strats["Speed Blocks Broken"].Obstacles["B"];
            Assert.True(locallyImpossibleStratObstacle.LogicallyRelevant);
            Assert.False(locallyImpossibleStratObstacle.LogicallyNever);
            Assert.True(locallyImpossibleStratObstacle.LogicallyNeverFromHere);
            Assert.False(locallyImpossibleStratObstacle.LogicallyAlways);
            Assert.False(locallyImpossibleStratObstacle.LogicallyFree);

            StratObstacle freeToDestroyStratObstacle = ModelWithOptions.Rooms["Pink Brinstar Hopper Room"].Links[2].To[1].Strats["Base"].Obstacles["B"];
            Assert.True(freeToDestroyStratObstacle.LogicallyRelevant);
            Assert.False(freeToDestroyStratObstacle.LogicallyNever);
            Assert.False(freeToDestroyStratObstacle.LogicallyNeverFromHere);
            Assert.True(freeToDestroyStratObstacle.LogicallyAlways);
            Assert.True(freeToDestroyStratObstacle.LogicallyFree);
        }

        [Fact]
        public void ApplyLogicalOptions_SetsLogicalPropertiesOnLinks()
        {
            // Given
            LogicalOptions logicalOptions = new LogicalOptions()
                .RegisterRemovedItem("Gravity")
                .RegisterDisabledTech("canSuitlessMaridia");

            // When
            ModelWithOptions.ApplyLogicalOptions(logicalOptions);

            // Expect
            Link noDestinationsLink = ModelWithOptions.Rooms["Crab Shaft"].Links[2];
            Assert.False(noDestinationsLink.LogicallyRelevant);

            Link possibleLink = ModelWithOptions.Rooms["Landing Site"].Links[1];
            Assert.True(possibleLink.LogicallyRelevant);
        }

        [Fact]
        public void ApplyLogicalOptions_SetsLogicalPropertiesOnLinkTos()
        {
            // Given
            LogicalOptions logicalOptions = new LogicalOptions()
                .RegisterRemovedItem("Gravity")
                .RegisterDisabledTech("canSuitlessMaridia");

            // When
            ModelWithOptions.ApplyLogicalOptions(logicalOptions);

            // Expect
            LinkTo noDestinationsLinkTo = ModelWithOptions.Rooms["Crab Shaft"].Links[2].To[1];
            Assert.False(noDestinationsLinkTo.LogicallyRelevant);
            Assert.False(noDestinationsLinkTo.LogicallyAlways);
            Assert.False(noDestinationsLinkTo.LogicallyFree);
            Assert.True(noDestinationsLinkTo.LogicallyNever);

            LinkTo possibleLinkTo = ModelWithOptions.Rooms["Landing Site"].Links[1].To[7];
            Assert.True(possibleLinkTo.LogicallyRelevant);
            Assert.False(possibleLinkTo.LogicallyAlways);
            Assert.False(possibleLinkTo.LogicallyFree);
            Assert.False(possibleLinkTo.LogicallyNever);

            LinkTo freeLinkTo = ModelWithOptions.Rooms["Landing Site"].Links[5].To[4];
            Assert.True(freeLinkTo.LogicallyRelevant);
            Assert.True(freeLinkTo.LogicallyAlways);
            Assert.True(freeLinkTo.LogicallyFree);
            Assert.False(freeLinkTo.LogicallyNever);
        }

        [Fact]
        public void ApplyLogicalOptions_SetsLogicalPropertiesOnRoomEnemies()
        {
            // Given
            LogicalOptions logicalOptions = new LogicalOptions()
                .RegisterRemovedItem("Bombs");
            logicalOptions.InternalStartConditions = StartConditions.CreateVanillaStartConditionsBuilder(ModelWithOptions)
                .StartingGameFlags(new List<GameFlag> { ModelWithOptions.GameFlags["f_DefeatedRidley"] })
                .Build();

            // When
            ModelWithOptions.ApplyLogicalOptions(logicalOptions);

            // Expect
            RoomEnemy alwaysSpawns = ModelWithOptions.RoomEnemies["Post Crocomire Farming Room Ripper 2"];
            Assert.True(alwaysSpawns.LogicallyRelevant);
            Assert.True(alwaysSpawns.LogicallyAlwaysSpawns);
            Assert.False(alwaysSpawns.LogicallyNeverSpawns);

            RoomEnemy neverMeetsSpawnConditions = ModelWithOptions.RoomEnemies["Bomb Torizo"];
            Assert.False(neverMeetsSpawnConditions.LogicallyRelevant);
            Assert.False(neverMeetsSpawnConditions.LogicallyAlwaysSpawns);
            Assert.True(neverMeetsSpawnConditions.LogicallyNeverSpawns);

            RoomEnemy alwaysMeetsStopSpawnConditions = ModelWithOptions.RoomEnemies["Ridley"];
            Assert.False(alwaysMeetsStopSpawnConditions.LogicallyRelevant);
            Assert.False(alwaysMeetsStopSpawnConditions.LogicallyAlwaysSpawns);
            Assert.True(alwaysMeetsStopSpawnConditions.LogicallyNeverSpawns);

            RoomEnemy notAlwaysSpawnConditions = ModelWithOptions.RoomEnemies["Attic Atomics"];
            Assert.True(notAlwaysSpawnConditions.LogicallyRelevant);
            Assert.False(notAlwaysSpawnConditions.LogicallyAlwaysSpawns);
            Assert.False(notAlwaysSpawnConditions.LogicallyNeverSpawns);

            RoomEnemy alwaysSpawnNotAlwaysStopSpawnConditions = ModelWithOptions.RoomEnemies["Flyway Mellows"];
            Assert.True(alwaysSpawnNotAlwaysStopSpawnConditions.LogicallyRelevant);
            Assert.False(alwaysSpawnNotAlwaysStopSpawnConditions.LogicallyAlwaysSpawns);
            Assert.False(alwaysSpawnNotAlwaysStopSpawnConditions.LogicallyNeverSpawns);
        }

        [Fact]
        public void ApplyLogicalOptions_SetsLogicalPropertiesOnRoomEnvironments()
        {
            // Given
            LogicalOptions logicalOptions = new LogicalOptions();

            // When
            ModelWithOptions.ApplyLogicalOptions(logicalOptions);

            // Expect
            foreach (RoomEnvironment roomEnvironment in ModelWithOptions.Rooms["Volcano Room"].RoomEnvironments)
            {
                Assert.True(roomEnvironment.LogicallyRelevant);
            }
        }

        [Fact]
        public void ApplyLogicalOptions_SetsLogicalPropertiesOnFarmCycles()
        {
            // Given
            LogicalOptions logicalOptions = new LogicalOptions()
                .RegisterRemovedItem("Grapple");

            // When
            ModelWithOptions.ApplyLogicalOptions(logicalOptions);

            // Expect
            FarmCycle impossibleCycle = ModelWithOptions.RoomEnemies["Gauntlet E-Tank Zebbo"].FarmCycles["Grapple three tiles away"];
            Assert.False(impossibleCycle.LogicallyRelevant);
            Assert.True(impossibleCycle.LogicallyNever);

            FarmCycle possibleCycle = ModelWithOptions.RoomEnemies["Gauntlet E-Tank Zebbo"].FarmCycles["Shoot and jump three tiles away"];
            Assert.True(possibleCycle.LogicallyRelevant);
            Assert.False(possibleCycle.LogicallyNever);
        }

        [Fact]
        public void ApplyLogicalOptions_SetsLogicalPropertiesOnNodes()
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

        [Fact]
        public void ApplyLogicalOptions_SetsLogicalPropertiesOnDoorEnvironments()
        {
            // Given
            LogicalOptions logicalOptions = new LogicalOptions();

            // When
            ModelWithOptions.ApplyLogicalOptions(logicalOptions);

            // Expect
            foreach (DoorEnvironment doorEnvironment in ModelWithOptions.Rooms["Volcano Room"].Nodes[2].DoorEnvironments)
            {
                Assert.True(doorEnvironment.LogicallyRelevant);
            }
        }

        [Fact]
        public void ApplyLogicalOptions_SetsLogicalPropertiesOnRunways()
        {
            // Given
            LogicalOptions logicalOptions = new LogicalOptions()
                .RegisterRemovedItem(SuperMetroidModel.GRAVITY_SUIT_NAME);
            logicalOptions.InternalStartConditions = StartConditions.CreateVanillaStartConditionsBuilder(ModelWithOptions).StartingInventory(
                ItemInventory.CreateVanillaStartingInventory(ModelWithOptions)
                    .ApplyAddItem(ModelWithOptions.Items[SuperMetroidModel.VARIA_SUIT_NAME])
                )
                .Build();

            // When
            ModelWithOptions.ApplyLogicalOptions(logicalOptions);

            // Expect
            Runway freeRunway = ModelWithOptions.GetNodeInRoom("Fast Pillars Setup Room", 2).Runways["Base Runway - Fast Pillars Setup Room Bottom Left Door (to Fast Rippers)"];
            Assert.True(freeRunway.LogicallyRelevant);
            Assert.True(freeRunway.LogicallyAlways);
            Assert.True(freeRunway.LogicallyFree);
            Assert.False(freeRunway.LogicallyNever);
            Assert.Equal(2 * (32M / 27M) + 10, freeRunway.LogicalEffectiveRunwayLength);
            Assert.Equal(2 * (32M / 27M) + 10, freeRunway.LogicalEffectiveReversibleRunwayLength);
            Assert.Equal(2 * (32M / 27M) + 10, freeRunway.LogicalEffectiveRunwayLengthNoCharge);

            Runway neverRunway = ModelWithOptions.GetNodeInRoom("Oasis", 2).Runways["Base Runway - Oasis Right Door (to East Sand Hall)"];
            Assert.False(neverRunway.LogicallyRelevant);
            Assert.False(neverRunway.LogicallyAlways);
            Assert.False(neverRunway.LogicallyFree);
            Assert.True(neverRunway.LogicallyNever);
            Assert.Equal(12, neverRunway.LogicalEffectiveRunwayLength);
            Assert.Equal(12, neverRunway.LogicalEffectiveReversibleRunwayLength);
            Assert.Equal(12, neverRunway.LogicalEffectiveRunwayLengthNoCharge);

            Runway possibleRunway = ModelWithOptions.GetNodeInRoom("Golden Torizo's Room", 2).Runways["Base Runway - Golden Torizo Room Right Door (to Screw Attack)"];
            Assert.True(possibleRunway.LogicallyRelevant);
            Assert.False(possibleRunway.LogicallyAlways);
            Assert.False(possibleRunway.LogicallyFree);
            Assert.False(possibleRunway.LogicallyNever);
            Assert.Equal(28, possibleRunway.LogicalEffectiveRunwayLength);
            Assert.Equal(28, possibleRunway.LogicalEffectiveReversibleRunwayLength);
            Assert.Equal(28, possibleRunway.LogicalEffectiveRunwayLengthNoCharge);
        }

        [Fact]
        public void ApplyLogicalOptions_SpeedBoosterPossible_SetsLogicalPropertiesOnCanLeaveChargeds()
        {
            // Given
            LogicalOptions logicalOptions = new LogicalOptions()
                .RegisterRemovedItem("Morph")
                .RegisterRemovedItem("Gravity")
                .RegisterDisabledTech("canShinespark");
            logicalOptions.TilesSavedWithStutter = 0;
            logicalOptions.TilesToShineCharge = 19;

            // When
            ModelWithOptions.ApplyLogicalOptions(logicalOptions);

            // Expect
            CanLeaveCharged neverByImpossibleRemote = ModelWithOptions.Rooms["Warehouse Kihunter Room"].Nodes[3].CanLeaveCharged.First();
            Assert.False(neverByImpossibleRemote.LogicallyRelevant);
            Assert.False(neverByImpossibleRemote.LogicallyAlways);
            Assert.False(neverByImpossibleRemote.LogicallyFree);
            Assert.True(neverByImpossibleRemote.LogicallyNever);
            Assert.Equal(31.5M, neverByImpossibleRemote.LogicalEffectiveRunwayLength);

            CanLeaveCharged neverByImpossibleStrat = ModelWithOptions.Rooms["Mt. Everest"].Nodes[3].CanLeaveCharged.First();
            Assert.False(neverByImpossibleStrat.LogicallyRelevant);
            Assert.False(neverByImpossibleStrat.LogicallyAlways);
            Assert.False(neverByImpossibleStrat.LogicallyFree);
            Assert.True(neverByImpossibleStrat.LogicallyNever);
            Assert.Equal(20.5M, neverByImpossibleStrat.LogicalEffectiveRunwayLength);

            CanLeaveCharged neverByImpossibleShinespark = ModelWithOptions.Rooms["Spore Spawn Farming Room"].Nodes[1].CanLeaveCharged.First();
            Assert.False(neverByImpossibleShinespark.LogicallyRelevant);
            Assert.False(neverByImpossibleShinespark.LogicallyAlways);
            Assert.False(neverByImpossibleShinespark.LogicallyFree);
            Assert.True(neverByImpossibleShinespark.LogicallyNever);
            Assert.Equal(19M + 2 * 4M / 3M, neverByImpossibleShinespark.LogicalEffectiveRunwayLength);

            CanLeaveCharged neverByShortRunway = ModelWithOptions.Rooms["Green Brinstar Main Shaft / Etecoon Room"].Nodes[9].CanLeaveCharged.First();
            Assert.False(neverByShortRunway.LogicallyRelevant);
            Assert.False(neverByShortRunway.LogicallyAlways);
            Assert.False(neverByShortRunway.LogicallyFree);
            Assert.True(neverByShortRunway.LogicallyNever);
            Assert.Equal(17.5M, neverByShortRunway.LogicalEffectiveRunwayLength);

            CanLeaveCharged notFreeBecauseSpeedNotFree = ModelWithOptions.Rooms["Morph Ball Room"].Nodes[3].CanLeaveCharged.First();
            Assert.True(notFreeBecauseSpeedNotFree.LogicallyRelevant);
            Assert.False(notFreeBecauseSpeedNotFree.LogicallyAlways);
            Assert.False(notFreeBecauseSpeedNotFree.LogicallyFree);
            Assert.False(notFreeBecauseSpeedNotFree.LogicallyNever);
            Assert.Equal(30, notFreeBecauseSpeedNotFree.LogicalEffectiveRunwayLength);
        }

        [Fact]
        public void ApplyLogicalOptions_SpeedBoosterRemoved_SetsLogicalPropertiesOnViewableNodes()
        {
            // Given
            LogicalOptions logicalOptions = new LogicalOptions()
                .RegisterRemovedItem("SpeedBooster");

            // When
            ModelWithOptions.ApplyLogicalOptions(logicalOptions);

            // Expect
            CanLeaveCharged neverByNoSpeedBooster = ModelWithOptions.Rooms["Morph Ball Room"].Nodes[3].CanLeaveCharged.First();
            Assert.False(neverByNoSpeedBooster.LogicallyRelevant);
            Assert.False(neverByNoSpeedBooster.LogicallyAlways);
            Assert.False(neverByNoSpeedBooster.LogicallyFree);
            Assert.True(neverByNoSpeedBooster.LogicallyNever);
            Assert.Equal(30, neverByNoSpeedBooster.LogicalEffectiveRunwayLength);
        }
        [Fact]
        public void ApplyLogicalOptions_SpeedBoosterFree_SetsLogicalPropertiesOnViewableNodes()
        {
            // Given
            LogicalOptions logicalOptions = new LogicalOptions();
            logicalOptions.InternalStartConditions = StartConditions.CreateVanillaStartConditionsBuilder(ModelWithOptions).StartingInventory(
                ItemInventory.CreateVanillaStartingInventory(ModelWithOptions)
                    .ApplyAddItem(ModelWithOptions.Items[SuperMetroidModel.SPEED_BOOSTER_NAME])
                    .ApplyAddItem(ModelWithOptions.Items[SuperMetroidModel.GRAVITY_SUIT_NAME])
                )
                .Build();

            // When
            ModelWithOptions.ApplyLogicalOptions(logicalOptions);

            // Expect
            CanLeaveCharged free = ModelWithOptions.Rooms["Mt. Everest"].Nodes[3].CanLeaveCharged.First();
            Assert.True(free.LogicallyRelevant);
            Assert.True(free.LogicallyAlways);
            Assert.True(free.LogicallyFree);
            Assert.False(free.LogicallyNever);
            Assert.Equal(20.5M, free.LogicalEffectiveRunwayLength);

            // Expect
            CanLeaveCharged notFreeBecauseShinespark = ModelWithOptions.Rooms["Spore Spawn Farming Room"].Nodes[1].CanLeaveCharged.First();
            Assert.True(notFreeBecauseShinespark.LogicallyRelevant);
            Assert.False(notFreeBecauseShinespark.LogicallyAlways);
            Assert.False(notFreeBecauseShinespark.LogicallyFree);
            Assert.False(notFreeBecauseShinespark.LogicallyNever);
            Assert.Equal(19M + 2 * 4M / 3M, notFreeBecauseShinespark.LogicalEffectiveRunwayLength);

            // Expect
            CanLeaveCharged notFreeBecauseStratNotFree = ModelWithOptions.Rooms["Botwoon's Room"].Nodes[1].CanLeaveCharged.First();
            Assert.True(notFreeBecauseStratNotFree.LogicallyRelevant);
            Assert.False(notFreeBecauseStratNotFree.LogicallyAlways);
            Assert.False(notFreeBecauseStratNotFree.LogicallyFree);
            Assert.False(notFreeBecauseStratNotFree.LogicallyNever);
            Assert.Equal(16, notFreeBecauseStratNotFree.LogicalEffectiveRunwayLength);

            // Expect
            CanLeaveCharged notFreeBecauseRemoteNotFree = ModelWithOptions.Rooms["Red Brinstar Fireflea Room"].Nodes[1].CanLeaveCharged.First();
            Assert.True(notFreeBecauseRemoteNotFree.LogicallyRelevant);
            Assert.False(notFreeBecauseRemoteNotFree.LogicallyAlways);
            Assert.False(notFreeBecauseRemoteNotFree.LogicallyFree);
            Assert.False(notFreeBecauseRemoteNotFree.LogicallyNever);
            Assert.Equal(13, notFreeBecauseRemoteNotFree.LogicalEffectiveRunwayLength);
        }

        [Fact]
        public void ApplyLogicalOptions_SetsLogicalPropertiesOnTwinDoorAddresses()
        {
            // Given
            LogicalOptions logicalOptions = new LogicalOptions();

            // When
            ModelWithOptions.ApplyLogicalOptions(logicalOptions);

            // Expect
            TwinDoorAddress twinDoorAddress = ModelWithOptions.Rooms["East Pants Room"].Nodes[2].TwinDoorAddresses.First();
            Assert.False(twinDoorAddress.LogicallyRelevant);
        }

        [Fact]
        public void ApplyLogicalOptions_SetsLogicalPropertiesOnNodeLocks()
        {
            // Given
            LogicalOptions logicalOptions = new LogicalOptions()
                .RegisterRemovedItem("Bombs");
            logicalOptions.InternalStartConditions = StartConditions.CreateVanillaStartConditionsBuilder(ModelWithOptions)
                .StartingGameFlags(new List<GameFlag> { ModelWithOptions.GameFlags["f_ZebesAwake"] })
                .Build();

            // When
            ModelWithOptions.ApplyLogicalOptions(logicalOptions);

            // Expect
            NodeLock neverActiveLock = ModelWithOptions.Rooms["Bomb Torizo Room"].Nodes[1].Locks["Bomb Torizo Room Grey Lock (to Flyway)"];
            Assert.False(neverActiveLock.LogicallyRelevant);
            Assert.False(neverActiveLock.LogicallyNever);
            Assert.True(neverActiveLock.LogicallyAlways);
            Assert.True(neverActiveLock.LogicallyFree);

            NodeLock unpassableLock = ModelWithOptions.Rooms["Green Brinstar Main Shaft / Etecoon Room"].Nodes[7].Locks["Etecoon Exit Grey Lock"];
            Assert.True(unpassableLock.LogicallyRelevant);
            Assert.True(unpassableLock.LogicallyNever);
            Assert.False(unpassableLock.LogicallyAlways);
            Assert.False(unpassableLock.LogicallyFree);

            NodeLock greyPossibleBypassableLock = ModelWithOptions.Rooms["West Ocean"].Nodes[4].Locks["West Ocean Ship Exit Grey Lock (to Gravity Suit Room)"];
            Assert.True(greyPossibleBypassableLock.LogicallyRelevant);
            Assert.False(greyPossibleBypassableLock.LogicallyNever);
            Assert.False(greyPossibleBypassableLock.LogicallyAlways);
            Assert.False(greyPossibleBypassableLock.LogicallyFree);

            NodeLock freeUnlockLock = ModelWithOptions.Rooms["Morph Ball Room"].Nodes[5].Locks["Blue Brinstar Power Bombs Spawn Lock"];
            Assert.True(freeUnlockLock.LogicallyRelevant);
            Assert.False(freeUnlockLock.LogicallyNever);
            Assert.True(freeUnlockLock.LogicallyAlways);
            Assert.True(freeUnlockLock.LogicallyFree);

            NodeLock possibleUnlockableLock = ModelWithOptions.Rooms["Construction Zone"].Nodes[2].Locks["Construction Zone Red Lock (to Ceiling E-Tank)"];
            Assert.True(possibleUnlockableLock.LogicallyRelevant);
            Assert.False(possibleUnlockableLock.LogicallyNever);
            Assert.False(possibleUnlockableLock.LogicallyAlways);
            Assert.False(possibleUnlockableLock.LogicallyFree);
        }

        [Fact]
        public void ApplyLogicalOptions_SetsLogicalPropertiesOnLogicalRequirements()
        {
            // Given
            LogicalOptions logicalOptions = new LogicalOptions()
                .RegisterDisabledGameFlag("f_ZebesAwake")
                .RegisterDisabledTech("canCrouchJump")
                .RegisterDisabledTech("canDownGrab");
            logicalOptions.InternalStartConditions = StartConditions.CreateVanillaStartConditionsBuilder(ModelWithOptions).StartingInventory(
                ItemInventory.CreateVanillaStartingInventory(ModelWithOptions)
                    .ApplyAddItem(ModelWithOptions.Items["Morph"])
                    .ApplyAddItem(ModelWithOptions.Items["Bombs"])
                )
                .Build();

            // When
            ModelWithOptions.ApplyLogicalOptions(logicalOptions);

            // Expect
            LogicalRequirements oneFreeOneNeverOnePossible = ModelWithOptions.Rooms["Blue Brinstar Energy Tank Room"].Links[1].To[3].Strats["Ceiling E-Tank Dboost"].Requires;
            Assert.True(oneFreeOneNeverOnePossible.LogicallyRelevant);
            Assert.True(oneFreeOneNeverOnePossible.LogicallyNever);
            Assert.False(oneFreeOneNeverOnePossible.LogicallyAlways);
            Assert.False(oneFreeOneNeverOnePossible.LogicallyFree);
            Assert.False(oneFreeOneNeverOnePossible.LogicallyOrNever);
            Assert.True(oneFreeOneNeverOnePossible.LogicallyOrAlways);
            Assert.True(oneFreeOneNeverOnePossible.LogicallyOrFree);

            LogicalRequirements allFree = ModelWithOptions.Helpers["h_canUseMorphBombs"].Requires;
            Assert.True(allFree.LogicallyRelevant);
            Assert.False(allFree.LogicallyNever);
            Assert.True(allFree.LogicallyAlways);
            Assert.True(allFree.LogicallyFree);
            Assert.False(allFree.LogicallyOrNever);
            Assert.True(allFree.LogicallyOrAlways);
            Assert.True(allFree.LogicallyOrFree);

            LogicalRequirements allNever = ModelWithOptions.Helpers["h_canCrouchJumpDownGrab"].Requires;
            Assert.True(allNever.LogicallyRelevant);
            Assert.True(allNever.LogicallyNever);
            Assert.False(allNever.LogicallyAlways);
            Assert.False(allNever.LogicallyFree);
            Assert.True(allNever.LogicallyOrNever);
            Assert.False(allNever.LogicallyOrAlways);
            Assert.False(allNever.LogicallyOrFree);

            LogicalRequirements allPossible = ModelWithOptions.Helpers["h_canOpenGreenDoors"].Requires;
            Assert.True(allPossible.LogicallyRelevant);
            Assert.False(allPossible.LogicallyNever);
            Assert.False(allPossible.LogicallyAlways);
            Assert.False(allPossible.LogicallyFree);
            Assert.False(allPossible.LogicallyOrNever);
            Assert.False(allPossible.LogicallyOrAlways);
            Assert.False(allPossible.LogicallyOrFree);

            LogicalRequirements empty = ModelWithOptions.Techs["canSuitlessMaridia"].Requires;
            Assert.True(empty.LogicallyRelevant);
            Assert.False(empty.LogicallyNever);
            Assert.True(empty.LogicallyAlways);
            Assert.True(empty.LogicallyFree);
            Assert.False(empty.LogicallyOrNever);
            Assert.True(empty.LogicallyOrAlways);
            Assert.True(empty.LogicallyOrFree);
        }

        [Fact]
        public void ApplyLogicalOptions_SetsLogicalPropertiesOnOrs()
        {
            // Given
            LogicalOptions logicalOptions = new LogicalOptions()
                .RegisterRemovedItem("Spazer")
                .RegisterRemovedItem("Wave");
            logicalOptions.InternalStartConditions = StartConditions.CreateVanillaStartConditionsBuilder(ModelWithOptions).StartingInventory(
                ItemInventory.CreateVanillaStartingInventory(ModelWithOptions)
                    .ApplyAddItem(ModelWithOptions.Items["Plasma"])
                    .ApplyAddItem(ModelWithOptions.Items[SuperMetroidModel.VARIA_SUIT_NAME])
                    .ApplyAddItem(ModelWithOptions.Items[SuperMetroidModel.GRAVITY_SUIT_NAME])
                )
                .Build();

            // When
            ModelWithOptions.ApplyLogicalOptions(logicalOptions);

            // Expect
            Or oneFreeOneNeverOnePossible = ModelWithOptions.Helpers["h_hasBeamUpgrade"].Requires.LogicalElement<Or>(0);
            Assert.True(oneFreeOneNeverOnePossible.LogicallyRelevant);
            Assert.False(oneFreeOneNeverOnePossible.LogicallyNever);
            Assert.True(oneFreeOneNeverOnePossible.LogicallyAlways);
            Assert.True(oneFreeOneNeverOnePossible.LogicallyFree);

            Or allFree = ModelWithOptions.Helpers["h_heatProof"].Requires.LogicalElement<Or>(0);
            Assert.True(allFree.LogicallyRelevant);
            Assert.False(allFree.LogicallyNever);
            Assert.True(allFree.LogicallyAlways);
            Assert.True(allFree.LogicallyFree);

            Or allNever = ModelWithOptions.Rooms["Morph Ball Room"].Links[1].To[6].Strats["Medium Sidehopper Kill"].Obstacles["C"].Requires.LogicalElement<Or>(0);
            Assert.True(allNever.LogicallyRelevant);
            Assert.True(allNever.LogicallyNever);
            Assert.False(allNever.LogicallyAlways);
            Assert.False(allNever.LogicallyFree);

            Or allPossible = ModelWithOptions.Rooms["Morph Ball Room"].Links[5].To[6].Strats["Bomb the Blocks"].Obstacles["A"].Requires.LogicalElement<Or>(0);
            Assert.True(allPossible.LogicallyRelevant);
            Assert.False(allPossible.LogicallyNever);
            Assert.False(allPossible.LogicallyAlways);
            Assert.False(allPossible.LogicallyFree);
        }

        [Fact]
        public void ApplyLogicalOptions_SetsLogicalPropertiesOnAnds()
        {
            // Given
            LogicalOptions logicalOptions = new LogicalOptions()
                .RegisterRemovedItem("Spazer")
                .RegisterRemovedItem("HiJump")
                .RegisterDisabledTech("canSuitlessMaridia");
            logicalOptions.InternalStartConditions = StartConditions.CreateVanillaStartConditionsBuilder(ModelWithOptions).StartingInventory(
                ItemInventory.CreateVanillaStartingInventory(ModelWithOptions)
                    .ApplyAddItem(ModelWithOptions.Items["Charge"])
                    .ApplyAddItem(ModelWithOptions.Items["SpeedBooster"])
                )
                .Build();

            // When
            ModelWithOptions.ApplyLogicalOptions(logicalOptions);

            // Expect
            And oneFreeOneNeverSomePossible = ModelWithOptions.Rooms["Metal Pirates Room"].Obstacles["A"].Requires.LogicalElement<Or>(1)
                .LogicalRequirements.LogicalElement<And>(1);
            Assert.Equal(5, oneFreeOneNeverSomePossible.LogicalRequirements.LogicalElements.Count()); // Sanity check to make sure we have the right And
            Assert.True(oneFreeOneNeverSomePossible.LogicallyRelevant);
            Assert.True(oneFreeOneNeverSomePossible.LogicallyNever);
            Assert.False(oneFreeOneNeverSomePossible.LogicallyAlways);
            Assert.False(oneFreeOneNeverSomePossible.LogicallyFree);

            And allFree = ModelWithOptions.Rooms["Green Hill Zone"].Links[1].To[2].Strats["Base"].Requires.LogicalElement<Or>(0).LogicalRequirements.LogicalElement<And>(0);
            Assert.True(allFree.LogicallyRelevant);
            Assert.False(allFree.LogicallyNever);
            Assert.True(allFree.LogicallyAlways);
            Assert.True(allFree.LogicallyFree);

            And allNever = ModelWithOptions.Rooms["West Sand Hole"].Links[7].To[5].Strats["Left Sand Pit Initial MidAir Morph"].Requires.LogicalElement<Or>(0).LogicalRequirements.LogicalElement<And>(0);
            Assert.True(allNever.LogicallyRelevant);
            Assert.True(allNever.LogicallyNever);
            Assert.False(allNever.LogicallyAlways);
            Assert.False(allNever.LogicallyFree);

            And allPossible = ModelWithOptions.Helpers["h_canBombThings"].Requires.LogicalElement<Or>(0).LogicalRequirements.LogicalElement<And>(0);
            Assert.True(allPossible.LogicallyRelevant);
            Assert.False(allPossible.LogicallyNever);
            Assert.False(allPossible.LogicallyAlways);
            Assert.False(allPossible.LogicallyFree);

        }

        [Fact]
        public void ApplyLogicalOptions_SetsLogicalPropertiesOnTechLogicalElements()
        {
            // Given
            LogicalOptions logicalOptions = new LogicalOptions();
            logicalOptions.RegisterDisabledTech("canPreciseWalljump");
            logicalOptions.InternalStartConditions = StartConditions.CreateVanillaStartConditionsBuilder(ModelWithOptions).StartingInventory(
                ItemInventory.CreateVanillaStartingInventory(ModelWithOptions)
                    .ApplyAddItem(ModelWithOptions.Items["Morph"])
                    .ApplyAddItem(ModelWithOptions.Items["Bombs"])
                )
                .Build();

            // When
            ModelWithOptions.ApplyLogicalOptions(logicalOptions);

            // Expect
            TechLogicalElement disabledTechElement = ModelWithOptions.Techs["canDelayedWalljump"].Requires.LogicalElement<TechLogicalElement>(0, element => element.Tech.Name == "canPreciseWalljump");
            Assert.True(disabledTechElement.LogicallyRelevant);
            Assert.False(disabledTechElement.LogicallyAlways);
            Assert.False(disabledTechElement.LogicallyFree);
            Assert.True(disabledTechElement.LogicallyNever);

            TechLogicalElement impossibleSubTechElement = ModelWithOptions.Techs["canInsaneWalljump"].Requires.LogicalElement<TechLogicalElement>(0, element => element.Tech.Name == "canDelayedWalljump");
            Assert.True(impossibleSubTechElement.LogicallyRelevant);
            Assert.False(impossibleSubTechElement.LogicallyAlways);
            Assert.False(impossibleSubTechElement.LogicallyFree);
            Assert.True(impossibleSubTechElement.LogicallyNever);

            TechLogicalElement nonFreeTechElement = ModelWithOptions.Locks["West Ocean Ship Exit Grey Lock (to Gravity Suit Room)"].BypassStrats["Bowling Skip"]
                .Requires.LogicalElement<TechLogicalElement>(0, element => element.Tech.Name == "canGrappleClip");
            Assert.True(nonFreeTechElement.LogicallyRelevant);
            Assert.False(nonFreeTechElement.LogicallyAlways);
            Assert.False(nonFreeTechElement.LogicallyFree);
            Assert.False(nonFreeTechElement.LogicallyNever);

            TechLogicalElement freeTechElement = ModelWithOptions.Techs["canPreciseWalljump"].Requires.LogicalElement<TechLogicalElement>(0, element => element.Tech.Name == "canWalljump");
            Assert.True(freeTechElement.LogicallyRelevant);
            Assert.True(freeTechElement.LogicallyAlways);
            Assert.True(freeTechElement.LogicallyFree);
            Assert.False(freeTechElement.LogicallyNever);

            TechLogicalElement freeByStartItemTechElement = ModelWithOptions.Techs["canJumpIntoIBJ"].Requires.LogicalElement<TechLogicalElement>(0, element => element.Tech.Name == "canIBJ");
            Assert.True(freeByStartItemTechElement.LogicallyRelevant);
            Assert.True(freeByStartItemTechElement.LogicallyAlways);
            Assert.True(freeByStartItemTechElement.LogicallyFree);
            Assert.False(freeByStartItemTechElement.LogicallyNever);
        }

        [Fact]
        public void ApplyLogicalOptions_SetsLogicalPropertiesOnNeverLogicalElements()
        {
            // Given
            LogicalOptions logicalOptions = new LogicalOptions();

            // When
            ModelWithOptions.ApplyLogicalOptions(logicalOptions);

            // Expect
            NeverLogicalElement never = ModelWithOptions.Locks["Etecoon Exit Grey Lock"].UnlockStrats["Base"].Requires.LogicalElement<NeverLogicalElement>(0);
            Assert.True(never.LogicallyRelevant);
            Assert.False(never.LogicallyAlways);
            Assert.False(never.LogicallyFree);
            Assert.True(never.LogicallyNever);
        }

        [Fact]
        public void ApplyLogicalOptions_SetsLogicalPropertiesOnItemLogicalElements()
        {
            // Given
            LogicalOptions logicalOptions = new LogicalOptions();
            logicalOptions.RegisterRemovedItem("Bombs");
            logicalOptions.InternalStartConditions = StartConditions.CreateVanillaStartConditionsBuilder(ModelWithOptions).StartingInventory(
                ItemInventory.CreateVanillaStartingInventory(ModelWithOptions)
                    .ApplyAddItem(ModelWithOptions.Items["Morph"])
                )
                .Build();

            // When
            ModelWithOptions.ApplyLogicalOptions(logicalOptions);

            // Expect
            ItemLogicalElement freeItemElement = ModelWithOptions.Helpers["h_canBombThings"].Requires.LogicalElement<ItemLogicalElement>(0, element => element.Item.Name == "Morph");
            Assert.True(freeItemElement.LogicallyRelevant);
            Assert.False(freeItemElement.LogicallyNever);
            Assert.True(freeItemElement.LogicallyAlways);
            Assert.True(freeItemElement.LogicallyFree);

            ItemLogicalElement removedItemElement = ModelWithOptions.Helpers["h_canUseMorphBombs"].Requires.LogicalElement<ItemLogicalElement>(0, element => element.Item.Name == "Bombs");
            Assert.True(removedItemElement.LogicallyRelevant);
            Assert.True(removedItemElement.LogicallyNever);
            Assert.False(removedItemElement.LogicallyAlways);
            Assert.False(removedItemElement.LogicallyFree);

            ItemLogicalElement obtainableItemElement = ModelWithOptions.Helpers["h_canUseSpringBall"].Requires.LogicalElement<ItemLogicalElement>(0, element => element.Item.Name == "SpringBall");
            Assert.True(obtainableItemElement.LogicallyRelevant);
            Assert.False(obtainableItemElement.LogicallyNever);
            Assert.False(obtainableItemElement.LogicallyAlways);
            Assert.False(obtainableItemElement.LogicallyFree);
        }

        [Fact]
        public void ApplyLogicalOptions_SetsLogicalPropertiesOnHelperLogicalElements()
        {
            // Given
            LogicalOptions logicalOptions = new LogicalOptions()
                .RegisterDisabledTech("canGateGlitch");
            logicalOptions.InternalStartConditions = StartConditions.CreateVanillaStartConditionsBuilder(ModelWithOptions).StartingInventory(
                ItemInventory.CreateVanillaStartingInventory(ModelWithOptions)
                    .ApplyAddItem(ModelWithOptions.Items["Morph"])
                    .ApplyAddItem(ModelWithOptions.Items["Bombs"])
                )
                .Build();

            // When
            ModelWithOptions.ApplyLogicalOptions(logicalOptions);

            // Expect
            HelperLogicalElement impossibleHelperElement = ModelWithOptions.Helpers["h_canHeatedBlueGateGlitch"].Requires.LogicalElement<HelperLogicalElement>(0, element => element.Helper.Name == "h_canBlueGateGlitch");
            Assert.True(impossibleHelperElement.LogicallyRelevant);
            Assert.False(impossibleHelperElement.LogicallyAlways);
            Assert.False(impossibleHelperElement.LogicallyFree);
            Assert.True(impossibleHelperElement.LogicallyNever);

            HelperLogicalElement nonFreeHelperElement = ModelWithOptions.Helpers["h_canOpenYellowDoors"].Requires.LogicalElement<HelperLogicalElement>(0, element => element.Helper.Name == "h_canUsePowerBombs");
            Assert.True(nonFreeHelperElement.LogicallyRelevant);
            Assert.False(nonFreeHelperElement.LogicallyAlways);
            Assert.False(nonFreeHelperElement.LogicallyFree);
            Assert.False(nonFreeHelperElement.LogicallyNever);

            HelperLogicalElement freeHelperElement = ModelWithOptions.Techs["canHBJ"].Requires.LogicalElement<HelperLogicalElement>(0, element => element.Helper.Name == "h_canUseMorphBombs");
            Assert.True(freeHelperElement.LogicallyRelevant);
            Assert.True(freeHelperElement.LogicallyAlways);
            Assert.True(freeHelperElement.LogicallyFree);
            Assert.False(freeHelperElement.LogicallyNever);
        }

        [Fact]
        public void ApplyLogicalOptions_SetsLogicalPropertiesOnGameFlagLogicalElements()
        {
            // Given
            LogicalOptions logicalOptions = new LogicalOptions();
            logicalOptions.RegisterDisabledGameFlag("f_AnimalsSaved");
            logicalOptions.InternalStartConditions = StartConditions.CreateVanillaStartConditionsBuilder(ModelWithOptions)
                .StartingGameFlags(new List<GameFlag> { ModelWithOptions.GameFlags["f_DefeatedCeresRidley"] })
                .Build();

            // When
            ModelWithOptions.ApplyLogicalOptions(logicalOptions);

            // Expect
            GameFlagLogicalElement alwaysFlagElement = ModelWithOptions.Locks["Ceres Ridley Room Grey Lock (to 58 Escape)"].UnlockStrats["Base"].Requires.LogicalElement<GameFlagLogicalElement>(0);
            Assert.True(alwaysFlagElement.LogicallyRelevant);
            Assert.False(alwaysFlagElement.LogicallyNever);
            Assert.True(alwaysFlagElement.LogicallyAlways);
            Assert.True(alwaysFlagElement.LogicallyFree);

            GameFlagLogicalElement removedFlagElement = ModelWithOptions.Locks["Animal Escape Grey Lock (to Flyway)"].UnlockStrats["Base"].Requires.LogicalElement<GameFlagLogicalElement>(0);
            Assert.True(removedFlagElement.LogicallyRelevant);
            Assert.True(removedFlagElement.LogicallyNever);
            Assert.False(removedFlagElement.LogicallyAlways);
            Assert.False(removedFlagElement.LogicallyFree);

            GameFlagLogicalElement obtainableFlagElement = ModelWithOptions.Locks["Blue Brinstar Power Bombs Spawn Lock"].UnlockStrats["Base"].Requires.LogicalElement<GameFlagLogicalElement>(0);
            Assert.True(obtainableFlagElement.LogicallyRelevant);
            Assert.False(obtainableFlagElement.LogicallyNever);
            Assert.False(obtainableFlagElement.LogicallyAlways);
            Assert.False(obtainableFlagElement.LogicallyFree);
        }

        [Fact]
        public void ApplyLogicalOptions_LessPossibleEnergyThanBestCaseDamage_SetsLogicalPropertiesOnDamageLogicalElements()
        {
            // Given
            LogicalOptions logicalOptions = new LogicalOptions()
                .RegisterRemovedItem(SuperMetroidModel.VARIA_SUIT_NAME)
                .RegisterRemovedItem(SuperMetroidModel.GRAVITY_SUIT_NAME);
            ResourceCount baseResouces = ResourceCount.CreateVanillaBaseResourceMaximums();
            baseResouces.ApplyAmount(RechargeableResourceEnum.RegularEnergy, 29);
            logicalOptions.InternalStartConditions = StartConditions.CreateVanillaStartConditionsBuilder(ModelWithOptions)
                .StartingInventory(
                    ItemInventory.CreateVanillaStartingInventory(ModelWithOptions)
                        .ApplyAddItem(ModelWithOptions.Items[SuperMetroidModel.SPEED_BOOSTER_NAME])
                )
                .BaseResourceMaximums(baseResouces)
                .StartingResources(baseResouces)
                .Build();
            logicalOptions.InternalAvailableResourceInventory = new ResourceItemInventory(baseResouces)
                .ApplyAddExpansionItem((ExpansionItem)ModelWithOptions.Items["Missile"], 46)
                .ApplyAddExpansionItem((ExpansionItem)ModelWithOptions.Items["Super"], 10)
                .ApplyAddExpansionItem((ExpansionItem)ModelWithOptions.Items["PowerBomb"], 10);

            // When
            ModelWithOptions.ApplyLogicalOptions(logicalOptions);

            // Expect
            AcidFrames acidFrames = ModelWithOptions.Rooms["Metroid Room 1"].Links[3].To[2].Strats["Fearless Dive"].Requires.LogicalElement<AcidFrames>(0);
            Assert.True(acidFrames.LogicallyRelevant);
            Assert.True(acidFrames.LogicallyNever);
            Assert.False(acidFrames.LogicallyAlways);
            Assert.False(acidFrames.LogicallyFree);

            DraygonElectricityFrames electricityFrames = ModelWithOptions.Techs["canDraygonTurretGrappleJump"].Requires.LogicalElement<DraygonElectricityFrames>(0);
            Assert.True(electricityFrames.LogicallyRelevant);
            Assert.True(electricityFrames.LogicallyNever);
            Assert.False(electricityFrames.LogicallyAlways);
            Assert.False(electricityFrames.LogicallyFree);

            HeatFrames heatFrames = ModelWithOptions.Rooms["Fast Pillars Setup Room"].Links[4].To[1].Strats["Base"].Requires.LogicalElement<HeatFrames>(0);
            Assert.True(heatFrames.LogicallyRelevant);
            Assert.True(heatFrames.LogicallyNever);
            Assert.False(heatFrames.LogicallyAlways);
            Assert.False(heatFrames.LogicallyFree);

            LavaFrames lavaFrames = ModelWithOptions.Rooms["Spiky Platforms Tunnel"].Links[2].To[1].Strats["Lava Bath"].Requires.LogicalElement<LavaFrames>(0);
            Assert.True(lavaFrames.LogicallyRelevant);
            Assert.True(lavaFrames.LogicallyNever);
            Assert.False(lavaFrames.LogicallyAlways);
            Assert.False(lavaFrames.LogicallyFree);

            LavaPhysicsFrames lavaPhysicsFrames = ModelWithOptions.Rooms["Lava Dive Room"].Links[2].To[1].Strats["Lava Dive Gravity Jump"].Requires.LogicalElement<LavaPhysicsFrames>(0);
            Assert.True(lavaPhysicsFrames.LogicallyRelevant);
            Assert.True(lavaPhysicsFrames.LogicallyNever);
            Assert.False(lavaPhysicsFrames.LogicallyAlways);
            Assert.False(lavaPhysicsFrames.LogicallyFree);

            HibashiHits hibashiHits = ModelWithOptions.Rooms["Wasteland"].Links[7].To[2].Strats["Morph Bombs"].Requires.LogicalElement<HibashiHits>(0);
            Assert.True(hibashiHits.LogicallyRelevant);
            Assert.True(hibashiHits.LogicallyNever);
            Assert.False(hibashiHits.LogicallyAlways);
            Assert.False(hibashiHits.LogicallyFree);

            SpikeHits spikeHits = ModelWithOptions.Rooms["Double Chamber"].Links[3].To[4].Strats["Double Chamber Spike IBJ"].Requires.LogicalElement<SpikeHits>(0);
            Assert.True(spikeHits.LogicallyRelevant);
            Assert.True(spikeHits.LogicallyNever);
            Assert.False(spikeHits.LogicallyAlways);
            Assert.False(spikeHits.LogicallyFree);

            EnemyDamage enemyDamage = ModelWithOptions.Rooms["Green Brinstar Beetom Room"].Links[1].To[2].Strats["Tank the Damage"].Requires.LogicalElement<EnemyDamage>(0);
            Assert.True(enemyDamage.LogicallyRelevant);
            Assert.True(enemyDamage.LogicallyNever);
            Assert.False(enemyDamage.LogicallyAlways);
            Assert.False(enemyDamage.LogicallyFree);

            // Can't test for ThornHits, there's none of them in the model

            EnergyAtMost freeEnergyAtMost = ModelWithOptions.Locks["Ceres Ridley Fight"].UnlockStrats["Base"].Requires.LogicalElement<EnergyAtMost>(0);
            Assert.True(freeEnergyAtMost.LogicallyRelevant);
            Assert.False(freeEnergyAtMost.LogicallyNever);
            Assert.True(freeEnergyAtMost.LogicallyAlways);
            Assert.True(freeEnergyAtMost.LogicallyFree);

            EnergyAtMost possibleEnergyAtMost = ModelWithOptions.Rooms["Big Boy Room"].Links[2].To[1].Strats["Get Drained"].Requires.LogicalElement<EnergyAtMost>(0);
            Assert.True(possibleEnergyAtMost.LogicallyRelevant);
            Assert.False(possibleEnergyAtMost.LogicallyNever);
            Assert.True(possibleEnergyAtMost.LogicallyAlways);
            Assert.False(possibleEnergyAtMost.LogicallyFree);

            CanShineCharge canShineCharge = ModelWithOptions.Rooms["West Ocean"].Links[13].To[5].Strats["Gravity Suit and Shinespark"].Requires.LogicalElement<CanShineCharge>(0);
            Assert.True(canShineCharge.LogicallyRelevant);
            Assert.True(canShineCharge.LogicallyNever);
            Assert.False(canShineCharge.LogicallyAlways);
            Assert.False(canShineCharge.LogicallyFree);

            CanShineCharge noSparkCanShineCharge = ModelWithOptions.Rooms["Parlor and Alcatraz"].Links[8].To[1].Strats["Parlor Quick Charge"].Requires.LogicalElement<CanShineCharge>(0);
            Assert.True(noSparkCanShineCharge.LogicallyRelevant);
            Assert.False(noSparkCanShineCharge.LogicallyNever);
            Assert.True(noSparkCanShineCharge.LogicallyAlways);
            Assert.True(noSparkCanShineCharge.LogicallyFree);

            CanComeInCharged canComeInCharged = ModelWithOptions.Rooms["Golden Torizo's Room"].Links[1].To[3].Strats["Shinespark"].Requires.LogicalElement<CanComeInCharged>(0);
            Assert.True(canComeInCharged.LogicallyRelevant);
            Assert.True(canComeInCharged.LogicallyNever);
            Assert.False(canComeInCharged.LogicallyAlways);
            Assert.False(canComeInCharged.LogicallyFree);

            CanComeInCharged noSparkCanComeInCharged = ModelWithOptions.Rooms["Parlor and Alcatraz"].Links[1].To[8].Strats["SpeedBooster"].Requires.LogicalElement<CanComeInCharged>(0);
            Assert.True(noSparkCanComeInCharged.LogicallyRelevant);
            Assert.False(noSparkCanComeInCharged.LogicallyNever);
            Assert.False(noSparkCanComeInCharged.LogicallyAlways);
            Assert.False(noSparkCanComeInCharged.LogicallyFree);

            CanLeaveCharged canLeaveCharged = ModelWithOptions.Rooms["Spore Spawn Farming Room"].Nodes[1].CanLeaveCharged.First();
            Assert.False(canLeaveCharged.LogicallyRelevant);
            Assert.True(canLeaveCharged.LogicallyNever);
            Assert.False(canLeaveCharged.LogicallyAlways);
            Assert.False(canLeaveCharged.LogicallyFree);

            CanLeaveCharged freeCanLeaveCharged = ModelWithOptions.Rooms["Morph Ball Room"].Nodes[3].CanLeaveCharged.First();
            Assert.True(freeCanLeaveCharged.LogicallyRelevant);
            Assert.False(freeCanLeaveCharged.LogicallyNever);
            Assert.True(freeCanLeaveCharged.LogicallyAlways);
            Assert.True(freeCanLeaveCharged.LogicallyFree);
        }

        [Fact]
        public void ApplyLogicalOptions_NormalPossibleEnergy_SetsLogicalPropertiesOnDamageLogicalElements()
        {
            // Given
            LogicalOptions logicalOptions = new LogicalOptions()
                .RegisterRemovedItem(SuperMetroidModel.VARIA_SUIT_NAME)
                .RegisterRemovedItem(SuperMetroidModel.GRAVITY_SUIT_NAME);
            ResourceCount baseResouces = ResourceCount.CreateVanillaBaseResourceMaximums();
            baseResouces.ApplyAmount(RechargeableResourceEnum.RegularEnergy, 29);
            logicalOptions.InternalStartConditions = StartConditions.CreateVanillaStartConditionsBuilder(ModelWithOptions)
                .StartingInventory(
                    ItemInventory.CreateVanillaStartingInventory(ModelWithOptions)
                        .ApplyAddItem(ModelWithOptions.Items[SuperMetroidModel.SPEED_BOOSTER_NAME])
                )
                .BaseResourceMaximums(baseResouces)
                .StartingResources(baseResouces)
                .Build();
            logicalOptions.InternalAvailableResourceInventory = new ResourceItemInventory(baseResouces)
                .ApplyAddExpansionItem((ExpansionItem)ModelWithOptions.Items["Super"], 10)
                .ApplyAddExpansionItem((ExpansionItem)ModelWithOptions.Items["Missile"], 46)
                .ApplyAddExpansionItem((ExpansionItem)ModelWithOptions.Items["Super"], 10)
                .ApplyAddExpansionItem((ExpansionItem)ModelWithOptions.Items["PowerBomb"], 10)
                .ApplyAddExpansionItem((ExpansionItem)ModelWithOptions.Items["ETank"], 14)
                .ApplyAddExpansionItem((ExpansionItem)ModelWithOptions.Items["ReserveTank"], 4);

            // When
            ModelWithOptions.ApplyLogicalOptions(logicalOptions);

            // Expect
            AcidFrames acidFrames = ModelWithOptions.Rooms["Metroid Room 1"].Links[3].To[2].Strats["Fearless Dive"].Requires.LogicalElement<AcidFrames>(0);
            Assert.True(acidFrames.LogicallyRelevant);
            Assert.False(acidFrames.LogicallyNever);
            Assert.False(acidFrames.LogicallyAlways);
            Assert.False(acidFrames.LogicallyFree);

            DraygonElectricityFrames electricityFrames = ModelWithOptions.Techs["canDraygonTurretGrappleJump"].Requires.LogicalElement<DraygonElectricityFrames>(0);
            Assert.True(electricityFrames.LogicallyRelevant);
            Assert.False(electricityFrames.LogicallyNever);
            Assert.False(electricityFrames.LogicallyAlways);
            Assert.False(electricityFrames.LogicallyFree);

            HeatFrames heatFrames = ModelWithOptions.Rooms["Fast Pillars Setup Room"].Links[4].To[1].Strats["Base"].Requires.LogicalElement<HeatFrames>(0);
            Assert.True(heatFrames.LogicallyRelevant);
            Assert.False(heatFrames.LogicallyNever);
            Assert.False(heatFrames.LogicallyAlways);
            Assert.False(heatFrames.LogicallyFree);

            LavaFrames lavaFrames = ModelWithOptions.Rooms["Spiky Platforms Tunnel"].Links[2].To[1].Strats["Lava Bath"].Requires.LogicalElement<LavaFrames>(0);
            Assert.True(lavaFrames.LogicallyRelevant);
            Assert.False(lavaFrames.LogicallyNever);
            Assert.False(lavaFrames.LogicallyAlways);
            Assert.False(lavaFrames.LogicallyFree);

            LavaPhysicsFrames lavaPhysicsFrames = ModelWithOptions.Rooms["Lava Dive Room"].Links[2].To[1].Strats["Lava Dive Gravity Jump"].Requires.LogicalElement<LavaPhysicsFrames>(0);
            Assert.True(lavaPhysicsFrames.LogicallyRelevant);
            Assert.False(lavaPhysicsFrames.LogicallyNever);
            Assert.False(lavaPhysicsFrames.LogicallyAlways);
            Assert.False(lavaPhysicsFrames.LogicallyFree);

            HibashiHits hibashiHits = ModelWithOptions.Rooms["Wasteland"].Links[7].To[2].Strats["Morph Bombs"].Requires.LogicalElement<HibashiHits>(0);
            Assert.True(hibashiHits.LogicallyRelevant);
            Assert.False(hibashiHits.LogicallyNever);
            Assert.False(hibashiHits.LogicallyAlways);
            Assert.False(hibashiHits.LogicallyFree);

            SpikeHits spikeHits = ModelWithOptions.Rooms["Double Chamber"].Links[3].To[4].Strats["Double Chamber Spike IBJ"].Requires.LogicalElement<SpikeHits>(0);
            Assert.True(spikeHits.LogicallyRelevant);
            Assert.False(spikeHits.LogicallyNever);
            Assert.False(spikeHits.LogicallyAlways);
            Assert.False(spikeHits.LogicallyFree);

            EnemyDamage enemyDamage = ModelWithOptions.Rooms["Green Brinstar Beetom Room"].Links[1].To[2].Strats["Tank the Damage"].Requires.LogicalElement<EnemyDamage>(0);
            Assert.True(enemyDamage.LogicallyRelevant);
            Assert.False(enemyDamage.LogicallyNever);
            Assert.False(enemyDamage.LogicallyAlways);
            Assert.False(enemyDamage.LogicallyFree);

            // Can't test for ThornHits, there's none of them in the model

            CanShineCharge canShineCharge = ModelWithOptions.Rooms["West Ocean"].Links[13].To[5].Strats["Gravity Suit and Shinespark"].Requires.LogicalElement<CanShineCharge>(0);
            Assert.True(canShineCharge.LogicallyRelevant);
            Assert.False(canShineCharge.LogicallyNever);
            Assert.False(canShineCharge.LogicallyAlways);
            Assert.False(canShineCharge.LogicallyFree);

            CanComeInCharged canComeInCharged = ModelWithOptions.Rooms["Golden Torizo's Room"].Links[1].To[3].Strats["Shinespark"].Requires.LogicalElement<CanComeInCharged>(0);
            Assert.True(canComeInCharged.LogicallyRelevant);
            Assert.False(canComeInCharged.LogicallyNever);
            Assert.False(canComeInCharged.LogicallyAlways);
            Assert.False(canComeInCharged.LogicallyFree);

            CanLeaveCharged canLeaveCharged = ModelWithOptions.Rooms["Spore Spawn Farming Room"].Nodes[1].CanLeaveCharged.First();
            Assert.True(canLeaveCharged.LogicallyRelevant);
            Assert.False(canLeaveCharged.LogicallyNever);
            Assert.False(canLeaveCharged.LogicallyAlways);
            Assert.False(canLeaveCharged.LogicallyFree);
        }

        [Fact]
        public void ApplyLogicalOptions_BothSuitsFree_SetsLogicalPropertiesOnDamageLogicalElements()
        {
            // Given
            LogicalOptions logicalOptions = new LogicalOptions();
            ResourceCount baseResouces = ResourceCount.CreateVanillaBaseResourceMaximums();
            baseResouces.ApplyAmount(RechargeableResourceEnum.RegularEnergy, 29);
            logicalOptions.InternalStartConditions = StartConditions.CreateVanillaStartConditionsBuilder(ModelWithOptions)
                .StartingInventory(
                    ItemInventory.CreateVanillaStartingInventory(ModelWithOptions)
                        .ApplyAddItem(ModelWithOptions.Items[SuperMetroidModel.SPEED_BOOSTER_NAME])
                        .ApplyAddItem(ModelWithOptions.Items[SuperMetroidModel.VARIA_SUIT_NAME])
                        .ApplyAddItem(ModelWithOptions.Items[SuperMetroidModel.GRAVITY_SUIT_NAME])
                )
                .BaseResourceMaximums(baseResouces)
                .StartingResources(baseResouces)
                .Build();

            // When
            ModelWithOptions.ApplyLogicalOptions(logicalOptions);

            // Expect
            AcidFrames acidFrames = ModelWithOptions.Rooms["Metroid Room 1"].Links[3].To[2].Strats["Fearless Dive"].Requires.LogicalElement<AcidFrames>(0);
            Assert.True(acidFrames.LogicallyRelevant);
            Assert.False(acidFrames.LogicallyNever);
            Assert.False(acidFrames.LogicallyAlways);
            Assert.False(acidFrames.LogicallyFree);

            DraygonElectricityFrames electricityFrames = ModelWithOptions.Techs["canDraygonTurretGrappleJump"].Requires.LogicalElement<DraygonElectricityFrames>(0);
            Assert.True(electricityFrames.LogicallyRelevant);
            Assert.False(electricityFrames.LogicallyNever);
            Assert.False(electricityFrames.LogicallyAlways);
            Assert.False(electricityFrames.LogicallyFree);

            HeatFrames heatFrames = ModelWithOptions.Rooms["Fast Pillars Setup Room"].Links[4].To[1].Strats["Base"].Requires.LogicalElement<HeatFrames>(0);
            Assert.True(heatFrames.LogicallyRelevant);
            Assert.False(heatFrames.LogicallyNever);
            Assert.True(heatFrames.LogicallyAlways);
            Assert.True(heatFrames.LogicallyFree);

            LavaFrames lavaFrames = ModelWithOptions.Rooms["Spiky Platforms Tunnel"].Links[2].To[1].Strats["Lava Bath"].Requires.LogicalElement<LavaFrames>(0);
            Assert.True(lavaFrames.LogicallyRelevant);
            Assert.False(lavaFrames.LogicallyNever);
            Assert.True(lavaFrames.LogicallyAlways);
            Assert.True(lavaFrames.LogicallyFree);

            LavaPhysicsFrames lavaPhysicsFrames = ModelWithOptions.Rooms["Lava Dive Room"].Links[2].To[1].Strats["Lava Dive Gravity Jump"].Requires.LogicalElement<LavaPhysicsFrames>(0);
            Assert.True(lavaPhysicsFrames.LogicallyRelevant);
            Assert.False(lavaPhysicsFrames.LogicallyNever);
            Assert.False(lavaPhysicsFrames.LogicallyAlways);
            Assert.False(lavaPhysicsFrames.LogicallyFree);

            HibashiHits hibashiHits = ModelWithOptions.Rooms["Wasteland"].Links[7].To[2].Strats["Morph Bombs"].Requires.LogicalElement<HibashiHits>(0);
            Assert.True(hibashiHits.LogicallyRelevant);
            Assert.False(hibashiHits.LogicallyNever);
            Assert.False(hibashiHits.LogicallyAlways);
            Assert.False(hibashiHits.LogicallyFree);

            SpikeHits spikeHits = ModelWithOptions.Rooms["Double Chamber"].Links[3].To[4].Strats["Double Chamber Spike IBJ"].Requires.LogicalElement<SpikeHits>(0);
            Assert.True(spikeHits.LogicallyRelevant);
            Assert.False(spikeHits.LogicallyNever);
            Assert.False(spikeHits.LogicallyAlways);
            Assert.False(spikeHits.LogicallyFree);

            EnemyDamage enemyDamage = ModelWithOptions.Rooms["Green Brinstar Beetom Room"].Links[1].To[2].Strats["Tank the Damage"].Requires.LogicalElement<EnemyDamage>(0);
            Assert.True(enemyDamage.LogicallyRelevant);
            Assert.False(enemyDamage.LogicallyNever);
            Assert.False(enemyDamage.LogicallyAlways);
            Assert.False(enemyDamage.LogicallyFree);

            // Can't test for ThornHits, there's none of them in the model

            CanShineCharge canShineCharge = ModelWithOptions.Rooms["West Ocean"].Links[13].To[5].Strats["Gravity Suit and Shinespark"].Requires.LogicalElement<CanShineCharge>(0);
            Assert.True(canShineCharge.LogicallyRelevant);
            Assert.False(canShineCharge.LogicallyNever);
            Assert.False(canShineCharge.LogicallyAlways);
            Assert.False(canShineCharge.LogicallyFree);

            CanComeInCharged canComeInCharged = ModelWithOptions.Rooms["Golden Torizo's Room"].Links[1].To[3].Strats["Shinespark"].Requires.LogicalElement<CanComeInCharged>(0);
            Assert.True(canComeInCharged.LogicallyRelevant);
            Assert.False(canComeInCharged.LogicallyNever);
            Assert.False(canComeInCharged.LogicallyAlways);
            Assert.False(canComeInCharged.LogicallyFree);

            CanLeaveCharged canLeaveCharged = ModelWithOptions.Rooms["Spore Spawn Farming Room"].Nodes[1].CanLeaveCharged.First();
            Assert.True(canLeaveCharged.LogicallyRelevant);
            Assert.False(canLeaveCharged.LogicallyNever);
            Assert.False(canLeaveCharged.LogicallyAlways);
            Assert.False(canLeaveCharged.LogicallyFree);
        }

        [Fact]
        public void ApplyLogicalOptions_SetsLogicalPropertiesOnPreviousNodes()
        {
            // Given
            LogicalOptions logicalOptions = new LogicalOptions();

            // When
            ModelWithOptions.ApplyLogicalOptions(logicalOptions);

            // Expect
            PreviousNode previousNode = ModelWithOptions.Rooms["Early Supers Room"].Links[3].To[1].Strats["Early Supers Quick Crumble Escape (Dual Quick Crumble)"].Requires.LogicalElement<PreviousNode>(0);
            Assert.True(previousNode.LogicallyRelevant);
            Assert.False(previousNode.LogicallyNever);
            Assert.False(previousNode.LogicallyAlways);
            Assert.False(previousNode.LogicallyFree);
        }

        [Fact]
        public void ApplyLogicalOptions_SetsLogicalPropertiesOnAdjacentRunways()
        {
            // Given
            LogicalOptions logicalOptions = new LogicalOptions();

            // When
            ModelWithOptions.ApplyLogicalOptions(logicalOptions);

            // Expect
            AdjacentRunway adjacentRunway = ModelWithOptions.Rooms["Screw Attack Room"].Links[2].To[3].Strats["Screw Attack Room Transition Screwjump"].Requires.LogicalElement<AdjacentRunway>(0);
            Assert.True(adjacentRunway.LogicallyRelevant);
            Assert.False(adjacentRunway.LogicallyNever);
            Assert.False(adjacentRunway.LogicallyAlways);
            Assert.False(adjacentRunway.LogicallyFree);
        }

        [Fact]
        public void ApplyLogicalOptions_SetsLogicalPropertiesOnAmmo()
        {
            // Given
            LogicalOptions logicalOptions = new LogicalOptions();
            logicalOptions.InternalAvailableResourceInventory = new ResourceItemInventory(ResourceCount.CreateVanillaBaseResourceMaximums())
                .ApplyAddExpansionItem((ExpansionItem)ModelWithOptions.Items["Missile"], 1)
                .ApplyAddExpansionItem((ExpansionItem)ModelWithOptions.Items["Super"], 10)
                .ApplyAddExpansionItem((ExpansionItem)ModelWithOptions.Items["PowerBomb"], 10)
                .ApplyAddExpansionItem((ExpansionItem)ModelWithOptions.Items["ETank"], 14)
                .ApplyAddExpansionItem((ExpansionItem)ModelWithOptions.Items["ReserveTank"], 4);

            // When
            ModelWithOptions.ApplyLogicalOptions(logicalOptions);

            // Expect
            Ammo impossibleAmmo = ModelWithOptions.Techs["canCrystalFlash"].Requires.LogicalElement<Ammo>(0);
            Assert.True(impossibleAmmo.LogicallyRelevant);
            Assert.True(impossibleAmmo.LogicallyNever);
            Assert.False(impossibleAmmo.LogicallyAlways);
            Assert.False(impossibleAmmo.LogicallyFree);

            Ammo possibleAmmo = ModelWithOptions.Techs["canCrystalFlash"].Requires.LogicalElement<Ammo>(1);
            Assert.True(possibleAmmo.LogicallyRelevant);
            Assert.False(possibleAmmo.LogicallyNever);
            Assert.False(possibleAmmo.LogicallyAlways);
            Assert.False(possibleAmmo.LogicallyFree);
        }

        [Fact]
        public void ApplyLogicalOptions_SetsLogicalPropertiesOnAmmoDrain()
        {
            // Given
            LogicalOptions logicalOptions = new LogicalOptions();
            logicalOptions.InternalAvailableResourceInventory = new ResourceItemInventory(ResourceCount.CreateVanillaBaseResourceMaximums())
                .ApplyAddExpansionItem((ExpansionItem)ModelWithOptions.Items["Super"], 10)
                .ApplyAddExpansionItem((ExpansionItem)ModelWithOptions.Items["PowerBomb"], 10)
                .ApplyAddExpansionItem((ExpansionItem)ModelWithOptions.Items["ETank"], 14)
                .ApplyAddExpansionItem((ExpansionItem)ModelWithOptions.Items["ReserveTank"], 4);

            // When
            ModelWithOptions.ApplyLogicalOptions(logicalOptions);

            // Expect
            AmmoDrain freeAmmoDrain = ModelWithOptions.Locks["Mother Brain 2 and 3 Fight"].UnlockStrats["Base"].Requires.LogicalElement<AmmoDrain>(0);
            Assert.True(freeAmmoDrain.LogicallyRelevant);
            Assert.False(freeAmmoDrain.LogicallyNever);
            Assert.True(freeAmmoDrain.LogicallyAlways);
            Assert.True(freeAmmoDrain.LogicallyFree);

            AmmoDrain nonFreeAmmoDrain = ModelWithOptions.Locks["Mother Brain 2 and 3 Fight"].UnlockStrats["Base"].Requires.LogicalElement<AmmoDrain>(1);
            Assert.True(nonFreeAmmoDrain.LogicallyRelevant);
            Assert.False(nonFreeAmmoDrain.LogicallyNever);
            Assert.True(nonFreeAmmoDrain.LogicallyAlways);
            Assert.False(nonFreeAmmoDrain.LogicallyFree);
        }

        [Fact]
        public void ApplyLogicalOptions_NoSpeedBooster_SetsLogicalPropertiesOnShineChargeLogicalElements()
        {
            // Given
            LogicalOptions logicalOptions = new LogicalOptions()
                .RegisterRemovedItem(SuperMetroidModel.SPEED_BOOSTER_NAME);

            // When
            ModelWithOptions.ApplyLogicalOptions(logicalOptions);

            // Expect
            CanComeInCharged canComeInCharged = ModelWithOptions.Rooms["Parlor and Alcatraz"].Links[1].To[8].Strats["SpeedBooster"].Requires.LogicalElement<CanComeInCharged>(0);
            Assert.True(canComeInCharged.LogicallyRelevant);
            Assert.True(canComeInCharged.LogicallyNever);
            Assert.False(canComeInCharged.LogicallyAlways);
            Assert.False(canComeInCharged.LogicallyFree);

            CanShineCharge canShineCharge = ModelWithOptions.Rooms["Parlor and Alcatraz"].Links[8].To[1].Strats["Parlor Quick Charge"].Requires.LogicalElement<CanShineCharge>(0);
            Assert.True(canShineCharge.LogicallyRelevant);
            Assert.True(canShineCharge.LogicallyNever);
            Assert.False(canShineCharge.LogicallyAlways);
            Assert.False(canShineCharge.LogicallyFree);
        }

        [Fact]
        public void ApplyLogicalOptions_FreeSpeedNoShinespark_SetsLogicalPropertiesOnShineChargeLogicalElements()
        {
            // Given
            LogicalOptions logicalOptions = new LogicalOptions()
                .RegisterDisabledTech("canShinespark");
            logicalOptions.InternalStartConditions = StartConditions.CreateVanillaStartConditionsBuilder(ModelWithOptions).StartingInventory(
                ItemInventory.CreateVanillaStartingInventory(ModelWithOptions)
                    .ApplyAddItem(ModelWithOptions.Items[SuperMetroidModel.SPEED_BOOSTER_NAME])
                )
                .Build();

            // When
            ModelWithOptions.ApplyLogicalOptions(logicalOptions);

            // Expect
            CanComeInCharged noSparkCanComeInCharged = ModelWithOptions.Rooms["Parlor and Alcatraz"].Links[1].To[8].Strats["SpeedBooster"].Requires.LogicalElement<CanComeInCharged>(0);
            Assert.True(noSparkCanComeInCharged.LogicallyRelevant);
            Assert.False(noSparkCanComeInCharged.LogicallyNever);
            Assert.False(noSparkCanComeInCharged.LogicallyAlways);
            Assert.False(noSparkCanComeInCharged.LogicallyFree);

            CanComeInCharged canComeInCharged = ModelWithOptions.Rooms["Green Brinstar Main Shaft / Etecoon Room"].Links[10].To[9].Strats["Shinespark"].Requires.LogicalElement<CanComeInCharged>(0);
            Assert.True(canComeInCharged.LogicallyRelevant);
            Assert.True(canComeInCharged.LogicallyNever);
            Assert.False(canComeInCharged.LogicallyAlways);
            Assert.False(canComeInCharged.LogicallyFree);

            CanShineCharge noSparkCanShineCharge = ModelWithOptions.Rooms["Parlor and Alcatraz"].Links[8].To[1].Strats["Parlor Quick Charge"].Requires.LogicalElement<CanShineCharge>(0);
            Assert.True(noSparkCanShineCharge.LogicallyRelevant);
            Assert.False(noSparkCanShineCharge.LogicallyNever);
            Assert.True(noSparkCanShineCharge.LogicallyAlways);
            Assert.True(noSparkCanShineCharge.LogicallyFree);

            CanShineCharge canShineCharge = ModelWithOptions.Rooms["Landing Site"].Links[7].To[1].Strats["Shinespark"].Requires.LogicalElement<CanShineCharge>(0);
            Assert.True(canShineCharge.LogicallyRelevant);
            Assert.True(canShineCharge.LogicallyNever);
            Assert.False(canShineCharge.LogicallyAlways);
            Assert.False(canShineCharge.LogicallyFree);
        }

        [Fact]
        public void ApplyLogicalOptions_NeedsMoreTilesToCharge_SetsLogicalPropertiesOnCanShineCharges()
        {
            // Given
            LogicalOptions logicalOptions = new LogicalOptions();
            logicalOptions.InternalStartConditions = StartConditions.CreateVanillaStartConditionsBuilder(ModelWithOptions).StartingInventory(
                ItemInventory.CreateVanillaStartingInventory(ModelWithOptions)
                    .ApplyAddItem(ModelWithOptions.Items[SuperMetroidModel.SPEED_BOOSTER_NAME])
                )
                .Build();
            logicalOptions.ShineChargesWithStutter = false;
            logicalOptions.TilesToShineCharge = 35;

            // When
            ModelWithOptions.ApplyLogicalOptions(logicalOptions);

            // Expect
            CanShineCharge canShineCharge = ModelWithOptions.Rooms["Parlor and Alcatraz"].Links[8].To[1].Strats["Parlor Quick Charge"].Requires.LogicalElement<CanShineCharge>(0);
            Assert.True(canShineCharge.LogicallyRelevant);
            Assert.True(canShineCharge.LogicallyNever);
            Assert.False(canShineCharge.LogicallyAlways);
            Assert.False(canShineCharge.LogicallyFree);
        }

        [Fact]
        public void ApplyLogicalOptions_SetsLogicalPropertiesOnEnemyKill()
        {
            // Given
            LogicalOptions logicalOptions = new LogicalOptions();
            logicalOptions.InternalStartConditions = StartConditions.CreateVanillaStartConditionsBuilder(ModelWithOptions).StartingInventory(
                ItemInventory.CreateVanillaStartingInventory(ModelWithOptions)
                    .ApplyAddItem(ModelWithOptions.Items["Spazer"])
                )
                .Build();
            logicalOptions.InternalAvailableResourceInventory = new ResourceItemInventory(ResourceCount.CreateVanillaBaseResourceMaximums())
                .ApplyAddExpansionItem((ExpansionItem)ModelWithOptions.Items["Missile"], 46)
                .ApplyAddExpansionItem((ExpansionItem)ModelWithOptions.Items["Super"], 10)
                .ApplyAddExpansionItem((ExpansionItem)ModelWithOptions.Items["ETank"], 14)
                .ApplyAddExpansionItem((ExpansionItem)ModelWithOptions.Items["ReserveTank"], 4);

            // When
            ModelWithOptions.ApplyLogicalOptions(logicalOptions);

            // Expect
            EnemyKill impossibleEnemyKill = ModelWithOptions.Rooms["Morph Ball Room"].Links[1].To[6].Strats["PB Sidehopper Kill with Bomb Blocks"].Obstacles["C"].Requires.LogicalElement<EnemyKill>(0);
            Assert.True(impossibleEnemyKill.LogicallyRelevant);
            Assert.True(impossibleEnemyKill.LogicallyNever);
            Assert.False(impossibleEnemyKill.LogicallyAlways);
            Assert.False(impossibleEnemyKill.LogicallyFree);

            EnemyKill possibleEnemyKill = ModelWithOptions.Rooms["Green Brinstar Beetom Room"].Links[1].To[2].Strats["Kill the Beetoms"].Requires.LogicalElement<EnemyKill>(0);
            Assert.True(possibleEnemyKill.LogicallyRelevant);
            Assert.False(possibleEnemyKill.LogicallyNever);
            Assert.False(possibleEnemyKill.LogicallyAlways);
            Assert.False(possibleEnemyKill.LogicallyFree);

            EnemyKill freeEnemyKill = ModelWithOptions.Rooms["Baby Kraid Room"].Links[1].To[2].Strats["Kill the Enemies"].Obstacles["A"].Requires.LogicalElement<EnemyKill>(0);
            Assert.True(freeEnemyKill.LogicallyRelevant);
            Assert.False(freeEnemyKill.LogicallyNever);
            Assert.True(freeEnemyKill.LogicallyAlways);
            Assert.True(freeEnemyKill.LogicallyFree);
        }

        [Fact]
        public void ApplyLogicalOptions_SetsLogicalPropertiesOnResetRoom()
        {
            // Given
            LogicalOptions logicalOptions = new LogicalOptions();

            // When
            ModelWithOptions.ApplyLogicalOptions(logicalOptions);

            // Expect
            ResetRoom resetRoom = ModelWithOptions.Runways["Base Runway - Green Shaft Mid-Low Left Door (to Firefleas)"].Strats["Base"].Requires.LogicalElement<ResetRoom>(0);
            Assert.True(resetRoom.LogicallyRelevant);
            Assert.False(resetRoom.LogicallyNever);
            Assert.False(resetRoom.LogicallyAlways);
            Assert.False(resetRoom.LogicallyFree);
        }

        #endregion
    }
}
