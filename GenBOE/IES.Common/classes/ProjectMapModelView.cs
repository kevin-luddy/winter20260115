// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2020 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace IES.Common
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;

    /// <summary>
    /// Model View for Project Map Items/Rows.
    /// </summary>
    [StartEndDateValidation(StartDate = "StartDate", EndDate = "EndDate", CanBeEqual = true, ErrorMessage = "Start Date must be before the End Date")]
    public class ProjectMapModelView : UpdateableDTO
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ProjectMapModelView"/> class.
        /// </summary>
        public ProjectMapModelView()
        {
            this.Id = -1;
        }

        /// <summary>
        /// WBS Number
        /// </summary>
        [Required(ErrorMessage = "WBS Number is required.")]
        public string WbsNumber { get; set; }

        /// <summary>
        /// Activity ID
        /// </summary>
        [Required(ErrorMessage = "Activity ID is required.")]
        public string ActivityID { get; set; }

        /// <summary>
        /// Activity Name
        /// </summary>
        [Required(ErrorMessage = "Activity Name is required.")]
        public string ActivityName { get; set; }

        /// <summary>
        /// WBS Element Title
        /// </summary>
        [Required(ErrorMessage = "WBS Element Title is required.")]
        public string WbsElementTitle { get; set; }

        /// <summary>
        /// Resource
        /// </summary>
        [Required(ErrorMessage = "Activity Type Code is required.")]
        public string InitialResource { get; set; }

        /// <summary>
        /// Gets or sets the LegacyID that corresponds to a LegacyID (SikorskyLegacyResourceDTO/ResourceTypeDto) or a LegacyResourceID (ProjectMap entity).
        /// </summary>
        public int? LegacyID { get; set; }

        /// <summary>
        /// Gets or sets the LegacyResourceID (formerly called OldResource) that corresponds to a LegacyID.
        /// </summary>
        public string LegacyResourceID { get; set; }

        /// <summary>
        /// Cost Center (also known as performing org)
        /// </summary>
        [Required(ErrorMessage = "Cost Center is required.")]
        public string CostCenter { get; set; }

        /// <summary>
        /// Start Date
        /// </summary>
        [Required(ErrorMessage = "Start Date cannot be blank and must be valid")]
        public DateTime? StartDate { get; set; }

        /// <summary>
        /// End Date
        /// </summary>
        [Required(ErrorMessage = "End Date cannot be blank and must be valid")]
        public DateTime? EndDate { get; set; }

        /// <summary>
        /// CLIN
        /// </summary>
        [Required(ErrorMessage = "CLIN is required.")]
        public string Clin { get; set; }

        /// <summary>
        /// SOW Number
        /// </summary>
        public string SowNumber { get; set; }

        /// <summary>
        /// SOW Title
        /// </summary>
        public string SowTitle { get; set; }

        /// <summary>
        /// Task Description
        /// </summary>
        public string Task { get; set; }

        /// <summary>
        /// Hours
        /// </summary>
        [HoursOrDollars(ErrorMessage = "Either Hours or Dollars must be set to nonzero value.")]
        public decimal? Hours { get; set; }

        /// <summary>
        /// Dollars
        /// </summary>
        public decimal? Dollars { get; set; }

        /// <summary>
        /// Rationale
        /// </summary>
        public string Rationale { get; set; }

        /// <summary>
        /// Cost Account Manager (CAM) Name
        /// </summary>
        public string CamName { get; set; }

        /// <summary>
        /// Category
        /// </summary>
        public string Category { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance should try to offload.
        /// </summary>
        public bool Offload { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether a warning should be shown because this instance is set to try to offload but cannot.
        /// </summary>
        public bool OffloadWarning { get; set; }

        /// <summary>
        /// Gets or sets the offload warning text.
        /// </summary>
        public string OffloadWarningText { get; set; }

        /// <summary>
        /// Gets or sets the add delete propricer value.
        /// </summary>
        [ValidValues("A", "D", ErrorMessage = "The field Add/Delete contains invalid values.")]
        public string AddDelete { get; set; } = "A";

        /// <summary>
        /// Gets or sets the class of cost.
        /// </summary>
        [Required(ErrorMessage = "Class of Cost is required.")]
        [ValidValues("REC", "NRE", ErrorMessage="The field Class Of Cost contains invalid values.")]
        public string ClassOfCost { get; set; }

        /// <summary>
        /// Tiered Percentage
        /// </summary>
        public decimal? TieredPercentage { get; set; }

        /// <summary>
        /// Gets or sets the discrete values over months.  Starting index is always January of start year of Workspace.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays")]
        public decimal?[] DiscreteMonths { get; set; }

        /// <summary>
        /// Gets or sets the workspace identifier.
        /// </summary>
        public int WorkspaceId { get; set; }

        /// <summary>
        /// Propagates the new 'parent' DTO Id to all 'child' DTOs in collections.
        /// </summary>
        /// <param name="newParentId">new id of the parent DTO</param>
        protected override void PropagateNewParentIdToChildDTOs(int newParentId)
        {
            // do nothing
        }

        /// <summary>
        /// Creates the spreads.
        /// </summary>
        /// <param name="startingMonth">The starting month.</param>
        /// <returns>A list of discrete spreads for this object.</returns>
        public ICollection<ProjectMapSpreadModelView> CreateSpreads(DateTime startingMonth)
        {
            List<ProjectMapSpreadModelView> spreads = new List<ProjectMapSpreadModelView>();

            if (this.DiscreteMonths != null && this.DiscreteMonths.Length > 0)
            {
                foreach (decimal? discreteMonth in this.DiscreteMonths)
                {
                    if (discreteMonth.HasValue)
                    {
                        spreads.Add(new ProjectMapSpreadModelView
                        {
                            ProjectMapId = this.Id,
                            SpreadDate = startingMonth,
                            SpreadValue = discreteMonth.Value,
                            Updateable = UpdateType.Upsert,
                            WorkspaceId = this.WorkspaceId
                        });
                    }

                    startingMonth = startingMonth.AddMonths(1);
                }
            }

            return spreads;
        }
    }
}