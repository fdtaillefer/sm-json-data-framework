﻿using sm_json_data_framework.Models.InGameStates;
using sm_json_data_framework.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace sm_json_data_framework.Models.Requirements.ObjectRequirements.SubRequirements
{
    public class Or : AbstractObjectLogicalElementWithSubRequirements<UnfinalizedOr, Or>
    {
        public Or(UnfinalizedOr innerElement, Action<Or> mappingsInsertionCallback, ModelFinalizationMappings mappings)
            : base(innerElement, mappingsInsertionCallback, mappings)
        {

        }

        protected override ExecutionResult ExecuteUseful(SuperMetroidModel model, ReadOnlyInGameState inGameState, int times = 1, int previousRoomCount = 0)
        {
            return LogicalRequirements.ExecuteOne(model, inGameState, times: times, previousRoomCount: previousRoomCount);
        }

        protected override void PropagateLogicalOptions(ReadOnlyLogicalOptions logicalOptions)
        {
            LogicalRequirements.ApplyLogicalOptions(logicalOptions);
        }

        protected override bool CalculateLogicallyNever()
        {
            // Delegate to requirements (interpreted as an Or)
            return LogicalRequirements.LogicallyOrNever;
        }

        protected override bool CalculateLogicallyAlways()
        {
            // Delegate to requirements (interpreted as an Or)
            return LogicalRequirements.LogicallyOrAlways;
        }

        protected override bool CalculateLogicallyFree()
        {
            // Delegate to requirements (interpreted as an Or)
            return LogicalRequirements.LogicallyOrFree;
        }
    }


    /// <summary>
    /// A logical element that is fulfilled by fulfilling any one of its inner logical elements.
    /// </summary>
    public class UnfinalizedOr : AbstractUnfinalizedObjectLogicalElementWithSubRequirements<UnfinalizedOr, Or>
    {
        public UnfinalizedOr()
        {

        }

        protected override Or CreateFinalizedElement(UnfinalizedOr sourceElement, Action<Or> mappingsInsertionCallback, ModelFinalizationMappings mappings)
        {
            return new Or(sourceElement, mappingsInsertionCallback, mappings);
        }

        public UnfinalizedOr(UnfinalizedLogicalRequirements logicalRequirements) : base(logicalRequirements)
        {

        }
    }
}
