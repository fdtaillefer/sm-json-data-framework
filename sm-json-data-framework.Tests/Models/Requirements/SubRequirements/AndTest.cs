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
using sm_json_data_framework.Models.Requirements.ObjectRequirements.SubRequirements;
using sm_json_data_framework.Rules.InitialState;
using sm_json_data_framework.Models.Requirements.StringRequirements;
using sm_json_data_framework.Models.Requirements.ObjectRequirements.SubObjects;
using sm_json_data_framework.Models.Requirements;

namespace sm_json_data_framework.Tests.Models.Requirements.SubRequirements
{
    public class AndTest
    {
        private static SuperMetroidModel Model = StaticTestObjects.UnmodifiableModel;
        private static SuperMetroidModel ModelWithOptions = StaticTestObjects.UnfinalizedModel.Finalize();

        #region Tests for construction from unfinalized model

        [Fact]
        public void CtorFromUnfinalized_SetsPropertiesCorrectly()
        {
            // Given/when standard model creation

            // Expect
            And and = Model.Helpers["h_canBombThings"].Requires.LogicalElement<Or>(0).LogicalRequirements.LogicalElement<And>(0);
            Assert.NotNull(and.LogicalRequirements);
            Assert.Equal(2, and.LogicalRequirements.LogicalElements.Count);
            Assert.NotNull(and.LogicalRequirements.LogicalElement<ItemLogicalElement>(0));
            Assert.NotNull(and.LogicalRequirements.LogicalElement<Ammo>(0));
        }

        #endregion

        #region Tests for Execute()

        [Fact]
        public void Execute_NoElementMet_Fails()
        {
            // Given
            InGameState inGameState = Model.CreateInitialGameState()
                .ApplyEnterRoom(Model.Rooms["Big Pink"].Nodes[13]);
            And and = Model.Rooms["Big Pink"].Links[13].To[10].Strats["Base"].Requires.LogicalElement<Or>(0).LogicalRequirements.LogicalElement<And>(0);

            //  When
            ExecutionResult result = and.Execute(Model, inGameState);

            // Expect
            Assert.Null(result);
        }

        [Fact]
        public void Execute_SomeElementsMetButNotAll_Fails()
        {
            // Given
            InGameState inGameState = Model.CreateInitialGameState()
                .ApplyEnterRoom(Model.Rooms["Green Hill Zone"].Nodes[1]);
            And and = Model.Rooms["Green Hill Zone"].Links[1].To[2].Strats["Base"].Requires.LogicalElement<Or>(0).LogicalRequirements.LogicalElement<And>(0);

            //  When
            ExecutionResult result = and.Execute(Model, inGameState);

            // Expect
            Assert.Null(result);
        }

        [Fact]
        public void Execute_AllElementMet_Succeeds()
        {
            // Given
            InGameState inGameState = Model.CreateInitialGameState()
                .ApplyAddItem(Model.Items[SuperMetroidModel.SPEED_BOOSTER_NAME])
                .ApplyAddItem(Model.Items["HiJump"])
                .ApplyEnterRoom(Model.Rooms["Big Pink"].Nodes[13]);
            And and = Model.Rooms["Big Pink"].Links[13].To[10].Strats["Base"].Requires.LogicalElement<Or>(0).LogicalRequirements.LogicalElement<And>(0);

            //  When
            ExecutionResult result = and.Execute(Model, inGameState);

            // Expect
            new ExecutionResultValidator(Model, inGameState)
                .ExpectItemInvolved(SuperMetroidModel.SPEED_BOOSTER_NAME)
                .ExpectItemInvolved("HiJump")
                .AssertRespectedBy(result);
        }

        #endregion

        #region Tests for ApplyLogicalOptions() that check applied logical properties

        [Fact]
        public void ApplyLogicalOptions_SetsLogicalProperties()
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

        #endregion
    }
}
