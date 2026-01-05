// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.ActionLogic
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
	using System.Threading.Tasks;
	using GenBOE.ActionLogic.ModelView.Admin;
    using GenBOE.Dtos;
    using IES.Common;
    using IES.Common.Exceptions;
    using ModelView;

    public interface IAdminControllerLogic
    {
        /// <summary>
        /// Gets the labor rate example file location
        /// </summary>
        string LABOR_RATES_EXAMPLE_LOCATION { get; }

        /// <summary>
        /// Gets the labor rate example file name
        /// </summary>
        string LABOR_RATES_EXAMPLE_NAME { get; }

        /// <summary>
        /// The logic for determining the default resources and returns the appropriate modelview 
        /// </summary>
        /// <returns>Default Resources Modelview</returns>
        DefaultResourcesModelView DisplayDefaultResourcesLogic();

		/// <summary>
		/// Refresh the system settings for Reports
		/// </summary>
		/// <returns>async void</returns>
		Task RefreshReportsSystemSettings();

        /// <summary>
        /// Gets the grid data for the Manage Zone Travel Origins Page
        /// </summary>
        /// <returns>Grid data</returns>
        ManageMSTZoneTravelOriginsGridModelView GetZoneTravelOriginGridData();

        /// <summary>
        /// Gets the zone travel origins.
        /// </summary>
        /// <returns>A collection of Zone Travel Origin data.</returns>
        ICollection<MSTZoneTravelOriginModelView> GetZoneTravelOrigins();

        /// <summary>
        /// Gets the grid data for the Manage Zone Travel Destinations Page
        /// </summary>
        /// <returns>grid data</returns>
        ManageMSTZoneTravelDestinationsGridModelView GetZoneTravelDestinationsGridData();

        /// <summary>
        /// Gets the grid data for the Manage Fees and Costs for Nonzone Travel page
        /// </summary>
        /// <returns>grid data</returns>
        NonzoneFeesAndCostsGridModelView GetNonzoneFeesAndCostsGridData();

        /// <summary>
        /// Saves Origin and Resource Data for MST Zone Travel - Upsert or Delete
        /// </summary>
        /// <param name="data">Origin MV with data to be saved</param>
        void SaveOriginData(ICollection<MSTZoneTravelOriginModelView> data);

        /// <summary>
        /// Saves Destination Data for MST Zone Travel - updates to assigned zone
        /// </summary>
        /// <param name="data">Destination MV with data to be saved</param>
        void SaveDestinationData(MSTZoneTravelDestinationModelView data);

        /// <summary>
        /// Saves Fees and Costs data for Nonzone Travel
        /// </summary>
        /// <param name="data">NonzoneFeesAndCosts MV with data to be saved</param>
        void SaveFeesAndCostsData(NonzoneFeesAndCostsModelView data);

        /// <summary>
        /// Validates the escalation rate.
        /// </summary>
        /// <param name="rate">The escalation rate.</param>
        /// <param name="allRates">All current system rates.</param>
        /// <param name="validationErrors">A list of validation errors</param>
        void ValidateEscalationRate(EscalationRateModelView rate, ICollection<EscalationRatesDTO> allRates, ICollection<ValidationMessage> validationErrors);

        /// <summary>
        /// Validates the offload rate.
        /// </summary>
        /// <param name="rate">The rate.</param>
        /// <param name="allRates">All current offload rates.</param>
        /// <param name="validationErrors">A list of validation errors.</param>
        void ValidateOffloadRate(OffloadRateModelView rate, ICollection<OffloadRatesDTO> allRates, Collection<ValidationMessage> validationErrors);

		/// <summary>
		/// Kicks off DataAnnotation validation for the system settings
		/// </summary>
		/// <param name="systemSettings">System settings to validate</param>
		/// <param name="validationErrors">A list of validation errors.</param>
		void ValidateSystemSettings(ICollection<SystemSettingDTO> systemSettings, Collection<ValidationMessage> validationErrors);

		/// <summary>
		/// Save ucot system settings
		/// </summary>
		/// <param name="systemSettings">System settings to save</param>
		bool SaveUcotSystemSettings(decimal ucot);

		/// <summary>
		/// Get ucot system settings
		/// </summary>
		/// <param name="systemSettings">System settings to save</param>
		decimal GetUcotSystemSettingsValue();

		/// <summary>
		/// Peform validation that is shared between saving or editing an Output Format Template
		/// </summary>
		/// <param name="templateName">Template Name</param>
		/// <param name="templateDescription">Template Description</param>
		/// <returns>Collection of any validation messages</returns>
		Collection<ValidationMessage> ValidateTemplateOnSaveOrEdit(string templateName, string templateDescription);

        /// <summary>
        /// Gets the overdue training for users in the system.
        /// </summary>
        /// <param name="activeDirectoryUtilities">The AD Utilities class.</param>
        /// <param name="models">The models.</param>
        /// <param name="courseName">The name of the course to get overdue training for.</param>
        /// <returns>A list of users that have overdue training along with last completed date.</returns>
        ICollection<TrainingModelView> GetOverdueTraining(IActiveDirectoryUtilities activeDirectoryUtilities, ICollection<TrainingModelView> models, string courseName);

        /// <summary>
        /// Gets the training course groups.
        /// </summary>
        /// <returns>Training course groups</returns>
        ICollection<TrainingCourseGroupModelView> GetTrainingCourseGroups();

		/// <summary>
		/// Get System Settings from the database
		/// </summary>
		/// <returns>System settings</returns>
		ICollection<SystemSettingDTO> GetSystemSettings();
	}
}
