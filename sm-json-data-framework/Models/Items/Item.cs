﻿using sm_json_data_framework.Models.Enemies;
using sm_json_data_framework.Options;
using sm_json_data_framework.Rules;
using System;
using System.Collections.Generic;
using System.Text;

namespace sm_json_data_framework.Models.Items
{
    /// <summary>
    /// Represents an item, regardless of whether this is explicitly an item in the game or just implicitly an item.
    /// </summary>
    public class Item : AbstractModelElement<UnfinalizedItem, Item>, ILogicalExecutionPreProcessable
    {
        public Item(UnfinalizedItem sourceElement, Action<Item> mappingsInsertionCallback)
            : base(sourceElement, mappingsInsertionCallback)
        {
            Name = sourceElement.Name;
        }

        /// <summary>
        /// The unique name of this item.
        /// </summary>
        public string Name { get; }

        protected override void PropagateLogicalOptions(ReadOnlyLogicalOptions logicalOptions, SuperMetroidModel model)
        {
            // Nothing to do here
        }

        protected override void UpdateLogicalProperties(SuperMetroidModel model)
        {
            base.UpdateLogicalProperties(model);
            LogicallyNever = CalculateLogicallyNever(model);
            LogicallyAlways = CalculateLogicallyAlways(model);
        }

        public override bool CalculateLogicallyRelevant(SuperMetroidModel model)
        {
            // An item that can't be obtained may as well not exist
            return !CalculateLogicallyNever(model);
        }

        public bool LogicallyNever { get; private set; }

        /// <summary>
        /// Calculates what the value of <see cref="LogicallyNever"/> should currently be.
        /// </summary>
        /// <param name="model">The model this element belongs to</param>
        /// <returns></returns>
        protected bool CalculateLogicallyNever(SuperMetroidModel model)
        {
            // If the item is logically removed, it can never be used.
            return !AppliedLogicalOptions.IsItemInGame(this);
        }

        public bool LogicallyAlways { get; private set; }

        /// <summary>
        /// Calculates what the value of <see cref="LogicallyAlways"/> should currently be.
        /// </summary>
        /// <param name="model">The model this element belongs to</param>
        /// <returns></returns>
        protected bool CalculateLogicallyAlways(SuperMetroidModel model)
        {
            // Item is always usable if the game always starts with it (and it's not logically removed)
            return AppliedLogicalOptions.IsItemInGame(this) && AppliedLogicalOptions.StartConditions.StartingInventory.HasItem(this);
        }

        public bool LogicallyFree => LogicallyAlways; // Just owning an item can never cost resources
    }

    public class UnfinalizedItem: AbstractUnfinalizedModelElement<UnfinalizedItem, Item>
    {
        public string Name { get; set; }

        public UnfinalizedItem() { }

        public UnfinalizedItem (string name)
        {
            Name = name;
        }

        protected override Item CreateFinalizedElement(UnfinalizedItem sourceElement, Action<Item> mappingsInsertionCallback, ModelFinalizationMappings mappings)
        {
            return new Item(sourceElement, mappingsInsertionCallback);
        }
    }
}
