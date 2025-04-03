// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenTRAC.DataBridge.Core.DTO.User
{
	using System;
	using IES.Common.Core.Enums;
	using IES.Common.Core.Models;

	/// <summary>
	/// Encapsulate a user in the system
	/// </summary>
	[Serializable]
	public class UserDTO : UpdateableDTO, IES.Common.Core.Interfaces.ICachableDTO
	{
		/// <summary>
		/// Constructor
		/// </summary>
		public UserDTO()
		{
			Id = int.MinValue;
			Ntid = string.Empty;
			DisplayName = string.Empty;
			EmailAddress = string.Empty;
			PhoneNumber = string.Empty;
			FirstName = string.Empty;
			LastName = string.Empty;
			IsGroup = false;
			TimeLastAccessed = DateTime.Now;
		}

		/// <summary>
		/// for ICachableDTO..
		/// </summary>
		/// <returns>Primary Key</returns>
		public int GetPrimaryKeyID()
		{
			return Id;
		}

		/// <summary>
		/// Phone Number
		/// </summary>
		public string PhoneNumber { get; set; }

		/// <summary>
		/// Email Address
		/// </summary>
		public string EmailAddress { get; set; }

		/// <summary>
		/// Ntid
		/// </summary>
		[System.Text.Json.Serialization.JsonPropertyName("NTID")]
		public string Ntid { get; set; }

		/// <summary>
		/// First Name
		/// </summary>
		public string FirstName { get; set; }

		/// <summary>
		/// Last Name
		/// </summary>
		public string LastName { get; set; }

		/// <summary>
		/// Display Name
		/// </summary>
		public string DisplayName { get; set; }

		/// <summary>
		/// UserType Enum
		/// </summary>
		public UserType UserType { get; set; }

		/// <summary>
		/// determines if the user is an individual or group
		/// </summary>
		public bool IsGroup { get; set; }

		/// <summary>
		/// The date/time the user last accessed the server
		/// </summary>
		public DateTime TimeLastAccessed { get; set; }

		/// <summary>
		/// The last proposal accessed by the user.
		/// </summary>
		public string LastProposalAccessed { get; set; }

		/// <summary>
		/// Time in h:mm:ss since the last program accessed.
		/// </summary>
		public string TimeSinceLastAccess
		{
			get
			{
				return DateTime.Now.Subtract(TimeLastAccessed).ToString(@"h\:mm\:ss");
			}
		}

		/// <summary>
		/// To String method
		/// </summary>
		/// <returns>Returns a string representation of the User Dto</returns>
		public override string ToString()
		{
			return string.Format("userid {0}, displayname {1}, Ntid {2}, emailaddress {3}, phonenumber {4}, firstname {5}, lastname {6}, timelastaccessed {7}, timesincelastaccess {8}",
								Id,
								DisplayName,
								Ntid,
								EmailAddress,
								PhoneNumber,
								FirstName,
								LastName,
								TimeLastAccessed,
								TimeSinceLastAccess);
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
		/// Passthrough for Id
		/// </summary>
		public int UserID
		{
			get
			{
				return this.Id;
			}
			set
			{
				this.Id = value;
			}
		}
	}
}
