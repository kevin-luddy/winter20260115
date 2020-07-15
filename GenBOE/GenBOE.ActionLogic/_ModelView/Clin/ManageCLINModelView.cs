// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2020 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.ActionLogic.ModelView.Clin
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.Diagnostics.CodeAnalysis;
    using GenBOE.ActionLogic.Validation;
    using GenBOE.Dtos;
    using IES.Common;

    [ExcludeFromCodeCoverage]
    [StartEndDateValidation(StartDate = "StartDate", EndDate = "EndDate", CanBeEqual = true, ErrorMessage = "Start Date must be before the End Date")]
    public class ManageCLINModelView : PersistedDataModelView
    {
        public ManageCLINModelView()
        {
            this.ClinID = -1;
            this.ClinNumber = string.Empty;
            this.ClinPaddedNumber = string.Empty;
            this.ClinTitle = string.Empty;
            this.StartDate = string.Empty;
            this.EndDate = string.Empty;
            this.WorkSpaceID = 0;
            this.Deleted = false;
            this.InUse = false;
            this.ContractType = Constants.CONTRACT_TYPE_NOT_SET;
            this.ContractTypeText = Constants.CONTRACT_TYPE_NOT_SET_STRING;
        }

        public ManageCLINModelView(ClinDTO inClinDTO, string contractTypeAsText) : this()
        {
            if (inClinDTO == null)
            {
                throw new ArgumentNullException(nameof(inClinDTO));
            }

            this.ClinID = inClinDTO.Id;
            this.ClinNumber = inClinDTO.ClinNumber;
            this.ClinPaddedNumber = inClinDTO.ClinPaddedNumber;
            this.ClinTitle = inClinDTO.ClinTitle;
            this.StartDate = (inClinDTO.StartDate!=null)?  inClinDTO.StartDate.Value.ToString("MM/yyyy"): "";
            this.EndDate = (inClinDTO.EndDate != null) ? inClinDTO.EndDate.Value.ToString("MM/yyyy") : "";
            this.WorkSpaceID = inClinDTO.WorkspaceID;
            this.UpdateDate = inClinDTO.UpdateDate;
            this.InUse = inClinDTO.InUse;
            this.ContractType = inClinDTO.ContractType;
            this.ContractTypeText = contractTypeAsText;
        }
        
        public int ClinID { get; set; }

        [Required(ErrorMessage = "CLIN # is required.")]
        [StringLength(50, ErrorMessage = "A maximum of 50 characters are allowed for the CLIN #.")]
        public string ClinNumber { get; set; }

        public string ClinPaddedNumber { get; set; }

        [Required(ErrorMessage = "CLIN Title required.")]
        [StringLength(100, ErrorMessage = "A maximum of 100 characters are allowed for the CLIN Title.")]
        public string ClinTitle { get; set; }

        public string ClinString { get { return Utilities.FormatNumberTitleString(this.ClinNumber, this.ClinTitle, " "); } }

        [RegularExpression(ValidationConstants.DATE_MONTH_YEAR, ErrorMessage = "Start Date must be in MM/YYYY format.")]
        [DisplayFormat(DataFormatString = "{0:MM/yyyy}", ApplyFormatInEditMode = true)]

        public string StartDate { get; set; }

        [RegularExpression(ValidationConstants.DATE_MONTH_YEAR, ErrorMessage = "End Date must be in MM/YYYY format.")]
        [DisplayFormat(DataFormatString = "{0:MM/yyyy}", ApplyFormatInEditMode = true)]
        public string EndDate { get; set; }

        public int WorkSpaceID { get; set; }

        public bool Deleted { get; set; }

        public bool InUse { get; set; }

        public int ContractType { get; set; }

        public string ContractTypeText { get; set; }
    }
}
