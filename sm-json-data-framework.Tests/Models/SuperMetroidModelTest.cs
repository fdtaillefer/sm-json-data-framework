using sm_json_data_framework.Models;
using sm_json_data_framework.Models.Connections;
using sm_json_data_framework.Models.Enemies;
using sm_json_data_framework.Models.GameFlags;
using sm_json_data_framework.Models.Helpers;
using sm_json_data_framework.Models.Items;
using sm_json_data_framework.Models.Requirements.ObjectRequirements.Integers;
using sm_json_data_framework.Models.Requirements.ObjectRequirements.SubObjects;
using sm_json_data_framework.Models.Requirements.StringRequirements;
using sm_json_data_framework.Models.Rooms;
using sm_json_data_framework.Models.Rooms.Nodes;
using sm_json_data_framework.Models.Techs;
using sm_json_data_framework.Models.Weapons;
using sm_json_data_framework.Rules;
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

        #region Tests for Ctor(UnfinalizedSuperMetroidModel)

        [Fact]
        public void Ctor_AssignsAllData()
        {
            // Given
            UnfinalizedSuperMetroidModel unfinalizedModel = new UnfinalizedSuperMetroidModel(StaticTestObjects.RawModel);

            // When
            SuperMetroidModel model = new SuperMetroidModel(unfinalizedModel);

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
        public void Ctor_CreatesOneInstancePerUnfinalizedRoom()
        {
            // Given
            UnfinalizedSuperMetroidModel unfinalizedModel = new UnfinalizedSuperMetroidModel(StaticTestObjects.RawModel);

            // When
            SuperMetroidModel model = new SuperMetroidModel(unfinalizedModel);

            // Expect all properties that reference a room, to have the same instance as the one in the main model
            Room arbitraryRoom = model.Rooms["Climb"];
            RoomNode node = model.GetNodeInRoom("Climb", 1);
            Assert.Same(arbitraryRoom, node.Room);

            RoomEnvironment roomEnvironment = model.Rooms["Climb"].RoomEnvironments[0];
            Assert.Same(arbitraryRoom, roomEnvironment.Room);

            RoomEnemy roomEnemy = model.RoomEnemies["Climb Pirates"];
            Assert.Same(arbitraryRoom, roomEnemy.Room);

            RoomObstacle roomObstacle = model.Rooms["Climb"].Obstacles["A"];
            Assert.Same(arbitraryRoom, roomObstacle.Room);
        }

        [Fact]
        public void Ctor_CreatesOneInstancePerUnfinalizedNode()
        {
            // Given
            UnfinalizedSuperMetroidModel unfinalizedModel = new UnfinalizedSuperMetroidModel(StaticTestObjects.RawModel);

            // When
            SuperMetroidModel model = new SuperMetroidModel(unfinalizedModel);

            // Expect all properties that reference a node, to have the same instance as the one in the Room model
            // Nodes in models
            RoomNode arbitraryNode = model.GetNodeInRoom("Big Pink", 14);
            RoomNode node = model.GetNodeInRoom("Big Pink", 5);
            Assert.Same(arbitraryNode, node.SpawnAtNode);

            LinkTo linkTo = model.Rooms["Big Pink"].Links[5].To[14];
            Assert.Same(arbitraryNode, linkTo.TargetNode);

            Assert.Same(arbitraryNode, model.Nodes["Big Pink X-Ray Climb Setup Junction"]);

            RoomNode arbitraryNode2 = model.GetNodeInRoom("Landing Site", 1);
            CanLeaveCharged canLeaveCharged = model.GetNodeInRoom("Landing Site", 1).CanLeaveCharged[0];
            Assert.Same(arbitraryNode2, canLeaveCharged.Node);

            InitiateRemotely initiateRemotely = model.GetNodeInRoom("Landing Site", 1).CanLeaveCharged[0].InitiateRemotely;
            Assert.Same(arbitraryNode2, initiateRemotely.ExitNode);

            DoorEnvironment doorEnvironment = model.GetNodeInRoom("Landing Site", 1).DoorEnvironments[0];
            Assert.Same(arbitraryNode2, doorEnvironment.Node);

            NodeLock nodeLock = model.Locks["Landing Site Top Left Escape Lock (to Gauntlet)"];
            Assert.Same(arbitraryNode2, nodeLock.Node);

            node = model.GetNodeInRoom("Gauntlet Entrance", 2);
            Assert.Same(arbitraryNode2, node.OutNode);

            Runway runway = model.GetNodeInRoom("Landing Site", 1).Runways["Base Runway - Landing Site Top Left Door (to Gauntlet)"];
            Assert.Same(arbitraryNode2, runway.Node);

            RoomNode arbitraryNode3 = model.GetNodeInRoom("Landing Site", 3);
            initiateRemotely = model.GetNodeInRoom("Landing Site", 1).CanLeaveCharged[0].InitiateRemotely;
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
            PreviousNode previousNodeLogicalElement = (PreviousNode)model.Rooms["Pink Brinstar Power Bomb Room"].Links[3].To[4].Strats["Mission Impossible"].Requires.LogicalElements
                .Where(element => typeof(PreviousNode).IsAssignableFrom(element.GetType()))
                .First();
            Assert.Same(arbitraryNode10, previousNodeLogicalElement.Node);

            // AdjacentRunway
            RoomNode arbitraryNode11 = model.GetNodeInRoom("Construction Zone", 3);
            AdjacentRunway adjacentRunway = (AdjacentRunway)model.Rooms["Construction Zone"].Links[3].To[4].Strats["Construction Room X-Ray Climb"].Requires.LogicalElements
                .Where(element => typeof(AdjacentRunway).IsAssignableFrom(element.GetType()))
                .First();
            Assert.Same(arbitraryNode11, adjacentRunway.FromNode);
            // ResetRoom
            ResetRoom resetRoom = (ResetRoom)model.Rooms["Construction Zone"].Links[3].To[4].Strats["Construction Room X-Ray Climb"].Requires.LogicalElements
                .Where(element => typeof(ResetRoom).IsAssignableFrom(element.GetType()))
                .First();
            Assert.Same(arbitraryNode11, resetRoom.Nodes[3]);

            RoomNode arbitraryNode12 = model.GetNodeInRoom("Big Pink", 13);
            resetRoom = (ResetRoom)model.Rooms["Big Pink"].Links[5].To[4].Strats["Big Pink Left-Side X-Ray Climb"].Requires.LogicalElements
                .Where(element => typeof(ResetRoom).IsAssignableFrom(element.GetType()))
                .First();
            Assert.Same(arbitraryNode12, resetRoom.NodesToAvoid[13]);

            // CanComeInCharged
            RoomNode arbitraryNode13 = model.GetNodeInRoom("Green Brinstar Main Shaft / Etecoon Room", 10);
            CanComeInCharged canComeInCharged = (CanComeInCharged)model.Rooms["Green Brinstar Main Shaft / Etecoon Room"].Links[10].To[9].Strats["Shinespark"].Requires.LogicalElements
                .Where(element => typeof(CanComeInCharged).IsAssignableFrom(element.GetType()))
                .First();
            Assert.Same(arbitraryNode13, canComeInCharged.FromNode);
        }

        [Fact]
        public void Ctor_CreatesOneInstancePerUnfinalizedStrat()
        {
            // Given
            UnfinalizedSuperMetroidModel unfinalizedModel = new UnfinalizedSuperMetroidModel(StaticTestObjects.RawModel);

            // When
            SuperMetroidModel model = new SuperMetroidModel(unfinalizedModel);

            // Expect all properties that reference a strat, to have the same instance as the one in the Room model
            Strat arbitraryStrat = model.Rooms["Landing Site"].Links[3].To[1].Strats["Shinespark"];
            var initiateRemotelyPathToDoorNode = model.GetNodeInRoom("Landing Site", 1).CanLeaveCharged.First().InitiateRemotely.PathToDoor[0];
            Assert.Same(arbitraryStrat, initiateRemotelyPathToDoorNode.strats["Shinespark"]);
        }

        [Fact]
        public void Ctor_CreatesOneInstancePerUnfinalizedEnemy()
        {
            // Given
            UnfinalizedSuperMetroidModel unfinalizedModel = new UnfinalizedSuperMetroidModel(StaticTestObjects.RawModel);

            // When
            SuperMetroidModel model = new SuperMetroidModel(unfinalizedModel);

            // Expect all properties that reference an enemy, to have the same instance as the one in the main model
            Enemy arbitraryEnemy = model.Enemies["Sidehopper"];
            RoomEnemy roomEnemy = model.RoomEnemies["Morph Ball Room Sidehoppers"];
            Assert.Same(arbitraryEnemy, roomEnemy.Enemy);

            EnemyDamage enemyDamage = (EnemyDamage) model.Rooms["Morph Ball Room"].Links[1].To[6].Strats["Run Through"].Requires.LogicalElements
                .Where(element => typeof(EnemyDamage).IsAssignableFrom(element.GetType()))
                .First();
            Assert.Same(arbitraryEnemy, enemyDamage.Enemy);

            EnemyKill enemyKill = (EnemyKill)model.Rooms["Morph Ball Room"].Links[1].To[6].Strats["PB Sidehopper Kill with Bomb Blocks"].Obstacles["C"].Requires.LogicalElements
                .Where(element => typeof(EnemyKill).IsAssignableFrom(element.GetType()))
                .First();
            Assert.Same(arbitraryEnemy, enemyKill.GroupedEnemies.First().First());
        }

        [Fact]
        public void Ctor_CreatesOneInstancePerUnfinalizedWeapon()
        {
            // Given
            UnfinalizedSuperMetroidModel unfinalizedModel = new UnfinalizedSuperMetroidModel(StaticTestObjects.RawModel);

            // When
            SuperMetroidModel model = new SuperMetroidModel(unfinalizedModel);

            // Expect all properties that reference a weapon, to have the same instance as the one in the main model
            Weapon arbitraryWeapon = model.Weapons["Missile"];
            WeaponMultiplier weaponMultiplier = model.Enemies["Alcoon"].WeaponMultipliers["Missile"];
            Assert.Same(arbitraryWeapon, weaponMultiplier.Weapon);

            WeaponSusceptibility weaponSusceptibility = model.Enemies["Alcoon"].WeaponSusceptibilities["Missile"];
            Assert.Same(arbitraryWeapon, weaponSusceptibility.Weapon);

            Enemy enemy = model.Enemies["Boyon"];
            Assert.Same(arbitraryWeapon, enemy.InvulnerableWeapons["Missile"]);

            EnemyKill enemyKill = (EnemyKill)model.Rooms["Pink Brinstar Power Bomb Room"].Links[1].To[4].Strats["Good Weapon Sidehopper Kill"].Obstacles["A"].Requires.LogicalElements
                .Where(element => typeof(EnemyKill).IsAssignableFrom(element.GetType()))
                .First();
            Assert.Same(arbitraryWeapon, enemyKill.ExplicitWeapons["Missile"]);

            Assert.Same(arbitraryWeapon, enemyKill.ValidWeapons["Missile"]);

            enemyKill = (EnemyKill)model.Rooms["Metroid Room 1"].Links[1].To[3].Strats["Tank and PB Kill"].Requires.LogicalElements
                .Where(element => typeof(EnemyKill).IsAssignableFrom(element.GetType()))
                .First();
            Assert.Same(arbitraryWeapon, enemyKill.ExcludedWeapons["Missile"]);
        }

        [Fact]
        public void Ctor_CreatesOneInstancePerUnfinalizedWeaponMultiplier()
        {
            // Given
            UnfinalizedSuperMetroidModel unfinalizedModel = new UnfinalizedSuperMetroidModel(StaticTestObjects.RawModel);

            // When
            SuperMetroidModel model = new SuperMetroidModel(unfinalizedModel);

            // Expect all properties that reference a weaponMultiplier, to have the same instance as the one in the enemy
            WeaponMultiplier arbitraryWeaponMultiplier = model.Enemies["Alcoon"].WeaponMultipliers["Missile"];
            WeaponSusceptibility weaponSusceptibility = model.Enemies["Alcoon"].WeaponSusceptibilities["Missile"];
            Assert.Same(arbitraryWeaponMultiplier, weaponSusceptibility.WeaponMultiplier);
        }

        [Fact]
        public void Ctor_CreatesOneInstancePerUnfinalizedTech()
        {
            // Given
            UnfinalizedSuperMetroidModel unfinalizedModel = new UnfinalizedSuperMetroidModel(StaticTestObjects.RawModel);

            // When
            SuperMetroidModel model = new SuperMetroidModel(unfinalizedModel);

            // Expect all properties that reference a tech, to have the same instance as the one in the main model
            Tech arbitraryTech = model.Techs["canDelayedWalljump"];
            TechLogicalElement techLogicalElement = (TechLogicalElement)model.Rooms["Landing Site"].Links[5].To[7].Strats["Gauntlet Walljumps"].Requires.LogicalElements
                .Where(element => typeof(TechLogicalElement).IsAssignableFrom(element.GetType()))
                .First();
            Assert.Same(arbitraryTech, techLogicalElement.Tech);
            Tech tech = model.Techs["canPreciseWalljump"];
            Assert.Same(arbitraryTech, tech.ExtensionTechs["canDelayedWalljump"]);
        }

        [Fact]
        public void Ctor_CreatesOneInstancePerUnfinalizedHelper()
        {
            // Given
            UnfinalizedSuperMetroidModel unfinalizedModel = new UnfinalizedSuperMetroidModel(StaticTestObjects.RawModel);

            // When
            SuperMetroidModel model = new SuperMetroidModel(unfinalizedModel);

            // Expect all properties that reference a helper, to have the same instance as the one in the main model
            Helper arbitraryHelper = model.Helpers["h_canDestroyBombWalls"];
            HelperLogicalElement helperLogicalElement = (HelperLogicalElement)model.Rooms["Landing Site"].Links[1].To[7].Strats["Base"].Obstacles["A"].Requires.LogicalElements
                .Where(element => typeof(HelperLogicalElement).IsAssignableFrom(element.GetType()))
                .First();
            Assert.Same(arbitraryHelper, helperLogicalElement.Helper);
        }

        [Fact]
        public void Ctor_CreatesOneInstancePerUnfinalizedItem()
        {
            // Given
            UnfinalizedSuperMetroidModel unfinalizedModel = new UnfinalizedSuperMetroidModel(StaticTestObjects.RawModel);

            // When
            SuperMetroidModel model = new SuperMetroidModel(unfinalizedModel);

            // Expect all properties that reference an item, to have the same instance as the one in the main model
            Item arbitraryItem = model.Items["Morph"];
            RoomNode node = model.GetNodeInRoom("Morph Ball Room", 4);
            Assert.Same(arbitraryItem, node.NodeItem);

            ItemLogicalElement itemLogicalElement = (ItemLogicalElement)model.Rooms["Parlor and Alcatraz"].Links[2].To[8].Strats["Base"].Requires.LogicalElements
                .Where(element => typeof(ItemLogicalElement).IsAssignableFrom(element.GetType()))
                .First();
            Assert.Same(arbitraryItem, itemLogicalElement.Item);
        }

        [Fact]
        public void Ctor_CreatesOneInstancePerUnfinalizedGameFlag()
        {
            // Given
            UnfinalizedSuperMetroidModel unfinalizedModel = new UnfinalizedSuperMetroidModel(StaticTestObjects.RawModel);

            // When
            SuperMetroidModel model = new SuperMetroidModel(unfinalizedModel);

            // Expect all properties that reference a game flag, to have the same instance as the one in the main model
            GameFlag arbitraryGameFlag = model.GameFlags["f_ZebesAwake"];
            NodeLock nodeLock = model.Locks["Pit Room Left Grey Lock (to Climb)"];
            Assert.Same(arbitraryGameFlag, nodeLock.Yields["f_ZebesAwake"]);

            GameFlag arbitraryGameFlag2 = model.GameFlags["f_DefeatedSporeSpawn"];
            RoomNode node = model.GetNodeInRoom("Spore Spawn Room", 3);
            Assert.Same(arbitraryGameFlag2, node.Yields["f_DefeatedSporeSpawn"]);

            GameFlagLogicalElement gameFlagLogicalElement = (GameFlagLogicalElement)model.RoomEnemies["Spore Spawn"].StopSpawn.LogicalElements
                .Where(element => typeof(GameFlagLogicalElement).IsAssignableFrom(element.GetType()))
                .First();
            Assert.Same(arbitraryGameFlag2, gameFlagLogicalElement.GameFlag);
        }

        [Fact]
        public void Ctor_CreatesOneInstancePerUnfinalizedRoomEnemy()
        {
            // Given
            UnfinalizedSuperMetroidModel unfinalizedModel = new UnfinalizedSuperMetroidModel(StaticTestObjects.RawModel);

            // When
            SuperMetroidModel model = new SuperMetroidModel(unfinalizedModel);

            // Expect all properties that reference a room enemy, to have the same instance as the one in the room
            RoomEnemy arbitraryRoomEnemy = model.Rooms["West Ocean"].Enemies["e1"];
            Assert.Same(arbitraryRoomEnemy, model.RoomEnemies["West Ocean Zeb"]);

            FarmCycle farmCycle = model.RoomEnemies["West Ocean Zeb"].FarmCycles["Crouch over spawn point"];
            Assert.Same(arbitraryRoomEnemy, farmCycle.RoomEnemy);
        }

        [Fact]
        public void Ctor_CreatesOneInstancePerUnfinalizedRoomObstacle()
        {
            // Given
            UnfinalizedSuperMetroidModel unfinalizedModel = new UnfinalizedSuperMetroidModel(StaticTestObjects.RawModel);

            // When
            SuperMetroidModel model = new SuperMetroidModel(unfinalizedModel);

            // Expect all properties that reference a room obstacle, to have the same instance as the one in the room
            RoomObstacle arbitraryObstacle = model.Rooms["Green Brinstar Main Shaft / Etecoon Room"].Obstacles["A"];
            ResetRoom resetRoom = (ResetRoom)model.Runways["Base Runway - Green Shaft Mid-Low Left Door (to Firefleas)"].Strats["Base"].Requires.LogicalElements
                .Where(element => typeof(ResetRoom).IsAssignableFrom(element.GetType()))
                .First();
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
            // Given
            UnfinalizedSuperMetroidModel unfinalizedModel = new UnfinalizedSuperMetroidModel(StaticTestObjects.RawModel);

            // When
            SuperMetroidModel model = new SuperMetroidModel(unfinalizedModel);

            // Expect all properties that reference a connection, to have the same instance as the one in the main model
            Connection arbitraryConnection = model.Connections[model.GetNodeInRoom("Landing Site", 1).IdentifyingString];
            RoomNode node = model.GetNodeInRoom("Landing Site", 1);
            Assert.Same(arbitraryConnection, node.OutConnection);
        }

        [Fact]
        public void Ctor_CreatesOneInstancePerUnfinalizedLinkTo()
        {
            // Given
            UnfinalizedSuperMetroidModel unfinalizedModel = new UnfinalizedSuperMetroidModel(StaticTestObjects.RawModel);

            // When
            SuperMetroidModel model = new SuperMetroidModel(unfinalizedModel);

            // Expect all properties that reference a LinkTo, to have the same instance as the one in the parent Link
            LinkTo arbitraryLinkTo = model.Rooms["Landing Site"].Links[3].To[1];
            var initiateRemotelyPathToDoorNode = model.GetNodeInRoom("Landing Site", 1).CanLeaveCharged.First().InitiateRemotely.PathToDoor[0];
            Assert.Same(arbitraryLinkTo, initiateRemotelyPathToDoorNode.linkTo);

            RoomNode node = model.GetNodeInRoom("Landing Site", 3);
            Assert.Same(arbitraryLinkTo, node.LinksTo[1]);
        }

        // Then a test for optional stuff??

        #endregion
    }
}
