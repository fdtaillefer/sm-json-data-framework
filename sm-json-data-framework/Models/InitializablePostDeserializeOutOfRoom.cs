using System;
using System.Collections.Generic;
using System.Text;

namespace sm_json_data_framework.Models
{
    /// <summary>
    /// An interface for models that can be told to initialize some data after the deserializing process, and whose existence is not within the context of a room.
    /// Those will not need a room to be provided when being initialized.
    /// </summary>
    public interface InitializablePostDeserializeOutOfRoom
    {
        /// <summary>
        /// Initializes properties in this model, which wouldn't be initialized by simply parsing the relevant json file.
        /// </summary>
        /// <param name="model">The model to use to initialize the foreign properties</param>
        public void InitializeProperties(SuperMetroidModel model);

        /// <summary>
        /// <para>Goes through all logical elements within this model and any relevant sub-model,
        /// attempting to resolve all references within logical elements into the object they are referencing.
        /// Those resolved values are then used to initialize corresponding properties.</para>
        /// <para>Concrete implementations are allowed to depend on data initialized by <see cref="Initialize(SuperMetroidModel)"/>, so this must be
        /// called only after that method has been called.</para>
        /// </summary>
        /// <param name="model">A SuperMetroidModel that contains global data</param>
        /// <returns>A sequence of strings describing references that could not be resolved.</returns>
        public IEnumerable<string> InitializeReferencedLogicalElementProperties(SuperMetroidModel model);
    }
}
