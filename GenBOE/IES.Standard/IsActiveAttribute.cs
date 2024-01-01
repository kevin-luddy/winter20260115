// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace IES.Standard
{
    using System;

    /// <summary>
    /// Flag for indicating whether enum value is active or inactive
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    public sealed class IsActiveAttribute : Attribute
    {

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="isActive">Is Active</param>
		public IsActiveAttribute(bool isActive)
        {
            this.IsActive = isActive;
        }

		/// <summary>
		/// Is Active
		/// </summary>
		public bool IsActive { get; }
	}
}
