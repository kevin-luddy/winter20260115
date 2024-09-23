// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2024 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.ActionLogic.Common
{
	using GenBOE.Objects;
	using System.Diagnostics.CodeAnalysis;

	/// <summary>
	/// Utility Class to hold Action Logic Methods
	/// </summary>
	public class ActionLogicUtility
	{
		/// <summary>
		/// Retrieves the Proposal Title and RFP Number from the workspace.
		/// </summary>
		/// <param name="workspace">Workspace</param>
		/// <returns>A string in the form { Proposal Number } / { RFP }</returns>
		[SuppressMessage("Microsoft.Design", "CA1011: Consider passing base types as parameters")]
		public static string GetProposalTitleAndRfpNumber(FullWorkspace workspace)
		{
			if (workspace != null)
			{

				return string.IsNullOrWhiteSpace(workspace.RFPNumber) ? $"{workspace.ProposalTitle}" : $"{workspace.ProposalTitle} / {workspace.RFPNumber}";
			}

			return string.Empty;
		}
	}
}
