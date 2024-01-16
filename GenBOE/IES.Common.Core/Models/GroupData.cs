// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace IES.Common.Core.Models
{
	using System;
	using System.Diagnostics.CodeAnalysis;
	using IES.Common.Core.Utilities;

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
				return ntid;
			}

			set
			{
				if (value == null)
				{
					throw new ArgumentNullException(nameof(value), "Ntid is being set to null");
				}

				ntid = value.ToLower();
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
				Ntid,
				DisplayName);
		}

		/// <summary>
		/// Compare Ntids which identify a unique group.
		/// </summary>
		/// <param name="other">the 'other' UserData to compare</param>
		/// <returns>true if Ntids are equal, false otherwise</returns>
		public bool Equals(GroupData other)
		{
			// Check whether the compared object is null.
			if (ReferenceEquals(other, null))
			{
				return false;
			}

			// Check whether the compared object references the same data.
			if (ReferenceEquals(this, other))
			{
				return true;
			}

			return other.Ntid.IsEquivalentTo(Ntid);
		}

		/// <summary>
		/// Returns the hash code for the user
		/// </summary>
		/// <returns>The hash code</returns>
		public override int GetHashCode()
		{
			return Ntid.GetHashCode();
		}
	}
}
