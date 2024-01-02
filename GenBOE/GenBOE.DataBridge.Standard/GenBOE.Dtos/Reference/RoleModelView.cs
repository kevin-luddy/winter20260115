using System;
using System.Diagnostics.CodeAnalysis;

namespace GenBOE.Dtos
{
    [ExcludeFromCodeCoverage]
    [Serializable()]
    public class RoleModelView
    {
        public RoleModelView()
        {
            RoleID = 0;
            RoleName = String.Empty;
        }

        public int RoleID { get; set; }

        public string RoleName { get; set; }
    }
}
