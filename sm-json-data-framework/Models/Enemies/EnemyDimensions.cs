﻿using sm_json_data_framework.Models.Raw.Enemies;
using sm_json_data_framework.Options;
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
        private UnfinalizedEnemyDimensions InnerElement { get; set; }

        public EnemyDimensions(UnfinalizedEnemyDimensions innerElement, Action<EnemyDimensions> mappingsInsertionCallback)
            : base(innerElement, mappingsInsertionCallback)
        {
            InnerElement = innerElement;
        }

        /// <summary>
        /// The height of the enemy, in pixels.
        /// </summary>
        public int Height => InnerElement.Height;

        /// <summary>
        /// The width of the enemy, in pixels.
        /// </summary>
        public int Width => InnerElement.Width;

        protected override void PropagateLogicalOptions(ReadOnlyLogicalOptions logicalOptions)
        {
            // Nothing to do here
        }

        public override bool CalculateLogicallyRelevant()
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
