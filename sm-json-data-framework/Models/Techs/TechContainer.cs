using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace sm_json_data_framework.Models.Techs
{
    public class TechContainer
    {
        public IList<TechCategory> TechCategories { get; set; } = new List<TechCategory>();

        /// <summary>
        /// Builds and returns a list of all techs found inside this category (at any level).
        /// </summary>
        /// <returns></returns>
        public IList<Tech> SelectAllTechs()
        {
            return TechCategories.SelectMany(category => category.Techs).SelectMany(tech => tech.SelectWithExtensions()).ToList();
        }
    }
}
