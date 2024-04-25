using sm_json_data_framework.Models.InGameStates;
using sm_json_data_framework.Models.Items;
using System;
using System.Collections.Generic;
using System.Text;

namespace sm_json_data_framework.Models.Requirements.ObjectRequirements.Integers
{
    /// <summary>
    /// A logical element which requires Samus to spend some frames in lava while under lava physics (i.e. with Gravity Suit turned off even if available).
    /// </summary>
    public class LavaPhysicsFrames: AbstractDamageNumericalValueLogicalElement
    {
        public LavaPhysicsFrames()
        {

        }

        public LavaPhysicsFrames(int frames) : base(frames)
        {

        }

        public override int CalculateDamage(SuperMetroidModel model, ReadOnlyInGameState inGameState, int times = 1, int previousRoomCount = 0)
        {
            int baseDamage = model.Rules.CalculateLavaPhysicsDamage(inGameState, Value) * times;
            return (int)(baseDamage * model.LogicalOptions.LavaLeniencyMultiplier);
        }

        public override IEnumerable<Item> GetDamageReducingItems(SuperMetroidModel model, ReadOnlyInGameState inGameState)
        {
            return model.Rules.GetLavaPhysicsDamageReducingItems(model, inGameState);
        }
    }
}
