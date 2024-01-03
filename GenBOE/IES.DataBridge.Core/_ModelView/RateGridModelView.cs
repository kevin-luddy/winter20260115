// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace IES.DataBridge.ModelViews
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using IES.Core;
    using IES.Core.Exceptions;

    /// <summary>
    /// The Model for a grid in the Rate Grid/Table.
    /// </summary>
    public class RateGridModelView : IESModelView
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RateGridModelView"/> class.
        /// </summary>
        public RateGridModelView()
        {
            this.Versions = new Collection<RevisionOptionModelView>();
            this.LockInfo = new LockModelView();
        }

        /// <summary>
        /// Gets or sets the selected revision Id
        /// </summary>
        public int? SelectedRevisionId { get; set; }

        /// <summary>
        /// Gets or sets the versions of rates.
        /// This does include Work in Progress.
        /// </summary>
        public ICollection<RevisionOptionModelView> Versions { get; set; }

        /// <summary>
        /// Gets or sets the rates.
        /// </summary>
        public ICollection<RateDetailModelView> Rates { get; set; }

        /// <summary>
        /// Gets or sets the rate categories to use in dropdowns on front-end.
        /// </summary>
        public ICollection<OptionModelView> RateCategories { get; set; }

        /// <summary>
        /// Gets or sets the sections to use in dropdowns on front-end. 
        /// </summary>
        public ICollection<OptionModelView> Sections { get; set; }

        /// <summary>
        /// Gets or sets the RateTypes to use in dropdowns on front-end for propricer RateType mapping select.
        /// </summary>
        public ICollection<OptionModelView> RateTypes { get; set; }

		/// <summary>
		/// Gets or sets DisclosureTypes to use in dropdowns on front-end for ProPricer DisclosureType mapping select.
		/// </summary>
		public ICollection<OptionModelView> DisclosureTypes { get; set; }

        /// <summary>
        /// Gets or sets the propricer Resource Types to use in dropdowns on front-end for propricer ResourceTypes select.
        /// </summary>
        public ICollection<OptionModelView> ResourceTypes { get; set; }

        /// <summary>
        /// Gets or sets the propricer Resource Classes to use in dropdowns on front-end for propricer Resource Class select.
        /// </summary>
        public ICollection<OptionModelView> ResourceClasses { get; set; }

        /// <summary>
        /// Gets or sets the CommercialBurdenPools to use in dropdowns on front-end for CommercialBurdenPools.
        /// </summary>
        public ICollection<OptionModelView> CommercialBurdenPools { get; set; }
         
        /// <summary>
        /// Gets or sets the GovernmentBurdenPools  to use in dropdowns on front-end for GovernmentBurdenPools.
        /// </summary>
        public ICollection<OptionModelView> GovernmentBurdenPools { get; set; }

        /// <summary>
        /// Gets or sets lock information
        /// </summary>
        public LockModelView LockInfo { get; set; }

        /// <summary>
        /// Gets or sets the validation messages.
        /// </summary>
        public ICollection<ValidationMessage> ReplicationValidationMessages { get; set; }
    }
}
