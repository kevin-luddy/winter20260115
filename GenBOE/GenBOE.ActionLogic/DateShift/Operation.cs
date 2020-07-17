// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2020 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.ActionLogic.DateShift
{
    /// <summary>
    /// A DateShift Operation
    /// </summary>
    public enum Operation
    {
        /// <summary>
        /// A shift of the date range to left or right
        /// </summary>
        Shift = 0,

        /// <summary>
        /// A change of the date range's end time (duration) to left or right
        /// </summary>
        DurationChange = 1
    }
}
