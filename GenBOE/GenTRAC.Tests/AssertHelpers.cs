// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2020 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenTRAC.Tests
{
    using System;
    using System.Collections.Generic;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>
    /// Assert Helpers
    /// </summary>
    public static class AssertHelpers
    {
        /// <summary>
        /// Asert the equality of a (rounded) decimal value vs. the expected integer value.
        /// </summary>
        /// <param name="actual">Decimal value</param>
        /// <param name="expected">Integer value</param>
        public static void AssertAreEqual(decimal actual, int expected)
        {
            Assert.AreEqual(expected, System.Convert.ToInt32(decimal.Round(actual, System.MidpointRounding.AwayFromZero)));
        }

        /// <summary>
        /// Asert the equality of a (rounded) decimal value vs. the expected value.
        /// </summary>
        /// <param name="actual">Decimal value</param>
        /// <param name="expected">Expected value</param>
        /// <param name="decimals">Total number of decimals to round to</param>
        public static void AssertAreEqual(decimal actual, decimal expected, int decimals)
        {
            Assert.AreEqual(expected, decimal.Round(actual, decimals));
        }

        /// <summary>
        /// Assert that the items in each list have the same values in the same order
        /// </summary>
        /// <param name="expected">Expected list</param>
        /// <param name="actual">Actual list</param>
        public static void AssertAreEqual<T>(IEnumerable<T> expected, IEnumerable<T> actual) where T : IComparable
        {
            List<T> expectedValues = new List<T>(expected);
            List<T> actualValues = new List<T>(actual);

            Assert.AreEqual(expectedValues.Count, actualValues.Count);

            for (int i = 0; i < expectedValues.Count; i++)
            {
                Assert.AreEqual(0, expectedValues[i].CompareTo(actualValues[i]));
            }
        }
    }
}
