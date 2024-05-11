using sm_json_data_framework.Models.Requirements;
using sm_json_data_framework.Models.Requirements.ObjectRequirements.Integers;

namespace sm_json_data_framework.Rules
{
    public enum PhysicsEnum
    {
        Normal,
        Water,
        Lava,
        Acid
    }

    public static class PhysicsEnumExtensions{

        /// <summary>
        /// Creates and returns an executable that will attempt to spend the provided number of frames in this physics. This can only consume energy, depending on loadout.
        /// </summary>
        /// <param name="physics">This physics</param>
        /// <param name="frames">The number of frames to spend in this physics</param>
        /// <returns></returns>
        public static IExecutable FramesExecutable(this PhysicsEnum physics, int frames)
        {
            return physics switch
            {
                PhysicsEnum.Normal => UnfinalizedLogicalRequirements.AlwaysRequirements.Instance,
                PhysicsEnum.Water => UnfinalizedLogicalRequirements.AlwaysRequirements.Instance,
                PhysicsEnum.Lava => new UnfinalizedLavaFrames(frames),
                PhysicsEnum.Acid => new UnfinalizedAcidFrames(frames),
                _ => throw new System.NotImplementedException()
            };
        }
    }
}
