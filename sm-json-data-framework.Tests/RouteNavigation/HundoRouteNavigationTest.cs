using sm_json_data_framework.Models;
using sm_json_data_framework.Models.InGameStates;
using sm_json_data_framework.Models.Navigation;
using sm_json_data_framework.Models.Requirements;
using sm_json_data_framework.Tests.TestTools;
using sm_json_data_framework.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace sm_json_data_framework.Tests.RouteNavigation
{
    /// <summary>
    /// A class that is able to interface with a GameNavigator to run predefined operations, going through the game grabbing every item and beating the game. 
    /// This interface expects the GameNavigator to be at the start of the game, otherwise it's unclear where the attempts to navigate the game will go.
    /// This will NOT run the hundo speedrun route.
    /// </summary>
    public class HundoRouteNavigationTest
    {
        private static SuperMetroidModel ReusableModel() => StaticTestObjects.UnmodifiableModel;

        [Fact]
        public void HundoRouteTest()
        {
            // Given
            SuperMetroidModel model = ReusableModel();
            GameNavigator navigator = model.CreateInitialGameNavigator(maxPreviousStatesSize: 2);

            // When
            RunRoute(navigator);

            // Expect
            Assert.Equal(100, navigator.CurrentInGameState.Inventory.InGameItemCount);
            Assert.Contains("f_BeatSuperMetroid", navigator.CurrentInGameState.ActiveGameFlags.Keys);
        }

        private void RunRoute(GameNavigator navigator)
        {
            ExecuteCeres(navigator);
            ShipToBlueBrin(navigator);
            BlueBrinToAwake(navigator);
            AwakeBlueBrinVisitToParlor(navigator);
            ParlorToBombsToOutsideAlcatraz(navigator);
            BombMissilePickupsAndShipRefill(navigator);
            GauntletToGreenBrin(navigator);
            GreenBrinEarlyVisit(navigator);
            PinkBrinEarlyVisit(navigator);
            GreenHillToAlphaPbsToCrateria(navigator);
            CrateriaElevatorToShipRefill(navigator);
            GreenBrinSecondPass(navigator);
            PinkBrinSecondPass(navigator);
            BlueBrinSecondPass(navigator);
            GreenHillDownRedTower(navigator);
            Kraid(navigator);
            HiJumpIce(navigator);
            EastNorfair(navigator);
            RememberedHighwayToCroc(navigator);
            CrocLoop(navigator);
            EarlySpeedCleanup(navigator);
            WreckedShipAttic(navigator);
            WreckedShipCleanup(navigator);
            ForgottenHighwayToMaridiaTube(navigator);
            OuterMaridiaToAqueduct(navigator);
            AqueductAndSandHoleLoops(navigator);
            BotwoonAndDraygon(navigator);
            BusinessCenterToScrew(navigator);
            ScrewToRidley(navigator);
            NorfairEscape(navigator);
            ToG4(navigator);
            TourianToMotherBrain(navigator);
            TourianEscapeToShip(navigator);
        }

        private void ExecuteCeres(GameNavigator navigator)
        {
            // Enter at Ceres Elevator
            navigator.InteractWithNode().AssertFailed();
            navigator.MoveToNode(2).AssertSucceeded();
            navigator.InteractWithNode().AssertSucceeded();

            // Falling Tiles
            navigator.MoveToNode(2).AssertSucceeded();
            navigator.InteractWithNode().AssertSucceeded();

            // Magnet Stairs
            navigator.MoveToNode(2).AssertSucceeded();
            navigator.InteractWithNode().AssertSucceeded();

            // Dead Scientists
            navigator.MoveToNode(2).AssertSucceeded();
            navigator.InteractWithNode().AssertSucceeded();

            // 58 Escape
            navigator.MoveToNode(2).AssertSucceeded();
            navigator.InteractWithNode().AssertSucceeded();

            // Ceres Ridley
            navigator.InteractWithNode().AssertFailed();
            navigator.MoveToNode(2).AssertSucceeded();
            navigator.InteractWithNode().AssertSucceeded();
            navigator.MoveToNode(1).AssertSucceeded();
            navigator.InteractWithNode().AssertSucceeded();

            // 58 Escape
            navigator.MoveToNode(1).AssertSucceeded();
            navigator.InteractWithNode().AssertSucceeded();

            // Dead Scientists
            navigator.MoveToNode(1).AssertSucceeded();
            navigator.InteractWithNode().AssertSucceeded();

            // Magnet Stairs
            navigator.MoveToNode(1).AssertSucceeded();
            navigator.InteractWithNode().AssertSucceeded();

            // Falling Tiles
            navigator.MoveToNode(1).AssertSucceeded();
            navigator.InteractWithNode().AssertSucceeded();

            // Ceres Elevator
            navigator.MoveToNode(1).AssertSucceeded();
            navigator.InteractWithNode().AssertSucceeded();

            // Now at ship?
            navigator.InteractWithNode().AssertSucceeded();
        }

        private void ShipToBlueBrin(GameNavigator navigator)
        {
            // Landing Site
            navigator.MoveToNode(2).AssertSucceeded();
            navigator.InteractWithNode().AssertSucceeded();

            // Parlor
            navigator.MoveToNode(8).AssertSucceeded();
            navigator.MoveToNode(7).AssertSucceeded();
            navigator.InteractWithNode().AssertSucceeded();

            // Climb
            navigator.MoveToNode(6).AssertSucceeded();
            navigator.MoveToNode(5).AssertSucceeded();
            navigator.InteractWithNode().AssertSucceeded();

            // Pit Room
            navigator.InteractWithNode().AssertFailed();
            navigator.MoveToNode(2).AssertSucceeded();
            navigator.InteractWithNode().AssertSucceeded();

            // Blue Brin Elevator Room
            navigator.MoveToNode(2).AssertSucceeded();
            navigator.InteractWithNode().AssertSucceeded();
        }

        private void BlueBrinToAwake(GameNavigator navigator)
        {
            // Morph Ball Room
            navigator.MoveToNode(4).AssertSucceeded();
            navigator.InteractWithNode().AssertSucceeded();
            navigator.MoveToNode(2).AssertSucceeded();
            navigator.MoveToNode(3).AssertSucceeded();
            navigator.InteractWithNode().AssertSucceeded();

            // Construction Zone
            navigator.MoveToNode(4).AssertSucceeded();
            navigator.MoveToNode(3).AssertSucceeded();
            navigator.InteractWithNode().AssertSucceeded();

            // Alpha Missiles
            navigator.MoveToNode(2).AssertSucceeded();
            navigator.InteractWithNode().AssertSucceeded();
            navigator.MoveToNode(1).AssertSucceeded();
            navigator.InteractWithNode().AssertSucceeded();

            // Construction Zone
            navigator.MoveToNode(4).AssertSucceeded();
            navigator.MoveToNode(1).AssertSucceeded();
            navigator.InteractWithNode().AssertSucceeded();

            // Morph Ball Room
            navigator.MoveToNode(2).AssertSucceeded();
            navigator.InteractWithNode().AssertSucceeded();

            // Blue Brinstar Elevator
            navigator.MoveToNode(1).AssertSucceeded();
            navigator.InteractWithNode().AssertSucceeded();

            // Pit Room
            navigator.InteractWithNode().AssertSucceeded();
        }

        private void AwakeBlueBrinVisitToParlor(GameNavigator navigator)
        {
            // Blue Brin Elevator Room
            navigator.MoveToNode(2).AssertSucceeded();
            navigator.InteractWithNode().AssertSucceeded();

            // Morph Ball Room
            navigator.MoveToNode(3).AssertSucceeded();
            navigator.InteractWithNode().AssertSucceeded();

            // Construction Zone
            navigator.MoveToNode(4).AssertSucceeded();
            navigator.MoveToNode(2).AssertSucceeded();
            navigator.InteractWithNode().AssertSucceeded();

            // Ceiling E-Tank Room
            navigator.MoveToNode(3).AssertSucceeded();
            navigator.InteractWithNode().AssertSucceeded();
            navigator.MoveToNode(1).AssertSucceeded();
            navigator.MoveToNode(4).AssertSucceeded();
            navigator.InteractWithNode().AssertSucceeded();
            navigator.MoveToNode(1).AssertSucceeded();
            navigator.InteractWithNode().AssertSucceeded();

            // Construction Zone
            navigator.MoveToNode(4).AssertSucceeded();
            navigator.MoveToNode(1).AssertSucceeded();
            navigator.InteractWithNode().AssertSucceeded();

            // Morph Ball Room
            navigator.MoveToNode(2).AssertSucceeded();
            navigator.InteractWithNode().AssertSucceeded();

            // Blue Brinstar Elevator
            navigator.MoveToNode(1).AssertSucceeded();
            navigator.InteractWithNode().AssertSucceeded();

            // Pit Room
            navigator.MoveToNode(1).AssertSucceeded();
            navigator.InteractWithNode().AssertSucceeded();

            // Climb
            navigator.MoveToNode(6).AssertSucceeded();
            navigator.MoveToNode(1).AssertSucceeded();
            navigator.InteractWithNode().AssertSucceeded();
        }

        private void ParlorToBombsToOutsideAlcatraz(GameNavigator navigator)
        {
            // Parlor
            navigator.MoveToNode(8).AssertSucceeded();
            navigator.MoveToNode(5).AssertSucceeded();
            navigator.InteractWithNode().AssertSucceeded();

            // Flyway
            navigator.MoveToNode(2).AssertSucceeded();
            navigator.InteractWithNode().AssertSucceeded();

            // Bomb Torizo
            navigator.MoveToNode(3).AssertSucceeded();
            navigator.MoveToNode(2).AssertSucceeded();
            navigator.InteractWithNode().AssertSucceeded();
            navigator.MoveToNode(3).AssertSucceeded();
            navigator.InteractWithNode().AssertSucceeded();
            navigator.MoveToNode(1).AssertSucceeded();
            navigator.InteractWithNode().AssertSucceeded();

            // Flyway
            navigator.MoveToNode(1).AssertSucceeded();
            navigator.InteractWithNode().AssertSucceeded();

            // Parlor
            navigator.MoveToNode(8).AssertSucceeded();
        }

        private void BombMissilePickupsAndShipRefill(GameNavigator navigator)
        {
            // Parlor
            navigator.MoveToNode(3).AssertSucceeded();
            navigator.InteractWithNode().AssertSucceeded();

            // Final Missile Bombway
            navigator.MoveToNode(1).AssertSucceeded();
            navigator.InteractWithNode().AssertSucceeded();

            // Final Missile
            navigator.MoveToNode(2).AssertSucceeded();
            navigator.InteractWithNode().AssertSucceeded();
            navigator.MoveToNode(1).AssertSucceeded();
            navigator.InteractWithNode().AssertSucceeded();

            // Final Missile Bombway
            navigator.MoveToNode(2).AssertSucceeded();
            navigator.InteractWithNode().AssertSucceeded();

            // Parlor
            navigator.MoveToNode(8).AssertSucceeded();
            navigator.MoveToNode(7).AssertSucceeded();
            navigator.InteractWithNode().AssertSucceeded();

            // Climb
            navigator.MoveToNode(6).AssertSucceeded();
            navigator.MoveToNode(5).AssertSucceeded();
            navigator.InteractWithNode().AssertSucceeded();

            // Pit Room
            navigator.MoveToNode(3).AssertSucceeded();
            navigator.InteractWithNode().AssertSucceeded();
            navigator.MoveToNode(1).AssertSucceeded();
            navigator.InteractWithNode().AssertSucceeded();

            // Climb
            navigator.MoveToNode(6).AssertSucceeded();
            navigator.MoveToNode(1).AssertSucceeded();
            navigator.InteractWithNode().AssertSucceeded();

            // Parlor
            navigator.MoveToNode(8).AssertSucceeded();
            navigator.MoveToNode(4).AssertSucceeded();
            navigator.InteractWithNode().AssertSucceeded();

            // Landing Site
            navigator.MoveToNode(5).AssertSucceeded();
            navigator.InteractWithNode().AssertSucceeded();
        }

        private void GauntletToGreenBrin(GameNavigator navigator)
        {
            // Landing Site
            navigator.MoveToNode(7).AssertSucceeded();
            navigator.MoveToNode(1).AssertSucceeded();
            navigator.InteractWithNode().AssertSucceeded();

            // Gauntlet Entrance
            navigator.MoveToNode(1).AssertSucceeded();
            navigator.InteractWithNode().AssertSucceeded();

            // Gauntlet E-Tank
            navigator.MoveToNode(3).AssertSucceeded();
            navigator.InteractWithNode().AssertSucceeded();
            navigator.MoveToNode(4).AssertSucceeded();
            navigator.MoveToNode(1).AssertSucceeded();
            navigator.InteractWithNode().AssertSucceeded();

            // Green Pirates Shaft
            navigator.MoveToNode(8).AssertSucceeded();
            navigator.MoveToNode(5).AssertSucceeded();
            navigator.InteractWithNode().AssertSucceeded();
            navigator.MoveToNode(8).AssertSucceeded();
            navigator.MoveToNode(6).AssertSucceeded();
            navigator.InteractWithNode().AssertSucceeded();
            navigator.MoveToNode(8).AssertSucceeded();
            navigator.MoveToNode(9).AssertSucceeded();
            navigator.MoveToNode(2).AssertSucceeded();
            navigator.InteractWithNode().AssertSucceeded();

            // Terminator
            navigator.MoveToNode(3).AssertSucceeded();
            navigator.InteractWithNode().AssertSucceeded();
            navigator.MoveToNode(1).AssertSucceeded();
            navigator.InteractWithNode().AssertSucceeded();

            // Green Pirates Shaft
            navigator.MoveToNode(4).AssertSucceeded();
            navigator.MoveToNode(3).AssertSucceeded();
            navigator.InteractWithNode().AssertSucceeded();

            // Lower Mushrooms
            navigator.MoveToNode(1).AssertSucceeded();
            navigator.InteractWithNode().AssertSucceeded();

            // Green Brin Elevator
            navigator.MoveToNode(2).AssertSucceeded();
            navigator.InteractWithNode().AssertSucceeded();
        }

        private void GreenBrinEarlyVisit(GameNavigator navigator)
        {
            // Green Shaft
            navigator.MoveToNode(12).AssertSucceeded();
            navigator.MoveToNode(3).AssertSucceeded();
            navigator.InteractWithNode().AssertSucceeded();

            // Early Supers
            navigator.MoveToNode(2).AssertSucceeded();
            navigator.InteractWithNode().AssertSucceeded();

            // Brinstar Reserve
            navigator.MoveToNode(2).AssertSucceeded();
            navigator.InteractWithNode().AssertSucceeded();
            navigator.MoveToNode(3).AssertSucceeded();
            navigator.InteractWithNode().AssertSucceeded();
            navigator.MoveToNode(4).AssertSucceeded();
            navigator.InteractWithNode().AssertSucceeded();
            navigator.MoveToNode(3).AssertSucceeded();
            navigator.MoveToNode(2).AssertSucceeded();
            navigator.MoveToNode(1).AssertSucceeded();
            navigator.InteractWithNode().AssertSucceeded();

            // Early Supers
            navigator.MoveToNode(4).AssertSucceeded();
            navigator.InteractWithNode().AssertSucceeded();
            navigator.MoveToNode(1).AssertSucceeded();
            navigator.MoveToNode(3).AssertSucceeded();
            navigator.InteractWithNode().AssertSucceeded();
            navigator.MoveToNode(1).AssertSucceeded();
            navigator.InteractWithNode().AssertSucceeded();

            // Green Shaft
            navigator.MoveToNode(12).AssertSucceeded();
            navigator.MoveToNode(6).AssertSucceeded();
            navigator.InteractWithNode().AssertSucceeded();
        }

        private void PinkBrinEarlyVisit(GameNavigator navigator)
        {
            // Dachora Room
            navigator.MoveToNode(3).AssertSucceeded();
            navigator.InteractWithNode().AssertSucceeded();

            // Big Pink
            navigator.MoveToNode(13).AssertSucceeded();
            navigator.MoveToNode(2).AssertSucceeded();
            navigator.InteractWithNode().AssertSucceeded();

            // Spore Spawn Kihunters
            navigator.MoveToNode(2).AssertSucceeded();
            navigator.InteractWithNode().AssertSucceeded();

            // Spore Spawn
            navigator.MoveToNode(3).AssertSucceeded();
            navigator.InteractWithNode().AssertSucceeded();
            navigator.MoveToNode(1).AssertSucceeded();
            navigator.InteractWithNode().AssertSucceeded();

            // Spore Spawn Supers
            navigator.MoveToNode(3).AssertSucceeded();
            navigator.InteractWithNode().AssertSucceeded();
            navigator.MoveToNode(2).AssertSucceeded();
            navigator.InteractWithNode().AssertSucceeded();

            // Spore Spawn Farming Room
            navigator.MoveToNode(1).AssertSucceeded();
            navigator.InteractWithNode().AssertSucceeded();

            // Big Pink
            navigator.MoveToNode(13).AssertSucceeded();
            navigator.MoveToNode(10).AssertSucceeded();
            navigator.InteractWithNode().AssertSucceeded();
            navigator.MoveToNode(13).AssertSucceeded();
            navigator.MoveToNode(11).AssertSucceeded();
            navigator.InteractWithNode().AssertSucceeded();
            navigator.MoveToNode(12).AssertSucceeded();
            navigator.InteractWithNode().AssertSucceeded();
            navigator.MoveToNode(11).AssertSucceeded();
            navigator.MoveToNode(8).AssertSucceeded();
            navigator.InteractWithNode().AssertSucceeded();
        }

        private void GreenHillToAlphaPbsToCrateria(GameNavigator navigator)
        {
            // Green Hill Zone
            navigator.MoveToNode(4).AssertSucceeded();
            navigator.InteractWithNode().AssertSucceeded();
            navigator.MoveToNode(1).AssertSucceeded();
            navigator.MoveToNode(3).AssertSucceeded();
            navigator.InteractWithNode().AssertSucceeded();

            // Noob Bridge
            navigator.MoveToNode(2).AssertSucceeded();
            navigator.InteractWithNode().AssertSucceeded();

            // Red Tower
            navigator.MoveToNode(9).AssertSucceeded();
            navigator.MoveToNode(5).AssertSucceeded();
            navigator.InteractWithNode().AssertSucceeded();

            // Hellway
            navigator.MoveToNode(2).AssertSucceeded();
            navigator.InteractWithNode().AssertSucceeded();

            // Caterpillar Room
            navigator.MoveToNode(3).AssertSucceeded();
            navigator.InteractWithNode().AssertSucceeded();

            // Alpha Pbs
            navigator.MoveToNode(2).AssertSucceeded();
            navigator.InteractWithNode().AssertSucceeded();
            navigator.MoveToNode(3).AssertSucceeded();
            navigator.InteractWithNode().AssertSucceeded();
            navigator.MoveToNode(2).AssertSucceeded();
            navigator.MoveToNode(1).AssertSucceeded();
            navigator.InteractWithNode().AssertSucceeded();

            // Caterpillar Room
            navigator.MoveToNode(2).AssertSucceeded();
            navigator.MoveToNode(4).AssertSucceeded();
            navigator.MoveToNode(1).AssertSucceeded();
            navigator.InteractWithNode().AssertSucceeded();

            // Beta Pbs
            navigator.MoveToNode(3).AssertSucceeded();
            navigator.MoveToNode(2).AssertSucceeded();
            navigator.InteractWithNode().AssertSucceeded();
            navigator.MoveToNode(3).AssertSucceeded();
            navigator.MoveToNode(1).AssertSucceeded();
            navigator.InteractWithNode().AssertSucceeded();

            // Caterpillar Room
            navigator.MoveToNode(6).AssertSucceeded();
            navigator.InteractWithNode().AssertSucceeded();

            // Red Brinstar Elevator Room
            navigator.MoveToNode(1).AssertSucceeded();
            navigator.InteractWithNode().AssertSucceeded();
        }

        private void CrateriaElevatorToShipRefill(GameNavigator navigator)
        {
            // Crateria Kihunter Room
            navigator.MoveToNode(2).AssertSucceeded();
            navigator.InteractWithNode().AssertSucceeded();

            // Moat
            navigator.MoveToNode(3).AssertSucceeded();
            navigator.InteractWithNode().AssertSucceeded();
            navigator.MoveToNode(1).AssertSucceeded();
            navigator.InteractWithNode().AssertSucceeded();

            // Crateria Kihunter Room
            navigator.MoveToNode(1).AssertSucceeded();
            navigator.InteractWithNode().AssertSucceeded();

            // Crateria Tube
            navigator.MoveToNode(1).AssertSucceeded();
            navigator.InteractWithNode().AssertSucceeded();

            // Landing Site
            navigator.MoveToNode(3).AssertSucceeded();
            navigator.InteractWithNode().AssertSucceeded();

            // Crateria Pbs
            navigator.MoveToNode(2).AssertSucceeded();
            navigator.InteractWithNode().AssertSucceeded();
            navigator.MoveToNode(1).AssertSucceeded();
            navigator.InteractWithNode().AssertSucceeded();

            // Landing Site
            navigator.MoveToNode(4).AssertSucceeded();
            navigator.MoveToNode(5).AssertSucceeded();
            navigator.InteractWithNode().AssertSucceeded();
        }

        private void GreenBrinSecondPass(GameNavigator navigator)
        {
            // Landing Site
            navigator.MoveToNode(2).AssertSucceeded();
            navigator.InteractWithNode().AssertSucceeded();

            // Parlor
            navigator.MoveToNode(8).AssertSucceeded();
            navigator.MoveToNode(1).AssertSucceeded();
            navigator.InteractWithNode().AssertSucceeded();

            // Terminator
            navigator.MoveToNode(3).AssertSucceeded();
            navigator.MoveToNode(1).AssertSucceeded();
            navigator.InteractWithNode().AssertSucceeded();

            // Green Pirates Shaft
            navigator.MoveToNode(4).AssertSucceeded();
            navigator.MoveToNode(3).AssertSucceeded();
            navigator.InteractWithNode().AssertSucceeded();

            // Lower Mushrooms
            navigator.MoveToNode(1).AssertSucceeded();
            navigator.InteractWithNode().AssertSucceeded();

            // Green Brin Elevator
            navigator.MoveToNode(2).AssertSucceeded();
            navigator.InteractWithNode().AssertSucceeded();

            // Green Brin Main Shaft
            navigator.MoveToNode(12).AssertSucceeded();
            navigator.MoveToNode(13).AssertSucceeded();
            navigator.MoveToNode(8).AssertSucceeded();
            navigator.InteractWithNode().AssertSucceeded();

            // Green Brin Beetom Room
            navigator.MoveToNode(1).AssertSucceeded();
            navigator.InteractWithNode().AssertSucceeded();

            // Etecoon E-Tank
            navigator.MoveToNode(5).AssertSucceeded();
            navigator.InteractWithNode().AssertSucceeded();
            navigator.MoveToNode(1).AssertSucceeded();
            navigator.InteractWithNode().AssertSucceeded();

            // Etecoon Supers
            navigator.MoveToNode(2).AssertSucceeded();
            navigator.InteractWithNode().AssertSucceeded();
            navigator.MoveToNode(1).AssertSucceeded();
            navigator.InteractWithNode().AssertSucceeded();

            // Etecoon E-Tank
            navigator.MoveToNode(5).AssertSucceeded();
            navigator.MoveToNode(2).AssertSucceeded();
            navigator.MoveToNode(3).AssertSucceeded();
            navigator.MoveToNode(4).AssertSucceeded();
            navigator.InteractWithNode().AssertSucceeded();

            // Etecoons
            navigator.MoveToNode(14).AssertSucceeded();
            navigator.MoveToNode(9).AssertSucceeded();
            navigator.MoveToNode(15).AssertSucceeded();
            navigator.MoveToNode(11).AssertSucceeded();
            navigator.InteractWithNode().AssertSucceeded();
            navigator.MoveToNode(15).AssertSucceeded();
            navigator.MoveToNode(9).AssertSucceeded();
            navigator.InteractWithNode().AssertSucceeded();

            // Green Brin Main Shaft
            navigator.MoveToNode(13).AssertSucceeded();
            navigator.MoveToNode(12).AssertSucceeded();
            navigator.MoveToNode(6).AssertSucceeded();
            navigator.InteractWithNode().AssertSucceeded();
        }

        private void PinkBrinSecondPass(GameNavigator navigator)
        {
            // Dachora Room
            navigator.MoveToNode(3).AssertSucceeded();
            navigator.InteractWithNode().AssertSucceeded();

            // Big Pink
            navigator.MoveToNode(13).AssertSucceeded();
            navigator.MoveToNode(10).AssertSucceeded();
            navigator.MoveToNode(4).AssertSucceeded();
            navigator.InteractWithNode().AssertSucceeded();

            // Mission Impossible
            navigator.MoveToNode(4).AssertSucceeded();
            navigator.InteractWithNode().AssertSucceeded();
            navigator.MoveToNode(3).AssertSucceeded();
            navigator.InteractWithNode().AssertSucceeded();
            navigator.MoveToNode(2).AssertSucceeded();
            navigator.InteractWithNode().AssertSucceeded();

            // Big Pink
            navigator.MoveToNode(13).AssertSucceeded();
            navigator.MoveToNode(6).AssertSucceeded();
            navigator.InteractWithNode().AssertSucceeded();

            // Pink Brin Hopper Room
            navigator.MoveToNode(1, StratFilter.BreaksObstacle("A")).AssertSucceeded();
            navigator.MoveToNode(1, StratFilter.BreaksObstacle("B")).AssertSucceeded();
            navigator.UnlockNode().AssertSucceeded();
            navigator.MoveToNode(2).AssertSucceeded();
            navigator.InteractWithNode().AssertSucceeded();

            // Hopper E-Tank
            navigator.MoveToNode(2).AssertSucceeded();
            navigator.InteractWithNode().AssertSucceeded();
            navigator.MoveToNode(1).AssertSucceeded();
            navigator.InteractWithNode().AssertSucceeded();

            // Pink Brin Hopper Room
            navigator.MoveToNode(1).AssertSucceeded();
            navigator.InteractWithNode().AssertSucceeded();

            // Big Pink
            navigator.MoveToNode(13).AssertSucceeded();
            navigator.MoveToNode(11).AssertSucceeded();
            navigator.MoveToNode(8).AssertSucceeded();
            navigator.InteractWithNode().AssertSucceeded();

            // Green Hill Zone
            navigator.MoveToNode(2).AssertSucceeded();
            navigator.InteractWithNode().AssertSucceeded();
        }

        private void BlueBrinSecondPass(GameNavigator navigator)
        {
            // Morph Ball Room
            navigator.MoveToNode(6).AssertSucceeded();
            navigator.MoveToNode(5).AssertSucceeded();
            navigator.InteractWithNode().AssertSucceeded();
            navigator.MoveToNode(4).AssertSucceeded();
            navigator.MoveToNode(2).AssertSucceeded();
            navigator.MoveToNode(3).AssertSucceeded();
            navigator.InteractWithNode().AssertSucceeded();

            // Construction Zone
            navigator.MoveToNode(4).AssertSucceeded();
            navigator.MoveToNode(2).AssertSucceeded();
            navigator.InteractWithNode().AssertSucceeded();

            // Ceiling E-Tank
            navigator.MoveToNode(4).AssertSucceeded();
            navigator.MoveToNode(2).AssertSucceeded();
            navigator.InteractWithNode().AssertSucceeded();

            // Boulder Room
            navigator.MoveToNode(1).AssertSucceeded();
            navigator.InteractWithNode().AssertSucceeded();

            // Billy Mays
            navigator.MoveToNode(2).AssertSucceeded();
            navigator.InteractWithNode().AssertSucceeded();
            navigator.MoveToNode(3).AssertSucceeded();
            navigator.InteractWithNode().AssertSucceeded();
            navigator.MoveToNode(2).AssertSucceeded();
            navigator.MoveToNode(1).AssertSucceeded();
            navigator.InteractWithNode().AssertSucceeded();

            // Boulder Room
            navigator.MoveToNode(2).AssertSucceeded();
            navigator.InteractWithNode().AssertSucceeded();

            // Ceiling E-Tank
            navigator.MoveToNode(4).AssertSucceeded();
            navigator.MoveToNode(1).AssertSucceeded();
            navigator.InteractWithNode().AssertSucceeded();

            // Construction Zone
            navigator.MoveToNode(4).AssertSucceeded();
            navigator.MoveToNode(1).AssertSucceeded();
            navigator.InteractWithNode().AssertSucceeded();

            // Morph Ball Room
            navigator.MoveToNode(2).AssertSucceeded();
            navigator.MoveToNode(4).AssertSucceeded();
            navigator.MoveToNode(5).AssertSucceeded();
            navigator.MoveToNode(6).AssertSucceeded();
            navigator.MoveToNode(1).AssertSucceeded();
            navigator.InteractWithNode().AssertSucceeded();
        }

        private void GreenHillDownRedTower(GameNavigator navigator)
        {
            // Green Hill Zone
            navigator.MoveToNode(1).AssertSucceeded();
            navigator.MoveToNode(3).AssertSucceeded();
            navigator.InteractWithNode().AssertSucceeded();

            // Noob Bridge
            navigator.MoveToNode(2).AssertSucceeded();
            navigator.InteractWithNode().AssertSucceeded();

            // Red Tower
            navigator.MoveToNode(2).AssertSucceeded();
            navigator.InteractWithNode().AssertSucceeded();

            // Red Firefleas
            navigator.MoveToNode(1).AssertSucceeded();
            navigator.InteractWithNode().AssertSucceeded();

            // X-Ray
            navigator.MoveToNode(2).AssertSucceeded();
            navigator.InteractWithNode().AssertSucceeded();
            navigator.MoveToNode(3).AssertSucceeded();
            navigator.MoveToNode(1).AssertSucceeded();
            navigator.InteractWithNode().AssertSucceeded();

            // Red Firefleas
            navigator.MoveToNode(2).AssertSucceeded();
            navigator.InteractWithNode().AssertSucceeded();

            // Red Tower
            navigator.MoveToNode(1).AssertSucceeded();
            navigator.MoveToNode(6).AssertSucceeded();
            navigator.MoveToNode(7).AssertSucceeded();
            navigator.MoveToNode(8).AssertSucceeded();
            navigator.MoveToNode(3).AssertSucceeded();
            navigator.InteractWithNode().AssertSucceeded();

            // Sloaters Refill
            navigator.MoveToNode(2).AssertSucceeded();
            navigator.InteractWithNode().AssertSucceeded();
            navigator.MoveToNode(1).AssertSucceeded();
            navigator.InteractWithNode().AssertSucceeded();

            // Red Tower
            navigator.MoveToNode(8).AssertSucceeded();
            navigator.MoveToNode(4).AssertSucceeded();
            navigator.InteractWithNode().AssertSucceeded();

            // Bat Room
            navigator.MoveToNode(2).AssertSucceeded();
            navigator.InteractWithNode().AssertSucceeded();

            // Below Spazer
            navigator.MoveToNode(3).AssertSucceeded();
            navigator.InteractWithNode().AssertSucceeded();

            // Spazer
            navigator.MoveToNode(2).AssertSucceeded();
            navigator.InteractWithNode().AssertSucceeded();
            navigator.MoveToNode(1).AssertSucceeded();
            navigator.InteractWithNode().AssertSucceeded();

            // Below Spazer
            navigator.MoveToNode(1).AssertSucceeded();
            navigator.MoveToNode(2).AssertSucceeded();
            navigator.InteractWithNode().AssertSucceeded();

            // West Tunnel
            navigator.MoveToNode(2).AssertSucceeded();
            navigator.InteractWithNode().AssertSucceeded();

            // Glass Tube
            navigator.MoveToNode(5).AssertSucceeded();
            navigator.MoveToNode(3).AssertSucceeded();
            navigator.InteractWithNode().AssertSucceeded();

            // Boyon Gate Hall
            navigator.MoveToNode(2).AssertSucceeded();
            navigator.InteractWithNode().AssertSucceeded();
        }

        private void Kraid(GameNavigator navigator)
        {
            // Warehouse Entrance
            navigator.MoveToNode(2).AssertSucceeded();
            navigator.MoveToNode(4).AssertSucceeded();
            navigator.MoveToNode(3).AssertSucceeded();
            navigator.InteractWithNode().AssertSucceeded();

            // Warehouse Zeela Room
            navigator.MoveToNode(2).AssertSucceeded();
            navigator.MoveToNode(3).AssertSucceeded();
            navigator.InteractWithNode().AssertSucceeded();

            // Warehouse Kihunter Room
            navigator.MoveToNode(4).AssertSucceeded();
            navigator.InteractWithNode().AssertSucceeded();
            navigator.MoveToNode(1).AssertSucceeded();
            navigator.MoveToNode(2).AssertSucceeded();
            navigator.InteractWithNode().AssertSucceeded();

            // Baby Kraid Room
            navigator.MoveToNode(2).AssertSucceeded();
            navigator.InteractWithNode().AssertSucceeded();

            // Kraid Eye Door Room
            navigator.MoveToNode(3).AssertSucceeded();
            navigator.InteractWithNode().AssertSucceeded();

            // Kraid's Room
            navigator.MoveToNode(3).AssertSucceeded();
            navigator.InteractWithNode().AssertSucceeded();
            navigator.MoveToNode(2).AssertSucceeded();
            navigator.InteractWithNode().AssertSucceeded();

            // Varia
            navigator.MoveToNode(2).AssertSucceeded();
            navigator.InteractWithNode().AssertSucceeded();
            navigator.MoveToNode(1).AssertSucceeded();
            navigator.InteractWithNode().AssertSucceeded();

            // Kraid's Room
            navigator.MoveToNode(3).AssertSucceeded();
            navigator.InteractWithNode().AssertSucceeded();
            navigator.MoveToNode(1).AssertSucceeded();
            navigator.InteractWithNode().AssertSucceeded();

            // Kraid Eye Door Room
            navigator.MoveToNode(1).AssertSucceeded();
            navigator.InteractWithNode().AssertSucceeded();

            // Baby Kraid Room
            navigator.MoveToNode(1).AssertSucceeded();
            navigator.InteractWithNode().AssertSucceeded();

            // Warehouse Kihunter Room
            navigator.MoveToNode(1).AssertSucceeded();
            navigator.InteractWithNode().AssertSucceeded();

            // Warehouse Zeela Room
            navigator.MoveToNode(2).AssertSucceeded();
            navigator.InteractWithNode().AssertSucceeded();

            // Warehouse E-Tank
            navigator.MoveToNode(2).AssertSucceeded();
            navigator.InteractWithNode().AssertSucceeded();
            navigator.MoveToNode(1).AssertSucceeded();
            navigator.InteractWithNode().AssertSucceeded();

            // Warehouse Zeela Room
            navigator.MoveToNode(1).AssertSucceeded();
            navigator.InteractWithNode().AssertSucceeded();

            // Warehouse Entrance
            navigator.MoveToNode(4).AssertSucceeded();
            navigator.MoveToNode(2).AssertSucceeded();
            navigator.InteractWithNode().AssertSucceeded();
        }

        private void HiJumpIce(GameNavigator navigator)
        {
            // Business Center
            navigator.MoveToNode(8).AssertSucceeded();
            navigator.MoveToNode(3).AssertSucceeded();
            navigator.InteractWithNode().AssertSucceeded();

            // HiJump E-Tank
            navigator.MoveToNode(3).AssertSucceeded();
            navigator.InteractWithNode().AssertSucceeded();
            navigator.MoveToNode(5).AssertSucceeded();
            navigator.MoveToNode(1).AssertSucceeded();
            navigator.InteractWithNode().AssertSucceeded();

            // HiJump
            navigator.MoveToNode(2).AssertSucceeded();
            navigator.InteractWithNode().AssertSucceeded();
            navigator.MoveToNode(1).AssertSucceeded();
            navigator.InteractWithNode().AssertSucceeded();

            // HiJump E-Tank
            navigator.MoveToNode(4).AssertSucceeded();
            navigator.InteractWithNode().AssertSucceeded();
            navigator.MoveToNode(3).AssertSucceeded();
            navigator.MoveToNode(2).AssertSucceeded();
            navigator.InteractWithNode().AssertSucceeded();

            // Business Center
            navigator.MoveToNode(8).AssertSucceeded();
            navigator.MoveToNode(1).AssertSucceeded();
            navigator.InteractWithNode().AssertSucceeded();

            // Ice Beam Gate Room
            navigator.MoveToNode(2).AssertSucceeded();
            navigator.MoveToNode(3).AssertSucceeded();
            navigator.InteractWithNode().AssertSucceeded();

            // Crumble Shaft
            navigator.MoveToNode(3).AssertSucceeded();
            navigator.InteractWithNode().AssertSucceeded();
            navigator.MoveToNode(1).AssertSucceeded();
            navigator.InteractWithNode().AssertSucceeded();

            // Ice Beam Gate Room
            navigator.MoveToNode(2).AssertSucceeded();
            navigator.InteractWithNode().AssertSucceeded();

            // Ice Beam Acid Room
            navigator.MoveToNode(1).AssertSucceeded();
            navigator.InteractWithNode().AssertSucceeded();

            // Ice Beam Snake Room
            navigator.MoveToNode(4).AssertSucceeded();
            navigator.MoveToNode(2).AssertSucceeded();
            navigator.InteractWithNode().AssertSucceeded();

            // Ice
            navigator.MoveToNode(2).AssertSucceeded();
            navigator.InteractWithNode().AssertSucceeded();
            navigator.MoveToNode(1).AssertSucceeded();
            navigator.InteractWithNode().AssertSucceeded();

            // Ice Beam Snake Room
            navigator.MoveToNode(4).AssertSucceeded();
            navigator.MoveToNode(5).AssertSucceeded();
            navigator.MoveToNode(1).AssertSucceeded();
            navigator.InteractWithNode().AssertSucceeded();

            // Ice Beam Tutorial Room
            navigator.MoveToNode(2).AssertSucceeded();
            navigator.InteractWithNode().AssertSucceeded();

            // Ice Beam Gate Room
            navigator.MoveToNode(5).AssertSucceeded();
            navigator.MoveToNode(4).AssertSucceeded();
            navigator.InteractWithNode().AssertSucceeded();
        }

        private void EastNorfair(GameNavigator navigator)
        {
            // Business Center
            navigator.MoveToNode(8).AssertSucceeded();
            navigator.MoveToNode(6).AssertSucceeded();
            navigator.InteractWithNode().AssertSucceeded();

            // Cathedral Entrance
            navigator.MoveToNode(3).AssertSucceeded();
            navigator.MoveToNode(5).AssertSucceeded();
            navigator.MoveToNode(2).AssertSucceeded();
            navigator.InteractWithNode().AssertSucceeded();

            // Cathedral
            navigator.MoveToNode(4).AssertSucceeded();
            navigator.MoveToNode(3).AssertSucceeded();
            navigator.InteractWithNode().AssertSucceeded();
            navigator.MoveToNode(4).AssertSucceeded();
            navigator.MoveToNode(2).AssertSucceeded();
            navigator.InteractWithNode().AssertSucceeded();

            // Rising Tide
            navigator.MoveToNode(2).AssertSucceeded();
            navigator.InteractWithNode().AssertSucceeded();

            // Bubble Mountain
            navigator.MoveToNode(9).AssertSucceeded();
            navigator.MoveToNode(8).AssertSucceeded();
            navigator.InteractWithNode().AssertSucceeded();
            navigator.MoveToNode(9).AssertSucceeded();
            navigator.MoveToNode(1).AssertSucceeded();
            navigator.InteractWithNode().AssertSucceeded();

            // Bubble Missiles
            navigator.MoveToNode(3).AssertSucceeded();
            navigator.InteractWithNode().AssertSucceeded();
            navigator.MoveToNode(1).AssertSucceeded();
            navigator.InteractWithNode().AssertSucceeded();

            // Norfair Reserve
            navigator.MoveToNode(4).AssertSucceeded();
            navigator.MoveToNode(3).AssertSucceeded();
            navigator.InteractWithNode().AssertSucceeded();
            navigator.MoveToNode(4).AssertSucceeded();
            navigator.MoveToNode(2).AssertSucceeded();
            navigator.InteractWithNode().AssertSucceeded();
            navigator.MoveToNode(4).AssertSucceeded();
            navigator.MoveToNode(1).AssertSucceeded();
            navigator.InteractWithNode().AssertSucceeded();

            // Bubble Missiles
            navigator.MoveToNode(3).AssertSucceeded();
            navigator.MoveToNode(2).AssertSucceeded();
            navigator.InteractWithNode().AssertSucceeded();

            // Bubble Mountain
            navigator.MoveToNode(7).AssertSucceeded();
            navigator.InteractWithNode().AssertSucceeded();

            // Bat Cave
            navigator.MoveToNode(2).AssertSucceeded();
            navigator.InteractWithNode().AssertSucceeded();

            // Speed Booster Hall
            navigator.MoveToNode(2).AssertSucceeded();
            navigator.MoveToNode(3).AssertSucceeded();
            navigator.InteractWithNode().AssertSucceeded();
            navigator.MoveToNode(2).AssertSucceeded();
            navigator.InteractWithNode().AssertSucceeded();

            // Speed Booster
            navigator.MoveToNode(2).AssertSucceeded();
            navigator.InteractWithNode().AssertSucceeded();
            navigator.MoveToNode(1).AssertSucceeded();
            navigator.InteractWithNode().AssertSucceeded();

            // Speed Booster Hall
            navigator.MoveToNode(1).AssertSucceeded();
            navigator.InteractWithNode().AssertSucceeded();

            // Bat Cave
            navigator.MoveToNode(1).AssertSucceeded();
            navigator.InteractWithNode().AssertSucceeded();

            // Bubble Mountain
            navigator.MoveToNode(6).AssertSucceeded();
            navigator.InteractWithNode().AssertSucceeded();

            // Single Chamber
            navigator.MoveToNode(6).AssertSucceeded();
            navigator.MoveToNode(4).AssertSucceeded();
            navigator.InteractWithNode().AssertSucceeded();

            // Double Chamber
            navigator.MoveToNode(4).AssertSucceeded();
            navigator.InteractWithNode().AssertSucceeded();
            navigator.MoveToNode(3).AssertSucceeded();
            navigator.InteractWithNode().AssertSucceeded();

            // Wave
            navigator.MoveToNode(2).AssertSucceeded();
            navigator.InteractWithNode().AssertSucceeded();
            navigator.MoveToNode(1).AssertSucceeded();
            navigator.InteractWithNode().AssertSucceeded();

            // Double Chamber
            navigator.MoveToNode(5).AssertSucceeded();
            navigator.MoveToNode(2).AssertSucceeded();
            navigator.InteractWithNode().AssertSucceeded();
        }

        private void RememberedHighwayToCroc(GameNavigator navigator)
        {
            // Single Chamber
            navigator.MoveToNode(2).AssertSucceeded();
            navigator.InteractWithNode().AssertSucceeded();

            // Spiky Platforms Tunnel
            navigator.MoveToNode(2).AssertSucceeded();
            navigator.InteractWithNode().AssertSucceeded();

            // Volcano Room
            navigator.MoveToNode(2).AssertSucceeded();
            navigator.InteractWithNode().AssertSucceeded();

            // Kronic Boost Room
            navigator.MoveToNode(5).AssertSucceeded();
            navigator.MoveToNode(6).AssertSucceeded();
            navigator.MoveToNode(2).AssertSucceeded();
            navigator.InteractWithNode().AssertSucceeded();

            // Spiky Acid Snakes Tunnel
            navigator.MoveToNode(1).AssertSucceeded();
            navigator.InteractWithNode().AssertSucceeded();

            // Nutella Refill
            navigator.MoveToNode(3).AssertSucceeded();
            navigator.InteractWithNode().AssertSucceeded();
            navigator.MoveToNode(1).AssertSucceeded();
            navigator.InteractWithNode().AssertSucceeded();

            // Acid Snakes Tunnel
            navigator.MoveToNode(3).AssertSucceeded();
            navigator.MoveToNode(1).AssertSucceeded();
            navigator.InteractWithNode().AssertSucceeded();

            // Crocomire Speedway
            navigator.MoveToNode(6).AssertSucceeded();
            navigator.MoveToNode(3).AssertSucceeded();
            navigator.InteractWithNode().AssertSucceeded();
        }

        private void CrocLoop(GameNavigator navigator)
        {
            // Crocomire's Room
            navigator.MoveToNode(4).AssertSucceeded();
            navigator.InteractWithNode().AssertSucceeded();
            navigator.MoveToNode(6).AssertSucceeded();
            navigator.MoveToNode(3).AssertSucceeded();
            navigator.InteractWithNode().AssertSucceeded();
            navigator.MoveToNode(6).AssertSucceeded();
            navigator.MoveToNode(4).AssertSucceeded();
            navigator.MoveToNode(5).AssertSucceeded();
            navigator.MoveToNode(1).AssertSucceeded();
            navigator.InteractWithNode().AssertSucceeded();

            // Post Croc Farm Room
            navigator.MoveToNode(5).AssertSucceeded();
            navigator.MoveToNode(1).AssertSucceeded();
            navigator.InteractWithNode().AssertSucceeded();

            // Croc PBs
            navigator.MoveToNode(2).AssertSucceeded();
            navigator.InteractWithNode().AssertSucceeded();
            navigator.MoveToNode(1).AssertSucceeded();
            navigator.InteractWithNode().AssertSucceeded();

            // Post Croc Farm Room
            navigator.MoveToNode(5).AssertSucceeded();
            navigator.MoveToNode(2).AssertSucceeded();
            navigator.InteractWithNode().AssertSucceeded();

            // Post Croc Shaft
            navigator.MoveToNode(3).AssertSucceeded();
            navigator.InteractWithNode().AssertSucceeded();

            // Cosine Room
            navigator.MoveToNode(2).AssertSucceeded();
            navigator.InteractWithNode().AssertSucceeded();
            navigator.MoveToNode(1).AssertSucceeded();
            navigator.InteractWithNode().AssertSucceeded();

            // Post Croc Shaft
            navigator.MoveToNode(2).AssertSucceeded();
            navigator.InteractWithNode().AssertSucceeded();

            // PCJR
            navigator.MoveToNode(4).AssertSucceeded();
            navigator.MoveToNode(3).AssertSucceeded();
            navigator.InteractWithNode().AssertSucceeded();
            navigator.MoveToNode(5).AssertSucceeded();
            navigator.MoveToNode(1).AssertSucceeded();
            navigator.InteractWithNode().AssertSucceeded();

            // Grapple
            navigator.MoveToNode(3).AssertSucceeded();
            navigator.InteractWithNode().AssertSucceeded();
            navigator.MoveToNode(2).AssertSucceeded();
            navigator.InteractWithNode().AssertSucceeded();

            // PCJR
            navigator.MoveToNode(5).AssertSucceeded();
            navigator.MoveToNode(2).AssertSucceeded();
            navigator.InteractWithNode().AssertSucceeded();

            // Post Croc Shaft
            navigator.MoveToNode(3).AssertSucceeded();
            navigator.MoveToNode(4).AssertSucceeded();
            navigator.InteractWithNode().AssertSucceeded();

            // Post Croc Farm Room
            navigator.MoveToNode(5).AssertSucceeded();
            navigator.MoveToNode(4).AssertSucceeded();
            navigator.InteractWithNode().AssertSucceeded();

            // Crocomire's Room
            navigator.MoveToNode(5).AssertSucceeded();
            navigator.MoveToNode(2).AssertSucceeded();
            navigator.InteractWithNode().AssertSucceeded();

            // Crocomire Speedway
            navigator.MoveToNode(6).AssertSucceeded();
            navigator.MoveToNode(5).AssertSucceeded();
            navigator.MoveToNode(1).AssertSucceeded();
            navigator.InteractWithNode().AssertSucceeded();

            // Croc Escape
            navigator.MoveToNode(3).AssertSucceeded();
            navigator.InteractWithNode().AssertSucceeded();
            navigator.MoveToNode(1).AssertSucceeded();
            navigator.InteractWithNode().AssertSucceeded();

            // Business Center
            navigator.MoveToNode(8).AssertSucceeded();
            navigator.MoveToNode(7).AssertSucceeded();
            navigator.InteractWithNode().AssertSucceeded();
        }

        private void EarlySpeedCleanup(GameNavigator navigator)
        {
            // Warehouse Entrance
            navigator.MoveToNode(1).AssertSucceeded();
            navigator.InteractWithNode().AssertSucceeded();

            // Boyon Gate Hall
            navigator.MoveToNode(1).AssertSucceeded();
            navigator.InteractWithNode().AssertSucceeded();

            // Glass Tube
            navigator.MoveToNode(5).AssertSucceeded();
            navigator.MoveToNode(1).AssertSucceeded();
            navigator.InteractWithNode().AssertSucceeded();

            // West Tunnel
            navigator.MoveToNode(1).AssertSucceeded();
            navigator.InteractWithNode().AssertSucceeded();

            // Below Spazer
            navigator.MoveToNode(1).AssertSucceeded();
            navigator.InteractWithNode().AssertSucceeded();

            // Bat Room
            navigator.MoveToNode(1).AssertSucceeded();
            navigator.InteractWithNode().AssertSucceeded();

            // Red Tower
            navigator.MoveToNode(8).AssertSucceeded();
            navigator.MoveToNode(7).AssertSucceeded();
            navigator.MoveToNode(6).AssertSucceeded();
            navigator.MoveToNode(1).AssertSucceeded();
            navigator.InteractWithNode().AssertSucceeded();

            // Noob Bridge
            navigator.MoveToNode(1).AssertSucceeded();
            navigator.InteractWithNode().AssertSucceeded();

            // Green Hill Zone
            navigator.MoveToNode(1).AssertSucceeded();
            navigator.InteractWithNode().AssertSucceeded();

            // Big Pink
            navigator.MoveToNode(11).AssertSucceeded();
            navigator.MoveToNode(12).AssertSucceeded();
            navigator.MoveToNode(9).AssertSucceeded();
            navigator.InteractWithNode().AssertSucceeded();

            // Waterway
            navigator.MoveToNode(2).AssertSucceeded();
            navigator.InteractWithNode().AssertSucceeded();
            navigator.MoveToNode(1).AssertSucceeded();
            navigator.InteractWithNode().AssertSucceeded();

            // Big Pink
            navigator.MoveToNode(12).AssertSucceeded();
            navigator.MoveToNode(11).AssertSucceeded();
            navigator.MoveToNode(8).AssertSucceeded();
            navigator.InteractWithNode().AssertSucceeded();

            // Green Hill Zone
            navigator.MoveToNode(2).AssertSucceeded();
            navigator.InteractWithNode().AssertSucceeded();

            // Morph Ball Room
            navigator.MoveToNode(6).AssertSucceeded();
            navigator.MoveToNode(5).AssertSucceeded();
            navigator.MoveToNode(4).AssertSucceeded();
            navigator.MoveToNode(2).AssertSucceeded();
            navigator.InteractWithNode().AssertSucceeded();

            // Blue Brin Elevator Room
            navigator.MoveToNode(1).AssertSucceeded();
            navigator.InteractWithNode().AssertSucceeded();

            // Pit Room
            navigator.MoveToNode(1).AssertSucceeded();
            navigator.InteractWithNode().AssertSucceeded();

            // Climb
            navigator.MoveToNode(6).AssertSucceeded();
            navigator.MoveToNode(4).AssertSucceeded();
            navigator.InteractWithNode().AssertSucceeded();

            // Crateria Supers
            navigator.MoveToNode(3).AssertSucceeded();
            navigator.InteractWithNode().AssertSucceeded();
            navigator.MoveToNode(4).AssertSucceeded();
            navigator.MoveToNode(1).AssertSucceeded();
            navigator.InteractWithNode().AssertSucceeded();

            // Climb
            navigator.MoveToNode(6).AssertSucceeded();
            navigator.MoveToNode(1).AssertSucceeded();
            navigator.InteractWithNode().AssertSucceeded();

            // Parlor
            navigator.MoveToNode(8).AssertSucceeded();
            navigator.MoveToNode(4).AssertSucceeded();
            navigator.InteractWithNode().AssertSucceeded();

            // Landing Site
            navigator.MoveToNode(5).AssertSucceeded();
            navigator.InteractWithNode().AssertSucceeded();
        }

        private void WreckedShipAttic(GameNavigator navigator)
        {
            // Landing Site
            navigator.MoveToNode(4).AssertSucceeded();
            navigator.InteractWithNode().AssertSucceeded();

            // Crateria Tube
            navigator.MoveToNode(2).AssertSucceeded();
            navigator.InteractWithNode().AssertSucceeded();

            // Crateria Kihunter Room
            navigator.MoveToNode(2).AssertSucceeded();
            navigator.InteractWithNode().AssertSucceeded();

            // Moat
            navigator.MoveToNode(3).AssertSucceeded();
            navigator.MoveToNode(2).AssertSucceeded();
            navigator.InteractWithNode().AssertSucceeded();

            // West Ocean
            navigator.MoveToNode(13).AssertSucceeded();
            navigator.MoveToNode(11).AssertSucceeded();
            navigator.InteractWithNode().AssertSucceeded();
            navigator.MoveToNode(13).AssertSucceeded();
            navigator.MoveToNode(5).AssertSucceeded();
            navigator.InteractWithNode().AssertSucceeded();

            // Wrecked Ship Entrance
            navigator.MoveToNode(2).AssertSucceeded();
            navigator.InteractWithNode().AssertSucceeded();

            // Wrecked Ship Main Shaft
            navigator.MoveToNode(9).AssertSucceeded();
            navigator.MoveToNode(8).AssertSucceeded();
            navigator.InteractWithNode().AssertSucceeded();
            navigator.MoveToNode(9).AssertSucceeded();
            navigator.MoveToNode(7).AssertSucceeded();
            navigator.InteractWithNode().AssertSucceeded();

            // Basement
            navigator.MoveToNode(3).AssertSucceeded();
            navigator.InteractWithNode().AssertSucceeded();

            // Phantoon
            navigator.MoveToNode(2).AssertSucceeded();
            navigator.InteractWithNode().AssertSucceeded();
            navigator.MoveToNode(1).AssertSucceeded();
            navigator.InteractWithNode().AssertSucceeded();

            // Basement
            navigator.MoveToNode(2).AssertSucceeded();
            navigator.InteractWithNode().AssertSucceeded();

            // Wrecked Ship Main Shaft
            navigator.MoveToNode(9).AssertSucceeded();
            navigator.MoveToNode(6).AssertSucceeded();
            navigator.InteractWithNode().AssertSucceeded();

            // WS East Supers
            navigator.MoveToNode(2).AssertSucceeded();
            navigator.InteractWithNode().AssertSucceeded();
            navigator.MoveToNode(1).AssertSucceeded();
            navigator.InteractWithNode().AssertSucceeded();

            // Wrecked Ship Main Shaft
            navigator.MoveToNode(9).AssertSucceeded();
            navigator.MoveToNode(3).AssertSucceeded();
            navigator.InteractWithNode().AssertSucceeded();

            // WS West Supers
            navigator.MoveToNode(2).AssertSucceeded();
            navigator.InteractWithNode().AssertSucceeded();
            navigator.MoveToNode(1).AssertSucceeded();
            navigator.InteractWithNode().AssertSucceeded();

            // Wrecked Ship Main Shaft
            navigator.MoveToNode(9).AssertSucceeded();
            navigator.MoveToNode(1).AssertSucceeded();
            navigator.InteractWithNode().AssertSucceeded();

            // Attic
            navigator.MoveToNode(3).AssertSucceeded();
            navigator.InteractWithNode().AssertSucceeded();

            // WS East Missile
            navigator.MoveToNode(2).AssertSucceeded();
            navigator.InteractWithNode().AssertSucceeded();
            navigator.MoveToNode(1).AssertSucceeded();
            navigator.InteractWithNode().AssertSucceeded();

            // Attic
            navigator.MoveToNode(2).AssertSucceeded();
            navigator.MoveToNode(1).AssertSucceeded();
            navigator.InteractWithNode().AssertSucceeded();
        }

        private void WreckedShipCleanup(GameNavigator navigator)
        {
            // West Ocean
            navigator.MoveToNode(12).AssertSucceeded();
            navigator.MoveToNode(9).AssertSucceeded();
            navigator.InteractWithNode().AssertSucceeded();
            navigator.MoveToNode(12).AssertSucceeded();
            navigator.MoveToNode(14).AssertSucceeded();
            navigator.MoveToNode(10).AssertSucceeded();
            navigator.InteractWithNode().AssertSucceeded();
            navigator.MoveToNode(14).AssertSucceeded();
            navigator.MoveToNode(12).AssertSucceeded();
            navigator.MoveToNode(6).AssertSucceeded();
            navigator.InteractWithNode().AssertSucceeded();

            // Bowling Alley Path
            navigator.MoveToNode(2).AssertSucceeded();
            navigator.InteractWithNode().AssertSucceeded();

            // Homing Geemer Room
            navigator.MoveToNode(2).AssertSucceeded();
            navigator.InteractWithNode().AssertSucceeded();

            // Bowling Alley
            navigator.MoveToNode(6).AssertSucceeded();
            navigator.MoveToNode(3).AssertSucceeded();
            navigator.MoveToNode(4).AssertSucceeded();
            navigator.InteractWithNode().AssertSucceeded();
            navigator.MoveToNode(5).AssertSucceeded();
            navigator.InteractWithNode().AssertSucceeded();
            navigator.MoveToNode(4).AssertSucceeded();
            navigator.MoveToNode(3).AssertSucceeded();
            navigator.InteractWithNode().AssertSucceeded();

            // Gravity Suit Room
            navigator.MoveToNode(3).AssertSucceeded();
            navigator.InteractWithNode().AssertSucceeded();
            navigator.MoveToNode(1).AssertSucceeded();
            navigator.InteractWithNode().AssertSucceeded();

            // West Ocean
            navigator.MoveToNode(5).AssertSucceeded();
            navigator.InteractWithNode().AssertSucceeded();

            // Wrecked Ship Main Entrance
            navigator.MoveToNode(2).AssertSucceeded();
            navigator.InteractWithNode().AssertSucceeded();

            // Wrecked Ship Main Shaft
            navigator.MoveToNode(9).AssertSucceeded();
            navigator.MoveToNode(5).AssertSucceeded();
            navigator.InteractWithNode().AssertSucceeded();

            // Sponge Bath
            navigator.MoveToNode(2).AssertSucceeded();
            navigator.InteractWithNode().AssertSucceeded();

            // Spiky Death Room
            navigator.MoveToNode(2).AssertSucceeded();
            navigator.InteractWithNode().AssertSucceeded();

            // Electric Death Room
            navigator.MoveToNode(2).AssertSucceeded();
            navigator.MoveToNode(1).AssertSucceeded();
            navigator.InteractWithNode().AssertSucceeded();

            // Wrecked Ship Energy Tank Room
            navigator.MoveToNode(2).AssertSucceeded();
            navigator.InteractWithNode().AssertSucceeded();
            navigator.MoveToNode(1).AssertSucceeded();
            navigator.InteractWithNode().AssertSucceeded();

            // Electric Death Room
            navigator.MoveToNode(2).AssertSucceeded();
            navigator.InteractWithNode().AssertSucceeded();
        }

        private void ForgottenHighwayToMaridiaTube(GameNavigator navigator)
        {
            // East Ocean
            navigator.MoveToNode(3).AssertSucceeded();
            navigator.MoveToNode(4).AssertSucceeded();
            navigator.MoveToNode(2).AssertSucceeded();
            navigator.InteractWithNode().AssertSucceeded();

            // Forgotten Highway Kago Room
            navigator.MoveToNode(2).AssertSucceeded();
            navigator.InteractWithNode().AssertSucceeded();

            // Crab Maze
            navigator.MoveToNode(3).AssertSucceeded();
            navigator.MoveToNode(2).AssertSucceeded();
            navigator.InteractWithNode().AssertSucceeded();

            // Forgotten Highway Elbow
            navigator.MoveToNode(2).AssertSucceeded();
            navigator.InteractWithNode().AssertSucceeded();

            // Forgotten Highway Elevator
            navigator.MoveToNode(2).AssertSucceeded();
            navigator.InteractWithNode().AssertSucceeded();

            // Maridia Elevator Room
            navigator.MoveToNode(2).AssertSucceeded();
            navigator.MoveToNode(1).AssertSucceeded();
            navigator.InteractWithNode().AssertSucceeded();

            // Thread the Needle Room
            navigator.MoveToNode(1).AssertSucceeded();
            navigator.InteractWithNode().AssertSucceeded();

            // Bug Sand Hole
            navigator.MoveToNode(1).AssertSucceeded();
            navigator.InteractWithNode().AssertSucceeded();

            // Plasma Spark Room
            navigator.MoveToNode(2).AssertSucceeded();
            navigator.MoveToNode(1).AssertSucceeded();
            navigator.InteractWithNode().AssertSucceeded();

            // Toilet Bowl
            navigator.InteractWithNode().AssertSucceeded();

            // Oasis
            navigator.MoveToNode(6).AssertSucceeded();
            navigator.MoveToNode(5).AssertSucceeded();
            navigator.MoveToNode(2).AssertSucceeded();
            navigator.InteractWithNode().AssertSucceeded();

            // East Sand Hall
            navigator.MoveToNode(3).AssertSucceeded();
            navigator.MoveToNode(2).AssertSucceeded();
            navigator.InteractWithNode().AssertSucceeded();

            // Pants Room
            navigator.MoveToNode(4).AssertSucceeded();
            navigator.MoveToNode(5).AssertSucceeded();
            navigator.MoveToNode(2).AssertSucceeded();
            navigator.InteractWithNode().AssertSucceeded();

            // East Pants Room
            navigator.MoveToNode(2).AssertSucceeded();
            navigator.InteractWithNode().AssertSucceeded();

            // Shaktool Room
            navigator.MoveToNode(3).AssertSucceeded();
            navigator.InteractWithNode().AssertSucceeded();
            navigator.MoveToNode(2).AssertSucceeded();
            navigator.InteractWithNode().AssertSucceeded();

            // Spring Ball Room
            navigator.MoveToNode(2).AssertSucceeded();
            navigator.InteractWithNode().AssertSucceeded();
            navigator.MoveToNode(1).AssertSucceeded();
            navigator.InteractWithNode().AssertSucceeded();

            // Shaktool Room
            navigator.MoveToNode(3).AssertSucceeded();
            navigator.InteractWithNode().AssertSucceeded();
            navigator.MoveToNode(1).AssertSucceeded();
            navigator.InteractWithNode().AssertSucceeded();

            // East Pants Room
            navigator.MoveToNode(4).AssertSucceeded();
            navigator.MoveToNode(3).AssertSucceeded();
            navigator.InteractWithNode().AssertSucceeded();

            // Pants Room
            navigator.MoveToNode(4).AssertSucceeded();
            navigator.MoveToNode(1).AssertSucceeded();
            navigator.InteractWithNode().AssertSucceeded();

            // East Sand Hall
            navigator.MoveToNode(3).AssertSucceeded();
            navigator.MoveToNode(1).AssertSucceeded();
            navigator.InteractWithNode().AssertSucceeded();

            // Oasis
            navigator.MoveToNode(5).AssertSucceeded();
            navigator.MoveToNode(1).AssertSucceeded();
            navigator.InteractWithNode().AssertSucceeded();

            // West Sand Hall
            navigator.MoveToNode(3).AssertSucceeded();
            navigator.MoveToNode(1).AssertSucceeded();
            navigator.InteractWithNode().AssertSucceeded();

            // West Sand Hall Tunnel
            navigator.MoveToNode(1).AssertSucceeded();
            navigator.InteractWithNode().AssertSucceeded();

            // Crab Hole
            navigator.MoveToNode(1).AssertSucceeded();
            navigator.MoveToNode(2).AssertSucceeded();
            navigator.InteractWithNode().AssertSucceeded();

            // Boyon Gate Hall
            navigator.MoveToNode(4).AssertSucceeded();
            navigator.MoveToNode(1).AssertSucceeded();
            navigator.InteractWithNode().AssertSucceeded();
        }

        private void OuterMaridiaToAqueduct(GameNavigator navigator)
        {
            // Glass Tunnel
            navigator.MoveToNode(5).AssertSucceeded();
            navigator.InteractWithNode().AssertSucceeded();
            navigator.MoveToNode(6).AssertSucceeded();
            navigator.MoveToNode(8).AssertSucceeded();
            navigator.MoveToNode(4).AssertSucceeded();
            navigator.InteractWithNode().AssertSucceeded();

            // Main Street
            navigator.MoveToNode(8).AssertSucceeded();
            navigator.MoveToNode(6).AssertSucceeded();
            navigator.InteractWithNode().AssertSucceeded();
            navigator.MoveToNode(9).AssertSucceeded();
            navigator.MoveToNode(3).AssertSucceeded();
            navigator.InteractWithNode().AssertSucceeded();

            // Fish Tank
            navigator.MoveToNode(5).AssertSucceeded();
            navigator.MoveToNode(6).AssertSucceeded();
            navigator.MoveToNode(2).AssertSucceeded();
            navigator.InteractWithNode().AssertSucceeded();

            // Mama Turtle Room
            navigator.MoveToNode(2).AssertSucceeded();
            navigator.InteractWithNode().AssertSucceeded();
            navigator.MoveToNode(1).AssertSucceeded();
            navigator.MoveToNode(3).AssertSucceeded();
            navigator.InteractWithNode().AssertSucceeded();
            navigator.MoveToNode(1).AssertSucceeded();
            navigator.InteractWithNode().AssertSucceeded();

            // Fish Tank
            navigator.MoveToNode(6).AssertSucceeded();
            navigator.MoveToNode(5).AssertSucceeded();
            navigator.MoveToNode(4).AssertSucceeded();
            navigator.InteractWithNode().AssertSucceeded();

            // Mt. Everest
            navigator.MoveToNode(7).AssertSucceeded();
            navigator.MoveToNode(8).AssertSucceeded();
            navigator.MoveToNode(6).AssertSucceeded();
            navigator.InteractWithNode().AssertSucceeded();

            // Main Street (morph passage)
            navigator.MoveToNode(7).AssertSucceeded();
            navigator.InteractWithNode().AssertSucceeded();
            navigator.MoveToNode(5).AssertSucceeded();
            navigator.InteractWithNode().AssertSucceeded();

            // Mt. Everest
            navigator.MoveToNode(2).AssertSucceeded();
            navigator.MoveToNode(4).AssertSucceeded();
            navigator.InteractWithNode().AssertSucceeded();

            // Crab Shaft
            navigator.MoveToNode(3).AssertSucceeded();
            navigator.InteractWithNode().AssertSucceeded();

            // Pseudo Plasma Spark Room
            navigator.MoveToNode(3).AssertSucceeded();
            navigator.InteractWithNode().AssertSucceeded();
            navigator.MoveToNode(2).AssertSucceeded();
            navigator.MoveToNode(1).AssertSucceeded();
            navigator.InteractWithNode().AssertSucceeded();

            // Northwest Maridia Bug Room
            navigator.MoveToNode(1).AssertSucceeded();
            navigator.InteractWithNode().AssertSucceeded();

            // Watering Hole
            navigator.MoveToNode(4).AssertSucceeded();
            navigator.MoveToNode(3).AssertSucceeded();
            navigator.InteractWithNode().AssertSucceeded();
            navigator.MoveToNode(4).AssertSucceeded();
            navigator.MoveToNode(2).AssertSucceeded();
            navigator.InteractWithNode().AssertSucceeded();
            navigator.MoveToNode(4).AssertSucceeded();
            navigator.MoveToNode(1).AssertSucceeded();
            navigator.InteractWithNode().AssertSucceeded();

            // Northwest Maridia Bug Room
            navigator.MoveToNode(2).AssertSucceeded();
            navigator.InteractWithNode().AssertSucceeded();

            // Pseudo Plasma Spark Room
            navigator.MoveToNode(2).AssertSucceeded();
            navigator.InteractWithNode().AssertSucceeded();

            // Crab Shaft
            navigator.MoveToNode(1).AssertSucceeded();
            navigator.MoveToNode(2).AssertSucceeded();
            navigator.InteractWithNode().AssertSucceeded();
        }

        private void AqueductAndSandHoleLoops(GameNavigator navigator)
        {
            // Aqueduct
            navigator.MoveToNode(2).AssertSucceeded();
            navigator.MoveToNode(7).AssertSucceeded();
            navigator.InteractWithNode().AssertSucceeded();
            navigator.MoveToNode(8).AssertSucceeded();
            navigator.InteractWithNode().AssertSucceeded();
            navigator.MoveToNode(5).AssertSucceeded();
            navigator.MoveToNode(2).AssertSucceeded();
            navigator.MoveToNode(4).AssertSucceeded();
            navigator.MoveToNode(3).AssertSucceeded();
            navigator.InteractWithNode().AssertSucceeded();

            // West Aqueduct Quicksand Room
            navigator.MoveToNode(2).AssertSucceeded();
            navigator.InteractWithNode().AssertSucceeded();

            // West Sand Hole
            navigator.MoveToNode(7).AssertSucceeded();
            navigator.MoveToNode(5).AssertSucceeded();
            navigator.MoveToNode(6).AssertSucceeded();
            navigator.MoveToNode(4).AssertSucceeded();
            navigator.InteractWithNode().AssertSucceeded();
            navigator.MoveToNode(6).AssertSucceeded();
            navigator.MoveToNode(3).AssertSucceeded();
            navigator.InteractWithNode().AssertSucceeded();
            navigator.MoveToNode(6).AssertSucceeded();
            navigator.MoveToNode(5).AssertSucceeded();
            navigator.MoveToNode(7).AssertSucceeded();
            navigator.MoveToNode(1).AssertSucceeded();
            navigator.MoveToNode(2).AssertSucceeded();
            navigator.InteractWithNode().AssertSucceeded();

            // West Sand Hall
            navigator.MoveToNode(1).AssertSucceeded();
            navigator.InteractWithNode().AssertSucceeded();

            // West Sand Hall Tunnel
            WestSandHallTunnelToAqueduct(navigator);

            // Aqueduct
            navigator.MoveToNode(2).AssertSucceeded();
            navigator.MoveToNode(4).AssertSucceeded();
            navigator.InteractWithNode().AssertSucceeded();

            // East Aqueduct Quicksand Room
            navigator.MoveToNode(2).AssertSucceeded();
            navigator.InteractWithNode().AssertSucceeded();

            // East Sand Hole
            navigator.MoveToNode(3).AssertSucceeded();
            navigator.InteractWithNode().AssertSucceeded();
            navigator.MoveToNode(1).AssertSucceeded();
            navigator.MoveToNode(4).AssertSucceeded();
            navigator.InteractWithNode().AssertSucceeded();
            navigator.MoveToNode(1).AssertSucceeded();
            navigator.MoveToNode(2).AssertSucceeded();
            navigator.InteractWithNode().AssertSucceeded();

            //  East Sand Hall
            navigator.MoveToNode(1).AssertSucceeded();
            navigator.InteractWithNode().AssertSucceeded();

            // Oasis
            navigator.MoveToNode(5).AssertSucceeded();
            navigator.MoveToNode(1).AssertSucceeded();
            navigator.InteractWithNode().AssertSucceeded();

            // West Sand Hall
            navigator.MoveToNode(3).AssertSucceeded();
            navigator.MoveToNode(1).AssertSucceeded();
            navigator.InteractWithNode().AssertSucceeded();

            // West Sand Hall Tunnel
            WestSandHallTunnelToAqueduct(navigator);

            // Aqueduct
            navigator.MoveToNode(2).AssertSucceeded();
            navigator.MoveToNode(9).AssertSucceeded();
            navigator.MoveToNode(6).AssertSucceeded();
            navigator.InteractWithNode().AssertSucceeded();
        }

        /// <summary>
        /// Navigation only. In a helper method because it's done twice in the Sand Hole Loops
        /// </summary>
        private void WestSandHallTunnelToAqueduct(GameNavigator navigator)
        {
            // West Sand Hall Tunnel
            navigator.MoveToNode(1).AssertSucceeded();
            navigator.InteractWithNode().AssertSucceeded();

            // Crab Hole
            navigator.MoveToNode(1).AssertSucceeded();
            navigator.InteractWithNode().AssertSucceeded();

            // Crab Tunnel
            navigator.MoveToNode(1).AssertSucceeded();
            navigator.InteractWithNode().AssertSucceeded();

            // Main Street
            navigator.MoveToNode(8).AssertSucceeded();
            navigator.MoveToNode(3).AssertSucceeded();
            navigator.InteractWithNode().AssertSucceeded();

            // Fish Tank
            navigator.MoveToNode(5).AssertSucceeded();
            navigator.MoveToNode(4).AssertSucceeded();
            navigator.InteractWithNode().AssertSucceeded();

            // Mt. Everest
            navigator.MoveToNode(7).AssertSucceeded();
            navigator.MoveToNode(10).AssertSucceeded();
            navigator.MoveToNode(4).AssertSucceeded();
            navigator.InteractWithNode().AssertSucceeded();

            // Crab Shaft
            navigator.MoveToNode(2).AssertSucceeded();
            navigator.InteractWithNode().AssertSucceeded();
        }

        private void BotwoonAndDraygon(GameNavigator navigator)
        {
            // Botwoon Hallway
            navigator.MoveToNode(2).AssertSucceeded();
            navigator.InteractWithNode().AssertSucceeded();

            // Botwoon's Room
            navigator.MoveToNode(3).AssertSucceeded();
            navigator.InteractWithNode().AssertSucceeded();
            navigator.MoveToNode(5).AssertSucceeded();
            navigator.InteractWithNode().AssertSucceeded();
            navigator.MoveToNode(4).AssertSucceeded();
            navigator.MoveToNode(2).AssertSucceeded();
            navigator.InteractWithNode().AssertSucceeded();

            // Botwoon Energy Tank Room
            navigator.MoveToNode(5).AssertSucceeded();
            navigator.InteractWithNode().AssertSucceeded();
            navigator.MoveToNode(6).AssertSucceeded();
            navigator.MoveToNode(4).AssertSucceeded();
            navigator.InteractWithNode().AssertSucceeded();

            // Halfie Climb Room
            navigator.MoveToNode(1).AssertSucceeded();
            navigator.MoveToNode(4).AssertSucceeded();
            navigator.InteractWithNode().AssertSucceeded();

            // Colosseum
            navigator.MoveToNode(3).AssertSucceeded();
            navigator.MoveToNode(2).AssertSucceeded();
            navigator.InteractWithNode().AssertSucceeded();

            // Precious Room
            navigator.MoveToNode(3).AssertSucceeded();
            navigator.InteractWithNode().AssertSucceeded();
            navigator.MoveToNode(1).AssertSucceeded();
            navigator.MoveToNode(2).AssertSucceeded();
            navigator.InteractWithNode().AssertSucceeded();

            // Draygon's Room
            navigator.MoveToNode(3).AssertSucceeded();
            navigator.InteractWithNode().AssertSucceeded();
            navigator.MoveToNode(1).AssertSucceeded();
            navigator.InteractWithNode().AssertSucceeded();

            // Space Jump Room
            navigator.MoveToNode(2).AssertSucceeded();
            navigator.InteractWithNode().AssertSucceeded();
            navigator.MoveToNode(1).AssertSucceeded();
            navigator.InteractWithNode().AssertSucceeded();

            // Draygon's Room
            navigator.MoveToNode(3).AssertSucceeded();
            navigator.MoveToNode(2).AssertSucceeded();
            navigator.InteractWithNode().AssertSucceeded();

            // Precious Room
            navigator.MoveToNode(1).AssertSucceeded();
            navigator.InteractWithNode().AssertSucceeded();

            // Colosseum
            navigator.MoveToNode(3).AssertSucceeded();
            navigator.MoveToNode(1).AssertSucceeded();
            navigator.InteractWithNode().AssertSucceeded();

            // Halfie Climb Room
            navigator.MoveToNode(1).AssertSucceeded();
            navigator.InteractWithNode().AssertSucceeded();

            // East Cactus Alley Room
            navigator.MoveToNode(3).AssertSucceeded();
            navigator.MoveToNode(1).AssertSucceeded();
            navigator.InteractWithNode().AssertSucceeded();

            // West Cactus Alley Room
            navigator.MoveToNode(1).AssertSucceeded();
            navigator.InteractWithNode().AssertSucceeded();

            // Butterfly Room
            navigator.MoveToNode(3).AssertSucceeded();
            navigator.MoveToNode(1).AssertSucceeded();
            navigator.InteractWithNode().AssertSucceeded();

            // Plasma Spark Room
            navigator.MoveToNode(3).AssertSucceeded();
            navigator.MoveToNode(4).AssertSucceeded();
            navigator.InteractWithNode().AssertSucceeded();

            // Plasma Climb
            navigator.MoveToNode(2).AssertSucceeded();
            navigator.InteractWithNode().AssertSucceeded();

            // Plasma Tutorial Room
            navigator.MoveToNode(2).AssertSucceeded();
            navigator.InteractWithNode().AssertSucceeded();

            // Plasma Room
            navigator.MoveToNode(2).AssertSucceeded();
            navigator.InteractWithNode().AssertSucceeded();
            navigator.MoveToNode(3).AssertSucceeded();
            navigator.MoveToNode(1).AssertSucceeded();
            navigator.InteractWithNode().AssertSucceeded();

            // Plasma Tutorial Room
            navigator.MoveToNode(1).AssertSucceeded();
            navigator.InteractWithNode().AssertSucceeded();

            // Plasma Climb
            navigator.MoveToNode(1).AssertSucceeded();
            navigator.InteractWithNode().AssertSucceeded();

            // Plasma Spark Room
            navigator.MoveToNode(5).AssertSucceeded();
            navigator.MoveToNode(1).AssertSucceeded();
            navigator.InteractWithNode().AssertSucceeded();

            // Toilet
            navigator.InteractWithNode().AssertSucceeded();

            // Oasis
            navigator.MoveToNode(6).AssertSucceeded();
            navigator.MoveToNode(5).AssertSucceeded();
            navigator.MoveToNode(1).AssertSucceeded();
            navigator.InteractWithNode().AssertSucceeded();

            // West Sand Hall
            navigator.MoveToNode(3).AssertSucceeded();
            navigator.MoveToNode(1).AssertSucceeded();
            navigator.InteractWithNode().AssertSucceeded();

            // West Sand Hall Tunnel
            navigator.MoveToNode(1).AssertSucceeded();
            navigator.InteractWithNode().AssertSucceeded();

            // Crab Hole
            navigator.MoveToNode(1).AssertSucceeded();
            navigator.MoveToNode(2).AssertSucceeded();
            navigator.InteractWithNode().AssertSucceeded();

            // Boyon Gate Hall
            navigator.MoveToNode(4).AssertSucceeded();
            navigator.MoveToNode(1).AssertSucceeded();
            navigator.MoveToNode(2).AssertSucceeded();
            navigator.InteractWithNode().AssertSucceeded();

            // Warehouse Entrance
            navigator.MoveToNode(2).AssertSucceeded();
            navigator.InteractWithNode().AssertSucceeded();
        }

        private void BusinessCenterToScrew(GameNavigator navigator)
        {
            // Business Center
            navigator.MoveToNode(8).AssertSucceeded();
            navigator.MoveToNode(6).AssertSucceeded();
            navigator.InteractWithNode().AssertSucceeded();

            // Cathedral Entrance
            navigator.MoveToNode(3).AssertSucceeded();
            navigator.MoveToNode(5).AssertSucceeded();
            navigator.MoveToNode(2).AssertSucceeded();
            navigator.InteractWithNode().AssertSucceeded();

            // Cathedral
            navigator.MoveToNode(4).AssertSucceeded();
            navigator.MoveToNode(2).AssertSucceeded();
            navigator.InteractWithNode().AssertSucceeded();

            // Rising Tide
            navigator.MoveToNode(2).AssertSucceeded();
            navigator.InteractWithNode().AssertSucceeded();

            // Bubble Mountain
            navigator.MoveToNode(9).AssertSucceeded();
            navigator.MoveToNode(5).AssertSucceeded();
            navigator.InteractWithNode().AssertSucceeded();

            // Purple Shaft
            navigator.MoveToNode(2).AssertSucceeded();
            navigator.MoveToNode(3).AssertSucceeded();
            navigator.InteractWithNode().AssertSucceeded();

            // Magdollite Tunnel
            navigator.MoveToNode(2).AssertSucceeded();
            navigator.InteractWithNode().AssertSucceeded();

            // Kronic Boost Room
            navigator.MoveToNode(5).AssertSucceeded();
            navigator.MoveToNode(6).AssertSucceeded();
            navigator.MoveToNode(3).AssertSucceeded();
            navigator.InteractWithNode().AssertSucceeded();

            // Lava Dive Room
            navigator.MoveToNode(1).AssertSucceeded();
            navigator.InteractWithNode().AssertSucceeded();

            // Lower Norfair Elevator
            navigator.MoveToNode(3).AssertSucceeded();
            navigator.MoveToNode(4).AssertSucceeded();
            navigator.InteractWithNode().AssertSucceeded();

            // Main Hall
            navigator.MoveToNode(3).AssertSucceeded();
            navigator.MoveToNode(1).AssertSucceeded();
            navigator.InteractWithNode().AssertSucceeded();

            // Acid Statue Room
            navigator.MoveToNode(3).AssertSucceeded();
            navigator.InteractWithNode().AssertSucceeded();
            navigator.MoveToNode(4).AssertSucceeded();
            navigator.MoveToNode(5).AssertSucceeded();
            navigator.MoveToNode(2).AssertSucceeded();
            navigator.InteractWithNode().AssertSucceeded();

            // Golden Torizo Room
            navigator.MoveToNode(3).AssertSucceeded();
            navigator.InteractWithNode().AssertSucceeded();
            navigator.MoveToNode(5).AssertSucceeded();
            navigator.InteractWithNode().AssertSucceeded();
            navigator.MoveToNode(4).AssertSucceeded();
            navigator.InteractWithNode().AssertSucceeded();
            navigator.MoveToNode(5).AssertSucceeded();
            navigator.MoveToNode(2).AssertSucceeded();
            navigator.InteractWithNode().AssertSucceeded();

            // Screw Attack Room
            navigator.MoveToNode(4).AssertSucceeded();
            navigator.InteractWithNode().AssertSucceeded();
            navigator.MoveToNode(2).AssertSucceeded();
            navigator.MoveToNode(5).AssertSucceeded();
            navigator.MoveToNode(3).AssertSucceeded();
            navigator.InteractWithNode().AssertSucceeded();
        }

        private void ScrewToRidley(GameNavigator navigator)
        {
            // Fast Ripper Room
            navigator.MoveToNode(3).AssertSucceeded();
            navigator.MoveToNode(2).AssertSucceeded();
            navigator.InteractWithNode().AssertSucceeded();

            // Pillars Setup Room
            navigator.MoveToNode(5).AssertSucceeded();
            navigator.MoveToNode(3).AssertSucceeded();
            navigator.InteractWithNode().AssertSucceeded();

            // Pillar Room
            navigator.MoveToNode(2).AssertSucceeded();
            navigator.InteractWithNode().AssertSucceeded();

            // Worst Room in the Game
            navigator.MoveToNode(4).AssertSucceeded();
            navigator.MoveToNode(5).AssertSucceeded();
            navigator.MoveToNode(1).AssertSucceeded();
            navigator.InteractWithNode().AssertSucceeded();

            // Mickey Mouse Room
            navigator.MoveToNode(4).AssertSucceeded();
            navigator.MoveToNode(3).AssertSucceeded();
            navigator.InteractWithNode().AssertSucceeded();
            navigator.MoveToNode(4).AssertSucceeded();
            navigator.MoveToNode(2).AssertSucceeded();
            navigator.InteractWithNode().AssertSucceeded();

            // Worst Room in the Game
            navigator.MoveToNode(3).AssertSucceeded();
            navigator.InteractWithNode().AssertSucceeded();

            // Amphitheatre
            navigator.MoveToNode(2).AssertSucceeded();
            navigator.InteractWithNode().AssertSucceeded();

            // Red Kihunter Shaft
            navigator.MoveToNode(5).AssertSucceeded();
            navigator.MoveToNode(3).AssertSucceeded();
            navigator.MoveToNode(6).AssertSucceeded();
            navigator.MoveToNode(2).AssertSucceeded();
            navigator.InteractWithNode().AssertSucceeded();

            // Wasteland
            navigator.MoveToNode(7).AssertSucceeded();
            navigator.MoveToNode(4).AssertSucceeded();
            navigator.MoveToNode(6).AssertSucceeded();
            navigator.MoveToNode(3).AssertSucceeded();
            navigator.InteractWithNode().AssertSucceeded();
            navigator.MoveToNode(6).AssertSucceeded();
            navigator.MoveToNode(4).AssertSucceeded();
            navigator.MoveToNode(5).AssertSucceeded();
            navigator.MoveToNode(1).AssertSucceeded();
            navigator.InteractWithNode().AssertSucceeded();

            // Metal Pirates Room
            navigator.MoveToNode(3).AssertSucceeded();
            navigator.MoveToNode(3, StratFilter.BreaksObstacle("A")).AssertSucceeded();
            navigator.MoveToNode(1).AssertSucceeded();
            navigator.InteractWithNode().AssertSucceeded();

            // Plowerhouse Room
            navigator.MoveToNode(1).AssertSucceeded();
            navigator.InteractWithNode().AssertSucceeded();

            // Lower Norfair Farming Room
            navigator.MoveToNode(1).AssertSucceeded();
            navigator.InteractWithNode().AssertSucceeded();

            // Ridley's Room
            navigator.MoveToNode(3).AssertSucceeded();
            navigator.InteractWithNode().AssertSucceeded();
            navigator.MoveToNode(1).AssertSucceeded();
            navigator.InteractWithNode().AssertSucceeded();

            // Ridley Tank Room
            navigator.MoveToNode(2).AssertSucceeded();
            navigator.InteractWithNode().AssertSucceeded();
            navigator.MoveToNode(1).AssertSucceeded();
            navigator.InteractWithNode().AssertSucceeded();

            // Ridley's Room
            navigator.MoveToNode(3).AssertSucceeded();
            navigator.MoveToNode(2).AssertSucceeded();
            navigator.InteractWithNode().AssertSucceeded();
        }


        private void NorfairEscape(GameNavigator navigator)
        {
            // Lower Norfair Farming Room
            navigator.MoveToNode(2).AssertSucceeded();
            navigator.InteractWithNode().AssertSucceeded();

            // Plowerhouse Room
            navigator.MoveToNode(2).AssertSucceeded();
            navigator.InteractWithNode().AssertSucceeded();

            // Metal Pirates Room
            navigator.MoveToNode(2).AssertSucceeded();
            navigator.InteractWithNode().AssertSucceeded();

            // Wasteland
            navigator.MoveToNode(5).AssertSucceeded();
            navigator.MoveToNode(4).AssertSucceeded();
            navigator.MoveToNode(7).AssertSucceeded();
            navigator.MoveToNode(2).AssertSucceeded();
            navigator.InteractWithNode().AssertSucceeded();

            // Red Kihunter Shaft
            navigator.MoveToNode(6).AssertSucceeded();
            navigator.MoveToNode(3).AssertSucceeded();
            navigator.MoveToNode(5).AssertSucceeded();
            navigator.MoveToNode(4).AssertSucceeded();
            navigator.InteractWithNode().AssertSucceeded();

            // Lower Norfair Fireflea Room
            navigator.MoveToNode(6).AssertSucceeded();
            navigator.MoveToNode(7).AssertSucceeded();
            navigator.MoveToNode(5).AssertSucceeded();
            navigator.MoveToNode(4).AssertSucceeded();
            navigator.InteractWithNode().AssertSucceeded();
            navigator.MoveToNode(5).AssertSucceeded();
            navigator.MoveToNode(7).AssertSucceeded();
            navigator.MoveToNode(6).AssertSucceeded();
            navigator.MoveToNode(2).AssertSucceeded();
            navigator.MoveToNode(3).AssertSucceeded();
            navigator.MoveToNode(1).AssertSucceeded();
            navigator.InteractWithNode().AssertSucceeded();

            // Lower Norfair Spring Ball Maze Room
            navigator.MoveToNode(1).AssertSucceeded();
            navigator.MoveToNode(4).AssertSucceeded();
            navigator.InteractWithNode().AssertSucceeded();
            navigator.MoveToNode(5).AssertSucceeded();
            navigator.MoveToNode(2).AssertSucceeded();
            navigator.InteractWithNode().AssertSucceeded();

            // Norfair Escape Power Bomb Room
            navigator.MoveToNode(3).AssertSucceeded();
            navigator.InteractWithNode().AssertSucceeded();
            navigator.MoveToNode(2).AssertSucceeded();
            navigator.MoveToNode(1).AssertSucceeded();
            navigator.InteractWithNode().AssertSucceeded();

            // Lower Norfair Fireflea Room
            navigator.MoveToNode(1).AssertSucceeded();
            navigator.InteractWithNode().AssertSucceeded();

            // Lower Norfair Spring Ball Maze Room
            navigator.MoveToNode(1).AssertSucceeded();
            navigator.InteractWithNode().AssertSucceeded();

            // Three Musketeers' Room
            navigator.MoveToNode(4).AssertSucceeded();
            navigator.MoveToNode(3).AssertSucceeded();
            navigator.InteractWithNode().AssertSucceeded();
            navigator.MoveToNode(4).AssertSucceeded();
            navigator.MoveToNode(1).AssertSucceeded();
            navigator.InteractWithNode().AssertSucceeded();

            // Single Chamber
            navigator.MoveToNode(6).AssertSucceeded();
            navigator.MoveToNode(1).AssertSucceeded();
            navigator.InteractWithNode().AssertSucceeded();

            // Bubble Mountain
            navigator.MoveToNode(7).AssertSucceeded();
            navigator.MoveToNode(9).AssertSucceeded();
            navigator.MoveToNode(3).AssertSucceeded();
            navigator.InteractWithNode().AssertSucceeded();

            // Rising Tide
            navigator.MoveToNode(1).AssertSucceeded();
            navigator.InteractWithNode().AssertSucceeded();

            // Cathedral
            navigator.MoveToNode(4).AssertSucceeded();
            navigator.MoveToNode(1).AssertSucceeded();
            navigator.InteractWithNode().AssertSucceeded();

            // Cathedral Entrance
            navigator.MoveToNode(5).AssertSucceeded();
            navigator.MoveToNode(3).AssertSucceeded();
            navigator.MoveToNode(1).AssertSucceeded();
            navigator.InteractWithNode().AssertSucceeded();

            // Business Center
            navigator.MoveToNode(8).AssertSucceeded();
            navigator.MoveToNode(7).AssertSucceeded();
            navigator.InteractWithNode().AssertSucceeded();
        }

        private void ToG4(GameNavigator navigator)
        {
            // Warehouse Entrance
            navigator.MoveToNode(1).AssertSucceeded();
            navigator.InteractWithNode().AssertSucceeded();

            // Boyon Gate Hall
            navigator.MoveToNode(1).AssertSucceeded();
            navigator.InteractWithNode().AssertSucceeded();

            // Glass Tunnel
            navigator.MoveToNode(5).AssertSucceeded();
            navigator.MoveToNode(6).AssertSucceeded();
            navigator.MoveToNode(8).AssertSucceeded();
            navigator.MoveToNode(4).AssertSucceeded();
            navigator.InteractWithNode().AssertSucceeded();

            // Main Street
            navigator.MoveToNode(8).AssertSucceeded();
            navigator.MoveToNode(3).AssertSucceeded();
            navigator.InteractWithNode().AssertSucceeded();

            // Fish Tank
            navigator.MoveToNode(5).AssertSucceeded();
            navigator.MoveToNode(4).AssertSucceeded();
            navigator.InteractWithNode().AssertSucceeded();

            // Mt. Everest
            navigator.MoveToNode(7).AssertSucceeded();
            navigator.MoveToNode(8).AssertSucceeded();
            navigator.MoveToNode(11).AssertSucceeded();
            navigator.MoveToNode(9).AssertSucceeded();
            navigator.MoveToNode(5).AssertSucceeded();
            navigator.InteractWithNode().AssertSucceeded();

            // Red Fish Room
            navigator.MoveToNode(1).AssertSucceeded();
            navigator.InteractWithNode().AssertSucceeded();

            // Caterpillar Room
            navigator.MoveToNode(1).AssertSucceeded();
            navigator.MoveToNode(6).AssertSucceeded();
            navigator.InteractWithNode().AssertSucceeded();

            // Red Brinstar Elevator Room
            navigator.MoveToNode(1).AssertSucceeded();
            navigator.InteractWithNode().AssertSucceeded();

            // Crateria Kihunter Room
            navigator.MoveToNode(2).AssertSucceeded();
            navigator.MoveToNode(1).AssertSucceeded();
            navigator.InteractWithNode().AssertSucceeded();

            // Crateria Tube
            navigator.MoveToNode(1).AssertSucceeded();
            navigator.InteractWithNode().AssertSucceeded();

            // Landing Site
            navigator.MoveToNode(5).AssertSucceeded();
            navigator.MoveToNode(2).AssertSucceeded();
            navigator.InteractWithNode().AssertSucceeded();

            // Parlor
            navigator.MoveToNode(8).AssertSucceeded();
            navigator.MoveToNode(1).AssertSucceeded();
            navigator.InteractWithNode().AssertSucceeded();

            // Terminator Room
            navigator.MoveToNode(3).AssertSucceeded();
            navigator.MoveToNode(1).AssertSucceeded();
            navigator.InteractWithNode().AssertSucceeded();

            // Green Pirates Shaft
            navigator.MoveToNode(4).AssertSucceeded();
            navigator.InteractWithNode().AssertSucceeded();

            // Statues Hallway
            navigator.MoveToNode(2).AssertSucceeded();
            navigator.InteractWithNode().AssertSucceeded();
        }

        private void TourianToMotherBrain(GameNavigator navigator)
        {
            // Statues' Room
            navigator.MoveToNode(3).AssertSucceeded();
            navigator.InteractWithNode().AssertSucceeded();
            navigator.MoveToNode(4).AssertSucceeded();
            navigator.MoveToNode(2).AssertSucceeded();
            navigator.InteractWithNode().AssertSucceeded();

            // Tourian First Room
            navigator.MoveToNode(3).AssertSucceeded();
            navigator.MoveToNode(2).AssertSucceeded();
            navigator.InteractWithNode().AssertSucceeded();

            // Metroid Room 1
            navigator.MoveToNode(4, StratFilter.BreaksObstacle("A")).AssertSucceeded();
            navigator.MoveToNode(1).AssertSucceeded();
            navigator.InteractWithNode().AssertSucceeded();

            // Metroid Room 2
            navigator.MoveToNode(2, StratFilter.BreaksObstacle("A")).AssertSucceeded();
            navigator.InteractWithNode().AssertSucceeded();

            // Metroid Room 3
            navigator.MoveToNode(2, StratFilter.BreaksObstacle("A")).AssertSucceeded();
            navigator.InteractWithNode().AssertSucceeded();

            // Metroid Room 4
            navigator.MoveToNode(2, StratFilter.BreaksObstacle("A")).AssertSucceeded();
            navigator.InteractWithNode().AssertSucceeded();

            // Blue Hopper Room
            navigator.MoveToNode(2).AssertSucceeded();
            navigator.InteractWithNode().AssertSucceeded();

            // Dust Torizo Room
            navigator.MoveToNode(1).AssertSucceeded();
            navigator.InteractWithNode().AssertSucceeded();

            // Big Boy Room
            navigator.MoveToNode(1).AssertSucceeded();
            navigator.InteractWithNode().AssertSucceeded();

            // Seaweed Room
            navigator.MoveToNode(4).AssertSucceeded();
            navigator.MoveToNode(3).AssertSucceeded();
            navigator.InteractWithNode().AssertSucceeded();

            // Tourian Eye Door Room
            navigator.MoveToNode(2).AssertSucceeded();
            navigator.InteractWithNode().AssertSucceeded();

            // Rinka Shaft
            navigator.MoveToNode(2).AssertSucceeded();
            navigator.MoveToNode(3).AssertSucceeded();
            navigator.InteractWithNode().AssertSucceeded();

            // Mother Brain Room
            navigator.MoveToNode(8).AssertSucceeded();
            navigator.MoveToNode(3).AssertSucceeded();
            navigator.MoveToNode(4).AssertSucceeded();
            navigator.InteractWithNode().AssertSucceeded();
            navigator.MoveToNode(1).AssertSucceeded();
            navigator.InteractWithNode().AssertSucceeded();
        }

        private void TourianEscapeToShip(GameNavigator navigator)
        {
            // Tourian Escape Room 1
            navigator.MoveToNode(2).AssertSucceeded();
            navigator.InteractWithNode().AssertSucceeded();

            // Tourian Escape Room 2
            navigator.MoveToNode(2).AssertSucceeded();
            navigator.InteractWithNode().AssertSucceeded();

            // Tourian Escape Room 3
            navigator.MoveToNode(2).AssertSucceeded();
            navigator.InteractWithNode().AssertSucceeded();

            // Tourian Escape Room 4
            navigator.MoveToNode(2).AssertSucceeded();
            navigator.InteractWithNode().AssertSucceeded();

            // Climb
            navigator.MoveToNode(6).AssertSucceeded();
            navigator.MoveToNode(1).AssertSucceeded();
            navigator.InteractWithNode().AssertSucceeded();

            // Parlor
            navigator.MoveToNode(8).AssertSucceeded();
            navigator.MoveToNode(5).AssertSucceeded();
            navigator.InteractWithNode().AssertSucceeded();

            // Flyway
            navigator.MoveToNode(2).AssertSucceeded();
            navigator.InteractWithNode().AssertSucceeded();

            // Bomb Torizo Room
            navigator.InteractWithNode().AssertFailed();
            navigator.MoveToNode(4).AssertSucceeded();
            navigator.InteractWithNode().AssertSucceeded();
            navigator.MoveToNode(1).AssertSucceeded();
            navigator.InteractWithNode().AssertSucceeded();

            // Flyway
            navigator.MoveToNode(1).AssertSucceeded();
            navigator.InteractWithNode().AssertSucceeded();

            // Alcatraz
            navigator.MoveToNode(8).AssertSucceeded();
            navigator.MoveToNode(4).AssertSucceeded();
            navigator.InteractWithNode().AssertSucceeded();

            // Landing Site
            navigator.MoveToNode(5).AssertSucceeded();
            navigator.MoveToNode(6).AssertSucceeded();
            navigator.InteractWithNode().AssertSucceeded();
        }
    }
}
