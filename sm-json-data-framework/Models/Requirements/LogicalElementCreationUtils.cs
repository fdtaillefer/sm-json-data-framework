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
    /// A static class containing utility methods that are useful in the process of creating <see cref="AbstractUnfinalizedLogicalElement"/>s.
    /// </summary>
    public static class LogicalElementCreationUtils
    {
        /// <summary>
        /// A dictionary that maps an ObjectLogicalElementTypeEnum to a a subtype of <see cref="AbstractUnfinalizedObjectLogicalElement"/>
        /// that is the default Class to use to represent a logical element of that type.
        /// </summary>
        public static ReadOnlyDictionary<ObjectLogicalElementTypeEnum, Type> DefaultObjectLogicalElementTypes = new Dictionary<ObjectLogicalElementTypeEnum, Type>
        {
            { ObjectLogicalElementTypeEnum.And, typeof(UnfinalizedAnd) },
            { ObjectLogicalElementTypeEnum.Or, typeof(UnfinalizedOr) },

            { ObjectLogicalElementTypeEnum.AcidFrames, typeof(UnfinalizedAcidFrames) },
            { ObjectLogicalElementTypeEnum.DraygonElectricityFrames, typeof(UnfinalizedDraygonElectricityFrames) },
            { ObjectLogicalElementTypeEnum.EnergyAtMost, typeof(UnfinalizedEnergyAtMost) },
            { ObjectLogicalElementTypeEnum.HeatFrames, typeof(UnfinalizedHeatFrames) },
            { ObjectLogicalElementTypeEnum.HibashiHits, typeof(UnfinalizedHibashiHits) },
            { ObjectLogicalElementTypeEnum.LavaFrames, typeof(UnfinalizedLavaFrames) },
            { ObjectLogicalElementTypeEnum.LavaPhysicsFrames, typeof(UnfinalizedLavaPhysicsFrames) },
            { ObjectLogicalElementTypeEnum.PreviousNode, typeof(UnfinalizedPreviousNode) },
            { ObjectLogicalElementTypeEnum.SpikeHits, typeof(UnfinalizedSpikeHits) },
            { ObjectLogicalElementTypeEnum.ThornHits, typeof(UnfinalizedThornHits) },

            { ObjectLogicalElementTypeEnum.PreviousStratProperty, typeof(UnfinalizedPreviousStratProperty) },

            { ObjectLogicalElementTypeEnum.AdjacentRunway, typeof(UnfinalizedAdjacentRunway) },
            { ObjectLogicalElementTypeEnum.Ammo, typeof(UnfinalizedAmmo) },
            { ObjectLogicalElementTypeEnum.AmmoDrain, typeof(UnfinalizedAmmoDrain) },
            { ObjectLogicalElementTypeEnum.CanComeInCharged, typeof(UnfinalizedCanComeInCharged) },
            { ObjectLogicalElementTypeEnum.CanShineCharge, typeof(UnfinalizedCanShineCharge) },
            { ObjectLogicalElementTypeEnum.EnemyDamage, typeof(UnfinalizedEnemyDamage) },
            { ObjectLogicalElementTypeEnum.EnemyKill, typeof(UnfinalizedEnemyKill) },
            { ObjectLogicalElementTypeEnum.ResetRoom, typeof(UnfinalizedResetRoom) }
        }.AsReadOnly();

        /// <summary>
        /// A dictionary that maps an ObjectLogicalElementTypeEnum to a a subtype of <see cref="AbstractUnfinalizedObjectLogicalElement"/>
        /// that is the default Class to use to represent a logical element of that type.
        /// </summary>
        public static ReadOnlyDictionary<StringLogicalElementTypeEnum, Type> DefaultStringLogicalElementTypes = new Dictionary<StringLogicalElementTypeEnum, Type>
        {
            { StringLogicalElementTypeEnum.Never, typeof(UnfinalizedNeverLogicalElement) },
            { StringLogicalElementTypeEnum.Helper, typeof(UnfinalizedHelperLogicalElement) },
            { StringLogicalElementTypeEnum.Tech, typeof(UnfinalizedTechLogicalElement) },
            { StringLogicalElementTypeEnum.Item, typeof(UnfinalizedItemLogicalElement) },
            { StringLogicalElementTypeEnum.Gameflag, typeof(UnfinalizedGameFlagLogicalElement) }
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
                logicalElementTypes[typeEnum] = type;
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
                logicalElementTypes[typeEnum] = type;
            }

            return logicalElementTypes;
        }

        /// <summary>
        /// Creates and returns a LogicalElementCreationKnowledgeBase based on the provided parameters
        /// </summary>
        /// <param name="model">A SuperMetroidModel containing helpers, items, helpers and techs</param>
        /// <param name="overrideObjectTypes">Overrides types to apply for object logical elements. May be null.</param>
        /// <param name="overrideStringTypes">Overrides types to apply for string logical elements. May be null.</param>
        /// <returns></returns>
        public static LogicalElementCreationKnowledgeBase CreateLogicalElementCreationKnowledgeBase(UnfinalizedSuperMetroidModel model, 
            IEnumerable<(ObjectLogicalElementTypeEnum typeEnum, Type type)> overrideObjectTypes = null,
            IEnumerable<(StringLogicalElementTypeEnum typeEnum, Type type)> overrideStringTypes = null)
        {
            return new LogicalElementCreationKnowledgeBase
            {
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
