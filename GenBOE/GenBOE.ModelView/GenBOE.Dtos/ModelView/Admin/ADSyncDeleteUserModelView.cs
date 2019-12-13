using System;
using System.Diagnostics.CodeAnalysis;
using IES.Common;

namespace GenBOE.Dtos
{
    [ExcludeFromCodeCoverage]
    public class ADSyncDeleteUserModelView : IEquatable<ADSyncDeleteUserModelView>
    {
        public ADSyncDeleteUserModelView()
        {
            WorkspaceID = 0;
            UserID = 0;
            GroupID = 0;
            RoleID = (int)Role.WorkspaceUser;
        }

        public int WorkspaceID { get; set; }

        public int UserID { get; set; }

        public int GroupID { get; set; }

        public int RoleID { get; set; }

        public bool Equals(ADSyncDeleteUserModelView other)
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

            return (other.GroupID == this.GroupID &&
                    other.RoleID == this.RoleID &&
                    other.UserID == this.UserID &&
                    other.WorkspaceID == this.WorkspaceID);
        }

        public override int GetHashCode()
        {
            return this.GroupID ^ this.RoleID ^ this.UserID ^ this.WorkspaceID;
        }
    }
}
