// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2019 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.Dtos
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using System.Diagnostics.CodeAnalysis;
    using System.Collections.ObjectModel;
    using IES.Common;

    /// <summary>
    /// DTO to show all WBS info.
    /// </summary>
    [ExcludeFromCodeCoverage]
    [Serializable()]
    public class WbsDTO : UpdateableDTO, IWorkspaceMembership

    {
        /// <summary>
        /// Default Constructor
        /// </summary>
        public WbsDTO()
        {
            Id = -1;
            WbsNumber = null;
            WbsTitle = null;
            WorkspaceID = -1;
            Level = -1;
            ClinIDs = new Collection<int>();
            ClinsInUse = new Collection<int>();
            inUse = false;
        }

        /// <summary>
        /// Copy constructor
        /// </summary>
        /// <param name="dtoToCopy"></param>
        public WbsDTO(WbsDTO dtoToCopy)
        {
            if (dtoToCopy != null)
            {
                foreach (PropertyInfo prop in typeof(WbsDTO).GetProperties())
                {
                    if (prop.CanRead && prop.CanWrite)
                    {
                        this.GetType().GetProperty(prop.Name).SetValue(this, prop.GetValue(dtoToCopy, null), null);
                    }
                }
            }
        }

        /// <summary>
        /// Gets or sets WbsNumber
        /// </summary>
        public string WbsNumber { get; set; }

        /// <summary>
        /// Gets or sets WbsPaddedNumber
        /// </summary>
        public string WbsPaddedNumber { get; set; }

        /// <summary>
        /// Gets or sets WbsTitle
        /// </summary>
        public string WbsTitle { get; set; }

        /// <summary>
        /// This is the WBS Number + WBS Title
        /// </summary>
        public string WbsString { get { return IES.Common.Utilities.FormatNumberTitleString(WbsNumber, WbsTitle, " "); }
        }

        /// <summary>
        /// Gets or sets ClinIDs
        /// </summary>
        public Collection<int> ClinIDs { get; set; }

        /// <summary>
        /// Internal IEnum for loader processing only
        /// </summary>
        internal IEnumerable<int> ClinIDsIEnum { get; set; }


        /// <summary>
        /// Gets or sets ClinsInUse
        /// </summary>
        public Collection<int> ClinsInUse { get; set; }

        /// <summary>
        /// Internal IEnum for loader processing only
        /// </summary>
        internal IEnumerable<int> ClinsInUseIEnum { get; set; }

        /// <summary>
        /// Gets or sets WorkspaceID
        /// </summary>
        public int WorkspaceID { get; set; }

        /// <summary>
        /// Gets or sets Level
        /// </summary>
        public int Level { get; set; }

        /// <summary>
        /// Gets or sets inUse
        /// </summary>
        public bool inUse { get; set; }
    }
}
