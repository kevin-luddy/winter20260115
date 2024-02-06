// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenTRAC.DataBridge.DTO
{
	using System;
	using System.Collections.Generic;
	using System.Web.Mvc;
	using GenTRAC.DataBridge.DTO.Permission;
	using IES.Common;

	/// <summary>
	/// proposal loader interface
	/// </summary>
	public interface IProposalLoader : IDataLoader<ProposalDto>
	{
		/// <summary>
		/// Get All proposal Ids
		/// </summary>
		/// <returns>All proposal Ids</returns>
		System.Collections.Generic.ICollection<int> GetAllIds();

		/// <summary>
		/// get proposal id by proposal tracking number
		/// </summary>
		/// <param name="inTrackingNumber">proposal tracking number</param>
		/// <returns>proposal id</returns>
		int GetIdByTrackingNumber(string inTrackingNumber);

		/// <summary>
		/// Gets a list of ids for all proposals the designated users are allowed to see
		/// </summary>
		/// <param name="userids">List of user ids</param>
		/// <returns>List of proposal ids</returns>
		ICollection<int> GetProposalIdsByUser(System.Collections.Generic.ICollection<int> userids);

		/// <summary>
		/// Gets a list of Proposal Data to use for the Home Proposal View, it includes proposals that the passed in user is allowed
		/// to see, along with the proposal information of their reports (down the tree to Individual contributors) where the employee
		/// holds either the Pricer or Cost Volume Lead roles.
		/// If showProposalsForMyOrganization is set, include proposals for the user's organization 
		/// (i.e. Product Lines where their group(s) have the "Viewer" role).
		/// </summary>
		/// <param name="proposalStatus">The status of the proposals to view.</param>
		/// <param name="filterStartDate">The start date for the filter.</param>
		/// <param name="filterEndDate">The end date for the filter.</param>
		/// <param name="searchString">The search string.</param>
		/// <param name="ntID">The users NT ID.</param>
		/// <param name="showProposalsForMyOrganization">True to show proposals for the user's organization; false to show only the user's proposals.</param>
		/// <param name="userAndGroupIDs">XML list of User and Group IDs for the user. Only used when showProposalsForMyOrganization is true.</param>
		/// <param name="proposalClassFilterID">The filter ID for the proposal class of the proposals to view.</param>
		/// <returns>A list of proposals for the given user.</returns>
		ICollection<HomeProposalViewDto> GetProposalsByUser(int? proposalStatus, DateTime? filterStartDate, DateTime? filterEndDate, string searchString, string ntID, bool showProposalsForMyOrganization, string userAndGroupIDs, int? proposalClassFilterID);

		/// <summary>
		/// Get the proposal completed date. 
		/// </summary>
		/// <param name="inProposalId">proposal id</param>
		/// <returns>date of completed</returns>
		DateTime? GetProposalCompletedDate(int inProposalId);

		/// <summary>
		/// Get all completed proposals after an optional submit date
		/// </summary>
		/// <param name="cutoffDate">Earliest submit date to get proposals for</param>
		/// <returns>all completed proposals after an optional date</returns>
		ICollection<ProposalDto> GetAllCompletedProposalsAfterSubmitDate(DateTime? cutoffDate);

		/// <summary>
		/// Updates a proposal status
		/// </summary>
		/// <param name="proposalId">proposal id</param>
		/// <param name="updateDate">proposal update date</param>
		/// <param name="proposalStatus">proposal status</param>
		/// <returns>Id of the saved Proposal</returns>
		int? UpdateProposalStatus(int proposalId, DateTime updateDate, ProposalStatus proposalStatus);

		/// <summary>
		/// Updates the proposal forecast email sent on a proposal.
		/// </summary>
		/// <param name="proposalId">The proposal identifier.</param>
		/// <param name="updateDate">The update date.</param>
		void UpdateProposalForecastEmailSent(int proposalId, DateTime updateDate);

		/// <summary>
		/// Checks if the given proposal title is unique compared to all proposals.
		/// </summary>
		/// <param name="proposalId">proposal id</param>
		/// <param name="proposalTitle">proposal title</param>
		/// <returns>true or false</returns>
		bool IsProposalTitleUnique(int proposalId, string proposalTitle);

		/// <summary>
		/// Get the proposal status
		/// </summary>
		/// <param name="inProposalId">proposal id</param>
		/// <returns>proposal status</returns>
		ProposalStatus? GetProposalStatus(int inProposalId);

		/// <summary>
		/// Gets a list of proposals that are in the status passed into the method..
		/// </summary>
		/// <param name="workflowStatus">The workflow status.</param>
		/// <returns>A list of proposals.</returns>
		ICollection<ProposalDto> GetProposalsByWorkflowStatus(WorkflowStatus workflowStatus);

		/// <summary>
		/// Gets a list of proposals that are in the status passed into the method.
		/// </summary>
		/// <param name="workflowStatus">The proposal status.</param>
		/// <returns>A list of proposals.</returns>
		ICollection<ProposalDto> GetProposalsByProposalStatus(ProposalStatus proposalStatus);

		/// <summary>
		/// Gets a list of proposals that are in the status passed into the method and older than the cutoff date.
		/// </summary>
		/// <param name="workflowStatus">The workflow status.</param>
		/// <param name="cutoffDate">The cutoff date for retrieving proposals.</param>
		/// <returns>A list of proposals.</returns>
		ICollection<ProposalDto> GetProposalsByWorkflowStatusAndCutoffDate(WorkflowStatus workflowStatus, DateTime cutoffDate);

		/// <summary>
		/// Gets the forecast proposals past allowed date.
		/// </summary>
		/// <param name="cutoffDate">The cutoff date.</param>
		/// <returns>A list of proposals.</returns>
		ICollection<ProposalDto> GetForecastProposalsPastAllowedDate(DateTime cutoffDate);

		/// <summary>
		/// Gets the proposals certification timeline past due.
		/// </summary>
		/// <returns>A list of proposals.</returns>
		ICollection<ProposalDto> GetProposalsCertificationTimelinePastDue();

		/// <summary>
		/// Returns a Collection of slim ProposalDtos from the DB, each with only the bare minimum fields required
		/// to perform unit testing and RDSB.  This helps performance for anything that required fetching all proposals.
		/// </summary>
		/// <returns>Collection of slim Proposal DTOs</returns>
		ICollection<ProposalDto> GetAllSlim();

		/// <summary>
		/// Gets the proposals by user.
		/// </summary>
		/// <param name="ntID">The nt identifier.</param>
		/// <param name="includeWorkpaceCreator">Whether the Workspace Creator Role should be included when getting proposals</param>
		/// <returns>A collection of proposals.</returns>
		ICollection<ProposalDto> GetProposalsByUser(string ntID, bool includeWorkpaceCreator = false);

		/// <summary>
		/// Gets the next forecasted tracking number.
		/// </summary>
		/// <returns>The next forecasted tracking number.</returns>
		string GetNextForecastedTrackingNumber();

		/// <summary>
		/// Gets Revision History for the specific proposal
		/// </summary>
		/// <param name="proposalId">Proposal Id</param>
		/// <returns>Revision History</returns>
		ICollection<RevisionHistoryModelView> GetRevisionHistory(int proposalId);

		/// <summary>
		/// Gets Proposal Data for eEPP application, when the user is searching for a PTM record
		/// </summary>
		/// <param name="ntid">User's NTID</param>
		/// <param name="isAdmin">Is the user System Admin</param>
		/// <param name="searchString">Optional search string</param>
		/// <returns>Proposal Data</returns>
		ICollection<EppProposalData> GetEppProposalData(string ntid, bool isAdmin, string searchString);

		/// <summary>
		/// Gets Proposal Data for eEPP, by Proposal Tracking Number, when the application needs to check if the previously selected PTM record is out-of-date
		/// </summary>
		/// <param name="ntid">User's NTID</param>
		/// <param name="isAdmin">Is the user System Admin</param>
		/// <param name="trackingNumber">Proposal Tracking Number</param>
		/// <returns>Proposal Data</returns>
		EppProposalData GetEppProposalDataByProposalTrackingNumber(string ntid, bool isAdmin, string trackingNumber);

		/// <summary>
		/// Get options for the Previously Submitted ROM field in the Contracts Tab
		/// </summary>
		/// <param name="selectedValue">Selected option</param>
		/// <returns>Values for dropdown</returns>
		ICollection<SelectListItem> GetRomProposalOptions(int? selectedValue);

		/// <summary>
		/// Gets ROM Proposal's Previously Submitted ROM Value and Previously Submitted ROM Date
		/// </summary>
		/// <param name="proposalId">Proposal Id</param>
		/// <returns>Submitted Date and Submitted Value</returns>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
		Tuple<DateTime?, decimal?> GetRomDateAndValue(int proposalId);

		/// Gets Proposal Data for ACV application, when the user is searching for a PTM record
		/// 
		/// Search criteria:
		///     - proposal is in progress
		///     - user is either system admin, or either a Lead Estimator, or a Backup Lead Est.
		///     - if search string is provided, then a proposal matches if PTM Tracking Number or Proposal Title contain the search string
		///     
		/// The method will return data to top 100 records, as more data being returned to the user is not going to be helpful
		/// </summary>
		/// <param name="ntid">User's NTID</param>
		/// <param name="isAdmin">Is the user System Admin</param>
		/// <param name="searchString">Optional search string</param>
		/// <returns>Proposal Data</returns>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
		ICollection<(string PtmTrackingNumber, string ProposalTitle, int ProposalId)> GetCostVolumeProposalData(string ntid, bool isAdmin, string searchString);

		/// <summary>
		/// Gets a list of proposals that are missing mod executed date, and meet the waiting period.
		/// </summary>
		/// <returns>Collection of proposals meeting the send criteria</returns>
		ICollection<ProposalDto> GetModExecutedDateMissingNotifications();

		/// <summary>
		/// Get the Header data for ACV
		/// </summary>
		/// <param name="proposalId">Proposal ID</param>
		/// <returns>Header data for the proposal ID</returns>
		AcvHeaderDataDto GetAcvHeaderDataByProposalId(int proposalId);

		/// <summary>
		/// Get Proposal Roles for the given user that are needed for NLF
		/// </summary>
		/// <param name="ntid">NTID</param>
		/// <returns>Collection of Proposals and Roles for the user</returns>
		ICollection<ProposalRoleDto> GetProposalRolesForNlfByNtid(string ntid);
	}
}
