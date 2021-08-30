// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.ActionLogic.IO.Import
{
    using System;
    using System.Collections.ObjectModel;
    using System.Diagnostics.CodeAnalysis;
    using GenBOE.DataBridge.DTO;
    using GenBOE.Dtos;

    [ExcludeFromCodeCoverage]
    public class ProjectImport
    {
        public ProjectImport()
        {
            this.WBSs = new Collection<WbsDTO>();
            this.BOEs = new Collection<BOEImport>();
            this.CLINs = new Collection<ClinDTO>();
            this.BoeTaskElements = new Collection<BoeTaskElementDTO>();
            this.Messages = new Collection<ImportErrorMessage>();
        }

        public Collection<WbsDTO> WBSs { get; set; }

        public Collection<BOEImport> BOEs { get; set; }

        public Collection<ClinDTO> CLINs { get; set; }

        public Collection<BoeTaskElementDTO> BoeTaskElements { get; set; }

        public Collection<ImportErrorMessage> Messages { get; set; }

    }

    public class BOEImport
    {
        public BOEImport()
        {
            this.Department = String.Empty;
            this.thisBOE = new BoeDTO();
        }

        public String Department { get; set; }
        public BoeDTO thisBOE { get; set; }
    }
}

