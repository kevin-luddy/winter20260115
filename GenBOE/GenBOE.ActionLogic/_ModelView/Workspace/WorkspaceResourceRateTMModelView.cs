// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2020 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.ActionLogic.ModelView.Workspace
{
    using System.ComponentModel.DataAnnotations;
    using GenBOE.ActionLogic.Validation;
    using GenBOE.Dtos;
    using IES.Common;

    [StartEndDateValidation(StartDate = "StartDate", EndDate = "EndDate", CanBeEqual = true, ErrorMessage = "Start Date must be before the End Date")]
    public class WorkspaceResourceRateTMModelView : PersistedDataModelView
    {
        public const string SORT_ID_RESOURCE_NAME = "ResourceName";
        public const string SORT_ID_RESOURCE_RATE = "ResourceRate";
        public const string SORT_ID_RESOURCE_DESCRIPTION = "ResourceDescription";
        public const string SORT_ID_START_DATE = "StartDate";
        public const string SORT_ID_END_DATE = "EndDate";

        public WorkspaceResourceRateTMModelView()
        {
            this.ResourceRateID = -1;
            this.ResourceDescription = string.Empty;
            this.ResourceRate = string.Empty;
            this.StartDate = string.Empty;
            this.EndDate = string.Empty;
            this.InUse = false;
            this.ResourceID = -1;
            this.ResourceName = string.Empty;
            this.ToDelete = false;
        }

        public WorkspaceResourceRateTMModelView(TMResourceRateDTO inRateDTO, ResourceDTO inWorkspaceResourceDTO, bool inInUse)
            : this()
        {
            if (inRateDTO != null && inWorkspaceResourceDTO != null)
            {
                this.ResourceRateID = inRateDTO.ResourceRateID;
                this.UpdateDate = inRateDTO.UpdateDate;
                this.ResourceID = inRateDTO.ResourceID;
                this.ResourceName = inWorkspaceResourceDTO.ResourceName;
                this.ResourceType = inWorkspaceResourceDTO.ResourceTypeCategory;
                this.ResourceDescription = inWorkspaceResourceDTO.ResourceDesc;
                this.InUse = inInUse;
                this.ResourceRate = inRateDTO.ResourceRate.HasValue ? inRateDTO.ResourceRate.Value.ToString() : string.Empty;
                this.StartDate = inRateDTO.StartDate.HasValue ? inRateDTO.StartDate.Value.ToMonthString() : string.Empty;
                this.EndDate = inRateDTO.EndDate.HasValue ? inRateDTO.EndDate.Value.ToMonthString() : string.Empty;
            }
        }

        public int ResourceRateID { get; set; }
        [RegularExpression(ValidationConstants.TM_RESOURCE_RATE_DECIMAL, ErrorMessage = "Rate $ must be between .01 - 9999.99.")]
        public string ResourceRate { get; set; }

        [RegularExpression(ValidationConstants.DATE_MONTH_YEAR, ErrorMessage = "Start Date format must be mm/yyyy.")]
        public string StartDate { get; set; }
        [RegularExpression(ValidationConstants.DATE_MONTH_YEAR, ErrorMessage = "End Date format must be mm/yyyy.")]
        public string EndDate { get; set; }
        public bool InUse { get; set; }

        [Required(ErrorMessage = "Resource ID is required.")]
        public int ResourceID { get; set; }
        public string ResourceName { get; set; }
        public string ResourceDescription { get; set; }
        /// <summary>
        /// Resource type (e.g. E1, T2, Subcontractor)
        /// </summary>
        public string ResourceType { get; set; }
        public bool ToDelete { get; set; }

        public string WorkspaceResourceRateTMHeadingText
        {
            get; set;
        }

        public string WorkspaceResourceRateTMJumpDescription
        {
            get; set;
        }

        public string WorkspaceResourceRateTMControlDescription
        {
            get; set;
        }
    }
}
