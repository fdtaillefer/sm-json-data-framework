using System;
using System.Collections.Generic;
using System.Text;

namespace sm_json_data_framework.Models.Requirements.ObjectRequirements
{
    /// <summary>
    /// An enum identifying the possible types of object logical elements.
    /// </summary>
    public enum ObjectLogicalElementTypeEnum
    {
        And,
        Or,

        AcidFrames,
        DraygonElectricityFrames,
        EnergyAtMost,
        HeatFrames,
        HibashiHits,
        LavaFrames,
        LavaPhysicsFrames,
        PreviousNode,
        SpikeHits,
        ThornHits,

        PreviousStratProperty,

        AdjacentRunway,
        Ammo,
        AmmoDrain,
        CanComeInCharged,
        CanShineCharge,
        EnemyDamage,
        EnemyKill,
        ResetRoom
    }

    public enum ObjectLogicalElementSubTypeEnum
    {
        Integer,
        String,
        SubObject,
        SubRequirement
    }

    public static class ObjectLogicalElementTypeEnumExtensions
    {
        public static ObjectLogicalElementSubTypeEnum GetSubType(this ObjectLogicalElementTypeEnum type)
        {
            switch (type)
            {
                case ObjectLogicalElementTypeEnum.And:
                case ObjectLogicalElementTypeEnum.Or:
                    return ObjectLogicalElementSubTypeEnum.SubRequirement;
                case ObjectLogicalElementTypeEnum.AcidFrames:
                case ObjectLogicalElementTypeEnum.DraygonElectricityFrames:
                case ObjectLogicalElementTypeEnum.EnergyAtMost:
                case ObjectLogicalElementTypeEnum.HeatFrames:
                case ObjectLogicalElementTypeEnum.HibashiHits:
                case ObjectLogicalElementTypeEnum.LavaFrames:
                case ObjectLogicalElementTypeEnum.LavaPhysicsFrames:
                case ObjectLogicalElementTypeEnum.PreviousNode:
                case ObjectLogicalElementTypeEnum.SpikeHits:
                case ObjectLogicalElementTypeEnum.ThornHits:
                    return ObjectLogicalElementSubTypeEnum.Integer;
                case ObjectLogicalElementTypeEnum.PreviousStratProperty:
                    return ObjectLogicalElementSubTypeEnum.String;
                case ObjectLogicalElementTypeEnum.AdjacentRunway:
                case ObjectLogicalElementTypeEnum.Ammo:
                case ObjectLogicalElementTypeEnum.AmmoDrain:
                case ObjectLogicalElementTypeEnum.CanComeInCharged:
                case ObjectLogicalElementTypeEnum.CanShineCharge:
                case ObjectLogicalElementTypeEnum.EnemyDamage:
                case ObjectLogicalElementTypeEnum.EnemyKill:
                case ObjectLogicalElementTypeEnum.ResetRoom:
                    return ObjectLogicalElementSubTypeEnum.SubObject;
                default:
                    throw new ArgumentException($"Unsupported Objsect logical element type {type}");
            }
        }
    }

}
