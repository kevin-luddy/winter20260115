// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2020 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.ActionLogic.DateShift
{
    using System.ComponentModel;

    /// <summary>
    /// This enum is capturing how we will adjust the child objects during DateShift
    /// </summary>
    public enum ChildModificationType
    {
        /// <summary>
        /// Not Set
        /// </summary>
        [Description("Not Set")]
        NotSet = -1,

        /// <summary>
        /// No change to the child object.
        /// If selected in error handling option, it'll result in object not being changed and the user having to manually fix the issue after.
        /// </summary>
        [Description("No Change")]
        NoChange = 0,

        /// <summary>
        /// The change gets Flowed Down (applied the same way) to the child objects.
        /// </summary>
        [Description("Flowdown")]
        FlowDown = 1,

        /// <summary>
        /// The child object is aligned / filled to the Period of Performance (it will match the parent's start and end dates).
        /// </summary>
        [Description("To PoP - Period of Performance")]
        ToPoP = 2,

        /// <summary>
        /// Only applicable when doing a positive shift (e.g. moving the PoP to the future).
        /// We set the child start date to the parent start date.
        /// We keep the original duration of the child object.
        /// </summary>
        [Description("To Start")]
        ToStart = 3,

        /// <summary>
        /// Applicable when doing a negative shift, OR when doing a duration modification.
        ///     Operation.DurationChange - We keep the original child start date, modify duration to make the end dates the same (child and parent)
        ///     Operation.Shift (Negative) - Child end date is set to parent end date. Keep original child duration.
        /// </summary>
        [Description("To End")]
        ToEnd = 4
    }
}
