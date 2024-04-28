using sm_json_data_framework.Models.Requirements.ObjectRequirements;
using sm_json_data_framework.Models.Requirements.ObjectRequirements.Integers;
using sm_json_data_framework.Models.Requirements.ObjectRequirements.Strings;
using sm_json_data_framework.Models.Requirements.ObjectRequirements.SubObjects;
using sm_json_data_framework.Models.Requirements.ObjectRequirements.SubRequirements;
using sm_json_data_framework.Models.Requirements.StringRequirements;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace sm_json_data_framework.Models.Requirements
{
    /// <summary>
    /// A static class containing utility methods that are useful in the process of creating <see cref="AbstractLogicalElement"/>s.
    /// </summary>
    public static class LogicalElementCreationUtils
    {
        /// <summary>
        /// A dictionary that maps an ObjectLogicalElementTypeEnum to a a subtype of <see cref="AbstractObjectLogicalElement"/>
        /// that is the default Class to use to represent a logical element of that type.
        /// </summary>
        public static ReadOnlyDictionary<ObjectLogicalElementTypeEnum, Type> DefaultObjectLogicalElementTypes = new Dictionary<ObjectLogicalElementTypeEnum, Type>
        {
            { ObjectLogicalElementTypeEnum.And, typeof(And) },
            { ObjectLogicalElementTypeEnum.Or, typeof(Or) },

            { ObjectLogicalElementTypeEnum.AcidFrames, typeof(AcidFrames) },
            { ObjectLogicalElementTypeEnum.DraygonElectricityFrames, typeof(DraygonElectricityFrames) },
            { ObjectLogicalElementTypeEnum.EnergyAtMost, typeof(EnergyAtMost) },
            { ObjectLogicalElementTypeEnum.HeatFrames, typeof(HeatFrames) },
            { ObjectLogicalElementTypeEnum.HibashiHits, typeof(HibashiHits) },
            { ObjectLogicalElementTypeEnum.LavaFrames, typeof(LavaFrames) },
            { ObjectLogicalElementTypeEnum.LavaPhysicsFrames, typeof(LavaPhysicsFrames) },
            { ObjectLogicalElementTypeEnum.PreviousNode, typeof(PreviousNode) },
            { ObjectLogicalElementTypeEnum.SpikeHits, typeof(SpikeHits) },
            { ObjectLogicalElementTypeEnum.ThornHits, typeof(ThornHits) },

            { ObjectLogicalElementTypeEnum.PreviousStratProperty, typeof(PreviousStratProperty) },

            { ObjectLogicalElementTypeEnum.AdjacentRunway, typeof(AdjacentRunway) },
            { ObjectLogicalElementTypeEnum.Ammo, typeof(Ammo) },
            { ObjectLogicalElementTypeEnum.AmmoDrain, typeof(AmmoDrain) },
            { ObjectLogicalElementTypeEnum.CanComeInCharged, typeof(CanComeInCharged) },
            { ObjectLogicalElementTypeEnum.CanShineCharge, typeof(CanShineCharge) },
            { ObjectLogicalElementTypeEnum.EnemyDamage, typeof(EnemyDamage) },
            { ObjectLogicalElementTypeEnum.EnemyKill, typeof(EnemyKill) },
            { ObjectLogicalElementTypeEnum.ResetRoom, typeof(ResetRoom) }
        }.AsReadOnly();

        /// <summary>
        /// A dictionary that maps an ObjectLogicalElementTypeEnum to a a subtype of <see cref="AbstractObjectLogicalElement"/>
        /// that is the default Class to use to represent a logical element of that type.
        /// </summary>
        public static ReadOnlyDictionary<StringLogicalElementTypeEnum, Type> DefaultStringLogicalElementTypes = new Dictionary<StringLogicalElementTypeEnum, Type>
        {
            { StringLogicalElementTypeEnum.Never, typeof(NeverLogicalElement) },
            { StringLogicalElementTypeEnum.Helper, typeof(HelperLogicalElement) },
            { StringLogicalElementTypeEnum.Tech, typeof(TechLogicalElement) },
            { StringLogicalElementTypeEnum.Item, typeof(ItemLogicalElement) },
            { StringLogicalElementTypeEnum.Gameflag, typeof(GameFlagLogicalElement) }
        }.AsReadOnly();

        /// <summary>
        /// Creates and returns a dictionary containing ObjectLogicalElementTypeEnum-to-type mappings, 
        /// using the default mapping as a base but validating applying any provided override.
        /// </summary>
        /// <param name="overrideTypes">Overrides to apply. May be null.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentException">If any default type is not assignable from a requested override type</exception>
        public static IDictionary<ObjectLogicalElementTypeEnum, Type> CreateObjectLogicalElementTypeEnumMapping(IEnumerable<(ObjectLogicalElementTypeEnum typeEnum, Type type)> overrideTypes)
        {
            // Start with all default types
            IDictionary<ObjectLogicalElementTypeEnum, Type> logicalElementTypes = new Dictionary<ObjectLogicalElementTypeEnum, Type>(DefaultObjectLogicalElementTypes);

            // Validate and add override types
            overrideTypes ??= Array.Empty<(ObjectLogicalElementTypeEnum typeEnum, Type type)>();
            foreach (var (typeEnum, type) in overrideTypes)
            {
                Type defaultType = DefaultObjectLogicalElementTypes[typeEnum];
                if (!defaultType.IsAssignableFrom(type))
                {
                    throw new ArgumentException($"The C# type {type.Name} cannot be used to represent logical element '{typeEnum}' " +
                        $"Because type {defaultType} is not assignable from it");
                }
                logicalElementTypes.Add(typeEnum, type);
            }

            return logicalElementTypes;
        }

        /// <summary>
        /// Creates and returns a dictionary containing StringLogicalElementTypeEnum-to-type mappings, 
        /// using the default mapping as a base but validating applying any provided override.
        /// </summary>
        /// <param name="overrideTypes">Overrides to apply. May be null.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentException">If any default type is not assignable from a requested override type</exception>
        public static IDictionary<StringLogicalElementTypeEnum, Type> CreateStringLogicalElementTypeEnumMapping(IEnumerable<(StringLogicalElementTypeEnum typeEnum, Type type)> overrideTypes)
        {
            // Start with all default types
            IDictionary<StringLogicalElementTypeEnum, Type> logicalElementTypes = new Dictionary<StringLogicalElementTypeEnum, Type>(DefaultStringLogicalElementTypes);

            // Validate and add override types
            overrideTypes ??= Array.Empty<(StringLogicalElementTypeEnum typeEnum, Type type)>();
            foreach (var (typeEnum, type) in overrideTypes)
            {
                Type defaultType = DefaultStringLogicalElementTypes[typeEnum];
                if (!defaultType.IsAssignableFrom(type))
                {
                    throw new ArgumentException($"The C# type {type.Name} cannot be used to represent logical element '{typeEnum}' " +
                        $"Because type {defaultType} is not assignable from it");
                }
                logicalElementTypes.Add(typeEnum, type);
            }

            return logicalElementTypes;
        }

        /// <summary>
        /// Creates and returns a LogicalElementCreationKnowledgeBase based on the provided parameters
        /// </summary>
        /// <param name="model">A SuperMetroidModel containing helpers, items, helpers and techs</param>
        /// <param name="allowUninterpretedStringLogicalElements">Indicates whether it's ok to have string logical elements we are unable to interpret.
        /// If false, those will result in an exception instead</param>
        /// <param name="overrideObjectTypes">Overrides types to apply for object logical elements. May be null.</param>
        /// <param name="overrideStringTypes">Overrides types to apply for string logical elements. May be null.</param>
        /// <returns></returns>
        public static LogicalElementCreationKnowledgeBase CreateLogicalElementCreationKnowledgeBase(SuperMetroidModel model, 
            bool allowUninterpretedStringLogicalElements = false,
            IEnumerable<(ObjectLogicalElementTypeEnum typeEnum, Type type)> overrideObjectTypes = null,
            IEnumerable<(StringLogicalElementTypeEnum typeEnum, Type type)> overrideStringTypes = null)
        {
            return new LogicalElementCreationKnowledgeBase
            {
                AllowUninterpretedStringLogicalElements = allowUninterpretedStringLogicalElements,
                GameFlags = model.GameFlags,
                Helpers = model.Helpers,
                Items = model.Items,
                Techs = model.Techs,
                ObjectLogicalElementTypes = CreateObjectLogicalElementTypeEnumMapping(overrideObjectTypes),
                StringLogicalElementTypes = CreateStringLogicalElementTypeEnumMapping(overrideStringTypes)
            };
        }

    }
}
