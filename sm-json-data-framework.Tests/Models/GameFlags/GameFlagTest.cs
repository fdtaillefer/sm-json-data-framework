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
        private static SuperMetroidModel ReusableModel() => StaticTestObjects.UnmodifiableModel;
        private static SuperMetroidModel NewModelForOptions() => StaticTestObjects.UnfinalizedModel.Finalize();

        #region Tests for construction from unfinalized model

        [Fact]
        public void CtorFromUnfinalized_SetsPropertiesCorrectly()
        {
            // Given/when standard model creation
            SuperMetroidModel model = ReusableModel();

            // Expect
            GameFlag gameFlag = model.GameFlags["f_ZebesAwake"];

            Assert.Equal("f_ZebesAwake", gameFlag.Name);
        }

        #endregion

        #region Tests for ApplyLogicalOptions() that check applied logical properties

        [Fact]
        public void ApplyLogicalOptions_SetsLogicalProperties()
        {
            // Given
            SuperMetroidModel model = NewModelForOptions();
            LogicalOptions logicalOptions = new LogicalOptions();
            logicalOptions.RegisterDisabledGameFlag("f_AnimalsSaved");
            logicalOptions.InternalStartConditions = StartConditions.CreateVanillaStartConditionsBuilder(model)
                .StartingGameFlags(new List<string> { "f_DefeatedCeresRidley" })
                .Build();

            // When
            model.ApplyLogicalOptions(logicalOptions);

            // Expect
            GameFlag alwaysFlag = model.GameFlags["f_DefeatedCeresRidley"];
            Assert.True(alwaysFlag.LogicallyRelevant);
            Assert.False(alwaysFlag.LogicallyNever);
            Assert.True(alwaysFlag.LogicallyAlways);
            Assert.True(alwaysFlag.LogicallyFree);

            GameFlag removedFlag = model.GameFlags["f_AnimalsSaved"];
            Assert.False(removedFlag.LogicallyRelevant);
            Assert.True(removedFlag.LogicallyNever);
            Assert.False(removedFlag.LogicallyAlways);
            Assert.False(removedFlag.LogicallyFree);

            GameFlag obtainableFlag = model.GameFlags["f_ZebesAwake"];
            Assert.True(obtainableFlag.LogicallyRelevant);
            Assert.False(obtainableFlag.LogicallyNever);
            Assert.False(obtainableFlag.LogicallyAlways);
            Assert.False(obtainableFlag.LogicallyFree);
        }

        #endregion
    }
}
