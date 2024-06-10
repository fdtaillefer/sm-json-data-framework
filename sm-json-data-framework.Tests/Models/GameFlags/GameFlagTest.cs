using sm_json_data_framework.Models;
using sm_json_data_framework.Options;
using sm_json_data_framework.Tests.TestTools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using sm_json_data_framework.Models.GameFlags;
using sm_json_data_framework.Rules.InitialState;

namespace sm_json_data_framework.Tests.Models.GameFlags
{
    public class GameFlagTest
    {
        private static SuperMetroidModel Model = StaticTestObjects.UnmodifiableModel;
        private static SuperMetroidModel ModelWithOptions = StaticTestObjects.UnfinalizedModel.Finalize();

        #region Tests for construction from unfinalized model

        [Fact]
        public void CtorFromUnfinalized_SetsPropertiesCorrectly()
        {
            // Given/when standard model creation

            // Expect
            GameFlag gameFlag = Model.GameFlags["f_ZebesAwake"];

            Assert.Equal("f_ZebesAwake", gameFlag.Name);
        }

        #endregion

        #region Tests for ApplyLogicalOptions() that check applied logical properties

        [Fact]
        public void ApplyLogicalOptions_SetsLogicalProperties()
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
            GameFlag alwaysFlag = ModelWithOptions.GameFlags["f_DefeatedCeresRidley"];
            Assert.True(alwaysFlag.LogicallyRelevant);
            Assert.False(alwaysFlag.LogicallyNever);
            Assert.True(alwaysFlag.LogicallyAlways);
            Assert.True(alwaysFlag.LogicallyFree);

            GameFlag removedFlag = ModelWithOptions.GameFlags["f_AnimalsSaved"];
            Assert.False(removedFlag.LogicallyRelevant);
            Assert.True(removedFlag.LogicallyNever);
            Assert.False(removedFlag.LogicallyAlways);
            Assert.False(removedFlag.LogicallyFree);

            GameFlag obtainableFlag = ModelWithOptions.GameFlags["f_ZebesAwake"];
            Assert.True(obtainableFlag.LogicallyRelevant);
            Assert.False(obtainableFlag.LogicallyNever);
            Assert.False(obtainableFlag.LogicallyAlways);
            Assert.False(obtainableFlag.LogicallyFree);
        }

        #endregion
    }
}
