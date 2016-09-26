//-------------------------------------------------------------------------------------------------------
// Copyright (C) Microsoft Corporation and contributors. All rights reserved.
// See LICENSE.txt file in the project root for full license information.
//-------------------------------------------------------------------------------------------------------

namespace WebSec.Plugins
{
    /// <summary>
    /// Implementation of a Levenshtein Distance algorithm.
    /// </summary>
    public static class EditDistance
    {
        /// <summary>
        /// Maximum string index to be considered for comparisons.
        /// </summary>
        public const int MaxStringComparisonIndex = 16 * 1000;

        /// <summary>
        /// Iterative Levenshtein distance calculation with two rows.
        /// See: https://en.wikipedia.org/wiki/Levenshtein_distance
        /// </summary>
        /// <param name="firstValue">First string.</param>
        /// <param name="secondValue">Second string.</param>
        /// <returns>Levenshtein distance between the strings.</returns>
        public static int ComputeDistance(string firstValue, string secondValue)
        {
            // Allocates static array for Levenshtein distances between strings.
            int[] previousStringDistances = new int[MaxStringComparisonIndex];

            // Allocates static array for Levenshtein distances between strings.
            int[] newStringDistances = new int[MaxStringComparisonIndex];

            // If one of the strings is empty, returns the length of the other
            if (string.IsNullOrEmpty(firstValue))
            {
                return string.IsNullOrEmpty(secondValue) ? 0 : secondValue.Length;
            }

            if (string.IsNullOrEmpty(secondValue))
            {
                return firstValue.Length;
            }

            if (firstValue.Equals(secondValue))
            {
                return 0;
            }

            int maxS2Index = (secondValue.Length >= MaxStringComparisonIndex) ? MaxStringComparisonIndex : secondValue.Length + 1;

            // Initially: distance equal to number of chars up to that index.
            for (int i = 0; i < maxS2Index; i++)
            {
                previousStringDistances[i] = i;
            }

            int maxS1Index = (firstValue.Length < MaxStringComparisonIndex) ? firstValue.Length : (MaxStringComparisonIndex - 1);

            for (int i = 0; i < maxS1Index; i++)
            {
                newStringDistances[0] = i + 1;

                for (int j = 0; j < (maxS2Index - 1); j++)
                {
                    int cost = (firstValue[i] == secondValue[j]) ? 0 : 1;
                    int cost1 = newStringDistances[j] + 1;
                    int cost2 = previousStringDistances[j + 1] + 1;
                    int cost3 = previousStringDistances[j] + cost;

                    int min = (cost1 < cost2) ? cost1 : cost2;
                    newStringDistances[j + 1] = (min < cost3) ? min : cost3;
                }

                for (int j = 0; j < maxS2Index; j++)
                {
                    previousStringDistances[j] = newStringDistances[j];
                }
            }

            return newStringDistances[maxS2Index - 1];
        }
    }
}