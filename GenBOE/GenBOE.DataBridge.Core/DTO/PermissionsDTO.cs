using System;
using IES.Common;
using System.Diagnostics.CodeAnalysis;
using IES.Common.Core.Models;
using IES.Common.Core.Enums;

namespace GenBOE.DataBridge.Core.DTO
{
	[ExcludeFromCodeCoverage]
	[Serializable()]
	public class PermissionsDTO : UpdateableDTO, IEquatable<PermissionsDTO>
	{
		/// <summary>
		/// default constructor
		/// </summary>
		public PermissionsDTO()
		{
			PermissionId = -1;
			ETIUserId = int.MinValue;
			NTID = null;
			WorkspaceId = null;
			Role = Role.WorkspaceUser;
			BOEId = null;
			HideWorkspaceHelp = false;
		}

		public int PermissionId { get; set; }

		public int ETIUserId { get; set; }

		public string NTID { get; set; }

		public int? WorkspaceId { get; set; }

		public Role Role { get; set; }

		public int? BOEId { get; set; }

		public bool Equals(PermissionsDTO other)
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

			return other.ETIUserId == ETIUserId &&
					other.WorkspaceId == WorkspaceId &&
					other.Role == Role &&
					other.BOEId == BOEId;
		}

		public bool isGroup()
		{
			if (NTID.Contains("."))
			{
				return true;
			}
			else
			{
				return false;
			}
		}

		public override int GetHashCode()
		{
			return ETIUserId ^ (WorkspaceId ?? 0) ^ (int)Role ^ (BOEId ?? 0);
		}

		public bool HideWorkspaceHelp { get; set; }
	}
}
