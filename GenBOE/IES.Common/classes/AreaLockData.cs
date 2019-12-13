// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2019 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace IES.Common
{
    using System;
    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    /// Class representing Area Locking information, including time of lock and who created the lock
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class AreaLockData
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public AreaLockData()
        {
            this.Area = LockArea.None;
        }

        /// <summary>
        /// Gets/Sets the Area for the lock
        /// </summary>
        public LockArea Area { get; set; }

        /// <summary>
        /// Gets string of Area description
        /// </summary>
        public string AreaString
        {
            get
            {
                return this.Area.ToDescription();
            }
        }

        /// <summary>
        /// Gets/Sets the User Data for the user who created the lock
        /// </summary>
        public UserData LockedBy { get; set; }

        /// <summary>
        /// Gets/Sets the time the lock was created
        /// </summary>
        public DateTime TimeOfLock { get; set; }
    }
}
