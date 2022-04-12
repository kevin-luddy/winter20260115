// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.Dtos
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using IES.Common;

    /// <summary>
    /// Base DTO for BOE Forms.
    /// </summary>
    /// <seealso cref="GenBOE.Dtos.UpdateableDTO" />
    [ExcludeFromCodeCoverage]
    [Serializable()]
    public abstract class BOEFormDTO : UpdateableDTO
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BOEFormDTO"/> class.
        /// </summary>
        protected BOEFormDTO()
        {
            this.Id = -1;
            this.ResourceIds = new List<int>();
            this.ClinContractTypes = new List<BoeFormClinContractTypeDTO>();
            this.Revision = 1;
            this.FormName = string.Empty;
        }

        /// <summary>
        /// Gets the type of the boe form.
        /// </summary>
        public abstract BOEFormType BOEFormType {get;}

        /// <summary>
        /// Gets or sets the name of the form.
        /// </summary>
        public string FormName { get; set; }

        /// <summary>
        /// Gets or sets the resource ids.
        /// </summary>
        public ICollection<int> ResourceIds { get; set; }

        /// <summary>
        /// Enumeration for Resource IDs, only used during loading from Entity Framework.
        /// </summary>
        internal IEnumerable<int> ResourceIdsIEnum { get; set; }

        /// <summary>
        /// Gets or sets the clin contract types.
        /// </summary>
        public ICollection<BoeFormClinContractTypeDTO> ClinContractTypes { get; set; }

        /// <summary>
        /// Enumeration for Clin Contract Types, only used during loading from Entity Framework.
        /// </summary>
        internal IEnumerable<BoeFormClinContractTypeDTO> ClinContractTypesIEnum { get; set; }

        /// <summary>
        /// Gets or sets the version.
        /// </summary>
        public int Version { get; set; }

        /// <summary>
        /// Gets or sets the revision.
        /// </summary>
        public int Revision { get; set; }

        /// <summary>
        /// Gets or sets the proposal title.
        /// </summary>
        public string ProposalTitle { get; set; }

        /// <summary>
        /// Gets or sets the proposal date.
        /// </summary>
        public string ProposalDate { get; set; }

        /// <summary>
        /// Gets or sets the description.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the poc.
        /// </summary>
        public string Poc { get; set; }

        /// <summary>
        /// Gets or sets the poc phone.
        /// </summary>
        public string PocPhone { get; set; }

        /// <summary>
        /// Gets or sets the approver.
        /// </summary>
        public string Approver { get; set; }

        /// <summary>
        /// Gets or sets the approver phone.
        /// </summary>
        public string ApproverPhone { get; set; }

        /// <summary>
        /// Gets or sets the workspace identifier.
        /// </summary>
        public int WorkspaceId { get; set; }
    }
}
