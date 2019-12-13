// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2018 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenTRAC.Common
{
    using System;

    /// <summary>
    /// Flag for indicating whether enum value is active or inactive
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    public sealed class IsActiveAttribute : Attribute
    {
        /// <summary>
        /// Is Active
        /// </summary>
        private bool isActive;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="isActive">Is Active</param>
        public IsActiveAttribute(bool isActive)
        {
            this.isActive = isActive;
        }

        /// <summary>
        /// Is Active
        /// </summary>
        public bool IsActive
        {
            get
            {
                return this.isActive;
            }
        }
    }
}
