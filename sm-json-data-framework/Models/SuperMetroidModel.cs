using sm_json_data_parser.Models.Enemies;
using sm_json_data_parser.Models.GameFlags;
using sm_json_data_parser.Models.Helpers;
using sm_json_data_parser.Models.Items;
using sm_json_data_parser.Models.Rooms;
using sm_json_data_parser.Models.Techs;
using sm_json_data_parser.Models.Weapons;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace sm_json_data_parser.Models
{
    /// <summary>
    /// Represents a Super Metroid world, for a set of logical options. Think of this as being able to represent 
    /// e.g. the vanilla game or a randomizer seed.
    /// </summary>
    public class SuperMetroidModel
    {
        public SuperMetroidModel()
        {
            // Weapons can't have an initializer directly on itself because of the custom setter
            Weapons = new Dictionary<string, Weapon>();
        }

        public LogicalOptions LogicalOptions { get; set; } = new LogicalOptions();

        public IDictionary<string, Helper> Helpers { get; set; } = new Dictionary<string, Helper>();

        public IDictionary<string, Tech> Techs { get; set; } = new Dictionary<string, Tech>();

        public IDictionary<string, Item> Items { get; set; } = new Dictionary<string, Item>();

        public IDictionary<string, GameFlag> GameFlags { get; set; } = new Dictionary<string, GameFlag>();

        public IDictionary<string, Room> Rooms { get; set; } = new Dictionary<string, Room>();

        // STITCHME I think Connections can just be tacked onto nodes?
        private IDictionary<string, Weapon> _weapons;
        public IDictionary<string, Weapon> Weapons {
            get { return _weapons; }
            set
            {
                _weapons = value;
                WeaponsByCategory = Weapons.Values
                    .SelectMany(w => w.Categories.Select(c => (weapon: w, category: c)))
                    .GroupBy(pair => pair.category)
                    .ToDictionary(g => g.Key, g => g.ToList().Select(pair => pair.weapon));
            }
        }

        public IDictionary<WeaponCategoryEnum, IEnumerable<Weapon>> WeaponsByCategory { get; private set; }

        public IDictionary<string, Enemy> Enemies { get; set; } = new Dictionary<string, Enemy>();

        public IDictionary<string, Enemy> Bosses { get; set; } = new Dictionary<string, Enemy>();
    }
}
