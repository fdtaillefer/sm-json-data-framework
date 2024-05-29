using sm_json_data_framework.Models;
using sm_json_data_framework.Models.Raw;
using sm_json_data_framework.Options;
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
        /// A modifiable, unfinalized model built using <see cref="RawModel"/>. 
        /// Please do not modify this as this will have side-effects on other tests.
        /// If you need to modify an unfinalized model, create your own.
        /// </summary>
        public static readonly UnfinalizedSuperMetroidModel UnfinalizedModel = new UnfinalizedSuperMetroidModel(RawModel);

        /// <summary>
        /// A mostly unmodifiable SuperMetroidModel, created with no customizations - so it uses all default rules and logical elements.
        /// To avoid causing side-effects to other tests, do not apply <see cref="LogicalOptions"/> to this model.
        /// If there is a need to do that, use <see cref="ModelWithOptions"/>.
        /// </summary>
        public static readonly SuperMetroidModel UnmodifiableModel = new UnfinalizedSuperMetroidModel(RawModel).Finalize();
        
        /// <summary>
        /// A SuperMetroidModel instance intended to have <see cref="LogicalOptions"/> applied to it during a test.
        /// If there is no need to apply logical options, consider using <see cref="UnfinalizedModel"/> instead.
        /// </summary>
        public static readonly SuperMetroidModel ModelWithOptions = new UnfinalizedSuperMetroidModel(RawModel).Finalize();
    }
}
