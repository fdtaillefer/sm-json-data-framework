using sm_json_data_framework.Models;
using sm_json_data_framework.Models.Raw;
using sm_json_data_framework.Reading;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace sm_json_data_framework.Tests.TestTools
{
    public static class StaticTestObjects
    {
        /// <summary>
        /// The raw result of reading json files, available as a reusable object so the files can be read only once across all tests.
        /// </summary>
        public static readonly RawSuperMetroidModel RawModel = ModelReader.ReadRawModel();

        /// <summary>
        /// A mostly unmodifiable SuperMetroidModel, created with no customizations - so it uses all default rules and logical elements.
        /// Though mostly unmodifiable, there is a risk of side-effects if you apply logical options on this and don't undo them, so take care with that.
        /// </summary>
        public static readonly SuperMetroidModel UnmodifiableModel = new SuperMetroidModel(new UnfinalizedSuperMetroidModel(RawModel));
    }
}
