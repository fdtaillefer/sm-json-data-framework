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
        private static SuperMetroidModel ModelForApplyLogicalOptions = new UnfinalizedSuperMetroidModel(StaticTestObjects.RawModel).Finalize();

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
            ModelForApplyLogicalOptions.ApplyLogicalOptions(logicalOptions);

            // Expect
            ReadOnlyLogicalOptions appliedOptions = ModelForApplyLogicalOptions.Rooms["Landing Site"].AppliedLogicalOptions;
            Assert.NotSame(logicalOptions, appliedOptions);
            Assert.Equal(20, appliedOptions.TilesToShineCharge);

            Assert.Same(appliedOptions, ModelForApplyLogicalOptions.Weapons["Wave"].AppliedLogicalOptions);
            Assert.Same(appliedOptions, ModelForApplyLogicalOptions.Enemies["Geemer (blue)"].AppliedLogicalOptions);
            Assert.Same(appliedOptions, ModelForApplyLogicalOptions.Items["SpeedBooster"].AppliedLogicalOptions);
            Assert.Same(appliedOptions, ModelForApplyLogicalOptions.GameFlags["f_ZebesSetAblaze"].AppliedLogicalOptions);
            Assert.Same(appliedOptions, ModelForApplyLogicalOptions.Techs["canWalljump"].AppliedLogicalOptions);
            Assert.Same(appliedOptions, ModelForApplyLogicalOptions.Helpers["h_canOpenZebetites"].AppliedLogicalOptions);
            Assert.Same(appliedOptions, ModelForApplyLogicalOptions.Connections[ModelForApplyLogicalOptions.GetNodeInRoom("Landing Site", 1).IdentifyingString].AppliedLogicalOptions);
        }

        [Fact]
        public void ApplyLogicalOptions_AppliesCloneToTechProperties()
        {
            // Given
            LogicalOptions logicalOptions = new LogicalOptions();
            logicalOptions.TilesToShineCharge = 20;

            // When
            ModelForApplyLogicalOptions.ApplyLogicalOptions(logicalOptions);

            // Expect
            Tech arbitraryTech = ModelForApplyLogicalOptions.Techs["canWalljump"];
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
            ModelForApplyLogicalOptions.ApplyLogicalOptions(logicalOptions);

            // Expect
            Helper arbitraryHelper = ModelForApplyLogicalOptions.Helpers["h_canOpenZebetites"];
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
            ModelForApplyLogicalOptions.ApplyLogicalOptions(logicalOptions);

            // Expect
            Weapon arbitraryWeapon = ModelForApplyLogicalOptions.Weapons["Wave"];
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
            ModelForApplyLogicalOptions.ApplyLogicalOptions(logicalOptions);

            // Expect
            Enemy arbitraryEnemy = ModelForApplyLogicalOptions.Enemies["Geemer (blue)"];
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
            ModelForApplyLogicalOptions.ApplyLogicalOptions(logicalOptions);

            // Expect
            Connection arbitraryConnection = ModelForApplyLogicalOptions.Connections[ModelForApplyLogicalOptions.GetNodeInRoom("Landing Site", 1).IdentifyingString];
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
            ModelForApplyLogicalOptions.ApplyLogicalOptions(logicalOptions);

            // Expect
            Room arbitraryRoom = ModelForApplyLogicalOptions.Rooms["Climb"];
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
            ModelForApplyLogicalOptions.ApplyLogicalOptions(logicalOptions);

            // Expect
            Link arbitraryLink = ModelForApplyLogicalOptions.Rooms["Landing Site"].Links[1];
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
            ModelForApplyLogicalOptions.ApplyLogicalOptions(logicalOptions);

            // Expect
            LinkTo arbitraryLinkTo = ModelForApplyLogicalOptions.Rooms["Landing Site"].Links[1].To[4];
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
            ModelForApplyLogicalOptions.ApplyLogicalOptions(logicalOptions);

            // Expect
            Strat arbitraryStrat = ModelForApplyLogicalOptions.Rooms["Pink Brinstar Power Bomb Room"].Links[3].To[4].Strats["Mission Impossible"];
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
            ModelForApplyLogicalOptions.ApplyLogicalOptions(logicalOptions);

            // Expect
            RoomObstacle arbitraryRoomObstacle = ModelForApplyLogicalOptions.Rooms["Climb"].Obstacles["A"];
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
            ModelForApplyLogicalOptions.ApplyLogicalOptions(logicalOptions);

            // Expect
            RoomEnemy arbitraryRoomEnemy = ModelForApplyLogicalOptions.Rooms["Early Supers Room"].Enemies["e1"];
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
            ModelForApplyLogicalOptions.ApplyLogicalOptions(logicalOptions);

            // Expect
            FarmCycle arbitraryFarmCycle = ModelForApplyLogicalOptions.Rooms["Early Supers Room"].Enemies["e1"].FarmCycles["Crouch over spawn point"];
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
            ModelForApplyLogicalOptions.ApplyLogicalOptions(logicalOptions);

            // Expect
            RoomNode arbitraryNode = ModelForApplyLogicalOptions.GetNodeInRoom("Landing Site", 1);
            ReadOnlyLogicalOptions appliedOptions = arbitraryNode.AppliedLogicalOptions;
            Assert.NotSame(logicalOptions, appliedOptions);
            Assert.Equal(20, appliedOptions.TilesToShineCharge);

            Assert.Same(appliedOptions, arbitraryNode.Locks["Landing Site Top Left Escape Lock (to Gauntlet)"].AppliedLogicalOptions);
            Assert.Same(appliedOptions, arbitraryNode.DoorEnvironments.First().AppliedLogicalOptions);
            Assert.Same(appliedOptions, arbitraryNode.CanLeaveCharged.First().AppliedLogicalOptions);
            Assert.Same(appliedOptions, arbitraryNode.InteractionRequires.AppliedLogicalOptions);
            Assert.Same(appliedOptions, arbitraryNode.Runways["Base Runway - Landing Site Top Left Door (to Gauntlet)"].AppliedLogicalOptions);
            Assert.Same(appliedOptions, ModelForApplyLogicalOptions.GetNodeInRoom("Blue Brinstar Energy Tank Room", 1).ViewableNodes[0].AppliedLogicalOptions);
        }

        [Fact]
        public void ApplyLogicalOptions_AppliesCloneToRunwayProperties()
        {
            // Given
            LogicalOptions logicalOptions = new LogicalOptions();
            logicalOptions.TilesToShineCharge = 20;

            // When
            ModelForApplyLogicalOptions.ApplyLogicalOptions(logicalOptions);

            // Expect
            Runway arbitraryRunway = ModelForApplyLogicalOptions.GetNodeInRoom("Climb", 5).Runways["Base Runway - Climb Bottom Right Door (to Pit Room)"];
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
            ModelForApplyLogicalOptions.ApplyLogicalOptions(logicalOptions);

            // Expect
            CanLeaveCharged arbitraryCanLeaveCharged = ModelForApplyLogicalOptions.GetNodeInRoom("Landing Site", 1).CanLeaveCharged.First();
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
            ModelForApplyLogicalOptions.ApplyLogicalOptions(logicalOptions);

            // Expect
            ViewableNode arbitraryViewableNode = ModelForApplyLogicalOptions.GetNodeInRoom("Blue Brinstar Energy Tank Room", 1).ViewableNodes[0];
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
            ModelForApplyLogicalOptions.ApplyLogicalOptions(logicalOptions);

            // Expect
            NodeLock arbitraryLock = ModelForApplyLogicalOptions.GetNodeInRoom("West Ocean", 4).Locks["West Ocean Ship Exit Grey Lock (to Gravity Suit Room)"];
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
            ModelForApplyLogicalOptions.ApplyLogicalOptions(logicalOptions);

            // Expect
            LogicalRequirements arbitraryLogicalRequirements = ModelForApplyLogicalOptions.Rooms["Climb"].Links[6].To[3].Strats["Behemoth Spark Top"].Requires;
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
            ModelForApplyLogicalOptions.ApplyLogicalOptions(logicalOptions);

            // Expect
            Or arbitraryOr = (Or)ModelForApplyLogicalOptions.Helpers["h_canPassBombPassages"].Requires.LogicalElements.Where(element => typeof(Or).IsAssignableFrom(element.GetType())).First();
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
            ModelForApplyLogicalOptions.ApplyLogicalOptions(logicalOptions);

            // Expect
            And arbitraryAnd = ModelForApplyLogicalOptions.Helpers["h_canPassBombPassages"].Requires.LogicalElement<Or>(0)
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
        public void ApplyLogicalOptions_SetsLogicalPropertiesOnTechs()
        {
            // Given
            LogicalOptions logicalOptions = new LogicalOptions();
            logicalOptions.RegisterDisabledTech("canPreciseWalljump");
            logicalOptions.InternalStartConditions = StartConditions.CreateVanillaStartConditionsBuilder(ModelForApplyLogicalOptions).StartingInventory(
                ItemInventory.CreateVanillaStartingInventory(ModelForApplyLogicalOptions)
                    .ApplyAddItem(ModelForApplyLogicalOptions.Items["Morph"])
                    .ApplyAddItem(ModelForApplyLogicalOptions.Items["Bombs"])
                )
                .Build();

            // When
            ModelForApplyLogicalOptions.ApplyLogicalOptions(logicalOptions);

            // Expect
            Tech disabledTech = ModelForApplyLogicalOptions.Techs["canPreciseWalljump"];
            Assert.False(disabledTech.LogicallyRelevant);
            Assert.False(disabledTech.LogicallyAlways);
            Assert.False(disabledTech.LogicallyFree);
            Assert.True(disabledTech.LogicallyNever);

            Tech impossibleSubTech = ModelForApplyLogicalOptions.Techs["canDelayedWalljump"];
            Assert.False(impossibleSubTech.LogicallyRelevant);
            Assert.False(impossibleSubTech.LogicallyAlways);
            Assert.False(impossibleSubTech.LogicallyFree);
            Assert.True(impossibleSubTech.LogicallyNever);

            Tech nonFreeTech = ModelForApplyLogicalOptions.Techs["canGrappleClip"];
            Assert.True(nonFreeTech.LogicallyRelevant);
            Assert.False(nonFreeTech.LogicallyAlways);
            Assert.False(nonFreeTech.LogicallyFree);
            Assert.False(nonFreeTech.LogicallyNever);

            Tech freeTech = ModelForApplyLogicalOptions.Techs["canWalljump"];
            Assert.True(freeTech.LogicallyRelevant);
            Assert.True(freeTech.LogicallyAlways);
            Assert.True(freeTech.LogicallyFree);
            Assert.False(freeTech.LogicallyNever);

            Tech freeByStartItemTech = ModelForApplyLogicalOptions.Techs["canIBJ"];
            Assert.True(freeByStartItemTech.LogicallyRelevant);
            Assert.True(freeByStartItemTech.LogicallyAlways);
            Assert.True(freeByStartItemTech.LogicallyFree);
            Assert.False(freeByStartItemTech.LogicallyNever);
        }

        [Fact]
        public void ApplyLogicalOptions_SetsLogicalPropertiesOnHelpers()
        {
            // Given
            LogicalOptions logicalOptions = new LogicalOptions()
                .RegisterDisabledTech("canGateGlitch");
            logicalOptions.InternalStartConditions = StartConditions.CreateVanillaStartConditionsBuilder(ModelForApplyLogicalOptions).StartingInventory(
                ItemInventory.CreateVanillaStartingInventory(ModelForApplyLogicalOptions)
                    .ApplyAddItem(ModelForApplyLogicalOptions.Items["Morph"])
                    .ApplyAddItem(ModelForApplyLogicalOptions.Items["Bombs"])
                )
                .Build();

            // When
            ModelForApplyLogicalOptions.ApplyLogicalOptions(logicalOptions);

            // Expect
            Helper impossibleHelper = ModelForApplyLogicalOptions.Helpers["h_canBlueGateGlitch"];
            Assert.False(impossibleHelper.LogicallyRelevant);
            Assert.False(impossibleHelper.LogicallyAlways);
            Assert.False(impossibleHelper.LogicallyFree);
            Assert.True(impossibleHelper.LogicallyNever);

            Helper nonFreeHelper = ModelForApplyLogicalOptions.Helpers["h_hasBeamUpgrade"];
            Assert.True(nonFreeHelper.LogicallyRelevant);
            Assert.False(nonFreeHelper.LogicallyAlways);
            Assert.False(nonFreeHelper.LogicallyFree);
            Assert.False(nonFreeHelper.LogicallyNever);

            Helper freeHelper = ModelForApplyLogicalOptions.Helpers["h_canUseMorphBombs"];
            Assert.True(freeHelper.LogicallyRelevant);
            Assert.True(freeHelper.LogicallyAlways);
            Assert.True(freeHelper.LogicallyFree);
            Assert.False(freeHelper.LogicallyNever);

        }

        [Fact]
        public void ApplyLogicalOptions_SetsLogicalPropertiesOnWeapons()
        {
            // Given
            LogicalOptions logicalOptions = new LogicalOptions();
            logicalOptions.InternalAvailableResourceInventory = new ResourceItemInventory(ResourceCount.CreateVanillaBaseResourceMaximums())
                .ApplyAddExpansionItem((ExpansionItem)ModelForApplyLogicalOptions.Items["Super"], 10)
                .ApplyAddExpansionItem((ExpansionItem)ModelForApplyLogicalOptions.Items["PowerBomb"], 10)
                .ApplyAddExpansionItem((ExpansionItem)ModelForApplyLogicalOptions.Items["ETank"], 14)
                .ApplyAddExpansionItem((ExpansionItem)ModelForApplyLogicalOptions.Items["ReserveTank"], 4);

            logicalOptions.InternalStartConditions = StartConditions.CreateVanillaStartConditionsBuilder(ModelForApplyLogicalOptions).StartingInventory(
                ItemInventory.CreateVanillaStartingInventory(ModelForApplyLogicalOptions)
                    .ApplyAddItem(ModelForApplyLogicalOptions.Items["Wave"])
                )
                .Build();

            logicalOptions.RegisterRemovedItem("Ice");

            // When
            ModelForApplyLogicalOptions.ApplyLogicalOptions(logicalOptions);

            // Expect
            Weapon impossibleUseWeapon = ModelForApplyLogicalOptions.Weapons["Ice"];
            Assert.False(impossibleUseWeapon.LogicallyRelevant);
            Assert.False(impossibleUseWeapon.LogicallyAlways);
            Assert.False(impossibleUseWeapon.LogicallyFree);
            Assert.True(impossibleUseWeapon.LogicallyNever);

            Weapon impossibleShootWeapon = ModelForApplyLogicalOptions.Weapons["Missile"];
            Assert.False(impossibleShootWeapon.LogicallyRelevant);
            Assert.False(impossibleShootWeapon.LogicallyAlways);
            Assert.False(impossibleShootWeapon.LogicallyFree);
            Assert.True(impossibleShootWeapon.LogicallyNever);

            Weapon nonFreeWeapon = ModelForApplyLogicalOptions.Weapons["Charge+Wave"];
            Assert.True(nonFreeWeapon.LogicallyRelevant);
            Assert.False(nonFreeWeapon.LogicallyAlways);
            Assert.False(nonFreeWeapon.LogicallyFree);
            Assert.False(nonFreeWeapon.LogicallyNever);

            Weapon freeWeapon = ModelForApplyLogicalOptions.Weapons["Wave"];
            Assert.True(freeWeapon.LogicallyRelevant);
            Assert.True(freeWeapon.LogicallyAlways);
            Assert.True(freeWeapon.LogicallyFree);
            Assert.False(freeWeapon.LogicallyNever);
        }

        [Fact]
        public void ApplyLogicalOptions_SetsLogicalPropertiesOnEnemiesAndSubProperties()
        {
            // Given
            LogicalOptions logicalOptions = new LogicalOptions();
            logicalOptions.RegisterRemovedItem("Ice");

            // When
            ModelForApplyLogicalOptions.ApplyLogicalOptions(logicalOptions);

            // Expect
            Enemy enemy = ModelForApplyLogicalOptions.Enemies["Evir"];
            Assert.True(enemy.LogicallyRelevant);
            Assert.True(enemy.Attacks["contact"].LogicallyRelevant);
            Assert.False(enemy.Dimensions.LogicallyRelevant);
            Assert.False(enemy.InvulnerableWeapons["Ice"].LogicallyRelevant);
            Assert.True(enemy.InvulnerableWeapons["Grapple"].LogicallyRelevant);

            Enemy multiplierEnemy = ModelForApplyLogicalOptions.Enemies["Kihunter (red)"];
            Assert.False(multiplierEnemy.WeaponSusceptibilities["Ice"].LogicallyRelevant);
            Assert.False(multiplierEnemy.WeaponMultipliers["Ice"].LogicallyRelevant);
            Assert.True(multiplierEnemy.WeaponSusceptibilities["Spazer"].LogicallyRelevant);
            Assert.True(multiplierEnemy.WeaponMultipliers["Spazer"].LogicallyRelevant);
        }

        [Fact]
        public void ApplyLogicalOptions_SetsLogicalPropertiesOnConnectionsAndNodes()
        {
            // Given
            LogicalOptions logicalOptions = new LogicalOptions();
            logicalOptions.InternalAvailableResourceInventory = new ResourceItemInventory(ResourceCount.CreateVanillaBaseResourceMaximums())
                .ApplyAddExpansionItem((ExpansionItem)ModelForApplyLogicalOptions.Items["Missile"], 46)
                .ApplyAddExpansionItem((ExpansionItem)ModelForApplyLogicalOptions.Items["PowerBomb"], 10)
                .ApplyAddExpansionItem((ExpansionItem)ModelForApplyLogicalOptions.Items["ETank"], 14)
                .ApplyAddExpansionItem((ExpansionItem)ModelForApplyLogicalOptions.Items["ReserveTank"], 4);

            // When
            ModelForApplyLogicalOptions.ApplyLogicalOptions(logicalOptions);

            // Expect
            Connection unfollowableconnection = ModelForApplyLogicalOptions.Connections[ModelForApplyLogicalOptions.GetNodeInRoom("Big Pink", 8).IdentifyingString];
            // Followability is not considered in-scope for logical relevance
            Assert.True(unfollowableconnection.LogicallyRelevant);
            Assert.True(unfollowableconnection.FromNode.LogicallyRelevant);
            Assert.True(unfollowableconnection.ToNode.LogicallyRelevant);

            Connection followableconnection = ModelForApplyLogicalOptions.Connections[ModelForApplyLogicalOptions.GetNodeInRoom("Landing Site", 1).IdentifyingString];
            Assert.True(followableconnection.LogicallyRelevant);
            Assert.True(followableconnection.FromNode.LogicallyRelevant);
            Assert.True(followableconnection.ToNode.LogicallyRelevant);
        }

        [Fact]
        public void ApplyLogicalOptions_SetsLogicalPropertiesOnRooms()
        {
            // Given
            LogicalOptions logicalOptions = new LogicalOptions();
            logicalOptions.InternalAvailableResourceInventory = new ResourceItemInventory(ResourceCount.CreateVanillaBaseResourceMaximums())
                .ApplyAddExpansionItem((ExpansionItem)ModelForApplyLogicalOptions.Items["Missile"], 46)
                .ApplyAddExpansionItem((ExpansionItem)ModelForApplyLogicalOptions.Items["Super"], 10)
                .ApplyAddExpansionItem((ExpansionItem)ModelForApplyLogicalOptions.Items["ETank"], 14)
                .ApplyAddExpansionItem((ExpansionItem)ModelForApplyLogicalOptions.Items["ReserveTank"], 4);

            // When
            ModelForApplyLogicalOptions.ApplyLogicalOptions(logicalOptions);

            // Expect
            Room unreachableRoom = ModelForApplyLogicalOptions.Rooms["Norfair Map Room"];
            // Room accessibility is not considered in-scope for logical relevance
            Assert.True(unreachableRoom.LogicallyRelevant);

            Room reachableRoom = ModelForApplyLogicalOptions.Rooms["Landing Site"];
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
                .ApplyAddExpansionItem((ExpansionItem)ModelForApplyLogicalOptions.Items["Missile"], 46)
                .ApplyAddExpansionItem((ExpansionItem)ModelForApplyLogicalOptions.Items["Super"], 10)
                .ApplyAddExpansionItem((ExpansionItem)ModelForApplyLogicalOptions.Items["ETank"], 14)
                .ApplyAddExpansionItem((ExpansionItem)ModelForApplyLogicalOptions.Items["ReserveTank"], 4);

            // When
            ModelForApplyLogicalOptions.ApplyLogicalOptions(logicalOptions);

            // Expect
            RoomObstacle obstacle = ModelForApplyLogicalOptions.Rooms["Climb"].Obstacles["A"];
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
            logicalOptions.InternalStartConditions = StartConditions.CreateVanillaStartConditionsBuilder(ModelForApplyLogicalOptions).StartingInventory(
                ItemInventory.CreateVanillaStartingInventory(ModelForApplyLogicalOptions)
                    .ApplyAddItem(ModelForApplyLogicalOptions.Items["ScrewAttack"])
            )
            .Build();

            // When
            ModelForApplyLogicalOptions.ApplyLogicalOptions(logicalOptions);

            // Expect
            RoomObstacle obstacle = ModelForApplyLogicalOptions.Rooms["Climb"].Obstacles["A"];
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
            ModelForApplyLogicalOptions.ApplyLogicalOptions(logicalOptions);

            // Expect
            Strat impossibleRequirementsStrat = ModelForApplyLogicalOptions.Rooms["Pants Room"].Links[4].To[5].Strats["Base"];
            Assert.False(impossibleRequirementsStrat.LogicallyRelevant);
            Assert.False(impossibleRequirementsStrat.LogicallyAlways);
            Assert.False(impossibleRequirementsStrat.LogicallyFree);
            Assert.True(impossibleRequirementsStrat.LogicallyNever);

            Strat disabledStrat = ModelForApplyLogicalOptions.Rooms["Blue Brinstar Energy Tank Room"].Links[1].To[3].Strats["Ceiling E-Tank Dboost"];
            Assert.False(disabledStrat.LogicallyRelevant);
            Assert.False(disabledStrat.LogicallyAlways);
            Assert.False(disabledStrat.LogicallyFree);
            Assert.True(disabledStrat.LogicallyNever);

            Strat nonFreeStrat = ModelForApplyLogicalOptions.Rooms["Blue Brinstar Energy Tank Room"].Links[1].To[3].Strats["Ceiling E-Tank Speed Jump"];
            Assert.True(nonFreeStrat.LogicallyRelevant);
            Assert.False(nonFreeStrat.LogicallyAlways);
            Assert.False(nonFreeStrat.LogicallyFree);
            Assert.False(nonFreeStrat.LogicallyNever);

            Strat freeStrat = ModelForApplyLogicalOptions.Rooms["Landing Site"].Links[5].To[4].Strats["Base"];
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
            ModelForApplyLogicalOptions.ApplyLogicalOptions(logicalOptions);

            // Expect
            StratFailure stratFailure = ModelForApplyLogicalOptions.Rooms["Early Supers Room"].Links[3].To[1].Strats["Early Supers Quick Crumble Escape (Space)"].Failures["Crumble Failure"];
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
                .ApplyAddExpansionItem((ExpansionItem)ModelForApplyLogicalOptions.Items["Missile"], 46)
                .ApplyAddExpansionItem((ExpansionItem)ModelForApplyLogicalOptions.Items["Super"], 10)
                .ApplyAddExpansionItem((ExpansionItem)ModelForApplyLogicalOptions.Items["ETank"], 14)
                .ApplyAddExpansionItem((ExpansionItem)ModelForApplyLogicalOptions.Items["ReserveTank"], 4);
            logicalOptions.InternalStartConditions = StartConditions.CreateVanillaStartConditionsBuilder(ModelForApplyLogicalOptions).StartingInventory(
                ItemInventory.CreateVanillaStartingInventory(ModelForApplyLogicalOptions)
                    .ApplyAddItem(ModelForApplyLogicalOptions.Items["Morph"])
                )
                .Build();

            // When
            ModelForApplyLogicalOptions.ApplyLogicalOptions(logicalOptions);

            // Expect
            StratObstacle fullyImpossibleStratObstacle = ModelForApplyLogicalOptions.Rooms["Climb"].Links[2].To[6].Strats["Base"].Obstacles["A"];
            Assert.True(fullyImpossibleStratObstacle.LogicallyRelevant);
            Assert.True(fullyImpossibleStratObstacle.LogicallyNever);
            Assert.True(fullyImpossibleStratObstacle.LogicallyNeverFromHere);
            Assert.False(fullyImpossibleStratObstacle.LogicallyAlways);
            Assert.False(fullyImpossibleStratObstacle.LogicallyFree);

            StratObstacle locallyIndestructibleFreeToBypassStratObstacle = ModelForApplyLogicalOptions.Rooms["Post Crocomire Jump Room"].Links[5].To[1].Strats["PCJR Frozen Mella Door"].Obstacles["B"];
            Assert.True(locallyIndestructibleFreeToBypassStratObstacle.LogicallyRelevant);
            Assert.False(locallyIndestructibleFreeToBypassStratObstacle.LogicallyNever);
            Assert.False(locallyIndestructibleFreeToBypassStratObstacle.LogicallyNeverFromHere);
            Assert.True(locallyIndestructibleFreeToBypassStratObstacle.LogicallyAlways);
            Assert.True(locallyIndestructibleFreeToBypassStratObstacle.LogicallyFree);

            StratObstacle locallyImpossibleStratObstacle = ModelForApplyLogicalOptions.Rooms["Post Crocomire Jump Room"].Nodes[2].CanLeaveCharged.First().Strats["Speed Blocks Broken"].Obstacles["B"];
            Assert.True(locallyImpossibleStratObstacle.LogicallyRelevant);
            Assert.False(locallyImpossibleStratObstacle.LogicallyNever);
            Assert.True(locallyImpossibleStratObstacle.LogicallyNeverFromHere);
            Assert.False(locallyImpossibleStratObstacle.LogicallyAlways);
            Assert.False(locallyImpossibleStratObstacle.LogicallyFree);

            StratObstacle freeToDestroyStratObstacle = ModelForApplyLogicalOptions.Rooms["Pink Brinstar Hopper Room"].Links[2].To[1].Strats["Base"].Obstacles["B"];
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
            ModelForApplyLogicalOptions.ApplyLogicalOptions(logicalOptions);

            // Expect
            Link noDestinationsLink = ModelForApplyLogicalOptions.Rooms["Crab Shaft"].Links[2];
            Assert.False(noDestinationsLink.LogicallyRelevant);

            Link possibleLink = ModelForApplyLogicalOptions.Rooms["Landing Site"].Links[1];
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
            ModelForApplyLogicalOptions.ApplyLogicalOptions(logicalOptions);

            // Expect
            LinkTo noDestinationsLinkTo = ModelForApplyLogicalOptions.Rooms["Crab Shaft"].Links[2].To[1];
            Assert.False(noDestinationsLinkTo.LogicallyRelevant);
            Assert.False(noDestinationsLinkTo.LogicallyAlways);
            Assert.False(noDestinationsLinkTo.LogicallyFree);
            Assert.True(noDestinationsLinkTo.LogicallyNever);

            LinkTo possibleLinkTo = ModelForApplyLogicalOptions.Rooms["Landing Site"].Links[1].To[7];
            Assert.True(possibleLinkTo.LogicallyRelevant);
            Assert.False(possibleLinkTo.LogicallyAlways);
            Assert.False(possibleLinkTo.LogicallyFree);
            Assert.False(possibleLinkTo.LogicallyNever);

            LinkTo freeLinkTo = ModelForApplyLogicalOptions.Rooms["Landing Site"].Links[5].To[4];
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
            logicalOptions.InternalStartConditions = StartConditions.CreateVanillaStartConditionsBuilder(ModelForApplyLogicalOptions)
                .StartingGameFlags(new List<GameFlag> { ModelForApplyLogicalOptions.GameFlags["f_DefeatedRidley"] })
                .Build();

            // When
            ModelForApplyLogicalOptions.ApplyLogicalOptions(logicalOptions);

            // Expect
            RoomEnemy alwaysSpawns = ModelForApplyLogicalOptions.RoomEnemies["Post Crocomire Farming Room Ripper 2"];
            Assert.True(alwaysSpawns.LogicallyRelevant);
            Assert.True(alwaysSpawns.LogicallyAlwaysSpawns);
            Assert.False(alwaysSpawns.LogicallyNeverSpawns);

            RoomEnemy neverMeetsSpawnConditions = ModelForApplyLogicalOptions.RoomEnemies["Bomb Torizo"];
            Assert.False(neverMeetsSpawnConditions.LogicallyRelevant);
            Assert.False(neverMeetsSpawnConditions.LogicallyAlwaysSpawns);
            Assert.True(neverMeetsSpawnConditions.LogicallyNeverSpawns);

            RoomEnemy alwaysMeetsStopSpawnConditions = ModelForApplyLogicalOptions.RoomEnemies["Ridley"];
            Assert.False(alwaysMeetsStopSpawnConditions.LogicallyRelevant);
            Assert.False(alwaysMeetsStopSpawnConditions.LogicallyAlwaysSpawns);
            Assert.True(alwaysMeetsStopSpawnConditions.LogicallyNeverSpawns);

            RoomEnemy notAlwaysSpawnConditions = ModelForApplyLogicalOptions.RoomEnemies["Attic Atomics"];
            Assert.True(notAlwaysSpawnConditions.LogicallyRelevant);
            Assert.False(notAlwaysSpawnConditions.LogicallyAlwaysSpawns);
            Assert.False(notAlwaysSpawnConditions.LogicallyNeverSpawns);

            RoomEnemy alwaysSpawnNotAlwaysStopSpawnConditions = ModelForApplyLogicalOptions.RoomEnemies["Flyway Mellows"];
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
            ModelForApplyLogicalOptions.ApplyLogicalOptions(logicalOptions);

            // Expect
            foreach (RoomEnvironment roomEnvironment in ModelForApplyLogicalOptions.Rooms["Volcano Room"].RoomEnvironments)
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
            ModelForApplyLogicalOptions.ApplyLogicalOptions(logicalOptions);

            // Expect
            FarmCycle impossibleCycle = ModelForApplyLogicalOptions.RoomEnemies["Gauntlet E-Tank Zebbo"].FarmCycles["Grapple three tiles away"];
            Assert.False(impossibleCycle.LogicallyRelevant);
            Assert.True(impossibleCycle.LogicallyNever);

            FarmCycle possibleCycle = ModelForApplyLogicalOptions.RoomEnemies["Gauntlet E-Tank Zebbo"].FarmCycles["Shoot and jump three tiles away"];
            Assert.True(possibleCycle.LogicallyRelevant);
            Assert.False(possibleCycle.LogicallyNever);
        }

        [Fact]
        public void ApplyLogicalOptions_SetsLogicalPropertiesOnNodes()
        {
            // Given
            LogicalOptions logicalOptions = new LogicalOptions();

            // When
            ModelForApplyLogicalOptions.ApplyLogicalOptions(logicalOptions);

            // Expect
            RoomNode node = ModelForApplyLogicalOptions.GetNodeInRoom("Landing Site", 5);
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
            ModelForApplyLogicalOptions.ApplyLogicalOptions(logicalOptions);

            // Expect
            foreach (DoorEnvironment doorEnvironment in ModelForApplyLogicalOptions.Rooms["Volcano Room"].Nodes[2].DoorEnvironments)
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
            logicalOptions.InternalStartConditions = StartConditions.CreateVanillaStartConditionsBuilder(ModelForApplyLogicalOptions).StartingInventory(
                ItemInventory.CreateVanillaStartingInventory(ModelForApplyLogicalOptions)
                    .ApplyAddItem(ModelForApplyLogicalOptions.Items[SuperMetroidModel.VARIA_SUIT_NAME])
                )
                .Build();

            // When
            ModelForApplyLogicalOptions.ApplyLogicalOptions(logicalOptions);

            // Expect
            Runway freeRunway = ModelForApplyLogicalOptions.GetNodeInRoom("Fast Pillars Setup Room", 2).Runways["Base Runway - Fast Pillars Setup Room Bottom Left Door (to Fast Rippers)"];
            Assert.True(freeRunway.LogicallyRelevant);
            Assert.True(freeRunway.LogicallyAlways);
            Assert.True(freeRunway.LogicallyFree);
            Assert.False(freeRunway.LogicallyNever);
            Assert.Equal(2 * (32M / 27M) + 10, freeRunway.LogicalEffectiveRunwayLength);
            Assert.Equal(2 * (32M / 27M) + 10, freeRunway.LogicalEffectiveReversibleRunwayLength);
            Assert.Equal(2 * (32M / 27M) + 10, freeRunway.LogicalEffectiveRunwayLengthNoCharge);

            Runway neverRunway = ModelForApplyLogicalOptions.GetNodeInRoom("Oasis", 2).Runways["Base Runway - Oasis Right Door (to East Sand Hall)"];
            Assert.False(neverRunway.LogicallyRelevant);
            Assert.False(neverRunway.LogicallyAlways);
            Assert.False(neverRunway.LogicallyFree);
            Assert.True(neverRunway.LogicallyNever);
            Assert.Equal(12, neverRunway.LogicalEffectiveRunwayLength);
            Assert.Equal(12, neverRunway.LogicalEffectiveReversibleRunwayLength);
            Assert.Equal(12, neverRunway.LogicalEffectiveRunwayLengthNoCharge);

            Runway possibleRunway = ModelForApplyLogicalOptions.GetNodeInRoom("Golden Torizo's Room", 2).Runways["Base Runway - Golden Torizo Room Right Door (to Screw Attack)"];
            Assert.True(possibleRunway.LogicallyRelevant);
            Assert.False(possibleRunway.LogicallyAlways);
            Assert.False(possibleRunway.LogicallyFree);
            Assert.False(possibleRunway.LogicallyNever);
            Assert.Equal(28, possibleRunway.LogicalEffectiveRunwayLength);
            Assert.Equal(28, possibleRunway.LogicalEffectiveReversibleRunwayLength);
            Assert.Equal(28, possibleRunway.LogicalEffectiveRunwayLengthNoCharge);
        }

        [Fact]
        public void ApplyLogicalOptions_SetsLogicalPropertiesOnInitiateRemotelys()
        {
            // Given
            LogicalOptions logicalOptions = new LogicalOptions()
                .RegisterRemovedItem("Morph");

            // When
            ModelForApplyLogicalOptions.ApplyLogicalOptions(logicalOptions);

            // Expect
            InitiateRemotely neverInitiateRemotely = ModelForApplyLogicalOptions.Rooms["Warehouse Kihunter Room"].Nodes[3].CanLeaveCharged.First().InitiateRemotely;
            Assert.True(neverInitiateRemotely.LogicallyRelevant);
            Assert.False(neverInitiateRemotely.LogicallyAlways);
            Assert.False(neverInitiateRemotely.LogicallyFree);
            Assert.True(neverInitiateRemotely.LogicallyNever);

            InitiateRemotely freeInitiateRemotely = ModelForApplyLogicalOptions.Rooms["Mt. Everest"].Nodes[3].CanLeaveCharged.First().InitiateRemotely;
            Assert.True(freeInitiateRemotely.LogicallyRelevant);
            Assert.True(freeInitiateRemotely.LogicallyAlways);
            Assert.True(freeInitiateRemotely.LogicallyFree);
            Assert.False(freeInitiateRemotely.LogicallyNever);

            InitiateRemotely possibleInitiateRemotely = ModelForApplyLogicalOptions.Rooms["Early Supers Room"].Nodes[2].CanLeaveCharged.First().InitiateRemotely;
            Assert.True(possibleInitiateRemotely.LogicallyRelevant);
            Assert.False(possibleInitiateRemotely.LogicallyAlways);
            Assert.False(possibleInitiateRemotely.LogicallyFree);
            Assert.False(possibleInitiateRemotely.LogicallyNever);
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
            ModelForApplyLogicalOptions.ApplyLogicalOptions(logicalOptions);

            // Expect
            CanLeaveCharged neverByImpossibleRemote = ModelForApplyLogicalOptions.Rooms["Warehouse Kihunter Room"].Nodes[3].CanLeaveCharged.First();
            Assert.False(neverByImpossibleRemote.LogicallyRelevant);
            Assert.False(neverByImpossibleRemote.LogicallyAlways);
            Assert.False(neverByImpossibleRemote.LogicallyFree);
            Assert.True(neverByImpossibleRemote.LogicallyNever);
            Assert.Equal(31.5M, neverByImpossibleRemote.LogicalEffectiveRunwayLength);

            CanLeaveCharged neverByImpossibleStrat = ModelForApplyLogicalOptions.Rooms["Mt. Everest"].Nodes[3].CanLeaveCharged.First();
            Assert.False(neverByImpossibleStrat.LogicallyRelevant);
            Assert.False(neverByImpossibleStrat.LogicallyAlways);
            Assert.False(neverByImpossibleStrat.LogicallyFree);
            Assert.True(neverByImpossibleStrat.LogicallyNever);
            Assert.Equal(20.5M, neverByImpossibleStrat.LogicalEffectiveRunwayLength);

            CanLeaveCharged neverByImpossibleShinespark = ModelForApplyLogicalOptions.Rooms["Spore Spawn Farming Room"].Nodes[1].CanLeaveCharged.First();
            Assert.False(neverByImpossibleShinespark.LogicallyRelevant);
            Assert.False(neverByImpossibleShinespark.LogicallyAlways);
            Assert.False(neverByImpossibleShinespark.LogicallyFree);
            Assert.True(neverByImpossibleShinespark.LogicallyNever);
            Assert.Equal(19M + 2 * 4M / 3M, neverByImpossibleShinespark.LogicalEffectiveRunwayLength);

            CanLeaveCharged neverByShortRunway = ModelForApplyLogicalOptions.Rooms["Green Brinstar Main Shaft / Etecoon Room"].Nodes[9].CanLeaveCharged.First();
            Assert.False(neverByShortRunway.LogicallyRelevant);
            Assert.False(neverByShortRunway.LogicallyAlways);
            Assert.False(neverByShortRunway.LogicallyFree);
            Assert.True(neverByShortRunway.LogicallyNever);
            Assert.Equal(17.5M, neverByShortRunway.LogicalEffectiveRunwayLength);

            CanLeaveCharged notFreeBecauseSpeedNotFree = ModelForApplyLogicalOptions.Rooms["Morph Ball Room"].Nodes[3].CanLeaveCharged.First();
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
            ModelForApplyLogicalOptions.ApplyLogicalOptions(logicalOptions);

            // Expect
            CanLeaveCharged neverByNoSpeedBooster = ModelForApplyLogicalOptions.Rooms["Morph Ball Room"].Nodes[3].CanLeaveCharged.First();
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
            logicalOptions.InternalStartConditions = StartConditions.CreateVanillaStartConditionsBuilder(ModelForApplyLogicalOptions).StartingInventory(
                ItemInventory.CreateVanillaStartingInventory(ModelForApplyLogicalOptions)
                    .ApplyAddItem(ModelForApplyLogicalOptions.Items[SuperMetroidModel.SPEED_BOOSTER_NAME])
                    .ApplyAddItem(ModelForApplyLogicalOptions.Items[SuperMetroidModel.GRAVITY_SUIT_NAME])
                )
                .Build();

            // When
            ModelForApplyLogicalOptions.ApplyLogicalOptions(logicalOptions);

            // Expect
            CanLeaveCharged free = ModelForApplyLogicalOptions.Rooms["Mt. Everest"].Nodes[3].CanLeaveCharged.First();
            Assert.True(free.LogicallyRelevant);
            Assert.True(free.LogicallyAlways);
            Assert.True(free.LogicallyFree);
            Assert.False(free.LogicallyNever);
            Assert.Equal(20.5M, free.LogicalEffectiveRunwayLength);

            // Expect
            CanLeaveCharged notFreeBecauseShinespark = ModelForApplyLogicalOptions.Rooms["Spore Spawn Farming Room"].Nodes[1].CanLeaveCharged.First();
            Assert.True(notFreeBecauseShinespark.LogicallyRelevant);
            Assert.False(notFreeBecauseShinespark.LogicallyAlways);
            Assert.False(notFreeBecauseShinespark.LogicallyFree);
            Assert.False(notFreeBecauseShinespark.LogicallyNever);
            Assert.Equal(19M + 2 * 4M / 3M, notFreeBecauseShinespark.LogicalEffectiveRunwayLength);

            // Expect
            CanLeaveCharged notFreeBecauseStratNotFree = ModelForApplyLogicalOptions.Rooms["Botwoon's Room"].Nodes[1].CanLeaveCharged.First();
            Assert.True(notFreeBecauseStratNotFree.LogicallyRelevant);
            Assert.False(notFreeBecauseStratNotFree.LogicallyAlways);
            Assert.False(notFreeBecauseStratNotFree.LogicallyFree);
            Assert.False(notFreeBecauseStratNotFree.LogicallyNever);
            Assert.Equal(16, notFreeBecauseStratNotFree.LogicalEffectiveRunwayLength);

            // Expect
            CanLeaveCharged notFreeBecauseRemoteNotFree = ModelForApplyLogicalOptions.Rooms["Red Brinstar Fireflea Room"].Nodes[1].CanLeaveCharged.First();
            Assert.True(notFreeBecauseRemoteNotFree.LogicallyRelevant);
            Assert.False(notFreeBecauseRemoteNotFree.LogicallyAlways);
            Assert.False(notFreeBecauseRemoteNotFree.LogicallyFree);
            Assert.False(notFreeBecauseRemoteNotFree.LogicallyNever);
            Assert.Equal(13, notFreeBecauseRemoteNotFree.LogicalEffectiveRunwayLength);
        }

        [Fact]
        public void ApplyLogicalOptions_SetsLogicalPropertiesOnTwinDoorAddresses()
        {

        }

        [Fact]
        public void ApplyLogicalOptions_SetsLogicalPropertiesOnNodeLocks()
        {

        }

        [Fact]
        public void ApplyLogicalOptions_SetsLogicalPropertiesOnLogicalRequirements()
        {

        }

        [Fact]
        public void ApplyLogicalOptions_SetsLogicalPropertiesOnOrs()
        {

        }

        [Fact]
        public void ApplyLogicalOptions_SetsLogicalPropertiesOnAnds()
        {

        }

        [Fact]
        public void ApplyLogicalOptions_SetsLogicalPropertiesOnTechLogicalElements()
        {

        }

        [Fact]
        public void ApplyLogicalOptions_SetsLogicalPropertiesOnNeverLogicalElements()
        {

        }

        [Fact]
        public void ApplyLogicalOptions_SetsLogicalPropertiesOnItemLogicalElements()
        {

        }

        [Fact]
        public void ApplyLogicalOptions_SetsLogicalPropertiesOnHelperLogicalElements()
        {

        }

        [Fact]
        public void ApplyLogicalOptions_SetsLogicalPropertiesOnGameFlagLogicalElements()
        {

        }

        [Fact]
        public void ApplyLogicalOptions_SetsLogicalPropertiesOnDamageLogicalElements ()
        {

        }

        [Fact]
        public void ApplyLogicalOptions_SetsLogicalPropertiesOnEnergyAtMost()
        {

        }

        [Fact]
        public void ApplyLogicalOptions_SetsLogicalPropertiesOnPreviousNodes()
        {

        }

        [Fact]
        public void ApplyLogicalOptions_SetsLogicalPropertiesOnAdjacentRunways()
        {

        }

        [Fact]
        public void ApplyLogicalOptions_SetsLogicalPropertiesOnAmmo()
        {

        }

        [Fact]
        public void ApplyLogicalOptions_SetsLogicalPropertiesOnAmmoDrain()
        {

        }

        [Fact]
        public void ApplyLogicalOptions_SetsLogicalPropertiesOnCanComeInCharged()
        {

        }

        [Fact]
        public void ApplyLogicalOptions_SetsLogicalPropertiesOnCanShineCharge()
        {

        }

        [Fact]
        public void ApplyLogicalOptions_SetsLogicalPropertiesOnEnemyDamage()
        {

        }

        [Fact]
        public void ApplyLogicalOptions_SetsLogicalPropertiesOnEnemyKill()
        {

        }

        [Fact]
        public void ApplyLogicalOptions_SetsLogicalPropertiesOnResetRoom()
        {

        }

        #endregion
    }
}
