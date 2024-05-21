﻿using sm_json_data_framework.Models.Raw.Rooms.Nodes;
using sm_json_data_framework.Options;
using System;
using System.Collections.Generic;
using System.Text;

namespace sm_json_data_framework.Models.Rooms.Nodes
{
    /// <summary>
    /// Contains the address of a door that is an in-game twin of another door. Both doors lead to the same destination.
    /// This is mainly a way to represent Pants Room vs. East Pants Room.
    /// </summary>
    public class TwinDoorAddress : AbstractModelElement<UnfinalizedTwinDoorAddress, TwinDoorAddress>
    {
        private UnfinalizedTwinDoorAddress InnerElement { get; set; }

        public TwinDoorAddress(UnfinalizedTwinDoorAddress innerElement, Action<TwinDoorAddress> mappingsInsertionCallback)
            : base(innerElement, mappingsInsertionCallback)
        {
            InnerElement = innerElement;
        }

        /// <summary>
        /// The in-game address of the room in which this twin is found.
        /// </summary>
        public string RoomAddress => InnerElement.RoomAddress;

        /// <summary>
        /// The in-game address of the this twin door.
        /// </summary>
        public string DoorAddress => InnerElement.DoorAddress;

        protected override bool PropagateLogicalOptions(ReadOnlyLogicalOptions logicalOptions)
        {
            // Nothing to do here
            return false;
        }

        public override bool CalculateLogicallyRelevant()
        {
            // A TwinDoorAddress is just data with no logical implications.
            return false;
        }
    }

    public class UnfinalizedTwinDoorAddress: AbstractUnfinalizedModelElement<UnfinalizedTwinDoorAddress, TwinDoorAddress>
    {
        public string RoomAddress { get; set; }

        public string DoorAddress { get; set; }

        public UnfinalizedTwinDoorAddress()
        {

        }

        public UnfinalizedTwinDoorAddress(RawTwinDoorAddress rawTwinAddress)
        {
            DoorAddress = rawTwinAddress.DoorAddress;
            RoomAddress = rawTwinAddress.RoomAddress;
        }

        protected override TwinDoorAddress CreateFinalizedElement(UnfinalizedTwinDoorAddress sourceElement, Action<TwinDoorAddress> mappingsInsertionCallback, ModelFinalizationMappings mappings)
        {
            return new TwinDoorAddress(sourceElement, mappingsInsertionCallback);
        }
    }
}
