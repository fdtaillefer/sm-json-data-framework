using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace sm_json_data_framework.Models.Techs
{
    public class TechContainer
    {
        public IEnumerable<TechCategory> TechCategories { get; set; } = Enumerable.Empty<TechCategory>();

        /// <summary>
        /// Builds and returns a list of all techs found inside this category (at any level).
        /// </summary>
        /// <returns></returns>
        public IEnumerable<Tech> SelectAllTechs()
        {
            return TechCategories.SelectMany(category => category.Techs).SelectMany(tech => tech.SelectWithExtensions()).ToList();
        }
    }
}
