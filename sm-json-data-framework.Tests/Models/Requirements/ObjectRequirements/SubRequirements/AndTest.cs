using sm_json_data_framework.Models;
using sm_json_data_framework.Options;
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

namespace sm_json_data_framework.Tests.Models.Requirements.ObjectRequirements.SubRequirements
{
    public class AndTest
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
            And and = model.Helpers["h_canBombThings"].Requires.LogicalElement<Or>(0).LogicalRequirements.LogicalElement<And>(0);
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
            SuperMetroidModel model = ReusableModel();
            InGameState inGameState = model.CreateInitialGameState()
                .ApplyEnterRoom("Big Pink", 13);
            And and = model.Rooms["Big Pink"].Links[13].To[10].Strats["Base"].Requires.LogicalElement<Or>(0).LogicalRequirements.LogicalElement<And>(0);

            //  When
            ExecutionResult result = and.Execute(model, inGameState);

            // Expect
            Assert.Null(result);
        }

        [Fact]
        public void Execute_SomeElementsMetButNotAll_Fails()
        {
            // Given
            SuperMetroidModel model = ReusableModel();
            InGameState inGameState = model.CreateInitialGameState()
                .ApplyEnterRoom("Green Hill Zone", 1);
            And and = model.Rooms["Green Hill Zone"].Links[1].To[2].Strats["Base"].Requires.LogicalElement<Or>(0).LogicalRequirements.LogicalElement<And>(0);

            //  When
            ExecutionResult result = and.Execute(model, inGameState);

            // Expect
            Assert.Null(result);
        }

        [Fact]
        public void Execute_AllElementMet_Succeeds()
        {
            // Given
            SuperMetroidModel model = ReusableModel();
            InGameState inGameState = model.CreateInitialGameState()
                .ApplyAddItem(SuperMetroidModel.SPEED_BOOSTER_NAME)
                .ApplyAddItem("HiJump")
                .ApplyEnterRoom("Big Pink", 13);
            And and = model.Rooms["Big Pink"].Links[13].To[10].Strats["Base"].Requires.LogicalElement<Or>(0).LogicalRequirements.LogicalElement<And>(0);

            //  When
            ExecutionResult result = and.Execute(model, inGameState);

            // Expect
            new ExecutionResultValidator(model, inGameState)
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
            SuperMetroidModel model = NewModelForOptions();
            LogicalOptions logicalOptions = new LogicalOptions()
                .RegisterRemovedItem("Spazer")
                .RegisterRemovedItem("HiJump")
                .RegisterDisabledTech("canSuitlessMaridia");
            logicalOptions.InternalStartConditions = StartConditions.CreateVanillaStartConditionsBuilder(model).StartingInventory(
                ItemInventory.CreateVanillaStartingInventory(model)
                    .ApplyAddItem(model.Items["Charge"])
                    .ApplyAddItem(model.Items["SpeedBooster"])
                )
                .Build();

            // When
            model.ApplyLogicalOptions(logicalOptions);

            // Expect
            And oneFreeOneNeverSomePossible = model.Rooms["Metal Pirates Room"].Obstacles["A"].Requires.LogicalElement<Or>(1)
                .LogicalRequirements.LogicalElement<And>(1);
            Assert.Equal(5, oneFreeOneNeverSomePossible.LogicalRequirements.LogicalElements.Count()); // Sanity check to make sure we have the right And
            Assert.True(oneFreeOneNeverSomePossible.LogicallyRelevant);
            Assert.True(oneFreeOneNeverSomePossible.LogicallyNever);
            Assert.False(oneFreeOneNeverSomePossible.LogicallyAlways);
            Assert.False(oneFreeOneNeverSomePossible.LogicallyFree);

            And allFree = model.Rooms["Green Hill Zone"].Links[1].To[2].Strats["Base"].Requires.LogicalElement<Or>(0).LogicalRequirements.LogicalElement<And>(0);
            Assert.True(allFree.LogicallyRelevant);
            Assert.False(allFree.LogicallyNever);
            Assert.True(allFree.LogicallyAlways);
            Assert.True(allFree.LogicallyFree);

            And allNever = model.Rooms["West Sand Hole"].Links[7].To[5].Strats["Left Sand Pit Initial MidAir Morph"].Requires.LogicalElement<Or>(0).LogicalRequirements.LogicalElement<And>(0);
            Assert.True(allNever.LogicallyRelevant);
            Assert.True(allNever.LogicallyNever);
            Assert.False(allNever.LogicallyAlways);
            Assert.False(allNever.LogicallyFree);

            And allPossible = model.Helpers["h_canBombThings"].Requires.LogicalElement<Or>(0).LogicalRequirements.LogicalElement<And>(0);
            Assert.True(allPossible.LogicallyRelevant);
            Assert.False(allPossible.LogicallyNever);
            Assert.False(allPossible.LogicallyAlways);
            Assert.False(allPossible.LogicallyFree);
        }

        #endregion
    }
}
