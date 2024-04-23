﻿using sm_json_data_framework.Models.Rooms;
using sm_json_data_framework.Models.Rooms.Nodes;
using System;
using System.Collections.Generic;
using System.Text;

namespace sm_json_data_framework.Models
{
    /// <summary>
    /// An interface for models that can be told to initialize some data after the deserializing process, and that are positioned inside a node.
    /// Those will need the node (and its room) to be provided when being initialized.
    /// </summary>
    public interface InitializablePostDeserializeInNode
    {
        /// <summary>
        /// Initializes properties in this model, which wouldn't be initialized by simply parsing the relevant json file and are simple references to
        /// other objects of the model.
        /// </summary>
        /// <param name="model">The model to use to initialize the foreign properties</param>
        /// <param name="room">The room in which this model is located</param>
        /// <param name="node">The node in which this model is located</param>
        public void InitializeForeignProperties(SuperMetroidModel model, Room room, RoomNode node);

        /// <summary>
        /// Initializes properties in this model, which wouldn't be initialized by simply parsing the relevant json file and are not simple references to
        /// other objects of the model. This should be called after foreign properties have been initialized.
        /// </summary>
        /// <param name="model">The model to use to initialize the properties</param>
        /// <param name="room">The room in which this model is located</param>
        /// <param name="node">The node in which this model is located</param>
        public void InitializeOtherProperties(SuperMetroidModel model, Room room, RoomNode node);

        /// <summary>
        /// Eliminates values from this model, which are made useless by the logical options of the provided model.
        /// This should be called after foreign properties have been initialized.
        /// </summary>
        /// <param name="model">The model to use to initialize the properties</param>
        /// <param name="room">The room in which this model is located</param>
        /// <param name="node">The node in which this model is located</param>
        /// <returns>Whether this object is still useful itself after the cleanup</returns>
        public bool CleanUpUselessValues(SuperMetroidModel model, Room room, RoomNode node);

        /// <summary>
        /// <para>Goes through all logical elements within this model and any relevant sub-model,
        /// attempting to resolve all references within logical elements into the object they are referencing.
        /// Those resolved values are then used to initialize corresponding properties.</para>
        /// <para>Concrete implementations are allowed to depend on data initialized by <see cref="Initialize(SuperMetroidModel)"/>, so this must be
        /// called only after that method has been called.</para>
        /// </summary>
        /// <param name="model">A SuperMetroidModel that contains global data</param>
        /// <param name="room">The room in which this model is located</param>
        /// <param name="node">The node in which this model is located</param>
        /// <returns>A sequence of strings describing references that could not be resolved.</returns>
        public IEnumerable<string> InitializeReferencedLogicalElementProperties(SuperMetroidModel model, Room room, RoomNode node);
    }
}
