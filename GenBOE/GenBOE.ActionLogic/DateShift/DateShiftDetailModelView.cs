// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2019 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.ActionLogic.DateShift
{
    using IES.Common;

    /// <summary>
    /// Detail Model View for a Date Shift.
    /// </summary>
    public class DateShiftDetailModelView
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DateShiftDetailModelView"/> class.
        /// </summary>
        public DateShiftDetailModelView()
        {
            this.Errors = new DateShiftErrors();
        }

        /// <summary>
        /// Gets or sets the DateShift Operation.
        /// </summary>
        public Operation Operation { get; set; }

        /// <summary>
        /// Gets or sets the type of the child modification.
        /// </summary>
        public ChildModificationType ChildModificationType { get; set; }

        /// <summary>
        /// Gets or sets the spread handling.  Not needed if ChildModificationType.NoChange.
        /// </summary>
        public SpreadHandling? SpreadHandling { get; set; }

        /// <summary>
        /// Gets or sets the # of months of a change (positive means growth or shift into the future, negative means shrink or shift into the past).
        /// </summary>
        public int MonthChange { get; set; }

        /// <summary>
        /// Gets or sets the (optional) New Curve to use when SpreadHandling.NewCurve is selected.
        /// </summary>
        public SpreadCurves? NewCurve { get; set; }

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
        /// Gets or sets the errors.
        /// </summary>
        public DateShiftErrors Errors { get; set; }

        /// <summary>
        /// Clones this instance without the Error Handling set.
        /// Useful for running a second validation run after initial run.
        /// </summary>
        /// <returns></returns>
        public DateShiftDetailModelView Clone()
        {
            return new DateShiftDetailModelView
            {
                ChildModificationType = this.ChildModificationType,
                MonthChange = this.MonthChange,
                NewCurve = this.NewCurve,
                Operation = this.Operation,
                SpreadHandling = this.SpreadHandling,
            };
        }
    }
}
