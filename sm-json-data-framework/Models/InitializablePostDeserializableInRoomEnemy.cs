﻿using sm_json_data_framework.Models.Rooms;
using System;
using System.Collections.Generic;
using System.Text;

namespace sm_json_data_framework.Models
{
    /// <summary>
    /// An interface for models that can be told to initialize some data after the deserializing process, and that are positioned inside a roomEnemy.
    /// Those will need the roomEnemy, and its room to be provided when being initialized.
    /// </summary>
    public interface InitializablePostDeserializableInRoomEnemy
    {
        /// <summary>
        /// <para>Initializes additional properties in this model, which wouldn't be initialized by simply parsing the relevant json file.
        /// All such properties are expected to be identified in their own documentation and should not be read if this method isn't called.</para>
        /// <para>Returns an enumeration of callbacks that should be executed only after the rest of the room has been initialized</para>
        /// </summary>
        /// <param name="model">The model to use to initialize the additional properties</param>
        /// <param name="room">The room in which this model is located</param>
        /// <param name="roomEnemy">The roomEnemy in which this model is located</param>
        public IEnumerable<Action> Initialize(SuperMetroidModel model, Room room, RoomEnemy roomEnemy);

        /// <summary>
        /// <para>Goes through all logical elements within this model and any relevant sub-model,
        /// attempting to resolve all references within logical elements into the object they are referencing.
        /// Those resolved values are then used to initialize corresponding properties.</para>
        /// <para>Concrete implementations are allowed to depend on data initialized by <see cref="Initialize(SuperMetroidModel)"/>, so this must be
        /// called only after that method has been called.</para>
        /// </summary>
        /// <param name="model">A SuperMetroidModel that contains global data</param>
        /// <param name="room">The room in which this model is located</param>
        /// <param name="roomEnemy">The roomEnemy in which this model is located</param>
        /// <returns>A sequence of strings describing references that could not be resolved.</returns>
        public IEnumerable<string> InitializeReferencedLogicalElementProperties(SuperMetroidModel model, Room room, RoomEnemy roomEnemy);
    }
}
