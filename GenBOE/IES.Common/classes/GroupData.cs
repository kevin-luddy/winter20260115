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
    /// Stores Group Data obtained from AD
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class GroupData : IEquatable<GroupData>
    {
        /// <summary>
        /// The ntid
        /// </summary>
        private string ntid;

        /// <summary>
        /// Gets or sets the ntid
        /// </summary>
        public string Ntid
        {
            get
            {
                return this.ntid;
            }

            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException(nameof(value), "Ntid is being set to null");
                }

                this.ntid = value.ToLower();
            }
        }

        /// <summary>
        /// Gets or sets the display name
        /// </summary>
        public string DisplayName { get; set; }

        /// <summary>
        /// Overrides the to string to display all of the information.
        /// </summary>
        /// <returns>A string representation</returns>
        public override string ToString()
        {
            return string.Format("Ntid [{0}], DisplayName [{1}]",
                this.Ntid,
                this.DisplayName);
        }

        /// <summary>
        /// Compare Ntids which identify a unique group.
        /// </summary>
        /// <param name="other">the 'other' UserData to compare</param>
        /// <returns>true if Ntids are equal, false otherwise</returns>
        public bool Equals(GroupData other)
        {
            // Check whether the compared object is null.
            if (object.ReferenceEquals(other, null))
            {
                return false;
            }

            // Check whether the compared object references the same data.
            if (object.ReferenceEquals(this, other))
            {
                return true;
            }

            return other.Ntid.IsEquivalentTo(this.Ntid);
        }

        /// <summary>
        /// Returns the hash code for the user
        /// </summary>
        /// <returns>The hash code</returns>
        public override int GetHashCode()
        {
            return this.Ntid.GetHashCode();
        }
    }
}
