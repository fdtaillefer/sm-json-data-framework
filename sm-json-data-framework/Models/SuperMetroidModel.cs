using sm_json_data_framework.Models.Connections;
using sm_json_data_framework.Models.Enemies;
using sm_json_data_framework.Models.GameFlags;
using sm_json_data_framework.Models.Helpers;
using sm_json_data_framework.Models.InGameStates;
using sm_json_data_framework.Models.Items;
using sm_json_data_framework.Models.Rooms;
using sm_json_data_framework.Models.Techs;
using sm_json_data_framework.Models.Weapons;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace sm_json_data_framework.Models
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

        /// <summary>
        /// The helpers in this model, mapped by name.
        /// </summary>
        public IDictionary<string, Helper> Helpers { get; set; } = new Dictionary<string, Helper>();

        /// <summary>
        /// The techs in this model, mapped by name.
        /// </summary>
        public IDictionary<string, Tech> Techs { get; set; } = new Dictionary<string, Tech>();

        /// <summary>
        /// The items in this model, mapped by name.
        /// </summary>
        public IDictionary<string, Item> Items { get; set; } = new Dictionary<string, Item>();

        /// <summary>
        /// The game flags in this model, mapped by name.
        /// </summary>
        public IDictionary<string, GameFlag> GameFlags { get; set; } = new Dictionary<string, GameFlag>();

        /// <summary>
        /// The rooms in this model, mapped by name.
        /// </summary>
        public IDictionary<string, Room> Rooms { get; set; } = new Dictionary<string, Room>();

        /// <summary>
        /// A dictionary that maps a node's IdentifyingString to a connection.
        /// Be aware that this means each connection will likely be present twice.
        /// </summary>
        public IDictionary<string, Connection> Connections { get; set; } = new Dictionary<string, Connection>();

        private IDictionary<string, Weapon> _weapons;
        /// <summary>
        /// The weapons in this model, mapped by name.
        /// </summary>
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

        /// <summary>
        /// A dicionary mapping all weapon categories to a list of weapons in that category.
        /// </summary>
        public IDictionary<WeaponCategoryEnum, IEnumerable<Weapon>> WeaponsByCategory { get; private set; }

        /// <summary>
        /// The normal enemies in this model, mapped by name.
        /// </summary>
        public IDictionary<string, Enemy> Enemies { get; set; } = new Dictionary<string, Enemy>();

        /// <summary>
        /// The boss enemies in this model, mapped by name.
        /// </summary>
        public IDictionary<string, Enemy> Bosses { get; set; } = new Dictionary<string, Enemy>();

        public InGameState InitialGameState { private get; set; }

        /// <summary>
        /// Creates and returns a copy of the initial game state.
        /// </summary>
        /// <returns></returns>
        public InGameState CreateInitialGameStateCopy()
        {
            return new InGameState(InitialGameState);
        }
    }
}
