﻿using sm_json_data_parser.Converters;
using sm_json_data_parser.Models.Requirements.ObjectRequirements.SubRequirements;
using sm_json_data_parser.Models.Requirements.StringRequirements;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace sm_json_data_parser.Models.Requirements
{
    public class LogicalRequirements
    {
        public LogicalRequirements()
        {

        }

        public LogicalRequirements(IEnumerable<AbstractLogicalElement> logicalElements)
        {
            LogicalElements = LogicalElements.Concat(logicalElements);
        }

        public IEnumerable<AbstractLogicalElement> LogicalElements { get; set; } = Enumerable.Empty<AbstractLogicalElement>();

        /// <summary>
        /// Goes through all logical elements within this LogicalRequirements (and all LogicalRequirements within any of them),
        /// attempting to replace any RawStringLogicalElement by a more appropriate logical element, using the provided StringLogicalElementConverter.
        /// </summary>
        /// <param name="stringElementConverter">A StringLogicalElementConverter that can attempt to convert a raw string into a more specific logical element</param>
        /// <returns>All strings that couldn't be resolved to a more specific logical element</returns>
        public IEnumerable<string> ReplaceRawStringRequirements(StringLogicalElementConverter stringElementConverter)
        {
            List<string> unresolvedRawStrings = new List<string>();

            // For any logical element in this LogicalRequirements that has sub-LogicalRequirements, do a recursive call
            foreach(AbstractObjectLogicalElementWithSubRequirements logicalElement in LogicalElements.OfType<AbstractObjectLogicalElementWithSubRequirements>())
            {
                IEnumerable<string> unresolvedSubRawStrings = logicalElement.LogicalRequirements.ReplaceRawStringRequirements(stringElementConverter);
                unresolvedRawStrings.AddRange(unresolvedSubRawStrings);
            }

            // For any raw string logical element we contain directly, attempt to resolve it and substitute the new logical element
            LogicalElements = LogicalElements.Select(le =>
            {
                // If this is a raw string, get the converter to convert it to hopefully something better
                if(le is RawStringLogicalElement rawStringElement)
                {
                    AbstractLogicalElement newLogicalElement = stringElementConverter.CreateLogicalElement(rawStringElement.StringValue);
                    // if we still have a raw string, add the string to the list of unresolved strings we'll return
                    if (newLogicalElement is RawStringLogicalElement)
                    {
                        unresolvedRawStrings.Add(rawStringElement.StringValue);
                        return le;
                    }
                    // If we successfully resolved the string, substitute the new element
                    else
                    {
                        return newLogicalElement;
                    }
                }
                else
                {
                    return le;
                }
            });

            return unresolvedRawStrings.Distinct();
        }

        // When evaluating this, we should have an `and` parameter that defaults to true
    }
}
