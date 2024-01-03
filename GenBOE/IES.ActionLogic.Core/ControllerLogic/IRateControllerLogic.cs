// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace IES.ActionLogic.ControllerLogic
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using IES.Core.Exceptions;
    using IES.DataBridge.ModelViews;

    /// <summary>
    /// Interface for the Rate Controller Logic.
    /// </summary>
    public interface IRateControllerLogic : IRdmControllerLogic
    {
        /// <summary>
        /// Validates all the imported rates exist. We will not load only part of an import file.
        /// Also populates the RateCategory and RateCategoryDescription fields for each model view.
        /// </summary>
        /// <param name="existingRates">Collection of Rates from DB.</param>
        /// <param name="importRateDetails">Collection of imported rate details to validate.</param>
        /// <returns>A list of validation errors (if any).</returns>
        ICollection<ValidationMessage> ValidateImportedRates(ICollection<RateDetailModelView> existingRates,
            ICollection<RateDetailModelView> importRateDetails);

        /// <summary>
        /// Gets the rates.
        /// </summary>
        /// <param name="id">Revision Id</param>
        /// <param name="revisions">Collection of RDM revisions</param>
        /// <returns>Rates for the version requested</returns>
        RateGridModelView GetRatesByVersion(int? id, ICollection<RevisionModelView> revisions);

        /// <summary>
        /// Get the Version differences between the two given IDs for the Rates 
        /// </summary>
        /// <param name="revisionOptions">Revisions as an options list</param>
        /// <param name="firstId">ID of the first revision to compare</param>
        /// <param name="secondId">ID of the second revision to compare. -1 for previous revision.</param>
        /// <returns>Version differences for the rates between the two versions</returns>
        ICollection<RateDetailModelView> GetRatesVersionDifferences(ICollection<RevisionOptionModelView> revisionOptions, int firstId, int secondId);

        /// <summary>
        /// Validate imported rate codes and associated mapping data.
        /// </summary>
        /// <param name="existingRateCodes">Existing Rate Code MVs from DB</param>
        /// <param name="importedRateCodes">Imported Rate Code MVs to validate.</param>
        /// <returns>A list of validation errors (if any)</returns>
        ICollection<ValidationMessage> ValidateImportedRateCodes(RateDetailModelView[] existingRateCodes, RateDetailModelView[] importedRateCodes);

        /// <summary>
        /// ValidateRateDetailModelViews when saving rate values
        /// </summary>
        /// <param name="rateDetailModelViews">Collection of RateDetailModelViews to validate.</param>
        /// <param name="startYear">Revision Start Year</param>
        /// <param name="endYear">Revision End Year</param>
        /// <returns>A list of validation errors (if any).</returns>
        ICollection<ValidationMessage> ValidateRateDetailModelViews(Collection<RateDetailModelView> rateDetailModelViews, int startYear, int endYear);

        /// <summary>
        /// Load imported rates into database.
        /// </summary>
        /// <param name="existingRates">Collection of Rates from DB.</param>
        /// <param name="importedRates">Validated collection of rates to import.</param>
        /// <returns>Collection of imported rates for the current version.</returns>
        ICollection<RateDetailModelView> LoadImportedRates(ICollection<RateDetailModelView> existingRates,
            ICollection<RateDetailModelView> importedRates);

        /// <summary>
        /// Load imported rate codes into database.
        /// </summary>
        /// <param name="importedRateCodes">Validated collection of rate codes to import.</param>
        void LoadImportedRateCodes(ICollection<RateDetailModelView> importedRateCodes);

        /// <summary>
        /// Get the set of updated rate codes.
        /// </summary>
        /// <param name="existingRateCodes">Existing Rate Code MVs from DB</param>
        /// <param name="importedRateCodes">Imported Rate Code MVs to validate.</param>
        /// <returns>Collection of updated rate codes</returns>
        ICollection<RateDetailModelView> GetUpdatedRateCodes(ICollection<RateDetailModelView> existingRateCodes,
            ICollection<RateDetailModelView> importedRateCodes);
    }
}
