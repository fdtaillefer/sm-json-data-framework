﻿using sm_json_data_framework.Models;
using sm_json_data_framework.Options;
using sm_json_data_framework.Tests.TestTools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using sm_json_data_framework.Models.Rooms;

namespace sm_json_data_framework.Tests.Models.Rooms
{
    public class StratFailureTest
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
            StratFailure stratFailure = model.Rooms["Early Supers Room"].Links[1].To[2].Strats["Early Supers Mockball"].Failures["Crumble Fall"];
            Assert.Equal("Crumble Fall", stratFailure.Name);
            Assert.Same(model.Rooms["Early Supers Room"].Nodes[3], stratFailure.LeadsToNode);
            Assert.False(stratFailure.Softlock);
            Assert.True(stratFailure.ClearsPreviousNode);
            Assert.NotNull(stratFailure.Cost);
            Assert.Empty(stratFailure.Cost.LogicalElements);

            StratFailure stratFailureWithNoLeadsTo = model.Rooms["Early Supers Room"].Links[3].To[1].Strats["Early Supers Quick Crumble Escape (Dual Quick Crumble)"]
                .Failures["Crumble Failure"];
            Assert.Null(stratFailureWithNoLeadsTo.LeadsToNode);

            // There would be more things to test, but no strat failures are in the model with other values
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
            StratFailure stratFailure = model.Rooms["Early Supers Room"].Links[3].To[1].Strats["Early Supers Quick Crumble Escape (Space)"].Failures["Crumble Failure"];
            Assert.True(stratFailure.LogicallyRelevant);
        }

        #endregion
    }
}
