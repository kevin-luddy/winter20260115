// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace IES.Common.Core.Models
{
	using System;
	using System.Diagnostics.CodeAnalysis;
	using IES.Common.Core;

	/// <summary>
	/// Class representing AD information about a given user
	/// </summary>
	[ExcludeFromCodeCoverage, Serializable]
	public class UserData : IEquatable<UserData>
	{
		/// <summary>
		/// The ntid
		/// </summary>
		private string ntid;

		/// <summary>
		/// The email
		/// </summary>
		private string email;

		/// <summary>
		/// Constructor
		/// </summary>
		public UserData()
		{
		}

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
		/// Gets or sets the email
		/// </summary>
		public string Email
		{
			get
			{
				return email;
			}

			set
			{
				if (value == null)
				{
					throw new ArgumentNullException(nameof(value), "Email is being set to null");
				}

				email = value.ToLower();
			}
		}

		/// <summary>
		/// Gets or sets the first name
		/// </summary>
		public string FirstName { get; set; }

		/// <summary>
		/// Gets or sets the last name
		/// </summary>
		public string LastName { get; set; }

		/// <summary>
		/// Gets or sets the phone
		/// </summary>
		public string Phone { get; set; }

		/// <summary>
		/// Gets or sets the display name
		/// </summary>
		public string DisplayName { get; set; }

		/// <summary>
		/// Gets or sets the Company
		/// </summary>
		public string Company { get; set; }

		/// <summary>
		/// Gets or sets the State
		/// </summary>
		public string State { get; set; }

		/// <summary>
		/// Gets or sets the Country
		/// </summary>
		public string Country { get; set; }

		/// <summary>
		/// Gets or sets the Title
		/// </summary>
		public string Title { get; set; }

		/// <summary>
		/// Whether the account is a group account
		/// </summary>
		public bool IsGroup { get; set; }

		/// <summary>
		/// Gets or sets the employee Id.
		/// </summary>
		public string EmployeeId { get; set; }

		/// <summary>
		/// Overrides the to string to display all of the information.
		/// </summary>
		/// <returns>A string representation</returns>
		public override string ToString()
		{
			return string.Format("Ntid [{0}], Email [{1}], DisplayName [{2}], FirstName [{3}], LastName [{4}], Phone [{5}], Company [{6}], Title [{7}], Department [{8}], State [{9}], Country [{10}]",
				this.Ntid,
				this.Email,
				this.DisplayName,
				this.FirstName,
				this.LastName,
				this.Phone,
				this.Company,
				this.Title,
				this.Department,
				this.State,
				this.Country);
		}

		/// <summary>
		/// Compare Ntid ,which makes a user unique
		/// </summary>
		/// <param name="other">the 'other' UserData to compare</param>
		/// <returns>true if Ntids are equal, false otherwise</returns>
		public bool Equals(UserData other)
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

		/// <summary>
		/// Is the Person US based (vs international)
		/// 
		/// AD Property -> lmcUSAPersonIndicator -> values: Y / N
		/// </summary>
		public bool? IsUsPerson { get; set; }

		/// <summary>
		/// Is the Employee a subcontractor (vs LM employee)
		/// 
		/// AD Property -> employeeType -> values: G sub, E is emp
		/// </summary>
		public bool? IsSubcontractor { get; set; }

		/// <summary>
		/// Gets the user's department
		/// </summary>
		public string Department { get; set; }
	}
}
