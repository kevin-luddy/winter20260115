using System;
using System.Diagnostics.CodeAnalysis;
using IES.Common;

namespace GenBOE.Dtos
{
    [ExcludeFromCodeCoverage]
    [Serializable()]
    //DTO that contains the workspace version meta data
    public class WorkspaceVersionMetaDataDTO : UpdateableDTO
    {
        public WorkspaceVersionMetaDataDTO()
        {
            VersionID = -1;
            VersionName = string.Empty;
            DateCreated = DateTime.MinValue;
            VersionState = WorkspaceState.Working;
        }

        public int VersionID { get; set; }
        public string VersionName { get; set; }
        public DateTime DateCreated { get; set; }
        public int CreatedByID { get; set; }
        public WorkspaceState VersionState { get; set; }
        public int WorkspaceID { get; set; }

    }
}
