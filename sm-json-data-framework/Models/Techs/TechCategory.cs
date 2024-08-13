using sm_json_data_framework.Models.Raw.Techs;
using sm_json_data_framework.Options;
using sm_json_data_framework.Rules;
using sm_json_data_framework.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace sm_json_data_framework.Models.Techs
{
    /// <summary>
    /// A category of techs, which contains <see cref="Tech"/>s of a similar nature (as grouped in the json model).
    /// </summary>
    public class TechCategory : AbstractModelElement<UnfinalizedTechCategory, TechCategory>
    {
        public TechCategory(UnfinalizedTechCategory sourceElement, Action<TechCategory> mappingsInsertionCallback, ModelFinalizationMappings mappings)
            : base(sourceElement, mappingsInsertionCallback)
        {
            Name = sourceElement.Name;
            Description = sourceElement.Description;
            FirstLevelTechs = sourceElement.Techs.Select(tech => tech.Finalize(mappings)).ToDictionary(tech => tech.Name).AsReadOnly();
            Techs = FirstLevelTechs.Values.SelectMany(tech => tech.SelectWithExtensions()).ToDictionary(tech => tech.Name).AsReadOnly();
        }

        /// <summary>
        /// A name that uniquely identifies this TechCategory.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// A description which explains what kinds of techs this TechCategory contains.
        /// </summary>
        public string Description { get; }

        /// <summary>
        /// The first-level techs inside this category, mapped by name. This excludes any and all extension techs (to include those, see <see cref="Techs"/>.
        /// </summary>
        public IReadOnlyDictionary<string, Tech> FirstLevelTechs { get; } = new Dictionary<string, Tech>();

        /// <summary>
        /// All techs inside this category, mapped by name. This includes any and all extension techs (to exclude those, see <see cref="FirstLevelTechs"/>.
        /// </summary>
        public IReadOnlyDictionary<string, Tech> Techs { get; } = new Dictionary<string, Tech>();
        protected override void PropagateLogicalOptions(ReadOnlyLogicalOptions logicalOptions, SuperMetroidModel model)
        {
            foreach (Tech tech in Techs.Values)
            {
                tech.ApplyLogicalOptions(logicalOptions, model);
            }
        }
        public override bool CalculateLogicallyRelevant(SuperMetroidModel model)
        {
            // A cateogry with no relevant techs may as well not exist.
            return Techs.Values.WhereLogicallyRelevant().Any();
        }
    }

    public class UnfinalizedTechCategory : AbstractUnfinalizedModelElement<UnfinalizedTechCategory, TechCategory>
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public IList<UnfinalizedTech> Techs { get; set; } = new List<UnfinalizedTech>();

        public UnfinalizedTechCategory(RawTechCategory rawTechCategory)
        {
            Name = rawTechCategory.Name;
            Description = rawTechCategory.Description;
            Techs = rawTechCategory.Techs.Select(subTech => new UnfinalizedTech(subTech)).ToList();
        }

        protected override TechCategory CreateFinalizedElement(UnfinalizedTechCategory sourceElement, Action<TechCategory> mappingsInsertionCallback, ModelFinalizationMappings mappings)
        {
            return new TechCategory(sourceElement, mappingsInsertionCallback, mappings);
        }
    }
}
