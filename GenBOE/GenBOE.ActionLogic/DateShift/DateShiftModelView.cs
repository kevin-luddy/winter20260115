// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2019 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.ActionLogic.DateShift
{
    using System.Collections.Generic;
    using GenBOE.Objects;

    /// <summary>
    /// Model View for a Date Shift.
    /// </summary>
    public class DateShiftModelView
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DateShiftModelView"/> class.
        /// </summary>
        public DateShiftModelView()
        {
            this.Details = new List<DateShiftDetailModelView>();
        }

        /// <summary>
        /// Gets or sets the details.
        /// </summary>
        /// <value>
        /// The details.
        /// </value>
        public ICollection<DateShiftDetailModelView> Details { get; set; }

        /// <summary>
        /// Gets or sets the workspace.
        /// </summary>
        public FullWorkspace Workspace { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether Error1 (negative DateRange) is fixed in the results to return a single month.  
        /// If not true and there is a negative DateRange, an exception will be thrown by the back-end.
        /// </summary>
        public bool? Error1FixSingleMonth { get; set; }

        /// <summary>
        /// Gets or sets the Error2 Handling.  
        /// Error2 is when the child dates would be outside of the new parent PoP - this can occur on all changes, depending on settings that the user selects.
        /// </summary>
        public ChildModificationType? Error2Handling { get; set; }

        /// <summary>
        /// Gets or sets the email option.
        /// </summary>
        public EmailOption EmailOption { get; set; }
    }
}