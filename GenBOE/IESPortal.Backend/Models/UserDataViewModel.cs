// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2025 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace IESPortal.Backend.Models
{
	/// <summary>
	/// User data look up result view model.
	/// 
	/// THIS MUST MATCH THE BOE SERVICE, DO NOT CHANGE UNLESS CHANGES ARE MADE TO BOTH CLASSES.
	/// </summary>
	public class UserDataViewModel
	{
		/// <summary>
		/// The account associated to the user.
		/// </summary>
		public string UserAccount { get; set; }

		/// <summary>
		/// Full name of the user.
		/// </summary>
		public string UserFullName { get; set; }

		/// <summary>
		/// Is this a group?
		/// </summary>
		public bool IsGroup { get; set; }

		/// <summary>
		/// Work phone.
		/// </summary>
		public string WorkPhone { get; set; }
	}
}