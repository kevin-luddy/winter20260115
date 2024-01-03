// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace IES.ActionLogic.ControllerLogic
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Threading.Tasks;
    using IES.ActionLogic.Common;
    using IES.Core;
    using IES.Core;
    using IES.DataBridge.Loaders;
    using IES.DataBridge.ModelViews;
    using Mediator;

    /// <summary>
    /// Logic for the Version Controller
    /// </summary>
    public class VersionControllerLogic : RdmControllerLogic, IVersionControllerLogic
    {
        /// <summary>
        /// Rate Detail Loader
        /// </summary>
        private readonly IRateDetailLoader rateDetailLoader;

        /// <summary>
        /// The emailer
        /// </summary>
        private IIESEmailer emailer;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="rateDetailLoader">Rate Detail Loader</param>
        /// <param name="revisionMediator">Revision Mediator</param>
        /// <param name="areaLockingLoader">Area Locking Loader</param>
        /// <param name="adUtils">AD Utilities</param>
        /// <param name="securityInfo">Security Information</param>
        /// <param name="emailer">The emailer.</param>
        public VersionControllerLogic(IRateDetailLoader rateDetailLoader,
            IAreaLockingLoader areaLockingLoader, IRevisionMediator revisionMediator, IActiveDirectoryUtilities adUtils, ISecurityInformation securityInfo, IIESEmailer emailer)
            : base(areaLockingLoader, revisionMediator, adUtils, securityInfo)
        {
            this.rateDetailLoader = rateDetailLoader;
            this.emailer = emailer;
        }

        /// <summary>
        /// Get the differences for the Version Comparison grid when a new Version is selected
        /// </summary>
        /// <param name="id">Selected Revision Id, or -1 to return most recent revision</param>
        /// <param name="secondId">Second selected revision ID, or -1 to return previous revision</param>
        /// <param name="isRdmAdminUser">true if user is RDM Admin user</param>
        /// <param name="activeUser">The active user.</param>
        /// <returns>Differences based on the selected version</returns>
        public VersionComparisonModelView GetVersionDifferences(int id, int secondId, bool isRdmAdminUser, UserData activeUser)
        {
            ICollection<RevisionModelView> revisions = this.Revisions;
            if (revisions == null || revisions.Count == 0)
            {
                throw new ArgumentException("There are no revisions to compare.");
            }

            RevisionModelView wipRevision = this.WipRevision;

            VersionComparisonModelView modelView = new VersionComparisonModelView
            {
                AdminUser = isRdmAdminUser,
                WorkInProgressHistory = wipRevision.History,
                ReleaseNotes = wipRevision.ReleaseNotes,
                ReplicationValidationMessages = this.rateDetailLoader.VerifyRateCodeReplication(wipRevision.Id)
            };
            
            if (revisions.Count <= 1)
            {
                return modelView;
            }

            // Get first specified revision.  If revision id <= 0, return first revision in list (should be WIP revision for admin user)
            RevisionModelView firstSelectedRevision = id <= 0
                ? revisions.OrderByDescending(x => x.Id).ToList().FirstOrDefault()
                : revisions.FirstOrDefault(x => x.Id == id);
            
            if (firstSelectedRevision == null)
            {
                throw new ArgumentException("Could not find specified revision");
            }
            
            modelView.AvailableVersions = this.RevisionMediator.GetRevisionOptions(revisions);
            
            modelView.FirstSelectedRevision = modelView.AvailableVersions.First(x => x.Id == firstSelectedRevision.Id);

            int selectedVersionNumber;
            int.TryParse(firstSelectedRevision.Revision, out selectedVersionNumber);

            modelView.SelectedVersionNumber = selectedVersionNumber;
            modelView.SelectedVersionNumberDisplay = wipRevision != null && wipRevision.Id == firstSelectedRevision.Id
                ? CommonConstants.WorkInProgress
                : string.Format("Revision {0}", modelView.SelectedVersionNumber);
            
            // Update list of versions available to compare to 
            modelView.AvailableCompareToVersions = new List<RevisionOptionModelView>();
            foreach (RevisionOptionModelView version in modelView.AvailableVersions)
            {
                int versionNumber;
                int.TryParse(version.Revision, out versionNumber);
                if (versionNumber < selectedVersionNumber)
                {
                    modelView.AvailableCompareToVersions.Add(new RevisionOptionModelView()
                    {
                        Id = version.Id,
                        Label = version.Label,
                        Revision = version.Revision,
                        StartYear = version.StartYear,
                        EndYear = version.EndYear
                    });
                }
            }

            // Get second specified revision. If id <=0, return revision previous to first or empty modelview if no previous
            int indexOfFirstRevision = revisions.OrderByDescending(x => x.Id).ToList().IndexOf(firstSelectedRevision);
            modelView.IsEarliestVersion = indexOfFirstRevision == revisions.Count - 1;

            RevisionModelView secondSelectedRevision = modelView.IsEarliestVersion ? null // if earliest version selected, nothing to compare to
                : secondId <= 0 || !modelView.AvailableCompareToVersions.Any(x => x.Id == secondId) // if -1 or not available to compare to, get previous version
                    ? revisions.OrderByDescending(x => x.Id).ElementAt(indexOfFirstRevision + 1) 
                : revisions.FirstOrDefault(x => x.Id == secondId); // otherwise get that revision

            // Update label of Previous Version revision
            if (modelView.AvailableCompareToVersions.Any())
            {
                modelView.AvailableCompareToVersions.First().Label = CommonConstants.PreviousVersion;
            }

            // only throw exception when first selection is not the eariest version where it's expected to be null
            if (secondSelectedRevision == null && !modelView.IsEarliestVersion)
            {
                throw new ArgumentException("Could not find specified revision");
            }

			int previousVersionNumber;
			int.TryParse(secondSelectedRevision.Revision, out previousVersionNumber);

			modelView.PreviousVersionNumber = previousVersionNumber;
			modelView.PreviousVersionNumberDisplay = string.Format("Revision {0}", modelView.PreviousVersionNumber);

			// Don't update second revision and differences for eariest version as neither will exist in that case
			if (!modelView.IsEarliestVersion)
            {
                modelView.SecondSelectedRevision = modelView.AvailableCompareToVersions.FirstOrDefault(x => x.Id == secondSelectedRevision.Id);
                modelView.PPRDDifferences = this.RevisionMediator.GetVersionComparisonRows(secondSelectedRevision.Id, firstSelectedRevision.Id);
            }

            if (modelView.AdminUser)
            {
                modelView.ActiveLocks = this.AreaLockingLoader.GetActiveLocksByOtherUsers(activeUser);
            }

            return modelView;
        }

        /// <summary>
        /// Gets any rates for the given revision for which the rate would have only zero values for all displayed years.
        /// </summary>
        /// <param name="id">The revision ID to check.</param>
        /// <returns>Collection of invalid rates - empty if all valid</returns>
        public ICollection<string> GetInvalidRates(int? id)
        {
            RevisionModelView revision = this.RevisionMediator.GetById(id);
            ICollection<RateDetailModelView> rates = this.rateDetailLoader.GetRatesByRevision(revision);

            // Get the published year - use current year if not published.
            int publishYear = revision.DatePublished?.Year ?? DateTime.Now.Year;

            // Rates have match for at least one of the categories.
            var backwardLookingRates = rates.Where(r =>
                r.RateCategory == RateCategory.Fccom || r.RateCategory == RateCategory.Fringe ||
                r.RateCategory == RateCategory.GA || r.RateCategory == RateCategory.Overhead).ToList();

            // Rates do not have a match for any of the categories.
            var forwardLookingRates = rates.Where(r =>
                r.RateCategory != RateCategory.Fccom && r.RateCategory != RateCategory.Fringe &&
                r.RateCategory != RateCategory.GA && r.RateCategory != RateCategory.Overhead).ToList();

            // Backward looking rates check from (publishYear - RATE_TABLE_YEARS_TO_DISPLAY_BACKWARD_LOOKING_ADJUSTMENT) through (publishYear + RATE_TABLE_YEARS_TO_DISPLAY).
            // Forward looking rates check from (publishYear) through (publishYear + RATE_TABLE_YEARS_TO_DISPLAY).
            ICollection<string> toReturn = new Collection<string>();
            toReturn.AddRange(AllRatesHaveValues(backwardLookingRates, publishYear - CommonConstants.RATE_TABLE_YEARS_TO_DISPLAY_BACKWARD_LOOKING_ADJUSTMENT, CommonConstants.RATE_TABLE_YEARS_TO_DISPLAY + CommonConstants.RATE_TABLE_YEARS_TO_DISPLAY_BACKWARD_LOOKING_ADJUSTMENT));
            toReturn.AddRange(AllRatesHaveValues(forwardLookingRates, publishYear, CommonConstants.RATE_TABLE_YEARS_TO_DISPLAY));

            return toReturn.OrderBy(r => r).ToList();
        }

        /// <summary>
        /// Determines whether all rates have at least one value for the yearly range.
        /// </summary>
        /// <param name="rates">The rates to validate.</param>
        /// <param name="startYear">The starting year for validation.</param>
        /// <param name="yearsToValidate">The number of years to validate from the startYear onward.</param>
        /// <returns>Empty collection if all rates have at least one non-zero/non-null value, collection of rates with all zero/null values otherwise.</returns>
        private static ICollection<string> AllRatesHaveValues(ICollection<RateDetailModelView> rates, int startYear, int yearsToValidate)
        {
            ConcurrentBag<string> toReturn = new ConcurrentBag<string>();

            Parallel.ForEach(rates, (rate, state) =>
            {
                // If all values are 0 or null for a given year range, validation fails.
                if (rate.Values.Where(v => v.Year >= startYear && v.Year <= startYear + yearsToValidate)
                    .All(w => (w.Value == null || w.Value == 0)))
                {
                    toReturn.Add(rate.RateCode);
                }
            });

            return toReturn.ToList();
        }

        /// <summary>
        /// Sends the publish email.
        /// </summary>
        /// <param name="publishedRevision">The published revision.</param>
        /// <param name="currentUser">The current user</param>
        public void SendPublishEmail(RevisionModelView publishedRevision, UserData currentUser)
        {
            this.emailer.SendPublishEmail(publishedRevision, currentUser);
        }
    }
}
