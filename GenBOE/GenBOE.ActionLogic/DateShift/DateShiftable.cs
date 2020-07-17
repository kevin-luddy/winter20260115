// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2020 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.ActionLogic.DateShift
{
    using System;
    using System.Collections.Generic;
    using IES.Common;

    /// <summary>
    /// DTO implementation of IDateShiftable
    /// </summary>
    /// <seealso cref="IES.Common.IDateShiftable" />
    public class DateShiftable : IDateShiftable
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DateShiftable"/> class.
        /// </summary>
        public DateShiftable()
        {
            this.Children = new List<IDateShiftable>();
        }

        /// <summary>
        /// Gets or sets the start date.
        /// </summary>
        public DateTime? StartDate { get; set; }

        /// <summary>
        /// Gets or sets the end date.
        /// </summary>
        public DateTime? EndDate { get; set; }

        /// <summary>
        /// Gets the children that can be shifted.
        /// </summary>
        public ICollection<IDateShiftable> Children { get; private set; }

        /// <summary>
        /// Gets a value indicating whether this instance has a spread of values.
        /// </summary>
        public bool HasSpread { get; set; }

        /// <summary>
        /// Gets the date shift level.
        /// </summary>
        public Level DateShiftLevel { get; set; }

        /// <summary>
        /// Gets or sets the Update Type.
        /// </summary>
        public UpdateType Updateable { get; set; }

        /// <summary>
        /// Gets the linked boe identifier.
        /// </summary>
        public int BoeId { get; set; }
    }
}
