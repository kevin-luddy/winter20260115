// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2020 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.ActionLogic.ModelView
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.ComponentModel.DataAnnotations;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using GenBOE.ActionLogic.Validation;
    using GenBOE.Dtos;

    [ExcludeFromCodeCoverage]
    public class ManageWBSModelView : PersistedDataModelView
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public ManageWBSModelView()
        {
            this.WbsID = -1;
            this.WbsNumber = null;
            this.WbsTitle = null;
            this.ClinNumbers = string.Empty;
            this.ClinIDs = new List<int>();
            this.HasBOE = false;
        }

        public ManageWBSModelView(WbsDTO inWBSDTO, Collection<ClinDTO> inClins)
            : this()
        {
            if (inWBSDTO == null)
            {
                throw new ArgumentNullException(nameof(inWBSDTO));
            }
            if (inClins == null)
            {
                throw new ArgumentNullException(nameof(inClins));
            }

            this.WbsID = inWBSDTO.Id;
            this.WbsNumber = inWBSDTO.WbsNumber;
            this.WbsTitle = inWBSDTO.WbsTitle;
            this.UpdateDate = inWBSDTO.UpdateDate;
            this.Level = inWBSDTO.Level;
            this.ClinsInUse = inWBSDTO.ClinsInUse;
            this.InUse = inWBSDTO.inUse;

            this.ClinNumbers = string.Join(", ", inClins.Select<ClinDTO, string>(c => c.ClinNumber));
            this.ClinIDs = inClins.Select(c => c.Id).ToArray();
        }

        public int WbsID { get; set; }

        [Required(ErrorMessage = "WBS # is required.")]
        [StringLength(50, ErrorMessage = "WBS # must not exceed 50 chars.")]
        [RegularExpression(ValidationConstants.WBS_NUMBER, ErrorMessage = "WBS # can have up to 10 levels with a max of 50 characters. Each level must have 1-5 alphanumeric characters.")]
        public string WbsNumber { get; set; }

        [Required(ErrorMessage = "WBS Title is required.")]
        [StringLength(255, ErrorMessage = "WBS Title must not exceed 255 chars.")]
        public string WbsTitle { get; set; }

        /// <summary>
        /// A comma-separated string of CLIN IDs of newly-selected CLINs.
        /// </summary>
        public string ClinNumbers { get; set; }

        public ICollection<int> ClinIDs { get; set; }

        public ICollection <int> ClinsInUse { get; set; }

        public string WbsString { get { return IES.Common.Utilities.FormatNumberTitleString(this.WbsNumber, this.WbsTitle, " "); } }
        
        public int Level { get; set; }

        public bool Deleted { get; set; }

        public bool InUse { get; set; }

        public bool HasBOE { get; set; }

        /// <summary>
        /// Whether a parent WBS has a BOE
        /// </summary>
        public bool ParentHasBoe { get; set; }
    }
}
