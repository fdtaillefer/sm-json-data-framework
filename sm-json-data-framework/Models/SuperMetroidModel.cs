using sm_json_data_framework.Models.Connections;
using sm_json_data_framework.Models.Enemies;
using sm_json_data_framework.Models.GameFlags;
using sm_json_data_framework.Models.Helpers;
using sm_json_data_framework.Models.InGameStates;
using sm_json_data_framework.Models.Items;
using sm_json_data_framework.Models.Rooms;
using sm_json_data_framework.Models.Techs;
using sm_json_data_framework.Models.Weapons;
using sm_json_data_framework.Rules;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace sm_json_data_framework.Models
{
    // STITCHME This idea doesn't belong here, but there' no place to put it yet.
    // Maybe one way to go is figure out the direct requirements to get back to the start for all nodes, by moving backwards from the start.
    // If we can remember this cost for every node, then it'd be easy to know at any time what is required to be able to get out after reaching it.
    // And if we know we can get out, we know an acquired item, flag, or lock break is in logic.
    // This should pretty much do every possible route and stop evaluating any branch once it reaches the same situation a second time.
    // It can also stop evaluating anytime it reaches a point with a known escape that is strictly <= the current accumulated escape requirements.
    // Doing this will also require the ability to simplify logical requirements.
    //
    // The thing this will not account for is indirect route that use refills - that's another level.
    // I guess one way to do that would be to figure out the cost of reaching refill spots (maybe just from a given number of nodes or rooms away).
    // Then we could pick out nodes whose escape resource cost is above a given threshold and evaluate the option of going to the refill + escaping.
    // This will need a distinction between having X current resource and having a max of Y resource.
    // Example: I'm sitting somewhere in Norfair. I need to have 240 current energy to reach a refill, then I need at least 412 max energy to escape from there.
    //
    // Once that's all done, there would still be a forward movement pass to reach absolute requirements for putting anything in full logic w/ included escape.
    //
    // The SMZ3 version of this would evaluate not only from the ship, but also from all cross-game portals, because reaching LttP should count as a successful escape.
    // Should probably jump from one escape point to the next so that the later parts of one point's evaluation can run into the already-computed simpler escapes
    // around the portals, and stop processing.
    //
    // This whole process can probably still be applied if I decide to preprocess rooms to eliminate issues of in-room movement with obstacles.

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
        
        /// <summary>
        /// Options that describe what the player is expected to be able or unable to do.
        /// </summary>
        public LogicalOptions LogicalOptions { get; set; } = new LogicalOptions();

        /// <summary>
        /// A repository of game rules we are operating by.
        /// </summary>
        public SuperMetroidRules Rules { get; set; }

        /// <summary>
        /// The helpers in this model, mapped by name.
        /// </summary>
        public IDictionary<string, Helper> Helpers { get; set; } = new Dictionary<string, Helper>();

        /// <summary>
        /// The techs in this model, mapped by name.
        /// </summary>
        public IDictionary<string, Tech> Techs { get; set; } = new Dictionary<string, Tech>();

        /// <summary>
        /// Returns whether the shinespark tech is enabled.
        /// </summary>
        /// <returns></returns>
        public bool CanShinespark()
        {
            return LogicalOptions.IsTechEnabled(Techs["canShinespark"]);
        }

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
