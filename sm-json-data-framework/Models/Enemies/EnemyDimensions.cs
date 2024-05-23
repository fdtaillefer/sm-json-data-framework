using sm_json_data_framework.Models.Raw.Enemies;
using sm_json_data_framework.Options;
using sm_json_data_framework.Rules;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace sm_json_data_framework.Models.Enemies
{
    /// <summary>
    /// The graphical dimensions of an enemy.
    /// </summary>
    public class EnemyDimensions : AbstractModelElement<UnfinalizedEnemyDimensions, EnemyDimensions>
    {
        public EnemyDimensions(UnfinalizedEnemyDimensions sourceElement, Action<EnemyDimensions> mappingsInsertionCallback)
            : base(sourceElement, mappingsInsertionCallback)
        {
            Height = sourceElement.Height;
            Width = sourceElement.Width;
        }

        /// <summary>
        /// The height of the enemy, in pixels.
        /// </summary>
        public int Height { get; }

        /// <summary>
        /// The width of the enemy, in pixels.
        /// </summary>
        public int Width { get; }

        protected override void PropagateLogicalOptions(ReadOnlyLogicalOptions logicalOptions, SuperMetroidRules rules)
        {
            // Nothing to do here
        }

        public override bool CalculateLogicallyRelevant(SuperMetroidRules rules)
        {
            // Enemy dimensions are just data with no logical implications.
            // They have in-game implications but that's too complex to be calculated here.
            return false;
        }
    }

    public class UnfinalizedEnemyDimensions :AbstractUnfinalizedModelElement<UnfinalizedEnemyDimensions, EnemyDimensions>
    {
        public int Height { get; set; }

        public int Width { get; set; }

        public UnfinalizedEnemyDimensions()
        {

        }

        public UnfinalizedEnemyDimensions(RawEnemyDimensions dimensions)
        {
            Height = dimensions.H;
            Width = dimensions.W;
        }

        protected override EnemyDimensions CreateFinalizedElement(UnfinalizedEnemyDimensions sourceElement, Action<EnemyDimensions> mappingsInsertionCallback, ModelFinalizationMappings mappings)
        {
            return new EnemyDimensions(sourceElement, mappingsInsertionCallback);
        }
    }
}
