using sm_json_data_framework.Models.Requirements;
using sm_json_data_framework.Models.Requirements.ObjectRequirements.Integers;
using sm_json_data_framework.Options;

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
        /// <param name="logicalOptions">The logical options that are in effect, and may need to be applied to a created executable</param>
        /// <param name="rules">The rules that are in effect, and may be needed to apply logical options</param>
        /// <returns></returns>
        public static IExecutable FramesExecutable(this PhysicsEnum physics, int frames, ReadOnlyLogicalOptions logicalOptions, SuperMetroidRules rules)
        {
            switch (physics)
            {
                case PhysicsEnum.Normal:
                case PhysicsEnum.Water:
                    return LogicalRequirements.AlwaysRequirements.Instance;
                case PhysicsEnum.Lava:
                    LavaFrames lavaFrames = new LavaFrames(frames);
                    // You normally shouldn't apply logical options out of the blue, but this is a temporary element with no ties to any elements in the model,
                    // and it needs the logical options to have access to leniency
                    lavaFrames.ApplyLogicalOptions(logicalOptions, rules);
                    return lavaFrames;
                case PhysicsEnum.Acid:
                    AcidFrames acidFrames = new AcidFrames(frames);
                    // You normally shouldn't apply logical options out of the blue, but this is a temporary element with no ties to any elements in the model,
                    // and it needs the logical options to have access to leniency
                    acidFrames.ApplyLogicalOptions(logicalOptions, rules);
                    return acidFrames;
                default:
                    throw new System.NotImplementedException();
            }
        }
    }
}
