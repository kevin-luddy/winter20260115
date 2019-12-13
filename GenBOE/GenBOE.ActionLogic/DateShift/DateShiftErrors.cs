// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2019 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.ActionLogic.DateShift
{
    using System.Collections.Generic;

    /// <summary>
    /// Errors that occur during DateShift
    /// </summary>
    public class DateShiftErrors
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DateShiftErrors"/> class.
        /// </summary>
        public DateShiftErrors ()
        {
            this.Messages = new HashSet<string>();
            this.AffectedBoeIds = new HashSet<int>();
        }

        /// <summary>
        /// Gets or sets the messages.
        /// </summary>
        public ICollection<string> Messages { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this dateshift has Error1 (negative DateRange).
        /// </summary>
        public bool HasError1 { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this dateshift has Error2.
        /// Error2 is when the child dates would be outside of the new parent PoP.
        /// </summary>
        public bool HasError2 { get; set; }

        /// <summary>
        /// Gets or sets the affected boe ids that have errors.
        /// </summary>
        public HashSet<int> AffectedBoeIds { get; set; }
    }
}
