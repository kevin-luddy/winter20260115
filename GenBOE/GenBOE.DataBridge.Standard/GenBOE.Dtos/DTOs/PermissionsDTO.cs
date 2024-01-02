using System;
using IES.Standard;
using System.Diagnostics.CodeAnalysis;

namespace GenBOE.Dtos
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
            if (Object.ReferenceEquals(other, null))
            {
                return false;
            }

            // Check whether the compared object references the same data.
            if (Object.ReferenceEquals(this, other))
            {
                return true;
            }

            return (other.ETIUserId == this.ETIUserId &&
                    other.WorkspaceId == this.WorkspaceId &&
                    other.Role == this.Role &&
                    other.BOEId == this.BOEId);
        }

        public bool isGroup()
        {
            if (this.NTID.Contains("."))
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
            return this.ETIUserId ^ (this.WorkspaceId ?? 0) ^ (int)this.Role ^ (this.BOEId ?? 0);
        }

        public bool HideWorkspaceHelp { get; set; }
    }
}
