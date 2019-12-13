// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2019 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.ActionLogic.DateShift
{
    /// <summary>
    /// How to handle discrete spreads during DateShift.
    /// Discrete spreads will copy the array from the starting month, until the new duration is filled.
    ///     That means that if the duration is the same, or expanded, we do not need to do anything else
    ///     if the duration is negative though, then we will have some hours / costs left over and we need to decide how to handle these based on below choice.
    /// </summary>
    public enum SpreadHandling
    {
        /// <summary>
        /// Spread Handling is not set
        /// </summary>
        NotSet = 0,
        /// <summary>
        /// Apply a specific curve to the Discrete results. 
        /// The Curve will be selected outside of this Enum.
        /// </summary>
        NewCurve = 1,

        /// <summary>
        /// Leftovers are applied to the first month.
        /// </summary>
        DiscreteToFirst = 2,

        /// <summary>
        /// Leftovers are applied to the last month
        /// </summary>
        DiscreteToLast = 3,

        /// <summary>
        /// Leftovers are thrown away, this will result in an error, user will have to manually fix
        /// </summary>
        DiscreteToError = 4
    }
}
