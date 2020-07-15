// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2020 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.Web.ModelView
{
    using System.ComponentModel.DataAnnotations;
    using GenBOE.ActionLogic.ModelView;
    using IES.Common;
    using GenBOE.Dtos;

    /// <summary>
    /// Model View for the BOE Custom Field Resource.
    /// </summary>
    public class BOECustomFieldResourceModelView : BOECustomFieldOptionModelView
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        public BOECustomFieldResourceModelView()
        {
            Description = string.Empty;
            LaborType = string.Empty;
            SegmentRegion = string.Empty;
            ElementOfCostId = ElementOfCostType.LMLabor;
            RateTypeID = RateType.NotSet;
            RateTypeDisplay = string.Empty;
        }

        /// <summary>
        /// Nominal constructor
        /// </summary>
        /// <param name="resource">resource</param>
        /// <param name="rateTypeName">name of the rate type</param>
        /// <param name="elementOfCostName">name of the element of cost</param>
        public BOECustomFieldResourceModelView(ResourceDTO resource, string rateTypeName, string elementOfCostName)
            : base()
        {

            if (resource != null)
            {
                CustomFieldOptionID = resource.Id;
                ID = resource.ResourceName;
                Description = resource.ResourceDesc;
                SegmentRegion = resource.SegRegion;
                LaborType = resource.LaborType;
                UpdateDate = resource.UpdateDate;
                RateTypeID = resource.RateType;
                RateTypeDisplay = rateTypeName;
                ElementOfCostId = resource.ElementOfCost;
                ElementOfCostDisplay = elementOfCostName;
                Deleted = false;
                isSystemResource = resource.isSystemResource;
            }
        }

        /// <summary>
        /// Gets or sets a segment region
        /// </summary>
        [Required(ErrorMessage="Segment/Region is required.")]
        [StringLength(50, ErrorMessage = "A maximum of 50 characters are allowed")]
        public string SegmentRegion { get; set; }

        /// <summary>
        /// Gets or sets a labor type
        /// </summary>
        [Required(ErrorMessage="Labor Type is required.")]
        [StringLength(50, ErrorMessage = "A maximum of 50 characters are allowed")]
        public string LaborType { get; set; }

        /// <summary>
        /// Gets or sets element of code id
        /// </summary>
        [Range(1, 6, ErrorMessage = "Element of Cost is required.")]
        public ElementOfCostType ElementOfCostId { get; set; }

        /// <summary>
        /// Gets or sets element of cost display string
        /// </summary>
        public string ElementOfCostDisplay { get; set; }

        /// <summary>
        /// Gets or sets the rate type id
        /// </summary>
        [Range(RateTypeConstants.MinimumSelectableValue, RateTypeConstants.MaximumSelectableValue, ErrorMessage = "Rate Type is required.")]
        public RateType RateTypeID { get; set; }

        /// <summary>
        /// Gets or sets the rate type display string
        /// </summary>
        public string RateTypeDisplay { get; set; }

        /// <summary>
        /// Gets or sets the boolean indicating if the resource is a system resource
        /// </summary>
        public bool isSystemResource {get;set;}

        [Required(ErrorMessage = "Description is required.")]
        [StringLength(100, ErrorMessage = "A maximum of 100 characters are allowed")]
        public new string Description { get; set; }
    }
}