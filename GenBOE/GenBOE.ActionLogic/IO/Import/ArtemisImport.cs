// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.ActionLogic.IO.Import
{
    using System.Collections.ObjectModel;
    using System.Diagnostics.CodeAnalysis;
    using GenBOE.DataBridge.DTO;
    using GenBOE.Dtos;

    [ExcludeFromCodeCoverage]
    public class ArtemisImport
    {
        public ArtemisImport()
        {
            this.WBSs = new Collection<WbsDTO>();
            this.BOEs = new Collection<BoeDTO>();
            this.BoeTaskElements = new Collection<BoeTaskElementDTO>();
            this.Messages = new Collection<ImportErrorMessage>();
        }

        public Collection<WbsDTO> WBSs { get; set; }

        public Collection<BoeDTO> BOEs { get; set; }

        public Collection<BoeTaskElementDTO> BoeTaskElements { get; set; }

        public Collection<ImportErrorMessage> Messages { get; set; }
    }
}

