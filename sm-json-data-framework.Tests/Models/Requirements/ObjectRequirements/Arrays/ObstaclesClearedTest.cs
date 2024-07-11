using sm_json_data_framework.Models.Requirements.ObjectRequirements.SubObjects;
using sm_json_data_framework.Models;
using sm_json_data_framework.Tests.TestTools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using sm_json_data_framework.Models.Requirements.ObjectRequirements.Arrays;
using sm_json_data_framework.Models.Items;
using sm_json_data_framework.Models.Requirements;
using sm_json_data_framework.Options;
using sm_json_data_framework.InGameStates;

namespace sm_json_data_framework.Tests.Models.Requirements.ObjectRequirements.Arrays
{
    public class ObstaclesClearedTest
    {
        private static SuperMetroidModel ReusableModel() => StaticTestObjects.UnmodifiableModel;
        private static SuperMetroidModel NewModelForOptions() => StaticTestObjects.UnfinalizedModel.Finalize();

        #region Tests for construction from unfinalized model

        [Fact]
        public void CtorFromUnfinalized_SetsPropertiesCorrectly()
        {
            // Given/when standard model creation
            SuperMetroidModel model = ReusableModel();

            // Expect
            ObstaclesCleared obstaclesCleared = model.RoomEnemies["Etecoon E-Tank Left Zebbo"].FarmCycles["Crouch two tiles above spawn point"].Requires.LogicalElement<ObstaclesCleared>(0);
            Assert.Equal(1, obstaclesCleared.Obstacles.Count);
            Assert.Same(model.Rooms["Etecoon Energy Tank Room"].Obstacles["A"], obstaclesCleared.Obstacles["A"]);
        }

        #endregion

        #region Tests for Execute()

        [Fact]
        public void Execute_ObstacleNotDestroyed_Fails()
        {
            // Given
            SuperMetroidModel model = ReusableModel();
            ObstaclesCleared obstaclesCleared = model.RoomEnemies["Etecoon E-Tank Left Zebbo"].FarmCycles["Crouch two tiles above spawn point"].Requires.LogicalElement<ObstaclesCleared>(0);
            InGameState inGameState = model.CreateInitialGameState()
                .ApplyEnterRoom(model.Rooms["Etecoon Energy Tank Room"].Nodes[7]);

            // When
            ExecutionResult result = obstaclesCleared.Execute(model, inGameState);

            // Expect
            Assert.Null(result);
        }

        [Fact]
        public void Execute_ObstaclesCleared_Succeeds()
        {
            // Given
            SuperMetroidModel model = ReusableModel();
            ObstaclesCleared obstaclesCleared = model.RoomEnemies["Etecoon E-Tank Left Zebbo"].FarmCycles["Crouch two tiles above spawn point"].Requires.LogicalElement<ObstaclesCleared>(0);
            InGameState inGameState = model.CreateInitialGameState()
                .ApplyEnterRoom(model.Rooms["Etecoon Energy Tank Room"].Nodes[7])
                .ApplyDestroyObstacle("A");

            // When
            ExecutionResult result = obstaclesCleared.Execute(model, inGameState);

            // Expect
            new ExecutionResultValidator(model, inGameState)
                .AssertRespectedBy(result);
        }

        [Fact]
        public void Execute_ObstaclesDestroyedInWrongRoom_Fails()
        {
            // Given
            SuperMetroidModel model = ReusableModel();
            ObstaclesCleared obstaclesCleared = model.RoomEnemies["Etecoon E-Tank Left Zebbo"].FarmCycles["Crouch two tiles above spawn point"].Requires.LogicalElement<ObstaclesCleared>(0);
            InGameState inGameState = model.CreateInitialGameState()
                .ApplyEnterRoom(model.Rooms["Brinstar Pre-Map Room"].Nodes[1])
                .ApplyDestroyObstacle("A");

            // When
            ExecutionResult result = obstaclesCleared.Execute(model, inGameState);

            // Expect
            Assert.Null(result);
        }

        #endregion

        #region Tests for ApplyLogicalOptions() that check applied logical properties

        [Fact]
        public void ApplyLogicalOptions_SetsLogicalProperties()
        {
            // Given
            SuperMetroidModel model = NewModelForOptions();
            LogicalOptions logicalOptions = new LogicalOptions();

            // When
            model.ApplyLogicalOptions(logicalOptions);

            // Expect
            ObstaclesCleared obstaclesCleared = model.RoomEnemies["Etecoon E-Tank Left Zebbo"].FarmCycles["Crouch two tiles above spawn point"].Requires.LogicalElement<ObstaclesCleared>(0);
            Assert.True(obstaclesCleared.LogicallyRelevant);
            Assert.False(obstaclesCleared.LogicallyNever);
            Assert.False(obstaclesCleared.LogicallyAlways);
            Assert.False(obstaclesCleared.LogicallyFree);
        }

        // Could write a test for LogicallyNever being true, but no ObstaclesCleared currently need an obstacle that  has base common requirements

        #endregion
    }
}
