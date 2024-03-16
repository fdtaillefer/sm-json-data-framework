using System;
using System.Collections.Generic;
using System.Text;

namespace sm_json_data_framework.Models.Navigation.ConsoleInterface
{
    /// <summary>
    /// And abstract class for a command in the console interface.
    /// </summary>
    public abstract class ConsoleCommand
    {
        public string Name { get; protected set; }

        public string SampleFormat { get; protected set; }

        public string Description { get; protected set; }

        public string[] Options { get; protected set; } = null;

        public Predicate<string> ValidCommand { get; protected set; }

        public Func<GameNavigator, string, bool> Execution { get; protected set; }

        /// <summary>
        /// Extracts individual options from the options portion of a commandline string. They are returned as a series of keyword-value pairs.
        /// Option keywords must start with '-' to be recognized. Options without an accompanying value will be accompanied by a null value.
        /// All values will be interpreted as strings, and can optionally be encased in either single our double quotes (which will be left out).
        /// No escaping is supported. Spaces can only be interpreted correctly if the value is in quotes, and a single or double quote in the value can only be interpreted correct if the value is within the other type of quote.
        /// </summary>
        /// <param name="optionsString">The options string from which to extract individual options</param>
        /// <returns></returns>
        /// <exception cref="InvalidCommandStringException">Thrown if the string cannot be interpreted into options</exception>
        protected IEnumerable<(string keyword, string value)> ExtractOptions(string optionsString)
        {
            List<(string keyword, string value)> options = new List<(string keyword, string value)>();
            string remainingOptions = optionsString;
            while (remainingOptions != null && remainingOptions.Length > 0)
            {
                // An option's keyword should always start with '-'
                if (!remainingOptions.StartsWith("-"))
                {
                    throw new InvalidCommandStringException(GetType());
                }

                string[] tokens = remainingOptions.Split(' ', 2);
                string optionKeyword = tokens[0];

                string postKeywordString = tokens.Length == 1 ? null : tokens[1].Trim();

                // If this option came with no value
                if (postKeywordString == null || postKeywordString.Length == 0 || postKeywordString.StartsWith("-"))
                {
                    // Next option starts immediately after in the string
                    remainingOptions = postKeywordString;
                    options.Add((optionKeyword, null));
                } // There is a value, figure out what it is
                // Value is within single or double quotes
                else if (postKeywordString.StartsWith("'") || postKeywordString.StartsWith("\""))
                {
                    // First token will be empty, second token should be our value until the second quote, third token should be the remainder
                    string[] postKeywordTokens = postKeywordString.Split(postKeywordString[0], 3);
                    // If there isn't another paired quote, give up
                    if(postKeywordTokens.Length < 2)
                    {
                        throw new InvalidCommandStringException(GetType());
                    }
                    options.Add((optionKeyword, postKeywordTokens[1]));
                    remainingOptions = postKeywordTokens.Length == 2 ? null : postKeywordTokens[2].Trim();
                }
                // Value is not within quotes, so ends at the next space or at the end of the string
                else
                {
                    string[] postKeywordTokens = postKeywordString.Split(" ", 2);
                    options.Add((optionKeyword, postKeywordTokens[0]));
                    remainingOptions = postKeywordTokens.Length == 1 ? null : postKeywordTokens[1].Trim();
                }
            }
            
            return options;
        }
    }
}
