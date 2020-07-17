// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2020 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.ActionLogic.ModelView.Admin
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.ComponentModel.DataAnnotations;
    using System.Web.Mvc;
    using GenBOE.ActionLogic.ModelView;
    using IES.Common;
    using IES.Common.classes;
    using GenBOE.Dtos;

    /// <summary>
    /// Default resources model view
    /// </summary>
    public class DefaultResourcesModelView : PersistedDataModelView
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        public DefaultResourcesModelView()
        {
            this.ListID = -1;
            this.ListName = string.Empty;
            this.DefaultResources = new Collection<DefaultResourceModelView>();
        }

        /// <summary>
        /// Nominal constructor
        /// </summary>
        /// <param name="list">List of resources</param>
        public DefaultResourcesModelView(ResourceListDTO list)
            : this()
        {
            if (list != null)
            {
                this.ListID = list.ResourceListID;
                this.ListName = list.ResourceListName;
                this.UpdateDate = list.UpdateDate;
            }
        }

        /// <summary>
        /// Gets or sets List ID
        /// </summary>
        [Required(ErrorMessage = "PKID is required.")]
        public int ListID { get; set; }

        /// <summary>
        /// Gets or sets List Name
        /// </summary>
        [Required(ErrorMessage = "List Name is required.")]
        [StringLength(50, ErrorMessage = "A maximum of 50 characters are allowed")]
        public string ListName { get; set; }

        /// <summary>
        /// Gets or sets Default Resources
        /// </summary>
        public ICollection<DefaultResourceModelView> DefaultResources { get; set; }

        /// <summary>
        /// Gets or sets Default Resources
        /// </summary>
        public ICollection<SelectListItem> elementOfCostTypes { get; set; }

        /// <summary>
        /// Gets or sets Default Resources
        /// </summary>
        public ICollection<SelectListItem> rateTypes { get; set; }
    }

    /// <summary>
    /// Model View for a single default resource
    /// </summary>
    public class DefaultResourceModelView : PersistedDataModelView
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        public DefaultResourceModelView()
        {
            this.ResourceID = 0;
            this.ID = string.Empty;
            this.Description = string.Empty;
            this.LaborType = string.Empty;
            this.SegmentRegion = string.Empty;
            this.Deleted = false;
            this.ElementOfCostId = ElementOfCostType.NotSet;
            this.RateTypeID = RateType.NotSet;
            this.RateTypeDisplay = string.Empty;
            this.InUse = false;
        }

        /// <summary>
        /// Nominal Constructor
        /// </summary>
        /// <param name="resource">resource</param>
        /// <param name="rateTypeName">rate type name</param>
        /// <param name="elementOfCostName">element of cost name</param>
        public DefaultResourceModelView(ResourceDTO resource, string rateTypeName, string elementOfCostName)
        {
            if (resource != null)
            {
                this.ResourceID = resource.Id;
                this.ID = resource.ResourceName;
                this.Description = resource.ResourceDesc;
                this.SegmentRegion = resource.SegRegion;
                this.LaborType = resource.LaborType;
                this.UpdateDate = resource.UpdateDate;
                this.RateTypeID = resource.RateType;
                this.RateTypeDisplay = rateTypeName;
                this.ElementOfCostId = resource.ElementOfCost;
                this.ElementOfCostDisplay = elementOfCostName;
                this.Deleted = false;
            }
        }

        /// <summary>
        /// Gets or sets Resource id
        /// </summary>
        [Required(ErrorMessage = "PKID is required.")]
        public int ResourceID { get; set; }

        /// <summary>
        /// Gets or sets ID
        /// </summary>
        [Required(ErrorMessage = "Resource ID is required.")]
        [StringLength(20, ErrorMessage = "A maximum of 20 characters are allowed")]
        public string ID { get; set; }

        /// <summary>
        /// Gets or sets Description
        /// </summary>
        [Required(ErrorMessage = "Resource Description is required.")]
        [StringLength(100, ErrorMessage = "A maximum of 100 characters are allowed")]
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets Labor type
        /// </summary>
        [Required(ErrorMessage = "Labor Type is required.")]
        [StringLength(50, ErrorMessage = "A maximum of 50 characters are allowed")]
        public string LaborType { get; set; }

        /// <summary>
        /// Gets or sets Segment Region
        /// </summary>
        [Required(ErrorMessage = "Segment/Region is required.")]
        [StringLength(50, ErrorMessage = "A maximum of 50 characters are allowed")]
        public string SegmentRegion { get; set; }

        /// <summary>
        /// Gets or sets Element of Cost id
        /// </summary>
        [Range(ElementOfCostTypeConstants.MinimumSelectableValue, ElementOfCostTypeConstants.MaximumSelectableValue, ErrorMessage = "Element of Cost is Required.")]
        public ElementOfCostType ElementOfCostId { get; set; }

        /// <summary>
        /// Gets or sets rate type id
        /// </summary>
        [Range(RateTypeConstants.MinimumSelectableValue, RateTypeConstants.MaximumSelectableValue, ErrorMessage = "Rate Type is Required.")]
        public RateType RateTypeID { get; set; }
        
        /// <summary>
        /// Gets or sets Rate type display string
        /// </summary>
        public string RateTypeDisplay { get; set; }

        /// <summary>
        /// Gets or sets Element of Cost display string
        /// </summary>
        public string ElementOfCostDisplay { get; set; }

        /// <summary>
        /// Gets or sets boolean indicating if the resource is deleted
        /// </summary>
        public bool Deleted { get; set; }

        /// <summary>
        /// Gets or sets boolean indicating if the resource is in use
        /// </summary>
        public bool InUse { get; set; }

        #region ISGS Versus SSC terminology
        /// <summary>
        /// Indicates if the application is running as Space. If false, its running as ISGS
        /// </summary>
        public bool SpaceEnabled
        {
            get
            {
                return SystemConfiguration.Instance().CompanyMode == CompanyConfiguration.SpaceSystems;
            }
        }
        #endregion
    }
}
