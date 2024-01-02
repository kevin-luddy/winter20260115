// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.Dtos
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using IES.Standard;

    [ExcludeFromCodeCoverage]
    [Serializable()]
    public class CustomFieldDTO : UpdateableDTO, IWorkspaceMembership
    {
        public CustomFieldDTO()
        {
            CustomFieldName = string.Empty;
            CustomFieldRequired = false;
            IsOpenEnded = false;
            WorkspaceID = -1;
        }

        public string CustomFieldName { get; set; }

        public CustomFieldType CustomFieldDisplayID { get; set; }        

        public bool CustomFieldRequired { get; set; }

        /// <summary>
        /// Gets/Sets bool noting if Custom Field is Open Ended
        /// </summary>
        public bool IsOpenEnded { get; set; }

        public int WorkspaceID { get; set; }
    }
}
